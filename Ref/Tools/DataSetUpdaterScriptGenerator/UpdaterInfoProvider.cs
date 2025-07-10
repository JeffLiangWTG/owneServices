using System.Collections.Generic;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public static class UpdaterInfoProvider
	{
		public static IEnumerable<(IDataSetUpdaterInfo, int)> GetUpdaterInfos
		{
			get
			{
				yield return (new RefDataGroupingUpdaterInfo<IRefDataGrouping>(), 12);
				yield return (new OneTableDataSetUpdaterInfo<IRefAccTaxRate>(false), 1);
				yield return (new OneTableDataSetUpdaterInfo<IRefCusMapType>(false), 1);
				yield return (new OneTableDataSetUpdaterInfo<IRefCusMap>(false), 1);
				yield return (new OneTableDataSetUpdaterInfo<IRefExchangeRateZZ>(true), 4);
				yield return (new TwoTableDataSetUpdaterInfo<IRefVesselZZ, IRefVesselArrival>(true), 2);
				yield return (new OneTableDataSetUpdaterInfo<IRefHarbourRate>(true), 2);
				yield return (new OneTableDataSetUpdaterInfo<IRefCusAUNexdocECMCode>(true), 1);
				yield return (new OneTableDataSetUpdaterInfo<IRefDocOrgCusCode>(true), 3);
				yield return (new OneTableDataSetUpdaterInfo<IRefAirlineCommodityCode>(false), 3);
				yield return (new OneTableDataSetUpdaterInfo<IRefStlScript>(false), 8);
				yield return (new OneTableDataSetUpdaterInfo<IRefCusNomenclatureGroupType>(false), 1);
				yield return (new OneTableDataSetUpdaterInfo<IRefLanguageType>(false), 5);
				yield return (new OneTableDataSetUpdaterInfo<IRefCusTariffAdditionalCodeCategory>(false), 1);
				yield return (new TwoTableDataSetUpdaterInfo<IRefSysConfigType, IRefSysConfig>(true), 1);
				yield return (new TwoTableDataSetUpdaterInfo<IRefCusTradeGroup, IRefCusTradeGroupCountry>(true), 4);
				yield return (new TwoTableDataSetUpdaterInfo<IRefCusCodeType, IRefCusCodeTypeLanguage>(true), 1);
				yield return (new TwoTableDataSetUpdaterInfo<IRefCusConditionType, IRefCusConditionTypeLanguage>(false), 4);
				yield return (new TwoTableDataSetUpdaterInfo<IRefCusConditionValueType, IRefCusConditionValueTypeLanguage>(false), 1);
				yield return (new TwoTableDataSetUpdaterInfo<IUNDGSubstanceADR, IUNDGAttributeZZ>(true), 1);
				yield return (new TwoTableDataSetUpdaterInfo<IUNDGSubstanceRID, IUNDGAttributeZZ>(true), 1);
				yield return (new TwoTableDataSetUpdaterInfo<IUNDGSubstanceADN, IUNDGAttributeZZ>(true), 1);
				yield return (new TwoTableDataSetUpdaterInfo<IUNDGSubstanceJTT, IUNDGAttributeZZ>(true), 2);
				yield return (new TwoTableDataSetUpdaterInfo<IUNDGSubstanceCFR, IUNDGAttributeZZ>(true), 4);
				yield return (new TwoTableDataSetUpdaterInfo<IRefCusTariffType, IRefCusTariffTypeLanguage>(false), 9);
				yield return (new RefAirlineProduceCodeUpdaterInfo<IRefAirlineProductCode>(), 1);
				yield return (new RefCarrierCodeUpdaterInfo<IRefCarrierCode>(), 2);
				yield return (new RefCusCodeListAttributeNameUpdaterInfo(), 3);
				yield return (new RefCusCodeListUpdaterInfo<IRefCusCodeList>(), 2);
				yield return (new RefCusNomenclatureGroupUpdaterInfo<IRefCusNomenclatureGroup>(), 6);
				yield return (new RefCusPreferenceUpdaterInfo<IRefCusPreference>(), 5);
				yield return (new RefCusProcedureUpdaterInfo<IRefCusProcedure>(), 2);
				yield return (new RefCusRateTypeUpdaterInfo<IRefCusRateType>(), 3);
				yield return (new RefCusRulingUpdaterInfo<IRefCusRuling>(), 1);
				yield return (new RefCusTariffUpdaterInfo<IRefCusTariff>(), 14);
				yield return (new RefCusTaxOrFeeTypeUpdaterInfo<IRefCusTaxOrFeeType>(), 1);
				yield return (new OneTableDataSetUpdaterInfo<IRefCusConfiguration>(false), 1);
				yield return (new OneTableDataSetUpdaterInfo<IRefCusQuota>(false), 1);
				yield return (new OneTableDataSetUpdaterInfo<IRefStlFieldMapping>(false), 2);
				yield return (new RefUNLOCOUpdaterInfo(), 2);
				yield return (new OneTableDataSetUpdaterInfo<IRefCusTariffAttributeName>(false), 1);
				yield return (new TwoTableDataSetUpdaterInfo<IRefCusConditionCode, IRefCusConditionCodeLanguage>(false), 1);
				yield return (new RefMessagingBussPackageInfoUpdaterInfo<IRefMessagingBussPackageInfo>(), 1);
				yield return (new RefCusProfileTypeUpdaterInfo<IRefCusProfileType>(), 3);
				yield return (new RefCusProfileUpdaterInfo<IRefCusProfile>(), 2);
				yield return (new RefCusProfileQuestionUpdaterInfo<IRefCusProfileQuestion>(), 2);
				yield return (new RefCusProfileQuestionPathwayUpdaterInfo<IRefCusProfileQuestionPathway>(), 3);
				yield return (new OneTableDataSetUpdaterInfo<IRefAccElectronicProcessingFee>(true), 2);
				yield return (new OneTableDataSetUpdaterInfo<IRefGlbReleaseNote>(false), 1);
				yield return (new OneTableDataSetUpdaterInfo<IUNDGVersion>(false), 1);
				yield return (new OneTableDataSetUpdaterInfo<IRefAccessorial>(true), 1);
			}
		}
	}
}
