using System.IO;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Schema;
using Microsoft.XLANGs.BaseTypes;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.FR.Tests
{
  public class XMLValidator
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
