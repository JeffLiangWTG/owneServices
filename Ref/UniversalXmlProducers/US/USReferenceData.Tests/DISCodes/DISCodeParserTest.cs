using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.USReferenceData.Business;
using CargoWise.RefDbRepo.USReferenceData.Business.DISCodes;
using CargoWise.RefDbRepo.USReferenceData.Services;
using HtmlAgilityPack;
using Moq;
using NUnit.Framework;
using static CargoWise.RefDbRepo.USReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	public class DISCodeParserTest
	{
		[Test]
		public void TestNothingNewPublished()
		{
			var parser = GetDISCodeParser(true);

			var result = parser.DownloadPDFAndExportXML(Path.Combine(inputPath, @"ACEDISXMLSimple_11062024.pdf"));
			Assert.AreEqual("Processed 326 DIS codes.", result);

			result = parser.DownloadPDFAndExportXML(Path.Combine(inputPath, @"ACEDISXMLSimple_11062024.pdf"));
			Assert.AreEqual("US DIS Code: Nothing new published since last process. Skip processing this time.", result);

			checker.ClearData();
			result = parser.DownloadPDFAndExportXML(Path.Combine(inputPath, @"ACEDISXMLSimple_11062024.pdf"));
			Assert.AreEqual("Processed 326 DIS codes.", result);
		}

		[Test]
		public void TestParseToXmlWithFullData() => TestParseToXmlWithDifferFormatDate(true);

		[Test]
		public void TestParseToXmlWithAbbreviatedData() => TestParseToXmlWithDifferFormatDate(false);

		public void TestParseToXmlWithDifferFormatDate(bool isFullDate)
		{
			var parser = GetDISCodeParser(isFullDate);

			var result = parser.DownloadPDFAndExportXML(Path.Combine(inputPath, @"ACEDISXMLSimple_11062024.pdf"));
			Assert.AreEqual("Processed 326 DIS codes.", result);

			var path = Path.Combine(@"C:\RefDataRepo\Bin\UniversalXMLProducers\UXmlFiles\", "DIS_Codes.xml");
			Assert.IsTrue(File.Exists(path), "Should be able to parse and generate xml file.");

			File.Delete(path);
		}

		DISCodeParser GetDISCodeParser(bool isFullDate)
		{
			var mock = new Mock<IDownLoadService>();
			mock.Setup(c => c.FindNode(It.IsAny<string>(), It.IsAny<Func<HtmlNode, bool>>())).Returns(() =>
			{
				var document = new HtmlDocument();
				var parentNode = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "a" };

				var node = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "a" };
				node.Attributes.Add("href", "Test:ACEDISXMLSimple_11062024.pdf");
				node.Attributes.Add("title", "ACEDISXMLSimple_11062024.pdf");
				node.InnerHtml = "ACE DIS XML Implementation Guide";
				node.SetParent(parentNode);

				var dateNode = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "span" };
				dateNode.InnerHtml = isFullDate ? "December 29, 2022" : "Dec 29, 2022";

				parentNode.AppendChild(dateNode);

				return node;
			});

			mock.Setup(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Returns(() => true);

			var parser = new DISCodeParser(@"Test.html", @"Test:", @"C:\RefDataRepo\Bin\UniversalXMLProducers\UXmlFiles\", mock.Object)
			{
				GetNowInUnitedStates = () => new DateTime(2022, 05, 20, 15, 30, 00),
			};
			return parser;
		}

		[Test]
		public void TestFiledParsingPDFWithoutTableHeader()
		{
			var mock = new Mock<IDownLoadService>();
			mock.Setup(c => c.FindNode(It.IsAny<string>(), It.IsAny<Func<HtmlNode, bool>>())).Returns(() =>
			{
				var document = new HtmlDocument();
				var parentNode = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "a" };

				var node = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "a" };
				node.Attributes.Add("href", "Test:ACEDISXMLSimpple.pdf");
				node.Attributes.Add("title", "ACEDISXMLSimpple.pdf");
				node.InnerHtml = "ACE DIS XML Implementation Guide";
				node.SetParent(parentNode);

				var dateNode = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "span" };
				dateNode.InnerHtml = "March 19, 2024";

				parentNode.AppendChild(dateNode);

				return node;
			});

			mock.Setup(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Returns(() => true);

			var parser = new DISCodeParser(@"Test.html", @"Test:", @"C:\RefDataRepo\Bin\UniversalXMLProducers\UXmlFiles\", mock.Object)
			{
				GetNowInUnitedStates = () => new DateTime(2024, 03, 19, 15, 30, 00),
			};

			var result = parser.DownloadPDFAndExportXML(Path.Combine(inputPath, @"ACEDISXMLSimple.pdf"));
			Assert.AreEqual("Processed 45 DIS codes.", result);

			var path = Path.Combine(@"C:\RefDataRepo\Bin\UniversalXMLProducers\UXmlFiles\", "DIS_Codes.xml");
			Assert.IsTrue(File.Exists(path), "Should be able to parse and generate xml file.");

			File.Delete(path);
		}


		[Test]
		public void TestFiledParsingPDFWithoutTableHeaderON14Jun2024()
		{
			var mock = new Mock<IDownLoadService>();
			mock.Setup(c => c.FindNode(It.IsAny<string>(), It.IsAny<Func<HtmlNode, bool>>())).Returns(() =>
			{
				var document = new HtmlDocument();
				var parentNode = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "a" };

				var node = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "a" };
				node.Attributes.Add("href", "Test:ACEDISXMLSimple_11062024.pdf");
				node.Attributes.Add("title", "ACEDISXMLSimple_11062024.pdf");
				node.InnerHtml = "ACE DIS XML Implementation Guide";
				node.SetParent(parentNode);

				var dateNode = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "span" };
				dateNode.InnerHtml = "March 19, 2024";

				parentNode.AppendChild(dateNode);

				return node;
			});

			mock.Setup(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Returns(() => true);

			var parser = new DISCodeParser(@"Test.html", @"Test:", @"C:\RefDataRepo\Bin\UniversalXMLProducers\UXmlFiles\", mock.Object)
			{
				GetNowInUnitedStates = () => new DateTime(2024, 03, 19, 15, 30, 00),
			};

			var result = parser.DownloadPDFAndExportXML(Path.Combine(inputPath, @"ACEDISXMLSimple_11062024.pdf"));
			Assert.AreEqual("Processed 326 DIS codes.", result);

			var path = Path.Combine(@"C:\RefDataRepo\Bin\UniversalXMLProducers\UXmlFiles\", "DIS_Codes.xml");
			Assert.IsTrue(File.Exists(path), "Should be able to parse and generate xml file.");

			File.Delete(path);
		}

		[Test]
		public void TestFiledParsingPDFWithoutTableHeaderON28Jun2024()
		{
			var mock = new Mock<IDownLoadService>();
			mock.Setup(c => c.FindNode(It.IsAny<string>(), It.IsAny<Func<HtmlNode, bool>>())).Returns(() =>
			{
				var document = new HtmlDocument();
				var parentNode = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "a" };

				var node = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "a" };
				node.Attributes.Add("href", "Test:ACEDISXMLSimple_28062024.pdf");
				node.Attributes.Add("title", "ACEDISXMLSimple_28062024.pdf");
				node.InnerHtml = "ACE DIS XML Implementation Guide";
				node.SetParent(parentNode);

				var dateNode = new HtmlNode(HtmlNodeType.Element, document, 0) { Name = "span" };
				dateNode.InnerHtml = "June 28, 2024";

				parentNode.AppendChild(dateNode);

				return node;
			});

			mock.Setup(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Returns(() => true);

			var parser = new DISCodeParser(@"Test.html", @"Test:", @"C:\RefDataRepo\Bin\UniversalXMLProducers\UXmlFiles\", mock.Object)
			{
				GetNowInUnitedStates = () => new DateTime(2024, 06, 28, 15, 30, 00),
			};

			var result = parser.DownloadPDFAndExportXML(Path.Combine(inputPath, @"ACEDISXMLSimple_28062024.pdf"));
			Assert.AreEqual("Processed 327 DIS codes.", result);

			var path = Path.Combine(@"C:\RefDataRepo\Bin\UniversalXMLProducers\UXmlFiles\", "DIS_Codes.xml");
			Assert.IsTrue(File.Exists(path), "Should be able to parse and generate xml file.");

			File.Delete(path);
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
				node.Attributes.Add("href", "Test:ACE%20DIS%20XML%20Implementation%20Guide_21Dec2022_508c.pdf");
				node.Attributes.Add("title", "ACE DIS XML Implementation Guide_21Dec2022_508c.pdf");
				node.InnerHtml = "ACE DIS XML Implementation Guide";
				node.SetParent(parentNode);

				var dateNode = new HtmlNode(HtmlNodeType.Element, document, 2) { Name = "span" };
				dateNode.InnerHtml = "December 29, 2022";

				parentNode.AppendChild(dateNode);

				return node;
			});

			mock.Setup(c => c.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Returns(() => false);

			var parser = new DISCodeParser(@"Test:", @"Test.html", outputPath, mock.Object)
			{
				GetNowInUnitedStates = () => new DateTime(2022, 05, 20, 15, 30, 00),
			};

			var result = parser.DownloadPDFAndExportXML();
			Assert.AreEqual("Download pdf file from Test:ACE%20DIS%20XML%20Implementation%20Guide_21Dec2022_508c.pdf failed.", result);
		}

		[Test]
		public void TestParseToXmlWithEmptyUrl()
		{
			var mock = new Mock<IDownLoadService>();
			mock.Setup(c => c.FindNode(It.IsAny<string>(), It.IsAny<Func<HtmlNode, bool>>())).Returns(() => null);

			var parser = new DISCodeParser(inputPath + @"\ACE DIS XML Implementation Guide.html", @"Test:", outputPath, mock.Object)
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

				node.Attributes.Add("href", "Test:ACE%20DIS%20XML%20Implementation%20Guide_21Dec2022_508c.pdf");
				node.Attributes.Add("title", "ACE DIS XML Implementation Guide_21Dec2022_508c.pdf");
				node.InnerHtml = "ACE DIS XML Implementation Guide";

				return node;
			});

			var parser = new DISCodeParser(@"Test:", @"Test.html", outputPath, mock.Object)
			{
				GetNowInUnitedStates = () => new DateTime(2022, 05, 20, 15, 30, 00),
			};

			var exception = Assert.Throws<InvalidOperationException>(() => parser.DownloadPDFAndExportXML());
			Assert.AreEqual($"{parser.GetNowInUnitedStates().ToString("MM-dd-yyyy", CultureInfo.CreateSpecificCulture("en-US"))} : Can not find the last modified date (url: Test.html).", exception.Message);
		}

		[Test]
		public void TestGetAttributes()
		{
			var disCode = new DISCode();
			var attributes = ParserHelper.GetAttributes(disCode);
			Assert.AreEqual(attributes.Length, 2);
			Assert.AreEqual(attributes[0].ZZE_Value, AttributeValues.NOGROUP);
			Assert.AreEqual(attributes[0].ZZE_ZXE_NKName, AttributeNames.USDISFormGroup);
			Assert.AreEqual(attributes[1].ZZE_Value, AttributeValues.GEN);
			Assert.AreEqual(attributes[1].ZZE_ZXE_NKName, AttributeNames.USDISPackageCategory);

			disCode = new DISCode() { DocumentDescription = "" };
			attributes = ParserHelper.GetAttributes(disCode);
			Assert.AreEqual(attributes.Length, 2);
			Assert.AreEqual(attributes[0].ZZE_Value, AttributeValues.NOGROUP);
			Assert.AreEqual(attributes[0].ZZE_ZXE_NKName, AttributeNames.USDISFormGroup);
			Assert.AreEqual(attributes[1].ZZE_Value, AttributeValues.GEN);
			Assert.AreEqual(attributes[1].ZZE_ZXE_NKName, AttributeNames.USDISPackageCategory);
		}

		[SetUp]
		public void Setup()
		{
			inputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"DISCodes\TestFiles\Input");
			outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"DISCodes\TestFiles\Output");
			checker = new LocalFileStorage("US DIS Code");
			checker.ClearData();
		}
		string inputPath;
		string outputPath;
		LocalFileStorage checker;
	}
}
