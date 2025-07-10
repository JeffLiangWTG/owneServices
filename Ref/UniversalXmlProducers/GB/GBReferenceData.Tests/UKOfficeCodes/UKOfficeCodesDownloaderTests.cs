using System;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using CargoWise.RefDbRepo.GBReferenceData.Services.UKOfficeCodes;
using CargoWise.RefDbRepo.GBReferenceData.Tests;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.UKOfficeCodes.Tests
{
	[TestFixture]
	class UKOfficeCodesDownloaderTests
	{
		[Test]
		public void GetDataUrlFromPage_Good()
		{
			var webClient = new Mock<IWebClientWrapper>();
			webClient.Setup(x => x.GetContent(It.IsAny<string>())).Returns<string>((url) => TestHelper.ReadManifestResourceContent(url));
			var downloader = new UKOfficeCodesDownloader(webClient.Object);

			var result = downloader.GetDataUrlFromPage("CargoWise.RefDbRepo.GBReferenceData.Tests.UKOfficeCodes.TestFiles.LandingPageWithGoodAnchor.html");
			Assert.That(result, Is.EqualTo("https://test.value/spreadsheet.ods"));
		}

		[Test]
		public void GetDataUrlFromPage_Bad()
		{
			var webClient = new Mock<IWebClientWrapper>();
			webClient.Setup(x => x.GetContent(It.IsAny<string>())).Returns<string>((url) => TestHelper.ReadManifestResourceContent(url));
			var downloader = new UKOfficeCodesDownloader(webClient.Object);

			Assert.Throws(Is.TypeOf<ApplicationException>().And.Message.StartsWith("Download link could not be found using the regex"), () => downloader.GetDataUrlFromPage("CargoWise.RefDbRepo.GBReferenceData.Tests.UKOfficeCodes.TestFiles.LandingPageMissingExpectedAnchor.html"));
		}

		[Test]
		public void GetSpreadsheetData()
		{
			var magicBytes = new byte[] { 42, 1, 2, 3, 4 };

			var webClient = new Mock<IWebClientWrapper>();
			webClient.Setup(x => x.GetContent(It.IsAny<string>())).Returns<string>((url) => TestHelper.ReadManifestResourceContent(url));
			webClient.Setup(x => x.GetContentAsByteArray("https://test.value/spreadsheet.ods")).Returns(magicBytes);

			var downloader = new UKOfficeCodesDownloader(webClient.Object);
			var result = downloader.GetSpreadsheetData("CargoWise.RefDbRepo.GBReferenceData.Tests.UKOfficeCodes.TestFiles.LandingPageWithGoodAnchor.html");

			Assert.That(result, Is.Not.Null);
			Assert.That(result, Is.EqualTo(magicBytes));
		}
	}
}
