using System.Collections.Generic;
using System.Data;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;

namespace CargoWise.eServices.Billing.Collector.Misc.WindowsService.Plugins.WGReport
{
	public class Plugin : SqlBillingTransactionsPlugin
	{
		protected override sealed string QueryName
		{
			get { return "CargoWise.eServices.Billing.Collector.Misc.WindowsService.Plugins.WGReport.Query.sql"; }
		}

		protected override sealed IEnumerable<TimeStampedTransaction> CreateTransactions(IDataRecord record)
		{
			var incrementId = GetString(record, "IncrementId");
			var recordedTime = GetDateTime(record, "RecordedTime");
			var serverName = GetString(record, "ServerName");
			var databaseName = GetString(record, "DatabaseName");
			var usageType = GetString(record, "UsageType");
			var cpuUsage = GetInt(record, "CPUUsageSeconds");
			var sampleSeconds = GetString(record, "SampleSeconds");
			var sourceOfQuery = GetString(record, "SourceOfQuery");

			yield return new TimeStampedTransaction(recordedTime, new BillingTransaction
			{
				BillableCount = cpuUsage,
				Category = "WGR",
				PriceItemCode = "WGR",
				ClientID = databaseName.Insert(3, "???"),
				Reference1 = sampleSeconds,
				Reference2 = usageType,
				Reference3 = sourceOfQuery,
				Reference4 = serverName,
				Reference5 = incrementId,
				ReportingSource = "MSC",
				ServiceOccuredUTC = recordedTime,
				Version = 1,
			}, new UsageTransaction
			{
				UsageCode = "WGR",
				EnterpriseCode = databaseName.Substring(0, 3),
				ServerCode = databaseName.Substring(3),
				ServiceOccuredUTC = recordedTime,
				UsageCount = cpuUsage,
				AdditionalRefs = $@"{{
	""Category"": ""WGR"",
	""Reference1"": ""{sampleSeconds}"",
	""Reference2"": ""{usageType}"",
	""Reference3"": ""{sourceOfQuery}"",
	""Reference4"": ""{serverName}"",
	""Reference5"": ""{incrementId}"",
	""ReportingSource"": ""MSC"",
	""Version"": 1,
}}"
			});
		}
	}
}
