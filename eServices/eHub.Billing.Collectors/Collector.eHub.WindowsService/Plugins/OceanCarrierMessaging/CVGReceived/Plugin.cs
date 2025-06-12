using System.Collections.Generic;
using System.Data;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.OceanCarrierMessaging.CVGReceived
{
    public class Plugin : SqlBillingTransactionsPlugin
    {
        protected override sealed string QueryName
        {
            get { return "CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.OceanCarrierMessaging.CVGReceived.Query.sql"; }
        }

        protected override IEnumerable<TimeStampedTransaction> CreateTransactions(IDataRecord record)
        {
			yield return new TimeStampedTransaction(GetDateTime(record, "AM_ArchivedUTC"), new BillingTransaction
			{
				BillableCount = 1,
				ReportingSource = "HUB",
				ClientID = GetString(record, "ClientID"),
				Reference1 = GetGuid(record, "MessageTrackingID").ToString(),
				Reference2 = GetString(record, "ContainerNumber"),
				Reference3 = GetString(record, "BookingNumber", true),
				Reference4 = GetString(record, "BillNumber", true),
				Reference5 = GetString(record, "Sender"),
				ServiceOccuredUTC = GetDateTime(record, "AM_ReceivedFromSenderUTC"),
				Category = "SHI",
				PriceItemCode = GetString(record, "PriceItemCode"),
				MessageTrackingID = GetGuid(record, "MessageTrackingID").ToString(),
			});
		}
    }
}
