using System;
using System.Data;
using System.Linq;
using Hawking.eHub.Model.eHubTransactions;
using Hawking.eHub.Transform.Helper.Wrapper.Extensions;
using Hawking.Xslt.ExtensionObjects.Interfaces;

namespace Hawking.CargoWise.eHub.Core.Transforms.Helper
{
    public class DataModelAccessor : IDataModelAccessor
    {
        eHubTransactionsContext context;
        public DataModelAccessor(IeHubTransactionsContext transactionsContext)
        {
            context = transactionsContext as eHubTransactionsContext;
        }

        public string GetClientRegistrationCode(string clientID, string qualifier, string registrationTypeID)
        {
            if (string.IsNullOrEmpty(clientID) || string.IsNullOrEmpty(clientID.Trim()))
            {
                throw new ArgumentNullException(nameof(clientID));
            }

            if (string.IsNullOrEmpty(registrationTypeID) || string.IsNullOrEmpty(registrationTypeID.Trim()))
            {
                throw new ArgumentNullException(nameof(registrationTypeID));
            }

            if (string.IsNullOrWhiteSpace(qualifier))
            {
                qualifier = null;
            }

            var codes = context.eHubClientRegistration
                .Where(x => x.CX_Code == clientID &&
                x.CX_RTNavigation.RT_ID == registrationTypeID &&
                (x.CX_Qualifier == qualifier || x.CX_Qualifier == null)).Select(x => x.CX_Code).AsQueryable();

            //where r.eHubClient.CC_ID == clientID && r.eHubRegistrationType.RT_ID == registrationTypeID
            //&& (r.CX_Qualifier == qualifier || r.CX_Qualifier == null)
            //orderby r.CX_Qualifier descending
            //select r.CX_Code;

            return codes.FirstOrDefault() ?? string.Empty;
        }

        public void InsertSubscriptionValue(string subscriptionType, string providerID, string subscriberID, string value)
        {
            InsertSubscriptionValue(subscriptionType, providerID, subscriberID, value, null, null);
        }

        public void InsertSubscriptionValue(string subscriptionType, string providerID, string subscriberID, string value, string reference)
        {
            InsertSubscriptionValue(subscriptionType, providerID, subscriberID, value, reference, null);
        }

        public void InsertSubscriptionValue(string subscriptionType, string providerID, string subscriberID, string value, string reference, string referenceType)
        {
            if (subscriptionType == null) throw new ArgumentNullException("subscriptionType");
            if (subscriptionType.Trim() == String.Empty) throw new ArgumentException("Argument is empty or whitespace", "subscriptionType");
            if (providerID == null) throw new ArgumentNullException("providerID");
            if (providerID.Trim() == String.Empty) throw new ArgumentException("Argument is empty or whitespace", "providerID");
            if (subscriberID == null) throw new ArgumentNullException("subscriberID");
            if (subscriberID.Trim() == String.Empty) throw new ArgumentException("Argument is empty or whitespace", "subscriberID");
            if (value == null) throw new ArgumentNullException("value");

            var subType = (from s in context.eHubSubscriptionType where s.ST_ID == subscriptionType select s).FirstOrDefault();
            if (subType == null) throw new ArgumentException(String.Format("Subscription type not found: '{0}'", subscriptionType));

            var provider = (from c in context.eHubClient where c.CC_ID == providerID select new { c.CC_PK }).FirstOrDefault();
            if (provider == null) throw new ArgumentException(String.Format("Provider client ID not found: '{0}'", providerID));

            var subscriber = (from c in context.eHubClient where c.CC_ID == subscriberID select new { c.CC_PK }).FirstOrDefault();
            if (subscriber == null) throw new ArgumentException(String.Format("Subscriber client ID not found: '{0}'", subscriberID));

            using (var tran = context.Database.BeginTransaction())
            {
                var subValue = (from v in context.eHubSubscriptionValue
                                where v.SV_ST == subType.ST_PK
                                      && v.SV_CC_Sender == provider.CC_PK
                                      && v.SV_CC_Recipient == subscriber.CC_PK
                                      && v.SV_Value == value
                                      && v.SV_ReferenceType == referenceType
                                select v).FirstOrDefault();

                if (subValue == null)
                {
                    subValue = new eHubSubscriptionValue
                    {
                        SV_PK = Guid.NewGuid(),
                        SV_ST = subType.ST_PK,
                        SV_CC_Sender = provider.CC_PK,
                        SV_CC_Recipient = subscriber.CC_PK,
                        SV_Value = value,
                        SV_Reference = reference,
                        SV_ReferenceType = referenceType,
                        SV_SubscribedUTC = DateTime.UtcNow,
                        SV_ExpiryUTC = subType.ST_ExpiryDays.HasValue ? DateTime.UtcNow.AddDays(subType.ST_ExpiryDays.Value) : (DateTime?)null
                    };

                    context.eHubSubscriptionValue.Add(subValue);
                    try
                    {
                        context.SaveChanges();
                    }
                    catch (DataException ex)
                    {
                        if (ex.IsDuplicateKeyViolation())
                        {
                            return;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                else
                {
                    if (subType.ST_ExpiryDays.HasValue)
                    {
                        subValue.SV_Reference = reference;
                        subValue.SV_ExpiryUTC = DateTime.UtcNow.AddDays(subType.ST_ExpiryDays.Value);
                        context.SaveChanges();
                    }
                    else
                    {
                        return;
                    }
                }
                tran.Commit();
            }
        }
    }
}
