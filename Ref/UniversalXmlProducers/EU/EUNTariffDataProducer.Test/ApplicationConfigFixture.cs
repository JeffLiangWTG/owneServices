using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	internal class ApplicationConfigFixture
	{
		[Test]
		public void GetExistingFileIfExists()
		{
			var tempDir = Path.Combine(Path.GetTempPath(), "ADBACDA7-085F-4BE5-993F-5A0967B5C7E5");
			if (!Directory.Exists(tempDir))
			{
				Directory.CreateDirectory(tempDir);
			}

			var filePath = Path.Combine(tempDir, "ApplicationConfigFixture_GetExistingFileIfExistsFile.txt");
			using (var file = File.Create(filePath))
			{
				var lastWriteTime = DateTime.Now;
				File.SetLastWriteTime(filePath, lastWriteTime);
				var lookup = ApplicationConfig.GetExistingFileIfExists(tempDir, new[] { "ApplicationConfigFixture_GetExistingFileIfExistsFile.txt" }, null).FirstOrDefault();
				Assert.AreEqual(filePath, lookup.DownloadPath);
				Assert.AreEqual(lastWriteTime, lookup.LastModificationTime);
				lookup = ApplicationConfig.GetExistingFileIfExists(tempDir, new string[0], new Regex(".*GetExistingFileIfExistsFile.*")).FirstOrDefault();
				Assert.AreEqual(filePath, lookup.DownloadPath);
				Assert.AreEqual(lastWriteTime, lookup.LastModificationTime);
			}
			File.Delete(filePath);
			Directory.Delete(tempDir);
		}

		[Test]
		public void GetDefaultValues()
		{
			Assert.False(bool.Parse(ApplicationConfig.GetSettingValue<bool>("NonExistentKey")));
		}

		[Test]
		public void SetNomenclatureUXmlFile()
		{
			ApplicationConfig.ConfigEnvironment();
			Assert.AreEqual("..\\..\\UXmlFiles\\EUNNomenclature.xml", ApplicationConfig.NomenclatureUXmlFile);
			ApplicationConfig.SetNomenclatureUXmlFile("SomePath");
			Assert.AreEqual("SomePath", ApplicationConfig.NomenclatureUXmlFile);
		}

		[Test]
		public void ValidMeasureTypeIdsForMeasureConditionInImportRate()
		{
			ApplicationConfig.ConfigEnvironment();
			Assert.AreEqual(
				"277, 410, 464, 465, 474, 475, 477, 482, 483, 484, 495, 496, 705, 707, 710, 711, 712, 713, 714, 719, 722, 724, 726, 728, 730, 745, 746, 747, 748, 750, 755, 760, 761, 762, 763, 769, 774, 776",
				string.Join(", ", ApplicationConfig.ValidMeasureTypeIdsForMeasureConditionInImportRate));
		}

		[Test]
		public void MeasureConditionsCS01678500FilePath()
		{
			ApplicationConfig.ConfigEnvironment();

			Assert.That(ApplicationConfig.MeasureConditionsCS01678500FilePath, Is.EqualTo("Resources\\Measure Conditions CS01678500.xlsx"));
		}

		[Test]
		public void GetMissingConditionsFile()
		{
			ApplicationConfig.ConfigEnvironment();

			var webFileInfo = ApplicationConfig.GetMissingConditionsFile();
			Assert.That(webFileInfo, Is.Not.Null);
			Assert.That(webFileInfo.FileName, Is.EqualTo("Measure Conditions CS01678500.xlsx"));
			Assert.That(webFileInfo.Exception, Is.Null);
		}
	}
}
