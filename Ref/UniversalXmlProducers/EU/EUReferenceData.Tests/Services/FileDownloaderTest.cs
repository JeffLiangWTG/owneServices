using System.IO;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.EUReferenceData.Tests;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Services.Tests
{
	[TestFixture]
	class FileDownloaderTest
	{
		[Test]
		public void DownloadZipFile()
		{
			httpClientMock.Setup(x => x.GetWebPageAsync("url")).Returns(Task.FromResult(htmlContent));
			httpClientMock.Setup(x => x.GetAsync(RemoteUrl)).Returns(Task.FromResult(zipStream));

			var errorBuilder = new StringBuilder();
			var successful = FileDownloader.DownloadRDEntryListZipFile(httpClientMock.Object, "url", zipFileDownloadPath, ApplicationConfig.Instance.RDEntryFileLinkPattern, errorBuilder).GetAwaiter().GetResult();
			Assert.That(successful, Is.True);
			Assert.That(zipFileDownloadPath, Does.Exist);
			Assert.That(errorBuilder.ToString(), Is.Empty);
		}

		[Test]
		public void DownloadZipFile_FileDownloadFailed()
		{
			var invalidRemoteUrl = "test.zip";
			httpClientMock.Setup(x => x.GetWebPageAsync("url")).Returns(Task.FromResult(htmlContent));
			httpClientMock.Setup(x => x.GetAsync(invalidRemoteUrl)).Returns(Task.FromResult(zipStream));

			var errorBuilder = new StringBuilder();
			var successful = FileDownloader.DownloadRDEntryListZipFile(httpClientMock.Object, "url", zipFileDownloadPath, ApplicationConfig.Instance.RDEntryFileLinkPattern, errorBuilder).GetAwaiter().GetResult();
			Assert.That(successful, Is.False);
			Assert.That(errorBuilder.ToString(), Does.Contain($"File download failed, remote url: {RemoteUrl}, localFilePath: {zipFileDownloadPath}"));
		}

		[Test]
		public void DownloadZipFile_DownloadLinkWasNotFound()
		{
			httpClientMock.Setup(x => x.GetWebPageAsync("url")).Returns(Task.FromResult("new html content"));
			httpClientMock.Setup(x => x.GetAsync(RemoteUrl)).Returns(Task.FromResult(zipStream));

			var errorBuilder = new StringBuilder();
			var successful = FileDownloader.DownloadRDEntryListZipFile(httpClientMock.Object, "url", zipFileDownloadPath, ApplicationConfig.Instance.RDEntryFileLinkPattern, errorBuilder).GetAwaiter().GetResult();
			Assert.That(successful, Is.False);
			Assert.That(errorBuilder.ToString(), Does.Contain("The website has been modified or it's under maintenance. The download link was not found."));
		}

		[SetUp]
		public void SetUp()
		{
			httpClientMock = new Mock<IHttpClientHelper>();
			zipFileDownloadPath = Path.GetTempFileName();
			zipStream = TestHelper.ReadManifestResourceContentAsStream("CargoWise.RefDbRepo.EUReferenceData.Tests.OfficeCodes.TestFiles.Input.COL-Generic-20201222.zip");
			htmlContent = TestHelper.ReadManifestResourceContentAsString("CargoWise.RefDbRepo.EUReferenceData.Tests.OfficeCodes.TestFiles.Input.DownloadCOLAndRDMessages.html");
		}
		Mock<IHttpClientHelper> httpClientMock;
		Stream zipStream;
		const string RemoteUrl = "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/COL-Generic-20220520.zip";
		string htmlContent;
		string zipFileDownloadPath;

		[TearDown]
		public void TearDown()
		{
			if (File.Exists(zipFileDownloadPath))
			{
				File.Delete(zipFileDownloadPath);
			}
		}
	}
}
