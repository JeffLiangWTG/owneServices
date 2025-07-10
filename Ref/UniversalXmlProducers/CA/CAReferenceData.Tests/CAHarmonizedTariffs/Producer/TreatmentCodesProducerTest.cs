using System;
using System.Globalization;
using System.IO;
using System.Xml;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	[TestFixture]
	public class TreatmentCodesProducerTest
	{
		[Test]
		public void TestQueryDataAndParseToXMLFile()
		{
			using (var ttCodeExpectedResultXml = TestHelper.GetTestInputFile("20230302CAHarmonizedTTCode.xml"))
			{
				var publicationTime = new DateTime(2023, 03, 02, 00, 00, 00);
				var exportFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Output\" + publicationTime.Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "CAHarmonizedTTCode.xml");
				new TreatmentCodesProducer(publicationTime,exportFilePath).QueryDataAndParseToXMLFile();

				var file = new FileInfo(exportFilePath);
				Assert.IsTrue(file.Exists);
				var ttCodeXmlDoc = new XmlDocument();
				ttCodeXmlDoc.Load(exportFilePath);
				var ttCodeExpectedXmlDoc = new XmlDocument();
				ttCodeExpectedXmlDoc.Load(ttCodeExpectedResultXml);
				Assert.AreEqual(ttCodeXmlDoc.InnerXml, ttCodeExpectedXmlDoc.InnerXml);
				file.Delete();
			}
		}
	}
}
