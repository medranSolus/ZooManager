using ZooService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Test
{
    [TestClass]
    public class ConvertToZooComModelTest
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
            var objectResponse = service.ConvertToZooComModel(ModelType.Animal, animalUnit);

            Assert.IsInstanceOfType(objectResponse, comModel.GetType());
            ZooCom.Model.Animal animalObjectAfterConvertion = (ZooCom.Model.Animal)objectResponse;

            Assert.AreEqual(animalObjectAfterConvertion.ID, ID);
            Assert.AreEqual(animalObjectAfterConvertion.PlaceID, ID);
            Assert.AreEqual(animalObjectAfterConvertion.FoodID, ID);
            Assert.AreEqual(animalObjectAfterConvertion.Count, intTest);
            Assert.AreEqual(animalObjectAfterConvertion.Name, stringTest);
            Assert.AreEqual(animalObjectAfterConvertion.MaintenanceCost, decimalTest);
            Assert.AreNotSame(comModel, animalObjectAfterConvertion);
        }

        [TestMethod]
        public void Returns_NULL()
        {
            var response = service.ConvertToZooComModel(ModelType.Model, animalUnit);

            Assert.IsNull(response);
        }

        [TestMethod]
        public void Should_throw_Exception()
        {
            Animal emptyObject = null;

            Assert.ThrowsException<NullReferenceException>(() =>
            {
                service.ConvertToZooComModel(ModelType.Animal, emptyObject);
            });

            Assert.ThrowsException<NullReferenceException>(() =>
            {
                service.ConvertToZooComModel(ModelType.Attraction, emptyObject);
            });

            Assert.ThrowsException<NullReferenceException>(() =>
            {
                service.ConvertToZooComModel(ModelType.BalanceType, emptyObject);
            });

            Assert.ThrowsException<NullReferenceException>(() =>
            {
                service.ConvertToZooComModel(ModelType.CashBalance, emptyObject);
            });

            Assert.ThrowsException<NullReferenceException>(() =>
            {
                service.ConvertToZooComModel(ModelType.Food, emptyObject);
            });

            Assert.ThrowsException<NullReferenceException>(() =>
            {
                service.ConvertToZooComModel(ModelType.Overtime, emptyObject);
            });

            Assert.ThrowsException<NullReferenceException>(() =>
            {
                service.ConvertToZooComModel(ModelType.Place, emptyObject);
            });

            Assert.ThrowsException<NullReferenceException>(() =>
            {
                service.ConvertToZooComModel(ModelType.Worker, emptyObject);
            });
        }
    }
}
