using System;
using System.IO;
using System.Linq;
using System.Web;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using HtmlAgilityPack;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	public class WebFileLocatorFixture
	{
		[Test]
		public void SearchingTariffProducerFileNamesUsingStartsWithAndCaseInsensitive()
		{
			AssertSuccessfullyFoundFileName(new ImportTariffProducer(), @"TestFiles\SampleTariffProducerXlsx.html");
		}

		[Test]
		public void SearchingNomenclatureProducerFileNamesUsingStartsWithAndCaseInsensitive() => AssertSuccessfullyFoundFileName(new NomenclatureProducer(), @"TestFiles\SampleNomenclatureProducerXlsx.html");

		[Test]
		public void SearchingAdditionalCodesFileName() => AssertSuccessfullyFoundFileName(new AdditionalCodesProducer(), @"TestFiles\AdditionalCodesXlsx.html");

		[Test]
		public void ThrowExceptionIfLinkIsNotFoundInHtml()
		{
			var nomenclatureProducer = new Mock<NomenclatureProducer>();
			nomenclatureProducer.Setup(x => x.LocateWebFiles()).Returns(new[] { new WebFileInfo("ErrorFindingFile", new Exception("Error Finding File")) });
			try
			{
				nomenclatureProducer.Object.Run();
			}
			catch (Exception ex)
			{
				Assert.That(ex != null);
				Assert.That(ex.Message == "Error Finding File");
			}
		}

		[Test]
		public void GetLocationOfLatestFilesReturnsLatestDuplicatedFiles()
		{
			var webDriverHelper = new Mock<IWebDriverHelper>();
			webDriverHelper.SetupSequence(x => x.GetWebPage(".", 10)).Returns(string.Empty).Returns(ReadAllTextOfTestFile(@"TestFiles\NomenclatureTest.html"));
			webDriverHelper.Setup(x => x.GetWebPageByLinkText("2022", 10)).Returns(ReadAllTextOfTestFile(@"TestFiles\NomenclatureYear2022Test.html"));
			webDriverHelper.Setup(x => x.GetWebPageByLinkText("01 - January", 10)).Returns(ReadAllTextOfNomenclatureJan2022Test());
			var webFileLocator = new WebFileLocator(webDriverHelper.Object, new[] { "Declarable codes" });

			var webFileInfoCollection = webFileLocator.GetLocationOfLatestFiles(".");
			webDriverHelper.Verify(x => x.GetWebPage(".", 10), Times.AtLeast(2));
			Assert.That(webFileInfoCollection.Count(), Is.EqualTo(1));
			var webFileInfo = webFileInfoCollection.Single();
			Assert.AreEqual("https://circabc.europa.eu/rest/download/0d6d3a1a-c0df-493c-a523-f60107652767", webFileInfo.DownloadPath);

			webDriverHelper.SetupSequence(x => x.GetWebPage(".", 10)).Returns(string.Empty).Returns(ReadAllTextOfTestFile(@"TestFiles\NomenclatureTest.html"));
			webFileInfoCollection = webFileLocator.GetLocationOfLatestFiles(".", takeLatestOfDuplicatedFiles: true);
			webDriverHelper.Verify(x => x.GetWebPage(".", 10), Times.AtLeast(2));
			Assert.That(webFileInfoCollection.Count(), Is.EqualTo(1));
			webFileInfo = webFileInfoCollection.Single();
			Assert.AreEqual("https://circabc.europa.eu/rest/download/0d6d3a1a-c0df-493c-a523-f60107652767", webFileInfo.DownloadPath);
		}

		[Test]
		public void GetLocationOfLatestFilesRetriesAfterEncounteringLoadingText()
		{
			var webDriverHelper = new Mock<IWebDriverHelper>();
			var setupSequence = webDriverHelper.SetupSequence(x => x.GetWebPage(".", 10));
			for (var x = 1; x < ApplicationConfig.MaxOfReloadLatestYearPage; x++)
			{
				setupSequence.Returns(ReadAllTextOfTestFile(@"TestFiles\EUTaricBaseWebPageLoadingTextOnly.html"));
			}
			setupSequence.Returns(ReadAllTextOfTestFile(@"TestFiles\NomenclatureTest.html"));
			webDriverHelper.Setup(x => x.GetWebPageByLinkText("2022", 10)).Returns(ReadAllTextOfTestFile(@"TestFiles\NomenclatureYear2022Test.html"));
			webDriverHelper.Setup(x => x.GetWebPageByLinkText("01 - January", 10)).Returns(ReadAllTextOfNomenclatureJan2022Test());

			var webFileLocator = new WebFileLocator(webDriverHelper.Object, new[] { "Declarable codes" });

			var webFileInfoCollection = webFileLocator.GetLocationOfLatestFiles(".");
			webDriverHelper.Verify(x => x.GetWebPage(".", 10), Times.AtLeast(20));
			Assert.That(webFileInfoCollection.Count(), Is.EqualTo(1));
			var webFileInfo = webFileInfoCollection.Single();
			Assert.AreEqual("https://circabc.europa.eu/rest/download/0d6d3a1a-c0df-493c-a523-f60107652767", webFileInfo.DownloadPath);
		}

		[Test]
		public void GetLocationOfLatestFilesThrowExceptionAfterMaxNumberOfLoadsExceed()
		{
			var webDriverHelper = new Mock<IWebDriverHelper>();
			var setupSequence = webDriverHelper.SetupSequence(x => x.GetWebPage("testUri", 10));
			for (var x = 0; x < (ApplicationConfig.MaxOfReloadLatestYearPage + 1); x++)
			{
				setupSequence.Returns(ReadAllTextOfTestFile(@"TestFiles\EUTaricBaseWebPageLoadingTextOnly.html"));
			}
			var webFileLocator = new WebFileLocator(webDriverHelper.Object, new[] { "Declarable codes" });
			var webFileInfo = webFileLocator.GetLocationOfLatestFiles("testUri").First();
			Assert.That(webFileInfo, Is.Not.Null);
			Assert.That(webFileInfo.Exception, Is.Not.Null);
			Assert.That(webFileInfo.Exception.Message, Is.EqualTo("Unable to read latestMonthWebPage from testUri. "));
		}

		[Test]
		public void GetLocationOfLatestFilesWhenTheLastMonthFolderIsEmpty()
		{
			var webDriverHelper = new Mock<IWebDriverHelper>();
			var setupSequence = webDriverHelper.Setup(x => x.GetWebPage(".", 10)).Returns(ReadAllTextOfTestFile(@"TestFiles\NomenclatureTest.html"));
			webDriverHelper.Setup(x => x.GetWebPageByLinkText("2022", 10)).Returns(ReadAllTextOfTestFile(@"TestFiles\NomenclatureYearTest.html"));
			webDriverHelper.Setup(x => x.GetWebPageByLinkText("12 - December", 10)).Returns(ReadAllTextOfTestFile(@"TestFiles\NomenclatureMonthEmptyTest.html"));
			webDriverHelper.Setup(x => x.GetWebPageByLinkText("11 - November", 10)).Returns(ReadAllTextOfTestFile(@"TestFiles\NomenclatureMonthTest.html"));

			var webFileLocator = new WebFileLocator(webDriverHelper.Object, new[] { "Declarable codes" });

			var webFileInfoCollection = webFileLocator.GetLocationOfLatestFiles(".");
			Assert.That(webFileInfoCollection.Count(), Is.EqualTo(1), "Required file is located.");
			var webFileInfo = webFileInfoCollection.Single();
			const string expectedDownloadPath = "https://circabc.europa.eu/rest/download/6f22fd32-f78b-4c7c-bab7-d41765518ddc";
			Assert.AreEqual(expectedDownloadPath, webFileInfo.DownloadPath, "Download path equals to expected.");
			int expectedModificationMonth = 11;
			Assert.AreEqual(expectedModificationMonth, webFileInfo.LastModificationTime.Month, "The last modification date is from previous month - November.");
		}

		[Test]
		public void GetLocationOfLatestFilesWhenTheLastMonthIsJanuaryEmpty()
		{
			var webDriverHelper = new Mock<IWebDriverHelper>();
			var setupSequence = webDriverHelper.Setup(x => x.GetWebPage(".", 10)).Returns(ReadAllTextOfTestFile(@"TestFiles\NomenclatureTest.html"));
			webDriverHelper.Setup(x => x.GetWebPageByLinkText("2022", 10)).Returns(ReadAllTextOfTestFile(@"TestFiles\NomenclatureYear2022Test.html"));
			webDriverHelper.Setup(x => x.GetWebPageByLinkText("01 - January", 10)).Returns(ReadAllTextOfTestFile(@"TestFiles\NomenclatureMonthEmptyTest.html"));
			webDriverHelper.Setup(x => x.GetWebPageByLinkText("2021", 10)).Returns(ReadAllTextOfTestFile(@"TestFiles\NomenclatureYearTest.html"));
			webDriverHelper.Setup(x => x.GetWebPageByLinkText("12 - December", 10)).Returns(ReadAllTextOfTestFile(@"TestFiles\NomenclatureMonthTest.html"));

			var webFileLocator = new WebFileLocator(webDriverHelper.Object, new[] { "Declarable codes" });

			var webFileInfoCollection = webFileLocator.GetLocationOfLatestFiles(".");
			Assert.That(webFileInfoCollection.Count(), Is.EqualTo(1), "Required file is located.");
			var webFileInfo = webFileInfoCollection.Single();
			const string expectedDownloadPath = "https://circabc.europa.eu/rest/download/6f22fd32-f78b-4c7c-bab7-d41765518ddc";
			Assert.AreEqual(expectedDownloadPath, webFileInfo.DownloadPath, "Download path equals to expected.");
			int expectedModificationYear = 2021;
			Assert.AreEqual(expectedModificationYear, webFileInfo.LastModificationTime.Year, "The last modification date is from previous year - 2021.");
		}

		[Test]
		public void SetWebFileInfo()
		{
			var nomenclatureYear2022HtmlDocument = new HtmlDocument();
			nomenclatureYear2022HtmlDocument.LoadHtml(ReadAllTextOfNomenclatureJan2022Test());

			var webFileInfo = WebFileLocator.SetWebFileInfo(nomenclatureYear2022HtmlDocument, "Declarable codes");
			Assert.That(webFileInfo, Is.Not.Null);
			Assert.That(webFileInfo.Exception, Is.Null);
			Assert.AreEqual("https://circabc.europa.eu/rest/download/0d6d3a1a-c0df-493c-a523-f60107652767", webFileInfo.DownloadPath);
			Assert.AreEqual(new DateTime(2022, 1, 14, 17, 57, 0), webFileInfo.LastModificationTime);

			webFileInfo = WebFileLocator.SetWebFileInfo(nomenclatureYear2022HtmlDocument, "Declarable codes", new ContentRecordModifiedDateComparer());
			Assert.That(webFileInfo, Is.Not.Null);
			Assert.That(webFileInfo.Exception, Is.Null);
			Assert.AreEqual("https://circabc.europa.eu/rest/download/0d6d3a1a-c0df-493c-a523-f60107652767", webFileInfo.DownloadPath);
			Assert.AreEqual(new DateTime(2022, 1, 14, 17, 57, 0), webFileInfo.LastModificationTime);
		}

		[Test]
		public void SetWebFileInfo_WhenRequiredFileLinkNodeIsNotFound()
		{
			var htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(ReadAllTextOfTestFile("TestFiles\\AdditionalCodesXlsx.html"));

			var webFileInfo = WebFileLocator.SetWebFileInfo(htmlDocument, fileName: "Deliberately missing item");
			Assert.That(webFileInfo, Is.Not.Null);
			Assert.That(webFileInfo.Exception, Is.Not.Null);
			Assert.That(webFileInfo.Exception.Message, Is.EqualTo("Unable to retrieve HTML link node with text 'Deliberately missing item'."));
		}

		[Test]
		public void LocationOfLatestFiles()
		{
			var webDriverHelper = new Mock<IWebDriverHelper>();
			webDriverHelper.Setup(x => x.GetWebPage("testUri", 10)).Returns(ReadAllTextOfTestFile(@"TestFiles\NomenclatureErrorTest.html"));
			var filesToLocate = new[] { "Nomenclature EN", "Declarable codes" };
			var webFileLocator = new WebFileLocator(webDriverHelper.Object, filesToLocate);
			var webFileInfo = webFileLocator.GetLocationOfLatestFiles("testUri").FirstOrDefault();
			Assert.That(webFileInfo, Is.Not.Null);
			Assert.That(webFileInfo.Exception, Is.Not.Null);
			Assert.That(webFileInfo.Exception.Message, Is.EqualTo("Unable to read latestMonthWebPage from testUri. "));
		}

		#region Implementation

		void AssertSuccessfullyFoundFileName(BaseProducer producer, string htmlFilePath)
		{
			var page = HttpUtility.HtmlDecode(ReadAllTextOfTestFile(htmlFilePath));
			var htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(page);

			foreach (var file in producer.FilesToLocate)
			{
				var result = WebFileLocator.SetWebFileInfo(htmlDocument, file);
				Assert.Null(result.Exception);
				Assert.Greater(result.LastModificationTime, new DateTime(2018, 1, 1));
			}
		}

		static string ReadAllTextOfNomenclatureJan2022Test() => ReadAllTextOfTestFile(@"TestFiles\NomenclatureJan2022Test.html");

		static string ReadAllTextOfTestFile(string htmlFilePath) => File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, htmlFilePath));

		[SetUp]
		public void Setup()
		{
			ApplicationConfig.ConfigEnvironment();
		}

		#endregion
	}
}
