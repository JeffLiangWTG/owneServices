using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class N5101HMultiMessageManagerTest : TestCaseWithFactory
	{
		public void TestTopLevelBizObjToManage()
		{
			AssertEquals(sendingObjectParent.TopLevelBusinessObject, msgMultiManager.TopLevelBizObjToManage);
		}

		public void TestSendMessagesGroupedByAction()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_JobReference = "TWC123456";
			var bills = header.Bills;
			bills.AddNew();
			bills.AddNew();
			bills.AddNew();
			var sendingObjectParent = new MessageSendingObjectParent(header);
			var sendingObjs = sendingObjectParent.SendingObjectsCollection;
			AssertEquals(3, sendingObjs.Count);
			sendingObjs.Cast<MessageSendingObject>().ForEach(obj => obj.ShouldSend = true);
			sendingObjs[0].Action = "9";
			sendingObjs[1].Action = "5";
			sendingObjs[2].Action = "9";

			var msgMultiManager = new N5101HMultiMessageManager(sendingObjectParent);
			msgMultiManager.SendMessages(new SendsMessagesToCustomsGUI());

			AssertEquals("SendingObjects with same action should be grouped", 2, header.Messages.Count);
		}

		public void TestBillsMessageStatusAfterSendMessages()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_JobReference = "TWC123456";
			var bills = header.Bills;
			bills.AddNew();
			bills.AddNew();
			bills.AddNew();
			var sendingObjectParent = new MessageSendingObjectParent(header);
			var sendingObjs = sendingObjectParent.SendingObjectsCollection;
			sendingObjs.Cast<MessageSendingObject>().ForEach(obj => obj.ShouldSend = true);
			foreach (MessageSendingObject obj in sendingObjs)
			{
				obj.ShouldSend = true;
			}

			var msgMultiManager = new N5101HMultiMessageManager(sendingObjectParent);
			msgMultiManager.SendMessages(new SendsMessagesToCustomsGUI());

			Assert("All bills of message status should be AWR", bills.Cast<AsycudaBill>().All(c => c.ABL_MessageStatus == TWMessageStatusCodeList.Codes.AwaitingResponse));
		}

		public void TestSendWheneverPossibleOnceMessagingActive()
		{
			var manager = new N5101HMultiMessageManagerForTesting(sendingObjectParent);
			AssertEquals("SendWheneverPossibleOnceMessagingActive", true, manager.SendWheneverPossibleOnceMessagingActive_Exposed);
		}

		public void TestSendMessages_LogManifestEvent()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_JobReference = "TWC123456";
			var bills = header.Bills;
			var bill1 = bills.AddNew();
			var bill2 = bills.AddNew();
			var bill3 = bills.AddNew();
			var sendingObjectParent = new MessageSendingObjectParent(header, "send to Customs");
			var sendingObjs = sendingObjectParent.SendingObjectsCollection;

			foreach (MessageSendingObject obj in sendingObjs)
			{
				obj.ShouldSend = true;
				var pk = obj.Bill.PK;
				if (pk == bill1.PK)
				{
					obj.Action = ActionCodeList.Codes.Delete;
				}
				else if (pk == bill2.PK)
				{
					obj.Action = ActionCodeList.Codes.Replace;
				}
				else if (pk == bill3.PK)
				{
					obj.Action = ActionCodeList.Codes.New;
				}
			}

			var msgMultiManager = new N5101HMultiMessageManager(sendingObjectParent);
			msgMultiManager.SendMessages(new SendsMessagesToCustomsGUI());
			CombineAssertions(() =>
			{
				AssertEquals("send to Customs, SendingObjectsCollectionAction='1'", bill1.Logs.Find(a => a.SL_SE_NKEvent == Events.MessageSent.Code).OrderByDescending(b => b.SL_PostedTimeUtc).First().SL_Reference);
				AssertEquals("send to Customs, SendingObjectsCollectionAction='1'", bill1.Logs.Find(a => a.SL_SE_NKEvent == Events.DeclarationCancellationSent.Code).OrderByDescending(b => b.SL_PostedTimeUtc).First().SL_Reference);
				AssertEquals("send to Customs, SendingObjectsCollectionAction='5'", bill2.Logs.Find(a => a.SL_SE_NKEvent == Events.MessageSent.Code).OrderByDescending(b => b.SL_PostedTimeUtc).First().SL_Reference);
				AssertEquals("send to Customs, SendingObjectsCollectionAction='5'", bill2.Logs.Find(a => a.SL_SE_NKEvent == Events.DeclarationAmendmentSent.Code).OrderByDescending(b => b.SL_PostedTimeUtc).First().SL_Reference);
				AssertEquals("send to Customs, SendingObjectsCollectionAction='9'", bill3.Logs.Find(a => a.SL_SE_NKEvent == Events.MessageSent.Code).OrderByDescending(b => b.SL_PostedTimeUtc).First().SL_Reference);
				AssertEquals("send to Customs, SendingObjectsCollectionAction='9'", bill3.Logs.Find(a => a.SL_SE_NKEvent == Events.CustomsCommenced.Code).OrderByDescending(b => b.SL_PostedTimeUtc).First().SL_Reference);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.Bills.AddNew();
			sendingObjectParent = new MessageSendingObjectParent(header);
			msgMultiManager = new N5101HMultiMessageManager(sendingObjectParent);
		}

		MessageSendingObjectParent sendingObjectParent;
		N5101HMultiMessageManager msgMultiManager;

		class N5101HMultiMessageManagerForTesting : N5101HMultiMessageManager
		{
			public N5101HMultiMessageManagerForTesting(MessageSendingObjectParent n5101HWrapper) : base(n5101HWrapper)
			{
			}

			public bool SendWheneverPossibleOnceMessagingActive_Exposed => SendWheneverPossibleOnceMessagingActive;
		}
	}
}
