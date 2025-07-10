using System;
using System.Reflection;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Test.XmlParser
{
	[TestFixture]
	class UniversalXmlWrapperFixtures : BaseUnitTestFixture
	{
		[Test]
		public void IsValid()
		{
			var wrapper = new UniversalXmlWrapper();

			var expectedMessage = "PublicationTime element is missing.";
			wrapper.DataSource = "anything";

			wrapper.IsValid(out var message2);
			Assert.IsNotEmpty(message2);
			Assert.That(message2, Is.EqualTo(expectedMessage));

			expectedMessage = "Schema element is missing.";
			wrapper.PublicationTime = new DateTime(2018, 10, 1);

			wrapper.IsValid(out var message3);
			Assert.IsNotEmpty(message3);
			Assert.That(message3, Is.EqualTo(expectedMessage));

			wrapper.SchemaXml = "schema element";

			wrapper.IsValid(out var message4);
			Assert.IsEmpty(message4);
		}

		[Test]
		public void ValidateSchemaXmlContent_RefCusCodeListAttribute_EnableExpirable_MissingDates()
		{
			var schemaContent = @"<Schema>
    <EntityType Name=""RefCusCodeList"" Data=""true"">
      <Key>
        <PropertyRef Name=""ZZD_Code"" />
        <PropertyRef Name=""ZZD_ZZK_NKCodeType"" />
        <PropertyRef Name=""ZZD_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""RefCusCodeListAttribute"" Type=""RefCusCodeListAttribute"" />
      <Property Name=""ZZD_Code"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZZD_Description"" Type=""nvarchar"" MaxLength=""2000"" />
      <Property Name=""ZZD_EndDate"" Type=""smalldatetime"" DefaultValue=""2079-06-06T23:59:00"" />
      <Property Name=""ZZD_StartDate"" Type=""smalldatetime"" DefaultValue=""1900-01-01T00:00:00"" />
      <Property Name=""ZZD_ZZK_NKCodeType"" Type=""varchar"" MaxLength=""5"" DefaultValue=""TESTR"" />
      <Property Name=""ZZD_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""AU"" />
    </EntityType>
    <EntityType Name=""RefCusCodeListAttribute"" Data=""true"" EnableExpirable=""true"">
      <Key>
        <PropertyRef Name=""ZZE_ZXE_NKName"" />
      </Key>
      <Property Name=""ZZE_Value"" Type=""nvarchar"" MaxLength=""255"" />
      <Property Name=""ZZE_ZXE_NKName"" Type=""varchar"" MaxLength=""32"" DefaultValue=""TestName"" />
    </EntityType>
  </Schema>";
			var wrapper = new UniversalXmlWrapper { SchemaXml = schemaContent };
			wrapper.ValidateSchemaXmlContent(out var message);
			Assert.IsNotEmpty(message);
			Assert.That(message, Is.EqualTo($"{nameof(RefCusCodeListAttribute)}'s EnableExpirable attribute is set to true and is missing {nameof(RefCusCodeListAttribute.ZZE_StartDate)} and/or {nameof(RefCusCodeListAttribute.ZZE_EndDate)} element(s)."));
		}

		[Test]
		public void ValidateSchemaXmlContent_RefCusCodeListAttributeName_EnableExpirable()
		{
			var schemaContent = @"<Schema>
    <EntityType Name=""RefCusCodeListAttributeName"" Data=""true"">
      <Key>
        <PropertyRef Name=""ZXE_Name"" />
        <PropertyRef Name=""ZXE_ZZK_NKCodeType"" />
        <PropertyRef Name=""ZXE_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""ZXE_Description"" Type=""varchar"" MaxLength=""500"" />
      <Property Name=""ZXE_Name"" Type=""varchar"" MaxLength=""32"" />
      <Property Name=""ZXE_ValueDataType"" Type=""varchar"" MaxLength=""7"" />
      <Property Name=""ZXE_ZZK_NKCodeType"" Type=""varchar"" MaxLength=""5"" ConstantValue=""TAX"" />
      <Property Name=""ZXE_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""FR"" />
    </EntityType>
  </Schema>";
			var wrapper = new UniversalXmlWrapper { SchemaXml = schemaContent };
			wrapper.ValidateSchemaXmlContent(out var message);
			Assert.IsEmpty(message);
		}

		[Test]
		public void ValidateSchemaXmlContent_RefCusCodeListAttribute_NotEnableExpirable_AddedDates()
		{
			var schemaContent = @"<Schema>
    <EntityType Name=""RefCusCodeList"" Data=""true"">
      <Key>
        <PropertyRef Name=""ZZD_Code"" />
        <PropertyRef Name=""ZZD_ZZK_NKCodeType"" />
        <PropertyRef Name=""ZZD_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""RefCusCodeListAttribute"" Type=""RefCusCodeListAttribute"" />
      <Property Name=""ZZD_Code"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZZD_Description"" Type=""nvarchar"" MaxLength=""2000"" />
      <Property Name=""ZZD_EndDate"" Type=""smalldatetime"" DefaultValue=""2079-06-06T23:59:00"" />
      <Property Name=""ZZD_StartDate"" Type=""smalldatetime"" DefaultValue=""1900-01-01T00:00:00"" />
      <Property Name=""ZZD_ZZK_NKCodeType"" Type=""varchar"" MaxLength=""5"" DefaultValue=""TESTR"" />
      <Property Name=""ZZD_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""AU"" />
    </EntityType>
    <EntityType Name=""RefCusCodeListAttribute"" Data=""true"" EnableExpirable=""false"">
      <Key>
        <PropertyRef Name=""ZZE_ZXE_NKName"" />
      </Key>
      <Property Name=""ZZE_EndDate"" Type=""smalldatetime"" DefaultValue=""2079-06-06T23:59:00"" />
      <Property Name=""ZZE_StartDate"" Type=""smalldatetime"" DefaultValue=""1900-01-01T00:00:00"" />
      <Property Name=""ZZE_Value"" Type=""nvarchar"" MaxLength=""255"" />
      <Property Name=""ZZE_ZXE_NKName"" Type=""varchar"" MaxLength=""32"" DefaultValue=""TestName"" />
    </EntityType>
  </Schema>";
			var wrapper = new UniversalXmlWrapper { SchemaXml = schemaContent };
			wrapper.ValidateSchemaXmlContent(out var message);
			Assert.IsNotEmpty(message);
			Assert.That(message, Is.EqualTo($"{nameof(RefCusCodeListAttribute)}'s EnableExpirable attribute is missing or set to false and must not have {nameof(RefCusCodeListAttribute.ZZE_StartDate)} and {nameof(RefCusCodeListAttribute.ZZE_EndDate)} elements."));
		}

		[Test]
		public void ValidateSchemaXmlContent_RefCusCodeListAttribute_MissingEnableExpirable_AddedDates()
		{
			var schemaContent = @"<Schema>
    <EntityType Name=""RefCusCodeList"" Data=""true"">
      <Key>
        <PropertyRef Name=""ZZD_Code"" />
        <PropertyRef Name=""ZZD_ZZK_NKCodeType"" />
        <PropertyRef Name=""ZZD_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""RefCusCodeListAttribute"" Type=""RefCusCodeListAttribute"" />
      <Property Name=""ZZD_Code"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZZD_Description"" Type=""nvarchar"" MaxLength=""2000"" />
      <Property Name=""ZZD_EndDate"" Type=""smalldatetime"" DefaultValue=""2079-06-06T23:59:00"" />
      <Property Name=""ZZD_StartDate"" Type=""smalldatetime"" DefaultValue=""1900-01-01T00:00:00"" />
      <Property Name=""ZZD_ZZK_NKCodeType"" Type=""varchar"" MaxLength=""5"" DefaultValue=""TESTR"" />
      <Property Name=""ZZD_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""AU"" />
    </EntityType>
    <EntityType Name=""RefCusCodeListAttribute"" Data=""true"">
      <Key>
        <PropertyRef Name=""ZZE_ZXE_NKName"" />
      </Key>
      <Property Name=""ZZE_EndDate"" Type=""smalldatetime"" DefaultValue=""2079-06-06T23:59:00"" />
      <Property Name=""ZZE_StartDate"" Type=""smalldatetime"" DefaultValue=""1900-01-01T00:00:00"" />
      <Property Name=""ZZE_Value"" Type=""nvarchar"" MaxLength=""255"" />
      <Property Name=""ZZE_ZXE_NKName"" Type=""varchar"" MaxLength=""32"" DefaultValue=""TestName"" />
    </EntityType>
  </Schema>";
			var wrapper = new UniversalXmlWrapper { SchemaXml = schemaContent };
			wrapper.ValidateSchemaXmlContent(out var message);
			Assert.IsNotEmpty(message);
			Assert.That(message, Is.EqualTo($"{nameof(RefCusCodeListAttribute)}'s EnableExpirable attribute is missing or set to false and must not have {nameof(RefCusCodeListAttribute.ZZE_StartDate)} and {nameof(RefCusCodeListAttribute.ZZE_EndDate)} elements."));
		}
		[Test]
		public void ValidateSchemaXmlContent_RefCusTariffUOM_EnableExpirable_MissingDates()
		{
			var schemaContent = @"<Schema>
	<EntityType Name=""RefCusTariff"" Data=""true"">
      <Key>
        <PropertyRef Name=""ZZ1_IAMUnique"" />
        <PropertyRef Name=""ZZ1_TariffCode"" />
        <PropertyRef Name=""ZZ1_ZZI_NKTariffType"" />
        <PropertyRef Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" />
        <PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""RefCusTariffUOM"" Type=""RefCusTariffUOM"" />
      <Property Name=""ZZ1_CompositeKeyOnZZ5"" Type=""varchar"" MaxLength=""100"" />
      <Property Name=""ZZ1_Description"" Type=""nvarchar"" />
      <Property Name=""ZZ1_EndDate"" Type=""datetime"" />
      <Property Name=""ZZ1_IAMUnique"" Type=""smallint"" />
      <Property Name=""ZZ1_StartDate"" Type=""datetime"" />
      <Property Name=""ZZ1_TariffCode"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZZ1_ZZI_NKTariffType"" Type=""varchar"" MaxLength=""5"" />
      <Property Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""GB"" />
      <Property Name=""ZZ1_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""GB"" />
    </EntityType>
    <EntityType Name=""RefCusTariffUOM"" Data=""true"" EnableExpirable=""true"">
      <Key>
        <PropertyRef Name=""ZZ8_Type"" />
        <PropertyRef Name=""ZZ8_ZZA_NKTradeGroup"" />
        <PropertyRef Name=""ZZ8_ZZA_ZZZ_NKDataGrouping"" />
        <PropertyRef Name=""ZZ8_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""ZZ8_Type"" Type=""varchar"" MaxLength=""3"" />
      <Property Name=""ZZ8_UOM"" Type=""varchar"" MaxLength=""10"" />
      <Property Name=""ZZ8_ZZA_NKTradeGroup"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZZ8_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" />
      <Property Name=""ZZ8_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""GB"" />
    </EntityType>
  </Schema>";
			var wrapper = new UniversalXmlWrapper { SchemaXml = schemaContent };
			wrapper.ValidateSchemaXmlContent(out var message);
			Assert.IsNotEmpty(message);
			Assert.That(message, Is.EqualTo($"{nameof(RefCusTariffUOM)}'s EnableExpirable attribute is set to true and is missing {nameof(RefCusTariffUOM.ZZ8_StartDate)} and/or {nameof(RefCusTariffUOM.ZZ8_EndDate)} element(s)."));
		}

		[Test]
		public void ValidateSchemaXmlContent_RefCusTariffUOM_NotEnableExpirable_AddedDates()
		{
			var schemaContent = @"<Schema>
	<EntityType Name=""RefCusTariff"" Data=""true"">
      <Key>
        <PropertyRef Name=""ZZ1_IAMUnique"" />
        <PropertyRef Name=""ZZ1_TariffCode"" />
        <PropertyRef Name=""ZZ1_ZZI_NKTariffType"" />
        <PropertyRef Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" />
        <PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""RefCusTariffUOM"" Type=""RefCusTariffUOM"" />
      <Property Name=""ZZ1_CompositeKeyOnZZ5"" Type=""varchar"" MaxLength=""100"" />
      <Property Name=""ZZ1_Description"" Type=""nvarchar"" />
      <Property Name=""ZZ1_EndDate"" Type=""datetime"" />
      <Property Name=""ZZ1_IAMUnique"" Type=""smallint"" />
      <Property Name=""ZZ1_StartDate"" Type=""datetime"" />
      <Property Name=""ZZ1_TariffCode"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZZ1_ZZI_NKTariffType"" Type=""varchar"" MaxLength=""5"" />
      <Property Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""GB"" />
      <Property Name=""ZZ1_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""GB"" />
    </EntityType>
    <EntityType Name=""RefCusTariffUOM"" Data=""true"" EnableExpirable=""false"">
      <Key>
        <PropertyRef Name=""ZZ8_Type"" />
        <PropertyRef Name=""ZZ8_ZZA_NKTradeGroup"" />
        <PropertyRef Name=""ZZ8_ZZA_ZZZ_NKDataGrouping"" />
        <PropertyRef Name=""ZZ8_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""ZZ8_Type"" Type=""varchar"" MaxLength=""3"" />
      <Property Name=""ZZ8_UOM"" Type=""varchar"" MaxLength=""10"" />
      <Property Name=""ZZ8_EndDate"" Type=""smalldatetime"" DefaultValue=""2079-06-06T23:59:00"" />
      <Property Name=""ZZ8_StartDate"" Type=""smalldatetime"" DefaultValue=""1900-01-01T00:00:00"" />
      <Property Name=""ZZ8_ZZA_NKTradeGroup"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZZ8_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" />
      <Property Name=""ZZ8_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""GB"" />
    </EntityType>
  </Schema>";
			var wrapper = new UniversalXmlWrapper { SchemaXml = schemaContent };
			wrapper.ValidateSchemaXmlContent(out var message);
			Assert.IsNotEmpty(message);
			Assert.That(message, Is.EqualTo($"{nameof(RefCusTariffUOM)}'s EnableExpirable attribute is missing or set to false and must not have {nameof(RefCusTariffUOM.ZZ8_StartDate)} and {nameof(RefCusTariffUOM.ZZ8_EndDate)} elements."));
		}

		[Test]
		public void ValidateSchemaXmlContent_RefCusTariffUOM_MissingEnableExpirable_AddedDates()
		{
			var schemaContent = @"<Schema>
	<EntityType Name=""RefCusTariff"" Data=""true"">
      <Key>
        <PropertyRef Name=""ZZ1_IAMUnique"" />
        <PropertyRef Name=""ZZ1_TariffCode"" />
        <PropertyRef Name=""ZZ1_ZZI_NKTariffType"" />
        <PropertyRef Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" />
        <PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""RefCusTariffUOM"" Type=""RefCusTariffUOM"" />
      <Property Name=""ZZ1_CompositeKeyOnZZ5"" Type=""varchar"" MaxLength=""100"" />
      <Property Name=""ZZ1_Description"" Type=""nvarchar"" />
      <Property Name=""ZZ1_EndDate"" Type=""datetime"" />
      <Property Name=""ZZ1_IAMUnique"" Type=""smallint"" />
      <Property Name=""ZZ1_StartDate"" Type=""datetime"" />
      <Property Name=""ZZ1_TariffCode"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZZ1_ZZI_NKTariffType"" Type=""varchar"" MaxLength=""5"" />
      <Property Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""GB"" />
      <Property Name=""ZZ1_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""GB"" />
    </EntityType>
    <EntityType Name=""RefCusTariffUOM"" Data=""true"" >
      <Key>
        <PropertyRef Name=""ZZ8_Type"" />
        <PropertyRef Name=""ZZ8_ZZA_NKTradeGroup"" />
        <PropertyRef Name=""ZZ8_ZZA_ZZZ_NKDataGrouping"" />
        <PropertyRef Name=""ZZ8_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""ZZ8_Type"" Type=""varchar"" MaxLength=""3"" />
      <Property Name=""ZZ8_UOM"" Type=""varchar"" MaxLength=""10"" />
      <Property Name=""ZZ8_EndDate"" Type=""smalldatetime"" DefaultValue=""2079-06-06T23:59:00"" />
      <Property Name=""ZZ8_StartDate"" Type=""smalldatetime"" DefaultValue=""1900-01-01T00:00:00"" />
      <Property Name=""ZZ8_ZZA_NKTradeGroup"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZZ8_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" />
      <Property Name=""ZZ8_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""GB"" />
    </EntityType>
  </Schema>";
			var wrapper = new UniversalXmlWrapper { SchemaXml = schemaContent };
			wrapper.ValidateSchemaXmlContent(out var message);
			Assert.IsNotEmpty(message);
			Assert.That(message, Is.EqualTo($"{nameof(RefCusTariffUOM)}'s EnableExpirable attribute is missing or set to false and must not have {nameof(RefCusTariffUOM.ZZ8_StartDate)} and {nameof(RefCusTariffUOM.ZZ8_EndDate)} elements."));
		}

		[Test]
		public void HasErrors()
		{
			var wrapper = new UniversalXmlWrapper();

			wrapper.ErrorMessage = "Some error";
			Assert.IsTrue(wrapper.HasErrors());
		}

		[Test]
		public void PopulateCommanSeperatedMetaDataStructure_ValidSchema()
		{
			var wrapper = new UniversalXmlWrapper();
			wrapper.SchemaXml = @"<Schema>
    <EntityType Name=""RefCusTariff"" Data=""true"">
      <Key>
        <PropertyRef Name=""ZZ1_TariffCode"" />
        <PropertyRef Name=""ZZ1_ZZI_NKTariffType"" />
        <PropertyRef Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" />
        <PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""RefCusTariffRelationship"" Type=""RefCusTariffRelationship"" />
      <Property Name=""RefCusTariffBRCharacteristic"" Type=""RefCusTariffBRCharacteristic"" />
      <Property Name=""ZZ1_Description"" Type=""nvarchar"" />
      <Property Name=""ZZ1_EndDate"" Type=""datetime"" DefaultValue=""2079-06-06T23:59:00"" />
      <Property Name=""ZZ1_StartDate"" Type=""datetime"" />
      <Property Name=""ZZ1_TariffCode"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZZ1_ZZF_NKTaxOrFeeCode"" Type=""varchar"" MaxLength=""3"" />
      <Property Name=""ZZ1_ZZI_NKTariffType"" Type=""varchar"" MaxLength=""5"" />
      <Property Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""CN"" />
      <Property Name=""ZZ1_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""CN"" />
    </EntityType>
    <EntityType Name=""RefCusTariffRelationship"" Data=""true"">
      <Key>
        <PropertyRef Name=""ZZH_TariffCode"" />
        <PropertyRef Name=""ZZH_ZZI_NKTariffType"" />
        <PropertyRef Name=""ZZH_ZZI_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""ZZH_TariffCode"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZZH_ZZI_NKTariffType"" Type=""varchar"" MaxLength=""5"" ConstantValue=""HSN"" />
      <Property Name=""ZZH_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""CN"" />
    </EntityType>
  </Schema>";
			wrapper.PopulateXmlSchemaEntityRelationshipMapList();
			Assert.AreEqual(2, wrapper.XmlSchemaEntityRelationshipMapList.Count);
			Assert.AreEqual("RefCusTariff", wrapper.XmlSchemaEntityRelationshipMapList[0].ParentEntityName);
			Assert.AreEqual(1, wrapper.XmlSchemaEntityRelationshipMapList[0].ChildEntityList.Count);
			Assert.AreEqual("RefCusTariffRelationship", wrapper.XmlSchemaEntityRelationshipMapList[0].ChildEntityList[0]);

			Assert.AreEqual("RefCusTariffRelationship", wrapper.XmlSchemaEntityRelationshipMapList[1].ParentEntityName);
			Assert.AreEqual(0, wrapper.XmlSchemaEntityRelationshipMapList[1].ChildEntityList.Count);
		}

		[Test]
		public void PopulateCommanSeperatedMetaDataStructure_InvalidSchemaChildNotDeclared()
		{
			var wrapper = new UniversalXmlWrapper();
			wrapper.SchemaXml = @"<Schema>
    <EntityType Name=""RefCusTariff"" Data=""true"">
      <Key>
        <PropertyRef Name=""ZZ1_TariffCode"" />
        <PropertyRef Name=""ZZ1_ZZI_NKTariffType"" />
        <PropertyRef Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" />
        <PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""ZZ1_Description"" Type=""nvarchar"" />
      <Property Name=""ZZ1_EndDate"" Type=""datetime"" DefaultValue=""2079-06-06T23:59:00"" />
      <Property Name=""ZZ1_StartDate"" Type=""datetime"" />
      <Property Name=""ZZ1_TariffCode"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZZ1_ZZF_NKTaxOrFeeCode"" Type=""varchar"" MaxLength=""3"" />
      <Property Name=""ZZ1_ZZI_NKTariffType"" Type=""varchar"" MaxLength=""5"" />
      <Property Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""CN"" />
      <Property Name=""ZZ1_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""CN"" />
    </EntityType>
    <EntityType Name=""RefCusTariffRelationship"" Data=""true"">
      <Key>
        <PropertyRef Name=""ZZH_TariffCode"" />
        <PropertyRef Name=""ZZH_ZZI_NKTariffType"" />
        <PropertyRef Name=""ZZH_ZZI_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""ZZH_TariffCode"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZZH_ZZI_NKTariffType"" Type=""varchar"" MaxLength=""5"" ConstantValue=""HSN"" />
      <Property Name=""ZZH_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""CN"" />
    </EntityType>
  </Schema>";
			wrapper.PopulateXmlSchemaEntityRelationshipMapList();
			Assert.AreEqual(1, wrapper.XmlSchemaEntityRelationshipMapList.Count);
			Assert.AreEqual("RefCusTariff", wrapper.XmlSchemaEntityRelationshipMapList[0].ParentEntityName);
			Assert.AreEqual(0, wrapper.XmlSchemaEntityRelationshipMapList[0].ChildEntityList.Count);
		}

		[Test]
		public void PopulateCommanSeperatedMetaDataStructure_InvalidSchemaChildDeclaredButNotDefined()
		{
			var wrapper = new UniversalXmlWrapper();
			wrapper.SchemaXml = @"<Schema>
    <EntityType Name=""RefCusTariff"" Data=""true"">
      <Key>
        <PropertyRef Name=""ZZ1_TariffCode"" />
        <PropertyRef Name=""ZZ1_ZZI_NKTariffType"" />
        <PropertyRef Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" />
        <PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""RefCusTariffRelationship"" Type=""RefCusTariffRelationship"" />
      <Property Name=""ZZ1_Description"" Type=""nvarchar"" />
      <Property Name=""ZZ1_EndDate"" Type=""datetime"" DefaultValue=""2079-06-06T23:59:00"" />
      <Property Name=""ZZ1_StartDate"" Type=""datetime"" />
      <Property Name=""ZZ1_TariffCode"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZZ1_ZZF_NKTaxOrFeeCode"" Type=""varchar"" MaxLength=""3"" />
      <Property Name=""ZZ1_ZZI_NKTariffType"" Type=""varchar"" MaxLength=""5"" />
      <Property Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""CN"" />
      <Property Name=""ZZ1_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""CN"" />
    </EntityType>
  </Schema>";
			wrapper.PopulateXmlSchemaEntityRelationshipMapList();
			Assert.AreEqual(1, wrapper.XmlSchemaEntityRelationshipMapList.Count);
			Assert.AreEqual("RefCusTariff", wrapper.XmlSchemaEntityRelationshipMapList[0].ParentEntityName);
			Assert.AreEqual(0, wrapper.XmlSchemaEntityRelationshipMapList[0].ChildEntityList.Count);
			Assert.AreEqual("EntityType RefCusTariffRelationship element is missing.", wrapper.ErrorMessage);
		}
	}
}
