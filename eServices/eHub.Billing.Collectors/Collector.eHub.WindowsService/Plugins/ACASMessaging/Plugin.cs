using System.Collections.Generic;
using System.Data;
using System.IO;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ACASMessaging
{
    public class Plugin : SqlBillingTransactionsPlugin
    {
        protected sealed override string QueryName
        {
            get
            {
                return "CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.ACASMessaging.Query.sql";
            }
        }

        protected override IEnumerable<TimeStampedTransaction> CreateTransactions(IDataRecord record)
        {
            var timeStamp = GetDateTime(record, "AM_ArchivedUTC");
            var messageTrackingID = GetGuid(record, "MessageTrackingID").ToString();
            var documentName = GetString(record, "DocumentName");

            yield return new TimeStampedTransaction(timeStamp, new BillingTransaction
            {
                BillableCount = 1,
                ReportingSource = "HUB",
                ClientID = GetString(record, "ClientID"),
                Reference1 = GetString(record, "ShipmentNumber"),
                Reference2 = documentName,
                Reference3 = GetReference3(GetString(record, "Purpose")),
                Reference4 = GetString(record, "HAWBNumber"),
                Reference5 = messageTrackingID,
                ServiceOccuredUTC = GetDateTime(record, "AM_ReceivedFromSenderUTC"),
                Category = "ACS",
                PriceItemCode = GetPriceItemCode(GetString(record, "PortOfFirstArrival"), documentName),
                MessageTrackingID = messageTrackingID,
                Version = 1
            });
        }

        private static string GetReference3(string purpose)
        {
            switch (purpose)
            {
                case "ORG": return "Original";
                case "AMD": return "Amendment";
                case "WTH": return "Withdrawal";
                default: throw new InvalidDataException("Unrecognised Purpose for Reference3: " + purpose);
            }
        }

        private static string GetPriceItemCode(string portOfFirstArrival, string documentName)
        {
            if (portOfFirstArrival.StartsWith("BR"))
                return "CAB";
            else
                return "CAU";
        }
    }
}
