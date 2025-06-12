using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.OceanCarrierMessaging.ShippingInstructionSent
{
	public class Plugin : SqlBillingTransactionsPlugin
	{
		protected override sealed string QueryName
		{
			get { return "CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.OceanCarrierMessaging.ShippingInstructionSent.Query.sql"; }
		}

		protected override IEnumerable<TimeStampedTransaction> CreateTransactions(IDataRecord record)
		{
			var messageTrackingID = GetGuid(record, "MessageTrackingID").ToString();

			yield return new TimeStampedTransaction(GetDateTime(record, "AM_ArchivedUTC"), new BillingTransaction
			{
				BillableCount = 1,
				ReportingSource = "HUB",
				ClientID = GetString(record, "ClientID"),
				Reference1 = messageTrackingID,
				Reference2 = regex.Replace(GetString(record, "Provider"), ""),
				Reference3 = GetString(record, "Ref3"),
				Reference4 = GetString(record, "Ref4"),
				Reference5 = GetString(record, "Ref5", true),
				ServiceOccuredUTC = GetDateTime(record, "AM_ReceivedFromSenderUTC"),
				Category = "SHI",
				PriceItemCode = GetPriceItemCode(GetString(record, "DocumentName")),
				MessageTrackingID = messageTrackingID,
				Version = 3,
			});
		}

		static string GetPriceItemCode(string documentName)
		{
			switch (documentName)
			{
				case "Booking Request":
					return "BRT";
				case "Shipping Instruction":
					return "SHI";
				case "Verified Gross Container Weight":
					return "VGM";
				case "Shipping Order":
					return "SHO";
				case "eManifest":
					return "EMN";
				default:
					return null;
			}
		}
		Regex regex = new Regex("_[^_]*$", RegexOptions.Compiled);
	}
}
