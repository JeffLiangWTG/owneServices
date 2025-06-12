using System;
using System.Linq;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubArchiveOnline;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.Helpers.eHubClientFactory;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ZACustoms;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.Plugins.ZACustoms
{
	class ZACustomsTest : PluginTest<Plugin>
	{
		readonly DateTime messageReceivedTime = new DateTime(2017, 1, 1, 1, 01, 0);
		readonly DateTime messageArchivedTime = new DateTime(2017, 1, 1, 1, 05, 0);

		[Test]
		public void TestMessageFromClientToZACustoms_PriceItemCode_ZX1()
		{
			AddArchiveMessage(SetUpTransaction("MessageToZACustoms_ZX1_HAB"));

			var expectedTransactions = new[]
			{
				GetExpectedTransaction("ZX1")
			};
			
			var actual = RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1));
			Assert.That(actual, Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestMessageFromClientToZACustoms_NoCarrierCode()
		{
			AddArchiveMessage(SetUpTransaction("MessageToZACustoms_EmptyCarrierCode"));

			var actual = RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1));
			Assert.AreEqual(0, actual.Count());
		}

		[Test]
		public void TestMessageFromClientToZACustoms_PriceItemCode_ZX2()
		{
			AddArchiveMessage(SetUpTransaction("MessageToZACustoms_ZX2_FFM"));

			var expectedTransactions = new[]
			{
				GetExpectedTransaction("ZX2")
			};

			var actual = RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1));
			Assert.That(actual, Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestMessageFromClientToZACustoms_PriceItemCode_ZX3()
		{
			AddArchiveMessage(SetUpTransaction("MessageToZACustoms_ZX3_COM"));

			var expectedTransactions = new[]
			{
				GetExpectedTransaction("ZX3")
			};

			var actual = RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1));
			Assert.That(actual, Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestMessageFromClientToZACustoms_MessageName_IsNotInRange()
		{
			AddArchiveMessage(SetUpTransaction("MessageToZACustoms_MessageName_IsNotInRange_CCC"));

			var expectedTransactions = new eHubArchiveMessage[0];

			var actual = RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1));
			Assert.That(actual, Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestMessageFromClientToZACustoms_IsNotOriginal()
		{
			AddArchiveMessage(SetUpTransaction("MessageToZACustoms_IsNotOriginal_7"));

			var expectedTransactions = new eHubArchiveMessage[0];

			var actual = RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1));
			Assert.That(actual, Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestGetReferences()
		{
			AddArchiveMessage(SetUpTransaction("MessageToZACustoms_GetReferences"));

			var expectedTransactions = new[]
			{
				new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
				{
					ServiceOccuredUTC = messageReceivedTime,
					PriceItemCode = "ZX1",
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = "TSTCLIENT",
					Category = "ZAC",
					MessageTrackingID = "d903b320-6268-4228-8fb8-ea9826f3b51f",
					Reference1 = "SAFM",
					Reference2 = "SEABOLMANIFESTTES",
					Reference3 = "V87P",
					Reference4 = "C00028163",
					Reference5 = "20170407"
				}),
			};

			var actual = RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1));
			Assert.That(actual, Is.EquivalentTo(expectedTransactions));
		}

	    [Test]
	    public void TestMessageFromClientToZACustoms_NoColon_NAD()
	    {
	        AddArchiveMessage(SetUpTransaction("MessageToZACustoms_NoColon_NAD"));

	        var expectedTransactions = new[]
	        {
	            new TimeStampedTransaction(messageArchivedTime, new BillingTransaction
	            {
	                ServiceOccuredUTC = messageReceivedTime,
	                PriceItemCode = "ZX1",
	                BillableCount = 1,
	                ReportingSource = "HUB",
	                ClientID = "TSTCLIENT",
	                Category = "ZAC",
	                MessageTrackingID = "d903b320-6268-4228-8fb8-ea9826f3b51f",
	                Reference1 = "SAFM",
	                Reference2 = "SEABOLMANIFESTTES",
	                Reference3 = "V87P",
	                Reference4 = "C00028163",
	                Reference5 = "20170407"
	            }),
	        };

            var actual = RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1));
	        Assert.That(actual, Is.EquivalentTo(expectedTransactions));
	    }

		[Test]
		public void TestMessageFromClientToZACustoms_MsgType_AQM()
		{
			AddArchiveMessage(SetUpTransaction("MessageToZACustoms_MsgType_AQM"));

			var expectedTransactions = new[]
			{
				GetExpectedTransaction("ZX1")
			};

			var actual = RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1));
			Assert.That(actual, Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestMessageFromClientToZACustoms_MsgType_ALM()
		{
			AddArchiveMessage(SetUpTransaction("MessageToZACustoms_MsgType_ALM"));

			var expectedTransactions = new[]
			{
				GetExpectedTransaction("ZX1")
			};

			var actual = RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1));
			Assert.That(actual, Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestMessageFromClientToZACustoms_MsgType_ALH()
		{
			AddArchiveMessage(SetUpTransaction("MessageToZACustoms_MsgType_ALH"));

			var expectedTransactions = new[]
			{
				GetExpectedTransaction("ZX1")
			};

			var actual = RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1));
			Assert.That(actual, Is.EquivalentTo(expectedTransactions));
		}

		[Test]
		public void TestMessageFromClientToZACustoms_ALH_No_BillDate_NoBillingTransactionReturned()
		{
			AddArchiveMessage(SetUpTransaction("MessageToZACustoms_ALH_No_BillDate"));

			var actual = RunPlugin(messageArchivedTime.AddMinutes(-1), messageArchivedTime.AddMinutes(1));
			Assert.AreEqual(0, actual.Count());
		}

		eHubArchiveMessage SetUpTransaction(string fileName)
		{
			return new eHubArchiveMessage
			{
				AM_Status = 3,
				AM_ArchivedUTC = messageArchivedTime,
				AM_ApplicationCode = "ZAC",
				AM_CC_SenderInbox = client.CC_PK,
				AM_CC_RecipientInbox = ZACustoms.CC_PK,
				AM_SenderMessageRaw = GetMessageRaw(fileName, "txt"),
				AM_ReceivedFromSenderUTC = messageReceivedTime,
				AM_InboxMessageTrackingID = new Guid("{D903B320-6268-4228-8FB8-EA9826F3B51F}")
			};
		}

		TimeStampedTransaction GetExpectedTransaction(string priceItemCode)
		{
			var tx = new BillingTransaction
			{
				ServiceOccuredUTC = messageReceivedTime,
				PriceItemCode = priceItemCode,
				BillableCount = 1,
				ReportingSource = "HUB",
				ClientID = "TSTCLIENT",
				Category = "ZAC",
				MessageTrackingID = "d903b320-6268-4228-8fb8-ea9826f3b51f",
				Reference1 = "235",
				Reference3 = "TK0044",
				Reference4 = "CAI17B303493",
			};

			switch (priceItemCode)
			{
				case "ZX1":
					tx.Reference2 = "235-62857970";
					tx.Reference5 = "20170314";
					break;
				case "ZX3":
					tx.Reference2 = "20170314";
					break;
			}

			return new TimeStampedTransaction(messageArchivedTime, tx);
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();

			client = eHubClientFactory.CreateeHubClient(eHubClientType.CW1Client, "TSTCLIENT");
			ZACustoms = eHubClientFactory.CreateeHubClient(eHubClientType.ServiceProvider, "ZACustoms");
			AddClient(client);
			AddClient(ZACustoms);
		}

		eHubClient client;
		eHubClient ZACustoms;
	}
}
