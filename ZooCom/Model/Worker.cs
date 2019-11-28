namespace ZooCom.Model
{
    [System.Serializable]
    public class Worker : Model
    {
        int _age = 0;
        decimal _salary = 0;
        
        public string Surname { get; set; }
        public string Name { get; set; }
        public int Age
        {
            get { return _age; }
            set
            {
                if (value < 16)
                    throw new System.ArgumentOutOfRangeException($"Worker: Age cannot be lower than 16. Provided \"{value}\"");
                _age = value;
            }
        }
        public decimal Salary
        {
            get { return _salary; }
            set
            {
                if (value < 0)
                    throw new System.ArgumentOutOfRangeException($"Worker: Salary cannot be lower than 16. Provided \"{value}\"");
                _salary = value;
            }
        }
        public System.DateTime StartDate { get; set; } = System.DateTime.Today;
        public int PlaceID { get; set; }

        public override byte TypeID()
        {
            return (byte)ModelType.Worker;
        }
    }
}
