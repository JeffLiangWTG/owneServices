using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.UniversalXmlParser.Providers;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Test.Providers;

[TestFixture]
[TransactionedTestCase]
[Property("DAT:CapabilityRequirements", "SQL2019+")]
public class EntityValuesProviderFixtures : BaseUnitTestFixture
{
	#region Member Variables

	readonly List<Tuple<string, string>> tableColumnTuples = new List<Tuple<string, string>>
	{
		new Tuple<string, string>("RefCarrierCode", "ZZ4_PK"),
		new Tuple<string, string>("RefCarrierCodeAttribute", "ZZG_PK"),
		new Tuple<string, string>("RefCusApplicability", "ZZT_PK"),
		new Tuple<string, string>("RefCusCodeList", "ZZD_PK"),
		new Tuple<string, string>("RefCusCodeListAttribute", "ZZE_PK"),
		new Tuple<string, string>("RefCusCodeType", "ZZK_PK"),
		new Tuple<string, string>("RefCusCondition", "ZX1_PK"),
		new Tuple<string, string>("RefCusCondition", "ZX1_StartDate"),
		new Tuple<string, string>("RefCusCondition", "ZX1_EndDate"),
		new Tuple<string, string>("RefCusCondition", "ZX1_Source"),
		new Tuple<string, string>("RefCusCondition", "ZX1_Comment"),
		new Tuple<string, string>("RefCusCondition", "ZX1_IsImport"),
		new Tuple<string, string>("RefCusCondition", "ZX1_IsExport"),
		new Tuple<string, string>("RefCusCondition", "ZX1_ConditionValueTrueMeansStop"),
		new Tuple<string, string>("RefCusConditionType", "ZX2_PK"),
		new Tuple<string, string>("RefCusConditionValue", "ZX3_PK"),
		new Tuple<string, string>("RefCusConditionValue", "ZX3_LogicalORWithinGroup"),
		new Tuple<string, string>("RefCusConditionValueType", "ZX4_PK"),
		new Tuple<string, string>("RefCusConditionValueType", "ZX4_IsFormula"),
		new Tuple<string, string>("RefCusExcludedTradeGroup", "ZZC_PK"),
		new Tuple<string, string>("RefCusMap", "ZZM_PK"),
		new Tuple<string, string>("RefCusMapType", "ZZP_PK"),
		new Tuple<string, string>("RefCusMapType", "ZZP_Direction"),
		new Tuple<string, string>("RefCusMapType", "ZZP_IsReadonly"),
		new Tuple<string, string>("RefCusNomenclatureGroup", "ZZ5_PK"),
		new Tuple<string, string>("RefCusNomenclatureGroup", "ZZ5_ZZ9_NKNomenclatureGroupType"),
		new Tuple<string, string>("RefCusNomenclatureGroupNote", "ZZL_PK"),
		new Tuple<string, string>("RefCusNomenclatureGroupType", "ZZ9_PK"),
		new Tuple<string, string>("RefCusNomenclatureLanguage", "ZX8_PK"),
		new Tuple<string, string>("RefCusNomenclatureLanguage", "ZX8_Description"),
		new Tuple<string, string>("RefCusPreference", "ZZS_PK"),
		new Tuple<string, string>("RefCusPreferenceLanguage", "ZX9_PK"),
		new Tuple<string, string>("RefCusPreferenceLanguage", "ZX9_Description"),
		new Tuple<string, string>("RefCusProcedure", "ZZ6_PK"),
		new Tuple<string, string>("RefCusProcedure", "ZZ6_Category"),
		new Tuple<string, string>("RefCusProcedure", "ZZ6_PreviousProcedureCode"),
		new Tuple<string, string>("RefCusProcedure", "ZZ6_Concession"),
		new Tuple<string, string>("RefCusProcedure", "ZZ6_ShipmentType"),
		new Tuple<string, string>("RefCusProcedure", "ZZ6_CalculateDuty"),
		new Tuple<string, string>("RefCusProcedure", "ZZ6_Group"),
		new Tuple<string, string>("RefCusProcedure", "ZZ6_LandedCost"),
		new Tuple<string, string>("RefCusProcedure", "ZZ6_IntoWarehouse"),
		new Tuple<string, string>("RefCusProcedure", "ZZ6_OutOfWarehouse"),
		new Tuple<string, string>("RefCusRate", "ZZ2_PK"),
		new Tuple<string, string>("RefCusRate", "ZZ2_SelectorFormula"),
		new Tuple<string, string>("RefCusRate", "ZZ2_ZZZ_NKDataGrouping"),
		new Tuple<string, string>("RefCusRateCode", "ZY1_PK"),
		new Tuple<string, string>("RefCusRateType", "ZZR_PK"),
		new Tuple<string, string>("RefCusRateType", "ZZR_IsPayable"),
		new Tuple<string, string>("RefCusTariff", "ZZ1_PK"),
		new Tuple<string, string>("RefCusTariff", "ZZ1_ZZI_NKTariffType"),
		new Tuple<string, string>("RefCusTariff", "ZZ1_IAMUnique"),
		new Tuple<string, string>("RefCusTariff", "ZZ1_Description"),
		new Tuple<string, string>("RefCusTariff", "ZZ1_ZZF_NKTaxOrFeeCode"),
		new Tuple<string, string>("RefCusTariff", "ZZ1_ZZZ_NKDataGrouping"),
		new Tuple<string, string>("RefCusTariff", "ZZ1_CompositeKeyOnZZ5"),
		new Tuple<string, string>("RefCusTariffAttribute", "ZZ3_PK"),
		new Tuple<string, string>("RefCusTariffAttribute", "ZZ3_Name"),
		new Tuple<string, string>("RefCusTariffAttribute", "ZZ3_Value"),
		new Tuple<string, string>("RefCusTariffLanguage", "ZX7_PK"),
		new Tuple<string, string>("RefCusTariffLanguage", "ZX7_Description"),
		new Tuple<string, string>("RefCusTariffNationalCode", "ZZW_PK"),
		new Tuple<string, string>("RefCusTariffNationalCode", "ZZW_NationalCode"),
		new Tuple<string, string>("RefCusTariffRelationship", "ZZH_PK"),
		new Tuple<string, string>("RefCusTariffRelationship", "ZZH_ZZI_NKTariffType"),
		new Tuple<string, string>("RefCusTariffRelationship", "ZZH_TariffCode"),
		new Tuple<string, string>("RefCusTariffType", "ZZI_PK"),
		new Tuple<string, string>("RefCusTariffType", "ZZI_TariffType"),
		new Tuple<string, string>("RefCusTariffType", "ZZI_Description"),
		new Tuple<string, string>("RefCusTariffType", "ZZI_ZZZ_NKDataGrouping"),
		new Tuple<string, string>("RefCusTariffType", "ZZI_ZZR_NKRateType"),
		new Tuple<string, string>("RefCusTariffType", "ZZI_HasFormulaSpecificQuestions"),
		new Tuple<string, string>("RefCusTariffUOM", "ZZ8_PK"),
		new Tuple<string, string>("RefCusTaxOrFee", "ZZF_PK"),
		new Tuple<string, string>("RefCusTaxOrFee", "ZZF_Value"),
		new Tuple<string, string>("RefCusTradeGroup", "ZZA_PK"),
		new Tuple<string, string>("RefCusTradeGroup", "ZZA_StartDate"),
		new Tuple<string, string>("RefCusTradeGroup", "ZZA_EndDate"),
		new Tuple<string, string>("RefCusTradeGroupCountry", "ZZB_PK"),
		new Tuple<string, string>("RefCusTradeGroupCountry", "ZZB_StartDate"),
		new Tuple<string, string>("RefCusTradeGroupCountry", "ZZB_EndDate"),
		new Tuple<string, string>("RefCusVATApplicability", "ZX5_ZZF_NKTaxOrFeeCode"),
		new Tuple<string, string>("RefCusVATApplicability", "ZX5_StartDate"),
		new Tuple<string, string>("RefCusVATApplicability", "ZX5_EndDate"),
		new Tuple<string, string>("RefCusVATApplicability", "ZX5_AdditionalCode"),
		new Tuple<string, string>("RefCusVATApplicability", "ZX5_Description"),
		new Tuple<string, string>("RefExchangeRateZZ", "ZZN_PK"),
		new Tuple<string, string>("RefVesselZZ", "ZZO_PK"),
		new Tuple<string, string>("RefVesselZZ", "ZZO_RadioCallSign"),
		new Tuple<string, string>("RefVesselZZ", "ZZO_VesselType"),
		new Tuple<string, string>("RefVesselZZ", "ZZO_RN_NKCountryOfReg"),
		new Tuple<string, string>("RefVesselZZ", "ZZO_LloydsNumber"),
	};

	readonly List<Tuple<string, string>> nullableColumnTuples = new List<Tuple<string, string>>
	{
		new Tuple<string, string>("RefCusCodeListAttributeName", "ZXE_ZZK_NKCodeTypeForValueList"),
		new Tuple<string, string>("RefCusRateCode", "ZY1_ZZZ_NKDataGrouping"),
		new Tuple<string, string>("RefCusTariffUOM", "ZZ8_ZZA_NKTradeGroup"),
		new Tuple<string, string>("RefCusVATApplicability", "ZX5_ZZA_NKTradeGroup"),
		new Tuple<string, string>("RefCusApplicability", "ZZT_ZZA_ZZZ_NKSecondDataGrouping"),
		new Tuple<string, string>("DataChangeCapture", "DCC_OldValue"),
		new Tuple<string, string>("QRTZ_CRON_TRIGGERS", "TIME_ZONE_ID"),
		new Tuple<string, string>("RefCusCodeListAttributeNameLanguage", "ZXH_Name"),
		new Tuple<string, string>("RefStlFieldMapping", "SFM_Reference2"),
		new Tuple<string, string>("RefStlFieldMapping", "SFM_BillableCount"),
		new Tuple<string, string>("RefStlFieldMapping", "SFM_ServiceOccuredUTC"),
		new Tuple<string, string>("RefCusCondition", "ZX1_ZZS_NKPreference"),
		new Tuple<string, string>("RefCusConditionValue", "ZX3_ZX4_NKValueType"),
		new Tuple<string, string>("RefCusRate", "ZZ2_ZY1_ZZR_NKRateType"),
	};

	readonly List<Tuple<string, string>> notNullableColumnTuples = new List<Tuple<string, string>>
	{
		new Tuple<string, string>("RefCusRateCode", "ZY1_RateCode"),
		new Tuple<string, string>("RefCusRateCode", "ZY1_Description"),
		new Tuple<string, string>("RefCusRateType", "ZZR_Description"),
		new Tuple<string, string>("RefCusRuling", "ZZX_RN_NKCountryCode"),
		new Tuple<string, string>("RefApplicationAttributeTypeHistory", "RAT_Type"),
		new Tuple<string, string>("RefCarrierCode", "ZZ4_ZZZ_NKDataGrouping"),
		new Tuple<string, string>("NamedEntityClassification", "NEC_Language"),
		new Tuple<string, string>("RefLanguageText", "RLT_Language"),
		new Tuple<string, string>("QRTZ_CRON_TRIGGERS", "TRIGGER_NAME"),
		new Tuple<string, string>("SubscriptionEvent", "SSV_DataSetCode"),
		new Tuple<string, string>("QRTZ_TRIGGERS", "JOB_NAME"),
		new Tuple<string, string>("RefCusCodeListAttributeName", "ZXE_Description"),
		new Tuple<string, string>("RefCusConditionType", "ZX2_ConditionType"),
	};

	#endregion

	[Test]
	public void TestGetDefaultValue()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
		using var stagingRepository = new StagingRepository(TestConnectionString.GetAdmin(dbName));
		using var valuesProvider = new EntityValuesProvider(TestConnectionString.GetAdmin(dbName));

		foreach (var tuple in tableColumnTuples)
		{
			var defaultValue = valuesProvider.GetDefaultValue(tuple.Item1, tuple.Item2);
			Assert.IsNotNull(defaultValue);
		}
	}

	[Test]
	public void TestIsNullable()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
		using var stagingRepository = new StagingRepository(TestConnectionString.GetAdmin(dbName));
		using var valuesProvider = new EntityValuesProvider(TestConnectionString.GetAdmin(dbName));

		foreach (var tuple in nullableColumnTuples)
		{
			Assert.IsTrue(valuesProvider.IsNullable(tuple.Item1, tuple.Item2));
		}

		foreach (var tuple in notNullableColumnTuples)
		{
			Assert.IsFalse(valuesProvider.IsNullable(tuple.Item1, tuple.Item2));
		}
	}
}
