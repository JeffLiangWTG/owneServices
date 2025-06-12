namespace Hawking.Xslt.ExtensionObjects.Interfaces
{
    public interface IContextAccessor
    {
        string GetContextProperty(string contextItemName, string contextItemNamespace);
        string GetContextProperty(string contextItemName, string contextItemNamespace, string defaultValue);
        void SetContextProperty(string contextItemName, string contextItemNamespace, string replacementValue);
    }
}
