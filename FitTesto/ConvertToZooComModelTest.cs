using System;
using ZooService;
using fitlibrary;

namespace FitTesto
{
    public class ConvertToZooComModelTest : DoFixture
    {
        ZooService.ZooService service = new ZooService.ZooService(1111);
        Animal animalUnit;
        object comModel;

        int ID = 1;
        int placeID = 3;
        int foodID = 5;
        decimal maintenanceCost = 21;
        string name = "Cowabunga";
        int count = 42;
        
        public void SetUp()
        {
            comModel = new ZooCom.Model.Animal
            {
                ID = ID,
                MaintenanceCost = maintenanceCost,
                Name = name,
                PlaceID = placeID,
                FoodID = foodID,
                Count = count
            };

            animalUnit = new Animal
            {
                ID = ID,
                MaintenanceCost = maintenanceCost,
                Name = name,
                PlaceID = placeID,
                FoodID = foodID,
                Count = count
            };
        }

        public bool Is_COM_MODEL_after_conversion()
        {
            var objectResponse = service.ConvertToZooComModel(ModelType.Animal, animalUnit);

            return objectResponse.GetType() == comModel.GetType();
        }

        public bool Has_same_ID_after_conversion()
        {
            var objectResponse = service.ConvertToZooComModel(ModelType.Animal, animalUnit);
            
            ZooCom.Model.Animal animalObjectAfterConvertion = (ZooCom.Model.Animal)objectResponse;

            return animalObjectAfterConvertion.ID == ID;
        }

        public bool Has_same_PlaceID_after_conversion()
        {
            var objectResponse = service.ConvertToZooComModel(ModelType.Animal, animalUnit);
            
            ZooCom.Model.Animal animalObjectAfterConvertion = (ZooCom.Model.Animal)objectResponse;

            return animalObjectAfterConvertion.PlaceID == placeID;
        }

        public bool Has_same_FoodID_after_conversion()
        {
            var objectResponse = service.ConvertToZooComModel(ModelType.Animal, animalUnit);
            
            ZooCom.Model.Animal animalObjectAfterConvertion = (ZooCom.Model.Animal)objectResponse;

            return animalObjectAfterConvertion.FoodID == foodID;
        }

        public bool Has_same_Count_after_conversion()
        {
            var objectResponse = service.ConvertToZooComModel(ModelType.Animal, animalUnit);
            
            ZooCom.Model.Animal animalObjectAfterConvertion = (ZooCom.Model.Animal)objectResponse;

            return animalObjectAfterConvertion.Count == count;
        }

        public bool Has_same_Name_after_conversion()
        {
            var objectResponse = service.ConvertToZooComModel(ModelType.Animal, animalUnit);
            
            ZooCom.Model.Animal animalObjectAfterConvertion = (ZooCom.Model.Animal)objectResponse;
            
            return animalObjectAfterConvertion.Name == name;
        }

        public bool Has_same_Maintenance_Cost_after_conversion()
        {
            var objectResponse = service.ConvertToZooComModel(ModelType.Animal, animalUnit);
            
            ZooCom.Model.Animal animalObjectAfterConvertion = (ZooCom.Model.Animal)objectResponse;
            
            return animalObjectAfterConvertion.MaintenanceCost == maintenanceCost;
        }

        public bool Are_not_same_objects_after_conversion()
        {
            var objectResponse = service.ConvertToZooComModel(ModelType.Animal, animalUnit);
            
            ZooCom.Model.Animal animalObjectAfterConvertion = (ZooCom.Model.Animal)objectResponse;
            
            return comModel != animalObjectAfterConvertion;
        }
        
        public bool Returns_NULL()
        {
            var response = service.ConvertToZooComModel(ModelType.Model, animalUnit);

            return response == null;
        }
        
        public bool Should_throw_Exception()
        {
            Animal emptyObject = null;

            try
            {
                service.ConvertToZooComModel(ModelType.Animal, emptyObject);
                return false;
            }
            catch (NullReferenceException) { }
            catch
            {
                return false;
            }

            try
            {
                service.ConvertToZooComModel(ModelType.Attraction, emptyObject);
                return false;
            }
            catch (NullReferenceException) { }
            catch
            {
                return false;
            }

            try
            {
                service.ConvertToZooComModel(ModelType.BalanceType, emptyObject);
                return false;
            }
            catch (NullReferenceException) { }
            catch
            {
                return false;
            }

            try
            {
                service.ConvertToZooComModel(ModelType.CashBalance, emptyObject);
                return false;
            }
            catch (NullReferenceException) { }
            catch
            {
                return false;
            }

            try
            {
                service.ConvertToZooComModel(ModelType.Food, emptyObject);
                return false;
            }
            catch (NullReferenceException) { }
            catch
            {
                return false;
            }

            try
            {
                service.ConvertToZooComModel(ModelType.Overtime, emptyObject);
                return false;
            }
            catch (NullReferenceException) { }
            catch
            {
                return false;
            }

            try
            {
                service.ConvertToZooComModel(ModelType.Place, emptyObject);
                return false;
            }
            catch (NullReferenceException) { }
            catch
            {
                return false;
            }

            try
            {
                service.ConvertToZooComModel(ModelType.Worker, emptyObject);
                return false;
            }
            catch (NullReferenceException) { }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
