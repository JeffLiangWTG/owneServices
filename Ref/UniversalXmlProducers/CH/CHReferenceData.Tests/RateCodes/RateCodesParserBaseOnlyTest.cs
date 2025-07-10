using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.RefDbRepo.CHReferenceData.Business.RateCodes;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.RateCodes
{
	[TestFixture]
	internal class RateCodesParserBaseOnlyTest
	{
		[TestCase]
		public void TestNoImportWithSameDate()
		{
			using (var outputFile = new TemporaryOutputFile(@"RateCodes\TestNoImportIfAlreadyImported.xml"))
			{
				GetRateCodesParser("TestFiles.Input.TestLogFile_1.xml").ConvertToRefXML(outputFile.FullPath);
				Assert.That(File.Exists(outputFile.FullPath), Is.True, "When no logfile exists");
				File.Delete(outputFile.FullPath);
				GetRateCodesParser("TestFiles.Input.TestLogFile_1.xml").ConvertToRefXML(outputFile.FullPath);
				Assert.That(File.Exists(outputFile.FullPath), Is.False, "When same date downloaded");
				GetRateCodesParser("TestFiles.Input.TestLogFile_2.xml").ConvertToRefXML(outputFile.FullPath);
				Assert.That(File.Exists(outputFile.FullPath), Is.True, "When another date downloaded");
			}
		}

		[TestCase]
		public void TestMultipleRecordsForSameCode()
		{
			using (var outputFile = new TemporaryOutputFile(@"ImportTariffs\masterDataDownload.xml"))
			{
				GetRateCodesParser("TestFiles.Input.TestMultipleRecordsForSameCode.xml").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				assertRateCode("150", "150de2", "150fr2", "150it2", "150en2");
				assertRateCode("151", "151de2", "151fr2", "151it2", "151en2");
				assertRateCode("160", "160de2", "160fr2", "160it2", "160en2");

				void assertRateCode(string rateCode, string descriptionDe, string descriptionFr, string descriptionIt, string descriptionEn)
				{
					var rates = outputDoc.XPathSelectElements($@"//RefCusRateCode[ZY1_RateCode='{rateCode}']");
					Assert.That(rates.Count(), Is.EqualTo(1), $"{rateCode} rate should exist once");
					var rate = rates.First();
					Assert.That(rate.XPathSelectElement("ZY1_Description")?.Value, Is.EqualTo(descriptionDe), $"{rateCode} ZY1_Description");
					Assert.That(rate.XPathSelectElement("RefCusRateCodeLanguage[ZXC_ZX6_NKLanguage='FR']/ZXC_Description")?.Value, Is.EqualTo(descriptionFr), $"{rateCode} ZXC_Description FR");
					Assert.That(rate.XPathSelectElement("RefCusRateCodeLanguage[ZXC_ZX6_NKLanguage='IT']/ZXC_Description")?.Value, Is.EqualTo(descriptionIt), $"{rateCode} ZXC_Description IT");
					Assert.That(rate.XPathSelectElement("RefCusRateCodeLanguage[ZXC_ZX6_NKLanguage='EN']/ZXC_Description")?.Value, Is.EqualTo(descriptionEn), $"{rateCode} ZXC_Description EN");
				}
			}
		}

		RateCodesParser GetRateCodesParser(string inputResource)
		{
			DownloadResult masterDataDownload = new DownloadResult
			{
				Content = typeof(RateCodesParserBaseOnlyTest).GetTestArray(inputResource),
			};

			return new FeeRateCodesParser(masterDataDownload);
		}

		[SetUp]
		public void SetUp()
		{
			TestHelper.DeleteLogFiles();
		}

		[TearDown]
		public void TearDown()
		{
			TestHelper.DeleteLogFiles();
		}
	}
}
