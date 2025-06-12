using System.Collections.Generic;
using System.Data;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ForwardingPortMessaging.TNPA
{
	public class Plugin : SqlBillingTransactionsPlugin
	{
		protected override string QueryName
		{
			get { return "CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ForwardingPortMessaging.TNPA.Query.sql"; }
		}

		protected override IEnumerable<TimeStampedTransaction> CreateTransactions(IDataRecord record)
		{
			var timeStamp = GetDateTime(record, "AM_ArchivedUTC");
			var messageTrackingID = GetGuid(record, "MessageTrackingID").ToString();
			var priceItemCode = GetString(record, "PriceItemCode");
			var dataVersion = GetString(record, "DataVersion");
			var tempReference3 = GetString(record, "Reference3");
			var reference3 = GetReference3(tempReference3, dataVersion, priceItemCode);

			yield return new TimeStampedTransaction(timeStamp, new BillingTransaction
			{
				BillableCount = 1,
				ReportingSource = "HUB",
				ClientID = GetString(record, "ClientID"),
				Reference1 = GetString(record, "Reference1"),
				Reference2 = GetString(record, "Reference2", true),
				Reference3 = reference3,
				Reference4 = GetString(record, "Reference4", true),
				ServiceOccuredUTC = GetDateTime(record, "AM_ReceivedFromSenderUTC"),
				Category = "PMG",
				PriceItemCode = priceItemCode,
				MessageTrackingID = messageTrackingID,
			});
		}

		static string GetReference3(string tempReference3, string dataVersion, string priceItemCode)
		{
			var result = tempReference3;

			if (priceItemCode != "POZ") return result;

			if (tempReference3 != "WTH")
			{
				result = dataVersion == "1" ? "Original" : "Amendment";
			}
			else
			{
				result = "Withdrawal";
			}
			
			return result;
		}
	}
}
