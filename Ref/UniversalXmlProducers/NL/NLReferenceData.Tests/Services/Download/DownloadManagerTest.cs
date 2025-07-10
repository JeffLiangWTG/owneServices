using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.NLReferenceData.Business.Testing;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NLReferenceData.Services.Testing
{
	[TestFixture]
	sealed class DownloadManagerTest
	{
		[Test]
		public void DownloadFile()
		{
			var fileUrl = ApplicationConfig.DownloadUrlCodeBook;
			var localZipFile = Path.Combine(TempFolder, "CodeBook.zip");

			var webClientWrapper = new Mock<IWebClientWrapper>();
			webClientWrapper.Setup(x => x.DownloadFile(ApplicationConfig.DownloadUrlCodeBook, localZipFile)).Returns(TestHelper.SimulateDownload(localZipFile, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Download.Input.TestZip.zip"));
			webClientWrapper.Setup(x => x.ExtractLocalZipFile(It.IsAny<string>(), It.IsAny<string>())).Returns(new FileDownloaderWrapper().ExtractLocalZipFile(localZipFile));
			var downloadManager = new DownloadManager(webClientWrapper.Object);

			List<string> downloadedContent = downloadManager.DownloadCodeBook(fileUrl, TempFolder);
			downloadManager.Dispose();
			Assert.IsNotEmpty(downloadedContent, "No xml-files were downloaded in the zip-file");
		}

		[Test]
		public void DownloadFileFailed()
		{
			var fileUrl = "http://invalid.link/test.zip";
			var webClientWrapper = new Mock<IWebClientWrapper>();
			webClientWrapper.Setup(x => x.DownloadFile(fileUrl, It.IsAny<string>())).Throws<System.Net.WebException>();

			var downloadManager = new DownloadManager(webClientWrapper.Object);
			var ex = Assert.Throws<ProcessingException>(() => downloadManager.DownloadCodeBook(fileUrl, TempFolder));
			downloadManager.Dispose();
			Assert.That(ex.Message, Is.EqualTo($"Processing of {fileUrl} failed. "));
		}

		[Test]
		public void DownloadFileAsXmlDocument()
		{
			var fileUrl = ApplicationConfig.DownloadUrlTariff;
			var webClientWrapper = new Mock<IWebClientWrapper>();
			webClientWrapper.Setup(x => x.DownloadFileAsXmlDocument(fileUrl)).Returns(TestHelper.GetResourceContentAsXmlDocument("CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.ValidTariffFileList.xml"));

			var downloadManager = new DownloadManager(webClientWrapper.Object);
			var downloadedXml = downloadManager.DownloadFileAsXmlDocument(fileUrl);
			downloadManager.Dispose();

			Assert.IsNotEmpty(downloadedXml, "No xml-files were downloaded");
		}

		[Test]
		public void DownloadFileAsXmlDocumentFailedWebException()
		{
			var fileUrl = "http://invalid.link/test.xml";
			var webClientWrapper = new Mock<IWebClientWrapper>();
			webClientWrapper.Setup(x => x.DownloadFileAsXmlDocument(fileUrl)).Throws<System.Net.WebException>();

			var downloadManager = new DownloadManager(webClientWrapper.Object);
			var ex = Assert.Throws<ProcessingException>(() => downloadManager.DownloadFileAsXmlDocument(fileUrl));
			downloadManager.Dispose();

			Assert.That(ex.Message, Is.EqualTo($"Processing of {fileUrl} failed. "));
			Assert.That(ex.InnerException.ToString().Split('\r')[0], Is.EqualTo("System.Net.WebException: Operation is not valid due to the current state of the object."));
		}

		[Test]
		public void DownloadFileAsXmlDocumentFailedXmlException()
		{
			var fileUrl = "https://www.google.com";
			var webClientWrapper = new Mock<IWebClientWrapper>();
			webClientWrapper.Setup(x => x.DownloadFileAsXmlDocument(fileUrl)).Throws<System.Xml.XmlException>();

			var downloadManager = new DownloadManager(webClientWrapper.Object);
			var ex = Assert.Throws<ProcessingException>(() => downloadManager.DownloadFileAsXmlDocument(fileUrl));
			downloadManager.Dispose();

			Assert.That(ex.Message, Is.EqualTo($"Processing of {fileUrl} failed. "));
			Assert.That(ex.InnerException.ToString().Split('\r')[0], Is.EqualTo("System.Xml.XmlException: An XML error has occurred."));
		}

		[Test]
		public void SupplementsDownloadFilesFailed()
		{
			var fileUrl = "http://invalid.link/";
			var webDriverHelperWrapper = new Mock<IWebDriverHelperWrapper>();
			webDriverHelperWrapper.Setup(x => x.DownloadFiles(fileUrl)).Throws<System.Net.WebException>();

			var downloadManager = new DownloadManager(webDriverHelperWrapper.Object);
			var ex = Assert.Throws<ProcessingException>(() => downloadManager.DownloadFilesAsExcel(fileUrl));
			downloadManager.Dispose();

			Assert.That(ex.Message, Is.EqualTo($"Processing of {fileUrl} failed. "));
		}

		[Test]
		public void TariffDownloadFiles()
		{
			var webClientWrapper = new Mock<IWebClientWrapper>();
			var downloadedXml = TestHelper.GetResourceContentAsXmlDocument("CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.TestTariffFileList.xml");
			var downloadList = TariffProcessManager.ReadTariffDownloadElements(downloadedXml);

			var TempFolderTariffData = Path.Combine(TempFolder, "NLTariffData");
			Directory.CreateDirectory(TempFolderTariffData);
			var dummy1File = Path.Combine(TempFolderTariffData, "dummy1.zip");
			var dummy2File = Path.Combine(TempFolderTariffData, "dummy2.zip");
			var dummy3File = Path.Combine(TempFolderTariffData, "dummy3.zip");
			var dummy4File = Path.Combine(TempFolderTariffData, "dummy4.zip");

			TestHelper.SimulateDownload(dummy1File, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.dummy1.zip");
			TestHelper.SimulateDownload(dummy4File, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.dummy4.zip");
			var dummy1LastWriteTime = File.GetLastWriteTime(dummy1File);

			foreach (var download in downloadList)
			{
				var dummyFileName = download.FileName;
				var dummyFile = Path.Combine(TempFolderTariffData, dummyFileName);
				if (!File.Exists(dummyFile))
				{
					webClientWrapper.Setup(x => x.DownloadFile(download.Url, Path.Combine(TempFolderTariffData, dummyFile))).Returns(TestHelper.SimulateDownload(dummyFile, $"CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.{dummyFileName}"));
				}
			}

			var downloadManager = new DownloadManager(webClientWrapper.Object);
			downloadManager.DownloadNewFiles(downloadList, TempFolderTariffData);
			downloadManager.Dispose();

			Assert.AreEqual(3, Directory.GetFiles(TempFolderTariffData).Length, "File Count");
			Assert.IsFalse(File.Exists(dummy4File), "Existing file(s) not in XML are deleted, but it still exists");
			Assert.AreEqual(dummy1LastWriteTime, File.GetLastWriteTime(dummy1File), "Existing file dummy1.zip in XML is skipped for download");
			Assert.IsTrue(File.Exists(dummy2File), "File dummy2.zip is downloaded, but it doesn't exist");
			Assert.IsTrue(File.Exists(dummy3File), "File dummy3.zip is downloaded, but it doesn't exist");
		}

		[SetUp]
		public void Setup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
		}

		[TearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		string TempFolder;
	}
}
