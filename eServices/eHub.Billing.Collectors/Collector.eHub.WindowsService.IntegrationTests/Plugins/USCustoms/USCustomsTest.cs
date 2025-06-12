using System;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubArchiveOnline;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientFactory;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.USCustoms;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.Plugins.USCustoms
{
	class USCustomsTest : PluginTest<Plugin>
	{
		[Test]
		public void TestApplicationCodeAMA_GeneratesNoPriceItemCode()
		{
			var registry = new eHubUSCustomsRegistry
			{
				ER_CC_Client = client.CC_PK,
				ER_ApplicationCode = "AMA",
				ER_Name = "Participant Originator Code",
				ER_Value = "WTGTAAF",
				ER_IsProduction = true,
			};
			AddUSCustomsRegistry(registry);

			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ApplicationCode = "AMA",
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = USCustoms.CC_PK,
				AM_CC_RecipientOutbox = client.CC_PK,
				AM_RecipientMessageRaw = GetMessageXml("ApplicationCodeUSI"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_OutboxMessageTrackingID = new Guid("790BE798-2F09-4BA2-BC9A-458648472B6B"),
			};
			AddArchiveMessage(message);

			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.Empty);
		}

		[Test]
		public void TestApplicationCodeUSI_NotGeneratePriceItemCodeUSIAnyMore()
		{
			var registry = new eHubUSCustomsRegistry
			{
				ER_CC_Client = client.CC_PK,
				ER_ApplicationCode = "USI",
				ER_Name = "Entry Filer Code",
				ER_Value = "9WB",
				ER_IsProduction = true,
			};
			AddUSCustomsRegistry(registry);

			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ApplicationCode = "USI",
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = USCustoms.CC_PK,
				AM_CC_RecipientOutbox = client.CC_PK,
				AM_RecipientMessageRaw = GetMessageXml("ApplicationCodeUSI"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_OutboxMessageTrackingID = new Guid("790BE798-2F09-4BA2-BC9A-458648472B6B"),
			};
			AddArchiveMessage(message);

			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.Empty);
		}

		[Test]
		public void TestApplicationCodeUSE()
		{
			var registry = new eHubUSCustomsRegistry
			{
				ER_CC_Client = client.CC_PK,
				ER_ApplicationCode = "USE",
				ER_Name = "Entry Filer Code",
				ER_Value = "364135615",
				ER_IsProduction = true,
			};
			AddUSCustomsRegistry(registry);

			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ApplicationCode = "USE",
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = USCustoms.CC_PK,
				AM_CC_RecipientOutbox = client.CC_PK,
				AM_RecipientMessageRaw = GetMessageXml("ApplicationCodeUSE"),
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
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "USC",
					PriceItemCode = "USE",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "X20160204894071",
					Reference3 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "USC",
					PriceItemCode = "UXT",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestApplicationCodeUSI_GeneratePriceItemCodeISF()
		{
			var registry = new eHubUSCustomsRegistry
			{
				ER_CC_Client = client.CC_PK,
				ER_ApplicationCode = "USI",
				ER_Name = "Entry Filer Code",
				ER_Value = "AN7",
				ER_IsProduction = true,
			};
			AddUSCustomsRegistry(registry);

			var messageReceivedTime = new DateTime(2016, 4, 21, 21, 0, 0);
			var messageArchivedTime = new DateTime(2016, 4, 21, 21, 23, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ApplicationCode = "USI",
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = USCustoms.CC_PK,
				AM_CC_RecipientOutbox = client.CC_PK,
				AM_RecipientMessageRaw = GetMessageXml("ISF"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_OutboxMessageTrackingID = new Guid("7353A8FD-F921-40A9-AC83-33B57996255B"),
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "4701",
					Reference2 = "AN7-81669348021",
					Reference3 = "7353a8fd-f921-40a9-ac83-33b57996255b",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "USC",
					PriceItemCode = "ISF",
					MessageTrackingID = "7353a8fd-f921-40a9-ac83-33b57996255b",
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestApplicationCodeMAN_NotGeneratePriceItemCodeMANAnyMore()
		{
			var registry = new eHubUSCustomsRegistry
			{
				ER_CC_Client = client.CC_PK,
				ER_ApplicationCode = "MAN",
				ER_Name = "Entry Filer Code",
				ER_Value = "9WB",
				ER_IsProduction = true,
			};
			AddUSCustomsRegistry(registry);

			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ApplicationCode = "MAN",
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = USCustoms.CC_PK,
				AM_CC_RecipientOutbox = client.CC_PK,
				AM_RecipientMessageRaw = GetMessageXml("ApplicationCodeMAN"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_OutboxMessageTrackingID = new Guid("E44060AD-474E-48C0-A61B-6E5B85B567D7"),
			};
			AddArchiveMessage(message);

			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.Empty);
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			client = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "TSTCLIENT"));

			USCustoms = eHubClientFactory.CreateeHubClient(eHubClientType.ServiceProvider, "USC");
			USCustoms.CC_PK = new Guid("146e2f62-f80e-49bb-b8fa-c46fd057af4b");
			AddClient(USCustoms);

		}
		eHubClient client;
		eHubClient USCustoms;
	}
}
