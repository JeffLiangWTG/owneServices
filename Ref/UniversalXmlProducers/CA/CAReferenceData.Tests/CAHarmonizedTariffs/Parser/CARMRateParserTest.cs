using System;
using System.IO;
using System.Xml;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	[TestFixture]
	public class CARMRateParserTest : TestWithApplicationTestConfig
	{
		[Test]
		public void TestPopulateCARateXML()
		{
			var workingFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Input\CATariffDataSource");
			var rateTypeOutputFile = "CustomsHarmonizedTariffRate_CARM.xml";
			var rateTypeExportFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Output\" + rateTypeOutputFile + "");
			using (var rateTypeExpectedResultXml = TestHelper.GetTestInputFile("CustomsHarmonizedTariffRate_CARM.xml"))
			{
				var rateParser = new CARMRateParser(workingFolder);
				rateParser.ParseXMLfilesIntoXML(rateTypeExportFilePath, new DateTime(2021, 12, 08));
				var rateTypeXmlDoc = new XmlDocument();
				rateTypeXmlDoc.Load(rateTypeExportFilePath);
				var rateTypeExpectedXmlDoc = new XmlDocument();
				rateTypeExpectedXmlDoc.Load(rateTypeExpectedResultXml);
				Assert.AreEqual(rateTypeXmlDoc.InnerXml, rateTypeExpectedXmlDoc.InnerXml);
			}
		}
	}
}
