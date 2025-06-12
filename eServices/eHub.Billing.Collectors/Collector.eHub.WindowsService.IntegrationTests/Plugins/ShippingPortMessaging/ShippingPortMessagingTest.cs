using System;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubArchiveOnline;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ShippingPortMessaging;
using NUnit.Framework;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientFactory;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.Plugins.ShippingPortMessaging
{
	class ShippingPortMessagingTest : PluginTest<Plugin>
	{

		[Test]
		public void TestCOPARNMessageType()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = client.CC_PK,
				AM_CC_RecipientOutbox = recipient.CC_PK,
				AM_CC_RecipientInbox = shippingPortMessaging.CC_PK,
				AM_SenderMessageXML = GetMessageXml("COPARN"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("790BE798-2F09-4BA2-BC9A-458648472B6B"),
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference2 = "RECIPIENT Friendly Name",
					Reference3 = "PER",
					Reference4 = "B00005684-5",
					Reference5 = "1",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "SPM",
					PriceItemCode = "SPA",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference2 = "RECIPIENT Friendly Name",
					Reference3 = "PER",
					Reference4 = "B00005684-5",
					Reference5 = "2",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "SPM",
					PriceItemCode = "SPA",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestCOREORMessageType()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = client.CC_PK,
				AM_CC_RecipientOutbox = recipient.CC_PK,
				AM_CC_RecipientInbox = shippingPortMessaging.CC_PK,
				AM_SenderMessageXML = GetMessageXml("COREOR"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("790BE798-2F09-4BA2-BC9A-458648472B6B"),
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference2 = "RECIPIENT Friendly Name",
					Reference3 = "PIR",
					Reference4 = "UETU2371126",
					Reference5 = "D01406981",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "SPM",
					PriceItemCode = "SPE",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestCOPRARRMessageType()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = client.CC_PK,
				AM_CC_RecipientOutbox = recipient.CC_PK,
				AM_CC_RecipientInbox = shippingPortMessaging.CC_PK,
				AM_SenderMessageXML = GetMessageXml("COPRAR"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("790BE798-2F09-4BA2-BC9A-458648472B6B"),
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference2 = "RECIPIENT Friendly Name",
					Reference3 = "PIM",
					Reference4 = "V00001013",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "SPM",
					PriceItemCode = "SPR",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			client = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "TSTCLIENT"));
			recipient = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "RECIPIENT"));
			shippingPortMessaging = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.ServiceProvider, "ShippingPortMessaging"));
		}
		eHubClient client;
		eHubClient recipient;
		eHubClient shippingPortMessaging;
	}
}
