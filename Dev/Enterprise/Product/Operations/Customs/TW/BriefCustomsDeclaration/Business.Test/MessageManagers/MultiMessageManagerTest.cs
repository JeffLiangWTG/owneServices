using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.Business;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	sealed class MultiMessageManagerTest : TestCaseWithFactory
	{
		public void TestTopLevelBizObjToManage()
		{
			AssertEquals("TopLevelBizObjToManage", header, multiMessageManager.TopLevelBizObjToManage);
		}

		public void TestSendMessages()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_JobReference = "TWC123456";
			var sendingObjectParent = new MessageSendingObjectParent(header, MessageTypeList.Codes.IBC);
			var msgMultiManager = new MultiMessageManager(sendingObjectParent);
			msgMultiManager.SendMessages(new SendsMessagesToCustomsGUI());
			AssertEquals(1, header.Messages.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
			var wrapper = new MessageSendingObjectParent(header, MessageTypeList.Codes.IBC);
			multiMessageManager = new MultiMessageManager(wrapper);
		}

		MultiMessageManager multiMessageManager;
		AsycudaManifestHeader header;
	}
}
