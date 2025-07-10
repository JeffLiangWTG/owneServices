using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public static class UniversalReferenceDataHelper
	{
		public static ZString GetRateTypeDescription(BusinessObjectFactory factory, ZString dataGrouping, ZString rateType)
		{
			ZString result = ZString.Empty;
			if (!dataGrouping.IsEmpty && !rateType.IsEmpty)
			{
				result = factory.GetCachedValue(string.Join("_", "RateTypeDescription", dataGrouping, rateType), () =>
				{
					var cusRateType = RefCusRateType.Loader.Load(factory, dataGrouping, rateType);
					if (cusRateType == null)
					{
						var parentDataGroupingCode = RefDataGrouping.GetParentDataGroupingCode(factory, dataGrouping);
						if (!parentDataGroupingCode.IsEmpty)
						{
							cusRateType = RefCusRateType.Loader.Load(factory, parentDataGroupingCode, rateType);
						}
					}
					return cusRateType?.ZZR_Description ?? ZString.Empty;
				});
			}

			return result;
		}

		public static ZString GetConditionTypeDescription(BusinessObjectFactory factory, ZString dataGrouping, ZString conditionType)
		{
			ZString result = ZString.Empty;
			if (!dataGrouping.IsEmpty && !conditionType.IsEmpty)
			{
				result = factory.GetCachedValue(string.Join("_", "ConditionTypeDescription", dataGrouping, conditionType), () =>
				{
					var query = new ZQuery(RefCusConditionTypeSchema.ZX2_ConditionType, conditionType);
					query.AddToFilter(RefCusConditionTypeSchema.ZX2_ZZZ_NKDataGrouping, dataGrouping);
					var cusConditionType = factory.Load<RefCusConditionType>(query).FirstOrDefault();
					if (cusConditionType == null)
					{
						var parentDataGroupingCode = RefDataGrouping.GetParentDataGroupingCode(factory, dataGrouping);
						if (!parentDataGroupingCode.IsEmpty)
						{
							var newQuery = new ZQuery(RefCusConditionTypeSchema.ZX2_ConditionType, conditionType);
							newQuery.AddToFilter(RefCusConditionTypeSchema.ZX2_ZZZ_NKDataGrouping, parentDataGroupingCode);
							cusConditionType = factory.Load<RefCusConditionType>(newQuery).FirstOrDefault();
						}
					}
					return cusConditionType?.ZX2_Description ?? ZString.Empty;
				});
			}
			return result;
		}

		public static ZString GetTariffAdditionalCodeCategoryDescription(BusinessObjectFactory factory, ZString dataGrouping, ZString category)
		{
			ZString result = ZString.Empty;
			if (!dataGrouping.IsEmpty && !category.IsEmpty)
			{
				result = factory.GetCachedValue(string.Join("_", "TariffAdditionalCodeCategoryDescription", dataGrouping, category), () =>
				{
					var query = new ZQuery(RefCusTariffAdditionalCodeCategorySchema.ZY3_Category, category);
					query.AddToFilter(RefCusTariffAdditionalCodeCategorySchema.ZY3_ZZZ_NKDataGrouping, dataGrouping);
					var tariffAdditionalCodeCategory = factory.Load<RefCusTariffAdditionalCodeCategory>(query).FirstOrDefault();
					if (tariffAdditionalCodeCategory == null)
					{
						var parentDataGroupingCode = RefDataGrouping.GetParentDataGroupingCode(factory, dataGrouping);
						if (!parentDataGroupingCode.IsEmpty)
						{
							var newQuery = new ZQuery(RefCusTariffAdditionalCodeCategorySchema.ZY3_Category, category);
							newQuery.AddToFilter(RefCusTariffAdditionalCodeCategorySchema.ZY3_ZZZ_NKDataGrouping, parentDataGroupingCode);
							tariffAdditionalCodeCategory = factory.Load<RefCusTariffAdditionalCodeCategory>(newQuery).FirstOrDefault();
						}
					}
					return tariffAdditionalCodeCategory?.ZY3_Description ?? ZString.Empty;
				});
			}
			return result;
		}

		public static CodeDescriptionPairList GetPreferenceList(BusinessObjectFactory factory, bool useUniversalTariff, ZString tariff, ZString countryOfOrigin, TariffView universalTariff, IZZRateSelectionCriteria criteria, ZString dataGrouping)
		{
			CodeDescriptionPairList result;
			if (useUniversalTariff && !tariff.IsEmpty && !countryOfOrigin.IsEmpty)
			{
				result = GetDynamicPreferenceList(universalTariff, criteria);
			}
			else
			{
				result = GetPreferenceListByCountry(factory, dataGrouping);
			}

			return result;
		}

		public static CodeDescriptionPairList GetDynamicPreferenceList(TariffView tariff, params IZZRateSelectionCriteria[] criterias)
										=> GetDynamicPreferenceList(tariff, (IEnumerable<IZZRateSelectionCriteria>)criterias);

		public static CodeDescriptionPairList GetDynamicPreferenceList(TariffView tariff, IEnumerable<IZZRateSelectionCriteria> criterias)
		{
			var result = new CodeDescriptionPairList();
			if (tariff != null && criterias != null)
			{
				criterias = criterias.Where(c => !c.TradeGroupCountry.IsEmpty && !c.EffectiveDate.IsEmpty);
				if (criterias.Any())
				{
					var criteriaKeys = criterias.GetRateSelectionCacheKeys(includePrimaryPreference: false);
					var key = string.Join("_", "DynamicPreferenceList", tariff.PK, criteriaKeys);

					result = tariff.Factory.GetCachedValue(key, () => GetDynamicPreferenceListCore(tariff, criterias), CacheStalenessPolicy.StaleOnFactorySave);
				}
			}
			return result;
		}

		static CodeDescriptionPairList GetDynamicPreferenceListCore(TariffView tariff, IEnumerable<IZZRateSelectionCriteria> criterias)
		{
			var result = new CodeDescriptionPairList();

			var dynamicPreferenceList = GetRateSelectionCriteriaInfo(tariff, criterias)
											.Where(x => !x.ZZS_Preference.IsEmpty && criterias.Any(c => x.MatchExcludingPrimaryPreference(c)))
											.Select(x => new { x.ZZS_Preference, x.ZZS_Description, x.TranslatedPreferenceDescription })
											.Distinct()
											.OrderBy(x => x.ZZS_Preference);

			foreach (var preference in dynamicPreferenceList)
			{
				result.AddPairIfNotExist(preference.ZZS_Preference, GetPreferenceDescription(preference.TranslatedPreferenceDescription, preference.ZZS_Description));
			}

			return result;

			string GetPreferenceDescription(ZString translatedDescription, ZString defaultDescription)
			{
				return !translatedDescription.IsEmpty
					? translatedDescription
					: defaultDescription;
			}
		}

		public static CodeDescriptionPairList GetPreferenceListByCountry(BusinessObjectFactory factory, ZString coutryCode) => CusRefPreferenceView.Loader.GetList(factory, coutryCode);

		public static CodeDescriptionPairList GetDynamicOrderNumberList(TariffView tariff, params IZZRateSelectionCriteria[] criterias)
										=> GetDynamicOrderNumberList(tariff, (IEnumerable<IZZRateSelectionCriteria>)criterias);

		public static CodeDescriptionPairList GetDynamicOrderNumberList(TariffView tariff, IEnumerable<IZZRateSelectionCriteria> criterias)
		{
			var result = new CodeDescriptionPairList();
			if (tariff != null && criterias != null)
			{
				criterias = criterias.Where(c => !c.TradeGroupCountry.IsEmpty && !c.EffectiveDate.IsEmpty);
				if (criterias.Any())
				{
					var criteriaKeys = criterias.GetRateSelectionCacheKeys(includeConcessionOrder: false);
					var key = string.Join("_", "DynamicOrderNumberList", tariff.PK, criteriaKeys);

					result = tariff.Factory.GetCachedValue(key, () => GetDynamicOrderNumberListCore(tariff, criterias), CacheStalenessPolicy.StaleOnFactorySave);
				}
			}
			return result;
		}

		static CodeDescriptionPairList GetDynamicOrderNumberListCore(TariffView tariff, IEnumerable<IZZRateSelectionCriteria> criterias)
		{
			var result = new CodeDescriptionPairList();

			var dynamicOrderNumberList = GetRateSelectionCriteriaInfo(tariff, criterias)
											.Where(x => !x.ZZT_OrderNumber.IsEmpty && criterias.Any(c => x.MatchExcludingConcessionOrder(c)))
											.Select(x => x.ZZT_OrderNumber)
											.Distinct()
											.OrderBy(x => x);

			foreach (var orderNumber in dynamicOrderNumberList)
			{
				result.AddPairIfNotExist(orderNumber, orderNumber);
			}

			return result;
		}

		#region Rate

		public static CodeDescriptionPairList GetDynamicRateApplicabilityCodeList(TariffView tariff, CodeDescriptionPairList cachedListOfAdditionalCodeDescriptions, params IZZRateSelectionCriteria[] criterias)
												=> GetDynamicRateApplicabilityCodeList(tariff, cachedListOfAdditionalCodeDescriptions, (IEnumerable<IZZRateSelectionCriteria>)criterias);

		public static CodeDescriptionPairList GetDynamicRateApplicabilityCodeList(TariffView tariff, CodeDescriptionPairList cachedListOfAdditionalCodeDescriptions, IEnumerable<IZZRateSelectionCriteria> criterias)
		{
			var result = new CodeDescriptionPairList();
			if (tariff != null && criterias != null)
			{
				criterias = criterias.Where(c => !c.TradeGroupCountry.IsEmpty && !c.EffectiveDate.IsEmpty);
				if (criterias.Any())
				{
					var criteriaKeys = criterias.GetRateSelectionCacheKeys(includeAdditionalCodes: false);
					var key = string.Join("_", "DynamicRateApplicabilityCodeList", tariff.PK, criteriaKeys);
					result = tariff.Factory.GetCachedValue(key, () => GetDynamicRateApplicabilityCodeListCore(tariff, cachedListOfAdditionalCodeDescriptions, criterias), CacheStalenessPolicy.StaleOnFactorySave);
				}
			}
			return result;
		}

		static CodeDescriptionPairList GetDynamicRateApplicabilityCodeListCore(TariffView tariff, CodeDescriptionPairList cachedListOfAdditionalCodeDescriptions, IEnumerable<IZZRateSelectionCriteria> criterias)
		{
			var result = new CodeDescriptionPairList();

			var dynamicAdditionalCodeList = GetRateSelectionCriteriaInfo(tariff, criterias)
												.Where(x => !x.ZZT_AdditionalCode.IsEmpty && criterias.Any(c => x.MatchExcludingAdditionalCodes(c)))
												.Select(x => x.ZZT_AdditionalCode)
												.Distinct()
												.OrderBy(x => x);

			foreach (var additionalCode in dynamicAdditionalCodeList)
			{
				result.AddPairIfNotExist(additionalCode, cachedListOfAdditionalCodeDescriptions?.GetDescriptionFromCode(additionalCode) ?? additionalCode);
			}

			return result;
		}

		public static IEnumerable<RateSelectionCriteriaInfo> GetRateSelectionCriteriaInfo(TariffView tariff, params IZZRateSelectionCriteria[] criterias)
								=> GetRateSelectionCriteriaInfo(tariff, (IEnumerable<IZZRateSelectionCriteria>)criterias);

		static IEnumerable<RateSelectionCriteriaInfo> GetRateSelectionCriteriaInfo(TariffView tariff, IEnumerable<IZZRateSelectionCriteria> criterias)
		{
			IEnumerable<RateSelectionCriteriaInfo> result = Array.Empty<RateSelectionCriteriaInfo>();
			if (tariff != null && criterias.Any())
			{
				criterias = criterias.Where(c => !c.TradeGroupCountry.IsEmpty && !c.EffectiveDate.IsEmpty);
				if (criterias.Any())
				{
					var criteriaKeys = criterias.GetRateSelectionCacheKeys(false, false, false, false);
					var key = string.Join("_", "GetRateSelectionCriteriaInfo", tariff.PK, criteriaKeys);
					result = tariff.Factory.GetCachedValue(key, () => tariff.GetRateSelectionCriteriaInfo(criterias), CacheStalenessPolicy.StaleOnFactorySave);
				}
			}
			return result;
		}

		static string GetRateSelectionCacheKeys(this IEnumerable<IZZRateSelectionCriteria> criterias
			, bool includePrimaryPreference = true
			, bool includeConcessionOrder = true
			, bool includeAdditionalCodes = true
			, bool includeSecondTradeGroups = true)
		{
			return string.Join("_", criterias
				.Select(c => string.Join(",", GetCacheKeys(c)))
				.Distinct()
				.OrderBy(x => x)
			);

			IEnumerable<object> GetCacheKeys(IZZRateSelectionCriteria criteria)
			{
				yield return criteria.TradeGroupCountry;
				yield return criteria.EffectiveDate;
				yield return criteria.DataGrouping;
				yield return criteria.RateType;
				yield return criteria.RateCode;
				if (includePrimaryPreference)
				{
					yield return criteria.PrimaryPreference;
				}
				if (includeConcessionOrder)
				{
					yield return criteria.ConcessionOrder;
				}
				if (includeAdditionalCodes)
				{
					yield return criteria.FlatAdditionalCodes();
				}
				if (includeSecondTradeGroups)
				{
					yield return criteria.FlatSecondTradeGroups();
				}
			}
		}

		#endregion

		#region Additional Code

		public static CodeDescriptionPairList GetDynamicAdditionalCodeList(TariffView tariff,
			CodeDescriptionPairList cachedListOfAdditionalCodeDescriptions,
			IEnumerable<IZZRateSelectionCriteria> rateCriterias,
			IEnumerable<IZZConditionSelectionCriteria> conditionCriterias,
			ZString[] conditionTypesToExclude,
			IVATSelectionCriteria vatCriteria,
			ITariffAdditionalCodeSelectionCriteria tariffAdditionalCodeCriteria)
		{
			var result = new CodeDescriptionPairList();
			if (tariff != null && rateCriterias != null)
			{
				rateCriterias = rateCriterias.Where(c => c != null && !c.TradeGroupCountry.IsEmpty && !c.EffectiveDate.IsEmpty);
				conditionCriterias = conditionCriterias.Where(c => !c.TradeGroupCountry.IsEmpty && !c.EffectiveDate.IsEmpty);
				var rateCriteriaKeys = ZString.Empty;
				var conditionCriteriaKeys = ZString.Empty;
				var vatCriteriaKey = ZString.Empty;
				var tariffAdditionalCodeCriteriaKey = ZString.Empty;
				if (rateCriterias.Any())
				{
					rateCriteriaKeys = rateCriterias.GetRateSelectionCacheKeys(includeAdditionalCodes: false);
				}
				if (conditionCriterias.Any())
				{
					conditionCriteriaKeys = conditionCriterias.GetConditionCriteriaKeys();
				}
				if (vatCriteria != null)
				{
					vatCriteriaKey = vatCriteria.GetVATCriteriaKey();
				}

				if (tariffAdditionalCodeCriteria != null)
				{
					tariffAdditionalCodeCriteriaKey = tariffAdditionalCodeCriteria.GetTariffAdditionalCodeCriteriaKey();
				}
				if (!rateCriteriaKeys.IsEmpty || !conditionCriteriaKeys.IsEmpty || !vatCriteriaKey.IsEmpty || !tariffAdditionalCodeCriteriaKey.IsEmpty)
				{
					var key = string.Join("_", "DynamicAdditionalCodeList", tariff.PK, rateCriteriaKeys, conditionCriteriaKeys, vatCriteriaKey, tariffAdditionalCodeCriteriaKey);
					result = tariff.Factory.GetCachedValue(key, () => GetDynamicAdditionalCodeListCore(tariff, cachedListOfAdditionalCodeDescriptions, rateCriterias, conditionCriterias, conditionCriteriaKeys, conditionTypesToExclude, vatCriteria, vatCriteriaKey, tariffAdditionalCodeCriteria, tariffAdditionalCodeCriteriaKey), CacheStalenessPolicy.StaleOnFactorySave);
				}
			}
			return result;
		}

		static CodeDescriptionPairList GetDynamicAdditionalCodeListCore(TariffView tariff, CodeDescriptionPairList cachedListOfAdditionalCodeDescriptions, IEnumerable<IZZRateSelectionCriteria> rateCriterias, IEnumerable<IZZConditionSelectionCriteria> conditionCriterias, string conditionCriteriaKeys, ZString[] conditionTypesToExclude, IVATSelectionCriteria vatCriteria, string vatCriteriaKey, ITariffAdditionalCodeSelectionCriteria tariffAdditionalCodeCriteria, string tariffAdditionalCodeCriteriaKey)
		{
			var result = new CodeDescriptionPairList();

			var dynamicRateAdditionalCodeList = GetRateSelectionCriteriaInfo(tariff, rateCriterias)
												.Where(x => !x.ZZT_AdditionalCode.IsEmpty && rateCriterias.Any(c => x.MatchExcludingAdditionalCodes(c)))
												.Select(x => x.ZZT_AdditionalCode);
			var dynamicConditionAdditionalCodeList = GetConditionApplicabilitiesByCriteria(tariff, conditionCriterias, conditionCriteriaKeys)
												.Where(x => !x.ZZT_AdditionalCode.IsEmpty && !conditionTypesToExclude.Contains(x.ZX2_ConditionType))
												.Select(x => x.ZZT_AdditionalCode);
			var dynamicVATAdditionalCodeList = GetVATApplicabilities(tariff, vatCriteria, vatCriteriaKey)
												.Where(x => !x.ZX5_AdditionalCode.IsEmpty)
												.Select(x => x.ZX5_AdditionalCode);
			var dynamicTariffAdditionalCodeList = GetTariffAdditionalCodeApplicabilities(tariff, tariffAdditionalCodeCriteria, tariffAdditionalCodeCriteriaKey)
												.Where(x => !x.ZY2_AdditionalCode.IsEmpty)
												.Select(x => x.ZY2_AdditionalCode);
			var dynamicAdditionalCodeList = new List<ZString>();
			dynamicAdditionalCodeList.AddRange(dynamicRateAdditionalCodeList);
			dynamicAdditionalCodeList.AddRange(dynamicConditionAdditionalCodeList);
			dynamicAdditionalCodeList.AddRange(dynamicVATAdditionalCodeList);
			dynamicAdditionalCodeList.AddRange(dynamicTariffAdditionalCodeList);
			var orderedAdditionalCodeList = dynamicAdditionalCodeList.Distinct().OrderBy(x => x);

			foreach (var additionalCode in orderedAdditionalCodeList)
			{
				result.AddPairIfNotExist(additionalCode, cachedListOfAdditionalCodeDescriptions?.GetDescriptionFromCode(additionalCode) ?? additionalCode);
			}
			return result;
		}

		public static IEnumerable<TariffAdditionalCodeView> GetTariffAdditionalCodeApplicabilities(TariffView tariff, ITariffAdditionalCodeSelectionCriteria criteria, string tariffAdditionalCodeCriteriaKey = "")
		{
			IEnumerable<TariffAdditionalCodeView> result = null;
			if (tariff != null && criteria != null)
			{
				var criteriaKeys = string.IsNullOrEmpty(tariffAdditionalCodeCriteriaKey) ? criteria.GetTariffAdditionalCodeCriteriaKey() : tariffAdditionalCodeCriteriaKey;
				var key = string.Join("_", "GetTariffAdditionalCodeApplicabilitiesByCriteria", tariff.PK, criteriaKeys);
				result = tariff.Factory.GetCachedValue(key, () =>
				{
					var tariffCriteriaSet = new TariffAdditionalCodeLoadTariffCriteriaSet(tariff, criteria);
					var loader = new ApplicableTariffAdditionalCodeLoader(tariff.Factory);
					return loader.LoadTariffAdditionalCodesForSingleCriteriaSet(tariffCriteriaSet);
				});
			}
			return result ?? Array.Empty<TariffAdditionalCodeView>();
		}

		public static IEnumerable<VATApplicabilityView> GetVATApplicabilities(TariffView tariff, IVATSelectionCriteria criteria, string vatCriteriaKeys = "")
		{
			IEnumerable<VATApplicabilityView> result = Array.Empty<VATApplicabilityView>();
			if (tariff != null && criteria != null)
			{
				var criteriaKeys = string.IsNullOrEmpty(vatCriteriaKeys) ? criteria.GetVATCriteriaKey() : vatCriteriaKeys;
				var key = string.Join("_", "GetVATApplicabilitiesByCriteria", tariff.PK, criteriaKeys);
				result = tariff.Factory.GetCachedValue(key, () =>
				{
					var tradeGroupLoader = new CusRefTradeGroupView.Loader(tariff.Factory);
					var tradeGroups = tradeGroupLoader.Load(criteria.DataGrouping, criteria.TradeGroups.ToArray(), criteria.EffectiveDate).Select(x => x.PK);
					var vatApplicabilities = tariff.GetEffectiveVATApplicabilities(criteria.EffectiveDate)
						.Where(x => x.ZX5_ZZZ_NKDataGrouping == criteria.DataGrouping)
						.Where(x => x.ZX5_ZZF_NKTaxOrFeeCode == criteria.TaxOrFeeCode)
						.Where(x => !tradeGroups.Any() || tradeGroups.Contains(x.ZX5_ZZA_TradeGroup));
					return vatApplicabilities;
				});
			}
			return result;
		}

		public static IEnumerable<ConditionApplicabilitiesByCriteria> GetConditionApplicabilitiesByCriteria(TariffView tariff, IEnumerable<IZZConditionSelectionCriteria> criterias, string conditionCriteriaKeys = "")
		{
			IEnumerable<ConditionApplicabilitiesByCriteria> result = Array.Empty<ConditionApplicabilitiesByCriteria>();
			if (tariff != null && criterias.Any())
			{
				criterias = criterias.Where(c => !c.TradeGroupCountry.IsEmpty && !c.EffectiveDate.IsEmpty);
				if (criterias.Any())
				{
					var criteriaKeys = string.IsNullOrEmpty(conditionCriteriaKeys) ? criterias.GetConditionCriteriaKeys() : conditionCriteriaKeys;
					var key = string.Join("_", "GetConditionApplicabilitiesByCriteria", tariff.PK, criteriaKeys);
					result = tariff.Factory.GetCachedValue(key, () => tariff.GetConditionApplicabilitiesByCriteriaInfo(criterias), CacheStalenessPolicy.StaleOnFactorySave);
				}
			}
			return result;
		}

		static string GetConditionCriteriaKeys(this IEnumerable<IZZConditionSelectionCriteria> criterias) => string.Join("_", criterias
			.Select(c => string.Join(",", c.TradeGroupCountry, c.EffectiveDate, c.DataGrouping, c.ConditionClass, c.ConditionType, c.Direction.ToString(), c.PrimaryPreference, c.ConcessionOrder, c.FlatSecondTradeGroups()))
			.Distinct()
			.OrderBy(x => x)
		);

		static string GetVATCriteriaKey(this IVATSelectionCriteria criteria) => string.Join(",", criteria.FlatTradeGroups(), criteria.EffectiveDate, criteria.DataGrouping, criteria.TaxOrFeeCode);

		static string GetTariffAdditionalCodeCriteriaKey(this ITariffAdditionalCodeSelectionCriteria criteria) => string.Join(",", criteria.Category, criteria.ValidEffectiveDate(), criteria.DataGrouping, criteria.TradeGroupCountry);
	}

	#endregion
}
