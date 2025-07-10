using System;
using System.Collections.Concurrent;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	class OdataExpandHelperFixture
	{
		[Test]
		public void Expand()
		{
			var cacheProviderMock = new Mock<ICacheProvider>();
			cacheProviderMock.SetupGet(x => x.MetadataCache).Returns(() => new ConcurrentDictionary<string, Lazy<object>>());
			var cacheProvider = cacheProviderMock.Object;

			var cacheProviderForOriginalMock = new Mock<ICacheProvider>();
			cacheProviderForOriginalMock.SetupGet(x => x.MetadataCache).Returns(() => new ConcurrentDictionary<string, Lazy<object>>());
			var cacheProviderForOriginal = cacheProviderForOriginalMock.Object;

			var metadataProvider = new MetadataProvider(xmlContentWithMutilTables, cacheProvider, cacheProviderForOriginal);
			var expandString = OdataExpandHelper.Expand(typeof(RefCusTariff), metadataProvider, "");
			Assert.AreEqual("RefCusRates($expand=RefCusApplicabilities($expand=RefCusExcludedTradeGroups)),RefCusTariffAttributes,RefCusTariffNationalCodes($expand=RefCusRates($expand=RefCusApplicabilities($expand=RefCusExcludedTradeGroups)),RefCusVATApplicabilities,RefCusTariffAttributes,RefCusTariffUOMs),RefCusTariffRelationships,RefCusConditions($expand=RefCusConditionValues,RefCusApplicabilities($expand=RefCusExcludedTradeGroups)),RefCusTariffUOMs,RefCusVATApplicabilities",
				expandString);

			metadataProvider = new MetadataProvider(xmlContentWithSingleTable, cacheProvider, cacheProviderForOriginal);
			expandString = OdataExpandHelper.Expand(typeof(RefCusTariff), metadataProvider, "");
			Assert.AreEqual("", expandString);
		}

		[Test]
		public void ExpandWithOriginalSchema()
		{
			var cacheProviderMock = new Mock<ICacheProvider>();
			cacheProviderMock.SetupGet(x => x.MetadataCache).Returns(() => new ConcurrentDictionary<string, Lazy<object>>());
			var cacheProvider = cacheProviderMock.Object;

			var cacheProviderForOriginalMock = new Mock<ICacheProvider>();
			cacheProviderForOriginalMock.SetupGet(x => x.MetadataCache).Returns(() => new ConcurrentDictionary<string, Lazy<object>>());
			var cacheProviderForOriginal = cacheProviderForOriginalMock.Object;

			var metadataProvider = new MetadataProvider(xmlContentWithOriginalSchema, cacheProvider, cacheProviderForOriginal);
			var expandString = OdataExpandHelper.Expand(typeof(RefCusTariff), metadataProvider, "");
			Assert.AreEqual("RefCusRates($expand=RefCusApplicabilities($expand=RefCusExcludedTradeGroups),RefCusRateUOMs)",
				expandString);
		}

		const string xmlContentWithMutilTables = @"
<UniversalReferenceData>
	<PublicationTime>2017-08-04T00:00:00</PublicationTime>
	<UpdateType>FULL</UpdateType>
	<Schema>
		<EntityType Name=""RefCusTariff"" Data=""true"">
		  <Key>
			<PropertyRef Name=""ZZ1_ZZI_NKTariffType"" />
			<PropertyRef Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" />
			<PropertyRef Name=""ZZ1_TariffCode"" />
			<PropertyRef Name=""ZZ1_IAMUnique"" />
			<PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
		  </Key>
		  <Property Name=""ZZ1_ZZI_NKTariffType"" Type=""varchar"" MaxLength=""5"" ConstantValue=""IMP"" />
		  <Property Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""EUN"" />
		  <Property Name=""ZZ1_TariffCode"" Type=""varchar"" MaxLength=""35"" />
		  <Property Name=""ZZ1_IAMUnique"" Type=""smallint"" />
		  <Property Name=""ZZ1_Description"" Type=""nvarchar(max)"" />
		  <Property Name=""ZZ1_StartDate"" Type=""smalldatetime"" />
		  <Property Name=""ZZ1_EndDate"" Type=""smalldatetime"" />
		  <Property Name=""ZZ1_ZZF_NKTaxOrFeeCode"" Type=""varchar"" MaxLength=""3"" />
		  <Property Name=""ZZ1_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""EUN"" />
		  <Property Name=""ZZ1_CompositeKeyOnZZ5"" Type=""varchar"" MaxLength=""100"" />
		  <Property Name=""RefCusRate"" Type=""RefCusRate"" Mandatory=""true""/>
		  <Property Name=""RefCusTariffAttribute"" Type=""RefCusTariffAttribute"" />
		  <Property Name=""RefCusTariffNationalCode"" Type=""RefCusTariffNationalCode"" />
		  <Property Name=""RefCusTariffRelationship"" Type=""RefCusTariffRelationship"" />
		  <Property Name=""RefCusCondition"" Type=""RefCusCondition"" />
		  <Property Name=""RefCusTariffUOM"" Type=""RefCusTariffUOM"" />
		  <Property Name=""RefCusVATApplicability"" Type=""RefCusVATApplicability"" />
		</EntityType>		
		<EntityType Name=""RefCusApplicability"" Data=""true"">
		  <Key>
			<PropertyRef Name=""ZZT_ZZA_NKTradeGroup"" />
			<PropertyRef Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" />
			<PropertyRef Name=""ZZT_AdditionalCode"" />
		  </Key>
		  <Property Name=""ZZT_StartDate"" Type=""smalldatetime"" />
		  <Property Name=""ZZT_EndDate"" Type=""smalldatetime"" />
		  <Property Name=""ZZT_ZZA_NKTradeGroup"" Type=""varchar"" MaxLength=""35"" />
		  <Property Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""5"" />
		  <Property Name=""ZZT_AdditionalCode"" Type=""varchar"" MaxLength=""15"" />
		  <Property Name=""ZZT_OrderNumber"" Type=""varchar"" MaxLength=""15"" />
		  <Property Name=""RefCusExcludedTradeGroup"" Type=""RefCusExcludedTradeGroup"" />
		</EntityType>
		<EntityType Name=""RefCusCondition"" Data=""true"">
		  <Key>
			<PropertyRef Name=""ZX1_ZX2_NKConditionType"" />
			<PropertyRef Name=""ZX1_ZX2_ZZZ_NKDataGrouping"" />
			<PropertyRef Name=""ZX1_ZZZ_NKDataGrouping"" />
		  </Key>
		  <Property Name=""ZX1_ZX2_NKConditionType"" Type=""varchar"" MaxLength=""5"" />
		  <Property Name=""ZX1_ZX2_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" />
		  <Property Name=""ZX1_StartDate"" Type=""smalldatetime"" />
		  <Property Name=""ZX1_EndDate"" Type=""smalldatetime"" />
		  <Property Name=""ZX1_Source"" Type=""varchar(max)"" />
		  <Property Name=""ZX1_Comment"" Type=""varchar(max)"" />
		  <Property Name=""ZX1_IsImport"" Type=""bit"" />
		  <Property Name=""ZX1_IsExport"" Type=""bit"" />
		  <Property Name=""ZX1_ConditionValueTrueMeansStop"" Type=""bit"" />
		  <Property Name=""ZX1_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" />
		  <Property Name=""RefCusConditionValue"" Type=""RefCusConditionValue"" />
		  <Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
		</EntityType>
		<EntityType Name=""RefCusConditionValue"" Data=""true"">
		  <Key>
			<PropertyRef Name=""ZX3_Value"" />
			<PropertyRef Name=""ZX3_ZX4_NKValueType"" />
			<PropertyRef Name=""ZX3_ZX4_ZZZ_NKDataGrouping"" />
		  </Key>
		  <Property Name=""ZX3_ZX4_NKValueType"" Type=""varchar"" MaxLength=""5"" />
		  <Property Name=""ZX3_ZX4_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" />
		  <Property Name=""ZX3_Value"" Type=""varchar"" MaxLength=""500"" />
		  <Property Name=""ZX3_StartDate"" Type=""smalldatetime"" />
		  <Property Name=""ZX3_EndDate"" Type=""smalldatetime"" />
		</EntityType>
		<EntityType Name=""RefCusExcludedTradeGroup"" Data=""true"">
		  <Key>
			<PropertyRef Name=""ZZC_ZZA_NKTradeGroup"" />
			<PropertyRef Name=""ZZC_ZZA_ZZZ_NKDataGrouping"" />
		  </Key>
		  <Property Name=""ZZC_ZZA_NKTradeGroup"" Type=""varchar"" MaxLength=""35"" />
		  <Property Name=""ZZC_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""5"" />
		</EntityType>
		<EntityType Name=""RefCusRate"" Data=""true"">
		  <Key>
			<PropertyRef Name=""ZZ2_SelectorFormula"" />
			<PropertyRef Name=""ZZ2_ZZZ_NKDataGrouping"" />
			<PropertyRef Name=""ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"" />
			<PropertyRef Name=""ZZ2_ZY1_ZZR_NKRateType"" />
			<PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
			<PropertyRef Name=""ZZ2_ZZS_ZZZ_NKDataGrouping"" />
			<PropertyRef Name=""ZZ2_ZZS_NKPreference"" />
		  </Key>
		  <Property Name=""ZZ2_StartDate"" Type=""smalldatetime"" />
		  <Property Name=""ZZ2_EndDate"" Type=""smalldatetime"" />
		  <Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" MaxLength=""5"" />
		  <Property Name=""ZZ2_ZY1_ZZR_NKRateType"" Type=""varchar"" MaxLength=""5"" />
		  <Property Name=""ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" />
		  <Property Name=""ZZ2_RateFormula"" Type=""varchar"" MaxLength=""500"" />
		  <Property Name=""ZZ2_ZZS_NKPreference"" Type=""varchar"" MaxLength=""10"" />
		  <Property Name=""ZZ2_ZZS_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" />
		  <Property Name=""ZZ2_SelectorFormula"" Type=""varchar"" MaxLength=""500"" />
		  <Property Name=""ZZ2_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" />
		  <Property Name=""RefCusApplicability"" Type=""RefCusApplicability""  Mandatory=""true""/>
		</EntityType>
		<EntityType Name=""RefCusTariffAttribute"" Data=""true"">
		  <Key>
			<PropertyRef Name=""ZZ3_Name"" />
		  </Key>
		  <Property Name=""ZZ3_Name"" Type=""varchar"" MaxLength=""50"" />
		  <Property Name=""ZZ3_Value"" Type=""nvarchar(max)"" />
		</EntityType>
		<EntityType Name=""RefCusTariffNationalCode"" Data=""true"">
		  <Key>
			<PropertyRef Name=""ZZW_NationalCode"" />
			<PropertyRef Name=""ZZW_ZZF_NKTaxOrFeeCode"" />
			<PropertyRef Name=""ZZW_ZZZ_NKDataGrouping"" />
		  </Key>
		  <Property Name=""ZZW_NationalCode"" Type=""varchar"" MaxLength=""3"" />
		  <Property Name=""ZZW_Description"" Type=""nvarchar(max)"" />
		  <Property Name=""ZZW_ZZF_NKTaxOrFeeCode"" Type=""varchar"" MaxLength=""3"" />
		  <Property Name=""ZZW_StartDate"" Type=""smalldatetime"" />
		  <Property Name=""ZZW_EndDate"" Type=""smalldatetime"" />
		  <Property Name=""ZZW_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" />
		  <Property Name=""RefCusRate"" Type=""RefCusRate"" />
		  <Property Name=""RefCusVATApplicability"" Type=""RefCusVATApplicability"" />
		  <Property Name=""RefCusTariffAttribute"" Type=""RefCusTariffAttribute"" />
		  <Property Name=""RefCusTariffUOM"" Type=""RefCusTariffUOM"" />
		</EntityType>
		<EntityType Name=""RefCusTariffRelationship"" Data=""true"">
		  <Key>
			<PropertyRef Name=""ZZH_ZZI_NKTariffType"" />
		  </Key>
		  <Property Name=""ZZH_ZZI_NKTariffType"" Type=""varchar"" MaxLength=""5"" />
		  <Property Name=""ZZH_TariffCode"" Type=""varchar"" MaxLength=""35"" />
		</EntityType>
		<EntityType Name=""RefCusTariffUOM"" Data=""true"">
		  <Key>
			<PropertyRef Name=""ZZ8_Type"" />
		  </Key>
		  <Property Name=""ZZ8_Type"" Type=""varchar"" MaxLength=""3"" />
		  <Property Name=""ZZ8_UOM"" Type=""varchar"" MaxLength=""10"" />
		</EntityType>
		<EntityType Name=""RefCusVATApplicability"" Data=""true"">
		  <Key>
			<PropertyRef Name=""ZX5_ZZF_NKTaxOrFeeCode"" />
			<PropertyRef Name=""ZX5_AdditionalCode"" />
		  </Key>
		  <Property Name=""ZX5_ZZF_NKTaxOrFeeCode"" Type=""varchar"" MaxLength=""3"" />
		  <Property Name=""ZX5_StartDate"" Type=""smalldatetime"" />
		  <Property Name=""ZX5_EndDate"" Type=""smalldatetime"" />
		  <Property Name=""ZX5_AdditionalCode"" Type=""varchar"" MaxLength=""15"" />
		  <Property Name=""ZX5_Description"" Type=""nvarchar(max)"" />
		  <Property Name=""ZX5_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" />
		</EntityType>
	</Schema>
</UniversalReferenceData>
";

		const string xmlContentWithSingleTable = @"
<UniversalReferenceData>
	<PublicationTime>2017-08-04T00:00:00</PublicationTime>
	<UpdateType>FULL</UpdateType>
	<Schema>
		<EntityType Name=""RefCusTariff"" Data=""true"">
		  <Key>
			<PropertyRef Name=""ZZ1_ZZI_NKTariffType"" />
			<PropertyRef Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" />
			<PropertyRef Name=""ZZ1_TariffCode"" />
			<PropertyRef Name=""ZZ1_IAMUnique"" />
			<PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
		  </Key>
		  <Property Name=""ZZ1_ZZI_NKTariffType"" Type=""varchar"" MaxLength=""5"" ConstantValue=""IMP"" />
		  <Property Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""EUN"" />
		  <Property Name=""ZZ1_TariffCode"" Type=""varchar"" MaxLength=""35"" />
		  <Property Name=""ZZ1_IAMUnique"" Type=""smallint"" />
		  <Property Name=""ZZ1_Description"" Type=""nvarchar(max)"" />
		  <Property Name=""ZZ1_StartDate"" Type=""smalldatetime"" />
		  <Property Name=""ZZ1_EndDate"" Type=""smalldatetime"" />
		  <Property Name=""ZZ1_ZZF_NKTaxOrFeeCode"" Type=""varchar"" MaxLength=""3"" />
		  <Property Name=""ZZ1_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""EUN"" />
		  <Property Name=""ZZ1_CompositeKeyOnZZ5"" Type=""varchar"" MaxLength=""100"" />
		</EntityType>		
	</Schema>
</UniversalReferenceData>
";

		const string xmlContentWithOriginalSchema = @"
<UniversalReferenceData>
    <DataSource>Test For Merger query Data</DataSource>
    <PublicationTime>2022-01-18T13:31:25</PublicationTime>
    <UpdateType>Full</UpdateType>
    <Schema>
        <EntityType Name=""RefCusTariff"">
            <Property Name=""RefCusRateApplicability"" Type=""RefCusRateApplicability"" />
        </EntityType>
        <EntityType Name=""RefCusRateApplicability"" Data=""true"">
            <Key>
                <PropertyRef Name=""RefCusRateApplicabilityUOM"" />
                <PropertyRef Name=""RefCusExcludedTradeGroupNew"" />
            </Key>
            <Property Name=""RefCusRateApplicabilityUOM"" Type=""RefCusRateApplicabilityUOM"" />
            <Property Name=""RefCusExcludedTradeGroupNew"" Type=""RefCusExcludedTradeGroupNew"" />
        </EntityType>
        <EntityType Name=""RefCusRateApplicabilityUOM"" Data=""true"">
        </EntityType>
        <EntityType Name=""RefCusExcludedTradeGroupNew"" Data=""true"">
        </EntityType>
    </Schema>
    <OriginalSchema>
        <EntityType Name=""RefCusTariff"">
            <Key>
            </Key>
            <Property Name=""RefCusRate"" Type=""RefCusRate"" />
        </EntityType>
        <EntityType Name=""RefCusRate"" Data=""true"">
            <Key>
                <PropertyRef Name=""RefCusApplicability"" />
                <PropertyRef Name=""RefCusRateUOM"" />
            </Key>
            <Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
            <Property Name=""RefCusRateUOM"" Type=""RefCusRateUOM"" />
        </EntityType>
        <EntityType Name=""RefCusApplicability"" Data=""true"">
            <Key>
                <PropertyRef Name=""RefCusExcludedTradeGroup"" />
            </Key>
            <Property Name=""RefCusExcludedTradeGroup"" Type=""RefCusExcludedTradeGroup"" />
        </EntityType>
        <EntityType Name=""RefCusRateUOM"" Data=""true"">
        </EntityType>
        <EntityType Name=""RefCusExcludedTradeGroup"" Data=""true"">
        </EntityType>
    </OriginalSchema>
</UniversalReferenceData>";
	}
}
