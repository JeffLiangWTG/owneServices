using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Test
{
	[TestFixture]
	class MdbFileDownloaderFixture
	{
		[Test]
		public void GetMdbFileAndPublicationDateFromManualUpload()
		{
			try
			{
				ConfigurationProvider.SetConfigFileForTest("CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Test.config.json");
				using var mdbFileDownloader = new MdbFileDownloader(scrapePageHTMLDownloaderMock.Object, clientHelperMock.Object);
				var (downloadFilePath, publicationDate) = mdbFileDownloader.GetMdbFileAndPublicationDate();
				var callingExePath = Assembly.GetEntryAssembly()?.Location;
				Assert.That(downloadFilePath, Is.EqualTo(Path.Combine(callingExePath, ConfigurationProvider.ProgramSpecificConfigurationsUNECEFilePath)));
				Assert.That(publicationDate, Is.EqualTo(DateTime.Parse("2024-12-31", CultureInfo.InvariantCulture)));
			}
			finally
			{
				ConfigurationProvider.SetConfigFileForTest(null);
			}
		}

		[Test]
		public void GetMdbFileAndPublicationDateWithoutManualUpload()
		{
			string htmlContent;
			var stream = GetType().Assembly.GetManifestResourceStream("CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Test.ref.unece_codes.html");
			using var reader = new StreamReader(stream, Encoding.UTF8);
			htmlContent = reader.ReadToEnd();
			clientHelperMock.Setup(x => x.GetWebPageAsync(It.IsAny<string>())).Returns(Task.FromResult(htmlContent));
			tempMdbFile = Path.GetTempFileName();
			try
			{
				using FileStream fs = File.Create(tempMdbFile);
				scrapePageHTMLDownloaderMock.Setup(x => x.DownloadFile(It.IsAny<ScrapePageHTML>())).Returns(tempMdbFile);
				fs.Dispose();

				using var mdbFileDownloader = new MdbFileDownloader(scrapePageHTMLDownloaderMock.Object, clientHelperMock.Object);
				var (downloadFilePath, publicationDate) = mdbFileDownloader.GetMdbFileAndPublicationDate();
				scrapePageHTMLDownloaderMock.Verify(x => x.DownloadFile(It.IsAny<ScrapePageHTML>()), Times.AtLeastOnce);
			}
			finally
			{
				if (!string.IsNullOrEmpty(tempMdbFile) && File.Exists(tempMdbFile))
				{
					File.Delete(tempMdbFile);
				}
			}
		}

		[Test]
		public void GetMdbFileAndPublicationDateHttpRequestFails()
		{
			var originalOut = Console.Out;
			using var stringWriterOut = new StringWriter();
			Console.SetOut(stringWriterOut);
			try
			{
				clientHelperMock.Setup(x => x.GetWebPageAsync(It.IsAny<string>())).ThrowsAsync(new Exception("Simulated exception"));
				using var mdbFileDownloader = new MdbFileDownloader(scrapePageHTMLDownloaderMock.Object, clientHelperMock.Object);
				string downloadFilePath = null;
				var exception = Assert.Throws<AggregateException>(() => mdbFileDownloader.GetMdbFileAndPublicationDate());
				Assert.That(exception.InnerException.Message, Is.EqualTo("Simulated exception"));
				Assert.IsNull(downloadFilePath);
				Assert.That(stringWriterOut.ToString(), Does.Contain("Retry 2 failed with exception: One or more errors occurred. (Simulated exception)"));
			}
			finally
			{
				Console.SetOut(originalOut);
			}
		}

		[Test]
		public void GetMdbFileAndPublicationDateShouldRetryOnTimeout()
		{
			var callCount = 0;
			clientHelperMock.Setup(x => x.GetWebPageAsync(It.IsAny<string>()))
				.Callback(() => callCount++)
				.ThrowsAsync(new TaskCanceledException("Simulated timeout"));

			using var mdbFileDownloader = new MdbFileDownloader(scrapePageHTMLDownloaderMock.Object, clientHelperMock.Object);
			var exception = Assert.Throws<AggregateException>(() => mdbFileDownloader.GetMdbFileAndPublicationDate());
			Assert.That(exception.InnerException.Message, Is.EqualTo("Simulated timeout"));
			Assert.That(callCount, Is.EqualTo(3));
		}

		[SetUp]
		public void SetUp()
		{
			scrapePageHTMLDownloaderMock = new Mock<IScrapePageHTMLDownloader>();
			clientHelperMock = new Mock<IHttpClientHelper>();
		}

		Mock<IScrapePageHTMLDownloader> scrapePageHTMLDownloaderMock;
		Mock<IHttpClientHelper> clientHelperMock;
		string tempMdbFile;
	}
}
