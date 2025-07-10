using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NLReferenceData.Services;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NLReferenceData.Business.Testing
{
	[TestFixture]
	sealed class AdditionalReferenceBuilderTest
	{
		[Test]
		public void ConvertXmlFileToRefCusCodeList()
		{
			var errorCollector = new StringBuilder();
			var dateTimeProvider = new Mock<IDateTimeProvider>();
			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2024, 06, 14, 15, 25, 06));

			var builder = new AdditionalReferenceBuilderForTest(errorCollector, dateTimeProvider.Object);

			var resultList = builder.ConvertTableElementsToRefDataExposed(ReferenceData);

			Assert.IsNotNull(resultList, "TableElements could not be converted, no results are returned");
			Assert.That(resultList.Count == 6, "Not all TableElements could be converted to ResCusCodeList");
			Assert.AreEqual("AR44I", resultList[0].ZZD_ZZK_NKCodeType, "RefCusCodeList is generated with wrong Code Type ");
			Assert.AreEqual("AR44E", resultList[1].ZZD_ZZK_NKCodeType, "RefCusCodeList is generated with wrong Code Type ");
			Assert.AreEqual("NL", resultList[0].ZZD_ZZZ_NKDataGrouping, "RefCusCodeList should have 'NL' as Data Grouping value");
			Assert.AreEqual(new DateTime(1900, 01, 01, 00, 00, 00), resultList[0].ZZD_StartDate, "Start date should be equal to '1900-01-01 00:00:00'");
			Assert.AreEqual(new DateTime(2079, 6, 6, 23, 59, 00), resultList[0].ZZD_EndDate, "End date should be equal to '2079-06-06 23:59:00'");
			Assert.AreEqual("001", resultList[0].ZZD_Code, "Code of first element should be '001'");
			Assert.AreEqual("Description 1 - Legal description 1", resultList[0].ZZD_Description, "Description of first element (with legal description) does not match");
			Assert.AreEqual("Description 2", resultList[2].ZZD_Description, "Description of second element (without legal description) does not match");
			Assert.AreEqual(3, resultList[2].RefCusCodeListAttributes.Length, "No attributes are present on third element");
			Assert.AreEqual("Level", resultList[2].RefCusCodeListAttributes[0].ZZE_ZXE_NKName, "First attribute should have the name 'Level'");
			Assert.AreEqual("Header", resultList[2].RefCusCodeListAttributes[0].ZZE_Value, "First attribute should have the value 'Header'");
			Assert.AreEqual("Level", resultList[2].RefCusCodeListAttributes[1].ZZE_ZXE_NKName, "Second attribute should have the name 'Level'");
			Assert.AreEqual("Item", resultList[2].RefCusCodeListAttributes[1].ZZE_Value, "Second attribute should have the value 'Item'");
			Assert.AreEqual("Reference", resultList[2].RefCusCodeListAttributes[2].ZZE_ZXE_NKName, "Third attribute should have the name 'Reference'");
			Assert.AreEqual("Y", resultList[2].RefCusCodeListAttributes[2].ZZE_Value, "Third attribute should have the value 'Y'");
		}

		[Test]
		public void DuplicateDataInXml()
		{
			var duplicateData = new List<TableElement>();
			duplicateData.AddRange(ReferenceData);
			duplicateData.Add(ReferenceData[0]);

			var errorCollector = new StringBuilder();
			var builder = new AdditionalReferenceBuilderForTest(errorCollector);
			var refDataCollection = builder.ConvertTableElementsToRefDataExposed(duplicateData);

			Assert.That(errorCollector.ToString().StartsWith("RefCusCodeList duplicate exists. Key: '001_AR44I' Description: Description 1", StringComparison.InvariantCulture), "Duplicate data (001_AR44I) is not detected");
			Assert.AreEqual(6, refDataCollection.Count, "Resultset contains duplicate data, only 6 (unique) items should be in resultset");
		}

		[Test]
		public void InvalidDataInXml()
		{
			var errorCollector = new StringBuilder();
			var builder = new AdditionalReferenceBuilderForTest(errorCollector);
			var refDataCollection = builder.ConvertTableElementsToRefDataExposed(InvalidData);
			Console.WriteLine(errorCollector.ToString());
			Assert.That(errorCollector.ToString().Contains("RefCusCodeList validation error. Key '_AR44I' Errors: ZZD_Code is required. '"), "Invalid data (missing code) is not detected.");
			Assert.That(errorCollector.ToString().Contains("RefCusCodeList validation error. Key '003_AR44I' Errors: ZZD_Description is required. '"), "Invalid data (missing description) is not detected.");
			Assert.AreEqual(2, refDataCollection.Count, "Resultset contains invalid data, only 2 items should be in resultset");
		}

		[Test]
		public void GenerateUniversalReferenceDataXml()
		{
			var errorCollector = new StringBuilder();

			var dateTimeProvider = new Mock<IDateTimeProvider>();
			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2024, 06, 14, 15, 25, 06));

			var builder = new AdditionalReferenceBuilder(errorCollector, dateTimeProvider.Object);
			string expectedXml, generatedXml = string.Empty;

			builder.BuildXml(new DateTime(2024, 06, 14, 15, 25, 06), ReferenceData, TempFolder);

			var expectedFileName = Path.Combine(TempFolder, "NL CodeList - Additional Reference_152506000.xml");
			if (File.Exists(expectedFileName))
			{
				generatedXml = File.ReadAllText(expectedFileName);
			}

			expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.AdditionalReference.Output.RefCusCodeList_AdditionalReference.xml");
			Assert.IsTrue(File.Exists(expectedFileName), $"File with name '{expectedFileName} could not be found.");
			Assert.AreEqual(expectedXml, generatedXml, "Generated Universal Reference Data Xml does not match the correct format");
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			ReferenceData = new List<TableElement>()
			{
				new TableElement() {
					elementCode = "001",
					elementDescription = "Description 1",
					elementLegalDescription = "Legal description 1"
				},
				new TableElement()
				{
					elementCode = "002",
					elementDescription = "Description 2"
				},
				new TableElement()
				{
					elementCode = "003",
					elementDescription = "Description 3"
				}
			};

			InvalidData = new List<TableElement>()
			{
				new TableElement() {
					elementCode = "001",
					elementDescription = "Description 1",
					elementLegalDescription = "Legal description 1"
				},
				new TableElement()
				{
					elementCode = "",
					elementDescription = "Description 2"
				},
				new TableElement()
				{
					elementCode = "003",
					elementDescription = ""
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
		List<TableElement> ReferenceData, InvalidData;
	}

	public class AdditionalReferenceBuilderForTest : AdditionalReferenceBuilder
	{
		public AdditionalReferenceBuilderForTest(StringBuilder errorCollector, IDateTimeProvider dateTimeProvider = null) : base(errorCollector, dateTimeProvider)
		{
		}

		public Collection<RefCusCodeList> ConvertTableElementsToRefDataExposed(IEnumerable<TableElement> data) => base.ConvertTableElementsToRefData(data);
	}
}
