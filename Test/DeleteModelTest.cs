using Moq;
using ZooService;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    [TestClass]
    public class DeleteModelTest
    {
        [TestInitialize]
        public void Setup()
        {
        }

        [TestMethod]
        public void Check_if_worker_is_not_assigned_to_attraction()
        {//https://riptutorial.com/moq/example/23018/validating-call-order-with-mocksequence
            var seq = new MockSequence();
            var context = new Mock<ZooContext>();
            context.Setup(ctx => ctx.DeleteWorker(2)).Returns(new System.Tuple<bool, byte>(false, (byte)ModelType.Animal));
            context.Setup(ctx => ctx.DeleteAttraction(1)).Returns(new System.Tuple<bool, byte>(true, 0));
            //context.InSequence(seq).Setup(x => x.DeleteAttraction(1));
            //context.InSequence(seq).Setup(x => x.DeleteWorker(2));

            context.Object.DeleteAttraction(1);
            var ret = context.Object.DeleteWorker(2);
            Assert.IsFalse(ret.Item1);
            Assert.IsTrue(ret.Item2 == (byte)ModelType.Animal);

            context.Verify(x => x.DeleteAttraction(1));
            context.Verify(x => x.DeleteWorker(2));
        }
    }
}
