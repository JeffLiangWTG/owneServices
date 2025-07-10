using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	[TestFixture]
	public class TariffDataParserTest
	{
		const string parentPath = "CAHarmonizedTariff";

		[Test]
		[Property("DAT:CapabilityRequirements", (int)RefDbRepoMachineCapabilityRequirements.CanConnectToOdbc)]
		public void TestParseRefCusTariffsIntoXML()
		{
			var path = Path.GetTempFileName();
			var excelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Input\CAHarmonizedTariff\all-pga-programs.xlsx");
			using (var stream = TestHelper.GetTestInputFile(string.Format(CultureInfo.InvariantCulture, "{0}.01-99-2023-1-eng.zip", parentPath)))
			using (var tariffExpectedResultXml = TestHelper.GetTestInputFile("20230302CAHarmonizedTariff.xml"))
			using (var file = File.Create(path))
			{
				stream.Seek(0, SeekOrigin.Begin);
				stream.CopyTo(file);
				stream.Close();
				file.Close();

				var publicationTime = new DateTime(2023, 03, 02, 00, 00, 00);
				var exportFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Output\" + publicationTime.Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "CAHarmonizedTariff.xml");
				var parser = new TariffDataParser(path, excelPath);
				parser.ParseRefCusTariffsIntoXML(exportFilePath, publicationTime);

				var file1 = new FileInfo(exportFilePath);
				Assert.IsTrue(file1.Exists);
				var tariffXmlDoc = new XmlDocument();
				tariffXmlDoc.Load(exportFilePath);
				var tariffExpectedXmlDoc = new XmlDocument();
				tariffExpectedXmlDoc.Load(tariffExpectedResultXml);
				Assert.AreEqual(tariffXmlDoc.InnerXml, tariffExpectedXmlDoc.InnerXml);
				file1.Delete();
			}
		}

		[Test]
		[Property("DAT:CapabilityRequirements", (int)RefDbRepoMachineCapabilityRequirements.CanConnectToOdbc)]
		public void TestParseConditionData()
		{
			var path = Path.GetTempFileName();
			var excelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Input\CAHarmonizedTariff\all-pga-programs - 2.xlsx");
			using (var stream = TestHelper.GetTestInputFile(string.Format(CultureInfo.InvariantCulture, "{0}.01-99-2023-2-eng.zip", parentPath)))
			using (var tariffExpectedResultXml = TestHelper.GetTestInputFile("20230302CAHarmonizedTariff - 2.xml"))
			using (var file = File.Create(path))
			{
				stream.Seek(0, SeekOrigin.Begin);
				stream.CopyTo(file);
				stream.Close();
				file.Close();

				var publicationTime = new DateTime(2023, 03, 02, 00, 00, 00);
				var exportFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Output\" + publicationTime.Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "CAHarmonizedTariff.xml");
				var parser = new TariffDataParser(path, excelPath);
				parser.ParseRefCusTariffsIntoXML(exportFilePath, publicationTime);

				var file1 = new FileInfo(exportFilePath);
				Assert.IsTrue(file1.Exists);
				var tariffXmlDoc = new XmlDocument();
				tariffXmlDoc.Load(exportFilePath);
				var tariffExpectedXmlDoc = new XmlDocument();
				tariffExpectedXmlDoc.Load(tariffExpectedResultXml);
				Assert.AreEqual(tariffXmlDoc.InnerXml, tariffExpectedXmlDoc.InnerXml);
				file1.Delete();
			}
		}

		[Test]
		public void TestParseTradeGroupData()
		{
			var path = Path.GetTempFileName();
			using (var stream = TestHelper.GetTestInputFile(string.Format(CultureInfo.InvariantCulture, "{0}.countries-pays-2-eng.pdf", parentPath)))
			using (var tradeExpectedResultXml = TestHelper.GetTestInputFile("20230302CAHarmonizedTradeGroup.xml"))
			using (var file = File.Create(path))
			{
				stream.Seek(0, SeekOrigin.Begin);
				stream.CopyTo(file);
				stream.Close();
				file.Close();

				var publicationTime = new DateTime(2023, 03, 02, 00, 00, 00);
				var exportFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Output\" + publicationTime.Date.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "CAHarmonizedTradeGroup.xml");
				var parser = new TradeGroupDataParser(publicationTime, exportFilePath, helper.Object, "{0}");
				parser.ParseTradeGroupData(path);

				var file1 = new FileInfo(exportFilePath);
				Assert.IsTrue(file1.Exists);
				var tradeXmlDoc = new XmlDocument();
				tradeXmlDoc.Load(exportFilePath);
				var tradeExpectedXmlDoc = new XmlDocument();
				tradeExpectedXmlDoc.Load(tradeExpectedResultXml);
				Assert.AreEqual(tradeXmlDoc.InnerXml, tradeExpectedXmlDoc.InnerXml);
				file1.Delete();
			}
		}


		[SetUp]
		public void SetUp()
		{
			helper = new Mock<IHttpClientHelper>();
			helper.Setup(x => x.GetMatchedEntityCodesAsync(It.IsAny<string>(), It.IsAny<string[]>()))
				.Returns<string, string[]>((url, inputs) =>
				{
					return Task.FromResult(new[] { "GB", "US", "UY", "UZ", "VU", "TW", "VE", "VN", "FO", "US", "ML", "YE", "ZM", "ZW" });
				}
			);
		}
		Mock<IHttpClientHelper> helper;
	}
}
