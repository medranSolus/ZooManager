using Moq;
using ZooService;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    [TestClass]
    public class DeleteModelTest
    {
        MockSequence ctxSeq;
        Mock<ZooContext> ctx;
        ZooService.ZooService service;

        [TestInitialize]
        public void Setup()
        {
            ctxSeq = new MockSequence();
            ctx = new Mock<ZooContext>();
            ctx.Setup(x => x.DeleteWorker(2)).Returns(new System.Tuple<bool, byte>(false, (byte)ModelType.Attraction));
            ctx.InSequence(ctxSeq).Setup(x => x.DeleteAttraction(1)).Returns(new System.Tuple<bool, byte>(true, 0));
            ctx.InSequence(ctxSeq).Setup(x => x.DeleteWorker(2)).Returns(new System.Tuple<bool, byte>(true, 0));

            service = new ZooService.ZooService(1111);
        }

        [TestMethod]
        public void Delete_worker_before_attraction_fail()
        {
            var ret = service.DeleteModel(ctx.Object, ModelType.Worker, 2);
            Assert.IsFalse(ret.Item1);
            Assert.IsTrue(ret.Item2 == (byte)ModelType.Attraction);
        }

        [TestMethod]
        public void Delete_worker_after_deleting_his_attraction()
        {
            var ret1 = service.DeleteModel(ctx.Object, ModelType.Attraction, 1);
            Assert.IsTrue(ret1.Item1);
            Assert.IsTrue(ret1.Item2 == 0);

            var ret2 = service.DeleteModel(ctx.Object, ModelType.Worker, 2);
            Assert.IsTrue(ret2.Item1);
            Assert.IsTrue(ret2.Item2 == 0);

            ctx.Verify(x => x.DeleteAttraction(1));
            ctx.Verify(x => x.DeleteWorker(2));
        }
    }
}
