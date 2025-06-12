using System.Collections.Generic;
using System.IO;

namespace Hawking.Xslt.ExtensionObjects.ExtensionObjectsConfig
{
    public interface IExtensionXmlParser : IExtensionObjectsConfigProvider
    {
        IDictionary<string, object> ParseExtensionXml(Stream extensionXmlStream);
    }
}
