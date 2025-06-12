using System;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubArchiveOnline;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ClientMappings;
using NUnit.Framework;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientFactory;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.Plugins.ClientMappings
{
	class ClientMappingsTest : PluginTest<Plugin>
	{
		[Test]
		public void TestMessageFromThirdPartyToEnterprise()
		{
			var transformationSet = new eHubTransformationSet
			{
				TS_CC_Sender = thirdParty.CC_PK,
				TS_Name = "This is a name.",
				TS_CC_Recipient = enterprise.CC_PK,
                TS_BillingXPathSource = @"//*[local-name()=""DataContext""]/*[local-name()=""DataSourceCollection"" or local-name()=""DataTargetCollection""][contains(., ""ForwardingShipment"") and not(contains(., ""ForwardingConsol""))]",
				TS_BillingElement = "Per Message",
				TS_BillSender = false,
				TS_BillRecipient = true,
			};
			AddTransformationSet(transformationSet);

			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = thirdParty.CC_PK,
				AM_CC_RecipientOutbox = enterprise.CC_PK,
                AM_SenderMessageXML = GetMessageXml("MessageFromThirdPartyToEnterprise"),
                AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_OutboxMessageTrackingID = new Guid("790be798-2f09-4ba2-bc9a-458648472b6b"),
				AM_TS = transformationSet.TS_PK,
				AM_OutboxFileNameOverride = "Outbox file name",
				AM_InboxFileNameOverride = "Inbox file name"
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "ENTERPRISE",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference2 = "Outbox file name",
					Reference3 = "Per Message",
					Reference4 = "This is a name.",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "CMP",
					PriceItemCode = "CMP",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				})
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestMessageFromInsecureThirdPartyToEnterprise()
		{
			var transformationSet = new eHubTransformationSet
			{
				TS_CC_Sender = thirdParty.CC_PK,
				TS_Name = "This is an insecure interface name.",
				TS_CC_Recipient = enterprise.CC_PK,
				TS_BillingXPathSource = @"//*[local-name()=""DataContext""]/*[local-name()=""DataSourceCollection"" or local-name()=""DataTargetCollection""][contains(., ""ForwardingShipment"") and not(contains(., ""ForwardingConsol""))]",
				TS_BillingElement = "Per Message",
				TS_BillSender = false,
				TS_BillRecipient = true,
			};
			AddTransformationSet(transformationSet);

			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = thirdParty.CC_PK,
				AM_CC_RecipientOutbox = enterprise.CC_PK,
				AM_SenderMessageXML = GetMessageXml("MessageFromThirdPartyToEnterprise"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_OutboxMessageTrackingID = new Guid("790be798-2f09-4ba2-bc9a-458648472b6b"),
				AM_TS = transformationSet.TS_PK,
				AM_OutboxFileNameOverride = "Outbox file name",
				AM_InboxFileNameOverride = "Inbox file name"
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "ENTERPRISE",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference2 = "Outbox file name",
					Reference3 = "Per Message",
					Reference4 = "This is an insecure interface name.",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "CMP",
					PriceItemCode = "CMP",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "ENTERPRISE",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference2 = "Outbox file name",
					Reference3 = "Per Message",
					Reference4 = "This is an insecure interface name.",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "CMP",
					PriceItemCode = "CMF",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				})
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestE2EMessage()
		{
			var transformationSet = new eHubTransformationSet
			{
				TS_CC_Sender = enterprise.CC_PK,
				TS_Name = "This is a name.",
				TS_CC_Recipient = enterprise2.CC_PK,
                TS_BillingXPathTarget = @"/*[local-name()=""UniversalInterchange""]/*[local-name()=""Body""]/*[local-name()=""UniversalShipment""]",
                TS_BillingElement = "Shipment",
				TS_BillSender = false,
				TS_BillRecipient = true,
			};

			AddTransformationSet(transformationSet);

			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_BillingElementCount = 1,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = enterprise.CC_PK,
				AM_CC_RecipientOutbox = enterprise2.CC_PK,
                AM_RecipientMessageXML = GetMessageXml("E2EMessage"),
                AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_OutboxMessageTrackingID = new Guid("790be798-2f09-4ba2-bc9a-458648472b6b"),
				AM_TS = transformationSet.TS_PK,
				AM_OutboxFileNameOverride = "Outbox file name",
				AM_InboxFileNameOverride = "Inbox file name"
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
						BillableCount = 1,
						ReportingSource = "HUB",
						ClientID = "ENTERPRISE2",
						Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
						Reference2 = "Outbox file name",
						Reference3 = "Shipment",
						Reference4 = "This is a name.",
						ServiceOccuredUTC = messageReceivedTime,
						Category = "CMP",
						PriceItemCode = "CMP",
						MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				})
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestRecipientOutboxIsNull()
		{
			var transformationSet = new eHubTransformationSet
			{
				TS_CC_Sender = enterprise.CC_PK,
				TS_Name = "This is a name.",
				TS_CC_Recipient = thirdParty.CC_PK,
				TS_BillingElement = "Shipment",
				TS_BillSender = true,
				TS_BillRecipient = false,
			};
			AddTransformationSet(transformationSet);

			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_BillingElementCount = 1,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = enterprise.CC_PK,
				AM_CC_RecipientInbox = thirdParty.CC_PK,
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("A9881188-7E43-4D57-9F25-F203535D2D8E"),
				AM_OutboxMessageTrackingID = new Guid("790be798-2f09-4ba2-bc9a-458648472b6b"),
				AM_TS = transformationSet.TS_PK,
				AM_OutboxFileNameOverride = "Outbox file name",
				AM_InboxFileNameOverride = "Inbox file name"
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "ENTERPRISE",
					Reference1 = "a9881188-7e43-4d57-9f25-f203535d2d8e",
					Reference2 = "Inbox file name",
					Reference3 = "Shipment",
					Reference4 = "This is a name.",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "CMP",
					PriceItemCode = "CMP",
					MessageTrackingID = "a9881188-7e43-4d57-9f25-f203535d2d8e",
				})
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestThirdPartyToThirdParty()
		{
			var transformationSet = new eHubTransformationSet
			{
				TS_CC_Sender = thirdParty.CC_PK,
				TS_Name = "This is a name.",
				TS_CC_Recipient = thirdParty2.CC_PK,
				TS_BillingElement = "Agency Bill of Lading",
				TS_BillSender = false,
				TS_BillRecipient = false,
				TS_CC_BillOther = enterprise.CC_PK,
			};

			AddTransformationSet(transformationSet);

			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_BillingElementCount = 1,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = thirdParty.CC_PK,
				AM_CC_RecipientOutbox = thirdParty2.CC_PK,
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("A9881188-7E43-4D57-9F25-F203535D2D8E"),
				AM_OutboxMessageTrackingID = new Guid("790be798-2f09-4ba2-bc9a-458648472b6b"),
				AM_TS = transformationSet.TS_PK,
				AM_OutboxFileNameOverride = "Outbox file name",
				AM_InboxFileNameOverride = "Inbox file name"
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "ENTERPRISE",
					Reference1 = "a9881188-7e43-4d57-9f25-f203535d2d8e",
					Reference2 = "Inbox file name",
					Reference3 = "Agency Bill of Lading",
					Reference4 = "This is a name.",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "CMP",
					PriceItemCode = "CMP",
					MessageTrackingID = "a9881188-7e43-4d57-9f25-f203535d2d8e",
				})
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

        [Test]
        public void TestWhenNoTransformationSet_UniversalShipment()
        {
            var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
            var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
            var message = new eHubArchiveMessage
            {
                AM_Status = 3,
                AM_BillingElementCount = 1,
                AM_ArchivedUTC = messageArchivedTime,
                AM_CC_SenderInbox = enterprise.CC_PK,
                AM_CC_RecipientOutbox = thirdParty2.CC_PK,
                AM_ReceivedFromSenderUTC = messageReceivedTime,
                AM_RecipientMessageXML = GetMessageXml("WhenNoTransformationSet_UniversalShipment"),
                AM_ApplicationCode = "UDM",
                AM_InboxMessageTrackingID = new Guid("A9881188-7E43-4D57-9F25-F203535D2D8E"),
                AM_OutboxMessageTrackingID = new Guid("790be798-2f09-4ba2-bc9a-458648472b6b"),
                AM_OutboxFileNameOverride = "Outbox file name",
                AM_InboxFileNameOverride = "Inbox file name"
            };
            AddArchiveMessage(message);

            var expectedTransactions = new[]
            {
                new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
                {
                    BillableCount = 1,
                    ReportingSource = "HUB",
                    ClientID = "ENTERPRISE",
                    Reference1 = "a9881188-7e43-4d57-9f25-f203535d2d8e",
                    Reference2 = "Inbox file name",
                    ServiceOccuredUTC = messageReceivedTime,
                    Category = "CMP",
                    PriceItemCode = "CMP",
                    MessageTrackingID = "a9881188-7e43-4d57-9f25-f203535d2d8e",
                })
            };
            Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
        }

        [Test]
        public void TestWhenNoTransformationSet_UniversalEvent()
        {
            var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
            var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
            var message = new eHubArchiveMessage
            {
                AM_Status = 3,
                AM_BillingElementCount = 1,
                AM_ArchivedUTC = messageArchivedTime,
                AM_CC_SenderInbox = enterprise.CC_PK,
                AM_CC_RecipientOutbox = thirdParty2.CC_PK,
                AM_ReceivedFromSenderUTC = messageReceivedTime,
                AM_RecipientMessageXML = GetMessageXml("WhenNoTransformationSet_UniversalEvent"),
                AM_ApplicationCode = "UDM",
                AM_InboxMessageTrackingID = new Guid("A9881188-7E43-4D57-9F25-F203535D2D8E"),
                AM_OutboxMessageTrackingID = new Guid("790be798-2f09-4ba2-bc9a-458648472b6b"),
                AM_OutboxFileNameOverride = "Outbox file name",
                AM_InboxFileNameOverride = "Inbox file name"
            };
            AddArchiveMessage(message);

            var expectedTransactions = new[]
            {
                new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
                {
                    BillableCount = 1,
                    ReportingSource = "HUB",
                    ClientID = "ENTERPRISE",
                    Reference1 = "a9881188-7e43-4d57-9f25-f203535d2d8e",
                    Reference2 = "Inbox file name",
                    ServiceOccuredUTC = messageReceivedTime,
                    Category = "CMP",
                    PriceItemCode = "CMP",
                    MessageTrackingID = "a9881188-7e43-4d57-9f25-f203535d2d8e",
                })
            };
            Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
        }

        [Test]
        public void TestWhenNoTransformationSet_XmlInterchangeConsol()
        {
            var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
            var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
            var message = new eHubArchiveMessage
            {
                AM_Status = 3,
                AM_BillingElementCount = 1,
                AM_ArchivedUTC = messageArchivedTime,
                AM_CC_SenderInbox = enterprise.CC_PK,
                AM_CC_RecipientOutbox = thirdParty2.CC_PK,
                AM_ReceivedFromSenderUTC = messageReceivedTime,
                AM_RecipientMessageXML = GetMessageXml("WhenNoTransformationSet_XmlInterchangeConsol"),
                AM_ApplicationCode = "XMS",
                AM_InboxMessageTrackingID = new Guid("A9881188-7E43-4D57-9F25-F203535D2D8E"),
                AM_OutboxMessageTrackingID = new Guid("790be798-2f09-4ba2-bc9a-458648472b6b"),
                AM_OutboxFileNameOverride = "Outbox file name",
                AM_InboxFileNameOverride = "Inbox file name"
            };
            AddArchiveMessage(message);

            var expectedTransactions = new[]
            {
                new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
                {
                    BillableCount = 1,
                    ReportingSource = "HUB",
                    ClientID = "ENTERPRISE",
                    Reference1 = "a9881188-7e43-4d57-9f25-f203535d2d8e",
                    Reference2 = "Inbox file name",
                    ServiceOccuredUTC = messageReceivedTime,
                    Category = "CMP",
                    PriceItemCode = "CMP",
                    MessageTrackingID = "a9881188-7e43-4d57-9f25-f203535d2d8e",
                })
            };
            Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
        }

        [Test]
        public void TestWhenNoTransformationSet_XmlInterchangeCartageJob()
        {
            var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
            var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
            var message = new eHubArchiveMessage
            {
                AM_Status = 3,
                AM_BillingElementCount = 1,
                AM_ArchivedUTC = messageArchivedTime,
                AM_CC_SenderInbox = enterprise.CC_PK,
                AM_CC_RecipientOutbox = thirdParty2.CC_PK,
                AM_ReceivedFromSenderUTC = messageReceivedTime,
                AM_RecipientMessageXML = GetMessageXml("WhenNoTransformationSet_XmlInterchangeCartageJob"),
                AM_ApplicationCode = "XMS",
                AM_InboxMessageTrackingID = new Guid("A9881188-7E43-4D57-9F25-F203535D2D8E"),
                AM_OutboxMessageTrackingID = new Guid("790be798-2f09-4ba2-bc9a-458648472b6b"),
                AM_OutboxFileNameOverride = "Outbox file name",
                AM_InboxFileNameOverride = "Inbox file name"
            };
            AddArchiveMessage(message);

            var expectedTransactions = new[]
            {
                new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
                {
                    BillableCount = 1,
                    ReportingSource = "HUB",
                    ClientID = "ENTERPRISE",
                    Reference1 = "a9881188-7e43-4d57-9f25-f203535d2d8e",
                    Reference2 = "Inbox file name",
                    ServiceOccuredUTC = messageReceivedTime,
                    Category = "CMP",
                    PriceItemCode = "CMP",
                    MessageTrackingID = "a9881188-7e43-4d57-9f25-f203535d2d8e",
                })
            };
            Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
        }

        [Test]
        public void TestWhenNoTransformationSet_XmlInterchangeAgnecyBillsOfLading()
        {
            var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
            var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
            var message = new eHubArchiveMessage
            {
                AM_Status = 3,
                AM_BillingElementCount = 1,
                AM_ArchivedUTC = messageArchivedTime,
                AM_CC_SenderInbox = enterprise.CC_PK,
                AM_CC_RecipientOutbox = thirdParty2.CC_PK,
                AM_ReceivedFromSenderUTC = messageReceivedTime,
                AM_RecipientMessageXML = GetMessageXml("WhenNoTransformationSet_XmlInterchangeAgencyBillsOfLading"),
                AM_ApplicationCode = "XMS",
                AM_InboxMessageTrackingID = new Guid("A9881188-7E43-4D57-9F25-F203535D2D8E"),
                AM_OutboxMessageTrackingID = new Guid("790be798-2f09-4ba2-bc9a-458648472b6b"),
                AM_OutboxFileNameOverride = "Outbox file name",
                AM_InboxFileNameOverride = "Inbox file name"
            };
            AddArchiveMessage(message);

            var expectedTransactions = new[]
            {
                new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
                {
                    BillableCount = 30,
                    ReportingSource = "HUB",
                    ClientID = "ENTERPRISE",
                    Reference1 = "a9881188-7e43-4d57-9f25-f203535d2d8e",
                    Reference2 = "Inbox file name",
                    ServiceOccuredUTC = messageReceivedTime,
                    Category = "CMP",
                    PriceItemCode = "CMP",
                    MessageTrackingID = "a9881188-7e43-4d57-9f25-f203535d2d8e",
                })
            };
            Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
        }

		[Test]
		public void TestNoTransactionsWhenClientIdIsNull()
		{
			var transformationSet = new eHubTransformationSet
			{
				TS_CC_Sender = thirdParty.CC_PK,
				TS_Name = "This is a name.",
				TS_CC_Recipient = thirdParty2.CC_PK,
				TS_BillingXPathSource = @"//*[local-name()=""DataContext""]/*[local-name()=""DataSourceCollection"" or local-name()=""DataTargetCollection""][contains(., ""ForwardingShipment"") and not(contains(., ""ForwardingConsol""))]",
				TS_BillingElement = "Per Message",
				TS_BillSender = false,
				TS_BillRecipient = true,
			};
			AddTransformationSet(transformationSet);

			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = thirdParty.CC_PK,
				AM_CC_RecipientOutbox = thirdParty2.CC_PK,
				AM_SenderMessageXML = GetMessageXml("MessageFromThirdPartyToEnterprise"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_OutboxMessageTrackingID = new Guid("790be798-2f09-4ba2-bc9a-458648472b6b"),
				AM_TS = transformationSet.TS_PK,
				AM_OutboxFileNameOverride = "Outbox file name",
				AM_InboxFileNameOverride = "Inbox file name"
			};
			AddArchiveMessage(message);

			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.Empty);
		}

		[Test]
		public void TestCw1ToXh()
		{
			var transformationSet = new eHubTransformationSet
			{
				TS_CC_Sender = enterprise.CC_PK,
				TS_Name = "This is a name.",
				TS_CC_Recipient = xhClient.CC_PK,
				TS_BillingXPathSource = @"/*",
				TS_BillingElement = "Per Message",
				TS_BillSender = true,
				TS_BillRecipient = false,
			};
			AddTransformationSet(transformationSet);

			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = enterprise.CC_PK,
				AM_CC_RecipientOutbox = xhClient.CC_PK,
				AM_SenderMessageXML = GetMessageXml("UniversalTransaction"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("790be798-2f09-4ba2-bc9a-458648472b6b"),
				AM_TS = transformationSet.TS_PK,
				AM_OutboxFileNameOverride = "Outbox file name",
				AM_InboxFileNameOverride = "Inbox file name"
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "ENTERPRISE",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference2 = "Inbox file name",
					Reference3 = "Per Message",
					Reference4 = "This is a name.",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "CMP",
					PriceItemCode = "CMP",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				})
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestXhToCw1()
		{
			var transformationSet = new eHubTransformationSet
			{
				TS_CC_Sender = xhClient.CC_PK,
				TS_Name = "This is a name.",
				TS_CC_Recipient = enterprise.CC_PK,
				TS_BillingXPathTarget = @"/*[local-name()=""UniversalInterchange""]/*[local-name()=""Body""]/*[local-name()=""UniversalTransaction""]/*[local-name()=""TransactionInfo""]",
				TS_BillingElement = "TransactionInfo",
				TS_BillSender = false,
				TS_BillRecipient = true,
			};
			AddTransformationSet(transformationSet);

			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var message = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = xhClient.CC_PK,
				AM_CC_RecipientOutbox = enterprise.CC_PK,
				AM_RecipientMessageXML = GetMessageXml("UniversalTransaction"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_OutboxMessageTrackingID = new Guid("790be798-2f09-4ba2-bc9a-458648472b6b"),
				AM_TS = transformationSet.TS_PK,
				AM_OutboxFileNameOverride = "Outbox file name",
				AM_InboxFileNameOverride = "Inbox file name"
			};
			AddArchiveMessage(message);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 2,
					ReportingSource = "HUB",
					ClientID = "ENTERPRISE",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference2 = "Outbox file name",
					Reference3 = "TransactionInfo",
					Reference4 = "This is a name.",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "CMP",
					PriceItemCode = "CMP",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				})
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();

			enterprise = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "ENTERPRISE"));
			enterprise2 = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "ENTERPRISE2"));

			thirdParty = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.ThirdPartyClient, "THIRDPARTY"));
			thirdParty2 = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.ThirdPartyClient, "THIRDPARTY2"));

			xhClient = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.XhClient, "XHCLIENT"));
		}

		eHubClient enterprise;
		eHubClient enterprise2;
		eHubClient thirdParty;
		eHubClient thirdParty2;
		eHubClient xhClient;
	}
}
