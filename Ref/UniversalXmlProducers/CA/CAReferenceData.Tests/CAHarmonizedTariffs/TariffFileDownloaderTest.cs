using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	[TestFixture]
	public class TariffFileDownloaderTest
	{
		const string parentPath = "CAHarmonizedTariff";

		[Test]
		public void TestGetTradeGroupEffectiveDateAndUrl()
		{
			using (var stream1 = TestHelper.GetTestInputFile(string.Format(CultureInfo.InvariantCulture, "{0}.CAHarmonizedTariff.html", parentPath)))
			using (var stream2 = TestHelper.GetTestInputFile(string.Format(CultureInfo.InvariantCulture, "{0}.Customs Tariff chapter-by-chapter.html", parentPath)))
			using (var html1Stream = new StreamReader(stream1))
			using (var html2Stream = new StreamReader(stream2))
			{
				var html1 = html1Stream.ReadToEnd();
				var html2 = html2Stream.ReadToEnd();
				var downLoadPdfPath = Path.Combine(TestHelper.GetCurrentFolder(), @"TestFiles\countries-pays-2-eng.pdf");
				html2 = html2.Replace(@"countries-pays-2-eng.pdf", downLoadPdfPath);

				var downloader = new TariffFileDownloader("", "", "");
				var result = downloader.GetTradeGroupEffectiveDateAndUrl(html1, html2);

				Assert.AreEqual(new DateTime(2023, 02, 21), result.Item1);
				Assert.AreEqual(downLoadPdfPath, result.Item2);
			}
		}

		[Test]
		public void TestGetLastEditDateAndAccessDbUrl()
		{
			using (var stream = TestHelper.GetTestInputFile(string.Format(CultureInfo.InvariantCulture, "{0}.CAHarmonizedTariff.html", parentPath)))
			using (var htmlStream = new StreamReader(stream))
			{
				var html = htmlStream.ReadToEnd();
				var downLoadZipPath = Path.Combine(TestHelper.GetCurrentFolder(), @"TestFiles\01-99-2023-1-eng.zip");
				html = html.Replace(@"01-99-2023-3-eng.zip", downLoadZipPath);

				var downloader = new TariffFileDownloader("", "", "");
				var result = downloader.GetLastEditDateAndAccessDbUrl(html);

				Assert.AreEqual(new DateTime(2023, 03, 01), result.Item1);
				Assert.AreEqual(downLoadZipPath, result.Item2);
			}
		}

		[Test]
		public void TestGetHTMLStringFailed()
		{
			var downloader = new TariffFileDownloader("", "{0} Test URL", "");
			try
			{
				downloader.GetLastEditDateAndAccessDbUrl();
			}
			catch (Exception ex)
			{
				Assert.IsTrue(ex.Message.Contains($"The webpage download failed. URLs: {DateTime.Now.Year} Test URL and {DateTime.Now.Year + 1} Test URL"));
			}
			
		}

		[Test]
		public void TestGetConditionExcelUrl()
		{
			using (var stream = TestHelper.GetTestInputFile(string.Format(CultureInfo.InvariantCulture, "{0}.CACondition.html", parentPath)))
			using (var html = new StreamReader(stream))
			{
				var downloader = new TariffFileDownloader("", "", "");
				var result = downloader.GetConditionExcelModifiedDateAndUrl("", html.ReadToEnd());
				Assert.AreEqual(new DateTime(2023, 05, 25), result.Item1);
				Assert.AreEqual("https://Test/all-pga-programs-cbsa-sw-matching-criteria-1.xlsx", result.Item2);
			}
		}


		[Test]
		public void TestFailedDownloadFile()
		{
			var downloader = new TariffFileDownloader("", "", "");
			try
			{
				downloader.DownloadFile("test url", "");
			}
			catch (Exception ex)
			{
				Assert.IsTrue(ex.Message.Contains("File downloads failed. URL:"));
			}
		}
	}
}
