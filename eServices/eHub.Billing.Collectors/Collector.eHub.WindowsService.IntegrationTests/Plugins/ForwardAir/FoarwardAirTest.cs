using System;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubArchiveOnline;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ForwardAir;
using NUnit.Framework;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientFactory;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.Plugins.ForwardAir
{
	class ForwardAirTest : PluginTest<Plugin>
	{

		[Test]
		public void TestMessageToForwardAir()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = sender.CC_PK,
				AM_CC_RecipientOutbox = forwardAir.CC_PK,
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_SenderMessageXML = GetMessageXml("MessageToForwardAir"),
				AM_InboxMessageTrackingID = new Guid("{790BE798-2F09-4BA2-BC9A-458648472B6B}"),
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTENTSEN",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference2 = "CSTD00677824",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "FWA",
					PriceItemCode = "FWA",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				})
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestMessageFromForwardAir()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = forwardAir.CC_PK,
				AM_CC_RecipientOutbox = recipient.CC_PK,
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_RecipientMessageXML = GetMessageXml("MessageFromForwardAir"),
				AM_OutboxMessageTrackingID = new Guid("{790BE798-2F09-4BA2-BC9A-458648472B6B}"),
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTENTRCV",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference2 = "C00419309",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "FWA",
					PriceItemCode = "FWA",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				})
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			sender = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "TSTENTSEN"));

			forwardAir = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.ServiceProvider, "FORAIRCMH"));
			recipient = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "TSTENTRCV"));
		}

		eHubClient sender;
		eHubClient forwardAir;
		eHubClient recipient;
	}
}
