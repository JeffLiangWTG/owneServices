using System;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubArchiveOnline;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ACASMessaging;
using NUnit.Framework;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientFactory;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.Plugins.ACASMessaging
{
	class ACASTest : PluginTest<Plugin>
	{
		[Test]
		public void TestACASShipmentUS()
		{
			var messageReceivedTime = new DateTime(2019, 9, 16, 4, 10, 57);
			var messageArchivedTime = new DateTime(2019, 9, 16, 4, 50, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = senderInbox.CC_PK,
				AM_CC_RecipientInbox = recipientInbox.CC_PK,
				AM_CC_RecipientOutbox = recipientOutbox.CC_PK,
				AM_SenderMessageXML = GetMessageXml("ShipmentDeliveredToASAC_US"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("60304315-e6a9-43f3-9394-6d6048563a23"),
			};
			AddArchiveMessage(message);

			var messageToBeIgnored = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = senderInbox.CC_PK,
				AM_CC_RecipientInbox = recipientInbox.CC_PK,
				AM_CC_RecipientOutbox = senderInbox.CC_PK,
				AM_SenderMessageXML = GetMessageXml("MessageRejectedAndReturnedToSender"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("cc5e7129-7195-4c11-8a08-b42205d011c8"),
			};
			AddArchiveMessage(messageToBeIgnored);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "HVC000000000574546",
					Reference2 = "HVLV ACAS Shipment Report",
					Reference3 = "Original",
					Reference4 = "202400379006",
					Reference5 = "60304315-e6a9-43f3-9394-6d6048563a23",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "ACS",
					PriceItemCode = "CAU",
					MessageTrackingID = "60304315-e6a9-43f3-9394-6d6048563a23",
					Version = 1
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestACASShipmentBrazil()
		{
			var messageReceivedTime = new DateTime(2019, 9, 16, 4, 10, 57);
			var messageArchivedTime = new DateTime(2019, 9, 16, 4, 50, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = senderInbox.CC_PK,
				AM_CC_RecipientInbox = recipientInbox.CC_PK,
				AM_CC_RecipientOutbox = recipientOutbox.CC_PK,
				AM_SenderMessageXML = GetMessageXml("ShipmentDeliveredToASAC_BR"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("60304315-e6a9-43f3-9394-6d6048563a23"),
			};
			AddArchiveMessage(message);

			var messageToBeIgnored = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = senderInbox.CC_PK,
				AM_CC_RecipientInbox = recipientInbox.CC_PK,
				AM_CC_RecipientOutbox = senderInbox.CC_PK,
				AM_SenderMessageXML = GetMessageXml("MessageRejectedAndReturnedToSender"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("cc5e7129-7195-4c11-8a08-b42205d011c8"),
			};
			AddArchiveMessage(messageToBeIgnored);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "HVC000000000574546",
					Reference2 = "HVLV ACAS Shipment Report",
					Reference3 = "Amendment",
					Reference4 = "S54637772U",
					Reference5 = "60304315-e6a9-43f3-9394-6d6048563a23",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "ACS",
					PriceItemCode = "CAB",
					MessageTrackingID = "60304315-e6a9-43f3-9394-6d6048563a23",
					Version = 1
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestACASCargoBrazil()
		{
			var messageReceivedTime = new DateTime(2019, 9, 16, 4, 10, 57);
			var messageArchivedTime = new DateTime(2019, 9, 16, 4, 50, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = senderInbox.CC_PK,
				AM_CC_RecipientInbox = recipientInbox.CC_PK,
				AM_CC_RecipientOutbox = recipientOutbox.CC_PK,
				AM_SenderMessageXML = GetMessageXml("CargoDeliveredToASAC_BR"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("60304315-e6a9-43f3-9394-6d6048563a23"),
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "SBN1BNEA54638048",
					Reference2 = "Advanced Cargo Report",
					Reference3 = "Original",
					Reference4 = "081-78046780",
					Reference5 = "60304315-e6a9-43f3-9394-6d6048563a23",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "ACS",
					PriceItemCode = "CAB",
					MessageTrackingID = "60304315-e6a9-43f3-9394-6d6048563a23",
					Version = 1
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestACASCargoUS()
		{
			var messageReceivedTime = new DateTime(2019, 9, 16, 4, 10, 57);
			var messageArchivedTime = new DateTime(2019, 9, 16, 4, 50, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = senderInbox.CC_PK,
				AM_CC_RecipientInbox = recipientInbox.CC_PK,
				AM_CC_RecipientOutbox = recipientOutbox.CC_PK,
				AM_SenderMessageXML = GetMessageXml("CargoDeliveredToASAC_US"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("60304315-e6a9-43f3-9394-6d6048563a23"),
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "SBN1BNEA54638048",
					Reference2 = "Advanced Cargo Report",
					Reference3 = "Original",
					Reference4 = "081-78046780",
					Reference5 = "60304315-e6a9-43f3-9394-6d6048563a23",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "ACS",
					PriceItemCode = "CAU",
					MessageTrackingID = "60304315-e6a9-43f3-9394-6d6048563a23",
					Version = 1
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			senderInbox = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "TSTCLIENT"));
			recipientInbox = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.ServiceProvider, "ADVANCE_AIR_CARGO_REPORT"));
			recipientOutbox = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.ServiceProvider, "ACAS_US_FRI"));
		}
		eHubClient senderInbox;
		eHubClient recipientInbox;
		eHubClient recipientOutbox;
	}
}
