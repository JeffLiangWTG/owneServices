using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	class OGATest
	{
		[TestCase(01, 01, 2018)]
		public void TestOGA2018(int day, int month, int year)
		{
			OGA_OneDataFile(day, month, year, "xlsx");
		}

		[TestCase(01, 01, 2019)]
		public void TestOGA2019(int day, int month, int year)
		{
			OGA_OneDataFile(day, month, year, "xls");
		}

		[TestCase(01, 01, 2020)]
		public void TestOGA2020(int day, int month, int year)
		{
			OGA_OneDataFile(day, month, year, "xls");
		}

		[TestCase(01, 01, 2021)]
		public void TestOGA2021(int day, int month, int year)
		{
			OGA_OneDataFile(day, month, year, "xlsx");
		}

		[TestCase(01, 01, 2022)]
		public void TestOGA2022(int day, int month, int year)
		{
			OGA_TwoDataFile(day, month, year, "xls", "xls");
		}

		[TestCase(01, 01, 2023)]
		public void TestOGA2023(int day, int month, int year)
		{
			OGA_TwoDataFile(day, month, year, "xls", "xls");
		}

		[TestCase(01, 01, 2024)]
		public void TestOGA2024(int day, int month, int year)
		{
			OGA_TwoDataFile(day, month, year, "xls", "xlsx");
		}

		[TestCase(19, 05, 2024)]
		public void TestOGA2024_Import_1(int day, int month, int year)
		{
			OGA_TwoDataFile(day, month, year, "", "xlsx", "_1");
		}

		[TestCase(22, 05, 2024)]
		public void TestOGA2024_Import_2(int day, int month, int year)
		{
			OGA_TwoDataFile(day, month, year, "", "xlsx", "_2");
		}

		[TestCase(30, 07, 2024)]
		public void TestOGA2024_Import_3(int day, int month, int year)
		{
			OGA_TwoDataFile(day, month, year, "", "xlsx", "_3");
		}

		[TestCase(30, 07, 2024)]
		public void TestOGA2024_Export_1(int day, int month, int year)
		{
			OGA_TwoDataFile(day, month, year, "xlsx", "", "_1");
		}

		[TestCase(01, 01, 2025)]
		public void TestOGA2025(int day, int month, int year)
		{
			OGA_TwoDataFile(day, month, year, "xls", "xls");
		}

		[TestCase(14, 06, 2025)]
		public void TestOGA2025_Import_1(int day, int month, int year)
		{
			OGA_TwoDataFile(day, month, year, "", "xlsx", "_1");
		}

		void OGA_TwoDataFile(int day, int month, int year, string exportFileType, string importFileType, string addFileName = "")
		{
			var publicationDate = new DateTime(year, month, day);

			if (!string.IsNullOrEmpty(exportFileType))
			{
				var inputConfigPathExport = string.Format(CultureInfo.CurrentCulture, @"Res\OGA\{0}\OGA_{0}Configuration_Export.xml", year);
				var inputDataPathExport = Path.Combine(TestHelper.BaseTestFilePath, string.Format(CultureInfo.CurrentCulture, @"OGA\Input\{0}\OGA_{0}DataFile_Sample_Export{1}.{2}", year, addFileName, exportFileType));
				var outputFileExport = Path.Combine(TestHelper.BaseTestFilePath, string.Format(CultureInfo.CurrentCulture, @"OGA\Output\Y{0}\OGA_{0}_Export{1}.xml", year, addFileName));
				var expectedXMLExport = TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.OGA.Output.Y{0}.OGA_{0}_Export{1}.xml", year, addFileName));
				new OGAParser(inputConfigPathExport, inputDataPathExport).ConvertToXMLFile(outputFileExport, publicationDate, Constants.Suffix.Export);
				Assert.That(File.ReadAllText(outputFileExport), Is.EqualTo(expectedXMLExport));
			}
			if (!string.IsNullOrEmpty(importFileType))
			{
				var inputConfigPathImport = string.Format(CultureInfo.CurrentCulture, @"Res\OGA\{0}\OGA_{0}Configuration_Import.xml", year);
				var inputDataPathImport = Path.Combine(TestHelper.BaseTestFilePath, string.Format(CultureInfo.CurrentCulture, @"OGA\Input\{0}\OGA_{0}DataFile_Sample_Import{1}.{2}", year, addFileName, importFileType));
				var outputFileImport = Path.Combine(TestHelper.BaseTestFilePath, string.Format(CultureInfo.CurrentCulture, @"OGA\Output\Y{0}\OGA_{0}_Import{1}.xml", year, addFileName));
				var expectedXMLImport = TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.OGA.Output.Y{0}.OGA_{0}_Import{1}.xml", year, addFileName));
				new OGAParser(inputConfigPathImport, inputDataPathImport).ConvertToXMLFile(outputFileImport, publicationDate, Constants.Suffix.Import);
				Assert.That(File.ReadAllText(outputFileImport), Is.EqualTo(expectedXMLImport));
			}
		}

		void OGA_OneDataFile(int day, int month, int year, string fileType)
		{
			var publicationDate = new DateTime(year, month, day);

			var inputDataPath = Path.Combine(TestHelper.BaseTestFilePath, string.Format(CultureInfo.CurrentCulture, @"OGA\Input\{0}\OGA_{0}DataFile_Sample.{1}", year, fileType));

			var inputConfigPathExport = string.Format(CultureInfo.CurrentCulture, @"Res\OGA\{0}\OGA_{0}Configuration_Export.xml", year);
			var outputFileExport = Path.Combine(TestHelper.BaseTestFilePath, string.Format(CultureInfo.CurrentCulture, @"OGA\Output\Y{0}\OGA_{0}_Export.xml", year));
			var expectedXMLExport = TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.OGA.Output.Y{0}.OGA_{0}_Export.xml", year));
			new OGAParser(inputConfigPathExport, inputDataPath).ConvertToXMLFile(outputFileExport, publicationDate, Constants.Suffix.Export);
			Assert.That(File.ReadAllText(outputFileExport), Is.EqualTo(expectedXMLExport));

			var inputConfigPathImport = string.Format(CultureInfo.CurrentCulture, @"Res\OGA\{0}\OGA_{0}Configuration_Import.xml", year);
			var outputFileImport = Path.Combine(TestHelper.BaseTestFilePath, string.Format(CultureInfo.CurrentCulture, @"OGA\Output\Y{0}\OGA_{0}_Import.xml", year));
			var expectedXMLImport = TestHelper.ReadManifestResourceContent(string.Format(CultureInfo.CurrentCulture, "CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.OGA.Output.Y{0}.OGA_{0}_Import.xml", year));
			new OGAParser(inputConfigPathImport, inputDataPath).ConvertToXMLFile(outputFileImport, publicationDate, Constants.Suffix.Import);
			Assert.That(File.ReadAllText(outputFileImport), Is.EqualTo(expectedXMLImport));
		}
	}
}
