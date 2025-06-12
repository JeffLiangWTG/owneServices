using System;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubArchiveOnline;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientFactory;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.SGCustoms;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.Plugins.SGCustoms
{
	class SGCustomsTest : PluginTest<Plugin>
	{
		readonly DateTime messageReceivedTime = new DateTime(2015, 1, 11, 9, 31, 0);
		readonly DateTime messageArchivedTime = new DateTime(2015, 1, 11, 9, 35, 0);

		[Test]
		public void TestMsg_AEDSingleHouseWayBill_SingleTransaction()
		{
			AddArchiveMessage(SetUpTransaction("Msg_AED_SingleHouseWayBill"));

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "SAC",
					PriceItemCode = "SAE",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageReceivedTime,
					ClientID = "TSTCLIENT",
					Branch = "PRE",
					ClientStaffCode = "TKW",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference1 = "MAN0000384",
					Reference2 = "61849405436",
					Reference3 = "30A8R7VT3F3"
				})
			};

			var actual = RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1));
			Assert.That(actual, Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestMsg_AEDMultipleHouseWayBills_MultipleTransactions()
		{
			AddArchiveMessage(SetUpTransaction("Msg_AED_MultipleHouseWayBills"));

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "SAC",
					PriceItemCode = "SAE",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageReceivedTime,
					ClientID = "TSTCLIENT",
					Branch = "PRE",
					ClientStaffCode = "TKW",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference1 = "MAN0000652",
					Reference2 = "61849406523",
					Reference3 = "R8920RQXX47"
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "SAC",
					PriceItemCode = "SAE",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageReceivedTime,
					ClientID = "TSTCLIENT",
					Branch = "PRE",
					ClientStaffCode = "TKW",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference1 = "MAN0000652",
					Reference2 = "61849406523",
					Reference3 = "R8920RQXX38"
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "SAC",
					PriceItemCode = "SAE",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageReceivedTime,
					ClientID = "TSTCLIENT",
					Branch = "PRE",
					ClientStaffCode = "TKW",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference1 = "MAN0000652",
					Reference2 = "61849406523",
					Reference3 = "R8920RQXX3H"
				})
			};

			var actual = RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1));
			Assert.That(actual, Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestMsg_PCMSingleHouseWayBill_SingleTransaction()
		{
			AddArchiveMessage(SetUpTransaction("Msg_PCM_SingleHouseWayBill"));

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "SAC",
					PriceItemCode = "SAI",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageReceivedTime,
					ClientID = "TSTCLIENT",
					Branch = "PRE",
					ClientStaffCode = "SMS",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference1 = "MAN0001295",
					Reference2 = "61848613084",
					Reference3 = "H9307457635"
				})
			};

			var actual = RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1));
			Assert.That(actual, Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestMsg_PCMMultipleHouseWayBills_MultipleTransactions()
		{
			AddArchiveMessage(SetUpTransaction("Msg_PCM_MultipleHouseWayBills"));

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "SAC",
					PriceItemCode = "SAI",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageReceivedTime,
					ClientID = "TSTCLIENT",
					Branch = "PRE",
					ClientStaffCode = "SMS",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference1 = "MAN0001295",
					Reference2 = "61848613084",
					Reference3 = "H9307457635"
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "SAC",
					PriceItemCode = "SAI",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageReceivedTime,
					ClientID = "TSTCLIENT",
					Branch = "PRE",
					ClientStaffCode = "SMS",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference1 = "MAN0001295",
					Reference2 = "61848613084",
					Reference3 = "3143XY3J83B"
				}),
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					Category = "SAC",
					PriceItemCode = "SAI",
					BillableCount = 1,
					ReportingSource = "HUB",
					ServiceOccuredUTC = messageReceivedTime,
					ClientID = "TSTCLIENT",
					Branch = "PRE",
					ClientStaffCode = "SMS",
					MessageTrackingID = "790be798-2f09-4ba2-bc9a-458648472b6b",
					Reference1 = "MAN0001295",
					Reference2 = "61848613084",
					Reference3 = "V0169867605"
				})
			};

			var actual = RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1));
			Assert.That(actual, Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestMsg_ActionPurposeIsNotInRange_ZeroTransaction()
		{
			AddArchiveMessage(SetUpTransaction("Msg_ActionPurposeIsNotInRange"));

			var expectedTransactions = new eHubArchiveMessage[0];

			var actual = RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1));
			Assert.That(actual, Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestMsg_RecipientRoleCodeIsNotInRange_ZeroTransaction()
		{
			AddArchiveMessage(SetUpTransaction("Msg_RecipientRoleCodeIsNotInRange"));

			var expectedTransactions = new eHubArchiveMessage[0];

			var actual = RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1));
			Assert.That(actual, Is.EquivalentTo(expectedTransactions));
		}

		eHubArchiveMessage SetUpTransaction(string fileName)
		{
			return new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ApplicationCode = "SAC",
				AM_CC_SenderInbox = client.CC_PK,
				AM_CC_RecipientInbox = SGCustoms.CC_PK,
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_ArchivedUTC = messageArchivedTime,
				AM_SenderMessageXML = GetMessageXml(fileName),
				AM_InboxMessageTrackingID = new Guid("790be798-2f09-4ba2-bc9a-458648472b6b")
			};
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();

			client = eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "TSTCLIENT");
			SGCustoms = eHubClientFactory.CreateeHubClient(eHubClientType.ServiceProvider, "SGCustoms");
			AddClient(client);
			AddClient(SGCustoms);
		}

		eHubClient client;
		eHubClient SGCustoms;
	}
}
