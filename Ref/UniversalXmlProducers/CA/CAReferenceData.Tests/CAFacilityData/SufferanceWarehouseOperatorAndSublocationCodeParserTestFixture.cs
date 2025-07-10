using System;
using System.IO;
using System.Xml;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAFacilityData;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	[TestFixture]
	public class SufferanceWarehouseOperatorAndSublocationCodeParserTestFixture
	{
		[Test]
		public void SufferanceWarehouseOperatorAndSublocationCodeParserTest()
		{
			var outputFile = "SufferanceWarehouseOperatorAndSublocationCodeOutputXML.xml";
			var exportFilepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Output\" + outputFile + "");
			using (var expectedResultXml = TestHelper.GetTestInputFile("SufferanceWarehouseOperatorAndSublocationCodeExpected.xml"))
			using (var sufferanceWarehouseOperatorAndSublocationCode = TestHelper.GetTestInputFile("SufferanceWarehouseOperatorAndSublocationCode.html"))
			using (var sufferanceWarehouseOperatorAndSublocationCodeStreamReader = new StreamReader(sufferanceWarehouseOperatorAndSublocationCode))
			{
				var supportingDocument = new SufferanceWarehouseOperatorAndSublocationCodeParser(new WebpageTableToDataTable(sufferanceWarehouseOperatorAndSublocationCodeStreamReader.ReadToEnd()), exportFilepath);
				supportingDocument.ExportXml();

				var xmlDoc = new XmlDocument();
				xmlDoc.Load(exportFilepath);
				var expectedXmlDoc = new XmlDocument();
				expectedXmlDoc.Load(expectedResultXml);

				Assert.AreEqual(expectedXmlDoc.InnerXml, xmlDoc.InnerXml);
			}
		}
	}
}
