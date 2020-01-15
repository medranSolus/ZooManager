using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZooService;

namespace ConsoleDemoTest
{
    [TestFixture]
    class ConvertFromZooComModel
    {

        ZooService.ZooService service;
        object comModel;

        const int ID = 1;
        const decimal decimalTest = 5;
        const string stringTest = "testString";
        const int intTest= 50;

        Animal animalUnit;
        
        [SetUp]
        public void SetUp()
        {
            service = new ZooService.ZooService(1111);

            animalUnit = new Animal();
            animalUnit.ID = ID;
            animalUnit.MaintenanceCost = decimalTest;
            animalUnit.Name = stringTest;
            animalUnit.PlaceID = ID;
            animalUnit.FoodID = ID;
            animalUnit.Count = intTest;

            ZooDataModel model = (ZooDataModel)animalUnit;

            comModel = service.ConvertFromZooComModel(model, ZooService.ModelType.Animal);
        }

        [Test]
        public void Has_the_same_fields_values_after_conversion()
        {
            var objectResponse = service.ConvertFromZooComModel(comModel, ZooService.ModelType.Animal);

            Assert.IsInstanceOf(objectResponse.GetType(), objectResponse);
            Animal animalObjectAfterConvertion = (Animal)objectResponse;

            Assert.AreEqual(animalObjectAfterConvertion.ID, ID);
            Assert.AreEqual(animalObjectAfterConvertion.PlaceID, ID);
            Assert.AreEqual(animalObjectAfterConvertion.FoodID, ID);
            Assert.AreEqual(animalObjectAfterConvertion.Count, intTest);
            Assert.AreEqual(animalObjectAfterConvertion.Name, stringTest);
            Assert.AreEqual(animalObjectAfterConvertion.MaintenanceCost,decimalTest);
            Assert.AreNotSame(animalUnit, animalObjectAfterConvertion);
        }

        [Test]
        public void Returns_NULL()
        {
            var response = service.ConvertFromZooComModel(comModel, ZooService.ModelType.Model);

            Assert.IsNull(response);
        }

        [Test]
        public void Should_throw_Exception()
        {
            ZooDataModel emptyObject = (ZooDataModel) new Animal();

            Assert.Throws<NullReferenceException>(() =>
            {
                service.ConvertFromZooComModel(emptyObject, ZooService.ModelType.Animal);
            });

            Assert.Throws<NullReferenceException>(() =>
            {
                service.ConvertFromZooComModel(emptyObject, ZooService.ModelType.Attraction);
            });

            Assert.Throws<NullReferenceException>(() =>
            {
                service.ConvertFromZooComModel(emptyObject, ZooService.ModelType.BalanceType);
            });

            Assert.Throws<NullReferenceException>(() =>
            {
                service.ConvertFromZooComModel(emptyObject, ZooService.ModelType.CashBalance);
            });

            Assert.Throws<NullReferenceException>(() =>
            {
                service.ConvertFromZooComModel(emptyObject, ZooService.ModelType.Food);
            });

            Assert.Throws<NullReferenceException>(() =>
            {
                service.ConvertFromZooComModel(emptyObject, ZooService.ModelType.Overtime);
            });


            Assert.Throws<NullReferenceException>(() =>
            {
                service.ConvertFromZooComModel(emptyObject, ZooService.ModelType.Place);
            });

            Assert.Throws<NullReferenceException>(() =>
            {
                service.ConvertFromZooComModel(emptyObject, ZooService.ModelType.Worker);
            });
        }

    }
}
