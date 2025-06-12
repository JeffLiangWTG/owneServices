using System.Collections.Generic;
using System.Data;
using CargoWise.eServices.Billing.API;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.PortMessaging
{
	public class Plugin : SqlBillingTransactionsPlugin
	{
		protected override sealed string QueryName
		{
			get { return "CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.PortMessaging.Query.sql"; }
		}

		protected override IEnumerable<TimeStampedTransaction> CreateTransactions(IDataRecord record)
		{
			var timeStamp = GetDateTime(record, "AM_ArchivedUTC");
			var messageTrackingID = GetGuid(record, "MessageTrackingID").ToString();

			yield return new TimeStampedTransaction(timeStamp, new BillingTransaction
			{
				BillableCount = 1,
				ReportingSource = "HUB",
				ClientID = GetString(record, "ClientID"),
				Reference1 = GetString(record, "Consol"),
				Reference2 = GetString(record, "Shipment", true),
				Reference4 = messageTrackingID,
				ServiceOccuredUTC = GetDateTime(record, "AM_ReceivedFromSenderUTC"),
				Category = "PMG",
				PriceItemCode = GetPriceItemCode(GetString(record, "MessagePurpose")),
				MessageTrackingID = messageTrackingID,
			});
		}

		static string GetPriceItemCode(string messagePurpose)
		{
			switch (messagePurpose)
			{
				case "HDS":
					return "PM1";
				default:
					return "PM2";
			}
		}
	}
}