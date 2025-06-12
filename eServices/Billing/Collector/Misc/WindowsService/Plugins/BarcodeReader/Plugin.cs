using System.Collections.Generic;
using System.Data;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;

namespace CargoWise.eServices.Billing.Collector.Misc.WindowsService.Plugins.BarcodeReader
{
	public class Plugin : SqlBillingTransactionsPlugin
	{
		protected override sealed string QueryName
		{
			get { return "CargoWise.eServices.Billing.Collector.Misc.WindowsService.Plugins.BarcodeReader.Query.sql"; }
		}

		protected override sealed IEnumerable<TimeStampedTransaction> CreateTransactions(IDataRecord record)
		{
			var requestUTC = GetDateTime(record, "B0_RequestUTC");
			yield return new TimeStampedTransaction(requestUTC, new BillingTransaction
			{
				BillableCount = 1,
				Category = "BRD",
				PriceItemCode = "BRD",
				ClientID = GetString(record, "B0_LicenceCode").Replace("-", string.Empty),
				Reference1 = GetString(record, "B0_TransactionSubType"),
				Reference2 = GetString(record, "B0_RequestIP"),
				Reference3 = GetGuid(record, "B0_TransactionIdentifier").ToString(),
				Reference4 = GetString(record, "B0_UserName"),
				ReportingSource = "MSC",
				ServiceOccuredUTC = requestUTC,
				Version = 1,
			});
		}
	}
}
