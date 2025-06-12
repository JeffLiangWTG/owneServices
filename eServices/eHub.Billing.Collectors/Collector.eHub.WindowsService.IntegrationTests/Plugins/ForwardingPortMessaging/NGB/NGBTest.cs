using System;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubArchiveOnline;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ForwardingPortMessaging.NGB;
using NUnit.Framework;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientFactory;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.Plugins.ForwardingPortMessaging.NGB
{
	class NGBTest : PluginTest<Plugin>
	{

		[Test]
		public void TestPriceItemCodePMRFPM()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = testclient.CC_PK,
				AM_CC_RecipientInbox = fpmClient.CC_PK,
				AM_SenderMessageXML = GetMessageXml("MessagePRCFPM"),
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
					Reference1 = "C1900329069",
					Reference2 = "eTerminal Release Manifest",
					Reference3 = "Original",
					Reference4 = "SOROE MAERSK-923E",
					Reference5 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PMR",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 1
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "C11111",
					Reference2 = "eTerminal Release Manifest",
					Reference3 = "Original",
					Reference4 = "SOROE MAERSK-923E",
					Reference5 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PMR",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 1
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestPriceItemCodePML()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = testclient.CC_PK,
				AM_CC_RecipientInbox = fpmClient.CC_PK,
				AM_SenderMessageXML = GetMessageXml("MessagePLCFPM"),
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
					Reference1 = "C1900346186",
					Reference2 = "Container Load Plan",
					Reference3 = "Amendment",
					Reference4 = "111",
					Reference5 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PML",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 1
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "C1900346186",
					Reference2 = "Container Load Plan",
					Reference3 = "Amendment",
					Reference4 = "222",
					Reference5 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PML",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 1
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			testclient = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "TSTCLIENT"));
			fpmClient = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.ServiceProvider, "FORWARDING_PORT_MESSAGE"));
		}
		eHubClient testclient;
		eHubClient fpmClient;
	}
}
