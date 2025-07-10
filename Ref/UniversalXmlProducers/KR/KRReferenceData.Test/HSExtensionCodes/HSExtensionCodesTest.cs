using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	[TestFixture]
	sealed class HSExtensionCodesTest
	{
		[Test]
		public void TestHSExtensionCodes_2020()
		{
			AssertHSExtensionCodes(2020, 01, 01, false);
		}

		[Test]
		public void TestHSExtensionCodes_2021()
		{
			AssertHSExtensionCodes(2021, 01, 01, true);
		}

		[Test]
		public void TestHSExtensionCodes_2022()
		{
			AssertHSExtensionCodes(2022, 01, 01, false);
		}

		[Test]
		public void TestHSExtensionCodes_2023()
		{
			AssertHSExtensionCodes(2023, 01, 01, false);
		}

		[Test]
		public void TestHSExtensionCodes_2024()
		{
			AssertHSExtensionCodes(2024, 01, 01, false);
		}

		[Test]
		public void TestHSExtensionCodes_2025()
		{
			AssertHSExtensionCodes(2025, 01, 01, false);
		}

		void AssertHSExtensionCodes(int year, int month, int day, bool isXls)
		{
			var publicationDate = new DateTime(year, month, day);
			var fileExtension = isXls ? "xls" : "xlsx";
			var mainInputConfigPath = ApplicationConfig.HSExtensionCodeConfigFileMainInputPath;
			var subInputConfigPath = ApplicationConfig.HSExtensionCodeConfigFileSubInputPath;
			var outputDirPath = Path.Combine(TestHelper.BaseTestFilePath, @"HSExtensionCodes\Output\Y" + year);
			var outputDirInfo = new DirectoryInfo(outputDirPath);
			if (outputDirInfo.Exists)
			{
				foreach (var file in outputDirInfo.GetFiles())
				{
					file.Delete();
				}
			}

			var expectedOutputFiles = new string[]
			{
					TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.HSExtensionCodes.Output.Y{0}.HSExtensionCodes_Main_{0}.xml", year)),
					TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.HSExtensionCodes.Output.Y{0}.HSExtensionCodes_Sub_{0}_0.xml", year)),
					TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.HSExtensionCodes.Output.Y{0}.HSExtensionCodes_Sub_{0}_1.xml", year)),
					TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.HSExtensionCodes.Output.Y{0}.HSExtensionCodes_Sub_{0}_2.xml", year)),
					TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.HSExtensionCodes.Output.Y{0}.HSExtensionCodes_Sub_{0}_3.xml", year)),
					TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.HSExtensionCodes.Output.Y{0}.HSExtensionCodes_Sub_{0}_4.xml", year)),
					TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.HSExtensionCodes.Output.Y{0}.HSExtensionCodes_Sub_{0}_5.xml", year)),
					TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.HSExtensionCodes.Output.Y{0}.HSExtensionCodes_Sub_{0}_6.xml", year)),
					TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.HSExtensionCodes.Output.Y{0}.HSExtensionCodes_Sub_{0}_7.xml", year)),
					TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.HSExtensionCodes.Output.Y{0}.HSExtensionCodes_Sub_{0}_8.xml", year)),
					TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.HSExtensionCodes.Output.Y{0}.HSExtensionCodes_Sub_{0}_9.xml", year))
			};

			var outputMainFileCommonPath = Path.Combine(outputDirPath, "HSExtensionCodes_Main.xml");
			var outputSubFileCommonPath = Path.Combine(outputDirPath, "HSExtensionCodes_Sub.xml");
			var inputDataPath = Path.Combine(TestHelper.BaseTestFilePath, string.Format(CultureInfo.CurrentCulture, @"HSExtensionCodes\Input\{0}\HSExtensionCodes_{0}DataFileSample.{1}", year, fileExtension));
			new HSExtensionCodesMainCategoryParser(mainInputConfigPath, inputDataPath).ConvertToXMLFile(outputMainFileCommonPath, publicationDate);
			new HSExtensionCodesSubCategoryParser(subInputConfigPath, inputDataPath).ConvertToXMLFile(outputSubFileCommonPath, publicationDate);

			Assert.That(File.ReadAllText(outputDirPath + string.Format(CultureInfo.CurrentCulture, @"\HSExtensionCodes_Main_{0}.xml", year)), Is.EqualTo(expectedOutputFiles[0]));
			for (var i = 0; i < 10; i++)
			{
				Assert.That(File.ReadAllText(outputDirPath + string.Format(CultureInfo.CurrentCulture, @"\HSExtensionCodes_Sub_{0}_{1}.xml", year, i)), Is.EqualTo(expectedOutputFiles[i + 1]));
			}
		}
	}
}
