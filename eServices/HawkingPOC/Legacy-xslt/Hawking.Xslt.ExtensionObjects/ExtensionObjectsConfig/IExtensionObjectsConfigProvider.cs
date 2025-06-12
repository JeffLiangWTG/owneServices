using System.Collections.Generic;

namespace Hawking.Xslt.ExtensionObjects.ExtensionObjectsConfig
{
    public interface IExtensionObjectsConfigProvider
    {
        IDictionary<string, object> ExtensionObjects { get; }
        object GetExtensionObjectForNamespace(string namespaceName);
    }
}
