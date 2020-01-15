using ZooService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Test
{
    [TestClass]
    public class ConvertFromZooComModelTest
    {
        ZooService.ZooService service;
        Animal animalUnit;
        object comModel;

        const int ID = 1;
        const decimal decimalTest = 5;
        const string stringTest = "testString";
        const int intTest = 50;
                
        [TestInitialize]
        public void SetUp()
        {
            service = new ZooService.ZooService(1111);

            comModel = new ZooCom.Model.Animal
            {
                ID = ID,
                MaintenanceCost = decimalTest,
                Name = stringTest,
                PlaceID = ID,
                FoodID = ID,
                Count = intTest
            };

            animalUnit = new Animal
            {
                ID = ID,
                MaintenanceCost = decimalTest,
                Name = stringTest,
                PlaceID = ID,
                FoodID = ID,
                Count = intTest
            };
        }

        [TestMethod]
        public void Has_the_same_fields_values_after_conversion()
        {
            var objectResponse = service.ConvertFromZooComModel(comModel, ZooService.ModelType.Animal);

            Assert.IsInstanceOfType(objectResponse, animalUnit.GetType());
            Animal animalObjectAfterConvertion = (Animal)objectResponse;

            Assert.AreEqual(animalObjectAfterConvertion.ID, ID);
            Assert.AreEqual(animalObjectAfterConvertion.PlaceID, ID);
            Assert.AreEqual(animalObjectAfterConvertion.FoodID, ID);
            Assert.AreEqual(animalObjectAfterConvertion.Count, intTest);
            Assert.AreEqual(animalObjectAfterConvertion.Name, stringTest);
            Assert.AreEqual(animalObjectAfterConvertion.MaintenanceCost,decimalTest);
            Assert.AreNotSame(animalUnit, animalObjectAfterConvertion);
        }

        [TestMethod]
        public void Returns_NULL()
        {
            var response = service.ConvertFromZooComModel(comModel, ZooService.ModelType.Model);

            Assert.IsNull(response);
        }

        [TestMethod]
        public void Should_throw_Exception()
        {
            ZooCom.Model.Animal emptyObject = null;

            Assert.ThrowsException<NullReferenceException>(() =>
            {
                service.ConvertFromZooComModel(emptyObject, ZooService.ModelType.Animal);
            });

            Assert.ThrowsException<NullReferenceException>(() =>
            {
                service.ConvertFromZooComModel(emptyObject, ZooService.ModelType.Attraction);
            });

            Assert.ThrowsException<NullReferenceException>(() =>
            {
                service.ConvertFromZooComModel(emptyObject, ZooService.ModelType.BalanceType);
            });

            Assert.ThrowsException<NullReferenceException>(() =>
            {
                service.ConvertFromZooComModel(emptyObject, ZooService.ModelType.CashBalance);
            });

            Assert.ThrowsException<NullReferenceException>(() =>
            {
                service.ConvertFromZooComModel(emptyObject, ZooService.ModelType.Food);
            });

            Assert.ThrowsException<NullReferenceException>(() =>
            {
                service.ConvertFromZooComModel(emptyObject, ZooService.ModelType.Overtime);
            });
            
            Assert.ThrowsException<NullReferenceException>(() =>
            {
                service.ConvertFromZooComModel(emptyObject, ZooService.ModelType.Place);
            });

            Assert.ThrowsException<NullReferenceException>(() =>
            {
                service.ConvertFromZooComModel(emptyObject, ZooService.ModelType.Worker);
            });
        }
    }
}
