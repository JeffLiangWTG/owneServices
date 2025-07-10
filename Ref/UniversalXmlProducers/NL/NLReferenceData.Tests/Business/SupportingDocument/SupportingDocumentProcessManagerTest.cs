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
	sealed class SupportingDocumentProcessManagerTest
	{
		[Test]
		public void ProcessCorrectXml()
		{
			ProcessXml("CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.SupportingDocument.Output.RefCusCodeList_SupportingDocument_Correct.xml");
		}

		[Test]
		public void InvalidXml()
		{
			var localZipFile = Path.Combine(TempFolder, "InvalidXml.zip");
			var errorCollector = new StringBuilder();

			var webClientWrapper = new Mock<IWebClientWrapper>();
			webClientWrapper.Setup(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Returns(TestHelper.SimulateDownload(localZipFile, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.SupportingDocument.Input.InvalidXml.zip"));
			webClientWrapper.Setup(x => x.ExtractLocalZipFile(It.IsAny<string>(), It.IsAny<string>())).Returns(new FileDownloaderWrapper().ExtractLocalZipFile(localZipFile));

			var downloadManager = new DownloadManager(webClientWrapper.Object);
			var processManager = new Mock<SupportingDocumentTypesWebClientProcessManager>(new SupportingDocumentBuilder(errorCollector));
			processManager.Setup(x => x.DownloadManager).Returns(downloadManager);

			processManager.Object.RunProcess(TempFolder, errorCollector);
			downloadManager.Dispose();

			Assert.That(errorCollector.ToString(), Does.Contain("Downloaded file is not a valid CodeBook for"));
		}


		[Test]
		public void NoSupportingDocumentInXml()
		{
			var localZipFile = Path.Combine(TempFolder, "NoSupportingDocument.zip");
			var errorCollector = new StringBuilder();

			var webClientWrapper = new Mock<IWebClientWrapper>();
			webClientWrapper.Setup(x => x.DownloadFile(ApplicationConfig.DownloadUrlCodeBookDwuAangifteBehandeling, It.IsAny<string>())).Returns(TestHelper.SimulateDownload(localZipFile, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.SupportingDocument.Input.NoSupportingDocument.zip"));
			webClientWrapper.Setup(x => x.ExtractLocalZipFile(It.IsAny<string>(), It.IsAny<string>())).Returns(new FileDownloaderWrapper().ExtractLocalZipFile(localZipFile));
			var downloadManager = new DownloadManager(webClientWrapper.Object);
			var processManager = new Mock<SupportingDocumentTypesWebClientProcessManager>(new SupportingDocumentBuilder(errorCollector));
			processManager.Setup(x => x.DownloadManager).Returns(downloadManager);

			processManager.Object.RunProcess(TempFolder, errorCollector);
			downloadManager.Dispose();

			var files = new List<string>(Directory.GetFiles(TempFolder));
			Assert.That(files.Find(x => Path.GetFileName(x).StartsWith("NL CodeList - Supporting Document_", StringComparison.InvariantCulture)) == null);
		}

		[Test]
		public void TableNumber()
		{
			var errorCollector = new StringBuilder();
			var processManager = new SupportingDocumentTypesWebClientProcessManager(new SupportingDocumentBuilder(errorCollector));
			Assert.AreEqual("213", processManager.TableNumber, "TableNumber should be 213 for Supporting Documents");
		}

		[Test]
		public void Description()
		{
			var errorCollector = new StringBuilder();
			var processManager = new SupportingDocumentTypesWebClientProcessManager(new SupportingDocumentBuilder(errorCollector));
			Assert.AreEqual("Supporting Documents", processManager.Description);
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

		void ProcessXml(string expectedResource)
		{
			var localZipFile = Path.Combine(TempFolder, "CodeBookDWUAangiftebehandeling.zip");
			var errorCollector = new StringBuilder();
			var dateTimeProvider = new Mock<IDateTimeProvider>();
			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2024, 06, 14, 15, 25, 06));

			var webClientWrapper = new Mock<IWebClientWrapper>();
			webClientWrapper.Setup(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Returns(TestHelper.SimulateDownload(localZipFile, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.SupportingDocument.Input.CorrectXml.zip"));
			webClientWrapper.Setup(x => x.ExtractLocalZipFile(It.IsAny<string>(), It.IsAny<string>())).Returns(new FileDownloaderWrapper().ExtractLocalZipFile(localZipFile));

			var downloadManager = new DownloadManager(webClientWrapper.Object);
			var processManager = new Mock<SupportingDocumentTypesWebClientProcessManager>(new SupportingDocumentBuilder(errorCollector, dateTimeProvider.Object));
			processManager.Setup(x => x.Builder).Returns(new SupportingDocumentBuilder(errorCollector, dateTimeProvider.Object));
			processManager.Setup(x => x.DownloadManager).Returns(downloadManager);
			processManager.Setup(x => x.TableNumber).Returns(ApplicationConfig.SupportingDocumentTableNumber);

			processManager.Object.RunProcess(TempFolder, errorCollector);
			downloadManager.Dispose();

			var files = new List<string>(Directory.GetFiles(TempFolder));
			var generatedFile = string.Empty;
			foreach (var file in files.Where(x => Path.GetFileName(x).StartsWith("NL CodeList - Supporting Document_", StringComparison.InvariantCulture)))
			{
				generatedFile = file;
			}


			if (File.Exists(generatedFile))
			{
				var generatedXml = File.ReadAllText(generatedFile);
				var expectedXml = TestHelper.ReadManifestResourceContent(expectedResource);

				Assert.That(generatedXml, Is.EqualTo(expectedXml), "XML-file does not contain the correct information for the Supporting Document.");
			}
			else
			{
				if (errorCollector.Length > 0)
				{
					Assert.Fail($"No XML file was generated. Error occurred: {errorCollector}");
				}
				else
				{
					Assert.Fail("No XML File was generated.");
				}
			}
		}

		string TempFolder;
	}
}
