 using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;
using System.Data.SqlClient;

namespace ZooService
{
    public enum ModelType { Model = 0, Animal = 1, Attraction = 2, BalanceType = 4, CashBalance = 8, Food = 16, Overtime = 32, Place = 64, Worker = 128}

    public class ZooService
    {
        Assembly zooComDll;
        readonly Dictionary<ModelType, Type> modelTypes = new Dictionary<ModelType, Type>(Enum.GetNames(typeof(ModelType)).Length);

        TcpListener serverModifyModel;
        Thread threadModifyModel;
        TcpListener serverAddModel;
        Thread threadAddModel;
        TcpListener serverDeleteModel;
        Thread threadDeleteModel;
        TcpListener serverGetModels;
        Thread threadGetModels;
        readonly int tcpPort;
        readonly IPAddress localIp;
        volatile bool isServerRunning = false;

        public ZooService(int port)
        {
            zooComDll = Assembly.LoadFile($@"C:\Users\marek\Desktop\fit\FitSharp\ZooCom.dll");
            foreach (ModelType type in (ModelType[])Enum.GetValues(typeof(ModelType)))
            {
                modelTypes[type] = zooComDll.GetType($"ZooCom.Model.{Enum.GetName(typeof(ModelType), type)}");
            }
            tcpPort = port;
            IPHostEntry host = Dns.GetHostEntry("localhost");
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIp = ip;
                    break;
                }
            }
        }

        public virtual void Start()
        {
            serverModifyModel = new TcpListener(localIp, tcpPort + 3);
            serverAddModel = new TcpListener(localIp, tcpPort + 2);
            serverDeleteModel = new TcpListener(localIp, tcpPort + 1);
            serverGetModels = new TcpListener(localIp, tcpPort);
            threadModifyModel = new Thread(() =>
            {
                serverModifyModel.Start();
                while (isServerRunning)
                {
                    Socket client = serverModifyModel.AcceptSocket();
                    new Thread(() =>
                    {
                        Logger.LogInfo("New modify model request", GetType(), "threadModifyModel");
                        byte[] data = new byte[1];
                        client.Receive(data, 1, SocketFlags.None);
                        ModelType type = (ModelType)data[0];
                        data = new byte[sizeof(int)];
                        client.Receive(data, sizeof(int), SocketFlags.None);
                        data = new byte[BitConverter.ToInt32(data, 0)];
                        client.Receive(data, data.Length, SocketFlags.None);
                        object model = new BinaryFormatter().Deserialize(new MemoryStream(data));
                        using (ZooContext context = new ZooContext())
                        {
                            if (ModifyModel(context, type, ConvertFromZooComModel(model, type)))
                                Logger.LogInfo($"Modified {type.ToString()}", GetType(), "threadModifyModel");
                            else
                                Logger.LogWarning($"Cannot modify {type.ToString()}", GetType(), "threadModifyModel");
                        }
                        client.Close();
                    }).Start();
                }
                serverModifyModel.Stop();
            });
            threadAddModel = new Thread(() =>
            {
                serverAddModel.Start();
                while (isServerRunning)
                {
                    Socket client = serverAddModel.AcceptSocket();
                    new Thread(() =>
                    {
                        Logger.LogInfo("New add model request", GetType(), "threadAddModel");
                        byte[] data = new byte[1];
                        client.Receive(data, 1, SocketFlags.None);
                        ModelType type = (ModelType)data[0];
                        data = new byte[sizeof(int)];
                        client.Receive(data, sizeof(int), SocketFlags.None);
                        data = new byte[BitConverter.ToInt32(data, 0)];
                        client.Receive(data, data.Length, SocketFlags.None);
                        object model = new BinaryFormatter().Deserialize(new MemoryStream(data));
                        using (ZooContext context = new ZooContext())
                        {
                            if (AddModel(context, type, ConvertFromZooComModel(model, type)))
                                Logger.LogInfo($"Added new {type.ToString()}", GetType(), "threadAddModel");
                            else
                                Logger.LogWarning($"Cannot add new {type.ToString()}", GetType(), "threadAddModel");
                        }
                        client.Close();
                    }).Start();
                }
                serverAddModel.Stop();
            });
            threadDeleteModel = new Thread(() =>
            {
                serverDeleteModel.Start();
                while (isServerRunning)
                {
                    Socket client = serverDeleteModel.AcceptSocket();
                    new Thread(() =>
                    {
                        Logger.LogInfo("New delete model request", GetType(), "threadDeleteModel");
                        byte[] data = new byte[1];
                        client.Receive(data, 1, SocketFlags.None);
                        ModelType type = (ModelType)data[0];
                        data = new byte[sizeof(int)];
                        client.Receive(data, sizeof(int), SocketFlags.None);
                        using (ZooContext context = new ZooContext())
                        {
                            int id = BitConverter.ToInt32(data, 0);
                            Tuple<bool, byte> operationDetails = DeleteModel(context, type, id);
                            client.Send(BitConverter.GetBytes(operationDetails.Item1));
                            if (operationDetails.Item1)
                                Logger.LogInfo($"Deleted model of type {type.ToString()}, ID = {id}", GetType(), "threadDeleteModel");
                            else
                            {
                                Logger.LogWarning($"Cannot delete model of type {type.ToString()}, ID = {id}", GetType(), "threadDeleteModel");
                                client.Send(new byte[1] { operationDetails.Item2 });
                            }
                        }
                        client.Close();
                    }).Start();
                }
                serverDeleteModel.Stop();
            });
            threadGetModels = new Thread(() =>
            {
                serverGetModels.Start();
                while (isServerRunning)
                {
                    Socket client = serverGetModels.AcceptSocket();
                    new Thread(() =>
                    {
                        Logger.LogInfo("New get models request", GetType(), "threadGetModels");
                        byte[] data = new byte[1];
                        client.Receive(data, 1, SocketFlags.None);
                        ModelType type = (ModelType)data[0];
                        using (MemoryStream dataStream = new MemoryStream())
                        using (ZooContext context = new ZooContext())
                        {
                            try
                            {
                                new BinaryFormatter().Serialize(dataStream, GetModels(context, type).Select(model => ConvertToZooComModel(type, model)).ToList());
                                client.Send(BitConverter.GetBytes(dataStream.ToArray().Length), sizeof(int), SocketFlags.None);
                                client.Send(dataStream.ToArray());
                                Logger.LogInfo($"Send models of type {type.ToString()}", GetType(), "threadGetModels");
                            }
                            catch (Exception e)
                            {
                                Logger.LogError(e.ToString());
                            }
                        }
                        client.Close();
                    }).Start();
                }
                serverGetModels.Stop();
            });
            isServerRunning = true;
            threadModifyModel.Start();
            threadAddModel.Start();
            threadDeleteModel.Start();
            threadGetModels.Start();
        }

        public virtual void Stop()
        {
            isServerRunning = false;
        }

        public virtual bool ModifyModel(ZooContext context, ModelType type, ZooDataModel model)
        {
            switch (type)
            {
                case ModelType.Animal:
                    return context.ModifyModel((Animal)model);
                case ModelType.Attraction:
                    return context.ModifyModel((Attraction)model);
                case ModelType.BalanceType:
                    return context.ModifyModel((BalanceType)model);
                case ModelType.CashBalance:
                    return context.ModifyModel((CashBalance)model);
                case ModelType.Food:
                    return context.ModifyModel((Food)model);
                case ModelType.Overtime:
                    return context.ModifyModel((Overtime)model);
                case ModelType.Place:
                    return context.ModifyModel((Place)model);
                case ModelType.Worker:
                    return context.ModifyModel((Worker)model);
                default:
                    return false;
            }
        }

        public virtual bool AddModel(ZooContext context, ModelType type, ZooDataModel model)
        {
            switch (type)
            {
                case ModelType.Animal:
                    return context.AddModel((Animal)model);
                case ModelType.Attraction:
                    return context.AddModel((Attraction)model);
                case ModelType.BalanceType:
                    return context.AddModel((BalanceType)model);
                case ModelType.CashBalance:
                    CashBalance balance = (CashBalance)model;
                    if(balance.BalanceTypeID == 0)
                    {
                        PayMonthSalary();
                        return true;
                    }
                    else
                        return context.AddModel(balance);
                case ModelType.Food:
                    return context.AddModel((Food)model);
                case ModelType.Overtime:
                    return context.AddModel((Overtime)model);
                case ModelType.Place:
                    return context.AddModel((Place)model);
                case ModelType.Worker:
                    return context.AddModel((Worker)model);
                default:
                    return false;
            }
        }

        public virtual Tuple<bool, byte> DeleteModel(ZooContext context, ModelType type, int id)
        {
            switch (type)
            {
                case ModelType.Animal:
                    return context.DeleteAnimal(id);
                case ModelType.Attraction:
                    return context.DeleteAttraction(id);
                case ModelType.Food:
                    return context.DeleteFood(id);
                case ModelType.Place:
                    return context.DeletePlace(id);
                case ModelType.Worker:
                    return context.DeleteWorker(id);
                default:
                    return new Tuple<bool, byte>(false, 0);
            }
        }

        public virtual List<ZooDataModel> GetModels(ZooContext context, ModelType type)
        {
            switch (type)
            {
                case ModelType.Animal:
                    return context.GetModels<Animal>().Cast<ZooDataModel>().ToList();
                case ModelType.Attraction:
                    return context.GetModels<Attraction>().Cast<ZooDataModel>().ToList();
                case ModelType.BalanceType:
                    return context.GetModels<BalanceType>().Cast<ZooDataModel>().ToList();
                case ModelType.CashBalance:
                    return context.GetModels<CashBalance>().Cast<ZooDataModel>().ToList();
                case ModelType.Food:
                    return context.GetModels<Food>().Cast<ZooDataModel>().ToList();
                case ModelType.Overtime:
                    return context.GetModels<Overtime>().Cast<ZooDataModel>().ToList();
                case ModelType.Place:
                    return context.GetModels<Place>().Cast<ZooDataModel>().ToList();
                case ModelType.Worker:
                    return context.GetModels<Worker>().Cast<ZooDataModel>().ToList();
                default:
                    return null;
            }
        }

        public virtual object GetProperty(object model, string name)
        {
            return model.GetType().GetProperty(name).GetValue(model);
        }

        public virtual ZooDataModel ConvertFromZooComModel(object model, ModelType type)
        {
            switch (type)
            {
                case ModelType.Animal:
                    return new Animal()
                    {
                        ID = (int)GetProperty(model, "ID"),
                        Name = (string)GetProperty(model, "Name"),
                        Count = (int)GetProperty(model, "Count"),
                        MaintenanceCost = (decimal)GetProperty(model, "MaintenanceCost"),
                        PlaceID = (int)GetProperty(model, "PlaceID"),
                        FoodID = (int)GetProperty(model, "FoodID")
                    };
                case ModelType.Attraction:
                    return new Attraction()
                    {
                        ID = (int)GetProperty(model, "ID"),
                        Name = (string)GetProperty(model, "Name"),
                        Description = (string)GetProperty(model, "Description"),
                        AttractionManagerID = (int)GetProperty(model, "AttractionManagerID"),
                        PlaceID = (int)GetProperty(model, "PlaceID")
                    };
                case ModelType.BalanceType:
                    return new BalanceType()
                    {
                        ID = (int)GetProperty(model, "ID"),
                        Description = (string)GetProperty(model, "Description")
                    };
                case ModelType.CashBalance:
                    return new CashBalance()
                    {
                        ID = (int)GetProperty(model, "ID"),
                        SubmitDate = (DateTime)GetProperty(model, "SubmitDate"),
                        Money = (decimal)GetProperty(model, "Money"),
                        BalanceTypeID = (int)GetProperty(model, "BalanceTypeID"),
                        DetailedDescription = (string)GetProperty(model, "DetailedDescription")
                    };
                case ModelType.Food:
                    return new Food()
                    {
                        ID = (int)GetProperty(model, "ID"),
                        Name = (string)GetProperty(model, "Name"),
                        Amount = (double)GetProperty(model, "Amount")
                    };
                case ModelType.Overtime:
                    Overtime overtime = new Overtime()
                    {
                        ID = (int)GetProperty(model, "ID"),
                        Date = (DateTime)GetProperty(model, "Date"),
                        Hours = (int)GetProperty(model, "Hours"),
                        WorkerID = (int)GetProperty(model, "WorkerID")
                    };
                    overtime.SetPercentage();
                    return overtime;
                case ModelType.Place:
                    return new Place()
                    {
                        ID = (int)GetProperty(model, "ID"),
                        Name = (string)GetProperty(model, "Name"),
                        OpenTime = (TimeSpan)GetProperty(model, "OpenTime"),
                        CloseTime = (TimeSpan)GetProperty(model, "CloseTime"),
                        MaintenanceCost = (decimal)GetProperty(model, "MaintenanceCost")
                    };
                case ModelType.Worker:
                    return new Worker()
                    {
                        ID = (int)GetProperty(model, "ID"),
                        Surname = (string)GetProperty(model, "Surname"),
                        Name = (string)GetProperty(model, "Name"),
                        Age = (int)GetProperty(model, "Age"),
                        Salary = (decimal)GetProperty(model, "Salary"),
                        StartDate = (DateTime)GetProperty(model, "StartDate"),
                        PlaceID = (int)GetProperty(model, "PlaceID")
                    };
                default:
                    return null;
            }
        }

        public virtual object ConvertToZooComModel(ModelType type, ZooDataModel model)
        {
            if (ModelType.Model == type)
                return null;
            object comModel = Activator.CreateInstance(modelTypes[type]);
            SetProperty(comModel, "ID", model.ID);
            switch (type)
            {
                case ModelType.Animal:
                    if (model is Animal animal)
                    {
                        SetProperty(comModel, "Name", animal.Name);
                        SetProperty(comModel, "Count", animal.Count);
                        SetProperty(comModel, "MaintenanceCost", animal.MaintenanceCost);
                        SetProperty(comModel, "PlaceID", animal.PlaceID);
                        SetProperty(comModel, "FoodID", animal.FoodID);
                    }
                    break;
                case ModelType.Attraction:
                    if (model is Attraction attraction)
                    {
                        SetProperty(comModel, "Name", attraction.Name);
                        SetProperty(comModel, "Description", attraction.Description);
                        SetProperty(comModel, "AttractionManagerID", attraction.AttractionManagerID);
                        SetProperty(comModel, "PlaceID", attraction.PlaceID);
                    }
                    break;
                case ModelType.BalanceType:
                    if (model is BalanceType balanceType)
                    {
                        SetProperty(comModel, "Description", balanceType.Description);
                    }
                    break;
                case ModelType.CashBalance:
                    if (model is CashBalance cashBalance)
                    {
                        SetProperty(comModel, "SubmitDate", cashBalance.SubmitDate);
                        SetProperty(comModel, "Money", cashBalance.Money);
                        SetProperty(comModel, "BalanceTypeID", cashBalance.BalanceTypeID);
                        SetProperty(comModel, "DetailedDescription", cashBalance.DetailedDescription);
                    }
                    break;
                case ModelType.Food:
                    if (model is Food food)
                    {
                        SetProperty(comModel, "Name", food.Name);
                        SetProperty(comModel, "Amount", food.Amount);
                    }
                    break;
                case ModelType.Overtime:
                    if (model is Overtime overtime)
                    {
                        SetProperty(comModel, "Date", overtime.Date);
                        SetProperty(comModel, "Hours", overtime.Hours);
                        SetProperty(comModel, "PaymentPercentage", overtime.PaymentPercentage);
                        SetProperty(comModel, "WorkerID", overtime.WorkerID);
                    }
                    break;
                case ModelType.Place:
                    if (model is Place place)
                    {
                        SetProperty(comModel, "Name", place.Name);
                        SetProperty(comModel, "OpenTime", place.OpenTime);
                        SetProperty(comModel, "CloseTime", place.CloseTime);
                        SetProperty(comModel, "MaintenanceCost", place.MaintenanceCost);
                    }
                    break;
                case ModelType.Worker:
                    if (model is Worker worker)
                    {
                        SetProperty(comModel, "Surname", worker.Surname);
                        SetProperty(comModel, "Name", worker.Name);
                        SetProperty(comModel, "Age", worker.Age);
                        SetProperty(comModel, "Salary", worker.Salary);
                        SetProperty(comModel, "StartDate", worker.StartDate);
                        SetProperty(comModel, "PlaceID", worker.PlaceID);
                    }
                    break;
            }
            return comModel;
        }

        public virtual void SetProperty(object model, string name, object value)
        {
            model.GetType().GetProperty(name).SetValue(model, value);
        }

        public virtual void PayMonthSalary()
        {
            using (SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ZooContextMonthSalary"].ConnectionString))
            using (SqlCommand command = new SqlCommand("EXEC CreateMonthPayment", connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
        }
    }
}
