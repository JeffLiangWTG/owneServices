namespace Hawking.Xslt.ExtensionObjects.Interfaces
{
    public interface ISGCustomsDataModelAccessor
    {
        string GetSGCustomsAccount(string brokerID, string eHubClientID);
        string GetSGCustomsSenderID(string accountName);
    }
}