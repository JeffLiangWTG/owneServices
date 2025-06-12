using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Hawking.Xslt.Transformer
{
    public class XsltTransformer : IXsltTransformer
    {
        public Stream Execute(
            Stream xslStream,
            Stream inputStream,
            IDictionary<string, object> extensionObjects,
            bool executeCompiled = true)
        {
            ThrowArgumentNullExceptionIfNull(xslStream, nameof(xslStream));
            ThrowArgumentNullExceptionIfNull(inputStream, nameof(inputStream));

            xslStream.Position = 0;
            inputStream.Position = 0;

            using (var resultStream = new MemoryStream())
            {
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    Encoding = new UTF8Encoding(false)
                };

                using (var resultXmlWriter = XmlWriter.Create(resultStream, settings))
                {
                    var inputDocument = new XPathDocument(inputStream);
                    var transformArgs = new XsltArgumentList();
                    if (extensionObjects != null)
                    {
                        foreach (var extensionObject in extensionObjects)
                        {
                            transformArgs.AddExtensionObject(extensionObject.Key, extensionObject.Value);
                        }
                    }

                    var xmlReader = new XmlTextReader(xslStream);

                    if (executeCompiled)
                    {
                        var xslCompiledTransform = new XslCompiledTransform();
                        var xsltSettings = new XsltSettings(false, false);

                        xslCompiledTransform.Load(xmlReader, xsltSettings, new XmlUrlResolver());
                        xslCompiledTransform.Transform(inputDocument, transformArgs, resultXmlWriter);
                    }
                    else
                    {
                        var xslTransform = new XslTransform();
                        xslTransform.Load(xmlReader, new XmlUrlResolver());
                        xslTransform.Transform(inputDocument, transformArgs, resultXmlWriter);
                    }

                    return resultStream;
                }
            }
        }

        static void ThrowArgumentNullExceptionIfNull(object instance, string name)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(name);
            }
        }
    }
}
