using System;
using System.Collections.Generic;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Tests
{
	class RailincMessagingTest : SqlBillingTransactionsPluginTest<Plugins.RailincMessaging.Plugin>
	{
		protected override string[] QueryColumns
		{
			get { return new[] {"ClientID", "MsgType", "Consol", "Container", "AM_ReceivedFromSenderUTC", "MessageTrackingID", "EventType", "IsEstimate", "Location", "AM_ArchivedUTC" }; }
		}

		protected override IReadOnlyList<object[]> QueryResult
		{
			get
			{
				return new List<object[]>
				{
					new object[] {"MA3SGFSGF", "CLM", "C00039846", "IMTU9055778", DateTime.Parse("2014-09-30 23:09:16.013"), new Guid("b5950447-5af0-422d-8255-0f184bd917b2"), "ARV", "TRUE", "RIVERS MB", DateTime.Parse("2014-09-30 23:10:16.013")},
					new object[] {"MA3SGFSGF", "CLM", "C00039846", "IMTU9055778", DateTime.Parse("2014-09-30 23:09:16.013"), new Guid("b5950447-5af0-422d-8255-0f184bd917b2"), "ARV", "null", "RIVERS MB", DateTime.Parse("2014-09-30 23:10:16.013")},
				};
			}
		}

		protected override void AssertTransactions(TimeStampedTransaction[] transactions)
		{
			Assert.That(transactions.Length, Is.EqualTo(2));
			AssertTransaction(transactions[0], DateTime.Parse("2014-09-30 23:10:16.013"), 1, "MA3SGFSGF", null, null, "RIM", "RIC", "IMTU9055778", "C00039846", "CLM", "b5950447-5af0-422d-8255-0f184bd917b2", "ARV [E]-RIVERS MB", "HUB", DateTime.Parse("2014-09-30 23:09:16.013"), "b5950447-5af0-422d-8255-0f184bd917b2");
			AssertTransaction(transactions[1], DateTime.Parse("2014-09-30 23:10:16.013"), 1, "MA3SGFSGF", null, null, "RIM", "RIC", "IMTU9055778", "C00039846", "CLM", "b5950447-5af0-422d-8255-0f184bd917b2", "ARV [A]-RIVERS MB", "HUB", DateTime.Parse("2014-09-30 23:09:16.013"), "b5950447-5af0-422d-8255-0f184bd917b2");
		}
	}
}
