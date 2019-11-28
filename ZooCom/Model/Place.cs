namespace ZooCom.Model
{
    [System.Serializable]
    public class Place : Model
    {
        decimal _maintenanceCost = 0;
        
        public string Name { get; set; }
        public System.TimeSpan OpenTime { get; set; } = new System.TimeSpan(8, 0, 0);
        public System.TimeSpan CloseTime { get; set; } = new System.TimeSpan(18, 0, 0);
        public decimal MaintenanceCost
        {
            get { return _maintenanceCost; }
            set
            {
                if (value < 0)
                    throw new System.ArgumentOutOfRangeException($"Place: MaintenanceCost cannot be lower than 0. Provided \"{value}\"");
                _maintenanceCost = value;
            }
        }

        public override byte TypeID()
        {
            return (byte)ModelType.Place;
        }
    }
}
