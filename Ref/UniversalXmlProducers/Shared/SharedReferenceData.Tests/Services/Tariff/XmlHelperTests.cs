using System;
using System.IO;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.SharedReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Tests
{
	[TestFixture]
	class XmlHelperTests
	{
		[Test]
		public void ValidateFileFileNotFound()
		{
			var filePath = Path.Combine(ContentFolder, Path.GetRandomFileName(), "DoesNot.Exist");

			var exception = Assert.Throws(Is.TypeOf<InvalidOperationException>().And.Message.StartsWith($"Failed to validate file {filePath}"), () => xmlHelper.ValidateFile(filePath));
			Assert.That(exception.GetBaseException(), Is.TypeOf<InvalidOperationException>().And.Message.StartsWith("File does not exist"));
		}

		[Test]
		public void ValidateFileFileLocked()
		{
			var filePath = Path.Combine(ContentFolder, "Locked.file");
			using (var fs = File.Create(filePath))
			{
				var bytes = System.Text.Encoding.ASCII.GetBytes("Simulate Locked File");
				fs.Write(bytes, 0, bytes.Length);
				var exception = Assert.Throws(Is.TypeOf<InvalidOperationException>().And.Message.StartsWith($"Failed to validate file {filePath}"), () => xmlHelper.ValidateFile(filePath));
				Assert.That(exception.GetBaseException(), Is.TypeOf<IOException>().And.Message.StartsWith($"The process cannot access the file '{filePath}'"));
			}
		}

		[Test]
		public void ValidateFileInvalidSchema()
		{
			var filePath = Path.Combine(ContentFolder, "Invalid_Schema.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.Invalid_Schema.xml");

			var exception = Assert.Throws(Is.TypeOf<InvalidOperationException>().And.Message.StartsWith($"Failed to validate file {filePath}"), () => xmlHelper.ValidateFile(filePath));
			Assert.That(exception.GetBaseException(), Is.TypeOf<InvalidOperationException>().And.Message.StartsWith("Invalid schema"));
		}

		[Test]
		public void ValidateFileValid()
		{
			var filePath = Path.Combine(ContentFolder, "Content_001.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.Content_001.xml");

			Assert.DoesNotThrow(() => xmlHelper.ValidateFile(filePath));
		}

		[Test]
		public void ReadValidContent()
		{
			using (var stream = TestHelper.GetResourceStream("CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.Content_001.xml"))
			{
				var contentDetails = xmlHelper.ReadNext<ContentDetails>(stream, "ResultsInfo");

				Assert.That(contentDetails, Is.Not.Null);
				Assert.That(contentDetails.TotalRecords, Is.EqualTo(9));
				Assert.That(contentDetails.ExecutionDate, Is.EqualTo(new DateTime(2019, 10, 05, 20, 00, 35)));
				Assert.That(contentDetails.StartDate, Is.EqualTo(new DateTime(2019, 10, 04, 00, 00, 00)));
				Assert.That(contentDetails.EndDate, Is.EqualTo(new DateTime(2019, 10, 04, 23, 59, 59)));
			}
		}

		[Test]
		public void ReadInvalidContent()
		{
			using (var stream = TestHelper.GetResourceStream("CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.Invalid_Schema.xml"))
			{
				var contentDetails = new ContentDetails { TotalRecords = 1 };
				Assert.DoesNotThrow(() => { contentDetails = xmlHelper.ReadNext<ContentDetails>(stream, "ResultsInfo"); });
				Assert.That(contentDetails, Is.Null);
			}
		}

		[Test]
		public void ReadEndOfStream()
		{
			using (var stream = TestHelper.GetResourceStream("CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.Content_001.xml"))
			{
				var contentDetails = new ContentDetails { TotalRecords = 1 };

				stream.Seek(0, SeekOrigin.End);
				Assert.DoesNotThrow(() => { contentDetails = xmlHelper.ReadNext<ContentDetails>(stream, "ResultsInfo"); });
				Assert.That(contentDetails, Is.Null);
			}
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			xmlHelper = new XmlHelper();
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
			ContentFolder = Path.Combine(TempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(ContentFolder);
		}

		string TempFolder;
		string ContentFolder;
		XmlHelper xmlHelper;
	}
}
