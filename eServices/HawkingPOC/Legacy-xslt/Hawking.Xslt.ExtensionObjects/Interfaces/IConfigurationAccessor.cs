namespace Hawking.Xslt.ExtensionObjects.Interfaces
{
    public interface IConfigurationAccessor
    {
        string GetClientGroupToken(string clientId);
        string GetClientToken(string systemId, string staffCode);
    }
}
