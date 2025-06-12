using System.Collections.Generic;
using System.IO;

namespace Hawking.Xslt.Transformer
{
    public interface ILegacyXslMapper
    {
        Stream Transform(
            Stream xslStream,
            Stream inputStream,
            Stream extxmlStream,
            object userCSharpInstance,
            string userCSharpNamespace = "http://schemas.microsoft.com/BizTalk/2003/userCSharp",
            bool executeCompiled = true);

        Stream Transform(
            Stream xslStream,
            Stream inputStream,
            IDictionary<string, object> extensionObjects,
            bool executeCompiled = true);
    }
}
