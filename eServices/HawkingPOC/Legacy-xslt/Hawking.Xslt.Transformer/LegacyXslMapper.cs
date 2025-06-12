using System;
using System.Collections.Generic;
using System.IO;
using Hawking.Xslt.ExtensionObjects.ExtensionObjectsConfig;

namespace Hawking.Xslt.Transformer
{
    public class LegacyXslMapper : ILegacyXslMapper
    {
        readonly IXsltTransformer transformer;
        readonly IExtensionXmlParser extensionXmlParser;

        public LegacyXslMapper(
            IXsltTransformer xsltTransformer,
            IExtensionXmlParser extensionObjectsConfigProvider)
        {
            transformer = xsltTransformer;
            extensionXmlParser = extensionObjectsConfigProvider;
        }

        public Stream Transform(
            Stream xslStream,
            Stream inputStream,
            Stream extxmlStream,
            object userCSharpInstance,
            string userCSharpNamespace = "http://schemas.microsoft.com/BizTalk/2003/userCSharp",
            bool executeCompiled = true)
        {
            if (extensionXmlParser == null)
            {
                throw new NotSupportedException("No extension objects provider");
            }

            var extensionObjects = extensionXmlParser.ParseExtensionXml(extxmlStream);
            if (userCSharpInstance != null)
            {
                extensionObjects.Add(userCSharpNamespace, userCSharpInstance);
            }

            return Transform(inputStream, xslStream, extensionObjects, executeCompiled);
        }

        public Stream Transform(
            Stream xslStream,
            Stream inputStream,
            IDictionary<string, object> extensionObjects,
            bool executeCompiled = true)
        {
            return transformer.Execute(xslStream, inputStream, extensionObjects, executeCompiled);
        }
    }
}
