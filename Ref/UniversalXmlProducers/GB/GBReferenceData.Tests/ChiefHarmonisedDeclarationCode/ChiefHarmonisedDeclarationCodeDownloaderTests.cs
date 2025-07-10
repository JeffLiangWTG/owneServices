using System;
using System.IO;
using CargoWise.RefDbRepo.GBReferenceData.Services.ChiefHarmonisedDeclarationCode;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.ChiefHarmonisedDeclarationCode
{
	[TestFixture]
	class ChiefHarmonisedDeclarationCodeDownloaderTests
	{
		[Test]
		public void TariffDocumentsFileNotFoundTest()
		{
			//Test Html contains NewName.ods which will not match the Regex
			var missingTariffDocumentsHtml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.ChiefHarmonisedDeclarationCode.TestFiles.Input.MissingTariffDocuments.html");
			var webDriverMock = new Mock<IWebDriverHelper>();
			webDriverMock.Setup(x => x.GetWebPage("MissingTariffDocuments", It.IsAny<int>())).Returns(missingTariffDocumentsHtml);
			var fileDownloader = new Mock<IFileDownloaderWrapper>();

			Assert.Throws(Is.TypeOf<ApplicationException>().And.Message.EqualTo("Unable to find certificate_authorisation_codes_for_harmonised_declarations.ods on webpage: MissingTariffDocuments"),
				() => ChiefHarmonisedDeclarationCodeDownloader.Download(fileDownloader.Object, webDriverMock.Object, "MissingTariffDocuments", string.Empty));
		}

		[Test]
		public void LastUpdatedMissingTest()
		{
			//Test Html is missing the Last Updated information
			var missingLastUpdatedHtml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.ChiefHarmonisedDeclarationCode.TestFiles.Input.MissingLastUpdated.html");
			var webDriverMock = new Mock<IWebDriverHelper>();
			webDriverMock.Setup(x => x.GetWebPage("MissingLastUpdated", It.IsAny<int>())).Returns(missingLastUpdatedHtml);
			var fileDownloader = new Mock<IFileDownloaderWrapper>();

			Assert.Throws(Is.TypeOf<ApplicationException>().And.Message.EqualTo("Unable to find Last Updated details on webpage: MissingLastUpdated"),
				() => ChiefHarmonisedDeclarationCodeDownloader.Download(fileDownloader.Object, webDriverMock.Object, "MissingLastUpdated", string.Empty));
		}

		[Test]
		public void LastUpdatedFormatHasChanged()
		{
			//Test Html Last Updated information changed to 05 Jan 19, the regex used only finds formats that can be parsed
			var testDownloadFile = Path.GetTempFileName();
			var formatChangedLastUpdatedHtml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.ChiefHarmonisedDeclarationCode.TestFiles.Input.FormatChangedLastUpdated.html");
			var webDriverMock = new Mock<IWebDriverHelper>();
			webDriverMock.Setup(x => x.GetWebPage("FormatChangedLastUpdated", It.IsAny<int>())).Returns(formatChangedLastUpdatedHtml);
			var fileDownloader = new Mock<IFileDownloaderWrapper>();
			fileDownloader.Setup(x => x.DownloadFile("FormatChangedLastUpdated", testDownloadFile)).Returns(true);

			Assert.That(new DateTime(2019, 1, 5), Is.EqualTo(ChiefHarmonisedDeclarationCodeDownloader.Download(fileDownloader.Object, webDriverMock.Object, "FormatChangedLastUpdated", testDownloadFile)));
		}

		[Test]
		public void TestGetSpecificFileURL()
		{
			var downloadUrl = "https://www.gov.uk/government/publications/uk-trade-tariff-document-certificate-and-authorisation-codes-for-harmonised-declarations";
			var formatChangedLastUpdatedHtml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.ChiefHarmonisedDeclarationCode.TestFiles.Input.FormatChangedLastUpdated.html");
			var expectedSpecificUrl = new Uri("https://assets.publishing.service.gov.uk/government/uploads/system/uploads/attachment_data/file/912939/CHIEF-doc-codes-August20.ods");
			Assert.DoesNotThrow(() =>
			{
				var specificFileUrl = ChiefHarmonisedDeclarationCodeDownloader.GetSpecificFileURL(formatChangedLastUpdatedHtml, downloadUrl);
				Assert.AreEqual(expectedSpecificUrl, specificFileUrl);
			});
		}
	}
}
