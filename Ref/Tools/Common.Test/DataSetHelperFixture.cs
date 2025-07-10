using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Tools.Common.Test;

[TestFixture]
[TransactionedTestCase]
[Property("DAT:CapabilityRequirements", "SQL2019+")]
public class DataSetHelperFixture
{
	[Test]
	public void GetAllTableAndColumns()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		using var connection = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		connection.Open();
		var dataSetHelper = new DataSetHelper(connection);
		var tableAndColumns = dataSetHelper.GetAllTableAndColumns();
		Assert.False(tableAndColumns.Keys.Any(k => k.EndsWith("View") || k.EndsWith("History")));
		using var command = connection.CreateCommand();
		var sql = @"select count(1) from sys.tables where name not like '%History'";
		command.CommandText = sql;
		var tableCount = (int)command.ExecuteScalar();
		Assert.AreEqual(tableCount, tableAndColumns.Keys.Count);

		var columnList = new List<string>();
		sql = @"select name from sys.columns where object_id=OBJECT_ID('RefCusTariff')";
		command.CommandText = sql;
		using (var reader = command.ExecuteReader())
		{
			while (reader.Read())
			{
				columnList.Add(reader.GetString(0));
			}
		}
		var tariffColumns = tableAndColumns["RefCusTariff"];
		Assert.AreEqual(columnList.Count, tariffColumns.Count);
		CollectionAssert.AreEquivalent(columnList, tariffColumns);
	}

	[Test]
	public void GetAllTableAndFKs()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		using var connection = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		connection.Open();
		var dataSetHelper = new DataSetHelper(connection);
		var tableAndFKs = dataSetHelper.GetAllTableAndFKs();
		Assert.GreaterOrEqual(tableAndFKs.Keys.Count, 72);
		Assert.False(tableAndFKs.ContainsKey("RefDataGrouping"));

		var tariffFKs = tableAndFKs["RefCusTariff"];
		Assert.AreEqual(2, tariffFKs.Count);
		tariffFKs = tariffFKs.OrderBy(x => x.ReferencedTable).ToList();
		Assert.AreEqual("RefCusTariffType", tariffFKs[0].ReferencedTable);
		Assert.AreEqual("ZZI_PK", tariffFKs[0].ReferencedColumn);
		Assert.AreEqual("ZZ1_ZZI_TariffType", tariffFKs[0].Column);
		Assert.AreEqual("uniqueidentifier", tariffFKs[0].DataType);

		var rateFKs = tableAndFKs["RefCusRate"];
		Assert.AreEqual(4, rateFKs.Count);
		rateFKs = rateFKs.OrderBy(x => x.ReferencedTable).ToList();
		Assert.AreEqual("RefCusPreference", rateFKs[0].ReferencedTable);
		Assert.AreEqual("ZZS_PK", rateFKs[0].ReferencedColumn);
		Assert.AreEqual("RefCusRateCode", rateFKs[1].ReferencedTable);
		Assert.AreEqual("ZY1_PK", rateFKs[1].ReferencedColumn);
		Assert.AreEqual("RefCusTariff", rateFKs[2].ReferencedTable);
		Assert.AreEqual("ZZ1_PK", rateFKs[2].ReferencedColumn);
		Assert.AreEqual("RefCusTariffNationalCode", rateFKs[3].ReferencedTable);
		Assert.AreEqual("ZZW_PK", rateFKs[3].ReferencedColumn);
	}

	[Test]
	public void GetPKAndTableCode()
	{
		var columnsList = new List<string>() { "ZZZ_PK", "ZZZ_DataGrouping", "ZZZ_Description", "ZZZ_ZZZ_Grouping" };
		var result = DataSetHelper.GetPKAndTableCode(columnsList);
		Assert.AreEqual("ZZZ_PK", result.Item1);
		Assert.AreEqual("ZZZ", result.Item2);
	}

	[Test]
	public void ContainDataSetPKAndCodeColumns()
	{
		var columnsList = new List<string>() { "ZZZ_PK", "ZZZ_DataGrouping", "ZZZ_Description", "ZZZ_ZZZ_Grouping" };
		Tuple<string, string> parentPKAndCodeColumns = null;
		var contain = DataSetHelper.ContainDataSetPKAndCodeColumns(columnsList, out parentPKAndCodeColumns);
		Assert.False(contain);
		Assert.Null(parentPKAndCodeColumns);

		columnsList = new List<string>() { "ZZZ_PK", "ZZZ_DataGrouping", "ZZZ_Description", "ZZZ_ZZZ_Grouping", "ZZZ_DataSetPK", "ZZZ_DataSetCode" };
		contain = DataSetHelper.ContainDataSetPKAndCodeColumns(columnsList, out parentPKAndCodeColumns);
		Assert.True(contain);
		Assert.AreEqual(Tuple.Create("ZZZ_DataSetPK", "ZZZ_DataSetCode"), parentPKAndCodeColumns);
	}

	[Test]
	public void ContainParentPKAndCodeColumns()
	{
		var columnsList = new List<string>() { "ZZZ_PK", "ZZZ_DataGrouping", "ZZZ_Description", "ZZZ_ZZZ_Grouping" };
		Tuple<string, string> parentPKAndCodeColumns = null;
		var contain = DataSetHelper.ContainParentPKAndCodeColumns(columnsList, out parentPKAndCodeColumns);
		Assert.False(contain);
		Assert.Null(parentPKAndCodeColumns);

		columnsList = new List<string>() { "ZZZ_PK", "ZZZ_DataGrouping", "ZZZ_Description", "ZZZ_ZZZ_Grouping", "ZZZ_ParentCode", "ZZZ_ParentPK" };
		contain = DataSetHelper.ContainParentPKAndCodeColumns(columnsList, out parentPKAndCodeColumns);
		Assert.True(contain);
		Assert.AreEqual(Tuple.Create("ZZZ_ParentPK", "ZZZ_ParentCode"), parentPKAndCodeColumns);

		columnsList = new List<string>() { "ZZZ_PK", "ZZZ_DataGrouping", "ZZZ_Description", "ZZZ_ZZZ_Grouping", "ZZZ_ParentId", "ZZZ_ParentTableCode" };
		contain = DataSetHelper.ContainParentPKAndCodeColumns(columnsList, out parentPKAndCodeColumns);
		Assert.True(contain);
		Assert.AreEqual(Tuple.Create("ZZZ_ParentId", "ZZZ_ParentTableCode"), parentPKAndCodeColumns);
	}

	public static Dictionary<string, List<string>> CreateTableAndColumns()
	{
		var tableAndColumns = new Dictionary<string, List<string>>();
		tableAndColumns["RefDataGrouping"] = new List<string>() { "ZZZ_PK", "ZZZ_DataGrouping", "ZZZ_Description", "ZZZ_ZZZ_Grouping" };
		tableAndColumns["RefLanguageType"] = new List<string>() { "ZX6_PK", "ZX6_Description", "ZX6_Language" };
		tableAndColumns["RefCusCodeType"] = new List<string>() { "ZZK_PK", "ZZK_CodeType", "ZZK_Description", "ZZK_IsReadonly", "ZZK_MaxLength", "ZZK_ZZZ_NKDataGrouping" };
		tableAndColumns["RefCusCodeTypeLanguage"] = new List<string>() { "ZXI_PK", "ZXI_Description", "ZXI_ZX6_NKLanguage", "ZXI_ZZK_CodeType" };
		tableAndColumns["RefCusTradeGroup"] = new List<string>() { "ZZA_PK", "ZZA_Description", "ZZA_EndDate", "ZZA_StartDate", "ZZA_TradeGroup", "ZZA_ZZZ_NKDataGrouping" };
		tableAndColumns["RefCusTradeGroupCountry"] = new List<string>() { "ZZB_PK", "ZZB_Description", "ZZB_EndDate", "ZZB_RN_NKTradeGroupCountryCode", "ZZB_StartDate", "ZZB_ZZA_TradeGroup" };
		tableAndColumns["RefCusTradeGroupLanguage"] = new List<string>() { "ZXD_PK", "ZXD_Description", "ZXD_ZX6_NKLanguage", "ZXD_ZZA_TradeGroup" };

		tableAndColumns["RefCusTaxOrFeeType"] = new List<string>() { "ZX0_PK", "ZX0_Description", "ZX0_TaxOrFeeType" };
		tableAndColumns["RefCusTaxOrFee"] = new List<string>() { "ZZF_PK", "ZZF_Code", "ZZF_Description", "ZZF_EndDate", "ZZF_Maximum", "ZZF_Minimum", "ZZF_StartDate", "ZZF_Threshold", "ZZF_Value", "ZZF_ZX0_NKTaxOrFeeType", "ZZF_ZZZ_NKDataGrouping" };
		tableAndColumns["RefCusTaxOrFeeLanguage"] = new List<string>() { "ZXU_PK", "ZXU_Description", "ZXU_ZX6_NKLanguage", "ZXU_ZZF_TaxOrFee", "ZXU_DataSetCode", "ZXU_DataSetPK" };

		tableAndColumns["RefCusPreference"] = new List<string>() { "ZZS_PK", "ZZS_Description", "ZZS_Preference", "ZZS_ZZZ_NKDataGrouping" };
		tableAndColumns["RefCusNomenclatureGroup"] = new List<string>() { "ZZ5_PK", "ZZ5_CompositeKey", "ZZ5_Description", "ZZ5_EndDate", "ZZ5_StartDate", "ZZ5_Value", "ZZ5_ZZ9_NKNomenclatureGroupType", "ZZ5_ZZZ_NKDataGrouping" };
		tableAndColumns["RefCusConditionValueType"] = new List<string>() { "ZX4_PK", "ZX4_Description", "ZX4_IsFormula", "ZX4_ValueType", "ZX4_ZZZ_NKDataGrouping" };
		tableAndColumns["RefCusTariff"] = new List<string>() { "ZZ1_PK", "ZZ1_CompositeKeyOnZZ5", "ZZ1_Description", "ZZ1_EndDate", "ZZ1_IAMUnique", "ZZ1_PublishedDate", "ZZ1_StartDate", "ZZ1_TariffCode", "ZZ1_ZZF_NKTaxOrFeeCode", "ZZ1_ZZI_TariffType", "ZZ1_ZZZ_NKDataGrouping" };
		tableAndColumns["RefCusTariffNationalCode"] = new List<string>() { "ZZW_PK", "ZZW_Description", "ZZW_EndDate", "ZZW_NationalCode", "ZZW_PublishedDate", "ZZW_StartDate", "ZZW_ZZ1_Tariff", "ZZW_ZZF_NKTaxOrFeeCode", "ZZW_ZZZ_NKDataGrouping" };
		tableAndColumns["RefCusRate"] = new List<string>() { "ZZ2_PK", "ZZ2_EndDate", "ZZ2_RateFormula", "ZZ2_RateFormulaDerivedFrom", "ZZ2_RX_NKCurrencyOverride", "ZZ2_StartDate", "ZZ2_ZY1_RateCode", "ZZ2_ZZ1_Tariff", "ZZ2_ZZS_Preference", "ZZ2_ZZW_TariffNationalCode", "ZZ2_ZZZ_NKDataGrouping", "ZZ2_DataSetCode", "ZZ2_DataSetPK" };
		tableAndColumns["RefCusCondition"] = new List<string>() { "ZX1_PK", "ZX1_Comment", "ZX1_ConditionValueTrueMeansStop", "ZX1_EndDate", "ZX1_IsExport", "ZX1_IsImport", "ZX1_LogicalANDWithinGroup", "ZX1_Source", "ZX1_StartDate", "ZX1_ZX2_ConditionType", "ZX1_ZZ1_Tariff", "ZX1_ZZ5_Nomenclature", "ZX1_ZZS_Preference", "ZX1_ZZZ_NKDataGrouping", "ZX1_DataSetCode", "ZX1_DataSetPK" };
		tableAndColumns["RefCusApplicability"] = new List<string>() { "ZZT_PK", "ZZT_AdditionalCode", "ZZT_EndDate", "ZZT_OrderNumber", "ZZT_StartDate", "ZZT_ZX1_Conditions", "ZZT_ZY2_AdditionalCode", "ZZT_ZZ2_Rate", "ZZT_ZZA_SecondTradeGroup", "ZZT_ZZA_TradeGroup", "ZZT_DataSetCode", "ZZT_DataSetPK" };
		tableAndColumns["RefCusTariffAdditionalCode"] = new List<string>() { "ZY2_PK", "ZY2_AdditionalCode", "ZY2_DataSetCode", "ZY2_DataSetPK", "ZY2_Description", "ZY2_IsMandatory", "ZY2_ZY3_NKCategory", "ZY2_ZZ1_Tariff", "ZY2_ZZW_NationalCode", "ZY2_ZZZ_NKDataGrouping" };
		tableAndColumns["RefCountry"] = new List<string>() { "RN_PK", "RN_AddressFormattingRule", "RN_Code", "RN_CountryDialingCode", "RN_Desc", "RN_EconomicGrouping", "RN_IsActive", "RN_IsoAlpha3Code", "RN_IsoNumericUNM49Code", "RN_PostcodeValidationRule", "RN_RX_NKAirWaybillCurrency", "RN_RX_NKLocalCurrency", "RN_StateProvinceValidationRule", "RN_ValidationStatus" };
		tableAndColumns["RefLanguageText"] = new List<string>() { "RLT_PK", "RLT_ParentId", "RLT_ParentTableCode", "RLT_Language", "RLT_ColumnName", "RLT_Text" };

		tableAndColumns["RefUNLOCO"] = new List<string>() { "RL_PK", "RL_Code", "RL_IsActive", "RL_PortName", "RL_NameWithDiacriticals", "RL_IATA", "RL_CoOrdinates", "RL_R3", "RL_RN_NKCountryCode", "RL_RW", "RL_IATARegionCode", "RL_GeoLocation" };
		tableAndColumns["RefLocoMap"] = new List<string>() { "RY_PK", "RY_LocalPortCode", "RY_RL_NKLocoPort", "RY_SystemUsage", "RY_RN_NKCountryCode" };
		tableAndColumns["RefUNLOCOUtcOffset"] = new List<string>() { "RLO_PK", "RLO_RL_NKCode", "RLO_StartTimeUtc", "RLO_EndTimeUtc", "RLO_OffsetMinutesFromUtc" };
		return tableAndColumns;
	}

	public static Dictionary<string, List<FKRelationship>> CreateTableAndFKs()
	{
		var tableAndFKs = new Dictionary<string, List<FKRelationship>>();
		tableAndFKs["RefCusCodeTypeLanguage"] = new List<FKRelationship>()
		{
			new FKRelationship("RefCusCodeTypeLanguage", "RefCusCodeType", "ZXI_ZZK_CodeType", "ZZK_PK", "uniqueidentifier"),
			new FKRelationship("RefCusCodeTypeLanguage", "RefLanguageType", "ZXI_ZX6_NKLanguage", "ZX6_Language", "varchar")
		};
		tableAndFKs["RefCusTradeGroup"] = new List<FKRelationship>() { new FKRelationship("RefCusTradeGroup", "RefDataGrouping", "ZZA_ZZZ_NKDataGrouping", "ZZZ_DataGrouping", "varchar") };
		tableAndFKs["RefCusTradeGroupCountry"] = new List<FKRelationship>() { new FKRelationship("RefCusTradeGroupCountry", "RefCusTradeGroup", "ZZB_ZZA_TradeGroup", "ZZA_PK", "uniqueidentifier") };
		tableAndFKs["RefCusTradeGroupLanguage"] = new List<FKRelationship>()
		{
			new FKRelationship("RefCusTradeGroupLanguage", "RefCusTradeGroup", "ZXD_ZZA_TradeGroup", "ZZA_PK", "uniqueidentifier"),
			new FKRelationship("RefCusTradeGroupLanguage", "RefLanguageType", "ZXD_ZX6_NKLanguage", "ZX6_Language", "varchar")
		};
		tableAndFKs["RefCusTaxOrFee"] = new List<FKRelationship>()
		{
			new FKRelationship("RefCusTaxOrFee", "RefDataGrouping", "ZZF_ZZZ_NKDataGrouping", "ZZZ_DataGrouping", "varchar"),
			new FKRelationship("RefCusTaxOrFee", "RefCusTaxOrFeeType", "ZZF_ZX0_NKTaxOrFeeType", "ZX0_TaxOrFeeType", "varchar")
		};
		tableAndFKs["RefCusTaxOrFeeLanguage"] = new List<FKRelationship>()
		{
			new FKRelationship("RefCusTaxOrFeeLanguage", "RefCusTaxOrFee", "ZXU_ZZF_TaxOrFee", "ZZF_PK", "uniqueidentifier"),
			new FKRelationship("RefCusTaxOrFeeLanguage", "RefLanguageType", "ZXU_ZX6_NKLanguage", "ZX6_Language", "varchar")
		};

		tableAndFKs["RefCusTariff"] = new List<FKRelationship>() { new FKRelationship("RefCusTariff", "RefCusTariffType", "ZZ1_ZZI_TariffType", "ZZI_PK", "uniqueidentifier") };
		tableAndFKs["RefCusTariffNationalCode"] = new List<FKRelationship>()
		{
			new FKRelationship("RefCusTariffNationalCode", "RefCusTariff", "ZZW_ZZ1_Tariff", "ZZ1_PK", "uniqueidentifier"),
			new FKRelationship("RefCusTariffNationalCode", "RefDataGrouping", "ZZW_ZZZ_NKDataGrouping", "ZZZ_DataGrouping", "varchar")
		};
		tableAndFKs["RefCusRate"] = new List<FKRelationship>()
		{
			new FKRelationship("RefCusRate", "RefCusPreference", "ZZ2_ZZS_Preference", "ZZS_PK", "uniqueidentifier"),
			new FKRelationship("RefCusRate", "RefCusTariffNationalCode", "ZZ2_ZZW_TariffNationalCode", "ZZW_PK", "uniqueidentifier"),
			new FKRelationship("RefCusRate", "RefCusRateCode", "ZZ2_ZY1_RateCode", "ZY1_PK", "uniqueidentifier"),
			new FKRelationship("RefCusRate", "RefCusTariff", "ZZ2_ZZ1_Tariff", "ZZ1_PK", "uniqueidentifier")
		};
		tableAndFKs["RefCusCondition"] = new List<FKRelationship>()
		{
			new FKRelationship("RefCusCondition", "RefDataGrouping", "ZX1_ZZZ_NKDataGrouping", "ZZZ_DataGrouping", "varchar"),
			new FKRelationship("RefCusCondition", "RefCusNomenclatureGroup", "ZX1_ZZ5_Nomenclature", "ZZ5_PK", "uniqueidentifier"),
			new FKRelationship("RefCusCondition", "RefCusConditionType", "ZX1_ZX2_ConditionType", "ZX2_PK", "uniqueidentifier"),
			new FKRelationship("RefCusCondition", "RefCusTariff", "ZX1_ZZ1_Tariff", "ZZ1_PK", "uniqueidentifier"),
			new FKRelationship("RefCusCondition", "RefCusPreference", "ZX1_ZZS_Preference", "ZZS_PK", "uniqueidentifier")
		};
		tableAndFKs["RefCusApplicability"] = new List<FKRelationship>()
		{
			new FKRelationship("RefCusApplicability", "RefCusCondition", "ZZT_ZX1_Conditions", "ZX1_PK", "uniqueidentifier"),
			new FKRelationship("RefCusApplicability", "RefCusTradeGroup", "ZZT_ZZA_TradeGroup", "ZZA_PK", "uniqueidentifier"),
			new FKRelationship("RefCusApplicability", "RefCusTradeGroup", "ZZT_ZZA_SecondTradeGroup", "ZZA_PK", "uniqueidentifier"),
			new FKRelationship("RefCusApplicability", "RefCusRate", "ZZT_ZZ2_Rate", "ZZ2_PK", "uniqueidentifier"),
			new FKRelationship("RefCusApplicability", "RefCusTariffAdditionalCode", "ZZT_ZY2_AdditionalCode", "ZY2_PK", "uniqueidentifier")
		};
		tableAndFKs["RefCusTariffAdditionalCode"] = new List<FKRelationship>()
		{
			new FKRelationship("RefCusTariffAdditionalCode", "RefCusTariff", "ZY2_ZZ1_Tariff", "ZZ1_PK", "uniqueidentifier"),
			new FKRelationship("RefCusTariffAdditionalCode", "RefCusTariffNationalCode", "ZY2_ZZW_NationalCode", "ZZW_PK", "uniqueidentifier"),
			new FKRelationship("RefCusTariffAdditionalCode", "RefCusTariffAdditionalCodeCategory", "ZY2_ZY3_NKCategory", "ZY3_Category", "char"),
			new FKRelationship("RefCusTariffAdditionalCode", "RefCusTariffAdditionalCodeCategory", "ZY2_ZZZ_NKDataGrouping", "ZY3_ZZZ_NKDataGrouping", "varchar")
		};
		tableAndFKs["RefLocoMap"] = new List<FKRelationship>()
		{
			new FKRelationship("RefLocoMap", "RefUNLOCO", "RY_RL_NKLocoPort", "RL_Code", "varchar")
		};
		tableAndFKs["RefUNLOCOUtcOffset"] = new List<FKRelationship>()
		{
			new FKRelationship("RefUNLOCOUtcOffset", "RefUNLOCO", "RLO_RL_NKCode", "RL_Code", "varchar")
		};

		return tableAndFKs;
	}

#pragma warning disable CA2211
	public static List<string[]> DataSetsList =
	[
		new[] { "RefDataGrouping" },
		new[] { "RefLanguageType" },
		new[] { "RefCusTradeGroup", "RefCusTradeGroupCountry", "RefCusTradeGroupLanguage" },
		new[] { "RefCusTaxOrFeeType", "RefCusTaxOrFee", "RefCusTaxOrFeeLanguage" },
		new[] { "RefCusPreference", "RefCusCondition" },
		new[] { "RefCusNomenclatureGroup", "RefCusCondition" },
		new[] { "RefCusConditionValueType" },
		new[]
		{
			"RefCusTariff", "RefCusTariffNationalCode", "RefCusRate", "RefCusCondition", "RefCusApplicability",
			"RefCusTariffAdditionalCode"
		},

		new[] { "RefCountry", "RefLanguageText" },
		new[] { "RefUNLOCO", "RefLocoMap", "RefUNLOCOUtcOffset" }

	];
#pragma warning restore CA2211
}
