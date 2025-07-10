using System;
using System.IO;
using CargoWise.RefDbRepo.BEReferenceData.Business.Testing;
using Moq;
using NUnit.Framework;
using static CargoWise.RefDbRepo.BEReferenceData.Services.Testing.TestHelperClasses;

namespace CargoWise.RefDbRepo.BEReferenceData.Services.Testing
{
	[TestFixture]
	sealed class DownloadManagerTest
	{
		[Test]
		public void GetDownloadAndUnzip()
		{
			var webClientWrapper = new Mock<IWebDriverHelperWrapper>();
			webClientWrapper.Setup(x => x.DownloadFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Callback<string, string, bool>((url, localFile, isMonthly) => { TestHelper.SimulateDownload(Path.Combine(TempFolder, "export-20240401T000000-20240401T000500.zip"), "CargoWise.RefDbRepo.BEReferenceData.Tests.Tariff.TestFiles.Input.export-20240401T000000-20240401T000500.zip"); });
			downloadManager.TestWebDriverHelperWrapper = webClientWrapper.Object;

			TestDownloadAndUnzipForDate(new DateTime(2024, 4, 1),
										new string[] { "export-20240401T000000-20240401T000500.zip" },
										new string[]
										{
											"MonthlyMeasures/export-20220901T000000-110CertificateType.xml",
											"MonthlyMeasures/export-20220901T000000-235MeasureType.xml",
											"MonthlyMeasures/export-20220901T000000-350MeasureConditionCode.xml",
											"MonthlyMeasures/Measures/export-20220901T000000-Measure-Part-001-FROM-230_VAT.xml",
											"MonthlyMeasures/Measures/export-20220901T000000-Measure-Part-230-FROM-230_Conditions_Both.xml"
										}
			);
		}

		[Test]
		public void GetDownloadAndUnzip_Missing()
		{
			TestHelper.SimulateDownload(Path.Combine(TempFolder, "export-20240401T000000-20240401T000500.zip"), "CargoWise.RefDbRepo.BEReferenceData.Tests.Tariff.TestFiles.Input.export-20240401T000000-20240401T000500.zip");
			TestHelper.SimulateDownload(Path.Combine(TempFolder, "export-20240402T000000_20240402T235959-20240403T000500.zip"), "CargoWise.RefDbRepo.BEReferenceData.Tests.Tariff.TestFiles.Input.export-20240402T000000_20240402T235959-20240403T000500.zip");

			var webClientWrapper = new Mock<IWebDriverHelperWrapper>();
			webClientWrapper.Setup(x => x.DownloadFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Callback<string, string, bool>((url, localFile, isMonthly) => { TestHelper.SimulateDownload(Path.Combine(TempFolder, "export-20240401T000000_20240401T235959-20240402T000500.zip"), "CargoWise.RefDbRepo.BEReferenceData.Tests.Tariff.TestFiles.Input.export-20240401T000000_20240401T235959-20240402T000500.zip"); });
			downloadManager.TestWebDriverHelperWrapper = webClientWrapper.Object;

			TestDownloadAndUnzipForDate(new DateTime(2024, 4, 3),
										new string[] { "export-20240401T000000-20240401T000500.zip", "export-20240401T000000_20240401T235959-20240402T000500.zip", "export-20240402T000000_20240402T235959-20240403T000500.zip" },
										new string[]
										{
											"MonthlyMeasures/export-20220901T000000-110CertificateType.xml",
											"MonthlyMeasures/export-20220901T000000-235MeasureType.xml",
											"MonthlyMeasures/export-20220901T000000-350MeasureConditionCode.xml",
											"MonthlyMeasures/Measures/export-20220901T000000-Measure-Part-001-FROM-230_VAT.xml",
											"MonthlyMeasures/Measures/export-20220901T000000-Measure-Part-230-FROM-230_Conditions_Both.xml",
											"DailyMeasures/export-20220902T000000-110CertificateType.xml",
											"DailyMeasures/export-20220902T000000-235MeasureType.xml",
											"DailyMeasures/export-20220902T000000-350MeasureConditionCode.xml",
											"DailyMeasures/Measures/export-20220902T000000-Measure-Part-001-FROM-230.xml",
											"DailyMeasures/export-20220903T000000-110CertificateType.xml",
											"DailyMeasures/export-20220903T000000-235MeasureType.xml",
											"DailyMeasures/export-20220903T000000-350MeasureConditionCode.xml",
											"DailyMeasures/Measures/export-20220903T000000-Measure-Part-001-FROM-230.xml"
										}
			);
		}

		void TestDownloadAndUnzipForDate(DateTime downloadDate, string[] expectedZipFiles, string[] expectedContentFiles)
		{
			downloadManager.RunDownloadProcess(ContentFolder, downloadDate);

			foreach (var f in expectedZipFiles)
			{
				var fi = new FileInfo(Path.Combine(TempFolder, f));
				Assert.That(fi.Exists, $"Filename {f}");
				Assert.That(fi.Length > 0, $"Filename {f}");
			}

			foreach (var f in expectedContentFiles)
			{
				var fi = new FileInfo(Path.Combine(ContentFolder, f));
				Assert.That(fi.Exists, $"Filename {f}");
				Assert.That(fi.Length > 0, $"Filename {f}");
			}
		}

		[Test]
		public void RemoveUnexpectedFilesKeepPreviousMonth()
		{
			var filePreviousMonth = Path.Combine(TempFolder, "export-20240301T000000-20240301T000500.zip");
			TestHelper.SimulateDownload(filePreviousMonth, "CargoWise.RefDbRepo.BEReferenceData.Tests.Tariff.TestFiles.Input.export-20240401T000000-20240401T000500.zip");
			var filePreviousMonthDaily = Path.Combine(TempFolder, "export-20240301T000000_20240301T235959-20240302T000500.zip");
			TestHelper.SimulateDownload(filePreviousMonthDaily, "CargoWise.RefDbRepo.BEReferenceData.Tests.Tariff.TestFiles.Input.export-20240401T000000_20240401T235959-20240402T000500.zip");

			var webClientWrapper = new Mock<IWebDriverHelperWrapper>();

			var fileCurrentMonth = Path.Combine(TempFolder, "export-20240401T000000-20240401T000500.zip");
			webClientWrapper.Setup(x => x.DownloadFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Callback<string, string, bool>((url, localFile, isMonthly) => { TestHelper.SimulateDownload(fileCurrentMonth, "CargoWise.RefDbRepo.BEReferenceData.Tests.Tariff.TestFiles.Input.CorruptMonthlyMeasures.zip"); });

			downloadManager.TestWebDriverHelperWrapper = webClientWrapper.Object;
			downloadManager.RunDownloadProcess(ContentFolder, new DateTime(2024, 4, 1));

			Assert.That(File.Exists(filePreviousMonth), filePreviousMonth);
			Assert.That(File.Exists(filePreviousMonthDaily), filePreviousMonthDaily);
			Assert.That(!File.Exists(fileCurrentMonth), fileCurrentMonth);
		}

		[Test]
		public void RemoveUnexpectedFilesKeepNewMonth()
		{
			var filePreviousMonth = Path.Combine(TempFolder, "export-20240301T000000-20240301T000500.zip");
			TestHelper.SimulateDownload(filePreviousMonth, "CargoWise.RefDbRepo.BEReferenceData.Tests.Tariff.TestFiles.Input.export-20240401T000000-20240401T000500.zip");
			var filePreviousMonthDaily = Path.Combine(TempFolder, "export-20240301T000000_20240301T235959-20240302T000500.zip");
			TestHelper.SimulateDownload(filePreviousMonthDaily, "CargoWise.RefDbRepo.BEReferenceData.Tests.Tariff.TestFiles.Input.export-20240401T000000_20240401T235959-20240402T000500.zip");

			var webClientWrapper = new Mock<IWebDriverHelperWrapper>();

			var fileCurrentMonth = Path.Combine(TempFolder, "export-20240401T000000-20240401T000500.zip");
			webClientWrapper.Setup(x => x.DownloadFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Callback<string, string, bool>((url, localFile, isMonthly) => { TestHelper.SimulateDownload(fileCurrentMonth, "CargoWise.RefDbRepo.BEReferenceData.Tests.Tariff.TestFiles.Input.export-20240401T000000-20240401T000500.zip"); });

			downloadManager.TestWebDriverHelperWrapper = webClientWrapper.Object;
			downloadManager.RunDownloadProcess(ContentFolder, new DateTime(2024, 4, 1));

			Assert.That(!File.Exists(filePreviousMonth), filePreviousMonth);
			Assert.That(!File.Exists(filePreviousMonthDaily), filePreviousMonthDaily);
			Assert.That(File.Exists(fileCurrentMonth), fileCurrentMonth);
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
