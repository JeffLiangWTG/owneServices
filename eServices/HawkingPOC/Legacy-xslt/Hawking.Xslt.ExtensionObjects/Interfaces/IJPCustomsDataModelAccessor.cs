namespace Hawking.Xslt.ExtensionObjects.Interfaces
{
    public interface IJPCustomsDataModelAccessor
    {
        string GetUsername(string senderID);
        string GetPassword(string senderID);
    }
}
