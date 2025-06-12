using System;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubArchiveOnline;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.OceanTracing;
using NUnit.Framework;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientFactory;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.Plugins.OceanTracing
{
	class OceanTracingTest : PluginTest<Plugin>
	{

		[Test]
		public void TestReceiveTracingMessage()
		{
			var transformationSet = new eHubTransformationSet
			{
				TS_CC_Sender = oceanContainers.CC_PK,
				TS_Name = "Ocean Containers - Test Email",
				TS_CC_Recipient = client.CC_PK,
				TS_BillingElement = "Event",
				TS_BillingXPathTarget = "/*[local-name()=\"EFACT_D95B_CODECO\"]/*[local-name()=\"EQDLoop1\"]",
				TS_BillSender = false,
				TS_BillRecipient = true,
			};
			AddTransformationSet(transformationSet);

			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var messageT = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_TS = transformationSet.TS_PK,
				AM_CC_SenderInbox = oceanContainers.CC_PK,
				AM_CC_RecipientOutbox = client.CC_PK,
				AM_RecipientMessageXML = GetMessageXml("MessageToClient"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_OutboxMessageTrackingID = new Guid("{790BE798-2F09-4BA2-BC9A-458648472B6B}"),
			};
			AddArchiveMessage(messageT);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference2 = "34",
					Reference3 = "201504152336",
					Reference4 = "SEGU2136037",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "OCT",
					PriceItemCode = "OCT",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				}),
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			client = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "TSTCLIENT"));
			oceanContainers = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "TTTT"));
		}
		eHubClient client;
		eHubClient oceanContainers;
	}
}
