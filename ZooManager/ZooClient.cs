using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;

namespace ZooManager
{
    public enum ModelType { Model = 0, Animal = 1, Attraction = 2, BalanceType = 4, CashBalance = 8, Food = 16, Overtime = 32, Place = 64, Worker = 128 }

    public class ZooClient
    {
        Assembly zooComDll;
        Type zooComClientType;
        readonly Dictionary<ModelType, Type> modelTypes = new Dictionary<ModelType, Type>(Enum.GetNames(typeof(ModelType)).Length);
        Dictionary<string, MethodInfo> zooComMethods = new Dictionary<string, MethodInfo>(3);
        readonly object zooComClient;

        public ZooClient(int port)
        {
            zooComDll = Assembly.LoadFile($@"{AppDomain.CurrentDomain.BaseDirectory}ZooCom.dll");
            zooComClientType = zooComDll.GetType("ZooCom.ZooComClient");
            foreach (ModelType type in (ModelType[])Enum.GetValues(typeof(ModelType)))
            {
                modelTypes[type] = zooComDll.GetType($"ZooCom.Model.{Enum.GetName(typeof(ModelType), type)}");
            }
            zooComClient = Activator.CreateInstance(zooComClientType, new object[] { port });
            foreach (string method in new string[] { "ModifyModel", "AddModel", "DeleteModel", "GetModels" })
            {
                zooComMethods.Add(method, zooComClientType.GetMethod(method));
            }
        }

        public bool ModifyModel(object model)
        {
            if (!modelTypes.ContainsValue(model.GetType()))
            {
                Logger.LogWarning($"Unknown object passed to method of type: {model.GetType()}", GetType(), $"ModifyModel(model: {model})");
                return false;
            }
            try
            {
                if ((bool)zooComMethods["ModifyModel"].MakeGenericMethod(model.GetType()).Invoke(zooComClient, new object[] { model }))
                    return true;
                Logger.LogError("Cannot connect to database, internal connection error!", GetType(), $"ModifyModel(model: {model})");
            }
            catch (Exception e)
            {
                Logger.LogError($"Cannot modify object {model.GetType()}. Exception {e.ToString()}", GetType(), $"ModifyModel(model: {model})");
            }
            return false;
        }

        public bool AddModel(object model)
        {
            if (!modelTypes.ContainsValue(model.GetType()))
            {
                Logger.LogWarning($"Unknown object passed to method of type: {model.GetType()}", GetType(), $"AddModel(model: {model})");
                return false;
            }
            try
            {
                if ((bool)zooComMethods["AddModel"].MakeGenericMethod(model.GetType()).Invoke(zooComClient, new object[] { model }))
                    return true;
                Logger.LogError("Cannot connect to database, internal connection error!", GetType(), $"AddModel(model: {model})");
            }
            catch (Exception e)
            {
                Logger.LogError($"Cannot add object {model.GetType()} to database. Exception {e.ToString()}", GetType(), $"AddModel(model: {model})");
            }
            return false;
        }

        public Tuple<bool, byte> DeleteModel(ModelType type, int id)
        {
            try
            {
                Tuple<bool, byte> response = (Tuple<bool, byte>)zooComMethods["DeleteModel"].MakeGenericMethod(modelTypes[type]).Invoke(zooComClient, new object[] { id });
                if (response == null)
                    Logger.LogError("Cannot connect to database, internal connection error!", GetType(), $"DeleteModel(type: {type.ToString()}, id: {id})");
                else if (!response.Item1)
                    Logger.LogError($"Cannot delete model due to its contraints, error code: {Convert.ToString(response.Item2, 2)}", GetType(), $"DeleteModel(type: {type.ToString()}, id: {id})");
                return response;
            }
            catch (Exception e)
            {
                Logger.LogError($"Cannot delete {type.ToString()} with ID: {id}. Exception {e.ToString()}", GetType(), $"DeleteModel(type: {type}, id: {id})");
            }
            return null;
        }

        public List<object> GetModels(ModelType type)
        {
            try
            {
                IList models = (IList)zooComMethods["GetModels"].MakeGenericMethod(modelTypes[type]).Invoke(zooComClient, null);
                if (models != null)
                    return models.Cast<object>().ToList();
                Logger.LogError("Cannot connect to database, internal connection error!", GetType(), $"GetModels(type: {type.ToString()})");
            }
            catch (Exception e)
            {
                Logger.LogError($"Cannot get objects {type.ToString()}. Exception {e.ToString()}", GetType(), $"GetModels(model: {type})");
            }
            return null;
        }

        public Type GetModelType(ModelType type)
        {
            return modelTypes[type];
        }

        public object CreateModel(ModelType type)
        {
            return Activator.CreateInstance(modelTypes[type]);
        }

        public static object GetProperty(object model, string name)
        {
            return model.GetType().GetProperty(name).GetValue(model);
        }

        public static void SetProperty(object model, string name, object value)
        {
            model.GetType().GetProperty(name).SetValue(model, value);
        }

        public static string DecodeDeleteError(byte code)
        {
            StringBuilder msg = new StringBuilder("[ERROR] ");
            if (code == (byte)ModelType.Model)
                msg.Append("Unknown database internal error!");
            else
            {
                msg.Append("Cannot delete model due to existing contraints on following tables: ");
                if ((code & (byte)ModelType.Animal) > 0)
                    msg.Append("Animals, ");
                if ((code & (byte)ModelType.Attraction) > 0)
                    msg.Append("Attractions, ");
                if ((code & (byte)ModelType.BalanceType) > 0)
                    msg.Append("BalanceTypes, ");
                if ((code & (byte)ModelType.CashBalance) > 0)
                    msg.Append("CashBalances, ");
                if ((code & (byte)ModelType.Food) > 0)
                    msg.Append("Foods, ");
                if ((code & (byte)ModelType.Overtime) > 0)
                    msg.Append("Overtimes, ");
                if ((code & (byte)ModelType.Place) > 0)
                    msg.Append("Places, ");
                if ((code & (byte)ModelType.Worker) > 0)
                    msg.Append("Workers, ");
                msg.Remove(msg.Length - 2, 2);
                msg.Append(".");
            }
            return msg.ToString();
        }
    }
}
