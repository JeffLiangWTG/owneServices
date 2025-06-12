using System;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubArchiveOnline;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ForwardingPortMessaging.DAKOSY;
using NUnit.Framework;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientFactory;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.Plugins.ForwardingPortMessaging.DAKOSY
{
	class DAKOSYTest : PluginTest<Plugin>
	{

		[Test]
		public void TestPriceItemCodePM1Dakosyham()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = testclient.CC_PK,
				AM_CC_RecipientOutbox = dakosyClient.CC_PK,
				AM_SenderMessageXML = GetMessageXml("MessagePM1Dakosyham"),
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
					Reference1 = "C00001376",
					Reference2 = "S00001699",
					Reference4 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference5 = "DAKOSYHAM",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PM1",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 1
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestPriceItemCodePM2()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = testclient.CC_PK,
				AM_CC_RecipientOutbox = dakosyClient.CC_PK,
				AM_SenderMessageXML = GetMessageXml("MessagePM2"),
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
					Reference1 = "C00001376",
					Reference2 = "S00001699",
					Reference4 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference5 = "DAKOSYHAM",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PM2",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 1
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestPriceItemCodePM3Dakosyham()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = dakosyClient.CC_PK,
				AM_CC_RecipientOutbox = testclient.CC_PK,
				AM_RecipientMessageXML = GetMessageXml("MessagePM3Dakosyham"),
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
					Reference1 = "S601439282",
					Reference2 = "Z16008882120",
					Reference4 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference5 = "DAKOSYHAM",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PM3",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 1
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "S601435254",
					Reference2 = "Z16008846316",
					Reference4 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference5 = "DAKOSYHAM",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PM3",
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
			dakosyClient = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.ServiceProvider, "DAKOSYHAM"));
		}
		eHubClient testclient;
		eHubClient dakosyClient;
	}
}
