using System.Collections.Generic;
using System.Data;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.E2ELegacy
{
	public class Plugin : SqlBillingTransactionsPlugin
	{
		protected override string QueryName
		{
			get { return "CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.E2ELegacy.Query.sql"; }
		}

		protected override IEnumerable<TimeStampedTransaction> CreateTransactions(IDataRecord record)
		{
			var timeStamp = GetDateTime(record, "AM_ArchivedUTC");
			var messageTrackingID = GetGuid(record, "MessageTrackingID").ToString();

			yield return new TimeStampedTransaction(timeStamp, new BillingTransaction
			{
				Category = "E2E",
				PriceItemCode = "E2E",
				BillableCount = 1,
				ReportingSource = "HUB",
				ServiceOccuredUTC = GetDateTime(record, "AM_SentToRecipientUTC"),
				ClientID = GetString(record, "ReceiverID"),
				MessageTrackingID = messageTrackingID,
				Reference1 = GetString(record, "SenderID"),
				Reference2 = GetString(record, "JobNumber"),
			});
		}
	}
}
