using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Linq;
using Microsoft.XLANGs.BaseTypes;
using System.IO;
using System.Reflection;

namespace CargoWise.eHub.Products.ForwardAir.Tests
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
