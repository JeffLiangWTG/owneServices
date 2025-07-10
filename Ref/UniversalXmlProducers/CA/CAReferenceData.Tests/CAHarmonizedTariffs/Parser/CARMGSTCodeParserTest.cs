using System;
using System.IO;
using System.Xml;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	[TestFixture]
	public class CARMGSTCodeParserTest
	{
		[Test]
		public void TestPopulateCAGSTXML()
		{
			var gstExportFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Output\CustomsHarmonized_CAGSTCODE.xml");
			using (var gstExpectedResultXml = TestHelper.GetTestInputFile("CustomsHarmonized_CAGSTCODE.xml"))
			{
				var gstParser = new CARMGSTCodeParser(WorkingFolder);
				gstParser.ParseXMLfilesIntoXML(gstExportFilePath, new DateTime(2024, 12, 17));
				var gstActualXmlDoc = new XmlDocument();
				gstActualXmlDoc.Load(gstExportFilePath);
				var gstExpectedXmlDoc = new XmlDocument();
				gstExpectedXmlDoc.Load(gstExpectedResultXml);
				Assert.AreEqual(TestHelper.RemoveReturnCharactersIncaseEnvironmentNewlineDifference(gstExpectedXmlDoc.InnerXml), TestHelper.RemoveReturnCharactersIncaseEnvironmentNewlineDifference(gstActualXmlDoc.InnerXml));
			}
		}

		public string WorkingFolder => _workingFolder ?? (_workingFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Input\CATariffDataSource"));
		string _workingFolder;

		[SetUp]
		public void SetUp()
		{
			if (!Directory.Exists(WorkingFolder))
			{
				Directory.CreateDirectory(WorkingFolder);
			}
		}
	}
}
