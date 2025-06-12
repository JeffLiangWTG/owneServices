using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ForwardingPortMessaging.TPT
{
	public class Plugin : SqlBillingTransactionsPlugin
	{
		protected override string QueryName
		{
			get { return "CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ForwardingPortMessaging.TPT.Query.sql"; }
		}

		protected override IEnumerable<TimeStampedTransaction> CreateTransactions(IDataRecord record)
		{
			var timeStamp = GetDateTime(record, "AM_ArchivedUTC");
			var messageTrackingID = GetGuid(record, "MessageTrackingID").ToString();
			var priceItemCode = GetString(record, "PriceItemCode");
			var messageType = GetString(record, "MessageType");
			var reference1 = FormatReference1(GetString(record, "Reference1Prefix"), GetString(record, "ImportReferenceNumber"), GetString(record, "ExportReferenceNumber"));

			string reference2 = string.Empty;
			string reference3 = string.Empty;
			string reference4 = string.Empty;
			string reference5 = string.Empty;
			if (string.Compare(priceItemCode, "POZ", StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				reference2 = messageType;
				reference3 = FormatSubmissionType(GetString(record, "DataVersion"), GetString(record, "ActionCode"));
				reference4 = messageTrackingID;
			}
			else //PSZ
			{
				reference2 = GetString(record, "EventType");
				reference3 = messageType;
				reference4 = GetString(record, "OrderNumber");
				reference5 = messageTrackingID;
			}

			yield return new TimeStampedTransaction(timeStamp, new BillingTransaction
			{
				BillableCount = 1,
				ReportingSource = "HUB",
				ClientID = GetString(record, "ClientID"),
				Reference1 = reference1,
				Reference2 = reference2,
				Reference3 = reference3,
				Reference4 = reference4,
				Reference5 = reference5,
				ServiceOccuredUTC = GetDateTime(record, "AM_ReceivedFromSenderUTC"),
				Category = "PMG",
				PriceItemCode = priceItemCode,
				MessageTrackingID = messageTrackingID,
				Version = 1
			});
		}

		static string FormatReference1(string reference1Prefix, string importReferenceNumber, string exportReferenceNumber)
		{
			var reference1PostFix = !string.IsNullOrEmpty(importReferenceNumber) ? importReferenceNumber : exportReferenceNumber;
			return string.Format("{0}_{1}", (reference1Prefix != null) ? reference1Prefix : string.Empty, (reference1PostFix != null) ? reference1PostFix : string.Empty);
		}

		static string FormatSubmissionType(string dataVersion, string actionCode)
		{
			if (string.Compare(dataVersion, "1", StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				return "Original";
			}

			if (string.Compare(actionCode, "WTH", StringComparison.InvariantCultureIgnoreCase) == 0)
			{
				return "Withdrawal";
			}

			return "Amendment";
		}
	}
}
