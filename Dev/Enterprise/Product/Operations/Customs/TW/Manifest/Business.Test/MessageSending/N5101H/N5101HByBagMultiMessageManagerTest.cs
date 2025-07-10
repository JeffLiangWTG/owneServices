using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class N5101HByBagMultiMessageManagerTest : TestCaseWithFactory
	{
		public void TestSendMessagesGroupedByActionAndBagNumber()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.BagNumber = "B1";
			var bill2 = header.Bills.AddNew();
			bill2.BagNumber = "B1";
			var bill3 = header.Bills.AddNew();
			bill3.BagNumber = "B1";
			var bill4 = header.Bills.AddNew();
			bill4.BagNumber = "B2";
			var sendingObjectParent = new MessageSendingObjectParent(header);
			var sendingObjs = sendingObjectParent.SendingObjectsCollection;
			AssertEquals(4, sendingObjs.Count);
			foreach (MessageSendingObject obj in sendingObjs)
			{
				obj.ShouldSend = true;
			}
			sendingObjs[0].Action = "9";
			sendingObjs[1].Action = "5";
			sendingObjs[2].Action = "9";
			sendingObjs[2].Action = "9";

			var msgMultiManager = new N5101HByBagMultiMessageManager(sendingObjectParent);
			msgMultiManager.SendMessages(new SendsMessagesToCustomsGUI());
			AssertEquals("SendingObjects with same action and bag number should be grouped", 3, header.Messages.Count);
		}
	}
}
