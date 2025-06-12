using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;

namespace CargoWise.eServices.Billing.Collector.NET.ElasticSearch.WorkerService.Plugins.Test
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
				ReportingSource = "MSC",
				ServiceOccuredUTC = start,
				MessageTrackingID = "C3D5AA7A-0FCA-42C5-A164-28757C25D0FA",
			});
		}

		public override void UpdateSettings(PluginSettings settings)
		{
		}
	}
}
