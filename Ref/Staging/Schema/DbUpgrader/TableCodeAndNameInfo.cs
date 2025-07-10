using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public sealed class TableCodeAndNameInfo
	{
		public static List<(string TableCode, string TableName)> RootTables => new List<(string TableCode, string TableName)>
		{
			("ZZZ", "RefDataGrouping"),
			("ZX6", "RefLanguageType"),
			("ZZK", "RefCusCodeType"),
			("ZXE", "RefCusCodeListAttributeName"),
			("ZZR", "RefCusRateType"),
			("ZZN", "RefExchangeRateZZ"),
			("ZZ6", "RefCusProcedure"),
			("ZX0", "RefCusTaxOrFeeType"),
			("ZZ9", "RefCusNomenclatureGroupType"),
			("ZZI", "RefCusTariffType"),
			("ZZD", "RefCusCodeList"),
			("ZZ5", "RefCusNomenclatureGroup"),
			("ZZA", "RefCusTradeGroup"),
			("ZZ4", "RefCarrierCode"),
			("ZZP", "RefCusMapType"),
			("ZZM", "RefCusMap"),
			("DG",  "UNDGSubstance"),
			("RID", "UNDGSubstanceRID"),
			("ADN", "UNDGSubstanceADN"),
			("JTT", "UNDGSubstanceJTT"),
			("DC",  "UNDGCommonData"),
			("ADR", "UNDGSubstanceADR"),
			("CFR", "UNDGSubstanceCFR"),
			("ZZS", "RefCusPreference"),
			("ZX4", "RefCusConditionValueType"),
			("RV",  "RefVessel"),
			("ZZO", "RefVesselZZ"),
			("ZZ1", "RefCusTariff"),
			("ZY3", "RefCusTariffAdditionalCodeCategory"),
			("ZX2", "RefCusConditionType"),
			("RW",  "RefCountryStates"),
			("ZAT", "RefAccTaxRate"),
			("R3",  "RefTimeZoneSet"),
			("RL",  "RefUNLOCO"),
			("RLM", "RefUNLOCOPortMapping"),
			("RN",  "RefCountry"),
			("ZZX", "RefCusRuling"),
			("RX",  "RefCurrency"),
			("ZXF", "RefHarbourRate"),
			("ZY5", "RefCusAUNexdocECMCode"),
			("RSL", "RefShippingLine"),
			("RST", "RefShippingLineMessagingRequirementType"),
			("DOC", "RefDocOrgCusCode"),
			("ZRT", "RefSysConfigType"),
			("RCL", "RefComplianceList"),
			("RM",  "RefAirline"),
			("RPP", "RefPortPolygon"),
			("DCR", "UNDGCountryReference"),
			("RAC", "RefAirlineCommodityCode"),
			("RAR", "RefAirlineProductCode"),
			("STL", "RefStlScript"),
			("ZZJ", "RefCusConfiguration"),
			("ZXQ", "RefCusQuota"),
			("RFT", "RefFacility"),
			("SFM", "RefStlFieldMapping")
		};

		public static List<(string TableCode, string TableName, string DatasetNameOrParentPK)> TablesWithParentFKOrDataSetPK => new List<(string TableCode, string TableName, string DatasetNameOrParentPK)>
		{
			("ZXI", "RefCusCodeTypeLanguage",                   "ZZK_CodeType"),
			("ZXH", "RefCusCodeListAttributeNameLanguage",      "ZXE_CodeListAttributeName"),
			("ZY1", "RefCusRateCode",                           "ZZR_RateType"),
			("ZXT", "RefCusRateTypeLanguage",                   "ZZR_RateType"),
			("ZXB", "RefCusProcedureAttribute",                 "ZZ6_ProcedureCode"),
			("ZXV", "RefCusProcedureLanguage",                  "ZZ6_Procedure"),
			("ZXK", "RefCusTariffTypeLanguage",                 "ZZI_TariffType"),
			("ZZE", "RefCusCodeListAttribute",                  "ZZD_CodeList"),
			("ZXA", "RefCusCodeListLanguage",                   "ZZD_CodeList"),
			("ZZB", "RefCusTradeGroupCountry",                  "ZZA_TradeGroup"),
			("ZXD", "RefCusTradeGroupLanguage",                 "ZZA_TradeGroup"),
			("ZZG", "RefCarrierCodeAttribute",                  "ZZ4_CarrierCode"),
			("ZZQ", "RefCarrierVesselPivot",                    "ZZ4"),
			("ZCL", "RefCarrierCodeLanguage",                   "ZZ4_CarrierCode"),
			("DA",  "UNDGAttribute",                            "DG"),
			("DR",  "UNDGReference",                            "DG"),
			("ZX9", "RefCusPreferenceLanguage",                 "ZZS_Preference"),
			("ZX7", "RefCusTariffLanguage",                     "ZZ1_Tariff"),
			("ZZW", "RefCusTariffNationalCode",                 "ZZ1_Tariff"),
			("ZZH", "RefCusTariffRelationship",                 "ZZ1_Tariff"),
			("ZXW", "RefCusConditionTypeLanguage",              "ZX2_ConditionType"),
			("RSR", "RefShippingLineMessagingRequirement",      "RSL_ShippingLine"),
			("DCP", "UNDGCountryReferencePivot",                "DCR"),
			("RPC", "RefAirlineProductCodeCommodityCodePivot",  "RAR"),
			("RFL", "RefFacilityLocalCode",                     "RFT_NKFacilityCode"),
			("ZZY", "RefCusRulingConfig",                       "ZZX_CusRuling"),
			("ZXX", "RefCusConditionValueTypeLanguage",         "ZX4_ValueType"),
			("ZZL", "RefCusNomenclatureGroupNote",              "ZZ5_NomenclatureGroup"),
			("ZX8", "RefCusNomenclatureLanguage",               "ZZ5_NomenclatureGroup"),
			("DAZ", "UNDGAttributeZZ",                          "ParentPK"),
			("RLT", "RefLanguageText",                          "ParentId"),
			("ST",  "StmNote",                                  "ParentId"),
			("R2",  "RefTimeZone",                              "R3_TimeZoneSet"),
			("ZXC", "RefCusRateCodeLanguage",                   ""),
			("ZXU", "RefCusTaxOrFeeLanguage",                   ""),
			("ZB1", "RefCusTariffBRCharacteristic",             ""),
			("ZZ3", "RefCusTariffAttribute",                    ""),
			("ZZ8", "RefCusTariffUOM",                          ""),
			("ZZ2", "RefCusRate",                               ""),
			("ZXG", "RefCusRateUOM",                            ""),
			("ZX3", "RefCusConditionValue",                     ""),
			("ZXJ", "RefCusConditionLanguage",                  ""),
			("ZY2", "RefCusTariffAdditionalCode",               ""),
			("ZZT", "RefCusApplicability",                      ""),
			("ZZC", "RefCusExcludedTradeGroup",                 ""),
			("ZX5", "RefCusVATApplicability",                   ""),
			("ZY4", "RefCusTariffAdditionalCodeLanguage",       ""),
			("ZB3", "RefCusTariffBRCharacteristicAttribute",    ""),
			("ZB2", "RefCusTariffBRCharacteristicValue",        ""),
			("R4",  "RefTimeZoneRule",                          ""),
			("ZZU", "RefCusCodeOrAttributeTransportMode",       ""),
			("ZX1", "RefCusCondition",                          "")
		};

		public static List<(string TableCode, string TableName, string ParentTableCode, string ParentTableName, string MatchCondition)> TablesWithMatchValue => new List<(string TableCode, string TableName, string ParentTableCode, string ParentTableName, string MatchCondition)>
		{
			("ZZF", "RefCusTaxOrFee",       "ZX0",  "RefCusTaxOrFeeType",   "ZZF_ZX0_NKTaxOrFeeType = ZX0_TaxOrFeeType"),
			("RLO", "RefUNLOCOUtcOffset",   "RL",   "RefUNLOCO",            "RLO_RL_NKCode = RL_Code"),
			("ZRC", "RefSysConfig",         "ZRT",  "RefSysConfigType",     "ZRC_ZRT_NKConfigCode = ZRT_ConfigCode"),
			("RY",  "RefLocoMap",           "RL",   "RefUNLOCO",            "RY_RL_NKLocoPort = RL_Code")
		};

		public static Dictionary<string, string> CodeToNameDictionary()
		{
			Dictionary<string, string> codeToNameDict = new Dictionary<string, string>();
			RootTables.ForEach(t => { codeToNameDict[t.TableCode] = t.TableName; });
			TablesWithParentFKOrDataSetPK.ForEach(t => { codeToNameDict[t.TableCode] = t.TableName; });
			TablesWithMatchValue.ForEach(t => { codeToNameDict[t.TableCode] = t.TableName; });
			return codeToNameDict;
		}
	}
}
