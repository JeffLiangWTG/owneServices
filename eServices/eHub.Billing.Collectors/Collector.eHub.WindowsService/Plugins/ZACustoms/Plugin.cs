using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ZACustoms
{
	public class Plugin : SqlBillingTransactionsPlugin
	{
		protected override sealed string QueryName
		{
			get { return "CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ZACustoms.Query.sql"; }
		}

		protected override sealed IEnumerable<TimeStampedTransaction> CreateTransactions(IDataRecord record)
		{
			var clientID = GetString(record, "ClientID");
			var messageTrackingID = GetGuid(record, "MessageTrackingID").ToString();
			var receivedFromSenderUTC = GetDateTime(record, "AM_ReceivedFromSenderUTC");
			var timeStamp = GetDateTime(record, "AM_ArchivedUTC");
			var messageName = GetString(record, "MessageName");
			var priceItemCode = GetPriceItemCode(messageName);

			// Reference field order is determined by putting unique identifying fields first,
			// and then common fields in the same position (if possible).
			// Since BillDate is a key field for ZX3, but not ZX1, it position cannot be same for both.
			var tx = new BillingTransaction
			{
				BillableCount = 1,
				ReportingSource = "HUB",
				ClientID = clientID,
				ServiceOccuredUTC = receivedFromSenderUTC,
				Category = "ZAC",
				PriceItemCode = priceItemCode,
				MessageTrackingID = messageTrackingID,
				Reference1 = GetString(record, "CarrierCode"),
				Reference3 = GetString(record, "FlightVoyage"),
				Reference4 = GetString(record, "JobNumber"),
			};

			switch (priceItemCode)
			{
				case "ZX1":
					tx.Reference2 = GetString(record, "MasterBillNumber");
					tx.Reference5 = GetString(record, "BillDate");
					break;
				case "ZX3":
					tx.Reference2 = GetString(record, "BillDate");
					break;
			}

			yield return new TimeStampedTransaction(timeStamp, tx);
		}

		static string GetPriceItemCode(string messageName)
		{
			switch (messageName)
			{
				case "COH":
				case "HAB":
				case "AQM":
				case "ALM":
				case "ALH":
					return "ZX1";
				case "FFM":
				case "ECL":
				case "RFM":
				case "BBB":
				case "RMA":
					return "ZX2";
				case "COM":
				case "FWB":
					return "ZX3";
				default:
					throw new ArgumentOutOfRangeException(nameof(messageName), messageName, "Unknown message name");
			}
		}
	}
}
