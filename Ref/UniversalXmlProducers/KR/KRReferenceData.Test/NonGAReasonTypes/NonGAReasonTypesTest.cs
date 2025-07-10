using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	class NonGAReasonTypesTest
	{
		[TestCase(1, 12, 2022)]
		public void ExportNonGAReasonTypes(int day, int month, int year)
		{
			NonGAReasonTypes(day, month, year, "Export", true);
		}

		[TestCase(1, 12, 2022)]
		public void ImportNonGAReasonTypes2022(int day, int month, int year)
		{
			NonGAReasonTypes(day, month, year, "Import", true);
		}

		[TestCase(22, 11, 2024)]
		public void ImportNonGAReasonTypes2024(int day, int month, int year)
		{
			NonGAReasonTypes(day, month, year, "Import", false);
		}

		public void NonGAReasonTypes(int day, int month, int year, string classification, bool isXls)
		{
			var fileExtension = isXls ? "xls" : "xlsx";
			string inputConfigPath = ApplicationConfig.NonGAReasonExportConfigFileInputPath;
			string dataSource = Constants.DataSources.ExportNonGAReasonType;
			var publicationDate = new DateTime(year, month, day);
			if (classification == "Import")
			{
				inputConfigPath = ApplicationConfig.NonGAReasonImportConfigFileInputPath;
				dataSource = Constants.DataSources.ImportNonGAReasonType;
			}
			var outputFile = Path.Combine(TestHelper.BaseTestFilePath, string.Format(CultureInfo.CurrentCulture, @"NonGAReasonTypes\Output\KRNonGAReasonType_{0}.xml", classification));
			var expectedXML = TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.NonGAReasonTypes.Output.KRNonGAReasonType_{0}{1}.xml", classification, year));
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}

			var inputDataPath = Path.Combine(TestHelper.BaseRealFilePath, string.Format(CultureInfo.CurrentCulture, @"NonGAReasonTypes\{0}\NonGAReasonType_{0}.{1}", year, fileExtension));
			new NonGAReasonParser(inputConfigPath, inputDataPath, dataSource).ConvertToXMLFile(outputFile, publicationDate);
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXML));
		}
	}
}
