using System;
using System.IO;
using System.Net;
using CargoWise.RefDbRepo.GBReferenceData.Services;
using CargoWise.RefDbRepo.GBReferenceData.Services.Tariff;
using Moq;
using NUnit.Framework;
using static CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.Helpers.TestHelperClasses;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff
{
	[TestFixture]
	class DownloadManagerTests
	{
		[Test]
		public void GetFileListDownloadAndUnzip()
		{
			var webClientWrapper = new Mock<ITariffWebClientWrapper>();

			var contentDaily = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.TestFiles.Input.FileListDaily.json");
			var contentMonthly = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.TestFiles.Input.FileListMonthly.json");
			var contentAnnual = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.TestFiles.Input.FileListAnnual.json");

			webClientWrapper.Setup(x => x.GetContent(ConfigurationProvider.TariffDailyUrl)).Returns(contentDaily);
			webClientWrapper.Setup(x => x.GetContent(ConfigurationProvider.TariffMonthlyUrl)).Returns(contentMonthly);
			webClientWrapper.Setup(x => x.GetContent(ConfigurationProvider.TariffAnnualUrl)).Returns(contentAnnual);

			webClientWrapper.Setup(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((url, localFile) => { TestHelper.SimulateDownload(localFile, url); });

			downloadManager.TestWebClientWrapper = webClientWrapper.Object;
			downloadManager.RunDownloadProcess(ContentFolder);

			var expectedZipFiles = new string[]
			{
				"UT_tariff_dailyExtract_v1_20191001.gzip", // PkZip
				"UT_tariff_dailyExtract_v1_20191002.gzip",
				"UT_tariff_monthlyExtract_v1_20191001.gzip",
				"UT_tariff_monthlyExtract_v1_20191002.gzip",
				"UT_tariff_yearlyExtract_v1_20191002.gzip" // GZip
			};

			foreach (var f in expectedZipFiles)
			{
				var fi = new FileInfo(Path.Combine(TempFolder, f));
				Assert.That(fi.Exists, $"Filename {f}");
				Assert.That(fi.Length > 0, $"Filename {f}");
			}

			var expectedContentFiles = new string[]
			{
				"export-20191004T000000_20191004T235959-20191005T200035.xml",
				"export-20191010T000000_20191010T235959-20191011T200035.xml",
				"export-20191011T000000_20191011T235959-20191012T200035.xml",
				"export-20191012T000000_20191012T235959-20191013T200037.xml",
				"UT_tariff_yearlyExtract_v1_20191002.xml"
			};

			foreach (var f in expectedContentFiles)
			{
				var fi = new FileInfo(Path.Combine(ContentFolder, f));
				Assert.That(fi.Exists, $"Filename {f}");
				Assert.That(fi.Length > 0, $"Filename {f}");
			}
		}

		[Test]
		public void AbortOnFailure()
		{
			var webClientWrapper = new Mock<ITariffWebClientWrapper>();

			var contentMonthly = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.TestFiles.Input.FileListMonthly.json");

			webClientWrapper.Setup(x => x.GetContent(ConfigurationProvider.TariffDailyUrl)).Returns("[]");
			webClientWrapper.Setup(x => x.GetContent(ConfigurationProvider.TariffMonthlyUrl)).Returns(contentMonthly);
			webClientWrapper.Setup(x => x.GetContent(ConfigurationProvider.TariffAnnualUrl)).Returns("[]");

			webClientWrapper.Setup(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((url, localFile) =>
			{
				if (url.Contains("UT_tariff_monthlyExtract_v1_20191001.gzip"))
				{
					throw new Exception("Simulated failure on first monthly file");
				}

				TestHelper.SimulateDownload(localFile, url);
			});

			downloadManager.TestWebClientWrapper = webClientWrapper.Object;

			Assert.Throws(Is.TypeOf<ApplicationException>().And.Message.Contains($"Processing of files in {TempFolder} failed"), () => downloadManager.RunDownloadProcess(ContentFolder));
			Assert.That(!File.Exists(Path.Combine(TempFolder, "UT_tariff_monthlyExtract_v1_20191002.gzip")));
		}

		[Test]
		public void ExclusionList()
		{
			var webClientWrapper = new Mock<ITariffWebClientWrapper>();

			var contentDaily = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.TestFiles.Input.FileListWithExclusion.json");

			webClientWrapper.Setup(x => x.GetContent(ConfigurationProvider.TariffDailyUrl)).Returns(contentDaily);
			webClientWrapper.Setup(x => x.GetContent(ConfigurationProvider.TariffMonthlyUrl)).Returns("[]");
			webClientWrapper.Setup(x => x.GetContent(ConfigurationProvider.TariffAnnualUrl)).Returns("[]");

			var files = downloadManager.GetFileList(webClientWrapper.Object);

			Assert.That(files.Count, Is.EqualTo(1));
		}

		[Test]
		public void ContentFolderCreation()
		{
			var webClientWrapper = new Mock<ITariffWebClientWrapper>();

			webClientWrapper.Setup(x => x.GetContent(ConfigurationProvider.TariffDailyUrl)).Returns("[]");
			webClientWrapper.Setup(x => x.GetContent(ConfigurationProvider.TariffMonthlyUrl)).Returns("[]");
			webClientWrapper.Setup(x => x.GetContent(ConfigurationProvider.TariffAnnualUrl)).Returns("[]");

			Assert.That(!Directory.Exists(ContentFolder));

			downloadManager.TestWebClientWrapper = webClientWrapper.Object;
			downloadManager.RunDownloadProcess(ContentFolder);
			Assert.That(Directory.Exists(ContentFolder));
		}

		[Test]
		public void CleanUpOldFiles()
		{
			var downloads = 0;

			var file1 = Path.Combine(TempFolder, "ThisFileShouldBeDeleted.txt");
			TestHelper.SimulateDownload(file1, "CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.TestFiles.Input.FileListMonthly.json");

			var file2 = Path.Combine(TempFolder, "UT_tariff_dailyExtract_v1_20191001.gzip");
			TestHelper.SimulateDownload(file2, "CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.TestFiles.Input.UT_tariff_dailyExtract_v1_20191001.gzip");

			var contentDaily = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.TestFiles.Input.FileListWithExclusion.json");

			var webClientWrapper = new Mock<ITariffWebClientWrapper>();
			webClientWrapper.Setup(x => x.GetContent(ConfigurationProvider.TariffDailyUrl)).Returns(contentDaily);
			webClientWrapper.Setup(x => x.GetContent(ConfigurationProvider.TariffMonthlyUrl)).Returns("[]");
			webClientWrapper.Setup(x => x.GetContent(ConfigurationProvider.TariffAnnualUrl)).Returns("[]");

			webClientWrapper.Setup(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((url, localFile) => downloads++);

			downloadManager.TestWebClientWrapper = webClientWrapper.Object;
			downloadManager.RunDownloadProcess(ContentFolder);

			Assert.That(!File.Exists(file1));
			Assert.That(File.Exists(file2));
			Assert.That(downloads, Is.EqualTo(0));
		}

		[Test]
		public void GetAuthorisationHeader()
		{
			var webClientWrapper = new Mock<ITariffWebClientWrapper>();

			var authResponse = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.TestFiles.Output.UT_AuthResponse_001.json");

			webClientWrapper.Setup(x => x.GetContentFromPost(It.IsAny<string>(), It.IsAny<System.Collections.Specialized.NameValueCollection>())).Returns(authResponse);

			var result = downloadManager.GetAuthorisationHeader(webClientWrapper.Object);
			Assert.That(result, Is.EqualTo("bearer ThisIsJustARandomToken4TestingPurposesOK"));
		}

		[Test]
		public void GetAuthorisationFailure()
		{
			var webClientWrapper = new Mock<ITariffWebClientWrapper>();
			webClientWrapper.Setup(x => x.GetContentFromPost(It.IsAny<string>(), It.IsAny<System.Collections.Specialized.NameValueCollection>())).Returns<string, System.Collections.Specialized.NameValueCollection>((s, nvc) => { throw new Exception("Simulated auth failure"); });

			downloadManager.TestWebClientWrapper = webClientWrapper.Object;
			Assert.Throws(Is.TypeOf<ApplicationException>().And.Message.Contains("Failed to retrieve authorisation token"), () => downloadManager.RunDownloadProcess(ContentFolder));
		}

		[Test]
		public void RetryDownloadFailed()
		{
			var webClientWrapper = new Mock<ITariffWebClientWrapper>();
			var contentMonthly = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.TestFiles.Input.FileListMonthly.json");
			webClientWrapper.Setup(x => x.GetContent(ConfigurationProvider.TariffDailyUrl)).Returns("[]");
			webClientWrapper.Setup(x => x.GetContent(ConfigurationProvider.TariffMonthlyUrl)).Returns(contentMonthly);
			webClientWrapper.Setup(x => x.GetContent(ConfigurationProvider.TariffAnnualUrl)).Returns("[]");
			webClientWrapper.Setup(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((url, localFile) =>
			{
				if (url.Contains("UT_tariff_monthlyExtract_v1_20191001.gzip"))
				{
					throw new WebException("The remote server returned an error: (503) Service Unavailable");
				}
				TestHelper.SimulateDownload(localFile, url);
			});

			downloadManager.TestWebClientWrapper = webClientWrapper.Object;
			Assert.Throws(Is.TypeOf<ApplicationException>().And.Message.StartsWith($"The following processing errors occurred"), () => downloadManager.RunDownloadProcess(ContentFolder));

			var output = downloadManager.ConsoleOuput.ToString();
			StringAssert.Contains("Retry 1", output);
			StringAssert.Contains("Retry 2", output);
			StringAssert.Contains("Retry 3", output);
			StringAssert.Contains("Retry 4", output);
		}

		[Test]
		public void RetryDownloadSucceeded()
		{
			var filename = "UT_tariff_monthlyExtract_v1_20191001.gzip";
			var webClientWrapper = new Mock<ITariffWebClientWrapper>();
			var contentMonthly = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.TestFiles.Input.FileListMonthly.json");
			webClientWrapper.Setup(x => x.GetContent(ConfigurationProvider.TariffDailyUrl)).Returns("[]");
			webClientWrapper.Setup(x => x.GetContent(ConfigurationProvider.TariffMonthlyUrl)).Returns(contentMonthly);
			webClientWrapper.Setup(x => x.GetContent(ConfigurationProvider.TariffAnnualUrl)).Returns("[]");

			var attempt = 1;
			webClientWrapper.Setup(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((url, localFile) =>
			{
				if (url.Contains(filename) && attempt < 3)
				{
					attempt++;
					throw new WebException("The remote server returned an error: (503) Service Unavailable");
				}

				TestHelper.SimulateDownload(localFile, url);
			});

			downloadManager.TestWebClientWrapper = webClientWrapper.Object;
			Assert.DoesNotThrow(() => downloadManager.RunDownloadProcess(ContentFolder));

			var output = downloadManager.ConsoleOuput.ToString();
			StringAssert.Contains("Retry 1", output);
			StringAssert.Contains("Retry 2", output);

			var fileInfo = new FileInfo(Path.Combine(TempFolder, filename));
			Assert.That(fileInfo.Exists, $"Filename {filename}");
			Assert.That(fileInfo.Length > 0, $"Filename {filename}");
		}

		[SetUp]
		public void Setup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			ContentFolder = Path.Combine(TempFolder, "Content");
			Directory.CreateDirectory(TempFolder);
			downloadManager = new DownloadManagerTester(TempFolder);
		}

		[TearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		DownloadManagerTester downloadManager;
		string TempFolder;
		string ContentFolder;
	}
}
