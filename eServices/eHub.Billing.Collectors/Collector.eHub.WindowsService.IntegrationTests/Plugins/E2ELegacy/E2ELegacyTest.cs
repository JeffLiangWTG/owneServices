using System;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubArchiveOnline;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientFactory;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.Plugins.E2ELegacy
{
	class E2ELegacyTest : PluginTest<WindowsService.Plugins.E2ELegacy.Plugin>
	{
		[Test]
		public void TestE2ELegacyShipmentConsolMessages()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var messageTrackingID1 = new Guid("9B4CBC43-DC04-40ED-9EA9-BC8509C479CF");
			var messageTrackingID2 = new Guid("C9481CF3-1061-42BF-BF5C-D3102C504AE2");

			var message1 = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_BillingElementCount = 1,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = enterprise1.CC_PK,
				AM_CC_RecipientOutbox = enterprise2.CC_PK,
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_SentToRecipientUTC = messageReceivedTime,
				AM_SenderMessageXML = GetMessageXml("SingleShipmentConsol"),
				AM_ApplicationCode = "XMS",
				AM_InboxMessageTrackingID = messageTrackingID1,
				AM_OutboxMessageTrackingID = new Guid("4B11D8D4-68C4-45B8-BD43-51D5CF7E7767"),
				AM_OutboxFileNameOverride = "Outbox file name",
				AM_InboxFileNameOverride = "Inbox file name"
			};
			AddArchiveMessage(message1);

			var message2 = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_BillingElementCount = 1,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = enterprise3.CC_PK,
				AM_CC_RecipientOutbox = enterprise4.CC_PK,
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_SentToRecipientUTC = messageReceivedTime,
				AM_SenderMessageXML = GetMessageXml("MultipleShipmentConsol"),
				AM_ApplicationCode = "XMS",
				AM_InboxMessageTrackingID = messageTrackingID2,
				AM_OutboxMessageTrackingID = new Guid("18E40F0E-EEBD-4D80-9846-602CB6ECFE1F"),
				AM_OutboxFileNameOverride = "Outbox file name",
				AM_InboxFileNameOverride = "Inbox file name"
			};
			AddArchiveMessage(message2);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "E2E",
					PriceItemCode = "E2E",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageReceivedTime,
					ClientID = "ENTERPRISE2",
					MessageTrackingID = messageTrackingID1.ToString(),
					Reference1 = "ENTERPRISE1",
					Reference2 = "ConsolC00015849"
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "E2E",
					PriceItemCode = "E2E",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageReceivedTime,
					ClientID = "ENTERPRISE2",
					MessageTrackingID = messageTrackingID1.ToString(),
					Reference1 = "ENTERPRISE1",
					Reference2 = "ShipmentS00017721"
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "E2E",
					PriceItemCode = "E2E",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageReceivedTime,
					ClientID = "ENTERPRISE4",
					MessageTrackingID = messageTrackingID2.ToString(),
					Reference1 = "ENTERPRISE3",
					Reference2 = "ConsolC00015849"
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "E2E",
					PriceItemCode = "E2E",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageReceivedTime,
					ClientID = "ENTERPRISE4",
					MessageTrackingID = messageTrackingID2.ToString(),
					Reference1 = "ENTERPRISE3",
					Reference2 = "ShipmentS00017721"
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "E2E",
					PriceItemCode = "E2E",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageReceivedTime,
					ClientID = "ENTERPRISE4",
					MessageTrackingID = messageTrackingID2.ToString(),
					Reference1 = "ENTERPRISE3",
					Reference2 = "ShipmentS00017722"
				})
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestE2ELegacyMessages()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var messageTrackingID1 = new Guid("CAB0D061-81DE-4B55-B0E1-06B68677D1EA");
			var messageTrackingID2 = new Guid("C1CE8415-55B7-4CD2-A78C-B93B13AED8AE");

			var message1 = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_BillingElementCount = 1,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = enterprise1.CC_PK,
				AM_CC_RecipientOutbox = enterprise2.CC_PK,
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_SentToRecipientUTC = messageReceivedTime,
				AM_SenderMessageXML = GetMessageXml("LegacyConsol"),
				AM_ApplicationCode = "XMS",
				AM_InboxMessageTrackingID = messageTrackingID1,
				AM_OutboxMessageTrackingID = new Guid("FFFEE118-D528-40F8-9A2E-9573AFF3D2F5"),
				AM_OutboxFileNameOverride = "Outbox file name",
				AM_InboxFileNameOverride = "Inbox file name"
			};
			AddArchiveMessage(message1);

			var message2 = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_BillingElementCount = 1,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = enterprise3.CC_PK,
				AM_CC_RecipientOutbox = enterprise4.CC_PK,
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_SentToRecipientUTC = messageReceivedTime,
				AM_SenderMessageXML = GetMessageXml("LegacyShipment"),
				AM_ApplicationCode = "XMS",
				AM_InboxMessageTrackingID = messageTrackingID2,
				AM_OutboxMessageTrackingID = new Guid("2BA79417-28CF-40AA-9613-F0A90E249A8A"),
				AM_OutboxFileNameOverride = "Outbox file name",
				AM_InboxFileNameOverride = "Inbox file name"
			};
			AddArchiveMessage(message2);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "E2E",
					PriceItemCode = "E2E",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageReceivedTime,
					ClientID = "ENTERPRISE2",
					MessageTrackingID = messageTrackingID1.ToString(),
					Reference1 = "ENTERPRISE1",
					Reference2 = "ConsolC00001000"
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "E2E",
					PriceItemCode = "E2E",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageReceivedTime,
					ClientID = "ENTERPRISE2",
					MessageTrackingID = messageTrackingID1.ToString(),
					Reference1 = "ENTERPRISE1",
					Reference2 = "ConsolC00001001"
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "E2E",
					PriceItemCode = "E2E",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageReceivedTime,
					ClientID = "ENTERPRISE4",
					MessageTrackingID = messageTrackingID2.ToString(),
					Reference1 = "ENTERPRISE3",
					Reference2 = "ShipmentS00001000"
				})
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestCartageJob()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var messageTrackingID1 = new Guid("CAB0D061-81DE-4B55-B0E1-06B68677D1EA");

			var message1 = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_BillingElementCount = 1,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = enterprise1.CC_PK,
				AM_CC_RecipientOutbox = enterprise2.CC_PK,
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_SentToRecipientUTC = messageReceivedTime,
				AM_SenderMessageXML = GetMessageXml("CartageJob"),
				AM_ApplicationCode = "XMS",
				AM_InboxMessageTrackingID = messageTrackingID1,
				AM_OutboxMessageTrackingID = new Guid("FFFEE118-D528-40F8-9A2E-9573AFF3D2F5"),
				AM_OutboxFileNameOverride = "Outbox file name",
				AM_InboxFileNameOverride = "Inbox file name"
			};
			AddArchiveMessage(message1);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "E2E",
					PriceItemCode = "E2E",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageReceivedTime,
					ClientID = "ENTERPRISE2",
					MessageTrackingID = messageTrackingID1.ToString(),
					Reference1 = "ENTERPRISE1",
					Reference2 = "CartageJobB00158769/I"
				})
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestDocumentMessage()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var messageTrackingID1 = new Guid("CAB0D061-81DE-4B55-B0E1-06B68677D1EA");

			var message1 = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_BillingElementCount = 1,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = enterprise1.CC_PK,
				AM_CC_RecipientOutbox = enterprise2.CC_PK,
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_SentToRecipientUTC = messageReceivedTime,
				AM_SenderMessageXML = GetMessageXml("DocumentMessage"),
				AM_ApplicationCode = "XMS",
				AM_InboxMessageTrackingID = messageTrackingID1,
				AM_OutboxMessageTrackingID = new Guid("FFFEE118-D528-40F8-9A2E-9573AFF3D2F5"),
				AM_OutboxFileNameOverride = "Outbox file name",
				AM_InboxFileNameOverride = "Inbox file name"
			};
			AddArchiveMessage(message1);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "E2E",
					PriceItemCode = "E2E",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageReceivedTime,
					ClientID = "ENTERPRISE2",
					MessageTrackingID = messageTrackingID1.ToString(),
					Reference1 = "ENTERPRISE1",
					Reference2 = "DocumentMessageT00033387"
				})
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestOrder()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var messageTrackingID1 = new Guid("CAB0D061-81DE-4B55-B0E1-06B68677D1EA");

			var message1 = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_BillingElementCount = 1,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = enterprise1.CC_PK,
				AM_CC_RecipientOutbox = enterprise2.CC_PK,
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_SentToRecipientUTC = messageReceivedTime,
				AM_SenderMessageXML = GetMessageXml("Order"),
				AM_ApplicationCode = "XMS",
				AM_InboxMessageTrackingID = messageTrackingID1,
				AM_OutboxMessageTrackingID = new Guid("FFFEE118-D528-40F8-9A2E-9573AFF3D2F5"),
				AM_OutboxFileNameOverride = "Outbox file name",
				AM_InboxFileNameOverride = "Inbox file name"
			};
			AddArchiveMessage(message1);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "E2E",
					PriceItemCode = "E2E",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageReceivedTime,
					ClientID = "ENTERPRISE2",
					MessageTrackingID = messageTrackingID1.ToString(),
					Reference1 = "ENTERPRISE1",
					Reference2 = "Order7830630053211~69~MKCORP"
				})
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestNettingClearingJournals()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var messageTrackingID1 = new Guid("CAB0D061-81DE-4B55-B0E1-06B68677D1EA");

			var message1 = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_BillingElementCount = 1,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = enterprise1.CC_PK,
				AM_CC_RecipientOutbox = enterprise2.CC_PK,
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_SentToRecipientUTC = messageReceivedTime,
				AM_SenderMessageXML = GetMessageXml("NettingClearingJournals"),
				AM_ApplicationCode = "XMS",
				AM_InboxMessageTrackingID = messageTrackingID1,
				AM_OutboxMessageTrackingID = new Guid("FFFEE118-D528-40F8-9A2E-9573AFF3D2F5"),
				AM_OutboxFileNameOverride = "Outbox file name",
				AM_InboxFileNameOverride = "Inbox file name"
			};
			AddArchiveMessage(message1);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "E2E",
					PriceItemCode = "E2E",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageReceivedTime,
					ClientID = "ENTERPRISE2",
					MessageTrackingID = messageTrackingID1.ToString(),
					Reference1 = "ENTERPRISE1",
					Reference2 = "TxnHeaderAR~SENGLOHAM~2020-03-25~671796.30"
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "E2E",
					PriceItemCode = "E2E",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageReceivedTime,
					ClientID = "ENTERPRISE2",
					MessageTrackingID = messageTrackingID1.ToString(),
					Reference1 = "ENTERPRISE1",
					Reference2 = "TxnHeaderAP~SENINTBJS~2020-03-26~-3638.00"
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "E2E",
					PriceItemCode = "E2E",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageReceivedTime,
					ClientID = "ENTERPRISE2",
					MessageTrackingID = messageTrackingID1.ToString(),
					Reference1 = "ENTERPRISE1",
					Reference2 = "TxnHeaderAR~SENINTORD~2020-03-27~48.21"
				})
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			enterprise1 = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "ENTERPRISE1"));
			enterprise2 = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "ENTERPRISE2"));
			enterprise3 = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "ENTERPRISE3"));
			enterprise4 = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "ENTERPRISE4"));
		}

		eHubClient enterprise1;
		eHubClient enterprise2;
		eHubClient enterprise3;
		eHubClient enterprise4;
	}
}
