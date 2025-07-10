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
	sealed class AdditionalSupplementsBuilderTest
	{
		[Test]
		public void ConvertXmlFileToRefCusCodeList()
		{
			var errorCollector = new StringBuilder();
			var builder = new AdditionalSupplementsBuilderForTest(errorCollector);

			var resultList = builder.ConvertTableElementsToRefDataExposed(ReferenceData);

			Assert.IsNotNull(resultList, "AdditionalSupplementsData could not be converted, no results are returned");
			Assert.That(resultList.Count == 3, "Not all AdditionalSupplementsData could be converted to ResCusCodeList");
			Assert.AreEqual("ADDSU", resultList[0].ZZD_ZZK_NKCodeType, "RefCusCodeList is generated with wrong Code Type ");
			Assert.AreEqual("NL", resultList[0].ZZD_ZZZ_NKDataGrouping, "RefCusCodeList should have 'NL' as Data Grouping value");
			Assert.AreEqual(new DateTime(1900, 1, 1, 0, 0, 0), resultList[0].ZZD_StartDate, "Start date should be equal to '1900-01-01T00:00:00'");
			Assert.AreEqual(new DateTime(2079, 6, 6, 23, 59, 00), resultList[0].ZZD_EndDate, "End date should be equal to '2079-06-06T23:59:00'");
			Assert.AreEqual("Q001", resultList[0].ZZD_Code, "Code of first element should be 'Q001'");
			Assert.AreEqual("Description U", resultList[1].ZZD_Description, "Description of second element does not match");
		}

		[Test]
		public void DuplicateDataInXml()
		{
			var duplicateData = new List<AdditionalSupplementsData>();
			duplicateData.AddRange(ReferenceData);
			duplicateData.Add(ReferenceData[0]);

			var errorCollector = new StringBuilder();
			var builder = new AdditionalSupplementsBuilderForTest(errorCollector);
			var refDataCollection = builder.ConvertTableElementsToRefDataExposed(duplicateData);

			Assert.That(errorCollector.ToString().StartsWith("RefCusCodeList duplicate exists. Key: 'Q001_ADDSU' Description: Description Q", StringComparison.InvariantCulture), "Duplicate data (Q001_ADDSU) is not detected");
			Assert.AreEqual(3, refDataCollection.Count, "Resultset contains duplicate data, only 3 (unique) items should be in resultset");
		}

		[Test]
		public void InvalidDataInXml()
		{
			var errorCollector = new StringBuilder();
			var builder = new AdditionalSupplementsBuilderForTest(errorCollector);
			var refDataCollection = builder.ConvertTableElementsToRefDataExposed(InvalidData);
			Console.WriteLine(errorCollector.ToString());
			Assert.That(errorCollector.ToString().Contains("RefCusCodeList validation error. Key '_ADDSU' Errors: ZZD_Code is required. '"), "Invalid data (missing code) is not detected.");
			Assert.That(errorCollector.ToString().Contains("RefCusCodeList validation error. Key 'V003_ADDSU' Errors: ZZD_Description is required. '"), "Invalid data (missing description) is not detected.");
			Assert.AreEqual(1, refDataCollection.Count, "Resultset contains invalid data, only 1 item should be in resultset");
		}

		[Test]
		public void GenerateUniversalReferenceDataXml()
		{
			var errorCollector = new StringBuilder();
			var builder = new AdditionalSupplementsBuilder(errorCollector);
			string expectedXml, generatedXml = string.Empty;

			builder.BuildXml(new DateTime(2021, 04, 26, 10, 43, 38), ReferenceData, TempFolder);

			var expectedFileName = Path.Combine(TempFolder, "NL CodeList - Additional Supplements_104338000.xml");
			if (File.Exists(expectedFileName))
			{
				generatedXml = File.ReadAllText(expectedFileName);
			}

			expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.AdditionalSupplements.Output.RefCusCodeList_AdditionalSupplements.xml");

			Assert.IsTrue(File.Exists(expectedFileName), $"File with name '{expectedFileName} could not be found.");
			Assert.AreEqual(expectedXml, generatedXml, "Generated Universal Reference Data Xml does not match the correct format");
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			ReferenceData = new List<AdditionalSupplementsData>()
			{
				new AdditionalSupplementsData() {
					Code = "Q001",
					Description = "Description Q"
				},
				new AdditionalSupplementsData()
				{
					Code = "U002",
					Description = "Description U"
				},
				new AdditionalSupplementsData()
				{
					Code = "V003",
					Description = "Description V"
				}
			};

			InvalidData = new List<AdditionalSupplementsData>()
			{
				new AdditionalSupplementsData() {
					Code = "Q001",
					Description = "Description Q"
				},
				new AdditionalSupplementsData()
				{
					Code = "",
					Description = "Description U"
				},
				new AdditionalSupplementsData()
				{
					Code = "V003",
					Description = ""
				}
			};
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
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
		List<AdditionalSupplementsData> ReferenceData, InvalidData;
	}

	public class AdditionalSupplementsBuilderForTest : AdditionalSupplementsBuilder
	{
		public AdditionalSupplementsBuilderForTest(StringBuilder errorCollector) : base(errorCollector)
		{
		}

		public Collection<RefCusCodeList> ConvertTableElementsToRefDataExposed(IEnumerable<AdditionalSupplementsData> data) => base.ConvertTableElementsToRefData(data);
	}
}
