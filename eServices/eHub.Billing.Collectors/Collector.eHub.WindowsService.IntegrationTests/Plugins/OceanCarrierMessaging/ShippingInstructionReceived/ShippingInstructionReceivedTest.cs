using System;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubArchiveOnline;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientFactory;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.OceanCarrierMessaging.ShippingInstructionReceived;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.Plugins.OceanCarrierMessaging.ShippingInstructionReceived
{
	class ShippingInstructionReceivedTest : PluginTest<Plugin>
	{

		[Test]
		public void TestReceivedValidMessageFromServiceProvider()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var messageToShippingInstruction = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_ApplicationCode = "UDM",
				AM_CC_SenderInbox = provider.CC_PK,
				AM_CC_RecipientOutbox = client.CC_PK,
				AM_RecipientMessageXML = GetMessageXml("MessageFromServiceProvider"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_OutboxMessageTrackingID = new Guid("{790BE798-2F09-4BA2-BC9A-458648472B6B}"),
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
					Reference3 = "C600726188",
					Reference4 = "MAA",
					Reference5 = "NAM2471591",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "SHI",
					PriceItemCode = "SHR",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Version = 3
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestStatusEvent()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var messageToShippingInstruction = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_ApplicationCode = "UDM",
				AM_CC_SenderInbox = provider.CC_PK,
				AM_CC_RecipientOutbox = client.CC_PK,
				AM_RecipientMessageXML = GetMessageXml("StatusEvent"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_OutboxMessageTrackingID = new Guid("{a54aa5bf-0cba-409e-9930-16ecdf5f2923}"),
			};
			AddArchiveMessage(messageToShippingInstruction);
			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "a54aa5bf-0cba-409e-9930-16ecdf5f2923",
					Reference2 = "INTTRA",
					Reference3 = "C600726188",
					Reference4 = "MAA",
					Reference5 = "NAM2471591",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "SHI",
					PriceItemCode = "SHR",
					MessageTrackingID = "a54aa5bf-0cba-409e-9930-16ecdf5f2923",
					Version = 3
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			client = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "TSTCLIENT"));
			provider = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.ServiceProvider, "INTTRA"));
			shippingInstruction = eHubClientFactory.CreateeHubClient(eHubClientType.ServiceProvider, "SHIPPING_INSTRUCTION");
			shippingInstruction.CC_PK = new Guid("a8f34356-f459-4805-8026-9cb072b7e0a3");
			AddClient(shippingInstruction);
			AddServiceProvider(new eHubServiceProvider(shippingInstruction.CC_PK, provider.CC_PK));
		}
		eHubClient client;
		eHubClient provider;
		eHubClient shippingInstruction;
	}
}
