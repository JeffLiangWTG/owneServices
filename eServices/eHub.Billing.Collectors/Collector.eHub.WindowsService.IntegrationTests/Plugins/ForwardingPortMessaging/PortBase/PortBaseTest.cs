using System;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubArchiveOnline;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ForwardingPortMessaging.PortBase;
using NUnit.Framework;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientFactory;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.Plugins.ForwardingPortMessaging.PortBase
{
	class PortBaseTest : PluginTest<Plugin>
	{
		[Test]
		public void TestImportMessageFromCW1ToPortbasePMN_NoImportReferenceNumber_UseENNumber()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = testclient.CC_PK,
				AM_CC_RecipientInbox = portbaseClient.CC_PK,
				AM_SenderMessageXML = GetMessageXml("ImportMessagePMN_NoImportReference_UseENNumber"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("790BE798-2F09-4BA2-BC9A-458648472B6B")
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "C600799752",
					Reference2 = "0",
					Reference3 = "PORTBASE",
					Reference4 = null,
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PMN",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 1
				})
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestImportMessageFromCW1ToPortbasePMN_DuplicatedImportReferences_SingleTransaction()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = testclient.CC_PK,
				AM_CC_RecipientInbox = portbaseClient.CC_PK,
				AM_SenderMessageXML = GetMessageXml("ImportMessagePMN_DuplicatedImportReferences_SingleTransaction"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("790BE798-2F09-4BA2-BC9A-458648472B6B")
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "C600799752",
					Reference2 = "1",
					Reference3 = "PORTBASE",
					Reference4 = null,
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PMN",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 1
				})
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestImportMessageFromCW1ToPortbasePMN_MultipleSubShipments_MultipleTransactions()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = testclient.CC_PK,
				AM_CC_RecipientInbox = portbaseClient.CC_PK,
				AM_SenderMessageXML = GetMessageXml("ImportMessagePMN_MultipleSubShipments_MultipleTransactions"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("790BE798-2F09-4BA2-BC9A-458648472B6B")
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "C600799752",
					Reference2 = "1",
					Reference3 = "PORTBASE",
					Reference4 = null,
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PMN",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 1
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "C600799752",
					Reference2 = "2",
					Reference3 = "PORTBASE",
					Reference4 = null,
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PMN",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 1
				})
			};

			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestImportMessageFromCW1ToPortbasePMN_CombineAllCases_MultipleTransactions()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = testclient.CC_PK,
				AM_CC_RecipientInbox = portbaseClient.CC_PK,
				AM_SenderMessageXML = GetMessageXml("ImportMessagePMN_CombineAllCases_MultipleTransactions"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("790BE798-2F09-4BA2-BC9A-458648472B6B")
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "C600799752",
					Reference2 = "0",
					Reference3 = "PORTBASE",
					Reference4 = null,
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PMN",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 1
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "C600799752",
					Reference2 = "1",
					Reference3 = "PORTBASE",
					Reference4 = null,
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PMN",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 1
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "C600799752",
					Reference2 = "2",
					Reference3 = "PORTBASE",
					Reference4 = null,
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PMN",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 1
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "C600799752",
					Reference2 = "3",
					Reference3 = "PORTBASE",
					Reference4 = null,
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PMN",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 1
				})
			};

			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestMessageFromCW1ToPortbasePMN_MultipleMRN_SingleTransaction()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = testclient.CC_PK,
				AM_CC_RecipientInbox = portbaseClient.CC_PK,
				AM_SenderMessageXML = GetMessageXml("MessagePMN_SingleMRN"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("790BE798-2F09-4BA2-BC9A-458648472B6B")
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "C600799752",
					Reference2 = "16DE930346632679E1",
					Reference3 = "PORTBASE",
					Reference4 = null,
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PMN",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 1
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestMessageFromCW1ToPortbasePMN_MultipleMRNs_MultipleTransactions()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = testclient.CC_PK,
				AM_CC_RecipientInbox = portbaseClient.CC_PK,
				AM_SenderMessageXML = GetMessageXml("MessagePMN_MultipleMRNs"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("790BE798-2F09-4BA2-BC9A-458648472B6B")
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "C600799752",
					Reference2 = "16DE930346632679E1",
					Reference3 = "PORTBASE",
					Reference4 = null,
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PMN",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 1
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "C600799752",
					Reference2 = "26DE930346632679E1",
					Reference3 = "PORTBASE",
					Reference4 = null,
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PMN",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 1
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "C600799752",
					Reference2 = "36DE930346632679E1",
					Reference3 = "PORTBASE",
					Reference4 = null,
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PMN",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 1
				})
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestMessageFromPortbaseToCW1PSN_SingleMRN_SingleTransaction()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = portbaseClient.CC_PK,
				AM_CC_RecipientOutbox = testclient.CC_PK,
				AM_RecipientMessageXML = GetMessageXml("MessagePSN_SingleMRN"),
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
					Reference1 = null,
					Reference2 = "16DE810648434537E0",
					Reference3 = "PORTBASE",
					Reference4 = "STU(4)",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PSN",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 1
				})
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestMessageFromCW1ToPortbase_AlternativeReference1()
		{
			var messageReceivedTime = new DateTime(2018, 05, 21, 9, 31, 0);
			var messageArchivedTime = new DateTime(2018, 05, 21, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = portbaseClient.CC_PK,
				AM_CC_RecipientOutbox = testclient.CC_PK,
				AM_RecipientMessageXML = GetMessageXml("MessagePSN_Refence1Missing"),
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
					Reference1 = "4045736050",
					Reference2 = "18DE585382935075E6",
					Reference3 = "PORTBASE",
					Reference4 = "STU(4)",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PSN",
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
			portbaseClient = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.ServiceProvider, "PORTBASE"));
		}
		eHubClient testclient;
		eHubClient portbaseClient;
	}
}
