using System.Collections.Generic;
using System.Data;
using System.IO;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ForwardingPortMessaging.NGB
{
	public class Plugin : SqlBillingTransactionsPlugin
	{
		protected sealed override string QueryName
		{
			get
			{
				return "CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ForwardingPortMessaging.NGB.Query.sql";
			}
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
				Reference1 = GetString(record, "Reference1"),
				Reference2 = GetString(record, "MessageType"),
				Reference3 = GetSubmissionType(GetString(record, "SubmissionType")),
				Reference4 = GetString(record, "Reference4"),
				Reference5 = messageTrackingID,
				ServiceOccuredUTC = GetDateTime(record, "AM_ReceivedFromSenderUTC"),
				Category = "PMG",
				PriceItemCode = GetString(record, "PriceItemCode"),
				MessageTrackingID = messageTrackingID,
				Version = 1
			});
		}

		private static string GetSubmissionType(string submissinCode)
		{
			switch (submissinCode)
			{
				case "ORG": return "Original";
				case "AMD": return "Amendment";
				case "WTH": return "Withdraw";
				default: throw new InvalidDataException("Unrecognised submission type: " + submissinCode);
			}
		}
	}
}
