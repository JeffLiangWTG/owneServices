using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubArchiveOnline;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientFactory;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.Plugins.E2E
{
	class E2ETest : PluginTest<WindowsService.Plugins.E2E.Plugin>
	{
		[Test]
		public void TestE2EMessage()
		{
			var messageSentTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);

			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ApplicationCode = "UDM",
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = sender.CC_PK,
				AM_CC_RecipientOutbox = recipient.CC_PK,
				AM_SenderMessageXML = GetMessageXml("MessageE2E"),
				AM_SentToRecipientUTC = messageSentTime,
				AM_InboxMessageTrackingID = new Guid("{790BE798-2F09-4BA2-BC9A-458648472B6B}"),
				AM_ReceivedFromSenderUTC = messageSentTime,
			};

			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "E2E",
					PriceItemCode = "E2E",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageSentTime,
					ClientID = "TSTENTRCV",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference1 = "TSTENTSNT",
					Reference2 = "TransportBookingConsolidationCM00747875",
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "E2E",
					PriceItemCode = "E2E",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageSentTime,
					ClientID = "TSTENTRCV",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference1 = "TSTENTSNT",
					Reference2 = "TransportBookingConsolidationCM00747875",
					Reference3 = "ForwardingConsolC02386421",
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "E2E",
					PriceItemCode = "E2E",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageSentTime,
					ClientID = "TSTENTRCV",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference1 = "TSTENTSNT",
					Reference2 = "TransportBookingConsolidationCM00747875",
					Reference3 = "ForwardingConsolC02386421",
					Reference4 = "ForwardingShipmentSTMF0014973",
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "E2E",
					PriceItemCode = "E2E",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageSentTime,
					ClientID = "TSTENTRCV",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference1 = "TSTENTSNT",
					Reference2 = "TransportBookingConsolidationCM00747875",
					Reference3 = "TransportBookingTB00866160",
				}),
			};

			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)),
				Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestE2EMessageFailed()
		{
			var messageSentTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);

			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ApplicationCode = "UDM",
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = senderRecipientFailed.CC_PK,
				AM_CC_RecipientOutbox = senderRecipientFailed.CC_PK,
				AM_SenderMessageXML = GetMessageXml("MessageE2EFailed"),
				AM_SentToRecipientUTC = messageSentTime,
				AM_InboxMessageTrackingID = new Guid("{790BE798-2F09-4BA2-BC9A-458648472B6B}"),
				AM_ReceivedFromSenderUTC = messageSentTime,
			};

			AddArchiveMessage(message);

			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.Empty);
		}

		[Test]
		public void TestE2EUniversalTransactionMessage()
		{
			var messageSentTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);

			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ApplicationCode = "UDM",
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = sender.CC_PK,
				AM_CC_RecipientOutbox = recipient.CC_PK,
				AM_SenderMessageXML = GetMessageXml("UniversalTransactionE2E"),
				AM_SentToRecipientUTC = messageSentTime,
				AM_InboxMessageTrackingID = new Guid("{89F31D7C-6C45-49A7-A571-850DEE88EE9C}"),
				AM_ReceivedFromSenderUTC = messageSentTime,
			};

			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "E2E",
					PriceItemCode = "E2T",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageSentTime,
					ClientID = "TSTENTRCV",
					MessageTrackingID = "89f31d7c-6c45-49a7-a571-850dee88ee9c",
					Reference1 = "TSTENTSNT",
					Reference2 = "AccountingInvoiceACT0000001",
				})
			};

			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)),
				Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestE2E_ManyLevel3SubShipments()
		{
			var messageSentTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ApplicationCode = "UDM",
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = sender.CC_PK,
				AM_CC_RecipientOutbox = recipient.CC_PK,
				AM_SenderMessageXML = GetMessageXml("MessageE2E_ManyLevel3SubShipments"),
				AM_SentToRecipientUTC = messageSentTime,
				AM_InboxMessageTrackingID = new Guid("{790BE798-2F09-4BA2-BC9A-458648472B6B}"),
				AM_ReceivedFromSenderUTC = messageSentTime,
			};

			AddArchiveMessage(message);

			var transactions = RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)).ToArray();
			var expectedTransactions = new List<TimeStampedTransaction>();

			for (int i = 1; i <= 102; i++)
			{
				var transaction = new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "E2E",
					PriceItemCode = "E2E",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageSentTime,
					ClientID = "TSTENTRCV",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference1 = "TSTENTSNT",
					Reference2 = "ForwardingConsolCCCCCCC1",
				});

				if (i >= 2)
				{
					transaction.BillingTransaction.Reference3 = "ForwardingShipmentCCCCCCS1";
				}

				if (i >= 3)
				{
					transaction.BillingTransaction.Reference4 = $"ForwardingShipmentSSSSSSS{(i - 2)}";
				}
				expectedTransactions.Add(transaction);
			}

			Assert.That(transactions, Is.EquivalentTo(expectedTransactions));
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			sender = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "TSTENTSNT"));
			recipient = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "TSTENTRCV"));
			senderRecipientFailed =
				AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "HYETSTTST"));
		}

		eHubClient sender;
		eHubClient recipient;
		eHubClient senderRecipientFailed;
	}
}
