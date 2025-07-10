using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.NLReferenceData.Business.Testing;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NLReferenceData.Services.Testing
{
	[TestFixture]
	sealed class DownloadManagerHelperTest
	{
		[Test]
		public void PrepareEnvironment()
		{
			var downloadPath = Path.Combine(TempFolder, "PrepareEnvironment");
			DownloadManagerHelper.PrepareEnvironment(downloadPath);
			Assert.IsTrue(Directory.Exists(downloadPath));
		}

		[Test]
		public void RemoveUnexpectedFiles_ByExtention()
		{
			TestHelper.SimulateDownload(Path.Combine(TempFolder, "TestFile.xml"), "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Download.Input.TestFile.zip");
			TestHelper.SimulateDownload(Path.Combine(TempFolder, "TestFile.txt"), "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Download.Input.TestFile.txt");
			TestHelper.SimulateDownload(Path.Combine(TempFolder, "TestFile.log"), "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Download.Input.TestFile.log");

			var fileList = new List<string>()
			{
				Path.Combine(TempFolder, "TestFile.xml"),
				Path.Combine(TempFolder, "TestFile.txt"),
				Path.Combine(TempFolder, "TestFile.log")
			};

			DownloadManagerHelper.RemoveUnexpectedFiles(".txt", fileList);

			Assert.AreEqual(1, fileList.Count, "Only 1 file is expected, not all files have been removed");
			Assert.AreEqual(Path.Combine(TempFolder, "TestFile.txt"), fileList[0]);
			Assert.IsTrue(File.Exists(Path.Combine(TempFolder, "TestFile.txt")), "TestFile.txt is expected to exist, but was removed.");
			Assert.IsFalse(File.Exists(Path.Combine(TempFolder, "TestFile.zip")), "TestFile.zip is expected to be deleted, but it still exists");
			Assert.IsFalse(File.Exists(Path.Combine(TempFolder, "TestFile.log")), "TestFile.log is expected to be deleted, but it still exists");
		}

		[Test]
		public void RemoveUnexpectedFiles_ByFiles()
		{
			TestHelper.SimulateDownload(Path.Combine(TempFolder, "dummy1.zip"), "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.dummy1.zip");
			TestHelper.SimulateDownload(Path.Combine(TempFolder, "dummy4.zip"), "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.dummy4.zip");

			var fileList = new List<string>()
			{
				Path.Combine(TempFolder, "dummy1.zip"),
				Path.Combine(TempFolder, "dummy4.zip")
			};

			var expectedFiles = new List<string>()
			{
				"dummy1.zip",
				"dummy2.zip",
				"dummy3.zip"
			};

			DownloadManagerHelper.RemoveUnexpectedFiles(expectedFiles, fileList);

			Assert.IsTrue(File.Exists(Path.Combine(TempFolder, "dummy1.zip")), "dummy1.zip is expected to exist, but was removed.");
			Assert.IsFalse(File.Exists(Path.Combine(TempFolder, "dummy4.zip")), "dummy4.zip is expected to be deleted, but it still exists");
		}

		[Test]
		public void RemoveExpectedFiles()
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

			DownloadManagerHelper.RemoveExpectedFiles(fileList);

			Assert.AreEqual(0, fileList.Count, "No files expected, not all files have been removed");
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
