using Moq;
using ZooService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Test
{
    [TestClass]
    public class GetModelsTest
    {
        ZooService.ZooService service;
        Mock<ZooContext> IZooContext;
        List<Animal> animals;

        [TestInitialize]
        public void SetUp()
        {
            IZooContext = new Mock<ZooContext>();
            service = new ZooService.ZooService(1111);
            
            animals = new List<Animal>();
            
            var animal = new Animal();

            animals.Add(animal);
        }

        [TestMethod]
        public void Returns_not_empty_list()
        {
            IZooContext.Setup(x => x.GetModels<Animal>()).Returns(animals);

            var models = service.GetModels(IZooContext.Object, ZooService.ModelType.Animal);

            Assert.IsTrue(models is List<ZooDataModel>);
            Assert.IsTrue(models != null);
            Assert.IsTrue(models.Count > 0);

        }

        [TestMethod]
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
            Assert.IsTrue(models != null);
            Assert.IsTrue(models.Count == 0);
            models = service.GetModels(IZooContext.Object, ZooService.ModelType.Place);
            Assert.IsTrue(models != null);
            Assert.IsTrue(models.Count == 0);
            models = service.GetModels(IZooContext.Object, ZooService.ModelType.Worker);
            Assert.IsTrue(models != null);
            Assert.IsTrue(models.Count == 0);
            models = service.GetModels(IZooContext.Object, ZooService.ModelType.Attraction);
            Assert.IsTrue(models != null);
            Assert.IsTrue(models.Count == 0);
            models = service.GetModels(IZooContext.Object, ZooService.ModelType.BalanceType);
            Assert.IsTrue(models != null);
            Assert.IsTrue(models.Count == 0);
            models = service.GetModels(IZooContext.Object, ZooService.ModelType.CashBalance);
            Assert.IsTrue(models != null);
            Assert.IsTrue(models.Count == 0);
            models = service.GetModels(IZooContext.Object, ZooService.ModelType.Food);
            Assert.IsTrue(models != null);
            Assert.IsTrue(models.Count == 0);

        }
        
        [TestMethod]
        public void Returns_null()
        {
            var models = service.GetModels(IZooContext.Object, ZooService.ModelType.Model);
            Assert.IsNull(models);
        }
    }
}
