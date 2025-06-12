using Microsoft.XLANGs.BaseTypes;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Schema;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.IntegrationTests
{
    public class XmlValidator
    {
        public static string Validate<T>(string inputFile) where T : SchemaBase, new()
        {
            var document = XDocument.Load(GetEmbeddedResource(inputFile));
            var report = string.Empty;
            document.Validate(new T().SchemaSet, (o, e) =>
            {
                report = string.Format("{0}\r\n{1}", report, e.Message);
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
