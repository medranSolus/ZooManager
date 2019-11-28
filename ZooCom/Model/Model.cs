using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ZooCom.Model
{
    public enum ModelType { Model = 0, Animal = 1, Attraction = 2, BalanceType = 4, CashBalance = 8, Food = 16, Overtime = 32, Place = 64, Worker = 128 }

    [Serializable]
    public abstract class Model
    {
        public int ID { get; set; }
        
        public long SerializedBytes()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, this);
                return stream.Length;
            }
        }

        public virtual byte TypeID()
        {
            return (byte)ModelType.Model;
        }
    }
}