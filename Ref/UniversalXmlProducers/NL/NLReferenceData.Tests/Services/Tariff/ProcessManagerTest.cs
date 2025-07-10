using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.NLReferenceData.Business.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NLReferenceData.Services.Testing
{
	[TestFixture]
	sealed class TariffProcessManagerTests
	{
		[Test]
		public void ReadTariffDownloadElements()
		{
			var downloadedXml = TestHelper.GetResourceContentAsXmlDocument("CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.TestTariffFileList.xml");
			var downloadList = TariffProcessManager.ReadTariffDownloadElements(downloadedXml);

			Assert.AreEqual(downloadList.GetType(), typeof(List<TariffDownloadElement>), "Type");
			Assert.AreEqual(3, downloadList.Count, "List Count");
			Assert.AreEqual(new string[] { "dummy1.zip", "dummy2.zip", "dummy3.zip" }, downloadList.Select(x => x.FileName).ToArray(), "List FileName values");
			Assert.AreEqual(new string[] { "https://download.belastingdienst.nl/douane_sw/tariff/dummy1.zip", "https://download.belastingdienst.nl/douane_sw/tariff/dummy2.zip", "https://download.belastingdienst.nl/douane_sw/tariff/dummy3.zip" }, downloadList.Select(x => x.Url).ToArray(), "List Url values");
		}

		[Test]
		public void GetTariffZipFilesToBeProcessed()
		{
			var tempFolderTariffData = Path.Combine(TempFolder, "NLTariffData");
			Directory.CreateDirectory(tempFolderTariffData);
			var fileThu = Path.Combine(tempFolderTariffData, "nederlands_gebruikstarief-2023_09_28_22_00_03_514.zip");
			var fileFri = Path.Combine(tempFolderTariffData, "nederlands_gebruikstarief-2023_09_29_22_00_05_679.zip");
			var fileSat = Path.Combine(tempFolderTariffData, "nederlands_gebruikstarief-2023_09_30_12_46_15_940.zip");
			var fileMon = Path.Combine(tempFolderTariffData, "nederlands_gebruikstarief-2023_10_02_22_00_03_499.zip");
			var fileTue = Path.Combine(tempFolderTariffData, "nederlands_gebruikstarief-2023_10_03_22_00_11_448.zip");
			TestHelper.SimulateDownload(fileThu, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.nederlands_gebruikstarief-2023_09_28_22_00_03_514.zip");
			TestHelper.SimulateDownload(fileFri, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.nederlands_gebruikstarief-2023_09_29_22_00_05_679.zip");
			TestHelper.SimulateDownload(fileSat, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.nederlands_gebruikstarief-2023_09_30_12_46_15_940.zip");
			TestHelper.SimulateDownload(fileMon, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.nederlands_gebruikstarief-2023_10_02_22_00_03_499.zip");
			TestHelper.SimulateDownload(fileTue, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.nederlands_gebruikstarief-2023_10_03_22_00_11_448.zip");

			var files = TariffProcessManager.GetTariffZipFilesToBeProcessed(tempFolderTariffData);

			Assert.AreEqual(3, files.Count, "Files count");
			Assert.AreEqual(new string[] { "nederlands_gebruikstarief-2023_10_03_22_00_11_448.zip", "nederlands_gebruikstarief-2023_10_02_22_00_03_499.zip", "nederlands_gebruikstarief-2023_09_30_12_46_15_940.zip" }, files.Select(x => x.Name), "List files");
		}

		[Test]
		public void GetTariffZipFilesToBeProcessedWithSunday()
		{
			var tempFolderTariffData = Path.Combine(TempFolder, "NLTariffData");
			Directory.CreateDirectory(tempFolderTariffData);
			var fileThu = Path.Combine(tempFolderTariffData, "nederlands_gebruikstarief-2023_09_28_22_00_03_514.zip");
			var fileFri = Path.Combine(tempFolderTariffData, "nederlands_gebruikstarief-2023_09_29_22_00_05_679.zip");
			var fileSat = Path.Combine(tempFolderTariffData, "nederlands_gebruikstarief-2023_09_30_12_46_15_940.zip");
			var fileMon = Path.Combine(tempFolderTariffData, "nederlands_gebruikstarief-2023_10_02_22_00_03_499.zip");
			var fileTue = Path.Combine(tempFolderTariffData, "nederlands_gebruikstarief-2023_10_03_22_00_11_448.zip");
			var fileSun = Path.Combine(tempFolderTariffData, "nederlands_gebruikstarief-2024_01_07_22_00_11_448.zip");
			TestHelper.SimulateDownload(fileThu, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.nederlands_gebruikstarief-2023_09_28_22_00_03_514.zip");
			TestHelper.SimulateDownload(fileFri, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.nederlands_gebruikstarief-2023_09_29_22_00_05_679.zip");
			TestHelper.SimulateDownload(fileSat, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.nederlands_gebruikstarief-2023_09_30_12_46_15_940.zip");
			TestHelper.SimulateDownload(fileMon, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.nederlands_gebruikstarief-2023_10_02_22_00_03_499.zip");
			TestHelper.SimulateDownload(fileTue, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.nederlands_gebruikstarief-2023_10_03_22_00_11_448.zip");
			TestHelper.SimulateDownload(fileSun, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.nederlands_gebruikstarief-2024_01_07_22_00_11_448.zip");

			var files = TariffProcessManager.GetTariffZipFilesToBeProcessed(tempFolderTariffData);

			Assert.AreEqual(1, files.Count, "Files count");
			Assert.AreEqual(new string[] { "nederlands_gebruikstarief-2024_01_07_22_00_11_448.zip" }, files.Select(x => x.Name), "List files");
		}

		[Test]
		public void ExtractTariffZipFiles()
		{
			var tempFolderTariffData = Path.Combine(TempFolder, "NLTariffData");
			Directory.CreateDirectory(tempFolderTariffData);

			var fileIncremental1 = Path.Combine(tempFolderTariffData, "nederlands_gebruikstarief-2025_03_03_22_00_12_590.zip");
			var fileFull1 = Path.Combine(tempFolderTariffData, "nederlands_gebruikstarief-2025_02_28_20_30_26_783.zip");
			var fileIncremental2 = Path.Combine(tempFolderTariffData, "nederlands_gebruikstarief-2025_02_25_22_00_08_973.zip");
			var fileFull2 = Path.Combine(tempFolderTariffData, "nederlands_gebruikstarief-2025_02_22_12_50_33_072.zip");
			TestHelper.SimulateDownload(fileIncremental1, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.nederlands_gebruikstarief-2025_03_03_22_00_12_590.zip");
			TestHelper.SimulateDownload(fileFull1, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.nederlands_gebruikstarief-2025_02_28_20_30_26_783.zip");
			TestHelper.SimulateDownload(fileIncremental2, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.nederlands_gebruikstarief-2025_02_25_22_00_08_973.zip");
			TestHelper.SimulateDownload(fileFull2, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.nederlands_gebruikstarief-2025_02_22_12_50_33_072.zip");

			var fileInfoList = new List<FileInfo>
			{
				new FileInfo(fileIncremental1),
				new FileInfo(fileFull1),
				new FileInfo(fileIncremental2),
				new FileInfo(fileFull2),
			};

			var contentTempFolderTariffData = Path.Combine(tempFolderTariffData, "content");
			var files = TariffProcessManager.ExtractTariffZipFiles(fileInfoList, contentTempFolderTariffData);

			Assert.Multiple(() =>
			{
				Assert.AreEqual(new string[] { "MeasureConditionCode_20250222.xml", "IncrementalObjectTraderExport_20250225.xml", "MeasureConditionCode_20250228.xml", "IncrementalObjectTraderExport_20250303.xml" },
					files.Select(x => x.Name), "Files are in order - Name");

				Assert.That(files.Select(x => x.LastWriteTime), Is.Ordered.Ascending, "Files are in order - LastWriteTime");
			});
		}

		[Test]
		public void Cleanup()
		{
			var tempFolderTariffData = Path.Combine(TempFolder, "NLTariffData");
			var tempFolderTariffOutputDir = Path.Combine(TempFolder, "TariffOutputDir");

			var prManager = new Mock<TariffProcessManager>();
			prManager.Protected().Setup<string>("DownloadDir").Returns(tempFolderTariffData);
			prManager.Protected().Setup<string>("TariffOutputDirectory").Returns(tempFolderTariffOutputDir);

			Directory.CreateDirectory(tempFolderTariffData);
			Directory.CreateDirectory(tempFolderTariffOutputDir);

			prManager.Object.Cleanup();

			Assert.That(!Directory.Exists(tempFolderTariffData), "Cleanup should have run and deleted the TariffData folder.");
			Assert.That(!Directory.Exists(tempFolderTariffOutputDir), "Cleanup should have run and deleted the Tariff Output Dir.");
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
