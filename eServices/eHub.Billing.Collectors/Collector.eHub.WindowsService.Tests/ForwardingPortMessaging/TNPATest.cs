using System;
using System.Collections.Generic;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Tests.ForwardingPortMessaging
{
	class TNPATest : SqlBillingTransactionsPluginTest<Plugins.ForwardingPortMessaging.TNPA.Plugin>
	{
		protected override string[] QueryColumns
		{
			get { return new[] { "ClientID", "PriceItemCode", "Reference1", "Reference2", "Reference3", "Reference4", "DataVersion", "MessageTrackingID", "AM_ReceivedFromSenderUTC", "AM_ArchivedUTC" }; }
		}

		protected override IReadOnlyList<object[]> QueryResult => new List<object[]>
		{
			new object[] { "TSTCLIENT", "POZ", "C00679358", "Cargo Dues - Export", "APP","", "1", Guid.Parse("790be798-2f09-4ba2-bc9a-458648472b6b"), DateTime.Parse("2015-01-11 09:31:00.000"), DateTime.Parse("2015-01-11 09:35:00.000")},
			new object[] { "TSTCLIENT", "POZ", "C00679358", "Cargo Dues - Export", "APP","", "2", Guid.Parse("790be798-2f09-4ba2-bc9a-458648472b6b"), DateTime.Parse("2015-01-11 09:31:00.000"), DateTime.Parse("2015-01-11 09:35:00.000")},
			new object[] { "TSTCLIENT", "POZ", "C00679358", "Cargo Dues - Export", "WTH","", "1", Guid.Parse("790be798-2f09-4ba2-bc9a-458648472b6b"), DateTime.Parse("2015-01-11 09:31:00.000"), DateTime.Parse("2015-01-11 09:35:00.000")},
			new object[] { "TSTCLIENT", "PSZ", "C00679036", "Cargo Dues - Export", "MAA", "3705518950", "", Guid.Parse("790be798-2f09-4ba2-bc9a-458648472b6b"), DateTime.Parse("2015-01-11 09:31:00.000"), DateTime.Parse("2015-01-11 09:35:00.000")},
			new object[] { "TSTCLIENT", "PSZ", "C00679036", "Cargo Dues - Export", "MWA", "3705518950", "", Guid.Parse("790be798-2f09-4ba2-bc9a-458648472b6b"), DateTime.Parse("2015-01-11 09:31:00.000"), DateTime.Parse("2015-01-11 09:35:00.000")},
			new object[] { "TSTCLIENT", "PSZ", "C00679036", "Cargo Dues - Export", "STU", "3705518950", "", Guid.Parse("790be798-2f09-4ba2-bc9a-458648472b6b"), DateTime.Parse("2015-01-11 09:31:00.000"), DateTime.Parse("2015-01-11 09:35:00.000")}
		};

		protected override void AssertTransactions(TimeStampedTransaction[] transactions)
		{
			Assert.That(transactions.Length, Is.EqualTo(QueryResult.Count));
			AssertTransaction(transactions[0], DateTime.Parse("2015-01-11 09:35:00.000"), 1, "TSTCLIENT", null, null, "PMG", "POZ", "C00679358", "Cargo Dues - Export", "Original", "", null, "HUB", DateTime.Parse("2015-01-11 09:31:00.000"), "790be798-2f09-4ba2-bc9a-458648472b6b");
			AssertTransaction(transactions[1], DateTime.Parse("2015-01-11 09:35:00.000"), 1, "TSTCLIENT", null, null, "PMG", "POZ", "C00679358", "Cargo Dues - Export", "Amendment", "", null, "HUB", DateTime.Parse("2015-01-11 09:31:00.000"), "790be798-2f09-4ba2-bc9a-458648472b6b");
			AssertTransaction(transactions[2], DateTime.Parse("2015-01-11 09:35:00.000"), 1, "TSTCLIENT", null, null, "PMG", "POZ", "C00679358", "Cargo Dues - Export", "Withdrawal", "", null, "HUB", DateTime.Parse("2015-01-11 09:31:00.000"), "790be798-2f09-4ba2-bc9a-458648472b6b");
			AssertTransaction(transactions[3], DateTime.Parse("2015-01-11 09:35:00.000"), 1, "TSTCLIENT", null, null, "PMG", "PSZ", "C00679036", "Cargo Dues - Export", "MAA", "3705518950", null, "HUB", DateTime.Parse("2015-01-11 09:31:00.000"), "790be798-2f09-4ba2-bc9a-458648472b6b");
			AssertTransaction(transactions[4], DateTime.Parse("2015-01-11 09:35:00.000"), 1, "TSTCLIENT", null, null, "PMG", "PSZ", "C00679036", "Cargo Dues - Export", "MWA", "3705518950", null, "HUB", DateTime.Parse("2015-01-11 09:31:00.000"), "790be798-2f09-4ba2-bc9a-458648472b6b");
			AssertTransaction(transactions[5], DateTime.Parse("2015-01-11 09:35:00.000"), 1, "TSTCLIENT", null, null, "PMG", "PSZ", "C00679036", "Cargo Dues - Export", "STU", "3705518950", null, "HUB", DateTime.Parse("2015-01-11 09:31:00.000"), "790be798-2f09-4ba2-bc9a-458648472b6b");
		}
	}
}
