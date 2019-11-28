namespace ZooCom.Model
{
    [System.Serializable]
    public class Animal : Model
    {
        decimal _maintenanceCost = 0;
        
        public string Name { get; set; }
        public int Count { get; set; } = 0;
        public decimal MaintenanceCost
        {
            get { return _maintenanceCost; }
            set
            {
                if (value < 0)
                    throw new System.ArgumentOutOfRangeException($"Animal: MaintenanceCost cannot be lower than 0. Provided \"{value}\"");
                _maintenanceCost = value;
            }
        }
        public int PlaceID { get; set; }
        public int FoodID { get; set; }

        public override byte TypeID()
        {
            return (byte)ModelType.Animal;
        }
    }
}
