using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NLReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NLReferenceData.Business.Testing
{
	[TestFixture]
	sealed class ECCNCodeListBuilderTest
	{
		[Test]
		public void ReadXlsFileIntoResults()
		{
			TestHelper.SimulateDownload(TempFolder + "ECCNCodes.xlsx", "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.ECCNCodes.Input.ECCNCodes.xlsx");
			var files = new List<string> { TempFolder + "ECCNCodes.xlsx" };
			var parser = new ECCNCodeListExcelParser();
			var resultList = parser.ReadXlsFile(files);
			Assert.IsNotNull(resultList, "Excel could not be read, no results are returned");
			Assert.That(resultList.Count.Equals(7), "There should be 7 results from the excel");
			Assert.AreEqual(resultList[0].Code, "0A001a", "First element should have code '0A001a'");
			Assert.AreEqual(resultList[0].TariffCode, "8401100000", "First element should have tariff code '8401100000'");
		}

		[Test]
		public void ConvertTableElementsToRefData()
		{
			var errorCollector = new StringBuilder();
			var builder = new ECCNCodeListBuilderForTest(errorCollector);

			var resultList = builder.ConvertTableElementsToRefDataExposed(ReferenceData);
			Assert.IsNotNull(resultList, "ECCNCodeList could not be converted, no results are returned");
			Assert.That(resultList.Count().Equals(3), "Not all ECCNCodeList could be converted to RefCusCodeList");
			Assert.AreEqual("0A001a", resultList.First().ZZD_Code, "Code of first element should be '0A001a'");
			Assert.AreEqual("0A001a", resultList.First().ZZD_Description, "Description of first element should be '0A001a'");
		}

		[Test]
		public void GenerateUniversalReferenceDataXml()
		{
			var errorCollector = new StringBuilder();
			var builder = new ECCNCodeListBuilder(errorCollector);
			string expectedXml, generatedXml = string.Empty;

			builder.GenerateUniversalReferenceDataXml(ReferenceData, new DateTime(2025, 04, 22, 09, 44, 19), TempFolder);

			var expectedFileName = Path.Combine(TempFolder, "RefCusCodeList_ECCN Codes_094419000.xml");
			if (File.Exists(expectedFileName))
			{
				generatedXml = File.ReadAllText(expectedFileName);
			}

			expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.ECCNCodes.Output.ECCNCodes.xml");

			Assert.IsTrue(File.Exists(expectedFileName), $"File with name '{expectedFileName} could not be found.");
			Assert.AreEqual(expectedXml, generatedXml, "Generated Universal Reference Data Xml does not match the correct format");
		}

		[Test]
		public void InvalidData()
		{
			var errorCollector = new StringBuilder();
			var builder = new ECCNCodeListBuilderForTest(errorCollector);
			var refDataCollection = builder.ConvertTableElementsToRefDataExposed(InvalidReferenceData);

			Assert.That(errorCollector.ToString().Contains("ECCN Code is required."), "Invalid data (missing code) is not detected for first element.");
			Assert.That(errorCollector.ToString().Contains("Tariff Code is required"), "Invalid data (missing tariff code) is not detected for second element.");
			Assert.AreEqual(1, refDataCollection.Count(), "Resultset contains invalid data, only 1 item should be in resultset");
		}

		[Test]
		public void DuplicateData()
		{
			var duplicateData = new List<ECCNCodeListData>();
			duplicateData.AddRange(InvalidReferenceData);
			duplicateData.Add(ReferenceData[0]);

			var errorCollector = new StringBuilder();
			var builder = new ECCNCodeListBuilderForTest(errorCollector);
			var refDataCollection = builder.ConvertTableElementsToRefDataExposed(InvalidReferenceData);

			Assert.That(errorCollector.ToString().Contains("Duplicate ECCN code/Tariff Code relation exists: '0A001a 8401100000'"));
			Assert.AreEqual(1, refDataCollection.Count(), "Resultset contains duplicate data, only 1 (unique) items should be in resultset");
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			ReferenceData = new List<ECCNCodeListData>()
			{
				new ECCNCodeListData() {
					Code = "0A001a",
					TariffCode = "8401100000"
				},
				new ECCNCodeListData() {
					Code = "0A001b",
					TariffCode = "8401100000"
				},
				new ECCNCodeListData() {
					Code = "0A001c",
					TariffCode = "8426110000"
				},
				new ECCNCodeListData() {
					Code = "0A001c",
					TariffCode = "8426190000"
				},
				new ECCNCodeListData() {
					Code = "0A001c",
					TariffCode = "8426990000"
				}
			};

			InvalidReferenceData = new List<ECCNCodeListData>()
			{
				new ECCNCodeListData() {
					Code = "",
					TariffCode = "8401100000"
				},
				new ECCNCodeListData() {
					Code = "0A001a",
					TariffCode = ""
				},
				new ECCNCodeListData() {
					Code = "0A001a",
					TariffCode = "8401100000"
				},
				new ECCNCodeListData() {
					Code = "0A001a",
					TariffCode = "8401100000"
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
		List<ECCNCodeListData> ReferenceData, InvalidReferenceData;

		class ECCNCodeListBuilderForTest : ECCNCodeListBuilder
		{
			public ECCNCodeListBuilderForTest(StringBuilder errorCollector) : base(errorCollector)
			{
			}

			public IEnumerable<RefCusCodeList> ConvertTableElementsToRefDataExposed(IEnumerable<ECCNCodeListData> data) => ConvertToRefCusCodeList(data);
		}
	}
}
