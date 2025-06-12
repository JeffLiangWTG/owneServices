using System.Collections.Generic;
using System.Data;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.AirMessaging
{
	public class Plugin : SqlBillingTransactionsPlugin
	{
		protected override sealed string QueryName
		{
			get { return "CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.AirMessaging.Query.sql"; }
		}

		protected override sealed IEnumerable<TimeStampedTransaction> CreateTransactions(IDataRecord record)
		{
			var clientID = GetString(record, "ClientID");
			var network = FormatNetwork(GetString(record, "Network"));
			var receivedFromSenderUTC = GetDateTime(record, "AM_ReceivedFromSenderUTC");
			var timeStamp = GetDateTime(record, "AM_ArchivedUTC");
			var messageTrackingID = GetGuid(record, "MessageTrackingID").ToString();

			yield return new TimeStampedTransaction(timeStamp, new BillingTransaction
			{
				BillableCount = 1,
				ReportingSource = "HUB",
				ClientID = clientID,
				Reference1 = messageTrackingID,
				Reference2 = network,
				ServiceOccuredUTC = receivedFromSenderUTC,
				Category = "AMG",
				PriceItemCode = "ALM",
				MessageTrackingID = messageTrackingID,
			});
			yield return new TimeStampedTransaction(timeStamp, new BillingTransaction
			{
				BillableCount = 1,
				ReportingSource = "HUB",
				ClientID = clientID,
				Reference1 = GetString(record, "MAWB"),
				Reference2 = GetString(record, "HAWB", true),
				Reference3 = GetString(record, "AirlineCodeOrPrefix"),
				Reference4 = network,
				Reference5 = GetString(record, "StatusCode", true),
				ServiceOccuredUTC = receivedFromSenderUTC,
				Category = "AMG",
				PriceItemCode = GetString(record, "MsgType"),
			});
		}

		static string FormatNetwork(string network)
		{
			switch (network)
			{
				case "Traxon_EDP":
					return "TraxonEDP";
				case "Traxon_RCF":
					return "TraxonRCF";
				case null:
					return null;
				default:
					var underscoreIndex = network.IndexOf('_');
					return underscoreIndex >= 0 ? network.Substring(0, underscoreIndex) : network;
			}
		}
	}
}
