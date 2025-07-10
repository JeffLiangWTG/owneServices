using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NLReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NLReferenceData.Business.Testing
{
	[TestFixture]
	sealed class ItineraryCountriesBuilderTest
	{
		[Test]
		public void ReadXlsFileIntoResults()
		{
			TestHelper.SimulateDownload(TempFolder + "ItineraryCountries_Correct.xlsx", "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.ItineraryCountries.Input.ItineraryCountries_Correct.xlsx");
			var files = new List<string> { TempFolder + "ItineraryCountries_Correct.xlsx" };
			var parser = new ItineraryCountriesExcelParser();
			var resultList = parser.ReadXlsFile(files);
			Assert.IsNotNull(resultList, "Excel could not be read, no results are returned");
			Assert.That(resultList.Count.Equals(5), "There should be 5 results from the excel");
			Assert.AreEqual(resultList[0].Code, "AD", "First element should have code 'AD'");
			Assert.AreEqual(resultList[0].Description, "ANDORRA");
		}

		[Test]
		public void ConvertTableElementsToRefData()
		{
			var errorCollector = new StringBuilder();
			var builder = new ItineraryCountriesBuilderForTest(errorCollector);

			var resultList = builder.ConvertTableElementsToRefDataExposed(ReferenceData);
			Assert.IsNotNull(resultList, "ItineraryCountruesData could not be converted, no results are returned");
			Assert.That(resultList.Count.Equals(5), "Not all ItineraryCountriesData could be converted to RefCusCodeList");
			Assert.AreEqual("AD", resultList[0].ZZD_Code, "Code of first element should be 'AD'");
			Assert.AreEqual("ANDORRA", resultList[0].ZZD_Description, "Description of first element does not match");
		}

		[Test]
		public void GenerateUniversalReferenceDataXml()
		{
			var errorCollector = new StringBuilder();
			var builder = new ItineraryCountriesBuilder(errorCollector);
			string expectedXml, generatedXml = string.Empty;

			builder.BuildXml(new DateTime(2020, 08, 28, 20, 08, 40), ReferenceData, TempFolder);

			var expectedFileName = Path.Combine(TempFolder, "NL CodeList - Itinerary Countries_200840000.xml");
			if (File.Exists(expectedFileName))
			{
				generatedXml = File.ReadAllText(expectedFileName);
			}

			expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.ItineraryCountries.Output.RefCusCodeList_NLItineraryCountries_Correct.xml");

			Assert.IsTrue(File.Exists(expectedFileName), $"File with name '{expectedFileName} could not be found.");
			Assert.AreEqual(expectedXml, generatedXml, "Generated Universal Reference Data Xml does not match the correct format");
		}

		[Test]
		public void InvalidData()
		{
			var errorCollector = new StringBuilder();
			var builder = new ItineraryCountriesBuilderForTest(errorCollector);
			var refDataCollection = builder.ConvertTableElementsToRefDataExposed(InvalidReferenceData);

			Assert.That(errorCollector.ToString().Contains("RefCusCodeList validation error. Key '_' Errors: ZZD_Code is required."), "Invalid data (missing code) is not detected for first element.");
			Assert.That(errorCollector.ToString().Contains("RefCusCodeList validation error. Key 'AE_' Errors: ZZD_Description is required."), "Invalid data (missing description) is not detected for second element.");
			Assert.AreEqual(1, refDataCollection.Count, "Resultset contains invalid data, only 1 item should be in resultset");
		}

		[Test]
		public void DuplicateData()
		{
			var duplicateData = new List<ItineraryCountriesData>();
			duplicateData.AddRange(InvalidReferenceData);
			duplicateData.Add(ReferenceData[0]);

			var errorCollector = new StringBuilder();
			var builder = new ItineraryCountriesBuilderForTest(errorCollector);
			var refDataCollection = builder.ConvertTableElementsToRefDataExposed(InvalidReferenceData);

			Assert.That(errorCollector.ToString().Contains("RefCusCodeList duplicate exists. Key: 'AF_' Description: AFGHANISTAN"), "Duplicate data (AF) is not detected");
			Assert.AreEqual(1, refDataCollection.Count, "Resultset contains duplicate data, only 1 (unique) items should be in resultset");
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			ReferenceData = new List<ItineraryCountriesData>()
			{
				new ItineraryCountriesData() {
					Code = "AD",
					Description = "ANDORRA"
				},
				new ItineraryCountriesData() {
					Code = "AE",
					Description = "VER. ARAB. EMIRATEN"
				},
				new ItineraryCountriesData() {
					Code = "AF",
					Description = "AFGHANISTAN"
				},
				new ItineraryCountriesData() {
					Code = "AG",
					Description = "ANTIGUA EN BARBUDA"
				},
				new ItineraryCountriesData() {
					Code = "AI",
					Description = "ANGUILLA"
				}
			};

			InvalidReferenceData = new List<ItineraryCountriesData>()
			{
				new ItineraryCountriesData() {
					Code = "",
					Description = "Andorra"
				},
				new ItineraryCountriesData() {
					Code = "AE",
					Description = ""
				},
				new ItineraryCountriesData() {
					Code = "AF",
					Description = "AFGHANISTAN"
				},
				new ItineraryCountriesData() {
					Code = "AF",
					Description = "AFGHANISTAN"
				}
			};

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

		string TempFolder;
		List<ItineraryCountriesData> ReferenceData, InvalidReferenceData;

		class ItineraryCountriesBuilderForTest : ItineraryCountriesBuilder
		{
			public ItineraryCountriesBuilderForTest(StringBuilder errorCollector) : base(errorCollector)
			{
			}

			public Collection<RefCusCodeList> ConvertTableElementsToRefDataExposed(IEnumerable<ItineraryCountriesData> data) => ConvertTableElementsToRefData(data);
		}
	}
}
