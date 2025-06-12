using System.Collections.Generic;
using System.Data;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ForwardingPortMessaging.DAKOSY
{
	public class Plugin : SqlBillingTransactionsPlugin
	{
		protected override sealed string QueryName
		{
			get { return "CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ForwardingPortMessaging.DAKOSY.Query.sql"; }
		}

		protected override IEnumerable<TimeStampedTransaction> CreateTransactions(IDataRecord record)
		{
			var timeStamp = GetDateTime(record, "AM_ArchivedUTC");
			var messageTrackingID = GetGuid(record, "MessageTrackingID").ToString();
			var priceItemCode = GetPriceItemCode(GetString(record, "MessagePurpose", true));

			yield return new TimeStampedTransaction(timeStamp, new BillingTransaction
			{
				BillableCount = 1,
				ReportingSource = "HUB",
				ClientID = GetString(record, "ClientID"),
				Reference1 = GetString(record, "Reference1"),
				Reference2 = GetString(record, "Reference2", true),
				Reference4 = messageTrackingID,
				Reference5 = GetString(record, "ServiceProvider"),
				ServiceOccuredUTC = GetDateTime(record, "AM_ReceivedFromSenderUTC"),
				Category = "PMG",
				PriceItemCode = priceItemCode,
				MessageTrackingID = messageTrackingID,
				Version = 1
			});
		}

		static string GetPriceItemCode(string messagePurpose)
		{
			if (messagePurpose == "PM3")
			{
				return "PM3";
			}
			else if (messagePurpose == "HDS")
			{
				return "PM1";
			}
			else
			{
				return "PM2";
			}
		}
	}
}
