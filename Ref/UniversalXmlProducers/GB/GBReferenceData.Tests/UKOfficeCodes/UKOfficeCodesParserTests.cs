using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.GBReferenceData.Business.UKOfficeCodes;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using CargoWise.RefDbRepo.GBReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.UKOfficeCodes.Tests
{
	[TestFixture]
	sealed class UKOfficeCodesParserTests
	{
		[Test]
		public void Constructor()
		{
			Assert.Throws(Is.TypeOf<ArgumentNullException>().And.Message.StartsWith("Value cannot be null"), () => new UKOfficeCodesParser(null, new byte[] { 1, 2, 3, 4 }));
			Assert.Throws(Is.TypeOf<ArgumentNullException>().And.Message.StartsWith("Value cannot be null"), () => new UKOfficeCodesParser(new StringBuilder(), null));
		}

		[Test]
		public void Parse()
		{
			parser.Parse();
			Assert.That(File.Exists(parser.OutputFileForTest));
			Assert.That(errorCollector.ToString(), Is.Empty);
		}

		[Test]
		public void ParseFail()
		{
			var odsData = TestHelper.ReadManifestResourceContentBytes("CargoWise.RefDbRepo.GBReferenceData.Tests.UKOfficeCodes.TestFiles.CDSUKCustomsOfficeCodesBad.ods");
			parser = new UKOfficeCodesParserForTest(errorCollector, odsData);
			parser.Parse();
			Assert.That(!File.Exists(parser.OutputFileForTest));

			var errorMessage = errorCollector.ToString();
			var expectedErrorMessage = new StringBuilder().AppendLine("Unable to import any UK Customs Office records. No valid data found!").ToString();
			Assert.That(errorMessage, Is.Not.Empty);
			Assert.That(errorMessage, Is.EqualTo(expectedErrorMessage));
		}

		[Test]
		public void GetUKOfficeCodes()
		{
			var ukOfficeCodeList = parser.GetUKOfficeCodesForTest();
			Assert.That(ukOfficeCodeList.Count, Is.EqualTo(129));
			Assert.That(errorCollector.ToString(), Is.Empty);

			var odsData = TestHelper.ReadManifestResourceContentBytes("CargoWise.RefDbRepo.GBReferenceData.Tests.UKOfficeCodes.TestFiles.CDSUKCustomsOfficeCodesInvalid.ods");
			parser = new UKOfficeCodesParserForTest(errorCollector, odsData);
			var expectedErrorMessage = new StringBuilder()
				.AppendLine("Unable to import the following UK Customs Office records:")
				.AppendLine("- Code: GB000014 | Description:  | Usual Name:  | City:  | Region: NORTHERN IRELAND").ToString();

			ukOfficeCodeList = parser.GetUKOfficeCodesForTest();
			Assert.That(ukOfficeCodeList.Count, Is.EqualTo(2));
			var errorMessage = errorCollector.ToString();
			Assert.That(errorMessage, Is.Not.Empty);
			Assert.That(errorMessage, Is.EqualTo(expectedErrorMessage));
		}

		[Test]
		public void ConvertToModel()
		{
			using (var dataTable = new DataTable())
			{
				var columns = new string[] { "Region", "City", "Usual name", "COL code" };
				dataTable.Columns.AddRange(columns.Select(x => new DataColumn(x)).ToArray());
				dataTable.Rows.Add("NORTH", "—", "Central Community Transit Office", "GB000001");
				dataTable.Rows.Add("SOUTH", "Harwich", "Avonmouth 1", "GB000033");
				dataTable.Rows.Add("NORTH", "Hull", "Hull", string.Empty);
				dataTable.Rows.Add("NORTH", string.Empty, string.Empty, "GB000072");

				var model1 = parser.ConvertToModelForTest(dataTable.Rows[0]);
				AssertModel(model1, "GB000001", "Central Community Transit Office", string.Empty, "NORTH", "Central Community Transit Office");

				var model2 = parser.ConvertToModelForTest(dataTable.Rows[1]);
				AssertModel(model2, "GB000033", "Avonmouth 1", "Harwich", "SOUTH", "Avonmouth 1 Harwich");

				var model3 = parser.ConvertToModelForTest(dataTable.Rows[2]);
				AssertModel(model3, string.Empty, "Hull", "Hull", "NORTH", "Hull Hull", false);

				var model4 = parser.ConvertToModelForTest(dataTable.Rows[3]);
				AssertModel(model4, "GB000072", string.Empty, string.Empty, "NORTH", string.Empty, false);
			}
		}

		void AssertModel(UKOfficeCode officeCode, string code, string usualName, string city, string region, string description, bool isValid = true)
		{
			Assert.That(officeCode.Code, Is.EqualTo(code));
			Assert.That(officeCode.UsualName, Is.EqualTo(usualName));
			Assert.That(officeCode.City, Is.EqualTo(city));
			Assert.That(officeCode.Region, Is.EqualTo(region));
			Assert.That(officeCode.Description, Is.EqualTo(description));
			Assert.That(officeCode.IsValid, Is.EqualTo(isValid));

			Assert.That(officeCode.Roles, Is.Not.Null);
			Assert.That(officeCode.Roles.Count, Is.EqualTo(2));
			var roles = officeCode.Roles.ToArray();
			Assert.That(roles[0].Value, Is.EqualTo("EXT"));
			Assert.That(roles[1].Value, Is.EqualTo("EXP"));
		}

		[Test]
		public void ConvertModelToRefModel()
		{
			var codes = new[]
			{
				new UKOfficeCode { Code = "Code1", UsualName = "UsualName1", City = "City1", Region = "Region1", Roles = new UKOfficeCodeRole[] { new UKOfficeCodeRole("EXT"), new UKOfficeCodeRole("EXP") } },
				new UKOfficeCode { Code = "Code2", UsualName = "UsualName2", City = string.Empty, Region = "Region2", Roles = new UKOfficeCodeRole[] { new UKOfficeCodeRole("EXT"), new UKOfficeCodeRole("EXP") } },
				new UKOfficeCode { Code = "Code3", UsualName = string.Empty, City = string.Concat(Enumerable.Repeat("A", 2001)), Region = "Region3", Roles = new UKOfficeCodeRole[] { new UKOfficeCodeRole("EXT"), new UKOfficeCodeRole("EXP") } },
			};

			var refModels = parser.ConvertToRefModelsForTest(codes).ToList();
			Assert.That(refModels, Is.Not.Null);
			Assert.That(refModels.Count, Is.EqualTo(3));

			AssertRefModel(refModels[0], "Code1", "UsualName1 City1");
			AssertRefModel(refModels[1], "Code2", "UsualName2");
			AssertRefModel(refModels[2], "Code3", $"{string.Concat(Enumerable.Repeat("A", 1999))}…");
		}

		void AssertRefModel(RefCusCodeList refModel, string code, string description)
		{
			Assert.That(refModel.ZZD_Code, Is.EqualTo(code));
			Assert.That(refModel.ZZD_Description, Is.EqualTo(description));

			Assert.That(refModel.RefCusCodeListAttributes, Is.Not.Null);
			Assert.That(refModel.RefCusCodeListAttributes.Count, Is.EqualTo(2));
			AssertRefModelRoleAttributes(refModel.RefCusCodeListAttributes[0], "EXT");
			AssertRefModelRoleAttributes(refModel.RefCusCodeListAttributes[1], "EXP");
		}

		void AssertRefModelRoleAttributes(RefCusCodeListAttribute attribute, string value)
		{
			Assert.That(attribute.ZZE_ZXE_NKName, Is.EqualTo("ROLE"));
			Assert.That(attribute.ZZE_Value, Is.EqualTo(value));
		}

		[Test]
		public void HeaderTag()
		{
			Assert.That(parser.HeaderTagForTest, Is.EqualTo("Region"));
		}

		[Test]
		public void XMLWriterDataSource()
		{
			Assert.That(parser.XMLWriterDataSourceForTest, Is.EqualTo("UK Customs Office Codes"));
		}

		[Test]
		public void XmlWriterConfiguration()
		{
			var config = parser.XmlWriterConfigurationForTest();
			Assert.That(config, Is.Not.Null);
			var refType = typeof(RefCusCodeList);
			var codeListConfig = config.GetConfiguration(refType);

			Assert.That(codeListConfig, Is.Not.Null);
			Assert.That(codeListConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_Code))));
			Assert.That(codeListConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_ZZK_NKCodeType))));
			Assert.That(codeListConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_ZZZ_NKDataGrouping))));
			Assert.That(codeListConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_Description))));
			Assert.That(codeListConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_StartDate))));
			Assert.That(codeListConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.ZZD_EndDate))));
			Assert.That(codeListConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeList.RefCusCodeListAttributes))));
			AssertKeySets(nameof(RefCusCodeList), codeListConfig.GetKeySets(), nameof(RefCusCodeList.ZZD_Code), nameof(RefCusCodeList.ZZD_ZZK_NKCodeType), nameof(RefCusCodeList.ZZD_ZZZ_NKDataGrouping));

			refType = typeof(RefCusCodeListAttribute);
			var attributeConfig = config.GetConfiguration(refType);
			Assert.That(attributeConfig, Is.Not.Null);
			Assert.That(attributeConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeListAttribute.ZZE_ZXE_NKName))));
			Assert.That(attributeConfig.IsIncluded(refType.GetProperty(nameof(RefCusCodeListAttribute.ZZE_Value))));
			AssertKeySets(nameof(RefCusCodeListAttribute), attributeConfig.GetKeySets(), nameof(RefCusCodeListAttribute.ZZE_ZXE_NKName), nameof(RefCusCodeListAttribute.ZZE_Value));
		}

		void AssertKeySets(string entity, IEnumerable<KeySet> keySets, params string[] properties)
		{
			var keys = keySets.Select(x => x.PropertySchema.Name).OrderBy(x => x).ToList();
			var fields = properties.OrderBy(x => x).ToList();
			Assert.That(string.Join(", ", fields), Is.EqualTo(string.Join(", ", keys)), $"Keys for {entity}");
		}

		[Test]
		public void ExportToXMLFile()
		{
			var officeCodes = parser.GetUKOfficeCodesForTest();
			var content = parser.ConvertToRefModelsForTest(officeCodes);
			parser.ExportToXMLFileForTest(content, UpdateType.Partial, new DateTime(2024, 5, 23, 14, 15, 16));

			Assert.That(File.Exists(parser.OutputFileForTest));
			var xml = File.ReadAllText(parser.OutputFileForTest);
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.UKOfficeCodes.TestFiles.CDSUKCustomsOfficeCodes.xml");
			Assert.That(xml, Is.EqualTo(expectedXml));
		}

		[SetUp]
		public void Setup()
		{
			errorCollector = new StringBuilder();
			var odsData = TestHelper.ReadManifestResourceContentBytes("CargoWise.RefDbRepo.GBReferenceData.Tests.UKOfficeCodes.TestFiles.CDSUKCustomsOfficeCodes.ods");
			parser = new UKOfficeCodesParserForTest(errorCollector, odsData);
		}

		[TearDown]
		public void TearDown()
		{
			if (File.Exists(parser.OutputFileForTest))
			{
				File.Delete(parser.OutputFileForTest);
			}
		}

		StringBuilder errorCollector;
		UKOfficeCodesParserForTest parser;
	}

	sealed class UKOfficeCodesParserForTest : UKOfficeCodesParser
	{
		public UKOfficeCodesParserForTest(StringBuilder errorCollector, byte[] odsData) : base(errorCollector, odsData)
		{
			OutputFileForTest = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "GB_RefCusCodeListZZ_CUSOF.xml");
		}

		public IEnumerable<UKOfficeCode> GetUKOfficeCodesForTest() => GetUKOfficeCodes();

		public UKOfficeCode ConvertToModelForTest(DataRow row) => ConvertToModel(row);
		public IEnumerable<RefCusCodeList> ConvertToRefModelsForTest(IEnumerable<UKOfficeCode> data) => ConvertToRefModels(data);

		public XmlWriterConfiguration XmlWriterConfigurationForTest() => XmlWriterConfiguration();
		public void ExportToXMLFileForTest<T>(IEnumerable<T> codeList, UpdateType updateType, DateTime publicationDate) => ExportToXMLFile<T>(codeList, updateType, publicationDate);
		public string HeaderTagForTest => HeaderTag;
		public string XMLWriterDataSourceForTest => XMLWriterDataSource;

		public readonly string OutputFileForTest;
		protected override string OutputFile => OutputFileForTest;
	}
}
