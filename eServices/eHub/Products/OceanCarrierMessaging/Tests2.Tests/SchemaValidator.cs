using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.XLANGs.BaseTypes;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  public class SchemaValidator
  {
    public void ValidateSchema<T>(string expectedOutput, List<string> errorWhiteList) where T : SchemaBase, new()
    {
      var schemaLogs = XmlValidator.Validate<T>(expectedOutput).Split(Environment.NewLine.ToCharArray()).ToList();

      List<string> result = new List<string>();
      foreach (var log in schemaLogs)
      {
        if (!ContainsAny(log, errorWhiteList))
        {
          result.Add(log);
        }
      }

      var actualLogs = result.Where(x => !string.IsNullOrEmpty(x)).ToList();
      var logs = string.Join("\r\n", actualLogs);
      Assert.AreEqual(0, actualLogs.Count, expectedOutput + "\r\n" + typeof(T).Name + " is not valid: \r\n " + logs);
    }

    internal bool ContainsAny(string input, List<string> containsKeywords)
    {
      return containsKeywords.Any(keyword => input.IndexOf(keyword, StringComparison.CurrentCultureIgnoreCase) >= 0);
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