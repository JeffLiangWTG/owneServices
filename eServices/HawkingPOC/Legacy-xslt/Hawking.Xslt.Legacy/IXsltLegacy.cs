using System.Collections.Generic;
using System.IO;

namespace Hawking.Xslt.Legacy
{
    public interface IXsltLegacy
    {
        Stream XslContent { get; }
        string ExtxmlContent { get; }
        string UserCSharpNamespaceName { get; }
        bool HasUserCSharpScripts { get; }
        KeyValuePair<string, object> CreateUserCsharpExtensionObject();
    }
}
