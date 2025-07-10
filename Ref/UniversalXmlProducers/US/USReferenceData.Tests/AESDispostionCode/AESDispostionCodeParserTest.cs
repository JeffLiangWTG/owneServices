using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.USReferenceData.Business;
using CargoWise.RefDbRepo.USReferenceData.Business.AESDispostionCode;
using CargoWise.RefDbRepo.USReferenceData.Services;
using HtmlAgilityPack;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	sealed class AESDispostionCodeParserTest
	{
		[Test]
		public void TestNothingNewPublished()
		{
			var parser = GetAESDispostionCodeParser(true);

			var result = parser.DownloadPDFAndExportXML(Path.Combine(inputPath, @"ACE Appendix A - Commodity Filing Response Messages 05212024_508C.pdf"));
			Assert.AreEqual("Processed 726 AES Dispostion Codes.\r\n", result);

			result = parser.DownloadPDFAndExportXML(Path.Combine(inputPath, @"ACE Appendix A - Commodity Filing Response Messages 05212024_508C.pdf"));
			Assert.AreEqual("US AES Dispostion Code: Nothing new published since last process. Skip processing this time.", result);

			checker.ClearData();
			result = parser.DownloadPDFAndExportXML(Path.Combine(inputPath, @"ACE Appendix A - Commodity Filing Response Messages 05212024_508C.pdf"));
			Assert.AreEqual("Processed 726 AES Dispostion Codes.\r\n", result);
		}

		[Test]
		public void TestParseToXmlWithFullData() => TestParseToXmlWithDifferFormatDate(true);

		[Test]
		public void TestParseToXmlWithAbbreviatedData() => TestParseToXmlWithDifferFormatDate(false);

		public void TestParseToXmlWithDifferFormatDate(bool isFullDate)
		{
			var parser = GetAESDispostionCodeParser(isFullDate);

			var result = parser.DownloadPDFAndExportXML(Path.Combine(inputPath, @"ACE Appendix A - Commodity Filing Response Messages 05212024_508C.pdf"));
			Assert.AreEqual("Processed 726 AES Dispostion Codes.\r\n", result);

			var path = Path.Combine(@"C:\RefDataRepo\Bin\UniversalXMLProducers\UXmlFiles\", "AES_Dispostio_Codes.xml");
			Assert.IsTrue(File.Exists(path), "Should be able to parse and generate xml file.");

			File.Delete(path);
		}

		AESDispostionCodeParser GetAESDispostionCodeParser(bool isFullDate)
		{
			var mock = new Mock<IDownLoadService>();
			mock.Setup(c => c.FindNode(It.IsAny<string>(), It.IsAny<Func<HtmlNode, bool>>())).Returns(() =>
			{
				var document = new HtmlDocument();
				var parentNode = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "a" };

				var node = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "a" };
				node.Attributes.Add("href", "Test:ACE%20Appendix%20A%20-%20Commodity%20Filing%20Response%20Messages%2005212024_508C.pdf");
				node.InnerHtml = "ACE AESTIR Appendix A - Commodity Filing Response Messages";
				node.SetParent(parentNode);

				var dateNode = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "span" };
				dateNode.InnerHtml = isFullDate ? "December 29, 2022" : "Dec 29, 2022";

				parentNode.AppendChild(dateNode);

				return node;
			});

			mock.Setup(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Returns(() => true);

			var parser = new AESDispostionCodeParser(@"Test.html", @"Test:", @"C:\RefDataRepo\Bin\UniversalXMLProducers\UXmlFiles\", mock.Object)
			{
				GetNowInUnitedStates = () => new DateTime(2022, 05, 20, 15, 30, 00),
			};
			return parser;
		}

		[Test]
		public void TestFailedDownloadFile()
		{
			var mock = new Mock<IDownLoadService>();
			mock.Setup(c => c.FindNode(It.IsAny<string>(), It.IsAny<Func<HtmlNode, bool>>())).Returns(() =>
			{
				var document = new HtmlDocument();
				var parentNode = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "div" };

				var node = new HtmlNode(HtmlNodeType.Element, document, 1) { Name = "a" };
				node.Attributes.Add("href", "Test:ACE%20Appendix%20A%20-%20Commodity%20Filing%20Response%20Messages%2005212024_508C.pdf");
				node.InnerHtml = "ACE AESTIR Appendix A - Commodity Filing Response Messages";
				node.SetParent(parentNode);

				var dateNode = new HtmlNode(HtmlNodeType.Element, document, 2) { Name = "span" };
				dateNode.InnerHtml = "December 29, 2022";

				parentNode.AppendChild(dateNode);

				return node;
			});

			mock.Setup(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Returns(() => false);

			var parser = new AESDispostionCodeParser(@"Test:", @"Test.html", outputPath, mock.Object)
			{
				GetNowInUnitedStates = () => new DateTime(2022, 05, 20, 15, 30, 00),
			};

			var result = parser.DownloadPDFAndExportXML();
			Assert.AreEqual("Download pdf file from Test:ACE%20Appendix%20A%20-%20Commodity%20Filing%20Response%20Messages%2005212024_508C.pdf failed.", result);
		}

		[Test]
		public void TestParseToXmlWithEmptyUrl()
		{
			var mock = new Mock<IDownLoadService>();
			mock.Setup(c => c.FindNode(It.IsAny<string>(), It.IsAny<Func<HtmlNode, bool>>())).Returns(() => null);

			var parser = new AESDispostionCodeParser(inputPath + @"\Test.html", @"Test:", outputPath, mock.Object)
			{
				GetNowInUnitedStates = () => new DateTime(2022, 05, 20, 15, 30, 00),
			};

			var exception = Assert.Throws<InvalidOperationException>(() => parser.DownloadPDFAndExportXML());
			Assert.AreEqual($"{parser.GetNowInUnitedStates().ToString("MM-dd-yyyy", CultureInfo.CreateSpecificCulture("en-US"))} : Can not find the last modified date (url: Test:).", exception.Message);
		}

		[Test]
		public void TestParseToXmlWithEmptyPublishDate()
		{
			var mock = new Mock<IDownLoadService>();
			mock.Setup(c => c.FindNode(It.IsAny<string>(), It.IsAny<Func<HtmlNode, bool>>())).Returns(() =>
			{
				var node = new HtmlNode(HtmlNodeType.Element, new HtmlDocument(), 0) { Name = "a" };

				node.Attributes.Add("href", "Test:ACE%20Appendix%20A%20-%20Commodity%20Filing%20Response%20Messages%2005212024_508C.pdf");
				node.InnerHtml = "ACE AESTIR Appendix A - Commodity Filing Response Messages";

				return node;
			});

			var parser = new AESDispostionCodeParser(@"Test:", @"Test.html", outputPath, mock.Object)
			{
				GetNowInUnitedStates = () => new DateTime(2022, 05, 20, 15, 30, 00),
			};

			var exception = Assert.Throws<InvalidOperationException>(() => parser.DownloadPDFAndExportXML());
			Assert.AreEqual($"{parser.GetNowInUnitedStates().ToString("MM-dd-yyyy", CultureInfo.CreateSpecificCulture("en-US"))} : Can not find the last modified date (url: Test.html).", exception.Message);
		}

		[SetUp]
		public void Setup()
		{
			inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"AESDispostionCode\TestFiles\Input");
			outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"AESDispostionCode\TestFiles\Output");
			checker = new LocalFileStorage("US AES Dispostion Code");
			checker.ClearData();
		}
		string inputPath;
		string outputPath;
		LocalFileStorage checker;
	}
}
