using System.IO;
using CargoWise.RefDbRepo.BEReferenceData.Business.Testing;
using CargoWise.RefDbRepo.BEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BEReferenceData.Testing
{
	[TestFixture]
	sealed class FileHelperTest
	{
		[Test]
		public void TestFileExists()
		{
			Assert.That(FileHelper.FileExists(TempFolder, "*.zip"), Is.False);
			TestHelper.SimulateDownload(Path.Combine(TempFolder, "CorruptMonthlyMeasures.zip"), "CargoWise.RefDbRepo.BEReferenceData.Tests.Tariff.TestFiles.Input.CorruptMonthlyMeasures.zip");
			Assert.That(FileHelper.FileExists(TempFolder, "*.zip"), Is.True);
		}

		[Test]
		public void TestUnzipFile()
		{
			var corruptMonthlyMeasures = Path.Combine(TempFolder, "CorruptMonthlyMeasures.zip");
			TestHelper.SimulateDownload(corruptMonthlyMeasures, "CargoWise.RefDbRepo.BEReferenceData.Tests.Tariff.TestFiles.Input.CorruptMonthlyMeasures.zip");
			Assert.That(FileHelper.UnzipFile(corruptMonthlyMeasures, ContentFolder), Is.Empty);

			var monthlyMeasures = Path.Combine(TempFolder, "export-20240401T000000-20240401T000500.zip");
			TestHelper.SimulateDownload(monthlyMeasures, "CargoWise.RefDbRepo.BEReferenceData.Tests.Tariff.TestFiles.Input.export-20240401T000000-20240401T000500.zip");
			Assert.That(FileHelper.UnzipFile(monthlyMeasures, ContentFolder), !Is.Empty);
		}

		[OneTimeSetUp]
		public void OnewTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			ContentFolder = Path.Combine(TempFolder, "Content");
			Directory.CreateDirectory(ContentFolder);
		}

		[OneTimeTearDown]
		public void OnewTimeTearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		string TempFolder;
		string ContentFolder;
	}
}
