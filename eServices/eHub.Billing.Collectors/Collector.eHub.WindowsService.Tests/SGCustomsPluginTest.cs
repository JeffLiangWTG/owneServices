using System;
using System.Collections.Generic;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Tests
{
	class SGCustomsPluginTest : SqlBillingTransactionsPluginTest<Plugins.SGCustoms.Plugin>
	{
		protected override string[] QueryColumns
		{
			get { return new[] { "ClientID", "Reference1", "Reference2", "Reference3", "ActionPurpose", "Branch", "ClientStaffCode", "AM_ReceivedFromSenderUTC", "AM_ArchivedUTC", "MessageTrackingID" }; }
		}

		protected override IReadOnlyList<object[]> QueryResult
		{
			get
			{
				return new List<object[]>
				{
					new object[] { "UPESINPRD", "MAN0000001", "61849405436", "30A8R7VT3F3", "AED", "PRE", "TKW", DateTime.Parse("2014-10-20 11:31:33.013"), DateTime.Parse("2014-10-20 11:32:33.013"), new Guid("acc5e337-af74-473e-b367-7c91341ae7cd") },
					new object[] { "UPESINPRD", "MAN0000002", "71849405437", "40A8R7VT3F3", "PCM", "PRE", "SMS", DateTime.Parse("2014-10-21 11:31:33.013"), DateTime.Parse("2014-10-21 11:32:33.013"), new Guid("1dce09cd-1d59-4a47-8b37-cd88b288ac17") },
					new object[] { "UPESINPRD", "MAN0000003", "81849405437", "50A8R7VT3F3", "PCM", "PRE", "SMS", DateTime.Parse("2014-10-22 11:31:33.013"), DateTime.Parse("2014-10-22 11:32:33.013"), new Guid("2dce09cd-1d59-4a47-8b37-cd88b288ac17") }
				};
			}
		}

		protected override void AssertTransactions(TimeStampedTransaction[] transactions)
		{
			Assert.That(transactions.Length, Is.EqualTo(3));
			AssertTransaction(transactions[0], DateTime.Parse("2014-10-20 11:32:33.013"), 1, "UPESINPRD", null, "TKW", "SAC", "SAE", "MAN0000001", "61849405436", "30A8R7VT3F3", null, null, "HUB", DateTime.Parse("2014-10-20 11:31:33.013"), "acc5e337-af74-473e-b367-7c91341ae7cd");
			AssertTransaction(transactions[1], DateTime.Parse("2014-10-21 11:32:33.013"), 1, "UPESINPRD", null, "SMS", "SAC", "SAI", "MAN0000002", "71849405437", "40A8R7VT3F3", null, null, "HUB", DateTime.Parse("2014-10-21 11:31:33.013"), "1dce09cd-1d59-4a47-8b37-cd88b288ac17");
			AssertTransaction(transactions[2], DateTime.Parse("2014-10-22 11:32:33.013"), 1, "UPESINPRD", null, "SMS", "SAC", "SAI", "MAN0000003", "81849405437", "50A8R7VT3F3", null, null, "HUB", DateTime.Parse("2014-10-22 11:31:33.013"), "2dce09cd-1d59-4a47-8b37-cd88b288ac17");
		}
	}
}
