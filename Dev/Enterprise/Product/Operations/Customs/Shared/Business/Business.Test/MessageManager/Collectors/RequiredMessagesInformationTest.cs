using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class RequiredMessagesInformationTest : TestCaseWithFactory
	{
		public void TestReadOnlyProperties()
		{
			AmendmentWithdrawalReason reason = new AmendmentWithdrawalReason();
			topLevelBizObj.AmendmentWithdrawalReasonExposed = reason;

			topLevelBizObj.SupportBackDoorForSavingWhenAmendmentDetected = true;

			DeferredAmendmentSavingOptions savingOptions = new DeferredAmendmentSavingOptions();
			topLevelBizObj.DeferredAmendmentSavingOptionsExposed = savingOptions;

			RequiredMessagesInformation information = new RequiredMessagesInformation(topLevelBizObj);
			AssertEquals(reason, information.AmendmentWithdrawalReason);
			AssertEquals(true, information.SupportBackDoorForSavingWhenAmendmentDetected);
		}

		public void TestHasMessagesToSend()
		{
			AssertEquals("No messages to send yet", false, information.HasMessagesToSend);

			information.AddBridge(new ManagerCollectorSingleMessageManagerBridge(originalCollector, new SingleMessageManager[] { singleManager }));

			AssertEquals("has messages to send", true, information.HasMessagesToSend);
		}

		public void TestHasMessageManagersWaitingForResponses()
		{
			AssertEquals("Nothing waits for responses ", false, information.HasMessageManagersWaitingForResponses);

			singleManager.IsWaitingForResponseExposed = false;
			information.AddBridge(new ManagerCollectorSingleMessageManagerBridge(originalCollector, new SingleMessageManager[] { singleManager }));

			AssertEquals("Nothing waits for responses ", false, information.HasMessageManagersWaitingForResponses);

			singleManager2.IsWaitingForResponseExposed = true;
			information.AddBridge(new ManagerCollectorSingleMessageManagerBridge(originalCollector, new SingleMessageManager[] { singleManager2 }));

			AssertEquals("Nothing waits for responses ", true, information.HasMessageManagersWaitingForResponses);
		}

		public void TestFullDescriptionsForMessagesToSend()
		{
			AssertEquals("Full Descriptions for messages to send", "", information.FullDescriptionsForMessagesToSend);

			information.AddBridge(new ManagerCollectorSingleMessageManagerBridge(originalCollector, new SingleMessageManager[] { singleManager }));
			AssertEquals("Full Descriptions for messages to send", "\r\n\tapple", information.FullDescriptionsForMessagesToSend);

			information.AddBridge(new ManagerCollectorSingleMessageManagerBridge(originalCollector, new SingleMessageManager[] { singleManager2 }));
			AssertEquals("Full Descriptions for messages to send", "\r\n\tapple\r\n\torange", information.FullDescriptionsForMessagesToSend);
		}

		public void TestMessageTypesToSend()
		{
			AssertEquals("Message Types To Send", "", information.MessageTypesToSend);

			information.AddBridge(new ManagerCollectorSingleMessageManagerBridge(originalCollector, new SingleMessageManager[] { singleManager }));
			AssertEquals("MessageTypesToSend", "original(s)", information.MessageTypesToSend);

			information.AddBridge(new ManagerCollectorSingleMessageManagerBridge(amendmentCollector, new SingleMessageManager[] { singleManager2 }));
			AssertEquals("MessageTypesToSend", "original(s) and amendment(s)", information.MessageTypesToSend);

			information = new RequiredMessagesInformation(topLevelBizObj);
			information.AddBridge(new ManagerCollectorSingleMessageManagerBridge(amendmentCollector, new SingleMessageManager[] { singleManager2 }));
			AssertEquals("MessageTypesToSend", "amendment(s)", information.MessageTypesToSend);
		}

		public void TestManagersToSendMessagesFor()
		{
			List<SingleMessageManager> result = new List<SingleMessageManager>(information.ManagersToSendMessagesFor);
			AssertEquals("ManagersToSendMessagesFor", 0, result.Count);

			information.AddBridge(new ManagerCollectorSingleMessageManagerBridge(originalCollector, new SingleMessageManager[] { singleManager }));
			result = new List<SingleMessageManager>(information.ManagersToSendMessagesFor);
			AssertEquals("ManagersToSendMessagesFor", 1, result.Count);
			AssertEquals("ManagersToSendMessagesFor", singleManager, result[0]);

			information.AddBridge(new ManagerCollectorSingleMessageManagerBridge(amendmentCollector, new SingleMessageManager[] { singleManager2 }));
			result = new List<SingleMessageManager>(information.ManagersToSendMessagesFor);
			AssertEquals("ManagersToSendMessagesFor", 2, result.Count);
			AssertEquals("ManagersToSendMessagesFor", singleManager, result[0]);
			AssertEquals("ManagersToSendMessagesFor", singleManager2, result[1]);
		}

		public void TestBizObjsToSendMessageForForAmendmentOrWithdrawal()
		{
			List<BusinessObject> result = new List<BusinessObject>(information.BizObjsToSendMessageForForAmendmentOrWithdrawal);
			AssertEquals("BizObjsToSendMessageForForAmendmentOrWithdrawal", 0, result.Count);

			information.AddBridge(new ManagerCollectorSingleMessageManagerBridge(originalCollector, new SingleMessageManager[] { singleManager }));
			result = new List<BusinessObject>(information.BizObjsToSendMessageForForAmendmentOrWithdrawal);
			AssertEquals("BizObjsToSendMessageForForAmendmentOrWithdrawal", 0, result.Count);

			information.AddBridge(new ManagerCollectorSingleMessageManagerBridge(amendmentCollector, new SingleMessageManager[] { singleManager2 }));
			result = new List<BusinessObject>(information.BizObjsToSendMessageForForAmendmentOrWithdrawal);
			AssertEquals("BizObjsToSendMessageForForAmendmentOrWithdrawal", 1, result.Count);
			AssertEquals("BizObjsToSendMessageForForAmendmentOrWithdrawal", topLevelBizObj2, result[0]);
		}

		public void TestManagerCollectors()
		{
			List<ManagerCollector> result = new List<ManagerCollector>(information.ManagerCollectors);
			AssertEquals("ManagerCollectors", 0, result.Count);

			information.AddBridge(new ManagerCollectorSingleMessageManagerBridge(originalCollector, new SingleMessageManager[] { singleManager }));
			result = new List<ManagerCollector>(information.ManagerCollectors);
			AssertEquals("ManagerCollectors", 1, result.Count);
			AssertEquals("ManagerCollectors", originalCollector, result[0]);

			information.AddBridge(new ManagerCollectorSingleMessageManagerBridge(amendmentCollector, new SingleMessageManager[] { singleManager2 }));
			result = new List<ManagerCollector>(information.ManagerCollectors);
			AssertEquals("ManagerCollectors", 2, result.Count);
			AssertEquals("ManagerCollectors", originalCollector, result[0]);
			AssertEquals("ManagerCollectors", amendmentCollector, result[1]);
		}

		public void TestAllNotifications()
		{
			AssertEquals("AllNotifications", 0, information.AllNotifications.Count);

			singleManager.AdditionalAmendmentNotifications.AddError("Amendment Error NOT relevant");
			singleManager.AdditionalOriginalNotifications.AddWarning("Original warning relevant");
			information.AddBridge(new ManagerCollectorSingleMessageManagerBridge(originalCollector, new SingleMessageManager[] { singleManager }));
			AssertEquals("AllNotifications", 2, information.AllNotifications.Count);
			AssertEquals("It should not contain AmendmentNotification", false, information.AllNotifications.ContainsError("Amendment Error NOT relevant"));
			AssertEquals("It should contain OriginalWarning", true, information.AllNotifications.ContainsWarning("Original warning relevant"));

			singleManager2.AdditionalAmendmentNotifications.AddError("Amendment Error relevant");
			singleManager2.AdditionalOriginalNotifications.AddWarning("Original warning NOT relevant");
			information.AddBridge(new ManagerCollectorSingleMessageManagerBridge(amendmentCollector, new SingleMessageManager[] { singleManager2 }));
			AssertEquals("AllNotifications", 4, information.AllNotifications.Count);
			AssertEquals("It should contain AmendmentNotification", true, information.AllNotifications.ContainsError("Amendment Error relevant"));
			AssertEquals("It should not contain OriginalWarning", false, information.AllNotifications.ContainsWarning("Original warning NOT relevant"));
		}

		public void TestIsThereAmendmentOrWithdrawal()
		{
			AssertEquals("IsThereAmendmentOrWithdrawal", false, information.IsThereAmendmentOrWithdrawal);

			information.AddBridge(new ManagerCollectorSingleMessageManagerBridge(originalCollector, new SingleMessageManager[] { singleManager }));
			AssertEquals("IsThereAmendmentOrWithdrawal", false, information.IsThereAmendmentOrWithdrawal);

			information.AddBridge(new ManagerCollectorSingleMessageManagerBridge(amendmentCollector, new SingleMessageManager[] { singleManager2 }));
			AssertEquals("IsThereAmendmentOrWithdrawal", true, information.IsThereAmendmentOrWithdrawal);
		}

		public void TestIsThereAnOriginal()
		{
			AssertEquals("IsThereOriginal", false, information.IsThereOriginal);

			information.AddBridge(new ManagerCollectorSingleMessageManagerBridge(amendmentCollector, new SingleMessageManager[] { singleManager2 }));
			AssertEquals("IsThereOriginal", false, information.IsThereOriginal);

			information.AddBridge(new ManagerCollectorSingleMessageManagerBridge(originalCollector, new SingleMessageManager[] { singleManager }));
			AssertEquals("IsThereOriginal", true, information.IsThereOriginal);
		}

		MessageManageableBusinessObject topLevelBizObj;
		MessageManageableBusinessObject topLevelBizObj2;

		RequiredMessagesInformation information;

		TestHelperSingleMessageManager singleManager;
		TestHelperSingleMessageManager singleManager2;

		OriginalManagerCollector originalCollector;
		AmendmentManagerCollector amendmentCollector;

		protected override void SetUp()
		{
			base.SetUp();
			topLevelBizObj = Factory.New<MessageManageableBusinessObject>();
			topLevelBizObj2 = Factory.New<MessageManageableBusinessObject>();

			information = new RequiredMessagesInformation(topLevelBizObj);

			singleManager = new TestHelperSingleMessageManager(topLevelBizObj, "apple");
			singleManager2 = new TestHelperSingleMessageManager(topLevelBizObj2, "orange");

			originalCollector = new OriginalManagerCollector(new SingleMessageManager[] { singleManager }, true);
			amendmentCollector = new AmendmentManagerCollector(new SingleMessageManager[] { singleManager2 }, true);
		}
	}
}
