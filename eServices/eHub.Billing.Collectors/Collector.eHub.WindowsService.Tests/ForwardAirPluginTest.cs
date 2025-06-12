using System;
using System.Collections.Generic;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Tests
{
	class ForwardAirPluginTest : SqlBillingTransactionsPluginTest<Plugins.ForwardAir.Plugin>
	{

		protected override string[] QueryColumns
		{
			get { return new[] { "ClientID", "MessageTrackingID", "Consol", "AM_ReceivedFromSenderUTC", "AM_ArchivedUTC" }; }
		}

		protected override IReadOnlyList<object[]> QueryResult
		{
			get
			{
				return new List<object[]>
				{
					new object[] {"GFEMCIMCI", new Guid("8d2e88a3-445b-4212-b057-ab376245b17c"), "C00133188", DateTime.Parse("2014-09-30 23:09:16.013"), DateTime.Parse("2014-09-30 23:10:16.013")},
				};
			}
		}

		protected override void AssertTransactions(TimeStampedTransaction[] transactions)
		{
			Assert.That(transactions.Length, Is.EqualTo(1));
			AssertTransaction(transactions[0], DateTime.Parse("2014-09-30 23:10:16.013"), 1, "GFEMCIMCI", null, null, "FWA", "FWA", "8d2e88a3-445b-4212-b057-ab376245b17c", "C00133188", null, null, null, "HUB", DateTime.Parse("2014-09-30 23:09:16.013"), "8d2e88a3-445b-4212-b057-ab376245b17c");
		}
	}
}
