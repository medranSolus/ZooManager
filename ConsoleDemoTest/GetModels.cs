using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using ZooService;

namespace ConsoleDemoTest
{
    [TestFixture]
    class GetModels
    {
        ZooService.ZooService service;
        Mock<ZooContext> IZooContext;
        List<Animal> animals;

        [SetUp]
        public void SetUp()
        {
            IZooContext = new Mock<ZooContext>();
            service = new ZooService.ZooService(1111);
            
            animals = new List<Animal>();
            
            var animal = new Animal();

            animals.Add(animal);
        }

        [Test]
        public void Returns_not_empty_list()
        {
            IZooContext.Setup(x => x.GetModels<Animal>()).Returns(animals);

            var models = service.GetModels(IZooContext.Object, ZooService.ModelType.Animal);

            Assert.IsTrue(models is List<ZooDataModel>);
            Assert.IsNotEmpty(models);

        }

        [Test]
        public void Returns_empty_list()
        {
            IZooContext.Setup(x => x.GetModels<Overtime>()).Returns(new List<Overtime>());
            IZooContext.Setup(x => x.GetModels<Place>()).Returns(new List<Place>());
            IZooContext.Setup(x => x.GetModels<Worker>()).Returns(new List<Worker>());
            IZooContext.Setup(x => x.GetModels<Attraction>()).Returns(new List<Attraction>());
            IZooContext.Setup(x => x.GetModels<BalanceType>()).Returns(new List<BalanceType>());
            IZooContext.Setup(x => x.GetModels<CashBalance>()).Returns(new List<CashBalance>());
            IZooContext.Setup(x => x.GetModels<Food>()).Returns(new List<Food>());

            var models = service.GetModels(IZooContext.Object, ZooService.ModelType.Overtime);
            CollectionAssert.IsEmpty(models);
            models = service.GetModels(IZooContext.Object, ZooService.ModelType.Place);
            CollectionAssert.IsEmpty(models);
            models = service.GetModels(IZooContext.Object, ZooService.ModelType.Worker);
            CollectionAssert.IsEmpty(models);
            models = service.GetModels(IZooContext.Object, ZooService.ModelType.Attraction);
            CollectionAssert.IsEmpty(models);
            models = service.GetModels(IZooContext.Object, ZooService.ModelType.BalanceType);
            CollectionAssert.IsEmpty(models);
            models = service.GetModels(IZooContext.Object, ZooService.ModelType.CashBalance);
            CollectionAssert.IsEmpty(models);
            models = service.GetModels(IZooContext.Object, ZooService.ModelType.Food);
            CollectionAssert.IsEmpty(models);

        }
        
        [Test]
        public void Returns_null()
        {
            var models = service.GetModels(IZooContext.Object, ZooService.ModelType.Model);
            Assert.IsNull(models);
        }

        static void Main(string[] args)
        { }
    }
}
