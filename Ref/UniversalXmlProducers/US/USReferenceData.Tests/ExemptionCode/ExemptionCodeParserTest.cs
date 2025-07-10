using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.USReferenceData.Business;
using CargoWise.RefDbRepo.USReferenceData.Business.ExemptionCode;
using CargoWise.RefDbRepo.USReferenceData.Services;
using HtmlAgilityPack;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	public class ExemptionCodeParserTest
	{
		[Test]
		public void TestNothingNewPublished()
		{
			var parser = GetExemptionCodeParser(true);

			var result = parser.DownloadPDFAndExportXML(Path.Combine(inputPath, @"DDTC ITAR Exemption Codes.pdf"));
			Assert.AreEqual("Processed 64 Exemption Codes.", result);

			result = parser.DownloadPDFAndExportXML(Path.Combine(inputPath, @"DDTC ITAR Exemption Codes.pdf"));
			Assert.AreEqual("US DDTC ITAR Exemption Codes: Nothing new published since last process. Skip processing this time.", result);

			checker.ClearData();
			result = parser.DownloadPDFAndExportXML(Path.Combine(inputPath, @"DDTC ITAR Exemption Codes.pdf"));
			Assert.AreEqual("Processed 64 Exemption Codes.", result);
		}

		[Test]
		public void TestParseToXmlWithFullData() => TestParseToXmlWithDifferFormatDate(true);

		[Test]
		public void TestParseToXmlWithAbbreviatedData() => TestParseToXmlWithDifferFormatDate(false);

		public void TestParseToXmlWithDifferFormatDate(bool isFullDate)
		{
			var parser = GetExemptionCodeParser(isFullDate);
			var result = parser.DownloadPDFAndExportXML(Path.Combine(inputPath, @"DDTC ITAR Exemption Codes.pdf"));

			Assert.AreEqual("Processed 64 Exemption Codes.", result);

			var path = Path.Combine(@"C:\RefDataRepo\Bin\UniversalXMLProducers\UXmlFiles\", "Exemption_Codes.xml");
			Assert.IsTrue(File.Exists(path), "Should be able to parse and generate xml file.");

			File.Delete(path);
		}

		ExemptionCodeParser GetExemptionCodeParser(bool isFullDate)
		{
			var mock = new Mock<IDownLoadService>();
			mock.Setup(c => c.FindNode(It.IsAny<string>(), It.IsAny<Func<HtmlNode, bool>>())).Returns(() =>
			{
				var document = new HtmlDocument();
				var parentNode = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "a" };

				var node = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "a" };
				node.Attributes.Add("href", "Test:ACE%20Appendix%20O%20%E2%80%93%20DDTC%20ITAR%20Exemption%20Codes_932024%20_0%20%281%29_508.pdf");
				node.InnerHtml = "ACE Appendix O – DDTC ITAR Exemption Codes";
				node.SetParent(parentNode);

				var dateNode = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "span" };
				dateNode.InnerHtml = isFullDate ? "December 29, 2022" : "Dec 29, 2022";

				parentNode.AppendChild(dateNode);

				return node;
			});

			mock.Setup(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Returns(() => true);

			var parser = new ExemptionCodeParser(@"DDTC ITAR Exemption Codes.html", @"Test:", @"C:\RefDataRepo\Bin\UniversalXMLProducers\UXmlFiles\", mock.Object)
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
				node.Attributes.Add("href", "Test:ACE%20Appendix%20O%20%E2%80%93%20DDTC%20ITAR%20Exemption%20Codes_932024%20_0%20%281%29_508.pdf");
				node.InnerHtml = "ACE Appendix O – DDTC ITAR Exemption Codes";
				node.SetParent(parentNode);

				var dateNode = new HtmlNode(HtmlNodeType.Element, document, 2) { Name = "span" };
				dateNode.InnerHtml = "December 29, 2022";

				parentNode.AppendChild(dateNode);

				return node;
			});

			mock.Setup(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Returns(() => false);

			var parser = new ExemptionCodeParser(@"Test:", @"Test.html", outputPath, mock.Object)
			{
				GetNowInUnitedStates = () => new DateTime(2022, 05, 20, 15, 30, 00),
			};

			var result = parser.DownloadPDFAndExportXML();
			Assert.AreEqual("Download pdf file from Test:ACE%20Appendix%20O%20%E2%80%93%20DDTC%20ITAR%20Exemption%20Codes_932024%20_0%20%281%29_508.pdf failed.", result);
		}

		[Test]
		public void TestParseToXmlWithEmptyUrl()
		{
			var mock = new Mock<IDownLoadService>();
			mock.Setup(c => c.FindNode(It.IsAny<string>(), It.IsAny<Func<HtmlNode, bool>>())).Returns(() => null);

			var parser = new ExemptionCodeParser(inputPath + @"\Test.html", @"Test:", outputPath, mock.Object)
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

				node.Attributes.Add("href", "Test:ACE%20Appendix%20O%20%E2%80%93%20DDTC%20ITAR%20Exemption%20Codes_932024%20_0%20%281%29_508.pdf");
				node.InnerHtml = "ACE Appendix O – DDTC ITAR Exemption Codes";

				return node;
			});

			var parser = new ExemptionCodeParser(@"Test:", @"Test.html", outputPath, mock.Object)
			{
				GetNowInUnitedStates = () => new DateTime(2022, 05, 20, 15, 30, 00),
			};

			var exception = Assert.Throws<InvalidOperationException>(() => parser.DownloadPDFAndExportXML());
			Assert.AreEqual($"{parser.GetNowInUnitedStates().ToString("MM-dd-yyyy", CultureInfo.CreateSpecificCulture("en-US"))} : Can not find the last modified date (url: Test.html).", exception.Message);
		}

		[SetUp]
		public void Setup()
		{
			inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"ExemptionCode\TestFiles\Input");
			outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"ExemptionCode\TestFiles\Output");
			checker = new LocalFileStorage("US DDTC ITAR Exemption Codes");
			checker.ClearData();
		}
		string inputPath;
		string outputPath;
		LocalFileStorage checker;
	}
}
