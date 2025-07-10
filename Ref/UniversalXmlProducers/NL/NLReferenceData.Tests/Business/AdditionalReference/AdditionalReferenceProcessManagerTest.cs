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
	sealed class AdditionalReferenceProcessManagerTest
	{
		[Test]
		public void ProcessCorrectXml()
		{
			ProcessXml("CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.AdditionalReference.Output.RefCusCodeList_AdditionalReference_Correct.xml");
		}

		[Test]
		public void InvalidXml()
		{
			var localZipFile = Path.Combine(TempFolder, "InvalidXml.zip");
			var errorCollector = new StringBuilder();

			var webClientWrapper = new Mock<IWebClientWrapper>();
			webClientWrapper.Setup(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Returns(TestHelper.SimulateDownload(localZipFile, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.AdditionalReference.Input.InvalidXml.zip"));
			webClientWrapper.Setup(x => x.ExtractLocalZipFile(It.IsAny<string>(), It.IsAny<string>())).Returns(new FileDownloaderWrapper().ExtractLocalZipFile(localZipFile));

			var downloadManager = new DownloadManager(webClientWrapper.Object);
			var processManager = new Mock<AdditionalReferenceWebClientProcessManager>(new AdditionalReferenceBuilder(errorCollector));
			processManager.Setup(x => x.DownloadManager).Returns(downloadManager);

			processManager.Object.RunProcess(TempFolder, errorCollector);
			downloadManager.Dispose();

			Assert.That(errorCollector.ToString(), Does.Contain("Downloaded file is not a valid CodeBook for"));
		}


		[Test]
		public void NoAdditionalReferenceInXml()
		{
			var localZipFile = Path.Combine(TempFolder, "NoAdditionalReference.zip");
			var errorCollector = new StringBuilder();

			var webClientWrapper = new Mock<IWebClientWrapper>();
			webClientWrapper.Setup(x => x.DownloadFile(ApplicationConfig.DownloadUrlCodeBookDwuAangifteBehandeling, It.IsAny<string>())).Returns(TestHelper.SimulateDownload(localZipFile, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.AdditionalReference.Input.NoAdditionalReference.zip"));
			webClientWrapper.Setup(x => x.ExtractLocalZipFile(It.IsAny<string>(), It.IsAny<string>())).Returns(new FileDownloaderWrapper().ExtractLocalZipFile(localZipFile));
			var downloadManager = new DownloadManager(webClientWrapper.Object);
			var processManager = new Mock<AdditionalReferenceWebClientProcessManager>(new AdditionalReferenceBuilder(errorCollector));
			processManager.Setup(x => x.DownloadManager).Returns(downloadManager);

			processManager.Object.RunProcess(TempFolder, errorCollector);
			downloadManager.Dispose();

			Assert.That(errorCollector.ToString(), Does.Contain("Downloaded file does not contain a code list for"));
		}

		[Test]
		public void TableNumber()
		{
			var errorCollector = new StringBuilder();
			var processManager = new AdditionalReferenceWebClientProcessManager(new AdditionalReferenceBuilder(errorCollector));
			Assert.AreEqual("380", processManager.TableNumber, "TableNumber should be 380 for Additional References");
		}

		[Test]
		public void Description()
		{
			var errorCollector = new StringBuilder();
			var processManager = new AdditionalReferenceWebClientProcessManager(new AdditionalReferenceBuilder(errorCollector));
			Assert.AreEqual("Additional References", processManager.Description);
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
			webClientWrapper.Setup(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Returns(TestHelper.SimulateDownload(localZipFile, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.AdditionalReference.Input.CorrectXml.zip"));
			webClientWrapper.Setup(x => x.ExtractLocalZipFile(It.IsAny<string>(), It.IsAny<string>())).Returns(new FileDownloaderWrapper().ExtractLocalZipFile(localZipFile));

			var downloadManager = new DownloadManager(webClientWrapper.Object);
			var processManager = new Mock<AdditionalReferenceWebClientProcessManager>(new AdditionalReferenceBuilder(errorCollector, dateTimeProvider.Object));
			processManager.Setup(x => x.Builder).Returns(new AdditionalReferenceBuilder(errorCollector, dateTimeProvider.Object));
			processManager.Setup(x => x.DownloadManager).Returns(downloadManager);
			processManager.Setup(x => x.TableNumber).Returns(ApplicationConfig.AdditionalReferenceTableNumber);

			processManager.Object.RunProcess(TempFolder, errorCollector);
			downloadManager.Dispose();

			var files = new List<string>(Directory.GetFiles(TempFolder));
			var generatedFile = string.Empty;
			foreach (var file in files.Where(x => Path.GetFileName(x).StartsWith("NL CodeList - Additional Reference", StringComparison.InvariantCulture)))
			{
				generatedFile = file;
			}

			if (File.Exists(generatedFile))
			{
				var generatedXml = File.ReadAllText(generatedFile);
				var expectedXml = TestHelper.ReadManifestResourceContent(expectedResource);

				Assert.That(generatedXml.EndsWith(expectedXml, StringComparison.InvariantCulture), "XML-file does not contain the correct information for the Additional References.");
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
