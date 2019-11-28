namespace ZooCom.Model
{
    [System.Serializable]
    public class Attraction : Model
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int AttractionManagerID { get; set; }
        public int PlaceID { get; set; }

        public override byte TypeID()
        {
            return (byte)ModelType.Attraction;
        }
    }
}
