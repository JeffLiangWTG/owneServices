using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CargoWise.RefDbRepo.VNReferenceData.Tests
{
	public static class TestHelper
	{
		public static bool AreXmlFilesEqual(string xmlFilePath1, string xmlFilePath2, string[] tagsToIgnore = null)
		{
			var xmlString1 = File.ReadAllText(xmlFilePath1);
			var xmlString2 = File.ReadAllText(xmlFilePath2);
			return AreXmlStringsEqual(xmlString1, xmlString2, tagsToIgnore);
		}

		public static bool AreXmlStringsEqual(string xmlString1, string xmlString2, string[] tagsToIgnore = null)
		{
			var xml1 = XElement.Parse(xmlString1);
			var xml2 = XElement.Parse(xmlString2);

			tagsToIgnore ??= [];
			foreach (var tag in tagsToIgnore)
			{
				xml1.Descendants(tag).Remove();
				xml2.Descendants(tag).Remove();
			}
			xml1.DescendantNodes().OfType<XComment>().ToList().ForEach(c => c.Remove());
			xml2.DescendantNodes().OfType<XComment>().ToList().ForEach(c => c.Remove());

			return XNode.DeepEquals(xml1, xml2);
		}
	}
}
