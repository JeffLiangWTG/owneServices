using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ShippingPortMessaging
{
	public class Plugin : SqlBillingTransactionsPlugin
	{
		protected override sealed string QueryName
		{
			get { return "CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ShippingPortMessaging.Query.sql"; }
		}

		protected override sealed IEnumerable<TimeStampedTransaction> CreateTransactions(IDataRecord record)
		{
			var recipientRoleCode = GetString(record, "RecipientRoleCode");
			var messageTrackingID = GetGuid(record, "MessageTrackingID").ToString();

			var transaction = new TimeStampedTransaction(GetDateTime(record, "AM_ArchivedUTC"), new BillingTransaction()
			{
				BillableCount = 1,
				ReportingSource = "HUB",
				ClientID = GetString(record, "ClientID"),
				ServiceOccuredUTC = GetDateTime(record, "AM_ReceivedFromSenderUTC"),
				Category = "SPM",
				Reference1 = messageTrackingID,
				Reference2 = GetString(record, "MessageRecipient"),
				Reference3 = recipientRoleCode,
				MessageTrackingID = messageTrackingID,
			});

			switch (recipientRoleCode)
			{
				case "PER": //COPARN
					transaction.BillingTransaction.PriceItemCode = "SPA";
					transaction.BillingTransaction.Reference4 = GetString(record, "ContainerReleaseNumber");
					transaction.BillingTransaction.Reference5 = GetString(record, "ContainerCount");
					break;
				case "PIR": //COREOR
					transaction.BillingTransaction.PriceItemCode = "SPE";
					transaction.BillingTransaction.Reference4 = GetString(record, "ContainerNumber");
					transaction.BillingTransaction.Reference5 = GetString(record, "DataSourceKey");
					break;
				case "PEM": //COPRAR
				case "PIM": //COPRAR
					transaction.BillingTransaction.PriceItemCode = "SPR";
					transaction.BillingTransaction.Reference4 = GetString(record, "DataSourceKey");
					break;
				default:
					throw new ArgumentOutOfRangeException("RecipientRoleCode");
			}

			yield return transaction;
		}
	}
}
