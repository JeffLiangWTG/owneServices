using System.Collections.Generic;
using System.Data;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.RailincMessaging
{
    public class Plugin : SqlBillingTransactionsPlugin
    {
        protected override sealed string QueryName
        {
            get { return "CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.RailincMessaging.Query.sql"; }
        }

        protected override IEnumerable<TimeStampedTransaction> CreateTransactions(IDataRecord record)
        {
            var messageTrackingID = GetGuid(record, "MessageTrackingID").ToString();
            var msgType = GetString(record, "MsgType");
            
            var transaction = new TimeStampedTransaction(GetDateTime(record, "AM_ArchivedUTC"), new BillingTransaction
            {
                BillableCount = 1,
                ReportingSource = "HUB",
                ClientID = GetString(record, "ClientID"),
                Reference1 = msgType,
                Reference2 = GetString(record, "Consol"),
                Reference3 = GetString(record, "Container"),
                Reference4 = messageTrackingID,
                Reference5 = CreateReference5ForCLM(record, msgType),
                ServiceOccuredUTC = GetDateTime(record, "AM_ReceivedFromSenderUTC"),
                Category = "RIM",
                PriceItemCode = "RIM",
                MessageTrackingID = messageTrackingID,
                Version = 2,
            });

            if (transaction.BillingTransaction.Category.Equals("RIM")
                && transaction.BillingTransaction.PriceItemCode.Equals("RIM")
                && (transaction.BillingTransaction.Reference1.Equals("CLM") || transaction.BillingTransaction.Reference1.Equals("CLU"))
                && !transaction.BillingTransaction.Reference3.Equals("")
                && transaction.BillingTransaction.Reference2.StartsWith("C")) 
            {
                transaction.BillingTransaction.PriceItemCode = "RIC";
                var originalReference1 = transaction.BillingTransaction.Reference1;
                transaction.BillingTransaction.Reference1 = transaction.BillingTransaction.Reference3;
                transaction.BillingTransaction.Reference3 = originalReference1;
            }

            yield return transaction;
        }

        private string CreateReference5ForCLM(IDataRecord record, string msgType)
        {
            string ref5 = null;
            if (msgType == "CLM")
            {
                bool isEstimate;
                bool.TryParse(GetString(record, "IsEstimate", true), out isEstimate);
                if (isEstimate)
                {
                    ref5 = string.Concat(
                        GetString(record, "EventType", true),
                        " [E]-",
                        GetString(record, "Location", true));
                }
                else
                {
                    ref5 = string.Concat(
                        GetString(record, "EventType", true),
                        " [A]-",
                        GetString(record, "Location", true));
                }

            }

            return ref5;
        }
    }
}
