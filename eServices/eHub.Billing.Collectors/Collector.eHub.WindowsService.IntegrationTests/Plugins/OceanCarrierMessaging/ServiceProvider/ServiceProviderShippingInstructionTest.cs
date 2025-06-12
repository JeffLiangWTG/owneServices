using System;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubArchiveOnline;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.OceanCarrierMessaging.ServiceProviderShippingInstructions;
using NUnit.Framework;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientFactory;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.Plugins.OceanCarrierMessaging.ServiceProvider
{
	class ServiceProviderShippingInstructionsTest : PluginTest<Plugin>
	{
		
		[Test]
		public void TestSendShippingInstructions()
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
				AM_SenderMessageXML = GetMessageXml("ShippingInstructionsInbox"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("790BE798-2F09-4BA2-BC9A-458648472B6B"),
			};
			AddArchiveMessage(messageToShippingInstruction);
			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					ClientID = "TSTCLIENT",
					BillableCount = 1,
					ReportingSource = "HUB",
					Reference1 = provider.CC_ID,
					Reference2 = client.CC_ID,
					Reference3 = "C701006159",
					Reference4 = "LON/VER/150481",
					Reference5 = "ECUW",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "SHI",
					PriceItemCode = "SHN",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
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
