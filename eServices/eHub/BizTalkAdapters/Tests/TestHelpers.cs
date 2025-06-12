using System.Linq;
using System.Xml.Linq;

namespace CargoWise.eHub.BizTalkAdapters.Tests
{
	internal static class TestHelpers
	{
		public static XElement GetDefaultXml(string schema)
		{
			XNamespace xs = "http://www.w3.org/2001/XMLSchema";
			var schemaDoc = XDocument.Parse(schema);
			var configXsd = schemaDoc.Descendants(xs + "element").FirstOrDefault(e => e.Attribute("name")?.Value == "Config");
			return new XElement("Config", configXsd.Descendants(xs + "element")
				.Select(p => new XElement(p.Attribute("name").Value, p.Attribute("default")?.Value)));
		}
	}
}
