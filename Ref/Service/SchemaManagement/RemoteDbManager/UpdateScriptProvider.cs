using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public class UpdateScriptProvider : IUpdateScriptProvider
	{
#if DEBUG
		public
#endif
		static int SRDbLatestVersion => new UpgradeScriptProvider().LatestVersion;

		public IEnumerable<IUpdaterScriptInfo> GetScriptInfos(int dbVersion)
		{
			return GetScripts(UpdaterScriptInfos, dbVersion);
		}

		public static IEnumerable<IUpdaterScriptInfo> GetScripts(IEnumerable<(IUpdaterScriptInfo, int)> scriptList, int version)
		{
			return scriptList.Where(x => x.Item2 >= version)
				.GroupBy(x => x.Item1.DataSetName, x => x)
				.Select(x => x.OrderBy(y => y.Item2).FirstOrDefault())
				.Select(x => x.Item1);
		}

#if DEBUG
		public
#endif
		IEnumerable<(IUpdaterScriptInfo, int)> UpdaterScriptInfos
		{
			get
			{
				if (scriptInfos == null)
				{
					// Please add lines in alphabetical order
					scriptInfos = new[] {
						((IUpdaterScriptInfo)new RefAccTaxRateUpdaterInfo_1(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefAirlineCommodityCodeUpdaterInfo_1(), 336),
						((IUpdaterScriptInfo)new RefAirlineCommodityCodeUpdaterInfo_2(), 359),
						((IUpdaterScriptInfo)new RefAirlineCommodityCodeUpdaterInfo_3(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefAirlineProductCodeUpdaterInfo_1(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCarrierCodeUpdaterInfo_1(), 345),
						((IUpdaterScriptInfo)new RefCarrierCodeUpdaterInfo_2(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusAUNexdocECMCodeUpdaterInfo_1(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusCodeListAttributeNameUpdaterInfo_1(), 468),
						((IUpdaterScriptInfo)new RefCusCodeListAttributeNameUpdaterInfo_2(), 516),
						((IUpdaterScriptInfo)new RefCusCodeListAttributeNameUpdaterInfo_3(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusCodeListUpdaterInfo_1(), 516),
						((IUpdaterScriptInfo)new RefCusCodeListUpdaterInfo_2(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusCodeTypeUpdaterInfo_1(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusConditionTypeUpdaterInfo_1(), 334),
						((IUpdaterScriptInfo)new RefCusConditionTypeUpdaterInfo_2(), 357),
						((IUpdaterScriptInfo)new RefCusConditionTypeUpdaterInfo_3(), 471),
						((IUpdaterScriptInfo)new RefCusConditionTypeUpdaterInfo_4(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusConditionValueTypeUpdaterInfo_1(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusConfigurationUpdaterInfo_1(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusMapTypeUpdaterInfo_1(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusMapUpdaterInfo_1(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusNomenclatureGroupTypeUpdaterInfo_1(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusNomenclatureGroupUpdaterInfo_1(), 331),
						((IUpdaterScriptInfo)new RefCusNomenclatureGroupUpdaterInfo_2(), 334),
						((IUpdaterScriptInfo)new RefCusNomenclatureGroupUpdaterInfo_3(), 366),
						((IUpdaterScriptInfo)new RefCusNomenclatureGroupUpdaterInfo_4(), 451),
						((IUpdaterScriptInfo)new RefCusNomenclatureGroupUpdaterInfo_5(), 521),
						((IUpdaterScriptInfo)new RefCusNomenclatureGroupUpdaterInfo_6(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusPreferenceUpdaterInfo_1(), 331),
						((IUpdaterScriptInfo)new RefCusPreferenceUpdaterInfo_2(), 334),
						((IUpdaterScriptInfo)new RefCusPreferenceUpdaterInfo_3(), 451),
						((IUpdaterScriptInfo)new RefCusPreferenceUpdaterInfo_4(), 521),
						((IUpdaterScriptInfo)new RefCusPreferenceUpdaterInfo_5(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusProcedureUpdaterInfo_1(), 432),
						((IUpdaterScriptInfo)new RefCusProcedureUpdaterInfo_2(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusQuotaUpdaterInfo_1(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusRateTypeUpdaterInfo_1(), 357),
						((IUpdaterScriptInfo)new RefCusRateTypeUpdaterInfo_2(), 394),
						((IUpdaterScriptInfo)new RefCusRateTypeUpdaterInfo_3(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusRulingUpdaterInfo_1(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusTariffAdditionalCodeCategoryUpdaterInfo_1(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusTariffTypeUpdaterInfo_1(), 334),
						((IUpdaterScriptInfo)new RefCusTariffTypeUpdaterInfo_2(), 354),
						((IUpdaterScriptInfo)new RefCusTariffTypeUpdaterInfo_3(), 448),
						((IUpdaterScriptInfo)new RefCusTariffTypeUpdaterInfo_4(), 499),
						((IUpdaterScriptInfo)new RefCusTariffTypeUpdaterInfo_5(), 513),
						((IUpdaterScriptInfo)new RefCusTariffTypeUpdaterInfo_6(), 514),
						((IUpdaterScriptInfo)new RefCusTariffTypeUpdaterInfo_7(), 544),
						((IUpdaterScriptInfo)new RefCusTariffTypeUpdaterInfo_8(), 569),
						((IUpdaterScriptInfo)new RefCusTariffTypeUpdaterInfo_9(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusTariffUpdaterInfo_1(), 331),
						((IUpdaterScriptInfo)new RefCusTariffUpdaterInfo_2(), 334),
						((IUpdaterScriptInfo)new RefCusTariffUpdaterInfo_3(), 337),
						((IUpdaterScriptInfo)new RefCusTariffUpdaterInfo_4(), 354),
						((IUpdaterScriptInfo)new RefCusTariffUpdaterInfo_5(), 366),
						((IUpdaterScriptInfo)new RefCusTariffUpdaterInfo_6(), 411),
						((IUpdaterScriptInfo)new RefCusTariffUpdaterInfo_7(), 424),
						((IUpdaterScriptInfo)new RefCusTariffUpdaterInfo_8(), 451),
						((IUpdaterScriptInfo)new RefCusTariffUpdaterInfo_9(), 459),
						((IUpdaterScriptInfo)new RefCusTariffUpdaterInfo_10(), 521),
						((IUpdaterScriptInfo)new RefCusTariffUpdaterInfo_11(), 528),
						((IUpdaterScriptInfo)new RefCusTariffUpdaterInfo_12(), 538),
						((IUpdaterScriptInfo)new RefCusTariffUpdaterInfo_13(), 569),
						((IUpdaterScriptInfo)new RefCusTariffUpdaterInfo_14(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusTaxOrFeeTypeUpdaterInfo_1(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusTradeGroupUpdaterInfo_1(), 331),
						((IUpdaterScriptInfo)new RefCusTradeGroupUpdaterInfo_3(), 528),
						((IUpdaterScriptInfo)new RefCusTradeGroupUpdaterInfo_4(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefDataGroupingUpdaterInfo_1(), 334),
						((IUpdaterScriptInfo)new RefDataGroupingUpdaterInfo_2(), 335),
						((IUpdaterScriptInfo)new RefDataGroupingUpdaterInfo_3(), 345),
						((IUpdaterScriptInfo)new RefDataGroupingUpdaterInfo_4(), 357),
						((IUpdaterScriptInfo)new RefDataGroupingUpdaterInfo_5(), 448),
						((IUpdaterScriptInfo)new RefDataGroupingUpdaterInfo_6(), 451),
						((IUpdaterScriptInfo)new RefDataGroupingUpdaterInfo_7(), 499),
						((IUpdaterScriptInfo)new RefDataGroupingUpdaterInfo_8(), 513),
						((IUpdaterScriptInfo)new RefDataGroupingUpdaterInfo_9(), 514),
						((IUpdaterScriptInfo)new RefDataGroupingUpdaterInfo_10(), 528),
						((IUpdaterScriptInfo)new RefDataGroupingUpdaterInfo_11(), 561),
						((IUpdaterScriptInfo)new RefDataGroupingUpdaterInfo_12(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefDocOrgCusCodeUpdaterInfo_1(), 323),
						((IUpdaterScriptInfo)new RefDocOrgCusCodeUpdaterInfo_2(), 474),
						((IUpdaterScriptInfo)new RefDocOrgCusCodeUpdaterInfo_3(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefExchangeRateZZUpdaterInfo_1(), 330),
						((IUpdaterScriptInfo)new RefExchangeRateZZUpdaterInfo_2(), 473),
						((IUpdaterScriptInfo)new RefExchangeRateZZUpdaterInfo_3(), 502),
						((IUpdaterScriptInfo)new RefExchangeRateZZUpdaterInfo_4(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefHarbourRateUpdaterInfo_1(), 374),
						((IUpdaterScriptInfo)new RefHarbourRateUpdaterInfo_2(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefLanguageTypeUpdaterInfo_1(), 334),
						((IUpdaterScriptInfo)new RefLanguageTypeUpdaterInfo_2(), 345),
						((IUpdaterScriptInfo)new RefLanguageTypeUpdaterInfo_3(), 451),
						((IUpdaterScriptInfo)new RefLanguageTypeUpdaterInfo_4(), 514),
						((IUpdaterScriptInfo)new RefLanguageTypeUpdaterInfo_5(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefStlFieldMappingUpdaterInfo_1(), 350),
						((IUpdaterScriptInfo)new RefStlFieldMappingUpdaterInfo_2(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefStlScriptUpdaterInfo_1(), 327),
						((IUpdaterScriptInfo)new RefStlScriptUpdaterInfo_2(), 330),
						((IUpdaterScriptInfo)new RefStlScriptUpdaterInfo_3(), 333),
						((IUpdaterScriptInfo)new RefStlScriptUpdaterInfo_4(), 334),
						((IUpdaterScriptInfo)new RefStlScriptUpdaterInfo_5(), 414),
						((IUpdaterScriptInfo)new RefStlScriptUpdaterInfo_6(), 426),
						((IUpdaterScriptInfo)new RefStlScriptUpdaterInfo_7(), 550),
						((IUpdaterScriptInfo)new RefStlScriptUpdaterInfo_8(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefSysConfigTypeUpdaterInfo_1(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefUNLOCOUpdaterInfo_1(), 429),
						((IUpdaterScriptInfo)new RefUNLOCOUpdaterInfo_2(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefVesselZZUpdaterInfo_1(), 561),
						((IUpdaterScriptInfo)new RefVesselZZUpdaterInfo_2(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new UNDGSubstanceADNUpdaterInfo_1(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new UNDGSubstanceADRUpdaterInfo_1(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new UNDGSubstanceCFRUpdaterInfo_1(), 434),
						((IUpdaterScriptInfo)new UNDGSubstanceCFRUpdaterInfo_2(), 466),
						((IUpdaterScriptInfo)new UNDGSubstanceCFRUpdaterInfo_3(), 470),
						((IUpdaterScriptInfo)new UNDGSubstanceCFRUpdaterInfo_4(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new UNDGSubstanceJTTUpdaterInfo_1(), 492),
						((IUpdaterScriptInfo)new UNDGSubstanceJTTUpdaterInfo_2(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new UNDGSubstanceRIDUpdaterInfo_1(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusTariffAttributeNameUpdaterInfo_1(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusConditionCodeUpdaterInfo_1(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefMessagingBussPackageInfoUpdaterInfo_1(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusProfileTypeUpdaterInfo_1(), 513),
						((IUpdaterScriptInfo)new RefCusProfileTypeUpdaterInfo_2(), 514),
						((IUpdaterScriptInfo)new RefCusProfileTypeUpdaterInfo_3(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusProfileUpdaterInfo_1(), 549),
						((IUpdaterScriptInfo)new RefCusProfileUpdaterInfo_2(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusProfileQuestionUpdaterInfo_1(), 572),
						((IUpdaterScriptInfo)new RefCusProfileQuestionUpdaterInfo_2(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefCusProfileQuestionPathwayUpdaterInfo_1(), 549),
						((IUpdaterScriptInfo)new RefCusProfileQuestionPathwayUpdaterInfo_2(), 572),
						((IUpdaterScriptInfo)new RefCusProfileQuestionPathwayUpdaterInfo_3(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefAccElectronicProcessingFeeUpdaterInfo_1(), 522),
						((IUpdaterScriptInfo)new RefAccElectronicProcessingFeeUpdaterInfo_2(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefGlbReleaseNoteUpdaterInfo_1(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new UNDGVersionUpdaterInfo_1(), SRDbLatestVersion),
						((IUpdaterScriptInfo)new RefAccessorialUpdaterInfo_1(), SRDbLatestVersion),
					};
				}
				return scriptInfos;
			}
		}
		(IUpdaterScriptInfo, int)[] scriptInfos;
	}
}
