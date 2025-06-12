using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;
using Hawking.UnitTest.Tools.Xml;

namespace Hawking.UnitTest.Tools.Xslt
{
    public static class XsltTester
    {
        public static XmlDiffResult Execute(
            Stream xslStream, 
            Stream inputStream, 
            Stream expectedOutputStream, 
            IDictionary<string, object> extensionObjects)
        {
            xslStream.Position = 0;
            inputStream.Position = 0;
            expectedOutputStream.Position = 0;

            var inputDocument = new XPathDocument(inputStream);
            var xslCompiledTransform = new XslCompiledTransform(Debugger.IsAttached);
            var xsltSettings = new XsltSettings(false, false);
            var xsltFile = Path.GetTempFileName();
            if (Debugger.IsAttached)
            {
                var xsl = new StreamReader(xslStream).ReadToEnd();
                var xdoc = XDocument.Parse(xsl);
                
                using (var writer = new XmlTextWriter(xsltFile, new UTF8Encoding(false)) { Formatting = Formatting.Indented })
                {
                    xdoc.Save(writer);
                }

                xslCompiledTransform.Load(xsltFile, xsltSettings, new XmlUrlResolver());
            }
            else
            {
                var xmlReader = new XmlTextReader(xslStream);
                xslCompiledTransform.Load(xmlReader, xsltSettings, new XmlUrlResolver());
            }

            var transformArgs = new XsltArgumentList();
            foreach (var extensionObject in extensionObjects)
            {
                transformArgs.AddExtensionObject(extensionObject.Key, extensionObject.Value);
            }

            try
            {
                using (var resultStream = new MemoryStream())
                {
                    using (var resultXmlWriter = XmlHelper.CreateFormattedXmlWriter(resultStream))
                    {
                        xslCompiledTransform.Transform(inputDocument, transformArgs, resultXmlWriter);

                        var diffResult = XmlDiffTool.Execute(resultStream, expectedOutputStream);
                        return diffResult;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }
    }
}
