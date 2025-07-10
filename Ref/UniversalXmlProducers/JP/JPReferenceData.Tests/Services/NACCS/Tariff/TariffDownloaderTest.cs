using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.JPReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	[NonParallelizable]
	sealed class TariffDownloaderTest
	{
		[TestCase("0501000006")]
		[TestCase("0502900005")]
		[TestCase("0402211316")]
		[TestCase("080510000†2")]
		public void TestTranslateTariff(string tariff)
		{
			var pageName = $"TariffPageJP{tariff}.html";
			var tariffUrl = $"https://www.kanzei.or.jp/statistical/tariff/detail/index/j/{tariff}";
			mockSourceProvider.Setup(x => x.GetPageAsync(tariffUrl)).Returns(Task.FromResult(GetContent(pageName)));

			if (tariff.Contains('†'))
			{
				pageName = $"TariffPageAdditional{tariff}.html";
				tariffUrl = $"https://www.kanzei.or.jp/statistical/popcontent/naccs/tariff/{tariff}";
				mockSourceProvider.Setup(x => x.GetPageAsync(tariffUrl)).Returns(Task.FromResult(GetContent(pageName)));
			}

			pageName = $"TariffPage{tariff}.html";
			tariffUrl = $"https://www.kanzei.or.jp/statistical/tariff/detail/index/e/{tariff}";
			mockSourceProvider.Setup(x => x.GetPageAsync(tariffUrl)).Returns(Task.FromResult(GetContent(pageName)));

			var tariffScraper = new TariffForTest(tariffUrl, mockSourceProvider.Object, tariff);

			if (File.Exists(tariffScraper.FullPath))
			{
				File.Delete(tariffScraper.FullPath);
			}

			tariffScraper.Download();
			Assert.AreEqual(GetContent($"outputTariff{tariff}.csv"), File.ReadAllText(tariffScraper.FullPath));
		}

		[TestCase("01", "04")]
		public void TestTranslateChapter(string section, string chapter)
		{
			var pageName = "ChapterPage.html";
			mockSourceProvider.Setup(x => x.GetPageAsync(AppConfig.NACCS.CodeLists.JPNACCSTariffHomePageDownloadUrl)).Returns(Task.FromResult(GetContent(pageName)));
			var url = $"https://www.kanzei.or.jp/statistical/tariff/headline/hs4dig/e/{chapter}";
			pageName = $"TablePage{chapter}Excerpt.html";
			mockSourceProvider.Setup(x => x.GetPageAsync(url)).Returns(Task.FromResult(GetContent(pageName)));

			foreach (var code in new[] { "0402101103", "0402101210", "040210129†", "0402102116", "040210212†", "0402102164", "040210217†" })
			{
				pageName = $"TariffPage{code}.html";
				url = $"https://www.kanzei.or.jp/statistical/tariff/detail/index/e/{code}";
				mockSourceProvider.Setup(x => x.GetPageAsync(url)).Returns(Task.FromResult(GetContent(pageName)));

				pageName = $"TariffPageJP{code}.html";
				url = $"https://www.kanzei.or.jp/statistical/tariff/detail/index/j/{code}";
				mockSourceProvider.Setup(x => x.GetPageAsync(url)).Returns(Task.FromResult(GetContent(pageName)));

				if (code.Contains('†'))
				{
					pageName = $"TariffPageAdditional{code}.html";
					url = $"https://www.kanzei.or.jp/statistical/popcontent/naccs/tariff/{code}";
					mockSourceProvider.Setup(x => x.GetPageAsync(url)).Returns(Task.FromResult(GetContent(pageName)));
				}
			}

			var chapterScraper = new ChapterForTest($"https://www.kanzei.or.jp/statistical/tariff/headline/hs4dig/e/{chapter}", mockSourceProvider.Object, int.Parse(section, CultureInfo.InvariantCulture), int.Parse(chapter, CultureInfo.InvariantCulture));

			if (File.Exists(chapterScraper.FullPath))
			{
				File.Delete(chapterScraper.FullPath);
			}

			chapterScraper.Download();
			Assert.AreEqual(GetContent($"outputChapter{chapter}.csv"), File.ReadAllText(chapterScraper.FullPath));
		}

		[Test]
		public void TestTranslateSite()
		{
			var pageName = "ChapterPageExcerpt.html";
			var url = AppConfig.NACCS.CodeLists.JPNACCSTariffHomePageDownloadUrl;
			mockSourceProvider.Setup(x => x.GetPageAsync(url)).Returns(Task.FromResult(GetContent(pageName)));
			mockSourceProvider.Setup(x => x.GetAsync(AppConfig.NACCS.CodeLists.JPNACCS98TariffDownloadUrl)).Returns(() => Task.FromResult(GetStream("hcode-98.csv")));
			mockSourceProvider.Setup(x => x.GetAsync(AppConfig.NACCS.CodeLists.JPNACCS99TariffDownloadUrl)).Returns(() => Task.FromResult(GetStream("shogaku1.csv")));

			var chapter = "04";
			url = $"https://www.kanzei.or.jp/statistical/tariff/headline/hs4dig/e/{chapter}";
			pageName = $"TablePage{chapter}Excerpt.html";
			mockSourceProvider.Setup(x => x.GetPageAsync(url)).Returns(Task.FromResult(GetContent(pageName)));

			chapter = "08";
			pageName = $"TablePage{chapter}Excerpt.html";
			url = $"https://www.kanzei.or.jp/statistical/tariff/headline/hs4dig/e/{chapter}";
			mockSourceProvider.Setup(x => x.GetPageAsync(url)).Returns(Task.FromResult(GetContent(pageName)));

			foreach (var code in new[] { "0402101103", "0402101210", "040210129†", "0402102116", "040210212†", "0402102164", "040210217†", "080510000†2" })
			{
				pageName = $"TariffPage{code}.html";
				url = $"https://www.kanzei.or.jp/statistical/tariff/detail/index/e/{code}";
				mockSourceProvider.Setup(x => x.GetPageAsync(url)).Returns(Task.FromResult(GetContent(pageName)));

				pageName = $"TariffPageJP{code}.html";
				url = $"https://www.kanzei.or.jp/statistical/tariff/detail/index/j/{code}";
				mockSourceProvider.Setup(x => x.GetPageAsync(url)).Returns(Task.FromResult(GetContent(pageName)));

				if (code.Contains('†'))
				{
					pageName = $"TariffPageAdditional{code}.html";
					url = $"https://www.kanzei.or.jp/statistical/popcontent/naccs/tariff/{code}";
					mockSourceProvider.Setup(x => x.GetPageAsync(url)).Returns(Task.FromResult(GetContent(pageName)));
				}
			}

			var tariffScraper = new SiteForTest(AppConfig.NACCS.CodeLists.JPNACCSTariffHomePageDownloadUrl, mockSourceProvider.Object);

			if (File.Exists(tariffScraper.FullPath))
			{
				File.Delete(tariffScraper.FullPath);
			}

			tariffScraper.Download();

			var expectedText = GetContent($"outputJapanNaccsSite.csv");
			var actualText = File.ReadAllText(tariffScraper.FullPath);

			Assert.AreEqual(expectedText, actualText);
			Assert.AreEqual(true, tariffScraper.AlreadyDownloaded);

			mockSourceProvider.Setup(x => x.IsParallelMode).Returns(true);
			tariffScraper = new SiteForTest(AppConfig.NACCS.CodeLists.JPNACCSTariffHomePageDownloadUrl, mockSourceProvider.Object);

			if (File.Exists(tariffScraper.FullPath))
			{
				File.Delete(tariffScraper.FullPath);
			}

			tariffScraper.Download();

			expectedText = GetContent($"outputJapanNaccsSite.csv");
			actualText = File.ReadAllText(tariffScraper.FullPath);

			Assert.AreEqual(expectedText, actualText);
		}

		Stream GetStream(string fileName)
		{
			var path = Path.Combine(dirPath, "TestFiles", "JPNACCSTariff", fileName);
			return new MemoryStream(File.ReadAllBytes(path));
		}

		string GetContent(string fileName)
		{
			var path = Path.Combine(dirPath, "TestFiles", "JPNACCSTariff", fileName);
			return File.ReadAllText(path);
		}

		readonly string dirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		readonly Mock<IWebSourceProvider> mockSourceProvider = new Mock<IWebSourceProvider>();
	}
}
