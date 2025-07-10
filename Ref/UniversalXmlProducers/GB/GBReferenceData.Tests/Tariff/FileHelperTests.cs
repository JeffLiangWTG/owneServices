using System;
using System.IO;
using CargoWise.RefDbRepo.GBReferenceData.Services.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff
{
	[TestFixture]
	class FileHelperTests
	{
		#region Get File List
		[Test]
		public void GetFileListWithNullClient()
		{
			Assert.Throws(Is.TypeOf<ArgumentNullException>().And.Message.StartsWith("Value cannot be null"), () => FileHelper.GetFileList(null, "Unit Test"));
		}

		[Test]
		public void GetFilesExceptionOccurs()
		{
			var webClientWrapper = new Mock<ITariffWebClientWrapper>();
			webClientWrapper.Setup(x => x.GetContent("ExceptionURL")).Throws<Exception>();

			Assert.Throws(Is.TypeOf<ApplicationException>().And.Message.StartsWith("Could not get file list for 'ExceptionURL'"), () => FileHelper.GetFileList(webClientWrapper.Object, "ExceptionURL"));
		}

		[Test]
		public void GetFileListNoContent()
		{
			var webClientWrapper = new Mock<ITariffWebClientWrapper>();
			webClientWrapper.Setup(x => x.GetContent("NoContent")).Returns(string.Empty);

			var result = FileHelper.GetFileList(webClientWrapper.Object, "NoContent");

			Assert.That(result, Is.Not.Null.And.Empty);
		}

		[Test]
		public void GetFileListWithInvalidContent()
		{
			var webClientWrapper = new Mock<ITariffWebClientWrapper>();
			webClientWrapper.Setup(x => x.GetContent("InvalidContent")).Returns("Invalid Content which is not a json file list");

			Assert.Throws(Is.TypeOf<ApplicationException>().And.Message.StartsWith("Could not convert content to file list"), () => FileHelper.GetFileList(webClientWrapper.Object, "InvalidContent"));
		}

		[Test]
		public void GetFileListNoFiles()
		{
			var webClientWrapper = new Mock<ITariffWebClientWrapper>();
			webClientWrapper.Setup(x => x.GetContent("NoFiles")).Returns("[]");

			var result = FileHelper.GetFileList(webClientWrapper.Object, "NoFiles");

			Assert.That(result, Is.Not.Null.And.Empty);
		}

		[Test]
		public void GetFileListWithValidContent()
		{
			var content = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.TestFiles.Input.FileListValidContent.json");
			var webClientWrapper = new Mock<ITariffWebClientWrapper>();
			webClientWrapper.Setup(x => x.GetContent("ValidContent")).Returns(content);

			var result = FileHelper.GetFileList(webClientWrapper.Object, "ValidContent");

			Assert.That(result, Is.Not.Null.And.Count.EqualTo(2));
			Assert.That(result[0].Filename, Is.EqualTo("file1.gz"));
			Assert.That(result[0].DownloadURL, Is.EqualTo("https://unittest?token=file111"));
			Assert.That(result[0].FileSize, Is.EqualTo(1212));

			Assert.That(result[1].Filename, Is.EqualTo("file2.gz"));
			Assert.That(result[1].DownloadURL, Is.EqualTo("https://unittest?token=file222"));
			Assert.That(result[1].FileSize, Is.EqualTo(2323));
		}
		#endregion

		#region Download File
		[Test]
		public void InvalidDownloadParameters()
		{
			var webClientWrapper = new Mock<ITariffWebClientWrapper>();
			var fileDetails = new FileDetails();

			Assert.Throws(Is.TypeOf<ArgumentNullException>().And.Message.StartsWith("Value cannot be null"), () => FileHelper.DownloadFile(null, fileDetails, "Unit Test"));
			Assert.Throws(Is.TypeOf<ArgumentNullException>().And.Message.StartsWith("Value cannot be null"), () => FileHelper.DownloadFile(webClientWrapper.Object, null, "Unit Test"));
			Assert.Throws(Is.TypeOf<ArgumentException>().And.Message.StartsWith("Invalid argument"), () => FileHelper.DownloadFile(webClientWrapper.Object, fileDetails, string.Empty));
		}

		[Test]
		public void IsFileDownloaded()
		{
			var fileDetails = new FileDetails { Filename = TempFile, FileSize = 10 };

			var localFile = Path.Combine(TempFolder, TempFile);

			Assert.That(FileHelper.IsFileDownloaded(localFile, fileDetails.FileSize), Is.EqualTo(false));

			using (var fs = File.Create(Path.Combine(TempFolder, TempFile)))
			{
				Assert.That(FileHelper.IsFileDownloaded(localFile, fileDetails.FileSize), Is.EqualTo(false));

				var content = new byte[10];
				fs.Write(content, 0, 10);

				fs.Flush();
			}

			Assert.That(FileHelper.IsFileDownloaded(localFile, fileDetails.FileSize), Is.EqualTo(true));
		}

		[Test]
		public void DownloadWebClientException()
		{
			var fileDetails = new FileDetails { Filename = TempFile, FileSize = 10, DownloadURL = "ExceptionURL" };
			var webClientWrapper = new Mock<ITariffWebClientWrapper>();
			webClientWrapper.Setup(x => x.DownloadFile("ExceptionURL", Path.Combine(TempFolder, TempFile))).Throws<Exception>();

			Assert.Throws(Is.TypeOf<ApplicationException>().And.Message.StartsWith($"Could not download file '{TempFile}' from 'ExceptionURL'"), () => FileHelper.DownloadFile(webClientWrapper.Object, fileDetails, TempFolder));
		}

		[Test]
		public void DownloadSuccessful()
		{
			var resource = "CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.TestFiles.Input.UT_tariff_dailyExtract_v1_20191001.gzip";
			var resourceLength = TestHelper.GetManifestResourceLength(resource);
			var downloadFile = Path.Combine(TempFolder, TempFile);

			var fileDetails = new FileDetails { Filename = TempFile, FileSize = resourceLength, DownloadURL = "ShouldDownloadAFile" };
			var webClientWrapper = new Mock<ITariffWebClientWrapper>();
			webClientWrapper.Setup(x => x.DownloadFile("ShouldDownloadAFile", Path.Combine(TempFolder, TempFile))).Callback(() => { TestHelper.SimulateDownload(downloadFile, resource); });

			var actualFileName = FileHelper.DownloadFile(webClientWrapper.Object, fileDetails, TempFolder);

			Assert.That(actualFileName, Is.EqualTo(downloadFile));

			var actualFile = new FileInfo(actualFileName);

			Assert.That(actualFile.Exists);
			Assert.That(actualFile.Length, Is.EqualTo(resourceLength));
		}

		[Test]
		public void IgnoreDownloadedFile()
		{
			var downloads = 0;
			var resource = "CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.TestFiles.Input.UT_tariff_dailyExtract_v1_20191001.gzip";
			var resourceLength = TestHelper.GetManifestResourceLength(resource);
			var downloadFile = Path.Combine(TempFolder, TempFile);

			var fileDetails = new FileDetails { Filename = TempFile, FileSize = resourceLength, DownloadURL = "ShouldDownloadAFile" };
			var webClientWrapper = new Mock<ITariffWebClientWrapper>();
			webClientWrapper.Setup(x => x.DownloadFile("ShouldDownloadAFile", Path.Combine(TempFolder, TempFile))).Callback(() => { TestHelper.SimulateDownload(downloadFile, resource); downloads++; });

			FileHelper.DownloadFile(webClientWrapper.Object, fileDetails, TempFolder);
			FileHelper.DownloadFile(webClientWrapper.Object, fileDetails, TempFolder);

			Assert.That(downloads, Is.EqualTo(1));
		}
		#endregion

		#region Unzip File
		[Test]
		public void UnzipValidFile()
		{
			var workingFolder = TempFolder;
			var zipFile = Path.Combine(workingFolder, TempFile);
			var outputPath = workingFolder;
			var xmlFile = Path.Combine(workingFolder, "export-20191004T000000_20191004T235959-20191005T200035.xml");

			TestHelper.SimulateDownload(zipFile, "CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.TestFiles.Input.UT_tariff_dailyExtract_v1_20191001.gzip");

			FileHelper.UnzipFile(zipFile, outputPath);

			Assert.That(File.Exists(xmlFile));
			var content = File.ReadAllText(xmlFile);

			Assert.That(content, Is.Not.Empty.And.Contains("<ns2:TariffHistoryResponse xmlns:ns2=\"http://www.eurodyn.com/Tariff/services/DispatchDataExportXMLData/v03\">"));
		}

		[Test]
		public void UnzipInvalidFile()
		{
			var workingFolder = TempFolder;
			var zipFile = Path.Combine(workingFolder, TempFile);
			var outputPath = workingFolder;

			TestHelper.SimulateDownload(zipFile, "CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.TestFiles.Input.FileListValidContent.json");

			Assert.Throws(Is.TypeOf<FileNotFoundException>().And.Message.StartsWith($"Failed to extract {zipFile}"), () => FileHelper.UnzipFile(zipFile, outputPath));
		}

		[Test]
		public void UnzipCorruptFile()
		{
			var workingFolder = TempFolder;
			var zipFile = Path.Combine(workingFolder, TempFile);
			var outputPath = workingFolder;

			TestHelper.SimulateDownload(zipFile, "CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.TestFiles.Input.UT_tariff_dailyExtract_v1_Corrupt.gzip");

			Assert.Throws(Is.TypeOf<FileNotFoundException>().And.Message.StartsWith($"Failed to extract {zipFile}"), () => FileHelper.UnzipFile(zipFile, outputPath));
		}

		[Test]
		public void UnzipInvalidFilePaths()
		{
			var workingFolder = TempFolder;
			var zipFile = Path.Combine(workingFolder, TempFile);
			var invalidZipFile = Path.Combine(workingFolder, "DoesNotExist" + TempFile);
			var randomFolder = Path.Combine(workingFolder, Path.GetRandomFileName());

			Assert.Throws(Is.TypeOf<FileNotFoundException>().And.Message.StartsWith($"Zip file '{invalidZipFile}' does not exist"), () => FileHelper.UnzipFile(invalidZipFile, randomFolder));

			TestHelper.SimulateDownload(zipFile, "CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.TestFiles.Input.UT_tariff_dailyExtract_v1_20191001.gzip");

			Assert.Throws(Is.TypeOf<DirectoryNotFoundException>().And.Message.StartsWith($"Output path '{randomFolder}' does not exist"), () => FileHelper.UnzipFile(zipFile, randomFolder));
		}
		#endregion

		#region Setup
		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		[SetUp]
		public void Setup()
		{
			TempFile = Path.GetRandomFileName();
			ContentFolder = Path.Combine(TempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(ContentFolder);
		}

		string TempFile;
		string TempFolder;
		string ContentFolder;
		#endregion
	}
}
