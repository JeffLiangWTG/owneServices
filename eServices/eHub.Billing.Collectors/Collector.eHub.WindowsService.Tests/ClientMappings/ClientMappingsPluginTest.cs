using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ClientMappings;
using CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Tests
{
	class ClientMappingsPluginTest : SqlBillingTransactionsPluginTest<Plugins.ClientMappings.Plugin>
	{
		protected override string[] QueryColumns
		{
			get { return new[] { "ClientID", "SenderID", "RecipientID", "BillableCount", "AM_ReceivedFromSenderUTC", "MessageTrackingID", "FileName", "TS_BillingElement", "TS_Name", "AM_ArchivedUTC" }; }
		}

		protected override IReadOnlyList<object[]> QueryResult
		{
			get
			{
				return new List<object[]>
				{
					new object[] {"ABCDEFXYZ", "AAAAAAAAA_AAA", "BBBBBBBBB", 4, DateTime.Parse("2014-11-10 15:22:20.235"), new Guid("fe7e1cce-f133-4c27-aef5-557c67e0acee"), "Some_File.xml", "Shipment", "Interface to Import Charges and Costs for YJP Air Import Shipments", DateTime.Parse("2014-11-10 15:23:20.235")},
					new object[] { "AG1PQCSIN", "AG1PQCSIN_DOR", "AG1PQCSIN", 4, DateTime.Parse("2014-11-10 15:22:20.235"), new Guid("040eff10-ce3f-4a73-a016-99c4b3b9b192"), "Some_File.xml", "Shipment", "Dole Order csv-file: Receive OrderManager Orders", DateTime.Parse("2014-11-10 15:23:20.235")},
				};
			}
		}

		protected override void AssertTransactions(TimeStampedTransaction[] transactions)
		{
			Assert.That(transactions.Length, Is.EqualTo(3));
			AssertTransaction(transactions[0], DateTime.Parse("2014-11-10 15:23:20.235"), 4, "ABCDEFXYZ", null, null, "CMP", "CMP", "fe7e1cce-f133-4c27-aef5-557c67e0acee", "Some_File.xml", "Shipment", "Interface to Import Charges and Costs for YJP Air Import Shipments", null, "HUB", DateTime.Parse("2014-11-10 15:22:20.235"), "fe7e1cce-f133-4c27-aef5-557c67e0acee");
			AssertTransaction(transactions[1], DateTime.Parse("2014-11-10 15:23:20.235"), 4, "AG1PQCSIN", null, null, "CMP", "CMP", "040eff10-ce3f-4a73-a016-99c4b3b9b192", "Some_File.xml", "Shipment", "Dole Order csv-file: Receive OrderManager Orders", null, "HUB", DateTime.Parse("2014-11-10 15:22:20.235"), "040eff10-ce3f-4a73-a016-99c4b3b9b192");
			AssertTransaction(transactions[2], DateTime.Parse("2014-11-10 15:23:20.235"), 4, "AG1PQCSIN", null, null, "CMP", "CMF", "040eff10-ce3f-4a73-a016-99c4b3b9b192", "Some_File.xml", "Shipment", "Dole Order csv-file: Receive OrderManager Orders", null, "HUB", DateTime.Parse("2014-11-10 15:22:20.235"), "040eff10-ce3f-4a73-a016-99c4b3b9b192");
		}

		protected override void SetUpInternal()
		{
			base.SetUpInternal();
			var csvFilePath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "ClientMappings\\ClientMappingsInsecureFTPTest.csv");
			mockPlugin.Setup(_ => _.InsecureFTP).Returns(new ClientMappingsInsecureFTP(csvFilePath));
		}
	}
}
