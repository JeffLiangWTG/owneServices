using System;
using System.IO;
using System.Xml;
using CargoWise.RefDbRepo.USReferenceData.Business.ExemptionCode;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	sealed class ExemptionCodePDFParserTest
	{
		[TestCase]
		public void TestReadPDFAndExportXML()
		{
			var filePath = Path.Combine(inputPath, @"DDTC ITAR Exemption Codes.pdf");
			var parser = new PDFParser(filePath, @"C:\RefDataRepo\Bin\UniversalXMLProducers\UXmlFiles\", new DateTime(2023, 01, 01));

			var result = parser.ReadPDFAndExportXML();
			Assert.AreEqual("Processed 64 Exemption Codes.", result);

			Assert.IsTrue(File.Exists(actualXmlPath), "Should be able to parse and generate xml file.");

			using (var actualXmlStream = new FileStream(actualXmlPath, FileMode.Open))
			using (var expectedXmlStream = new FileStream(Path.Combine(outputPath, @"Exemption_Codes.xml"), FileMode.Open))
			{
				var actualXml = new XmlDocument();
				actualXml.Load(actualXmlStream);

				var expectedXml = new XmlDocument();
				expectedXml.Load(expectedXmlStream);

				Assert.AreEqual(expectedXml.InnerXml, actualXml.InnerXml);
			}
			File.Delete(actualXmlPath);
		}

		[TestCase]
		public void TestReadPDFAndExportXMLWhenNoExemptionCode()
		{
			var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ExemptionCode\TestFiles");
			var filePath = Path.Combine(dir, @"Input\Exemption Code does not exist in the file.pdf");

			var parser = new PDFParser(filePath, outputPath, new DateTime(2023, 01, 01));

			var result = parser.ReadPDFAndExportXML();
			Assert.AreEqual("No Exemption Number data defined in the file.", result);

			Assert.IsTrue(!File.Exists(actualXmlPath), "Should not be able to parse and generate xml file.");
		}

		[SetUp]
		public void Setup()
		{
			inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"ExemptionCode\TestFiles\Input");
			outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"ExemptionCode\TestFiles\Output");
			actualXmlPath = @"C:\RefDataRepo\Bin\UniversalXMLProducers\UXmlFiles\Exemption_Codes.xml";
			if (File.Exists(actualXmlPath))
			{
				File.Delete(actualXmlPath);
			}
		}
		string inputPath;
		string outputPath;
		string actualXmlPath;
	}
}
