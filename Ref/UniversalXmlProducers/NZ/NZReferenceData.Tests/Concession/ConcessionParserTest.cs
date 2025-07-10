using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.NZReferenceData.Business;
using CargoWise.RefDbRepo.NZReferenceData.Tests.Concessions;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NZReferenceData.Tests
{
	sealed class ConcessionParserTest
	{
		[Test]
		public void TestParseTariffDetails()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var assemblyDirectoryPath = Path.GetDirectoryName(assembly.Location);
			var outputFolderPath = Path.Combine(assemblyDirectoryPath, "TestOutput");
			Directory.CreateDirectory(outputFolderPath);

			var processingData = new NZConcessionProcessingData();
			var parser = new ConcessionParser(new Logger());
			parser.Parse(ConcessionTestHelper.TestBuildersFilePathFromTestFiles, GetDateProvider(), processingData);
			Assert.That(processingData.LastRunDateConcession, Is.EqualTo(new DateTime(2024, 12, 26, 4, 0, 13)));

			parser.ExportToXMLFile(outputFolderPath);

			var tests = new[]
			{
				"RefCusTariffConcessions_NZ_01-10.xml",
				"RefCusTariffConcessions_NZ_11-20.xml",
				"RefCusTariffConcessions_NZ_21-30.xml",
				"RefCusTariffConcessions_NZ_31-40.xml",
				"RefCusTariffConcessions_NZ_41-50.xml",
				"RefCusTariffConcessions_NZ_51-60.xml",
				"RefCusTariffConcessions_NZ_61-70.xml",
				"RefCusTariffConcessions_NZ_71-80.xml",
				"RefCusTariffConcessions_NZ_81-90.xml",
				"RefCusTariffConcessions_NZ_91-100.xml",
			};

			foreach(var fileName in tests)
			{
				var expectedImportXML = ConcessionTestHelper.ReadExpectedOutput(fileName);
				expectedImportXML = expectedImportXML.Replace("\r\n", "\n");
				var outputFilePath = Path.Combine(outputFolderPath, fileName);
				var actualOutputXml = File.ReadAllText(outputFilePath);
				actualOutputXml = actualOutputXml.Replace("\r\n", "\n");

				Assert.AreEqual(expectedImportXML, actualOutputXml, fileName);

				File.Delete(outputFilePath);
			}
		}

		[Test]
		public void TestParse_NotNewPublication()
		{
			var processingData = new NZConcessionProcessingData()
			{
				LastRunDateConcession = new DateTime(2025, 1, 1)
			};
			var parser = new ConcessionParser(new Logger());
			var parseResult = parser.Parse(ConcessionTestHelper.TestBuildersFilePathFromTestFiles, GetDateProvider(), processingData);
			Assert.That(!parseResult);
			Assert.That(parser.DataRepo.Get(), Has.Count.EqualTo(0));
		}

		IDateProvider GetDateProvider()
		{
			return Mock.Of<IDateProvider>(x => x.Today == new DateTime(2025, 1, 1) && x.ActiveDate == x.Today.AddYears(-5));
		}
	}
}
