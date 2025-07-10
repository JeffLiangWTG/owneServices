using System;
using System.IO;
using System.Reflection;
using System.Text;
using CargoWise.RefDbRepo.BEReferenceData.CmdLine;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	[TestFixture]
	sealed class LocationCodesTest
	{
		Assembly assembly;
		string outputPath;
		string testFilesInputPath;
		string testFilesOutputPath;

		[Test]
		public void TestGenerateSeptemberXML()
		{
			var fileName = "RefCusCodeListZZ_BE_FAC.xml";
			// fix some string issue of '&#xD;\r\n' changing into '&#xD;\n' when pushed to GIT, since this is the hardcoded testfile, it does not matter for the unit test
			var expectedContent = File.ReadAllText(Path.Combine(testFilesOutputPath, fileName));

			var publicationTime = new DateTime(2020, 9, 1);
			var outputFile = Path.Combine(outputPath, fileName);

			var locationCodes = ExcelParser.ReadLocationCodesXlsIntoResults(Path.Combine(testFilesInputPath, "Locatiecodes_200901.xlsx"), null);
			XMLGeneration.ExportToXMLFile("BE Customs Locations", outputFile, XMLGeneration.GetRefLocCodesWriterConfiguration(), publicationTime, locationCodes);

			var resultContent = File.ReadAllText(outputFile);

			BEReferenceDataTestHelper.AssertEqualXML(resultContent, expectedContent);
		}

		[Test]
		public void TestInvalidDirectory()
		{
			var errorCollector = new StringBuilder();

			LocationCodesProgram.DownloadFilePath("..\\..\\InvalidDirectory", "InvalidFilename", errorCollector);

			Assert.That(errorCollector.ToString().Replace("\r\n", ""), Is.EqualTo($"The directory '..\\..\\InvalidDirectory' does not exist."));
		}

		[Test]
		public void TestInvalidFilename()
		{
			var errorCollector = new StringBuilder();

			LocationCodesProgram.DownloadFilePath(".", "InvalidFilename", errorCollector);

			Assert.That(errorCollector.ToString().Replace("\r\n", ""), Is.EqualTo($"No files matching the pattern 'InvalidFilename'*.xls*' were found in the directory '.'."));
		}

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
			outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"LocationCodes\Output");
			testFilesInputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"LocationCodes\TestFiles\Input");
			testFilesOutputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"LocationCodes\TestFiles\Output");
		}
	}
}
