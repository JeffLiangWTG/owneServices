namespace Hawking.Xslt.ExtensionObjects.Interfaces
{
    public interface INEXDOCDataModelAccessor
    {
        string GetVendorToken(string recipientID);
        string GetInstallationToken(string recipientID);
        string GetInstallationPassword(string recipientID);
    }
}
