using System;
using System.Collections.Generic;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Tests.E2ELegacy
{
	class E2ELegacyPluginTest : SqlBillingTransactionsPluginTest<Plugins.E2ELegacy.Plugin>
	{
		protected override string[] QueryColumns
		{
			get { return new[] { "SenderID", "ReceiverID", "MessageTrackingID", "AM_SentToRecipientUTC", "AM_ArchivedUTC", "JobNumber" }; }
		}

		protected override IReadOnlyList<object[]> QueryResult
		{
			get
			{
				return new List<object[]>
				{
					new object[] { "TESTSENDUAT", "TESTRECVUAT", Guid.Parse("7B0BEC98-741A-4C3D-8FB5-420C49196347"), DateTime.Parse("2019-11-27 18:45:51.230"), DateTime.Parse("2019-11-27 19:45:51.230"), "CartageJobB00158769/I" },
					new object[] { "TESTSENDUAT", "TESTRECVUAT", Guid.Parse("7B0BEC98-741A-4C3D-8FB5-420C49196348"), DateTime.Parse("2019-11-27 18:45:51.230"), DateTime.Parse("2019-11-27 19:45:51.230"), "DocumentMessageT00033387" },
					new object[] { "TESTSENDUAT", "TESTRECVUAT", Guid.Parse("7B0BEC98-741A-4C3D-8FB5-420C49196349"), DateTime.Parse("2019-11-27 18:45:51.230"), DateTime.Parse("2019-11-27 19:45:51.230"), "Order7830630053211~69~MKCORP" }
				};
			}
		}

		protected override void AssertTransactions(TimeStampedTransaction[] transactions)
		{
			Assert.That(transactions.Length, Is.EqualTo(QueryResult.Count));
			AssertTransaction(transactions[0], DateTime.Parse("2019-11-27 19:45:51.230"), 1, "TESTRECVUAT", null, null, "E2E", "E2E", "TESTSENDUAT", "CartageJobB00158769/I", null, null, null, "HUB", DateTime.Parse("2019-11-27 18:45:51.230"), "7b0bec98-741a-4c3d-8fb5-420c49196347");
			AssertTransaction(transactions[1], DateTime.Parse("2019-11-27 19:45:51.230"), 1, "TESTRECVUAT", null, null, "E2E", "E2E", "TESTSENDUAT", "DocumentMessageT00033387", null, null, null, "HUB", DateTime.Parse("2019-11-27 18:45:51.230"), "7b0bec98-741a-4c3d-8fb5-420c49196348");
			AssertTransaction(transactions[2], DateTime.Parse("2019-11-27 19:45:51.230"), 1, "TESTRECVUAT", null, null, "E2E", "E2E", "TESTSENDUAT", "Order7830630053211~69~MKCORP", null, null, null, "HUB", DateTime.Parse("2019-11-27 18:45:51.230"), "7b0bec98-741a-4c3d-8fb5-420c49196349");
		}
	}
}
