using System;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubArchiveOnline;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.RailincMessaging;
using NUnit.Framework;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientFactory;
using System.Linq;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.Plugins.RailincMessaging
{
	class RailincMessagingTest : PluginTest<Plugin>
	{

		[Test]
		public void TestMessageToRailinc()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = client.CC_PK,
				AM_CC_RecipientOutbox = railinc.CC_PK,
				AM_RecipientMessageXML = GetMessageXml("MessageToRailinc"),
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
					Reference1 = "CLU",
					Reference2 = "B00447956SE15",
					Reference3 = "TCNU9471579",
					Reference4 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "RIM",
					PriceItemCode = "RIM",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 2,
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "CLU",
					Reference2 = "B00447956SE15",
					Reference3 = "TCNU5675770",
					Reference4 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "RIM",
					PriceItemCode = "RIM",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 2,
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestMessageFromRailinc_OutboxUniversalEvents()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_BillingElementCount = 0,
				AM_CC_SenderInbox = railinc.CC_PK,
				AM_CC_RecipientOutbox = client.CC_PK,
				AM_RecipientMessageXML = GetMessageXml("OutboxUniversalofInboundMessage"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_OutboxMessageTrackingID = new Guid("790BE798-2F09-4BA2-BC9A-458648472B6B"),
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "TRLU7096892",
					Reference2 = "C00217807",
					Reference3 = "CLM",
					Reference4 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference5 = "DEP [A]-NELSONS WI",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "RIM",
					PriceItemCode = "RIC",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 2,
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "TRLU7096891",
					Reference2 = "C00217806",
					Reference3 = "CLM",
					Reference4 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference5 = "ARV [E]-RAILPORT IL",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "RIM",
					PriceItemCode = "RIC",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 2,
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestMessageFromRailinc_ToContainerTracking_ShouldNotGenerateTransactions()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_BillingElementCount = 0,
				AM_CC_SenderInbox = railinc.CC_PK,
				AM_CC_RecipientOutbox = containerTracking.CC_PK,
				AM_RecipientMessageXML = GetMessageXml("OutboxUniversalofInboundMessage"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_OutboxMessageTrackingID = new Guid("790BE798-2F09-4BA2-BC9A-458648472B6B"),
			};
			AddArchiveMessage(message);
			
			var transactions = RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1));
			Assert.AreEqual(transactions.Count(), 0);
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			client = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "TSTCLIENT"));
			railinc = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.ServiceProvider, "RAILINCFC"));
			containerTracking = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "CONTAINER_TRACKING"));
		}
		eHubClient client;
		eHubClient railinc;
		eHubClient containerTracking;
	}
}
