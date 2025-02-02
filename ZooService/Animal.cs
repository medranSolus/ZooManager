//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ZooService
{
    using System;
    
    [Serializable]
    public partial class Animal : ZooDataModel
    {
        public string Name { get; set; }
        public Nullable<int> Count { get; set; }
        public Nullable<decimal> MaintenanceCost { get; set; }
        public int PlaceID { get; set; }
        public int FoodID { get; set; }

        [field: NonSerialized]
        public virtual Food Food { get; set; }
        [field: NonSerialized]
        public virtual Place Place { get; set; }

        public override void Assign(ZooDataModel model)
        {
            if (model is Animal animal)
            {
                Name = animal.Name;
                Count = animal.Count;
                MaintenanceCost = animal.MaintenanceCost;
                PlaceID = animal.PlaceID;
                FoodID = animal.FoodID;
            }
        }
    }
}
