using System;
using System.Collections.Generic;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Tests
{
	class JPCustomsPluginTest : SqlBillingTransactionsPluginTest<Plugins.JPCustoms.Plugin>
	{
		protected override string[] QueryColumns
		{
			get { return new[] { "ClientID", "MBOL", "HBOL", "MessageType", "MessageTrackingID", "AM_ReceivedFromSenderUTC", "AM_ArchivedUTC" }; }
		}

		protected override IReadOnlyList<object[]> QueryResult
		{
			get
			{
				return new List<object[]>
				{
					new object[] {"AGSSINAGS", "MOLU13301936012", "J03BS01710154", "AHR9", new Guid("8e730d02-62b5-4c76-bd66-9693206b6512"), DateTime.Parse("2014-10-24 11:31:33.013"), DateTime.Parse("2014-10-24 11:32:33.013")},
					new object[] {"CDSC05HKG", "KKLUXM06071900", "J01KXMN140000812", "CHR2", new Guid("1a86b93e-5cd0-4ef2-a732-73298ca00486"), DateTime.Parse("2014-10-22 22:53:37.217"), DateTime.Parse("2014-10-22 22:54:37.217")},
				};
			}
		}

		protected override void AssertTransactions(TimeStampedTransaction[] transactions)
		{
			Assert.That(transactions.Length, Is.EqualTo(4));
			AssertTransaction(transactions[0], DateTime.Parse("2014-10-24 11:32:33.013"), 1, "AGSSINAGS", null, null, "JPC", "JPC", "8e730d02-62b5-4c76-bd66-9693206b6512", null, null, null, null, "HUB", DateTime.Parse("2014-10-24 11:31:33.013"), "8e730d02-62b5-4c76-bd66-9693206b6512");
			AssertTransaction(transactions[1], DateTime.Parse("2014-10-24 11:32:33.013"), 1, "AGSSINAGS", null, null, "JPC", "AFR", "MOLU13301936012", "J03BS01710154", "AHR9", "8e730d02-62b5-4c76-bd66-9693206b6512", null, "HUB", DateTime.Parse("2014-10-24 11:31:33.013"), "8e730d02-62b5-4c76-bd66-9693206b6512");
			AssertTransaction(transactions[2], DateTime.Parse("2014-10-22 22:54:37.217"), 1, "CDSC05HKG", null, null, "JPC", "JPC", "1a86b93e-5cd0-4ef2-a732-73298ca00486", null, null, null, null, "HUB", DateTime.Parse("2014-10-22 22:53:37.217"), "1a86b93e-5cd0-4ef2-a732-73298ca00486");
			AssertTransaction(transactions[3], DateTime.Parse("2014-10-22 22:54:37.217"), 1, "CDSC05HKG", null, null, "JPC", "AFR", "KKLUXM06071900", "J01KXMN140000812", "CHR2", "1a86b93e-5cd0-4ef2-a732-73298ca00486", null, "HUB", DateTime.Parse("2014-10-22 22:53:37.217"), "1a86b93e-5cd0-4ef2-a732-73298ca00486");
		}
	}
}
