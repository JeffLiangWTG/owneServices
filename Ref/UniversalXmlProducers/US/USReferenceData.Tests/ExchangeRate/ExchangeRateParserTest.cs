using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.RefDbRepo.USReferenceData.Business.ExchangeRate;
using CargoWise.RefDbRepo.USReferenceData.Services;
using HtmlAgilityPack;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	[SetCulture("en-AU")]
	public class ExchangeRateParserTest
	{
		const string TestUrl = "https://www.cbp.gov//service.test";

		[TestCase]
		public void TestGetValidPublishDate()
		{
			var today = new DateTime(2023, 08, 11);
			var publishDateFromUrl = new DateTime(2023, 11, 08);
			var publishDateFromFile = new DateTime(2023, 08, 11);
			var result = ParserHelper.GetValidPublishDate(today, publishDateFromUrl, publishDateFromFile);
			Assert.AreEqual(publishDateFromFile, result);

			publishDateFromUrl = new DateTime(2023, 08, 11);
			publishDateFromFile = new DateTime(2023, 11, 08);
			result = ParserHelper.GetValidPublishDate(today, publishDateFromUrl, publishDateFromFile);
			Assert.AreEqual(publishDateFromUrl, result);

			today = new DateTime(2023, 02, 01);
			publishDateFromUrl = new DateTime(2023, 03, 02);
			publishDateFromFile = new DateTime(2023, 02, 03);
			result = ParserHelper.GetValidPublishDate(today, publishDateFromUrl, publishDateFromFile);
			Assert.AreEqual(publishDateFromFile, result);

			today = new DateTime(2023, 02, 06);
			publishDateFromUrl = new DateTime(2023, 02, 03);
			publishDateFromFile = new DateTime(2023, 03, 02);
			result = ParserHelper.GetValidPublishDate(today, publishDateFromUrl, publishDateFromFile);
			Assert.AreEqual(publishDateFromUrl, result);

			today = new DateTime(2023, 03, 09);
			publishDateFromFile = new DateTime(2023, 09, 03);
			var exception = Assert.Throws<InvalidOperationException>(() => ParserHelper.GetValidPublishDate(today, null, publishDateFromFile));
			Assert.AreEqual("Can not find a valid publish date from page or file.", exception.Message);

			publishDateFromUrl = new DateTime(2023, 09, 03);
			exception = Assert.Throws<InvalidOperationException>(() => ParserHelper.GetValidPublishDate(today, publishDateFromUrl, null));
			Assert.AreEqual("Can not find a valid publish date from page or file.", exception.Message);

			exception = Assert.Throws<InvalidOperationException>(() => ParserHelper.GetValidPublishDate(today, null, null));
			Assert.AreEqual("Can not find the publish date from page and file.", exception.Message);
		}

		[TestCase]
		public void TestParseToXml_PublishFormatIsMDDYYYY()
		{
			AssertParseToXmlCore(@"Daily_Currency_Notice_1-03-2024.xlsx", "Daily_Currency_Notice_1-03-2024.xlsx /t", new DateTime(2024, 01, 03), new DateTime(2024, 01, 03), true);
		}

		[TestCase]
		public void TestParseToXml_NoPublishDateFromExcelFileName()
		{
			// Due to the date parse logic inside \USReferenceData.Business\ExchangeRate\ExcelParser.cs, line 77-99:
			// When input date string in test excel file date is "01/03/2024"
			// It can be parsed to both "2024-01-03" and "2024-03-01" and then the final result will be the "which date is closer to today"
			// So the test input date has to be setup as below (exactly matching the logic to be tested) to pass whenever the test run
			var dateTime3rdJan = new DateTime(2024, 01, 03);
			var dateTime1stMar = new DateTime(2024, 03, 01);
			var today = DateTime.UtcNow.AddHours(-5).Date;

			// Below line is to match date parse implementation:
			// https://devops.wisetechglobal.com/wtg/RefDataRepo/_git/RefDataRepo?path=%2FUniversalXmlProducers%2FUS%2FUSReferenceData.Business%2FExchangeRate%2FExcelParser.cs&version=GBmaster&line=79&lineEnd=80&lineStartColumn=1&lineEndColumn=1&lineStyle=plain&_a=contents
			var mockPublishDate = new[] { dateTime3rdJan, dateTime1stMar }.OrderBy(x => Math.Abs(today.Subtract(x).Days)).FirstOrDefault();

			AssertParseToXmlCore(@"Daily_Currency_Notice.xlsx", "Daily_Currency_Notice.xlsx /t", mockPublishDate, mockPublishDate, true);
		}

		[TestCase]
		public void TestParseToXml_PublishDate()
		{
			AssertParseNormalFileToXml(new DateTime(2021, 4, 26), new DateTime(2021, 4, 26));
		}

		[TestCase]
		public void TestParseToXml_PublishYesterday()
		{
			AssertParseNormalFileToXml(new DateTime(2021, 4, 27), new DateTime(2021, 4, 26));
		}

		[TestCase]
		public void TestParseToXml_PublishTomorrow()
		{
			AssertParseNormalFileToXml(new DateTime(2021, 4, 25), new DateTime(2021, 4, 26));
			AssertParseNormalFileToXml(new DateTime(2021, 4, 26), new DateTime(2021, 4, 26), false);
		}

		[TestCase]
		public void TestParseToXml_PublishDateFallback()
		{
			var now = new DateTime(2021, 4, 26);
			var nodeText = @"Daily Foreign Currency Exchange Rate Multipliers 04-26-2021 (Excel Format)";

			AssertParseBrokenFileToXml(nodeText, now, now);
		}

		[TestCase]
		public void TestParseToXml_NoPublishDate()
		{
			var now = new DateTime(2021, 4, 26);
			var nodeText = @"Wrong Year 04-25-021";

			var exception = Assert.Throws<InvalidOperationException>(() => AssertParseBrokenFileToXml(nodeText, now, now));
			Assert.AreEqual("Can not find the publish date from page and file.", exception.Message);
		}

		[TestCase]
		public void TestParseToXml_PublishAtFriday()
		{
			var now = new DateTime(2021, 4, 23);
			var nodeText = @"Friday 04-23-2021";
			var fileName = @"Daily_Currency_Notice_WithoutPublishDate.xlsx";
			var inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"ExchangeRate\TestFiles\Input\{fileName}");
			var outputPath = @"..\UXmlFiles";
			var publishDateArr = new DateTime[] { now, now.AddDays(1), now.AddDays(2) };
			for (int i = 0; i < publishDateArr.Length; i++)
			{
				var outputXmlPath = GetOutPutXMLPath(outputPath, publishDateArr[i]);
				if (File.Exists(outputXmlPath))
				{
					File.Delete(outputXmlPath);
				}
			}

			var mock = new Mock<IDownLoadService>();
			mock.Setup(c => c.FindNodes(It.IsAny<string>(), It.IsAny<Func<HtmlNode, bool>>())).Returns(() =>
			{
				var document = new HtmlDocument();
				var node = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "a" };

				node.Attributes.Add("href", TestUrl + $@"/{fileName}");
				node.Attributes.Add("title", fileName);
				node.InnerHtml = nodeText;

				return new[] { node };
			});
			mock.Setup(c => c.DownloadData(It.IsAny<string>())).Returns(File.ReadAllBytes(inputPath));

			var parser = new ExchangeRateParser(TestUrl, @"https://www.cbp.gov", outputPath, mock.Object)
			{
				GetNowInUnitedStates = () => now,
			};
			var result = parser.ParseToXml();

			Assert.AreEqual("Processed 36 exchange rates.", result);

			var expectedXmlDoc = new XmlDocument();
			expectedXmlDoc.Load(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ExchangeRate\TestFiles\Output\"), @"RefExchangeRateZZ_US_CBP_CUS.xml"));
			var expectedXmlWithPattern = expectedXmlDoc.InnerXml;
			for (int i = 0; i < publishDateArr.Length; i++)
			{
				var publishDate = publishDateArr[i];
				var actualDocument = new XmlDocument();
				var outputXmlPath = GetOutPutXMLPath(outputPath, publishDate);
				actualDocument.Load(outputXmlPath);
				var expectedXml = expectedXmlWithPattern.Replace("@EndDate", publishDate.ToString("s")).Replace("@StartDate", publishDate.ToString("s"));
				Assert.AreEqual(expectedXml, actualDocument.InnerXml);
				File.Delete(outputXmlPath);
			}
		}

		[TestCase]
		public void TestParseToXml_PublishDateIsEqualToStartDate()
		{
			var now = new DateTime(2021, 4, 22);
			var publishDate = new DateTime(2021, 4, 20);

			var nodeText = @"Legacy Date 04-20-2021";

			AssertParseBrokenFileToXml(nodeText, now, publishDate);
		}

		[TestCase]
		public void TestParseToXml_PublishInWeekend()
		{
			var now = new DateTime(2021, 4, 25);
			var nodeText = @"Daily Foreign Currency Exchange Rate Multipliers 04-25-2021.xlsx";

			var exception = Assert.Throws<InvalidOperationException>(() => AssertParseBrokenFileToXml(nodeText, now, now));
			Assert.AreEqual($"US CBP publish exchange rates at {now}(Sunday), please dont check that Friday exchange rate is set to cover Friday, plus Saturday and Sunday.", exception.Message);
		}

		void AssertParseNormalFileToXml(DateTime now, DateTime exchangeRateDate, bool shouldPublishXml = true)
		{
			AssertParseToXmlCore(@"Daily_Currency_Notice_04-26-2021.xlsx", string.Empty, now, exchangeRateDate, shouldPublishXml);
		}

		void AssertParseBrokenFileToXml(string nodeText, DateTime now, DateTime publishDate)
		{
			AssertParseToXmlCore(@"Daily_Currency_Notice_WithoutPublishDate.xlsx", nodeText, now, publishDate, true);
		}

		void AssertParseToXmlCore(string fileName, string nodeText, DateTime now, DateTime publishDate, bool shouldPublishXml)
		{
			var mock = new Mock<IDownLoadService>();

			mock.Setup(c => c.FindNodes(It.IsAny<string>(), It.IsAny<Func<HtmlNode, bool>>())).Returns(() =>
			{
				var document = new HtmlDocument();
				var node = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "a" };

				node.Attributes.Add("href", TestUrl + $@"/{fileName}");
				node.Attributes.Add("title", fileName);
				node.InnerHtml = nodeText;

				return new[] { node };
			});

			var inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"ExchangeRate\TestFiles\Input\{fileName}");
			mock.Setup(c => c.DownloadData(It.IsAny<string>())).Returns(File.ReadAllBytes(inputPath));

			var outputPath = @"..\UXmlFiles";

			var outputXmlPath = Path.Combine(outputPath, $"{publishDate:yyyyMMdd}_RefExchangeRateZZ_US_CBP_CUS_Excel.xml");

			if (File.Exists(outputXmlPath))
			{
				File.Delete(outputXmlPath);
			}

			var parser = new ExchangeRateParser(TestUrl, @"https://www.cbp.gov", outputPath, mock.Object)
			{
				GetNowInUnitedStates = () => now,
			};

			var result = parser.ParseToXml();

			if (shouldPublishXml)
			{
				Assert.AreEqual("Processed 36 exchange rates.", result);

				var actualDocument = new XmlDocument();
				actualDocument.Load(outputXmlPath);

				var expectedXmlDoc = new XmlDocument();
				expectedXmlDoc.Load(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ExchangeRate\TestFiles\Output\"), @"RefExchangeRateZZ_US_CBP_CUS.xml"));

				var expectedXml = expectedXmlDoc.InnerXml
					.Replace("@EndDate", publishDate.ToString("s"))
					.Replace("@StartDate", publishDate.ToString("s"));

				Assert.AreEqual(expectedXml, actualDocument.InnerXml);

				File.Delete(outputXmlPath);
			}

			Assert.AreEqual($"No exchange rate data updated for {publishDate:MM-dd-yyyy}.", parser.ParseToXml());
			Assert.IsTrue(!File.Exists(outputXmlPath), "Not to generate the xml again since no data updated");
		}

		[TestCase]
		public void TestParseToXmlWithEmptyUrl()
		{
			var inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"ExchangeRate\TestFiles\Input");
			var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ExchangeRate\TestFiles\Output");

			var actualFile = string.Empty;
			var expectFile = Path.Combine(outputPath, @"ExchangeRateParserSaved05-20-2022 15-30.html");

			var mock = new Mock<IDownLoadService>();
			mock.Setup(c => c.FindNodes(It.IsAny<string>(), It.IsAny<Func<HtmlNode, bool>>())).Returns(() => null);

			mock.Setup(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Returns<string, string>((uri, path) =>
			{
				actualFile = path;
				return true;
			});

			var parser = new ExchangeRateParser(inputPath + @"\SourcePage.html", @"https://www.cbp.gov", outputPath, mock.Object)
			{
				GetNowInUnitedStates = () => new DateTime(2022, 05, 20, 15, 30, 00),
			};

			var exception = Assert.Throws<InvalidOperationException>(() => parser.ParseToXml());
			Assert.AreEqual($"{parser.GetNowInUnitedStates().ToString("MM-dd-yyyy", CultureInfo.CreateSpecificCulture("en-US"))} : Can not find the excel file (url: ) for downloading.", exception.Message);
			Assert.AreEqual(expectFile, actualFile, "When we can't find a url address, we save the webpage content.");
		}

		[TestCase]
		public void TestIsUrlForExchangeRateExcelFile()
		{
			var hrefWithCorrectTypoAndType = "<a href=\"https://www.cbp.gov/sites/default/files/assets/documents/2023-Aug/Daily_Currency_Notice.xlsx\" title=\"Daily_Currency_Notice.xlsx\">Daily Currency Notice.xlsx</a>";
			var node = HtmlNode.CreateNode(hrefWithCorrectTypoAndType);

			var isUrlForExchangeRateExcelFile = ParserHelper.IsUrlForExchangeRateExcelFile(node);
			Assert.AreEqual(true, isUrlForExchangeRateExcelFile, "It should return true if we can find a .xlsx file from the html.");

			var hrefWithWrongTypo = "<a href=\"https://www.cbp.gov/sites/default/files/assets/documents/2023-Aug/Daily_Cureency_Notice.xlsx\" title=\"Daily_Currency_Notice.xlsx\">Daily Currency Notice.xlsx</a>";
			node = HtmlNode.CreateNode(hrefWithWrongTypo);

			isUrlForExchangeRateExcelFile = ParserHelper.IsUrlForExchangeRateExcelFile(node);
			Assert.AreEqual(true, isUrlForExchangeRateExcelFile, "It should return true if we can find a .xlsx file from the html.");

			var hrefWithWrongType = "<a href=\"https://www.cbp.gov/sites/default/files/assets/documents/2023-Aug/Daily_Cureency_Notice.pdf\" title=\"Daily_Currency_Notice.xlsx\">Daily Currency Notice.pdf</a>";
			node = HtmlNode.CreateNode(hrefWithWrongType);

			isUrlForExchangeRateExcelFile = ParserHelper.IsUrlForExchangeRateExcelFile(node);
			Assert.AreEqual(false, isUrlForExchangeRateExcelFile, "It should return false if we cannot find a .xlsx file from the html.");
		}

		[TestCase]
		public void TestThrowExceptionWhenFindTwoOrMoreXLSXFiles()
		{
			var inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"ExchangeRate\TestFiles\Input");
			var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ExchangeRate\TestFiles\Output");

			var mock = new Mock<IDownLoadService>();
			mock.Setup(c => c.FindNodes(It.IsAny<string>(), It.IsAny<Func<HtmlNode, bool>>())).Returns(() =>
			{
				var document = new HtmlDocument();
				var node = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "a" };
				node.Attributes.Add("href", TestUrl + @"/Daily_Currency_Notice_08-30-2023.xlsx");

				var node2 = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "a" };
				node2.Attributes.Add("href", TestUrl + @"/Daily_Currency_Notice_08-31-2023.xlsx");

				return new[] { node, node2 };
			});
			var parser = new ExchangeRateParser(inputPath + @"\SourcePage.html", @"https://www.cbp.gov", outputPath, mock.Object)
			{
				GetNowInUnitedStates = () => new DateTime(2023, 09, 01),
			};

			var exception = Assert.Throws<InvalidOperationException>(() => parser.ParseToXml());
			Assert.AreEqual("Find two or more xlsx files from the url.", exception.Message);
		}

		[TestCase]
		public void TestParseToXmlWithFailedDownload()
		{
			var excelFileUrl = TestUrl + @"/Daily_Currency_Notice.xlsx";
			var inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"ExchangeRate\TestFiles\Input");
			var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ExchangeRate\TestFiles\Output");

			var mock = new Mock<IDownLoadService>();
			mock.Setup(c => c.FindNodes(It.IsAny<string>(), It.IsAny<Func<HtmlNode, bool>>())).Returns(() =>
			{
				var document = new HtmlDocument();
				var node = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "a" };

				node.Attributes.Add("href", TestUrl + @"/Daily_Currency_Notice.xlsx");

				return new[] { node };
			});

			var actualFile = string.Empty;
			var expectFile = Path.Combine(outputPath, @"ExchangeRateParserSaved05-20-2022 15-30.html");

			mock.Setup(c => c.DownloadData(It.IsAny<string>())).Returns(() => { return null; });
			mock.Setup(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Returns<string, string>((uri, path) =>
			{
				actualFile = path;
				return true;
			});

			var parser = new ExchangeRateParser(inputPath + @"\SourcePage.html", @"https://www.cbp.gov", outputPath, mock.Object)
			{
				GetNowInUnitedStates = () => new DateTime(2022, 05, 20, 15, 30, 00),
			};

			var result = parser.ParseToXml();

			Assert.AreEqual($"Download excel file from {excelFileUrl} failed.", result);
			Assert.AreEqual(expectFile, actualFile, "When we can't find a url address, we save the webpage content.");
		}

		[TestCase]
		public void TestParseToXmlWithNoneRates()
		{
			var mock = new Mock<IDownLoadService>();
			mock.Setup(c => c.FindNodes(It.IsAny<string>(), It.IsAny<Func<HtmlNode, bool>>())).Returns(() =>
			{
				var document = new HtmlDocument();
				var node = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "a" };

				node.Attributes.Add("href", TestUrl + @"/Daily_Currency_Notice.xlsx");

				return new[] { node };
			});

			var inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"ExchangeRate\TestFiles\Input\Empty_Daily_Currency_Notice.xlsx");
			mock.Setup(c => c.DownloadData(It.IsAny<string>())).Returns(File.ReadAllBytes(inputPath));

			var parser = new ExchangeRateParser(TestUrl, @"https://www.cbp.gov", @"\Output", mock.Object)
			{
				GetNowInUnitedStates = () => new DateTime(2021, 04, 20),
			};

			var result = parser.ParseToXml();

			Assert.AreEqual("No exchange rate data defined in the file.", result);
		}

		[TestCase]
		public void TestParseToXmlWithHref()
		{
			var parser = GetParser("Test", @"/Daily_Currency_Notice.xlsx");

			var result = parser.ParseToXml();
			Assert.IsTrue(Regex.IsMatch(result, @"Processed -?\d+ exchange rates."));
		}

		[TestCase]
		public void TestParseToXmlWithInnerText()
		{
			var parser = GetParser("Currency Exchange Rate", @"/Test.xlsx");

			var result = parser.ParseToXml();
			Assert.IsTrue(Regex.IsMatch(result, @"Processed -?\d+ exchange rates."));
		}

		ExchangeRateParser GetParser(string innerHtml, string href)
		{
			var inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"ExchangeRate\TestFiles\Input\Daily_Currency_Notice_04-26-2021.xlsx");
			var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ExchangeRate\TestFiles\Output");

			var document = new HtmlDocument();
			var aNode = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "a", InnerHtml = innerHtml };
			aNode.Attributes.Add("href", TestUrl + href);
			aNode.Attributes.Add("title", @"Daily_Currency_Notice_07-13-2022.xlsx");

			var mock = new Mock<IDownLoadService>();

			mock.Setup(c => c.FindNodes(It.IsAny<string>(), It.Is<Func<HtmlNode, bool>>(pre => pre(aNode)))).Returns(() =>
			{
				return new[] { aNode };
			});

			mock.Setup(c => c.DownloadData(It.IsAny<string>())).Returns(File.ReadAllBytes(inputPath));

			var parser = new ExchangeRateParser(TestUrl, @"https://www.cbp.gov", outputPath, mock.Object)
			{
				GetNowInUnitedStates = () => new DateTime(2022, 07, 14, 15, 30, 00),
			};
			return parser;
		}

		string GetOutPutXMLPath(string outputPath, DateTime publishDate) => Path.Combine(outputPath, $"{publishDate:yyyyMMdd}_RefExchangeRateZZ_US_CBP_CUS_Excel.xml");

		[SetUp]
		protected void Setup()
		{
			if (File.Exists(ExchangeRateParser.LogFilePath))
			{
				File.Delete(ExchangeRateParser.LogFilePath);
			}
		}
	}
}
