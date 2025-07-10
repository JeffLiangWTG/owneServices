using System;
using System.IO;
using System.Xml;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAFacilityData;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	[TestFixture]
	public class CustomsOfficeGenericSublocationCodeParserTestFixture
	{
		[Test]
		public void CustomsOfficeGenericSublocationCodeParserTest()
		{
			var outputFile = "CustomsOfficeGenericSublocationCodeOutputXML.xml";
			var exportFilepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Output\" + outputFile + "");
			using (var expectedResultXml = TestHelper.GetTestInputFile("CustomsOfficeGenericSublocationCodeExpected.xml"))
			using (var customsOfficeGenericSublocationCode = TestHelper.GetTestInputFile("CustomsOfficeGenericSublocationCode.html"))
			using (var customsOfficeGenericSublocationCodeStream = new StreamReader(customsOfficeGenericSublocationCode))
			{
				var supportingDocument = new CustomsOfficeGenericSublocationCodeParser(new WebpageTableToDataTable(customsOfficeGenericSublocationCodeStream.ReadToEnd()), exportFilepath);
				supportingDocument.ExportXml();
				var xmlDoc = new XmlDocument();
				xmlDoc.Load(exportFilepath);
				var expectedXmlDoc = new XmlDocument();
				expectedXmlDoc.Load(expectedResultXml);
				Assert.AreEqual(xmlDoc.InnerXml, expectedXmlDoc.InnerXml);
			}
		}
	}
}
