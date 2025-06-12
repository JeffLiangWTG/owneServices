using System;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubArchiveOnline;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.OceanCarrierMessaging.CSRSent;
using NUnit.Framework;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientFactory;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.Plugins.OceanCarrierMessaging.CSRSent
{
    class CSRSentTest : PluginTest<Plugin>
    {
        [Test]
        public void TestReceiveValidMessage()
        {
	        var RT_PK = Guid.NewGuid();
	        var eHubRegistrationType = new eHubRegistrationType
	        {
		        RT_PK = RT_PK,
		        RT_ID = "CARGOWISE",
		        RT_Description = "CargoWise Destination Party ID",
		        RT_RegistrantType = "Client"
	        };
	        AddRegistrationType(eHubRegistrationType);

	        var eHubClientRegistration = new eHubClientRegistration
	        {
		        CX_PK = Guid.NewGuid(),
		        CX_RT = RT_PK,
		        CX_CC = sender.CC_PK,
		        CX_Qualifier = "C1ST",
		        CX_Code = "",
		        CX_Attr1 = null,
		        CX_Password1 = null,
		        CX_Flag1 = 0,
		        CX_Flag2 = null,
		        CX_ConfigXml = "",
		        CX_IssuedUTC = null,
		        CX_ExpiryUTC = null
	        };
	        AddClientRegistration(eHubClientRegistration);
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var messageToShippingInstruction = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_ApplicationCode = "UDM",
				AM_CC_SenderInbox = sender.CC_PK,
				AM_CC_RecipientOutbox = client.CC_PK,
				AM_CC_RecipientInbox = shippingInstruction.CC_PK,
				AM_SenderMessageXML = GetMessageXml("SentMessageValid"),
				AM_RecipientMessageXML = GetMessageXml("ReceiveUniversalShipment"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("{790BE798-2F09-4BA2-BC9A-458648472B6B}"),
			};
			AddArchiveMessage(messageToShippingInstruction);
			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "1STOPTEST",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference2 = "ORG",
					Reference3 = "SSYD54624547",
					Reference4 = "TestCoLoadMasterBillNumber",
					Reference5 = "Booking Confirmation",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "SHI",
					PriceItemCode = "CSR",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

        [Test]
		public void TestReceiveValidMessageElectronicBillOfLoading()
		{
			var RT_PK = Guid.NewGuid();
			var eHubRegistrationType = new eHubRegistrationType
			{
				RT_PK = RT_PK,
				RT_ID = "CARGOWISE",
				RT_Description = "CargoWise Destination Party ID",
				RT_RegistrantType = "Client"
			};
			AddRegistrationType(eHubRegistrationType);

			var eHubClientRegistration = new eHubClientRegistration
			{
				CX_PK = Guid.NewGuid(),
				CX_RT = RT_PK,
				CX_CC = sender.CC_PK,
				CX_Qualifier = "C1ST",
				CX_Code = "",
				CX_Attr1 = null,
				CX_Password1 = null,
				CX_Flag1 = 0,
				CX_Flag2 = null,
				CX_ConfigXml = "",
				CX_IssuedUTC = null,
				CX_ExpiryUTC = null
			};
			AddClientRegistration(eHubClientRegistration);
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var messageToShippingInstruction = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_ApplicationCode = "UDM",
				AM_CC_SenderInbox = sender.CC_PK,
				AM_CC_RecipientOutbox = client.CC_PK,
				AM_CC_RecipientInbox = shippingInstruction.CC_PK,
				AM_SenderMessageXML = GetMessageXml("SentMessageValidElectronicBillOfLoading"),
				AM_RecipientMessageXML = GetMessageXml("ReceiveUniversalShipment"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("{790BE798-2F09-4BA2-BC9A-458648472B6B}"),
			};
			AddArchiveMessage(messageToShippingInstruction);
			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "1STOPTEST",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference2 = "ORG",
					Reference3 = "SSYD54624547",
					Reference4 = "TestCoLoadMasterBillNumber",
					Reference5 = "Electronic Bill of Lading",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "SHI",
					PriceItemCode = "CSR",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestReceiveValidMessageWithUniversalEvent()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var messageToShippingInstruction = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_ApplicationCode = "UDM",
				AM_CC_SenderInbox = sender.CC_PK,
				AM_CC_RecipientOutbox = client.CC_PK,
				AM_CC_RecipientInbox = shippingInstruction.CC_PK,
				AM_SenderMessageXML = GetMessageXml("SentMessageValid"),
				AM_RecipientMessageXML = GetMessageXml("ReceiveUniversalEvent"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("{790BE798-2F09-4BA2-BC9A-458648472B6B}"),
			};
			AddArchiveMessage(messageToShippingInstruction);
			TimeStampedTransaction[] expectedTransactions = new TimeStampedTransaction[0];
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestReceiveValidMessageWithUniversalShipment()
		{
			var RT_PK = Guid.NewGuid();
			var eHubRegistrationType = new eHubRegistrationType
			{
				RT_PK = RT_PK,
				RT_ID = "CARGOWISE",
				RT_Description = "CargoWise Destination Party ID",
				RT_RegistrantType = "Client"
			};
			AddRegistrationType(eHubRegistrationType);

			var eHubClientRegistration = new eHubClientRegistration
			{
				CX_PK = Guid.NewGuid(),
				CX_RT = RT_PK,
				CX_CC = sender.CC_PK,
				CX_Qualifier = "C1ST",
				CX_Code = "",
				CX_Attr1 = null,
				CX_Password1 = null,
				CX_Flag1 = 0,
				CX_Flag2 = null,
				CX_ConfigXml = "",
				CX_IssuedUTC = null,
				CX_ExpiryUTC = null
			};
			AddClientRegistration(eHubClientRegistration);
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var messageToShippingInstruction = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_ApplicationCode = "UDM",
				AM_CC_SenderInbox = sender.CC_PK,
				AM_CC_RecipientOutbox = client.CC_PK,
				AM_CC_RecipientInbox = shippingInstruction.CC_PK,
				AM_SenderMessageXML = GetMessageXml("SentMessageValid"),
				AM_RecipientMessageXML = GetMessageXml("ReceiveUniversalShipment"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("{790BE798-2F09-4BA2-BC9A-458648472B6B}"),
			};
			AddArchiveMessage(messageToShippingInstruction);
			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "1STOPTEST",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference2 = "ORG",
					Reference3 = "SSYD54624547",
					Reference4 = "TestCoLoadMasterBillNumber",
					Reference5 = "Booking Confirmation",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "SHI",
					PriceItemCode = "CSR",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestReceiveValidMessageWithEmptyCoLoadData()
		{
			var RT_PK = Guid.NewGuid();
			var eHubRegistrationType = new eHubRegistrationType
			{
				RT_PK = RT_PK,
				RT_ID = "CARGOWISE",
				RT_Description = "CargoWise Destination Party ID",
				RT_RegistrantType = "Client"
			};
			AddRegistrationType(eHubRegistrationType);

			var eHubClientRegistration = new eHubClientRegistration
			{
				CX_PK = Guid.NewGuid(),
				CX_RT = RT_PK,
				CX_CC = sender.CC_PK,
				CX_Qualifier = "C1ST",
				CX_Code = "",
				CX_Attr1 = null,
				CX_Password1 = null,
				CX_Flag1 = 0,
				CX_Flag2 = null,
				CX_ConfigXml = "",
				CX_IssuedUTC = null,
				CX_ExpiryUTC = null
			};
			AddClientRegistration(eHubClientRegistration);
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var messageToShippingInstruction = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_ApplicationCode = "UDM",
				AM_CC_SenderInbox = sender.CC_PK,
				AM_CC_RecipientOutbox = client.CC_PK,
				AM_CC_RecipientInbox = shippingInstruction.CC_PK,
				AM_SenderMessageXML = GetMessageXml("SentMessageValidWithEmptyCoLoadData"),
				AM_RecipientMessageXML = GetMessageXml("ReceiveUniversalShipment"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("{790BE798-2F09-4BA2-BC9A-458648472B6B}"),
			};
			AddArchiveMessage(messageToShippingInstruction);
			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "1STOPTEST",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference2 = "ORG",
					Reference3 = "SSYD54624547",
					Reference4 = "TestWayBillNumber",
					Reference5 = "Booking Confirmation",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "SHI",
					PriceItemCode = "CSR",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			client = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "TSTCLIENT"));
			sender = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "1STOPTEST"));
			shippingInstruction = eHubClientFactory.CreateeHubClient(eHubClientType.ServiceProvider, "SHIPPING_INSTRUCTION");
			shippingInstruction.CC_PK = new Guid("a8f34356-f459-4805-8026-9cb072b7e0a3");
			AddClient(shippingInstruction);
			AddServiceProvider(new eHubServiceProvider(shippingInstruction.CC_PK, client.CC_PK));
		}
		eHubClient client;
		eHubClient sender;
		eHubClient shippingInstruction;
	}
}
