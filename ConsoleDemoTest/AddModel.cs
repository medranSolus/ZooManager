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
    class AddModel
    {

        Mock<ZooContext> IZooContext;
        ZooService.ZooService service;

        [SetUp]
        public void Setup()
        {
            IZooContext = new Mock<ZooContext>();
            service = new ZooService.ZooService(1111);
        }

        [Test]
        public void Should_add_object()
        {
            Animal unit = new Animal()
            {
                ID = 1,
                Name = "tiger",
                Count = 50,
                FoodID = 1,
                PlaceID = 1,
                MaintenanceCost = 5
            };

            IZooContext.Setup(x => x.AddModel<Animal>(unit)).Returns(true);

            var response = service.AddModel(IZooContext.Object, ZooService.ModelType.Animal, unit);

            Assert.IsTrue(response);
        }

        [Test]
        public void Should_return_false_on_non_existing_method()
        {
            Animal unit = new Animal()
            {
                ID = 1,
                Name = "tiger",
                Count = 50,
                FoodID = 1,
                PlaceID = 1,
                MaintenanceCost = 5
            };

            IZooContext.Setup(x => x.AddModel<Animal>(unit)).Returns(true);

            var response = service.AddModel(IZooContext.Object, ZooService.ModelType.Model, (ZooDataModel) unit);

            Assert.IsFalse(response);
        }
    }
}
