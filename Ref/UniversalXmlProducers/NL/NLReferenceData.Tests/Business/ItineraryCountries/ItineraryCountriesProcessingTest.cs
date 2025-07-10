using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.NLReferenceData.Services;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NLReferenceData.Business.Testing
{
	[TestFixture]
	sealed class ItineraryCountriesProcessingTest
	{
		[Test]
		public void ProcessCorrectExcel()
		{
			var errorCollector = new StringBuilder();
			var prManager = SetupProcessManager(errorCollector, InputPathCorrect, true);
			prManager.Object.RunProcess(TempFolder, errorCollector);

			var files = new List<string>(Directory.GetFiles(TempFolder));

			var generatedFile = files.First(x => Path.GetFileName(x).StartsWith("NL CodeList - " + EntityName, StringComparison.InvariantCulture));
			Assert.That(errorCollector.ToString(), Is.EqualTo(string.Empty), $"No XML file was generated. Error occurred: {errorCollector}");
			Assert.That(File.Exists(generatedFile), "No XML File was generated.");

			var generatedXml = File.ReadAllText(generatedFile);
			var expectedXml = TestHelper.ReadManifestResourceContent(OutputPathCorrect);

			Assert.That(generatedXml.Equals(expectedXml, StringComparison.Ordinal), "XML-file does not contain the correct information for the " + EntityName + ".");
		}

		[Test]
		public void InvalidData()
		{
			var errorCollector = new StringBuilder();
			var prManager = SetupProcessManager(errorCollector, OutputPathCorrect, false);

			var ex = Assert.Throws<ProcessingException>(() => prManager.Object.RunProcess(TempFolder, errorCollector));
			Assert.That(ex.Message, Is.EqualTo("Processing failed"));
			Assert.That(ex.InnerException.ToString().Split('\r')[0], Is.EqualTo($"CargoWise.RefDbRepo.NLReferenceData.Services.ProcessingException: Excel file could not be found for {EntityName}"));
		}

		[Test]
		public void NoData()
		{
			var errorCollector = new StringBuilder();
			var prManager = SetupProcessManager(errorCollector, InputPathNoData, true);

			var ex = Assert.Throws<ProcessingException>(() => prManager.Object.RunProcess(TempFolder, errorCollector));
			Assert.That(ex.Message, Is.EqualTo("Processing failed"));
			Assert.That(ex.InnerException.ToString().Split('\r')[0], Is.EqualTo($"CargoWise.RefDbRepo.NLReferenceData.Services.ProcessingException: Downloaded file(s) has no code list for {EntityName}"));
		}

		[SetUp]
		public void Setup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
		}

		[TearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		static Mock<ItineraryCountriesProcessManager> SetupProcessManager(StringBuilder errorCollector, string resourceContent, bool isSimulateDownload)
		{
			var builder = new ItineraryCountriesBuilder(errorCollector);
			var prManager = new Mock<ItineraryCountriesProcessManager>(builder);

			if (isSimulateDownload)
			{
				prManager.Protected().Setup<bool>("SaveResourceContentToFile", ItExpr.IsAny<string>(), ItExpr.IsAny<string>()).Returns((string filename, string resource) => TestHelper.SimulateDownload(filename, resource));
			}
			else
			{
				prManager.Protected().Setup<bool>("SaveResourceContentToFile", ItExpr.IsAny<string>(), ItExpr.IsAny<string>()).Callback((string x, string y) => { });
			}

			prManager.Protected().Setup<string>("EntityName").Returns(EntityName);
			prManager.Protected().Setup<string>("ExcelFileName").Returns(FileName);
			prManager.Protected().Setup<string>("ResourceContent").Returns(resourceContent);
			prManager.Protected().Setup<DateTime>("PublicationTime").Returns(new DateTime(2020, 8, 28, 20, 08, 40));

			return prManager;
		}

		static string InputPathCorrect => "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.ItineraryCountries.Input.ItineraryCountries_Correct.xlsx";
		static string OutputPathCorrect => "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.ItineraryCountries.Output.ItineraryCountries_Correct.xml";
		static string InputPathNoData => "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.ItineraryCountries.Input.NoItineraryCountries.xlsx";
		static string FileName => "ItineraryCountries.xlsx";
		static string EntityName => "Itinerary Countries";

		string TempFolder;
	}
}
