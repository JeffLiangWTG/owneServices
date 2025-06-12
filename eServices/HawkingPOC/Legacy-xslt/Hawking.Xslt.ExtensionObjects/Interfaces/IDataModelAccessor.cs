namespace Hawking.Xslt.ExtensionObjects.Interfaces
{
    public interface IDataModelAccessor
    {
        string GetClientRegistrationCode(string clientID, string qualifier, string registrationTypeID);
        void InsertSubscriptionValue(string subscriptionType, string providerID, string subscriberID, string value);
        void InsertSubscriptionValue(string subscriptionType, string providerID, string subscriberID, string value, string reference);
        void InsertSubscriptionValue(string subscriptionType, string providerID, string subscriberID, string value, string reference, string referenceType);
    }
}
