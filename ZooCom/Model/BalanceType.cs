namespace ZooCom.Model
{
    [System.Serializable]
    public class BalanceType : Model
    {
        public string Description { get; set; }

        public override byte TypeID()
        {
            return (byte)ModelType.BalanceType;
        }
    }
}
