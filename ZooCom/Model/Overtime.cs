namespace ZooCom.Model
{
    [System.Serializable]
    public class Overtime : Model
    {
        public System.DateTime Date { get; set; }
        public int Hours { get; set; }
        public int PaymentPercentage { get; set; }
        public int WorkerID { get; set; }

        public override byte TypeID()
        {
            return (byte)ModelType.Overtime;
        }
    }
}
