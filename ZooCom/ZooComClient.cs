using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;

namespace ZooCom
{
    public class ZooComClient
    {
        readonly int tcpPort;

        public ZooComClient(int port)
        {
            tcpPort = port;
        }

        public bool ModifyModel<T>(T model) where T : Model.Model
        {
            using (TcpClient tcpClient = Connect(tcpPort + 3))
            {
                if (tcpClient == null)
                    return false;
                using (NetworkStream netStream = tcpClient.GetStream())
                using (MemoryStream dataStream = new MemoryStream())
                {
                    netStream.WriteByte(model.TypeID());
                    netStream.Flush();
                    new BinaryFormatter().Serialize(dataStream, model);
                    byte[] data = dataStream.ToArray();
                    netStream.Write(BitConverter.GetBytes(data.Length), 0, sizeof(int));
                    netStream.Flush();
                    netStream.Write(data, 0, data.Length);
                    netStream.Flush();
                }
                return true;
            }
        }
        
        public bool AddModel<T>(T model) where T : Model.Model
        {
            using (TcpClient tcpClient = Connect(tcpPort + 2))
            {
                if (tcpClient == null)
                    return false;
                using (NetworkStream netStream = tcpClient.GetStream())
                using (MemoryStream dataStream = new MemoryStream())
                {
                    netStream.WriteByte(model.TypeID());
                    netStream.Flush();
                    new BinaryFormatter().Serialize(dataStream, model);
                    byte[] data = dataStream.ToArray();
                    netStream.Write(BitConverter.GetBytes(data.Length), 0, sizeof(int));
                    netStream.Flush();
                    netStream.Write(data, 0, data.Length);
                    netStream.Flush();
                }
                return true;
            }
        }

        public Tuple<bool, byte> DeleteModel<T>(int id) where T : Model.Model, new()
        {
            using (TcpClient tcpClient = Connect(tcpPort + 1))
            {
                if (tcpClient == null)
                    return null;
                using (NetworkStream netStream = tcpClient.GetStream())
                {
                    netStream.WriteByte(new T().TypeID());
                    netStream.Write(BitConverter.GetBytes(id), 0, sizeof(int));
                    netStream.Flush();
                    byte[] response = new byte[sizeof(bool)];
                    netStream.Read(response, 0, response.Length);
                    bool status = BitConverter.ToBoolean(response, 0);
                    byte errorCode = (byte)Model.ModelType.Model;
                    if (!status)
                    {
                        netStream.Read(response, 0, 1);
                        errorCode = response[0];

                    }
                    return new Tuple<bool, byte>(status, errorCode);
                }
            }
        }

        public List<T> GetModels<T>() where T : Model.Model, new()
        {
            using (TcpClient tcpClient = Connect(tcpPort))
            {
                if (tcpClient == null)
                    return null;
                List<T> models = null;
                using (NetworkStream netStream = tcpClient.GetStream())
                {
                    netStream.WriteByte(new T().TypeID());
                    netStream.Flush();
                    byte[] data = new byte[sizeof(int)];
                    netStream.Read(data, 0, sizeof(int));
                    data = new byte[BitConverter.ToInt32(data, 0)];
                    netStream.Read(data, 0, data.Length);
                    models = ((List<object>)new BinaryFormatter().Deserialize(new MemoryStream(data))).Cast<T>().ToList();
                    netStream.Close();
                }
                return models;
            }
        }

        TcpClient Connect(int port, bool isFirstCall = true)
        {
            try
            {
                return new TcpClient("localhost", port)
                {
                    NoDelay = true
                };
            }
            catch (Exception e)
            {
                if (isFirstCall && System.Diagnostics.Process.GetProcessesByName("ZooService").Length <= 0)
                {
                    Logger.LogWarning("ZooService is down, starting up...", GetType(), $"Connect(port: {port}, isFirstCall: {isFirstCall})");
                    try
                    {
                        System.Diagnostics.Process.Start("ZooService.exe");
                        System.Threading.Thread.Sleep(2000);
                        return Connect(port, false);
                    }
                    catch (Exception)
                    {
                        Logger.LogError("Cannot start ZooService!", GetType(), $"Connect(port: {port}, isFirstCall: {isFirstCall})");
                        return null;
                    }
                }
                Logger.LogError($"Cannot open tcp connection on port. Exception: {e.ToString()}", GetType(), $"Connect(port: {port}, isFirstCall: {isFirstCall})");
                return null;
            }
        }
    }
}
