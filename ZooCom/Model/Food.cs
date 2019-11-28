namespace ZooCom.Model
{
    [System.Serializable]
    public class Food : Model
    {
        double _amount = 0.0;
        
        public string Name { get; set; }
        public double Amount
        {
            get { return _amount; }
            set
            {
                if (value < 0.0)
                    throw new System.ArgumentOutOfRangeException($"Food: Amount cannot be lower than 0. Provided \"{value}\"");
                _amount = value;
            }
        }

        public override byte TypeID()
        {
            return (byte)ModelType.Food;
        }
    }
}
