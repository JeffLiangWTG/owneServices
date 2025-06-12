using System;
using System.Xml.Linq;
using CargoWise.eHub.DataModel.Accessors;
using CargoWise.eHub.DataModel.eHubTransactions;

namespace CargoWise.eHub.Core.Orchestrations.Helper
{
    public class ReporteHubError
    {
        public static string InserteHubError(string clientID, string errorType, string description, string errorDetail, string inboxPK = null, string outboxPK = null)
        {
            Guid? inboxPKGuid = null;
            Guid? outboxPKGuid = null;

            if (string.IsNullOrWhiteSpace(clientID))
            {
                return $"{nameof(clientID)} is required.";
            }

            if (string.IsNullOrWhiteSpace(errorType))
            {
                return $"{nameof(errorType)} is required.";
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                return $"{nameof(description)} is required.";
            }

            if (!string.IsNullOrWhiteSpace(inboxPK))
            {
                Guid pk;
                if (Guid.TryParse(inboxPK, out pk))
                {
                    inboxPKGuid = pk;   
                }
                else
                {
                    return $"{nameof(inboxPK)} is not valid.";
                }
            }

            if (!string.IsNullOrWhiteSpace(outboxPK))
            {
                Guid pk;
                if (Guid.TryParse(outboxPK, out pk))
                {
                    outboxPKGuid = pk;
                }
                else
                {
                    return $"{nameof(outboxPK)} is not valid.";
                }
            }

            if (!string.IsNullOrEmpty(errorDetail))
            {
                errorDetail = (new XElement("ErrorDetail", errorDetail)).ToString();
            }

            try
            {
                if (eHubTransactionsAccessor.IsProductionClient(clientID))
                {
                    using (var context = GetContext())
                    {
                        context.eHubErrors.Add(new eHubError
                        {
                            EE_PK = NewGuid(),
                            EE_DateTimeUTC = CurrentUtcDate(),
                            EE_Source = "BTS",
                            EE_ErrorType = errorType,
                            EE_Description = description,
                            EE_ErrorDetail = errorDetail,
                            EE_EI_Inbox = inboxPKGuid,
                            EE_OI_Outbox = outboxPKGuid,
                            EE_Alerted = false
                        });

                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

            return string.Empty;
        }

        internal static Func<eHubTransactionsContext> GetContext = () => new eHubTransactionsContext();
        internal static Func<Guid> NewGuid = Guid.NewGuid;
        internal static Func<DateTime> CurrentUtcDate = () => DateTime.UtcNow;
    }
}
