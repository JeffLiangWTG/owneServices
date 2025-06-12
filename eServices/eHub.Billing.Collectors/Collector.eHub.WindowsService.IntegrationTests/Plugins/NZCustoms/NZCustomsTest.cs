using System;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubArchiveOnline;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.NZCustoms;
using NUnit.Framework;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientFactory;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.Plugins.NZCustoms
{
	class NZCustomsTest : PluginTest<Plugin>
	{

		[Test]
		public void TestMessageFromNZCustomsToClient()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var messageToJPCustoms = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_ApplicationCode = "NZC",
				AM_CC_SenderInbox = NZCustoms.CC_PK,
				AM_CC_RecipientOutbox = client.CC_PK,
				AM_SenderMessageRaw = GetMessageXml("MessageFromNZCustomsRaw"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_OutboxMessageTrackingID = new Guid("{790BE798-2F09-4BA2-BC9A-458648472B6B}"),
			};
			AddArchiveMessage(messageToJPCustoms);
			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "NZC",
					PriceItemCode = "NZC",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "B00002099",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "NZC",
					PriceItemCode = "CRE",
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestMessageFromClientToNZCustoms()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var messageToJPCustoms = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_ApplicationCode = "NZC",
				AM_CC_SenderInbox = client.CC_PK,
				AM_CC_RecipientInbox = NZCustoms.CC_PK,
				AM_SenderMessageRaw = GetMessageXml("MessageToNZCustomsRaw"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("{790BE798-2F09-4BA2-BC9A-458648472B6B}"),
			};
			AddArchiveMessage(messageToJPCustoms);
			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "NZC",
					PriceItemCode = "NZC",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				}),

				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "B00002858",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "NZC",
					PriceItemCode = "DEC",
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();

			client = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "TSTCLIENT"));
			NZCustoms = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.ServiceProvider, "NZCustoms"));
		}
		eHubClient client;
		eHubClient NZCustoms;
	}
}
