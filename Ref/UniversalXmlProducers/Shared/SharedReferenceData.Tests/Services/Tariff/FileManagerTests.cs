using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.SharedReferenceData.Tests;
using CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.Helpers;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Tests
{
	[TestFixture]
	class FileManagerTests
	{
		[Test]
		public void GetFileProcessingList()
		{
			var contentFolder = SetupContentFolder();
			var subFolder = Path.Combine(contentFolder, "SubFolder");
			Directory.CreateDirectory(subFolder);

			var file1 = Path.Combine(contentFolder, "UT_Content_001.xml");
			var file2 = Path.Combine(contentFolder, "UT_Content_002.xml");
			var file3 = Path.Combine(contentFolder, "ShouldExcludeMe.txt");
			var file4 = Path.Combine(subFolder, "ShouldFindMe.xml");

			TestHelper.SimulateDownload(file1, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.Content_001.xml");
			TestHelper.SimulateDownload(file2, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.Content_002.xml");
			TestHelper.SimulateDownload(file3, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.Content_002.xml");
			TestHelper.SimulateDownload(file4, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.Content_004.xml");

			var files = FileManagerForTest.GetFileProcessingList(contentFolder);

			Assert.That(files, Is.Not.Null.And.Count.EqualTo(3));
		}

		[Test]
		public void GetFileProcessingListInvalidPath()
		{
			var contentPath = Path.Combine(TempFolder, Path.GetRandomFileName());
			var fileManager = new FileManagerForTest(contentPath);

			var exception = Assert.Throws(Is.TypeOf<InvalidOperationException>().And.Message.StartsWith($"Failed to retrieve file list from {contentPath}"), () => FileManagerForTest.GetFileProcessingList(contentPath));
			Assert.That(exception.GetBaseException(), Is.TypeOf<DirectoryNotFoundException>().And.Message.StartsWith($"Could not find a part of the path '{contentPath}'"));
		}

		[Test]
		public void GetContentDetails()
		{
			var contentFolder = SetupContentFolder();
			var fileManager = new FileManagerForTest(contentFolder);
			var filePath = Path.Combine(contentFolder, "Content_001.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.Content_001.xml");

			var contentDetails = fileManager.GetContentDetails(filePath);

			Assert.That(contentDetails, Is.Not.Null);
			Assert.That(contentDetails.TotalRecords, Is.EqualTo(9));
			Assert.That(contentDetails.ExecutionDate, Is.EqualTo(new DateTime(2019, 10, 05, 20, 00, 35)));
			Assert.That(contentDetails.StartDate, Is.EqualTo(new DateTime(2019, 10, 04, 00, 00, 00)));
			Assert.That(contentDetails.EndDate, Is.EqualTo(new DateTime(2019, 10, 04, 23, 59, 59)));
			Assert.That(contentDetails.DatabaseDate, Is.EqualTo(DateTime.MinValue));
		}

		[Test]
		public void SortFileList()
		{
			var fileList = new List<FileDetails>()
			{
				new FileDetails { Content = new ContentDetails { TotalRecords = 4, StartDate = new DateTime(2019,03,03, 00,00,00), EndDate = new DateTime(2019,03,03, 23,59,59), ExecutionDate = new DateTime(2019,03,07, 22,02,04) }, Filename = "ReprocessDaily3" },
				new FileDetails { Content = new ContentDetails { TotalRecords = 5, StartDate = new DateTime(2019,03,01, 13,00,00), EndDate = new DateTime(2019,03,02, 07,59,59), ExecutionDate = new DateTime(2019,03,04, 20,02,04) }, Filename = "Overlap" },
				new FileDetails { Content = new ContentDetails { TotalRecords = 7, StartDate = new DateTime(2019,02,01, 00,00,00), EndDate = new DateTime(2019,02,28, 23,59,59), ExecutionDate = new DateTime(2019,03,05, 20,02,04) }, Filename = "Monthly2" },
				new FileDetails { Content = new ContentDetails { TotalRecords = 1, StartDate = new DateTime(2019,03,02, 00,00,00), EndDate = new DateTime(2019,03,02, 23,59,59), ExecutionDate = new DateTime(2019,03,04, 20,02,04) }, Filename = "Daily2" },
				new FileDetails { Content = new ContentDetails { TotalRecords = 9, StartDate = new DateTime(2018,01,01, 00,00,00), EndDate = new DateTime(2018,12,31, 23,59,59), ExecutionDate = new DateTime(2019,01,05, 20,01,53) }, Filename = "Annual1" },
				new FileDetails { Content = new ContentDetails { TotalRecords = 4, StartDate = new DateTime(2019,03,03, 00,00,00), EndDate = new DateTime(2019,03,03, 23,59,59), ExecutionDate = new DateTime(2019,03,05, 20,02,04) }, Filename = "Daily3" },
				new FileDetails { Content = new ContentDetails { TotalRecords = 2, StartDate = new DateTime(2019,03,01, 00,00,00), EndDate = new DateTime(2019,03,01, 23,59,59), ExecutionDate = new DateTime(2019,03,03, 20,02,04) }, Filename = "Daily1" },
				new FileDetails { Content = new ContentDetails { TotalRecords = 4, StartDate = new DateTime(2019,01,23, 00,00,00), EndDate = new DateTime(2019,01,23, 23,59,59), ExecutionDate = new DateTime(2019,03,10, 22,02,04) }, Filename = "ReprocessHistorical" },
				new FileDetails { Content = new ContentDetails { TotalRecords = 0, StartDate = new DateTime(2019,03,04, 00,00,00), EndDate = new DateTime(2019,03,04, 23,59,59), ExecutionDate = new DateTime(2019,03,06, 20,02,04) }, Filename = "NoRecords" },
				new FileDetails { Content = new ContentDetails { TotalRecords = 6, StartDate = new DateTime(2019,01,01, 00,00,00), EndDate = new DateTime(2019,01,31, 23,59,59), ExecutionDate = new DateTime(2019,02,05, 19,26,23) }, Filename = "Monthly1" },
				new FileDetails { Content = new ContentDetails { TotalRecords = 4, StartDate = new DateTime(2019,01,12, 00,00,00), EndDate = new DateTime(2019,01,14, 23,59,59), ExecutionDate = new DateTime(2019,01,20, 22,02,04) }, Filename = "IgnoreHistorical" },
				new FileDetails { Filename = "NoContent" }
			};

			var newList = FileManagerForTest.SortAndFilter(fileList);

			Assert.That(newList.Count, Is.EqualTo(8));
			Assert.That(newList[0].Filename, Is.EqualTo("Annual1"));
			Assert.That(newList[1].Filename, Is.EqualTo("Monthly1"));
			Assert.That(newList[2].Filename, Is.EqualTo("ReprocessHistorical"));
			Assert.That(newList[3].Filename, Is.EqualTo("Monthly2"));
			Assert.That(newList[4].Filename, Is.EqualTo("Daily1"));
			Assert.That(newList[5].Filename, Is.EqualTo("Overlap"));
			Assert.That(newList[6].Filename, Is.EqualTo("Daily2"));
			Assert.That(newList[7].Filename, Is.EqualTo("ReprocessDaily3"));
		}

		[Test]
		public void GetProcessingList()
		{
			var contentFolder = SetupContentFolder();
			var subFolder = Path.Combine(contentFolder, "SubFolder");
			Directory.CreateDirectory(subFolder);
			var fileManager = new FileManagerForTest(contentFolder);
			var file1 = Path.Combine(contentFolder, "Content_001.xml");
			TestHelper.SimulateDownload(file1, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.Content_001.xml");
			var file2 = Path.Combine(contentFolder, "Content_002.xml");
			TestHelper.SimulateDownload(file2, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.Content_002.xml");
			var file3 = Path.Combine(contentFolder, "Content_003.xml");
			TestHelper.SimulateDownload(file3, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.Content_003.xml");
			var file4 = Path.Combine(contentFolder, "Content_004.txt");
			TestHelper.SimulateDownload(file4, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.Content_002.xml");
			var file5 = Path.Combine(subFolder, "ShouldFindMe.xml");
			TestHelper.SimulateDownload(file5, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.Content_004.xml");

			var fileList = fileManager.GetProcessingList(contentFolder);

			Assert.That(fileList.Count, Is.EqualTo(3));
			Assert.That(fileList[0].Filename, Is.EqualTo(file5));
			Assert.That(fileList[1].Filename, Is.EqualTo(file3));
			Assert.That(fileList[2].Filename, Is.EqualTo(file1));
		}

		[Test]
		public void GetProcessingListException()
		{
			var contentFolder = SetupContentFolder();
			var fileManager = new FileManagerForTest(contentFolder);
			TestHelper.SimulateDownload(Path.Combine(contentFolder, "Invalid_Schema.xml"), "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.Invalid_Schema.xml");

			var exception = Assert.Throws(Is.TypeOf<InvalidOperationException>().And.Message.StartsWith($"Failed to get file processing list from {contentFolder}"), () => fileManager.GetProcessingList(contentFolder));
			Assert.That(exception.GetBaseException(), Is.TypeOf<InvalidOperationException>().And.Message.StartsWith("Invalid schema"));
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
		}

		string SetupContentFolder()
		{
			var cf = Path.Combine(TempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(cf);

			return cf;
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		string TempFolder;
	}
}
