using System.Collections.Generic;
using System.Data;
using CargoWise.Billing.API;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;
using System.Text.RegularExpressions;
using CargoWise.Billing.CollectorService.Plugin;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.OceanCarrierMessaging.ServiceProviderShippingInstructions
{
	public class Plugin : SqlBillingTransactionsPlugin
	{
		protected override sealed string QueryName
		{
			get { return "CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.OceanCarrierMessaging.ServiceProviderShippingInstructions.Query.sql"; }
		}

		protected override IEnumerable<TimeStampedTransaction> CreateTransactions(IDataRecord record)
		{
			var clientID = GetString(record, "ClientID");
			yield return new TimeStampedTransaction(GetDateTime(record, "AM_ArchivedUTC"), new BillingTransaction
			{

				ClientID = clientID,
				BillableCount = 1,
				ReportingSource = "HUB",
				Reference1 = regex.Replace(GetString(record, "ServiceProvider"), ""),
				Reference2 = clientID,
				Reference3 = GetString(record, "Consol"),
				Reference4 = GetString(record, "CoLoadBookingConfirmationReference", true) ?? GetString(record, "BookingConfirmationReference", true),
				Reference5 = GetString(record, "NVOCCSCAC", true) ?? GetString(record, "NVOCCCW1", true) ?? GetString(record, "CarrierSCAC", true) ?? GetString(record, "CarrierCW1", true),
				ServiceOccuredUTC = GetDateTime(record, "AM_ReceivedFromSenderUTC"),
				Category = "SHI",
				PriceItemCode = GetString(record, "PriceItemCode"),
				MessageTrackingID = GetGuid(record, "MessageTrackingID").ToString(),
			});
		}

		Regex regex = new Regex("_[^_]*$", RegexOptions.Compiled);
	}
}
