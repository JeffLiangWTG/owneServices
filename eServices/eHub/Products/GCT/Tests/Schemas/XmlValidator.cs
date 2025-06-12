using System.Xml.Schema;
using System.Xml.Linq;
using Microsoft.XLANGs.BaseTypes;
using System.IO;
using System.Reflection;

namespace CargoWise.eHub.Products.GCT.Tests
{
    public class XmlValidator
    {
        public static string Validate<T>(string inputFile) where T : SchemaBase, new()
        {
            var document = XDocument.Load(TestHelper.GetEmbeddedResource(inputFile));
            var report = string.Empty;
            document.Validate(new T().SchemaSet, (o, e) =>
            {
                report = string.Format("{0}\r\n{1}", report, e.Message);
            });
            return report.TrimStart();
        }
    }
}
