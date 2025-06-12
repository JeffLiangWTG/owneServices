using System;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubArchiveOnline;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ForwardingPortMessaging.TPT;
using NUnit.Framework;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientFactory;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.Plugins.ForwardingPortMessaging.TPT
{
	class TPTTest : PluginTest<Plugin>
	{
		[Test]
		public void TestPriceItemCodePOZ_ZATPT()
		{
			var messageReceivedTime = new DateTime(2017, 8, 8, 12, 07, 0);
			var messageArchivedTime = new DateTime(2017, 8, 8, 12, 11, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_ApplicationCode = "BIZ",
				AM_CC_SenderInbox = testclient.CC_PK,
				AM_CC_RecipientOutbox = tptClient.CC_PK,
				AM_SenderMessageXML = GetMessageXml("MessagePOZZATPTSender"),
				AM_RecipientMessageXML = GetMessageXml("MessagePOZZATPTRecipient"),
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
					Reference1 = "C00679885_JSA201706025000111",
					Reference2 = "Service Instruction - Shipping Order",
					Reference3 = "Original",
					Reference4 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference5 = string.Empty,
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "POZ",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 1
				}),
			};
			var actualTransactions = RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1));
			Assert.That(actualTransactions, Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestPriceItemCodePSZ_ZATPT()
		{
			var messageReceivedTime = new DateTime(2017, 8, 8, 12, 07, 0);
			var messageArchivedTime = new DateTime(2017, 8, 8, 12, 11, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_ApplicationCode = "BIZ",
				AM_CC_SenderInbox = tptClient.CC_PK,
				AM_CC_RecipientOutbox = testclient.CC_PK,
				AM_RecipientMessageXML = GetMessageXml("MessagePSZZATPT"),
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
					Reference1 = "C00679036_16DE810648434537E0",
					Reference2 = "STU",
					Reference3 = "Port Status",
					Reference4 = "3705518950",
					Reference5 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PSZ",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 1
				})
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			testclient = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "TSTCLIENT"));
			tptClient = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.ServiceProvider, "TPT"));
		}
		eHubClient testclient;
		eHubClient tptClient;
	}
}
