namespace ZooCom.Model
{
    [System.Serializable]
    public class CashBalance : Model
    {
        public System.DateTime SubmitDate { get; set; } = System.DateTime.Today;
        public decimal Money { get; set; } = 0;
        public int BalanceTypeID { get; set; }
        public string DetailedDescription { get; set; }

        public override byte TypeID()
        {
            return (byte)ModelType.CashBalance;
        }
    }
}
