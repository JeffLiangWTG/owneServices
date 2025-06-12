using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Schema;
using CargoWise.eHub.Products.TWCustoms.Schemas;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.XLANGs.BaseTypes;

namespace CargoWise.eHub.Products.TWCustoms.BizTalk.Tests
{
    [TestClass]
    public class SchemaTests
    {

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void ValidatePluginRequestSchema()
        {
            ValidateSchema<PluginRequest>("Schemas.TestFiles.PluginSendRequest.xml");
        }

        public static void ValidateSchema<T>(string pluginSendRequest) where T : SchemaBase, new()
        {
            var schemaLogs = XmlValidator.Validate<T>(pluginSendRequest).Split(Environment.NewLine.ToCharArray()).ToList();
            var result = schemaLogs.ToList();
            var actualLogs = result.Where(x => !string.IsNullOrEmpty(x)).ToList();
            var logs = string.Join("\r\n", actualLogs);
            Assert.AreEqual(0, actualLogs.Count, pluginSendRequest + "\r\n" + typeof(T).Name + " is not valid: \r\n " + logs);
        }
    }
    public class XmlValidator
    {
        public static string Validate<T>(string inputFile) where T : SchemaBase, new()
        {
            var document = XDocument.Load(GetEmbeddedResource(inputFile));
            var report = string.Empty;
            document.Validate(new T().SchemaSet, (o, e) =>
            {
                report = $"{report}\r\n{e.Message}";
            });
            return report.TrimStart();
        }

        private static Stream GetEmbeddedResource(string resourceName)
        {
            string fullResourceName = Assembly.GetExecutingAssembly().GetName().Name + '.' + resourceName;
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);
        }
    }
}
