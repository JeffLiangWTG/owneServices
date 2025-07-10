using System.Text.RegularExpressions;
using System.Xml;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	public static class BEReferenceDataTestHelper
	{
		public static void AssertEqualXML(string xml, string expectedXML)
		{
			var effectiveOutputXmlDocument = new XmlDocument();
			effectiveOutputXmlDocument.LoadXml(xml);
			var expectedOutputXmlDocument = new XmlDocument();
			expectedOutputXmlDocument.LoadXml(expectedXML);
			var effectiveOutputXmlDocumentInnerXml = effectiveOutputXmlDocument.InnerXml;
			var regex = new Regex(@"<AppProgramArgs>(.*?)</AppProgramArgs>");
			var match = regex.Match(effectiveOutputXmlDocumentInnerXml);
			if (match.Success)
			{
				effectiveOutputXmlDocumentInnerXml = effectiveOutputXmlDocumentInnerXml.Replace(match.Value, "<AppProgramArgs></AppProgramArgs>");
			}
			Assert.AreEqual(expectedOutputXmlDocument.InnerXml, effectiveOutputXmlDocumentInnerXml);
		}
	}
}
