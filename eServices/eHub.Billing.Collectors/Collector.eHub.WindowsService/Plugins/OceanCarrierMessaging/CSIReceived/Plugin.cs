using System.Collections.Generic;
using System.Data;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.OceanCarrierMessaging.CSIReceived
{
    public class Plugin : SqlBillingTransactionsPlugin
    {
        protected override sealed string QueryName
        {
            get
            {
                return "CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.OceanCarrierMessaging.CSIReceived.Query.sql";
            }
        }

        protected override IEnumerable<TimeStampedTransaction> CreateTransactions(IDataRecord record)
        {
            var messageTrackingID = GetGuid(record, "MessageTrackingID").ToString();
            yield return new TimeStampedTransaction(GetDateTime(record, "AM_ArchivedUTC"), new BillingTransaction
            {
                ClientID = GetString(record, "ClientID"),
                BillableCount = 1,
                ReportingSource = "HUB",
                Reference1 = messageTrackingID,
                Reference2 = GetString(record, "AgentsReference"),
                Reference3 = GetString(record, "BookingNumber"),
                Reference4 = GetString(record, "BillNumber"),
                Reference5 = GetString(record, "Sender"),
                ServiceOccuredUTC = GetDateTime(record, "AM_ReceivedFromSenderUTC"),
                PriceItemCode = "CSI",
                Category = "SHI",
                MessageTrackingID = GetGuid(record, "MessageTrackingID").ToString(),
            });
        }
    }
}
