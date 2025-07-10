using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CustomsStmProcessQueueLoaderTest : TestCaseWithFactory
	{
		public void TestLoadCreateProcessQueue()
		{
			Factory.RefreshEnabled = false;
			var dummyBizObj = Factory.New<DummyBusinessObject>();
			var queue = CustomsStmProcessQueueLoader.New(dummyBizObj, CustomsStmProcessQueueLoader.Constants.DocumentGeneratorApplicationCode, "~");
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var dummyBizObjFromNewFactory = newFactory.Load<DummyBusinessObject>(dummyBizObj.PK);

			var queueFromNewFactory = CustomsStmProcessQueueLoader.New(dummyBizObjFromNewFactory, CustomsStmProcessQueueLoader.Constants.DocumentGeneratorApplicationCode, "~");
			AssertNotNull("Expected queue be loaded from new factory", queueFromNewFactory);
			AssertNotEquals("Expected queue to reference the dummyBizObj", dummyBizObj, queueFromNewFactory.SW_ReferenceID);

			queue.Delete();
			Factory.Save();

			queueFromNewFactory = CustomsStmProcessQueueLoader.Load(dummyBizObjFromNewFactory, CustomsStmProcessQueueLoader.Constants.DocumentGeneratorApplicationCode, "~");
			AssertNull("Expected queueFromNewFactory to have been deleted in another factory", queueFromNewFactory);
		}
	}
}
