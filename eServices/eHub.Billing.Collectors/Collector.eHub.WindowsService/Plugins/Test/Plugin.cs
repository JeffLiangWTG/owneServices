using System;
using System.Collections.Generic;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.Test
{
	public class Plugin : AbstractPlugin
	{
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
				ReportingSource = "HUB",
				ServiceOccuredUTC = start,
				MessageTrackingID = "55B2D0DA-8230-43BE-83BF-5C7D5766343E",
			});
		}

		public override void UpdateSettings(PluginSettings settings)
		{
		}
	}
}
