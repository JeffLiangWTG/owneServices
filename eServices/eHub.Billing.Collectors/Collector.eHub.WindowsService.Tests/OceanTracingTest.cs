using System;
using System.Collections.Generic;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Tests
{
	class OceanTracingTest : SqlBillingTransactionsPluginTest<Plugins.OceanTracing.Plugin>
	{
		protected override string[] QueryColumns
		{
			get { return new[] { "AM_ReceivedFromSenderUTC", "ClientID", "MessageTrackingID", "EventType", "EventTime", "ContainerNumber", "AM_ArchivedUTC" }; }
		}

		protected override IReadOnlyList<object[]> QueryResult
		{
			get
			{
				return new List<object[]>
				{
					new object[] {DateTime.Parse("2014-11-17 22:20:38.310"), "PSCCHCCHC_EDI", new Guid("9c5f024b-d7a7-49d1-b37a-49a9736a1fb3"), "36", "201411181024", "CGSU0900244", DateTime.Parse("2014-11-17 22:21:38.310")},
				};
			}
		}

		protected override void AssertTransactions(TimeStampedTransaction[] transactions)
		{
			Assert.That(transactions.Length, Is.EqualTo(1));
			AssertTransaction(transactions[0], DateTime.Parse("2014-11-17 22:21:38.310"), 1, "PSCCHCCHC", null, null, "OCT", "OCT", "9c5f024b-d7a7-49d1-b37a-49a9736a1fb3", "36", "201411181024", "CGSU0900244", null, "HUB", DateTime.Parse("2014-11-17 22:20:38.310"), "9c5f024b-d7a7-49d1-b37a-49a9736a1fb3");
		}
	}
}
