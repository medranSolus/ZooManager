using Moq;
using ZooService;
using fitlibrary;

namespace FitTesto
{
    public class DeleteModelTest : DoFixture
    {
        MockSequence ctxSeq;
        Mock<ZooContext> ctx;
        ZooService.ZooService service;
        
        public void SetUp()
        {
            ctxSeq = new MockSequence();
            ctx = new Mock<ZooContext>();
            ctx.Setup(x => x.DeleteWorker(2)).Returns(new System.Tuple<bool, byte>(false, (byte)ModelType.Attraction));
            ctx.InSequence(ctxSeq).Setup(x => x.DeleteAttraction(1)).Returns(new System.Tuple<bool, byte>(true, 0));
            ctx.InSequence(ctxSeq).Setup(x => x.DeleteWorker(2)).Returns(new System.Tuple<bool, byte>(true, 0));

            service = new ZooService.ZooService(1111);
        }
        
        public bool Delete_worker_before_attraction_fail()
        {
            var ret = service.DeleteModel(ctx.Object, ModelType.Worker, 2);
            return ret.Item1 == false && ret.Item2 == (byte)ModelType.Attraction;
        }
        
        public bool Delete_worker_after_deleting_his_attraction()
        {
            var ret1 = service.DeleteModel(ctx.Object, ModelType.Attraction, 1);
            if (ret1.Item1 != true || ret1.Item2 != 0)
                return false;

            var ret2 = service.DeleteModel(ctx.Object, ModelType.Worker, 2);
            
            return ret2.Item1 && ret2.Item2 == 0;
        }
    }
}
