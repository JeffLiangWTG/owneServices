using System.Collections.Generic;
using System.Data;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.JPCustoms
{
	public class Plugin : SqlBillingTransactionsPlugin
	{
		protected override sealed string QueryName
		{
			get { return "CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.JPCustoms.Query.sql"; }
		}

		protected override sealed IEnumerable<TimeStampedTransaction> CreateTransactions(IDataRecord record)
		{
			var clientID = GetString(record, "ClientID");
			var messageTrackingID = GetGuid(record, "MessageTrackingID").ToString();
			var receivedFromSenderUTC = GetDateTime(record, "AM_ReceivedFromSenderUTC");
			var timeStamp = GetDateTime(record, "AM_ArchivedUTC");

			yield return new TimeStampedTransaction(timeStamp, new BillingTransaction
			{
				BillableCount = 1,
				ReportingSource = "HUB",
				ClientID = clientID,
				Reference1 = messageTrackingID,
				ServiceOccuredUTC = receivedFromSenderUTC,
				Category = "JPC",
				PriceItemCode = "JPC",
				MessageTrackingID = messageTrackingID,
			});

			yield return new TimeStampedTransaction(timeStamp, new BillingTransaction
			{
				BillableCount = 1,
				ReportingSource = "HUB",
				ClientID = clientID,
				Reference1 = GetString(record, "MBOL"),
				Reference2 = GetString(record, "HBOL"),
				Reference3 = GetString(record, "MessageType"),
				Reference4 = messageTrackingID,
				ServiceOccuredUTC = receivedFromSenderUTC,
				Category = "JPC",
				PriceItemCode = "AFR",
				MessageTrackingID = messageTrackingID,
			});
		}
	}
}
