using System;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubArchiveOnline;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientFactory;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.OceanCarrierMessaging.ShippingInstructionSent;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.Plugins.OceanCarrierMessaging.ShippingInstructionSent
{
	class ShippingInstructionSentTest : PluginTest<Plugin>
	{

		[Test]
		public void TestDocumentNameShippingInstruction()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var messageToShippingInstruction = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_ApplicationCode = "UDM",
				AM_CC_SenderInbox = client.CC_PK,
				AM_CC_RecipientInbox = shippingInstruction.CC_PK,
				AM_CC_RecipientOutbox = provider.CC_PK,
				AM_SenderMessageXML = GetMessageXml("ShippingInstruction"),
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
					ClientID = "TSTCLIENT",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference2 = "INTTRA",
					Reference3 = "C00839734",
					Reference4 = "MAEU",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "SHI",
					PriceItemCode = "SHI",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 3,
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestDocumentNameBookingRequest()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var messageToShippingInstruction = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_ApplicationCode = "UDM",
				AM_CC_SenderInbox = client.CC_PK,
				AM_CC_RecipientInbox = shippingInstruction.CC_PK,
				AM_CC_RecipientOutbox = provider.CC_PK,
				AM_SenderMessageXML = GetMessageXml("BookingRequest"),
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
					ClientID = "TSTCLIENT",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference2 = "INTTRA",
					Reference3 = "C03116235",
					Reference4 = "CMDU",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "SHI",
					PriceItemCode = "BRT",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 3,
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestDocumentNameVerifiedGrossContainerWeight()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var messageToShippingInstruction = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_ApplicationCode = "UDM",
				AM_CC_SenderInbox = client.CC_PK,
				AM_CC_RecipientInbox = shippingInstruction.CC_PK,
				AM_CC_RecipientOutbox = provider.CC_PK,
				AM_SenderMessageXML = GetMessageXml("VerifiedGrossContainerWeight"),
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
					ClientID = "TSTCLIENT",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference2 = "INTTRA",
					Reference3 = "C03083298",
					Reference4 = "MSCU",
					Reference5 = "CRXU9949608",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "SHI",
					PriceItemCode = "VGM",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 3,
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference2 = "INTTRA",
					Reference3 = "C03083298",
					Reference4 = "MSCU",
					Reference5 = "CRXU9949609",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "SHI",
					PriceItemCode = "VGM",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 3,
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestDocumentNameVerifiedShippingOrder()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var messageToShippingInstruction = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_ApplicationCode = "UDM",
				AM_CC_SenderInbox = client.CC_PK,
				AM_CC_RecipientInbox = shippingInstruction.CC_PK,
				AM_CC_RecipientOutbox = provider.CC_PK,
				AM_SenderMessageXML = GetMessageXml("ShippingOrder"),
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
					ClientID = "TSTCLIENT",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference2 = "INTTRA",
					Reference3 = "C03083298",
					Reference4 = "MSCU",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "SHI",
					PriceItemCode = "SHO",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 3,
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestDocumentNameVerifiedShippingOrderShipmentType()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var messageToShippingInstruction = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_ApplicationCode = "UDM",
				AM_CC_SenderInbox = client.CC_PK,
				AM_CC_RecipientInbox = shippingInstruction.CC_PK,
				AM_CC_RecipientOutbox = provider.CC_PK,
				AM_SenderMessageXML = GetMessageXml("ShippingOrderShipmentType"),
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
					ClientID = "TSTCLIENT",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference2 = "INTTRA",
					Reference3 = "C03083298",
					Reference4 = "WERMSCU",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "SHI",
					PriceItemCode = "SHO",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 3,
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		public void TestDocumentNameEManifestReceivedValidMessageFromServiceProvider()
		{
			var messageReceivedTime = new DateTime(2018, 12, 01, 9, 31, 0);
			var messageArchivedTime = new DateTime(2018, 12, 01, 9, 35, 0);
			var messageToShippingInstruction = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_ApplicationCode = "UDM",
				AM_CC_SenderInbox = client.CC_PK,
				AM_CC_RecipientInbox = shippingInstruction.CC_PK,
				AM_CC_RecipientOutbox = provider.CC_PK,
				AM_SenderMessageXML = GetMessageXml("eManifest"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("{29313aa1-5466-472d-b0e3-34422026f88c}"),
			};
			AddArchiveMessage(messageToShippingInstruction);
			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Version = 3,
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "29313aa1-5466-472d-b0e3-34422026f88c",
					Reference2 = "INTTRA",
					Reference3 = "CON00001",
					Reference4 = "ABCD",
					Reference5 = "SS1000001",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "SHI",
					PriceItemCode = "EMN",
					MessageTrackingID = "29313aa1-5466-472d-b0e3-34422026f88c",
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestDocumentNameEManifestReceivedValidMessage_MultiSubShipment()
		{
			var messageReceivedTime = new DateTime(2018, 12, 01, 9, 31, 0);
			var messageArchivedTime = new DateTime(2018, 12, 01, 9, 35, 0);
			var messageToShippingInstruction = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_ApplicationCode = "UDM",
				AM_CC_SenderInbox = client.CC_PK,
				AM_CC_RecipientInbox = shippingInstruction.CC_PK,
				AM_CC_RecipientOutbox = provider.CC_PK,
				AM_SenderMessageXML = GetMessageXml("eManifestMulti"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("{09111902-866c-41a1-87d6-8d73f8c4368a}"),
			};
			AddArchiveMessage(messageToShippingInstruction);
			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Version = 3,
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "09111902-866c-41a1-87d6-8d73f8c4368a",
					Reference2 = "INTTRA",
					Reference3 = "CON00002",
					Reference4 = "ABCD",
					Reference5 = "SS2000001",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "SHI",
					PriceItemCode = "EMN",
					MessageTrackingID = "09111902-866c-41a1-87d6-8d73f8c4368a",
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Version = 3,
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "09111902-866c-41a1-87d6-8d73f8c4368a",
					Reference2 = "INTTRA",
					Reference3 = "CON00002",
					Reference4 = "ABCD",
					Reference5 = "SS2000002",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "SHI",
					PriceItemCode = "EMN",
					MessageTrackingID = "09111902-866c-41a1-87d6-8d73f8c4368a",
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestDocumentNameEManifestReceivedValidMessage_CarrierCode()
		{
			var messageReceivedTime = new DateTime(2018, 12, 01, 9, 31, 0);
			var messageArchivedTime = new DateTime(2018, 12, 01, 9, 35, 0);
			var messageToShippingInstruction = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_ApplicationCode = "UDM",
				AM_CC_SenderInbox = client.CC_PK,
				AM_CC_RecipientInbox = shippingInstruction.CC_PK,
				AM_CC_RecipientOutbox = provider.CC_PK,
				AM_SenderMessageXML = GetMessageXml("eManifestCarrierCode"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("{4cf964ce-f6d2-4040-9728-69ddd7c05726}"),
			};
			AddArchiveMessage(messageToShippingInstruction);
			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Version = 3,
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "4cf964ce-f6d2-4040-9728-69ddd7c05726",
					Reference2 = "INTTRA",
					Reference3 = "CON00003",
					Reference4 = "TEST",
					Reference5 = "SS3000001",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "SHI",
					PriceItemCode = "EMN",
					MessageTrackingID = "4cf964ce-f6d2-4040-9728-69ddd7c05726",
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestDocumentNameBookingRequestV2()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var messageToShippingInstruction = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_ApplicationCode = "UDM",
				AM_CC_SenderInbox = client.CC_PK,
				AM_CC_RecipientInbox = shippingInstruction.CC_PK,
				AM_CC_RecipientOutbox = provider.CC_PK,
				AM_SenderMessageXML = GetMessageXml("BookingRequestV2"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("{17E0EEA6-B35F-44E0-B68A-87C167146E75}"),
			};
			AddArchiveMessage(messageToShippingInstruction);
			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "17e0eea6-b35f-44e0-b68a-87c167146e75",
					Reference2 = "INTTRA",
					Reference3 = "C102933325",
					Reference4 = "NAQA",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "SHI",
					PriceItemCode = "BRT",
					MessageTrackingID = "17e0eea6-b35f-44e0-b68a-87c167146e75",
					Version = 3,
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestDocumentNameBookingRequestColoadWithC1C()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var messageToShippingInstruction = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_ApplicationCode = "UDM",
				AM_CC_SenderInbox = client.CC_PK,
				AM_CC_RecipientInbox = shippingInstruction.CC_PK,
				AM_CC_RecipientOutbox = provider.CC_PK,
				AM_SenderMessageXML = GetMessageXml("BookingRequestColoadWithC1C"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("{17E0EEA6-B35F-44E0-B68A-87C167146E75}"),
			};
			AddArchiveMessage(messageToShippingInstruction);
			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "17e0eea6-b35f-44e0-b68a-87c167146e75",
					Reference2 = "INTTRA",
					Reference3 = "C102933325",
					Reference4 = "C17L",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "SHI",
					PriceItemCode = "BRT",
					MessageTrackingID = "17e0eea6-b35f-44e0-b68a-87c167146e75",
					Version = 3,
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestDocumentNameBookingRequestShippingLineAddressC1C()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var messageToShippingInstruction = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_ApplicationCode = "UDM",
				AM_CC_SenderInbox = client.CC_PK,
				AM_CC_RecipientInbox = shippingInstruction.CC_PK,
				AM_CC_RecipientOutbox = provider.CC_PK,
				AM_SenderMessageXML = GetMessageXml("BookingRequestShippingLineAddressC1C"),
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
					ClientID = "TSTCLIENT",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference2 = "INTTRA",
					Reference3 = "C03116235",
					Reference4 = "C17L",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "SHI",
					PriceItemCode = "BRT",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 3,
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			client = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "TSTCLIENT"));
			provider = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.ServiceProvider, "INTTRA_SI"));
			shippingInstruction = eHubClientFactory.CreateeHubClient(eHubClientType.ServiceProvider, "SHIPPING_INSTRUCTION");
			shippingInstruction.CC_PK = new Guid("a8f34356-f459-4805-8026-9cb072b7e0a3");
			AddClient(shippingInstruction);

		}
		eHubClient client;
		eHubClient provider;
		eHubClient shippingInstruction;

	}
}
