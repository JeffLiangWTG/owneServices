using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.NLReferenceData.Services;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NLReferenceData.Business.Testing
{
	[TestFixture]
	sealed class TransportDocumentProcessManagerTest
	{
		[Test]
		public void ProcessCorrectXml()
		{
			ProcessXml("CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.TransportDocument.Output.RefCusCodeList_TransportDocument_Correct.xml");
		}

		[Test]
		public void InvalidXml()
		{
			var localZipFile = Path.Combine(TempFolder, "InvalidXml.zip");
			var errorCollector = new StringBuilder();

			var webClientWrapper = new Mock<IWebClientWrapper>();
			webClientWrapper.Setup(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Returns(TestHelper.SimulateDownload(localZipFile, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.TransportDocument.Input.InvalidXml.zip"));
			webClientWrapper.Setup(x => x.ExtractLocalZipFile(It.IsAny<string>(), It.IsAny<string>())).Returns(new FileDownloaderWrapper().ExtractLocalZipFile(localZipFile));

			var downloadManager = new DownloadManager(webClientWrapper.Object);
			var processManager = new Mock<TransportDocumentsWebClientProcessManager>(new TransportDocumentBuilder(errorCollector));
			processManager.Setup(x => x.DownloadManager).Returns(downloadManager);

			processManager.Object.RunProcess(TempFolder, errorCollector);
			downloadManager.Dispose();

			Assert.That(errorCollector.ToString(), Does.Contain("Downloaded file is not a valid CodeBook for"));
		}

		[Test]
		public void NoTransportDocumentInXml()
		{
			var localZipFile = Path.Combine(TempFolder, "NoTransportDocument.zip");
			var errorCollector = new StringBuilder();

			var webClientWrapper = new Mock<IWebClientWrapper>();
			webClientWrapper.Setup(x => x.DownloadFile(ApplicationConfig.DownloadUrlCodeBookDwuAangifteBehandeling, It.IsAny<string>())).Returns(TestHelper.SimulateDownload(localZipFile, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.TransportDocument.Input.NoTransportDocument.zip"));
			webClientWrapper.Setup(x => x.ExtractLocalZipFile(It.IsAny<string>(), It.IsAny<string>())).Returns(new FileDownloaderWrapper().ExtractLocalZipFile(localZipFile));
			var downloadManager = new DownloadManager(webClientWrapper.Object);
			var processManager = new Mock<TransportDocumentsWebClientProcessManager>(new TransportDocumentBuilder(errorCollector));
			processManager.Setup(x => x.DownloadManager).Returns(downloadManager);

			processManager.Object.RunProcess(TempFolder, errorCollector);
			downloadManager.Dispose();

			Assert.That(errorCollector.ToString(), Does.Contain("Downloaded file does not contain a code list for"));
		}

		[Test]
		public void TableNumber()
		{
			var errorCollector = new StringBuilder();
			var processManager = new TransportDocumentsWebClientProcessManager(new TransportDocumentBuilder(errorCollector));
			Assert.AreEqual("754", processManager.TableNumber, "TableNumber should be 754 for Transport Documents");
		}

		[Test]
		public void Description()
		{
			var errorCollector = new StringBuilder();
			var processManager = new TransportDocumentsWebClientProcessManager(new TransportDocumentBuilder(errorCollector));
			Assert.AreEqual("Transport Documents", processManager.Description);
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

		void ProcessXml(string expectedResource)
		{
			var localZipFile = Path.Combine(TempFolder, "CodeBookDWUAangiftebehandeling.zip");
			var errorCollector = new StringBuilder();
			var dateTimeProvider = new Mock<IDateTimeProvider>();
			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2024, 07, 18, 15, 58, 07));

			var webClientWrapper = new Mock<IWebClientWrapper>();
			webClientWrapper.Setup(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Returns(TestHelper.SimulateDownload(localZipFile, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.TransportDocument.Input.CorrectXml.zip"));
			webClientWrapper.Setup(x => x.ExtractLocalZipFile(It.IsAny<string>(), It.IsAny<string>())).Returns(new FileDownloaderWrapper().ExtractLocalZipFile(localZipFile));

			var downloadManager = new DownloadManager(webClientWrapper.Object);
			var processManager = new Mock<TransportDocumentsWebClientProcessManager>(new TransportDocumentBuilder(errorCollector, dateTimeProvider.Object));
			processManager.Setup(x => x.Builder).Returns(new TransportDocumentBuilder(errorCollector, dateTimeProvider.Object));
			processManager.Setup(x => x.DownloadManager).Returns(downloadManager);
			processManager.Setup(x => x.TableNumber).Returns(ApplicationConfig.TransportDocumentTableNumber);

			processManager.Object.RunProcess(TempFolder, errorCollector);
			downloadManager.Dispose();

			var files = new List<string>(Directory.GetFiles(TempFolder));
			var generatedFile = string.Empty;
			foreach (var file in files.Where(x => Path.GetFileName(x).StartsWith("NL CodeList - Transport Document", StringComparison.InvariantCulture)))
			{
				generatedFile = file;
			}

			if (File.Exists(generatedFile))
			{
				var generatedXml = File.ReadAllText(generatedFile);
				var expectedXml = TestHelper.ReadManifestResourceContent(expectedResource);

				Assert.That(generatedXml.EndsWith(expectedXml, StringComparison.InvariantCulture), "XML-file does not contain the correct information for the TransportDocument.");
			}
			else
			{
				if (errorCollector.Length > 0)
				{
					Assert.Fail($"No XML file was generated. Error occurred: {errorCollector.ToString()}");
				}
				else
				{
					Assert.Fail("No XML File was generated.");
				}
			}
		}
	}
}
