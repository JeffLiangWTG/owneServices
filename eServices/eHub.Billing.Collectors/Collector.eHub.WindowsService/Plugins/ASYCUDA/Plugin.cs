using System.Collections.Generic;
using System.Data;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ASYCUDA
{
	public class Plugin : SqlBillingTransactionsPlugin
	{
		protected override sealed string QueryName
		{
			get { return "CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ASYCUDA.Query.sql"; }
		}

		protected override IEnumerable<TimeStampedTransaction> CreateTransactions(IDataRecord record)
		{
			var messageTrackingID = GetGuid(record, "MessageTrackingID").ToString();

			yield return new TimeStampedTransaction(GetDateTime(record, "AM_ArchivedUTC"), new BillingTransaction
			{
				BillableCount = 1,
				ReportingSource = "HUB",
				ClientID = GetString(record, "ClientID"),
				Reference1 = messageTrackingID,
				Reference2 = GetString(record, "Consol"),
				Reference3 = GetString(record, "CountryCode"),
				ServiceOccuredUTC = GetDateTime(record, "AM_ReceivedFromSenderUTC"),
				Category = "ASC",
				PriceItemCode = "ASC",
				MessageTrackingID = messageTrackingID,
			});
		}
	}
}
