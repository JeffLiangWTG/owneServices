using System;
using System.Linq;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubArchiveOnline;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.AirMessaging;
using NUnit.Framework;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientFactory;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.Plugins.AirMessaging
{
	class AirMessagingTest : PluginTest<Plugin>
	{
		[Test]
		public void TestMessageToProvider()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
			var messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);
			var messageToProvider = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = client.CC_PK,
				AM_CC_RecipientInbox = service.CC_PK,
				AM_CC_RecipientOutbox = provider.CC_PK,
				AM_SenderMessageRaw = GetMessageRaw("MessageToProvider", "xml"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("{790BE798-2F09-4BA2-BC9A-458648472B6B}"),
			};
			AddArchiveMessage(messageToProvider);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference2 = "APROVIDER",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "AMG",
					PriceItemCode = "ALM",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "20581530046",
					Reference2 = "YUS01448403",
					Reference3 = "NH",
					Reference4 = "APROVIDER",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "AMG",
					PriceItemCode = "FHL",
				})
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestMessageFromProvider()
		{
			var messageReceivedTime = new DateTime(2015, 1, 11, 12, 45, 40);
			var messageArchivedTime = new DateTime(2015, 1, 11, 12, 47, 30);
			var messageFromProvider = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = provider.CC_PK,
				AM_CC_RecipientOutbox = client.CC_PK,
				AM_SenderMessageXML = GetMessageXml("MessageFromProvider"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_OutboxMessageTrackingID = new Guid("{68DB048B-0F17-41E0-A3B6-D71CD8BAB43E}"),
				AM_DT_RecipientMessageType = messageType.DT_PK
			};
			AddArchiveMessage(messageFromProvider);

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "68db048b-0f17-41e0-a3b6-d71cd8bab43e",
					Reference2 = "APROVIDER",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "AMG",
					PriceItemCode = "ALM",
					MessageTrackingID = "68db048b-0f17-41e0-a3b6-d71cd8bab43e",
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Reference1 = "91968284",
					Reference3 = "933",
					Reference4 = "APROVIDER",
					ServiceOccuredUTC = messageReceivedTime,
					Category = "AMG",
					PriceItemCode = "FMA",
				})
			};
			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestMessageFromProvider_BT()
		{
			var messageReceivedTime = new DateTime(2017, 5, 19, 12, 45, 40);
			var messageArchivedTime = new DateTime(2017, 5, 19, 12, 47, 30);
			var messageFromProvider = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = providerBT.CC_PK,
				AM_CC_RecipientOutbox = client.CC_PK,
				AM_SenderMessageXML = null,
				AM_SenderMessageRaw = GetMessageRaw("MessageFromProvider_BT", "txt"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_OutboxMessageTrackingID = new Guid("{CCD0EF4F-34A9-444D-8790-CAAAEC3FD3B3}"),
				AM_DT_RecipientMessageType = messageType.DT_PK
			};
			AddArchiveMessage(messageFromProvider);

			var expectedTransactions = new TimeStampedTransaction[0];

			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestMessageFromProvider_GLSHK_ISAC()
		{
			var hkMessageType = new eHubMessageType("HKCustoms")
			{
				DT_PK = Guid.Parse("15A42A38-8111-40CC-8A89-D6C5981C236A")
			};
			AddMessageType(hkMessageType);
			var messageReceivedTime = new DateTime(2019, 1, 11, 12, 45, 40);
			var messageArchivedTime = new DateTime(2019, 1, 11, 12, 47, 30);
			var messageFromProvider = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = providerGLSHK.CC_PK,
				AM_CC_SenderOutbox = providerGLSHK.CC_PK,
				AM_CC_RecipientOutbox = client.CC_PK,
				AM_SenderMessageXML = null,
				AM_SenderMessageRaw = GetMessageRaw("MessageFromProvider_GLSHK_ISAC", "txt"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_OutboxMessageTrackingID = Guid.NewGuid(),
				AM_DT_RecipientMessageType = hkMessageType.DT_PK
			};
			AddArchiveMessage(messageFromProvider);

			var expectedTransactions = new TimeStampedTransaction[0];

			Assert.That(RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1)), Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestMessageFromProvider_EmptyMAWB()
		{
			var hkMessageType = new eHubMessageType("HKCustoms")
			{
				DT_PK = Guid.Parse("15A42A38-8111-40CC-8A89-D6C5981C236A")
			};
			AddMessageType(hkMessageType);
			var messageReceivedTime = new DateTime(2019, 1, 11, 12, 45, 40);
			var messageArchivedTime = new DateTime(2019, 1, 11, 12, 47, 30);
			var messageFromProvider = new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_CC_SenderInbox = providerGLSHK.CC_PK,
				AM_CC_SenderOutbox = providerGLSHK.CC_PK,
				AM_CC_RecipientOutbox = client.CC_PK,
				AM_SenderMessageXML = null,
				AM_SenderMessageRaw = GetMessageRaw("MessageFromProvider_EmptyMAWB", "xml"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_OutboxMessageTrackingID = Guid.NewGuid(),
				AM_DT_RecipientMessageType = hkMessageType.DT_PK
			};
			AddArchiveMessage(messageFromProvider);

			var result = RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1));
			Assert.AreEqual(result.Count(), 0);
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			client = eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "TSTCLIENT");
			provider = eHubClientFactory.CreateeHubClient(eHubClientType.ServiceProvider, "APROVIDER");
			providerBT = eHubClientFactory.CreateeHubClient(eHubClientType.ServiceProvider, "BT");
			provider.CC_IsAirServiceProvider = true;
			providerBT.CC_IsAirServiceProvider = true;
			providerGLSHK = eHubClientFactory.CreateeHubClient(eHubClientType.ServiceProvider, "GLSHK");
			providerGLSHK.CC_IsAirServiceProvider = true;
			service = eHubClientFactory.CreateeHubClient(eHubClientType.ServiceProvider, "eHubAirService");
			service.CC_PK = Guid.Parse("9819EFF9-9CD8-4622-B58E-32A251115722");
			messageType = new eHubMessageType("http://www.cargowise.com/Schemas#MessageType");
			AddClient(client);
			AddClient(provider);
			AddClient(providerBT);
			AddClient(providerGLSHK);
			AddClient(service);
			AddMessageType(messageType);
		}

		eHubClient client;
		eHubClient provider;
		eHubClient service;
		eHubClient providerBT;
		eHubClient providerGLSHK;
		private eHubMessageType messageType;
	}
}
