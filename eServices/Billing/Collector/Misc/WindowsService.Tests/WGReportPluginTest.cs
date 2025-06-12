using System;
using System.Collections.Generic;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.Misc.WindowsService.Tests
{
	class WGReportPluginTest : AbstracteHubAuditRequestPluginTest<Plugins.WGReport.Plugin>
	{
		protected override bool SendUsageTransaction => true;

		protected override string[] QueryColumns
		{
			get { return new[] { "IncrementId", "RecordedTime", "ServerName", "DatabaseName", "UsageType", "CPUUsageSeconds", "SampleSeconds", "SourceOfQuery" }; }
		}
		protected override IReadOnlyList<object[]> QueryResult
		{
			get
			{
				return new List<object[]>()
				{
					new object[] { "6779", DateTime.Parse("2021-08-13 18:33:00.000"), "LONWP-SSQL-4A.wisecloud.zone/INSTANCEP1", "SEIHAM", "ClientDirect", 8788, "900", "Client" },
					new object[] { "7000", DateTime.Parse("2021-08-13 18:33:20.000"), "LONWP-SSQL-4A.wisecloud.zone/INSTANCEP1", "TOCIND", "ClientDirect", 0, "280", "Client" },
				};
			}
		}

		protected override void AssertTransactions(TimeStampedTransaction[] transactions)
		{
			Assert.That(transactions.Length, Is.EqualTo(2));
			AssertTransaction(transactions[0], "WGR", "WGR", "SEI???HAM", "900", "ClientDirect", "Client", "LONWP-SSQL-4A.wisecloud.zone/INSTANCEP1", DateTime.Parse("2021-08-13 18:33:00"), 8788, "6779");
			AssertTransaction(transactions[1], "WGR", "WGR", "TOC???IND", "280", "ClientDirect", "Client", "LONWP-SSQL-4A.wisecloud.zone/INSTANCEP1", DateTime.Parse("2021-08-13 18:33:20"), 0, "7000");
		}

		protected override void AssertUsageTransactions(TimeStampedTransaction[] transactions)
		{
			Assert.That(transactions.Length, Is.EqualTo(2));
			AssertUsageTransaction(transactions[0], 8788, "WGR", "SEI", null, "HAM", null, null, null, $@"{{
	""Category"": ""WGR"",
	""Reference1"": ""900"",
	""Reference2"": ""ClientDirect"",
	""Reference3"": ""Client"",
	""Reference4"": ""LONWP-SSQL-4A.wisecloud.zone/INSTANCEP1"",
	""Reference5"": ""6779"",
	""ReportingSource"": ""MSC"",
	""Version"": 1,
}}", DateTime.Parse("2021-08-13 18:33:00"));
			AssertUsageTransaction(transactions[1], 0, "WGR", "TOC", null, "IND", null, null, null, $@"{{
	""Category"": ""WGR"",
	""Reference1"": ""280"",
	""Reference2"": ""ClientDirect"",
	""Reference3"": ""Client"",
	""Reference4"": ""LONWP-SSQL-4A.wisecloud.zone/INSTANCEP1"",
	""Reference5"": ""7000"",
	""ReportingSource"": ""MSC"",
	""Version"": 1,
}}", DateTime.Parse("2021-08-13 18:33:20"));
		}

		protected override void AssertQuery(string query)
		{
			base.AssertQuery(query);
			Assert.That(query.IndexOf("WHERE CPUUsageSeconds >= 0", StringComparison.Ordinal), Is.GreaterThanOrEqualTo(0));
		}
	}
}
