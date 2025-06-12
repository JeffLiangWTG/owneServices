using System.Collections.Generic;
using System.Data;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ForwardingPortMessaging.PortBase
{
	public class Plugin : SqlBillingTransactionsPlugin
	{
		protected override sealed string QueryName
		{
			get { return "CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ForwardingPortMessaging.PortBase.Query.sql"; }
		}

		protected override IEnumerable<TimeStampedTransaction> CreateTransactions(IDataRecord record)
		{
			var timeStamp = GetDateTime(record, "AM_ArchivedUTC");
			var priceItemCode = GetString(record, "PriceItemCode");
			var tempRef4 = GetString(record, "Reference4", expectNull: priceItemCode == "PMN" || priceItemCode == "PSN");

			yield return new TimeStampedTransaction(timeStamp, new BillingTransaction
			{
				BillableCount = 1,
				ReportingSource = "HUB",
				ClientID = GetString(record, "ClientID"),
				Reference1 = GetString(record, "Reference1"),
				Reference2 = GetString(record, "Reference2", expectNull: priceItemCode == "PMN"),
				Reference3 = GetString(record, "Reference3", true),
				Reference4 = GetReference4(tempRef4, priceItemCode),
				ServiceOccuredUTC = GetDateTime(record, "AM_ReceivedFromSenderUTC"),
				Category = "PMG",
				PriceItemCode = priceItemCode,
				MessageTrackingID = GetGuid(record, "MessageTrackingID").ToString(),
				Version = 1
			});
		}

		protected string GetReference4(string tempRef4, string priceItemCode)
		{
			var reference4 = tempRef4;

			if (priceItemCode == "PSN")
			{
				if (tempRef4 != null)
				{
					reference4 = "STU(" + tempRef4 + ")";
				}
				else
				{
					reference4 = "STU";
				}
			}

			return reference4;
		}
	}
}
