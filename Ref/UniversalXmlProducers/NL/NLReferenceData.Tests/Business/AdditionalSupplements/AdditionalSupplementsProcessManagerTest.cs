using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.NLReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NLReferenceData.Business.Testing
{
	[TestFixture]
	sealed class AdditionalSupplementsProcessManagerTest
	{
		[Test]
		public void ProcessCorrectXml()
		{
			TestHelper.SimulateDownload(Path.Combine(TempFolder, "Q.xlsx"), "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.AdditionalSupplements.Input.Q.xlsx");
			TestHelper.SimulateDownload(Path.Combine(TempFolder, "U.xlsx"), "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.AdditionalSupplements.Input.U.xlsx");
			TestHelper.SimulateDownload(Path.Combine(TempFolder, "V.xlsx"), "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.AdditionalSupplements.Input.V.xlsx");

			var fileList = new List<string>
			{
				Path.Combine(TempFolder, "Q.xlsx"),
				Path.Combine(TempFolder, "U.xlsx"),
				Path.Combine(TempFolder, "V.xlsx")
			};

			var errorCollector = new StringBuilder();

			var webDriverHelperWrapper = new Mock<IWebDriverHelperWrapper>();
			webDriverHelperWrapper.Setup(x => x.DownloadFiles(It.IsAny<string>())).Returns(fileList);

			var downloadManager = new DownloadManager(webDriverHelperWrapper.Object);
			var processManager = new Mock<AdditionalSupplementsProcessManager>();
			processManager.Setup(x => x.GetAdditionalSupplementsBuilder()).Returns(new AdditionalSupplementsBuilder(errorCollector));
			processManager.Setup(x => x.GetDownloadManager()).Returns(downloadManager);

			processManager.Object.RunProcess(TempFolder, errorCollector);
			downloadManager.Dispose();

			var generatedFiles = new List<string>(Directory.GetFiles(TempFolder));

			var generatedFile = generatedFiles.First(x => Path.GetFileName(x).StartsWith("NL CodeList - Additional Supplements_", StringComparison.InvariantCulture));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty), $"No XML file was generated. Error occurred: {errorCollector}");
			Assert.That(File.Exists(generatedFile), "No XML File was generated.");

			var generatedXml = File.ReadAllText(generatedFile);
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.AdditionalSupplements.Output.RefCusCodeList_AdditionalSupplements_Correct.xml");

			Assert.That(generatedXml.EndsWith(expectedXml, StringComparison.InvariantCulture), "XML-file does not contain the correct information for the Additional Supplements.");
		}

		[Test]
		public void InvalidData()
		{
			TestHelper.SimulateDownload(Path.Combine(TempFolder, "Invalid_Q.xlsx"), "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.AdditionalSupplements.Input.Invalid_Q.xlsx");
			TestHelper.SimulateDownload(Path.Combine(TempFolder, "Invalid_U.xlsx"), "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.AdditionalSupplements.Input.Invalid_U.xlsx");
			TestHelper.SimulateDownload(Path.Combine(TempFolder, "Invalid_V.xlsx"), "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.AdditionalSupplements.Input.Invalid_V.xlsx");

			var fileList = new List<string>
			{
				Path.Combine(TempFolder, "Invalid_Q.xlsx"),
				Path.Combine(TempFolder, "Invalid_U.xlsx"),
				Path.Combine(TempFolder, "Invalid_V.xlsx")
			};

			var errorCollector = new StringBuilder();

			var webDriverHelperWrapper = new Mock<IWebDriverHelperWrapper>();
			webDriverHelperWrapper.Setup(x => x.DownloadFiles(It.IsAny<string>())).Returns(fileList);

			var downloadManager = new DownloadManager(webDriverHelperWrapper.Object);
			var processManager = new Mock<AdditionalSupplementsProcessManager>();
			processManager.Setup(x => x.GetDownloadManager()).Returns(downloadManager);

			processManager.Object.RunProcess(TempFolder, errorCollector);
			downloadManager.Dispose();

			Assert.That(errorCollector.ToString(), Does.Contain("Downloaded file(s) has no code list for Additional Supplements"));
		}

		[Test]
		public void NotAllAdditionalSupplementsInData()
		{
			TestHelper.SimulateDownload(Path.Combine(TempFolder, "Q_2.xlsx"), "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.AdditionalSupplements.Input.Q_2.xlsx");
			TestHelper.SimulateDownload(Path.Combine(TempFolder, "U_2.xlsx"), "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.AdditionalSupplements.Input.U_2.xlsx");
			TestHelper.SimulateDownload(Path.Combine(TempFolder, "Invalid_V_2.xlsx"), "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.AdditionalSupplements.Input.Invalid_V_2.xlsx");

			var fileList = new List<string>
			{
				Path.Combine(TempFolder, "Q_2.xlsx"),
				Path.Combine(TempFolder, "U_2.xlsx"),
				Path.Combine(TempFolder, "Invalid_V_2.xlsx")
			};

			var errorCollector = new StringBuilder();

			var webDriverHelperWrapper = new Mock<IWebDriverHelperWrapper>();
			webDriverHelperWrapper.Setup(x => x.DownloadFiles(It.IsAny<string>())).Returns(fileList);

			var downloadManager = new DownloadManager(webDriverHelperWrapper.Object);
			var processManager = new Mock<AdditionalSupplementsProcessManager>();
			processManager.Setup(x => x.GetDownloadManager()).Returns(downloadManager);

			processManager.Object.RunProcess(TempFolder, errorCollector);
			downloadManager.Dispose();

			Assert.That(errorCollector.ToString(), Does.Contain("Downloaded file(s) does not contain a code list for all Additional Supplements Q, U and V"));
		}

		[Test]
		public void NoExcelFilesDownloaded()
		{
			var errorCollector = new StringBuilder();

			var webDriverHelperWrapper = new Mock<IWebDriverHelperWrapper>();
			webDriverHelperWrapper.Setup(x => x.DownloadFiles(It.IsAny<string>())).Returns(new List<string>());

			var downloadManager = new DownloadManager(webDriverHelperWrapper.Object);
			var processManager = new Mock<AdditionalSupplementsProcessManager>();
			processManager.Setup(x => x.GetDownloadManager()).Returns(downloadManager);

			processManager.Object.RunProcess(TempFolder, errorCollector);
			downloadManager.Dispose();

			Assert.That(errorCollector.ToString(), Does.Contain("No Excel file(s) downloaded for Additional Supplements"));
		}

		[SetUp]
		public void Setup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			TempFolderDownload = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
			Directory.CreateDirectory(TempFolderDownload);
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
		string TempFolderDownload;
	}
}
