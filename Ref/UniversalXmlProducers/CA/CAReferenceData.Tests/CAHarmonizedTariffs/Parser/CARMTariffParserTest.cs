using System;
using System.IO;
using System.Xml;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	[TestFixture]
	public class CARMTariffParserTest : TestWithApplicationTestConfig
	{
		[Test]
		public void TestPopulateCATariffXML()
		{
			var workingFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Input\CATariffDataSource");
			var tariffExportFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Output\CustomsHarmonizedTariff_CARM.xml");
			var conditionFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Input\CAHarmonizedTariff\all-pga-programs.xlsx");
			using (var tariffExpectedResultXml = TestHelper.GetTestInputFile("CustomsHarmonizedTariff_CARM.xml"))
			{
				var tariffParser = new CARMTariffParser(workingFolder, conditionFileName);
				tariffParser.ParseXMLfilesIntoXML(tariffExportFilePath, null, new DateTime(2021, 12, 08));
				var tariffActualXmlDoc = new XmlDocument();
				tariffActualXmlDoc.Load(tariffExportFilePath);
				var tariffExpectedXmlDoc = new XmlDocument();
				tariffExpectedXmlDoc.Load(tariffExpectedResultXml);
				Assert.AreEqual(RemoveReturnCharactersIncaseEnvironmentNewlineDifference(tariffExpectedXmlDoc.InnerXml), RemoveReturnCharactersIncaseEnvironmentNewlineDifference(tariffActualXmlDoc.InnerXml));
			}
		}

		[Test]
		public void TestPopulateCATariffXML_SpecialCase()
		{
			var workingFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Input\CATariffDataSource_SpecialCase");
			var tariffExportFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Output\CustomsHarmonizedTariff_CARM_SpecialCase.xml");
			var conditionFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Input\CAHarmonizedTariff\all-pga-programs.xlsx");
			using (var tariffExpectedResultXml = TestHelper.GetTestInputFile("CustomsHarmonizedTariff_CARM_SpecialCase.xml"))
			{
				var tariffParser = new CARMTariffParser(workingFolder, conditionFileName);
				tariffParser.ParseXMLfilesIntoXML(tariffExportFilePath, new DateTime(2024, 12, 1), new DateTime(2024, 12, 23));
				var tariffActualXmlDoc = new XmlDocument();
				tariffActualXmlDoc.Load(tariffExportFilePath);
				var tariffExpectedXmlDoc = new XmlDocument();
				tariffExpectedXmlDoc.Load(tariffExpectedResultXml);
				Assert.AreEqual(RemoveReturnCharactersIncaseEnvironmentNewlineDifference(tariffExpectedXmlDoc.InnerXml), RemoveReturnCharactersIncaseEnvironmentNewlineDifference(tariffActualXmlDoc.InnerXml));
			}
		}

		[Test]
		public void TestPopulateCATariffXML_PGAConditionPatch()
		{
			var workingFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Input\CATariffDataSource_PGAConditionPatch");
			var tariffExportFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Output\CustomsHarmonizedTariff_CARM_PGAConditionPatch.xml");
			var conditionFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Input\CAHarmonizedTariff\all-pga-programs.xlsx");
			using (var tariffExpectedResultXml = TestHelper.GetTestInputFile("CustomsHarmonizedTariff_CARM_PGAConditionPatch.xml"))
			{
				var tariffParser = new CARMTariffParser(workingFolder, conditionFileName);
				tariffParser.ParseXMLfilesIntoXML(tariffExportFilePath, new DateTime(2024, 12, 1), new DateTime(2024, 12, 23));
				var tariffActualXmlDoc = new XmlDocument();
				tariffActualXmlDoc.Load(tariffExportFilePath);
				var tariffExpectedXmlDoc = new XmlDocument();
				tariffExpectedXmlDoc.Load(tariffExpectedResultXml);
				Assert.AreEqual(RemoveReturnCharactersIncaseEnvironmentNewlineDifference(tariffExpectedXmlDoc.InnerXml), RemoveReturnCharactersIncaseEnvironmentNewlineDifference(tariffActualXmlDoc.InnerXml));
			}
		}

		string RemoveReturnCharactersIncaseEnvironmentNewlineDifference(string input)
		{
			return input.Replace("\r", "").Replace("\n", "");
		}
	}
}
