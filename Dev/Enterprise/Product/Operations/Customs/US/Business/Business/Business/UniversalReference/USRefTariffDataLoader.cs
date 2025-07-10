using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.Business
{
	public static class USRefTariffDataLoader
	{
		public static TariffViewAsCodeDescription[] GetSupTariffs(BusinessObjectFactory factory, ZString tariffCode, IZZRateSelectionCriteria criteria, string[] ruleLabels, ZString countryOfOrigin, bool isSteelOriginFromEUN)
		{
			var countryOfOriginRate = GetCountryOfOriginRate(countryOfOrigin);
			return factory.GetCachedValue("GetSupTariffsFor" + tariffCode + RateLoadTariffCriteriaSet.GetCriteriaSetCacheKey(ZGuid.Empty, criteria) + string.Join("_", ruleLabels) + countryOfOriginRate + isSteelOriginFromEUN.ToString(), delegate
			{
				var shouldAddNotApplicable = false;
				var effectiveValuationDate = criteria.ValidEffectiveDate();
				var result = new List<TariffViewAsCodeDescription>();
				if (IsValidTariffOnSpecificDate(factory, tariffCode, effectiveValuationDate))
				{
					var childTariffs = new TariffView.Loader(factory).GetEffectiveChildTariffs(Core.Constants.CountryCodes.UnitedStates, Constants.TariffTypes.HarmonizedSystem, tariffCode, effectiveValuationDate);
					if (childTariffs != null)
					{
						var isTAriffGAE = TariffViewHasRuleWithAttribute(factory, tariffCode, effectiveValuationDate, UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.GAE) != null;
						LoadAndCacheApplicableRates(factory, criteria, childTariffs);
						foreach (var ruleLabel in ruleLabels)
						{
							foreach (var childTariffView in childTariffs)
							{
								if ((childTariffView?.HasAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, ruleLabel) ?? false) &&
									(childTariffView?.GetApplicableRates(criteria)?.Any(x => x.RateApplicabilities.Any(y => y.IsApplicable(countryOfOriginRate, effectiveValuationDate))) ?? false))
								{
									TariffViewAsCodeDescription tariffViewAsCodeDescription = null;

									if (isTAriffGAE)
									{
										if (TariffViewHasRuleWithAttribute(factory, childTariffView.ZZ1_TariffCode, effectiveValuationDate, UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values._232) == null)
										{
											tariffViewAsCodeDescription = new TariffViewAsCodeDescription(childTariffView.ZZ1_TariffCode, childTariffView.ZZ1_Description, !childTariffView.HasAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.MAND, YesNoDefaultList.Codes.No));
										}
										else if (!isSteelOriginFromEUN)
										{
											tariffViewAsCodeDescription = new TariffViewAsCodeDescription(childTariffView.ZZ1_TariffCode, childTariffView.ZZ1_Description, !childTariffView.HasAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.MAND, YesNoDefaultList.Codes.No));
										}
									}
									else
									{
										tariffViewAsCodeDescription = new TariffViewAsCodeDescription(childTariffView.ZZ1_TariffCode, childTariffView.ZZ1_Description, !childTariffView.HasAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.MAND, YesNoDefaultList.Codes.No));
									}

									if (tariffViewAsCodeDescription != null)
									{
										result.Add(tariffViewAsCodeDescription);

										if (!shouldAddNotApplicable
										&& childTariffView.HasAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.RussianTariffs)
										&& childTariffView.HasAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.TYPE, UniversalReferenceConstants.TariffAttributeTypes.Values.BabyFomula))
										{
											shouldAddNotApplicable = true;
										}
									}
								}
							}
						}
					}
				}

				if (shouldAddNotApplicable)
				{
					result.Add(new TariffViewAsCodeDescription(TariffViewAsCodeDescription.NotApplicableCode, TariffViewAsCodeDescription.NotApplicableDescription, false));
				}
				return result.OrderBy(x => x.Tariff).ToArray();
			});
		}

		static void LoadAndCacheApplicableRates(BusinessObjectFactory factory, IZZRateSelectionCriteria criteria, TariffView[] tariffViews)
		{
			var applicableRateLoader = new ApplicableRateLoader(factory);
			var rateLoadTariffCriteriaSets = new List<RateLoadTariffCriteriaSet>();
			foreach (var tariffView in tariffViews)
			{
				rateLoadTariffCriteriaSets.Add(new RateLoadTariffCriteriaSet(tariffView, criteria));
			}
			applicableRateLoader.CacheRatesForMultipleCriteriaSets(rateLoadTariffCriteriaSets);
		}

		static bool IsValidTariffOnSpecificDate(BusinessObjectFactory factory, ZString tariffCode, ZDateTime valuationDate)
		{
			return new USCTariff.Loader(factory).LoadBestMatch(tariffCode, valuationDate) != null;
		}

		public static ZString[] GetEffectiveTariffRuleCodes(BusinessObjectFactory factory, ZString tariffCode, ZDateTime effectiveDate)
		{
			return factory.GetCachedValue($"TariffRuleCodesFor_{tariffCode}_{effectiveDate}", delegate
			{
				var result = new List<ZString>();
				var tariffView = new TariffView.Loader(factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.UnitedStates, Constants.TariffTypes.HarmonizedSystem, tariffCode, effectiveDate);
				var ruleAttribute = tariffView?.GetAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE);
				if (ruleAttribute != null && !ruleAttribute.ZZ3_Value.IsEmpty)
				{
					result.AddRange(ruleAttribute.ZZ3_Value.Trim().Split(','));
				}

				return result.ToArray();
			});
		}

		public static bool IsEmbroideryTariff(BusinessObjectFactory factory, ZString tariffCode, ZDateTime effectiveDate)
		{
			return TariffViewHasRuleWithAttribute(factory, tariffCode, effectiveDate, UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.MoreCodes.Embroidery) != null;
		}

		public static TariffView TariffViewHasRuleWithAttribute(BusinessObjectFactory factory, ZString tariffCode, ZDateTime effectiveDate, ZString attributeName, ZString attributeValue)
		{
			var effectiveValuationDate = effectiveDate.IsValid ? effectiveDate : ZDateTime.Today;

			return factory.GetCachedValue(tariffCode + effectiveValuationDate + attributeName + attributeValue, delegate
			{
				var tariffView = new TariffView.Loader(factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.UnitedStates, Constants.TariffTypes.HarmonizedSystem, tariffCode, effectiveValuationDate);
				var attribute = tariffView?.GetAttribute(attributeName);
				if (attribute != null && !attribute.ZZ3_Value.IsEmpty)
				{
					var attributeValues = attribute.ZZ3_Value.Trim().Split(',');
					if (attributeValues.Contains(attributeValue))
					{
						return tariffView;
					}
				}

				return null;
			});
		}

		public static bool IsTradeGroupEUNForSupTariff(BusinessObjectFactory factory, ZString countryCode, ZString supTariffCode, ZDateTime valuationDate)
		{
			var effectiveValuationDate = valuationDate.IsValid ? valuationDate : ZDateTime.Today;

			return factory.GetCachedValue("isTradeGroupEUNForSupTariff|" + supTariffCode + effectiveValuationDate, () =>
			{
				var tariffLoader = new TariffView.Loader(factory);
				var matchedTariff = tariffLoader.LoadMostRecentCachedTariff(countryCode, Universal.Constants.TariffTypes.HarmonizedSystem, supTariffCode, effectiveValuationDate);

				var tradeGroupLoader = new CusRefTradeGroupView.Loader(factory);
				var tradeGroupEUN = tradeGroupLoader.Load(countryCode, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, effectiveValuationDate);

				if (matchedTariff != null && tradeGroupEUN != null)
				{
					return matchedTariff.Rates.Any(x => x.ZZ2_ZZ1_ParentTariffOrNationalCode == matchedTariff.PK && x.ZZ2_StartDate <= effectiveValuationDate && x.ZZ2_EndDate >= effectiveValuationDate
											&& x.RateApplicabilities.Any(a => a.ZZT_ZZA_TradeGroup == tradeGroupEUN.PK && a.ZZT_StartDate <= effectiveValuationDate && a.ZZT_EndDate >= effectiveValuationDate));
				}

				return false;
			});
		}

		public static ZBool IsTariffMatchCondition(BusinessObjectFactory factory, ZString tariffCode, ZString conditionClass, ZString conditionType, ZString conditionValueType, ZString conditionValue, ZString tradeGroupCountry, ZDateTime valuationDate)
		{
			var effectiveValuationDate = valuationDate.IsValid ? valuationDate : ZDateTime.Today;
			var countryOfOriginRate = GetCountryOfOriginRate(tradeGroupCountry);
			return factory.GetCachedValue($"isTariffMatchCondition|{tariffCode}|{conditionClass}|{conditionType}|{conditionValue}|{countryOfOriginRate}|{effectiveValuationDate}", () =>
			{
				var result = false;
				if (!tariffCode.IsEmpty && !conditionClass.IsEmpty && !conditionType.IsEmpty)
				{
					var matchedTariff = new TariffView.Loader(factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.UnitedStates, Constants.TariffTypes.HarmonizedSystem, tariffCode, effectiveValuationDate);
					if (matchedTariff != null)
					{
						var criteria = new ZZConditionSelectionCriteria(effectiveValuationDate, countryOfOriginRate, ZString.Empty, null, ZString.Empty, Core.Constants.CountryCodes.UnitedStates, ConditionChecker.ConditionDirection.Import, conditionClass, conditionType);

						foreach (var condition in ConditionChecker.GetApplicableConditions(factory, matchedTariff, criteria))
						{
							if ((conditionValueType.IsEmpty && conditionValue.IsEmpty) || condition.ConditionValues.Any(x => x.ValueType == conditionValueType && x.ZX3_Value == conditionValue))
							{
								result = true;
								break;
							}
						}
					}
				}

				return result;
			});
		}

		public static RefCusCondition GetApplicableCondition(BusinessObjectFactory factory, ZString tariffCode, ZString conditionType, ZDateTime valuationDate)
		{
			var effectiveValuationDate = valuationDate.IsValid ? valuationDate : ZDateTime.Today;
			return factory.GetCachedValue($"GetApplicableCondition|{tariffCode}|{conditionType}|{effectiveValuationDate}", () =>
			{
				RefCusCondition result = null;
				if (!tariffCode.IsEmpty && !conditionType.IsEmpty)
				{
					var matchedTariff = new TariffView.Loader(factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.UnitedStates, Constants.TariffTypes.HarmonizedSystem, tariffCode, effectiveValuationDate);
					if (matchedTariff != null)
					{
						result = matchedTariff.Conditions.FirstOrDefault(x => x.ConditionType == conditionType);
					}
				}
				return result;
			});
		}

		static ZString GetCountryOfOriginRate(ZString countryOfOrigin ) => (CanadaProvinceTerritoryCodes.IsCanadianProvince(countryOfOrigin) || CanadaProvinceTerritoryCodes.IsCanadianSoftwoodLumberRegion(countryOfOrigin)) ? new ZString(Core.Constants.CountryCodes.Canada) : countryOfOrigin;
	}
}
