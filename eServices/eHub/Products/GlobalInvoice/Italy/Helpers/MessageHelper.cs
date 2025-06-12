using CargoWise.eHub.DataModel.eHubTransactions;
using Microsoft.XLANGs.BaseTypes;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.eHub.Core.Logging;

namespace CargoWise.eHub.Products.GlobalInvoice.Italy.Helpers
{
    public class MessageHelper
    {
        public static XmlDocument GetSoapFromResponse(XLANGMessage message)
        {
            var messageString = "";
            using (var sr = new StreamReader((Stream)message[0].RetrieveAs(typeof(Stream))))
                messageString = sr.ReadToEnd();

            var splited = messageString.Split(new string[] { "--" }, StringSplitOptions.None).Last();

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(splited);
            return xmlDoc;
        }

        public static bool IsDuplicate(String messageTrackingID, string outboxPK, string CC_Sender, string CC_Recipient, OrchestrationLogger logger)
        {
            try
            {
                var context = NewEHubTransactionContext();

                var subscriptionType = (from type in context.eHubSubscriptionTypes where type.ST_ID == "GEI_IT" select type).FirstOrDefault();

                // find subscriptions created by duplicate messages
                var previous = (
                    from subscription in context.eHubSubscriptionValues
                    where  subscription.SV_Value == messageTrackingID
                        && subscription.SV_ST == subscriptionType.ST_PK
                        && subscription.SV_CC_Sender == (from c in context.eHubClients where c.CC_ID == CC_Sender select c.CC_PK).FirstOrDefault()
                        && subscription.SV_CC_Recipient == (from c in context.eHubClients where c.CC_ID == CC_Recipient select c.CC_PK).FirstOrDefault()
                    select subscription
                ).FirstOrDefault();

                // If there are any matching subscriptions for this message it is a duplicate
                if (previous != null)
                {
                    logger.Log.WarnFormat("Duplicate message ignored. MessageTrackingID: {0}, OutboxPK: {1}", messageTrackingID, outboxPK);

                    // returning true causes the orchestration to skip all further processing on the message.
                    return true;
                }

                // This message is not a duplicate.
                // Save the messageTrackingID so other messages can check if they are a duplicate with this one.
                var subValue = new eHubSubscriptionValue
                {
                    SV_PK = Guid.NewGuid(),
                    SV_ST = subscriptionType.ST_PK,
                    SV_CC_Sender = (from c in context.eHubClients where c.CC_ID == CC_Sender select c.CC_PK).FirstOrDefault(),
                    SV_CC_Recipient = (from c in context.eHubClients where c.CC_ID == CC_Recipient select c.CC_PK).FirstOrDefault(),
                    SV_Value = messageTrackingID,
                    SV_SubscribedUTC = DateTime.UtcNow,
                    SV_ExpiryUTC = subscriptionType.ST_ExpiryDays.HasValue ? DateTime.UtcNow.AddDays(subscriptionType.ST_ExpiryDays.Value) : (DateTime?)null
                };
                context.eHubSubscriptionValues.Add(subValue);
                context.SaveChanges();
            }
            // We dont want any exceptions to bring this down as the IsDuplicate check is just nice to have.
            // So just return false instead.
            catch (Exception e)
            {
                logger.Log.WarnFormat("Exception thrown in IsDuplicate: " + e);
            }

            return false;
        }

        public static Func<eHubTransactionsContext> NewEHubTransactionContext = () => new eHubTransactionsContext();
    }
}
