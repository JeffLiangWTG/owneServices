using System;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubArchiveOnline;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.JPCustoms;
using NUnit.Framework;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientFactory;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.Plugins.JPCustoms
{
	class JPCustomsTest : PluginTest<Plugin>
	{

		[Test]
		public void TestBillingJPCustoms()
		{
			var registry = new eHubMessageReferenceRegistry()
			{
				CR_ApplicationCode = "JPC",
				CR_MessageReference = "JE3EX",
				CR_Password = "password",
				CR_CC_Client = client.CC_PK,
			};
			AddMessageReferenceRegistry(registry);

			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var messageToJPCustoms = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_ApplicationCode = "SUB",
				AM_CC_SenderInbox = client.CC_PK,
				AM_CC_RecipientOutbox = JPCustoms.CC_PK,
				AM_RecipientMessageXML = GetMessageXml("MessageToJPCustoms"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("{790BE798-2F09-4BA2-BC9A-458648472B6B}"),
			};
			AddArchiveMessage(messageToJPCustoms);

			var messageFromJPCustoms = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_ApplicationCode = "BIZ",
				AM_CC_SenderInbox = JPCustoms.CC_PK,
				AM_CC_RecipientOutbox = client.CC_PK,
				AM_RecipientMessageXML = GetMessageXml("MessageFromJPCustoms"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("{3B99CF0D-5146-43C8-AB97-47B209A02E82}"),
			};
			AddArchiveMessage(messageFromJPCustoms);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "JPC",
					PriceItemCode = "JPC",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				}),

				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "SAIC15122015",
					Reference2 = "E00C15122015",
					Reference3 = "AHR9",
					Reference4 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "JPC",
					PriceItemCode = "AFR",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				})
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}
		protected override void SetUpCore()
		{
			base.SetUpCore();
			client = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "TSTCLIENT"));
			JPCustoms = AddClient(eHubClientFactory.CreateeHubClient(eHubClientType.ServiceProvider, "JPCustoms"));
		}
		eHubClient client;
		eHubClient JPCustoms;
	}
}
