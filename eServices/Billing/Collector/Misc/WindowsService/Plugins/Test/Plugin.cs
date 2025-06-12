using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.API;
 using CargoWise.Billing.CollectorService.Plugin;
 using CargoWise.eServices.Billing.Collector.WindowsService.Common;
using Microsoft.Extensions.Logging;

namespace CargoWise.eServices.Billing.Collector.Misc.WindowsService.Plugins.Test
{
	public class Plugin : AbstractPlugin
	{
		protected override ILogger CreateLogger() => string.IsNullOrEmpty(LoggerName) ? base.CreateLogger() : loggerFactory?.CreateLogger(LoggerName);
		public override IEnumerable<TimeStampedTransaction> GetTransactions(DateTime start, DateTime end)
		{
			yield return new TimeStampedTransaction(end, new BillingTransaction
			{
				BillableCount = 1,
				ClientID = "TSTTSTTST",
				Category = "TST",
				PriceItemCode = "TST",
				Reference1 = "TEST",
				Reference2 = "TEST",
				Reference3 = "TEST",
				Reference4 = "TEST",
				ReportingSource = "MSC",
				ServiceOccuredUTC = start,
				MessageTrackingID = "55B2D0DA-8230-43BE-83BF-5C7D5766343E",
			});
		}

		public override void UpdateSettings(PluginSettings settings)
		{
			LoggerName = ValueOrEmptyString(settings?.Parameters?.FirstOrDefault(p => p.Name == "LoggerName"));
		}

		string LoggerName { get; set; }
	}
}
