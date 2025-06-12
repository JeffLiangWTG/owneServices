using System;
using System.Collections.Generic;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Tests
{
	public class ASYCUDATest : SqlBillingTransactionsPluginTest<Plugins.ASYCUDA.Plugin>
	{
		protected override string[] QueryColumns
		{
			get { return new[] { "ClientID", "MessageTrackingID", "Consol", "CountryCode", "AM_ReceivedFromSenderUTC", "AM_ArchivedUTC" }; }
		}

		protected override IReadOnlyList<object[]> QueryResult
		{
			get
			{
				return new List<object[]>
				{
					new object[] {"EDIEDIDAT", Guid.Parse("c94ec031-6ceb-472a-b07c-2bbf1d554c0f"),"C00001000", "SB", DateTime.Parse("2014-09-30 23:09:16.013"), DateTime.Parse("2014-09-30 23:10:16.013")}
				};
			}
		}

		protected override void AssertTransactions(TimeStampedTransaction[] transactions)
		{
			Assert.That(transactions.Length, Is.EqualTo(1));
			AssertTransaction(transactions[0], DateTime.Parse("2014-09-30 23:10:16.013"), 1, "EDIEDIDAT", null, null ,"ASC", "ASC", "c94ec031-6ceb-472a-b07c-2bbf1d554c0f", "C00001000", "SB", null, null, "HUB", DateTime.Parse("2014-09-30 23:09:16.013"), "c94ec031-6ceb-472a-b07c-2bbf1d554c0f");
		}
	}
}
