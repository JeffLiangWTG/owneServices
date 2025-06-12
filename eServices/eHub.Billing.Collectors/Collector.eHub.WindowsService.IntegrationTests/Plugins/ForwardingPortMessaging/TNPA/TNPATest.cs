using NUnit.Framework;
using System;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ForwardingPortMessaging.TNPA;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubArchiveOnline;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientFactory;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.Plugins.ForwardingPortMessaging.TNPA
{
	class TNPATest : PluginTest<Plugin>
	{
		[Test]
		public void TestPOZTNPASubmissionTypeWithdrawal()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = testClient.CC_PK,
				AM_CC_RecipientInbox = tnpaClient.CC_PK,
				AM_SenderMessageXML = GetMessageXml("MessageTNPAUShipmentWithdrawal"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("790be798-2f09-4ba2-bc9a-458648472b6b"),
				AM_DT_RecipientMessageType = MassageTypeCoprar.DT_PK
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "C00679358",
					Reference2 = "Cargo Dues - Export",
					Reference3 = "Withdrawal",
					Reference4 = "",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "POZ",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestPOZTNPASubmissionTypeAmendment()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = testClient.CC_PK,
				AM_CC_RecipientInbox = tnpaClient.CC_PK,
				AM_SenderMessageXML = GetMessageXml("MessageTNPAUShipmentAmendment"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("790be798-2f09-4ba2-bc9a-458648472b6b"),
				AM_DT_RecipientMessageType = MassageTypeCoprar.DT_PK
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "C00679358",
					Reference2 = "Cargo Dues - Export",
					Reference3 = "Amendment",
					Reference4 = "",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "POZ",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestPOZTNPASubmissionTypeOriginal()
		{
			var messageReceivedTime = new DateTime(2015, 5, 21, 23, 38, 0);
			var messageArchivedTime = new DateTime(2015, 5, 22, 3, 5, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = testClient.CC_PK,
				AM_CC_RecipientInbox = tnpaClient.CC_PK,
				AM_SenderMessageXML = GetMessageXml("MessageTNPAUShipmentOriginal"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("790be798-2f09-4ba2-bc9a-458648472b6b"),
				AM_DT_RecipientMessageType = MassageTypeCoprar.DT_PK
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "C00679358",
					Reference2 = "Cargo Dues - Export",
					Reference3 = "Original",
					Reference4 = "",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "POZ",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestPOZTNPAConsolOrDeclaration()
        {
			var messageReceivedTime = new DateTime(2015, 5, 21, 23, 38, 0);
			var messageArchivedTime = new DateTime(2015, 5, 22, 3, 5, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = testClient.CC_PK,
				AM_CC_RecipientInbox = tnpaClient.CC_PK,
				AM_SenderMessageXML = GetMessageXml("MessageTNPAUShipmentWithDeclaration"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("790be798-2f09-4ba2-bc9a-458648472b6b"),
				AM_DT_RecipientMessageType = MassageTypeCoprar.DT_PK
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "C00679000",
					Reference2 = "Cargo Dues - Export",
					Reference3 = "Original",
					Reference4 = "",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "POZ",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestPSZTNPAEventTypeMAAMessageTypeRoot()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = tnpaClient.CC_PK,
				AM_CC_RecipientOutbox = testClient.CC_PK,
				AM_RecipientMessageXML = GetMessageXml("MessageTNPAUniversalEventMAA"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("790be798-2f09-4ba2-bc9a-458648472b6b"),
				AM_DT_RecipientMessageType = MessageTypeRoot.DT_PK
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "C00679036",
					Reference2 = "Cargo Dues - Export",
					Reference3 = "MAA",
					Reference4 = "3705518950",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PSZ",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestPSZTNPAEventTypeMWAMessageTypeAPERAK()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = tnpaClient.CC_PK,
				AM_CC_RecipientOutbox = testClient.CC_PK,
				AM_RecipientMessageXML = GetMessageXml("MessageTNPAUniversalEventMWA"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("790be798-2f09-4ba2-bc9a-458648472b6b"),
				AM_DT_RecipientMessageType = MessageTypeAperak.DT_PK
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "C00679036",
					Reference2 = "Cargo Dues - Export",
					Reference3 = "MWA",
					Reference4 = "3705518950",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PSZ",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestPSZTNPAEventTypeSTUMessageTypeAPERAK()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = tnpaClient.CC_PK,
				AM_CC_RecipientOutbox = testClient.CC_PK,
				AM_RecipientMessageXML = GetMessageXml("MessageTNPAUniversalEventSTU"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("790be798-2f09-4ba2-bc9a-458648472b6b"),
				AM_DT_RecipientMessageType = MessageTypeAperak.DT_PK
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "C00679036",
					Reference2 = "Cargo Dues - Export",
					Reference3 = "STU",
					Reference4 = "3705518950",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PSZ",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestPSZTNPAConsolOrDeclaration()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = tnpaClient.CC_PK,
				AM_CC_RecipientOutbox = testClient.CC_PK,
				AM_RecipientMessageXML = GetMessageXml("MessageTNPAUniversalEventWithDeclaration"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("790be798-2f09-4ba2-bc9a-458648472b6b"),
				AM_DT_RecipientMessageType = MessageTypeAperak.DT_PK
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "C00679000",
					Reference2 = "Cargo Dues - Export",
					Reference3 = "STU",
					Reference4 = "3705518950",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "PMG",
					PriceItemCode = "PSZ",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			testClient = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "TSTCLIENT"));
			tnpaClient = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.ServiceProvider, "TNPA"));
			MassageTypeCoprar = AddMessageType(new eHubMessageType("http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006#EFACT_D95B_COPRAR"));
			MessageTypeRoot = AddMessageType(new eHubMessageType("http://schemas.microsoft.com/Edi/Edifact#Efact_Contrl_Root"));
			MessageTypeAperak = AddMessageType(new eHubMessageType("http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006#EFACT_D00B_APERAK"));
		}

		eHubMessageType MassageTypeCoprar;
		eHubMessageType MessageTypeRoot;
		eHubMessageType MessageTypeAperak;
		eHubClient testClient;
		eHubClient tnpaClient;
	}
}
