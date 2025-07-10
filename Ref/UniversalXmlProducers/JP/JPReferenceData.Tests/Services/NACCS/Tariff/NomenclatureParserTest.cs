using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.JPReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.JPReferenceData.Tests
{
	sealed class NomenclatureParserTest
	{
		readonly string inputFolderPath = @"TestFiles";
		readonly string expectedOutputFilePathImport = @"TestFiles\Expected_JP_ImportNomenclature.xml";
		readonly string actualOutputFilePathImport = "JPImportNomenclature.xml";
		readonly string expectedOutputFilePathExport = @"TestFiles\Expected_JP_ExportNomenclature.xml";
		readonly string actualOutputFilePathExport = "JPExportNomenclature.xml";

		[Test]
		public void TestParseImportNomenclatureData()
		{
			var dirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var expectedFilePath = Path.Combine(dirPath, expectedOutputFilePathImport);
			var actualFilePath = Path.Combine(AppConfig.Shared.OutputDirectory, actualOutputFilePathImport);
			var expectedXmlAsString = File.ReadAllText(expectedFilePath);

			try
			{
				var parser = new NomenclatureParser(new DateTime(2024, 1, 1), true);
				parser.Parse(inputFolderPath);

				var actualXmlAsString = File.ReadAllText(actualFilePath);
				Assert.That(actualXmlAsString, Is.EqualTo(expectedXmlAsString));
			}
			finally
			{
				File.Delete(actualFilePath);
			}
		}

		[Test]
		public void TestParseExportNomenclatureData()
		{
			var dirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var expectedFilePath = Path.Combine(dirPath, expectedOutputFilePathExport);
			var actualFilePath = Path.Combine(AppConfig.Shared.OutputDirectory, actualOutputFilePathExport);
			var expectedXmlAsString = File.ReadAllText(expectedFilePath);

			try
			{
				var parser = new NomenclatureParser(new DateTime(2024, 1, 1), false);
				parser.Parse(inputFolderPath);

				var actualXmlAsString = File.ReadAllText(actualFilePath);
				Assert.That(actualXmlAsString, Is.EqualTo(expectedXmlAsString));
			}
			finally
			{
				File.Delete(actualFilePath);
			}
		}
	}
}
