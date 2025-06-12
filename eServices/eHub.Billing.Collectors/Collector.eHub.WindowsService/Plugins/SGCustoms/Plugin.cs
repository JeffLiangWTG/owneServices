using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.SGCustoms
{
	public class Plugin : SqlBillingTransactionsPlugin
	{
		protected override sealed string QueryName
		{
			get { return "CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.SGCustoms.Query.sql"; }
		}

		protected override sealed IEnumerable<TimeStampedTransaction> CreateTransactions(IDataRecord record)
		{
			var messageTrackingID = GetGuid(record, "MessageTrackingID").ToString();
			var receivedFromSenderUTC = GetDateTime(record, "AM_ReceivedFromSenderUTC");
			var timeStamp = GetDateTime(record, "AM_ArchivedUTC");
			var actionPurpose = GetString(record, "ActionPurpose");
			var priceItemCode = GetPriceItemCode(actionPurpose);

			yield return new TimeStampedTransaction(timeStamp, new BillingTransaction
			{
				Category = "SAC", 
				PriceItemCode = priceItemCode,
				BillableCount = 1,
				ReportingSource = "HUB",
				ServiceOccuredUTC = receivedFromSenderUTC,
				ClientID = GetString(record, "ClientID"),
				Branch = GetString(record, "Branch"), 
				ClientStaffCode = GetString(record, "ClientStaffCode"),
				MessageTrackingID = messageTrackingID,
				Reference1 = GetString(record, "Reference1"),
				Reference2 = GetString(record, "Reference2"),
				Reference3 = GetString(record, "Reference3"),
			});
		}

		static string GetPriceItemCode(string actionPurpose)
		{
			switch (actionPurpose)
			{
				case "AED":
					return "SAE";
				case "PCM":
					return "SAI";
				default:
					throw new ArgumentOutOfRangeException(nameof(actionPurpose), actionPurpose, "Unknown action purpose");
			}
		}
	}
}
