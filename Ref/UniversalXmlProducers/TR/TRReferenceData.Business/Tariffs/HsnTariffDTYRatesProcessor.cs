using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Services;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using Microsoft.Extensions.Logging;

namespace CargoWise.RefDbRepo.TRReferenceData.Business
{
	public static class HsnTariffDTYRatesProcessor
	{
		public static void AttachDTYRates(IEnumerable<RefCusTariff> tariffs)
		{
			foreach (var tariff in tariffs)
			{
				var rule = RateLoader.GetRule(tariff.ZZ1_TariffCode);
				if (rule == null)
				{
					continue;
				}

				AttachRatesTo(tariff, rule);
			}

			var unusedRules = RateLoader.GetUnusedRules(tariffs).ToArray();
			if (unusedRules.Length > 0)
			{
				var unusedRulesMessage = $"Unused rules found: {string.Join(',', unusedRules.ToArray())}";
				StaticLogger.LogError(unusedRulesMessage);
			}
		}

		static void AttachRatesTo(RefCusTariff tariff, HsnTariffDTYRateRule rateRule)
		{
			var rates = new List<RefCusRate>(tariff.RefCusRates ?? Enumerable.Empty<RefCusRate>());
			var startDate = tariff.ZZ1_StartDate;

			foreach (var preferenceRule in rateRule.PreferenceRules)
			{
				var bestMatchingPreference = GetBestMatchingZZS_Preference(preferenceRule);
				if (!TryGetBestMatchingZZA_TradeGroupFor(tariff.ZZ1_TariffCode, bestMatchingPreference, out var bestMatchingTradeGroup))
				{
					continue;
				}

				var applicability = new RefCusApplicability
				{
					ZZT_StartDate = preferenceRule.StartDate ?? startDate,
					ZZT_ZZA_NKTradeGroup = bestMatchingTradeGroup,
					ZZT_ZZA_ZZZ_NKDataGrouping = Constants.CountryCodeTR
				};

				var prefExcludesFromFootnotes = GetPreferenceExcludedTradeGroups(rateRule).ToArray();
				if (prefExcludesFromFootnotes.Length > 0)
				{
					applicability.RefCusExcludedTradeGroups = prefExcludesFromFootnotes;
				}

				var preferenceRate = rates.FirstOrDefault(rate => rate.ZZ2_ZZS_NKPreference == bestMatchingPreference);
				if (preferenceRate == null)
				{
					preferenceRate = new RefCusRate
					{
						ZZ2_ZZS_NKPreference = bestMatchingPreference,
						ZZ2_ZZS_ZZZ_NKDataGrouping = Constants.CountryCodeTR,
					};
					rates.Add(preferenceRate);
				}
				preferenceRate.ZZ2_RateFormula = preferenceRule.Formula;
				preferenceRate.ZZ2_RateFormulaDerivedFrom = bestMatchingPreference;
				preferenceRate.ZZ2_StartDate = preferenceRule.StartDate ?? startDate;
				preferenceRate.ZZ2_ZY1_NKRateCode = preferenceRule.RateCode;
				preferenceRate.ZZ2_ZY1_ZZR_NKRateType = preferenceRule.RateType;
				preferenceRate.RefCusApplicabilities = new[] { applicability };

				if (preferenceRule.EndDate.HasValue)
				{
					preferenceRate.ZZ2_EndDate = preferenceRule.EndDate.Value;
					applicability.ZZT_EndDate = preferenceRule.EndDate.Value;
				}

				foreach (var preferenceFootnote in preferenceRule.Footnotes)
				{
					// TODO: Cases related to preferences with footnotes still need thorough testing, e.g. [II Sayìlì Liste (68-83. Fasìllar).xlsx]-section 70-700420991000.
					rates.AddRange(rateRule.FootnoteRules
						.Where(rule => rule.FootnoteCode == preferenceFootnote)
						.SelectMany(footnoteRule => footnoteRule.ToRates(startDate, tariff.ZZ1_TariffCode, preferenceRule))
					);
				}
			}
			rates.AddRange(rateRule.FootnoteRules.SelectMany(footnoteRule => footnoteRule.ToRates(startDate, tariff.ZZ1_TariffCode)));

			tariff.RefCusRates = rates.ToArray();
		}

		static IEnumerable<RefCusExcludedTradeGroup> GetPreferenceExcludedTradeGroups(HsnTariffDTYRateRule rateRule)
		{
			foreach (var footnoteRule in rateRule.FootnoteRules)
			{
				var footnoteIncludes = footnoteRule.TradingPartnersOverride ?? footnoteRule.TradingPartners;
				var hsnTariffDtyRateTradingPartners = footnoteIncludes as HsnTariffDTYRateTradingPartner[] ?? footnoteIncludes.ToArray();

				foreach (var includedTradingPartner in hsnTariffDtyRateTradingPartners)
				{
					if (includedTradingPartner.Type !=
						HsnTariffDTYRateTradingPartner.HsnTariffDTYRateTradingPartnerType.TradeGroup)
					{
						continue;
					}

					var prefExcTradeGroup = new RefCusExcludedTradeGroup
					{
						ZZC_ZZA_NKTradeGroup = includedTradingPartner.Code,
						ZZC_ZZA_ZZZ_NKDataGrouping = Constants.CountryCodeTR,
					};
					yield return prefExcTradeGroup;
				}

				if (hsnTariffDtyRateTradingPartners.Any(i => i.Type == HsnTariffDTYRateTradingPartner.HsnTariffDTYRateTradingPartnerType.Country))
				{
					var prefExcTradeGroup = new RefCusExcludedTradeGroup
					{
						ZZC_ZZA_NKTradeGroup = HsnTariffDTYRateLoader.GetTradingGroupCode(footnoteRule, isExclusion: false),
						ZZC_ZZA_ZZZ_NKDataGrouping = Constants.CountryCodeTR,
					};
					yield return prefExcTradeGroup;
				}
			}
		}

		static IEnumerable<RefCusRate> ToRates(this HsnTariffDTYRateFootnoteRule footnoteRule, DateTime startDate, string tariffcode, HsnTariffDTYRatePreferenceRule preferenceRule = null)
		{
			var preference = preferenceRule == null ? Constants.Preference.Code.OtherCountries : GetBestMatchingZZS_Preference(preferenceRule);

			var applicableTradingPartners = (footnoteRule.TradingPartnersOverride ?? footnoteRule.TradingPartners).ToArray();
			if (applicableTradingPartners.Length == 0)
			{
				applicableTradingPartners = HsnTariffDTYRateTradingPartner.From(preference).ToArray();
			}

			if (applicableTradingPartners.Length > 0)
			{
				var matchingTradeGroups = new List<string>();
				foreach (var tradingPartner in applicableTradingPartners)
				{
					// TODO: Check if returning the correct trading group
					if (TryGetBestMatchingZZA_TradeGroupFor(tariffcode, tradingPartner.Code, out var bestMatchingTradeGroup))
					{
						matchingTradeGroups.Add(bestMatchingTradeGroup);
					}
				}

				if (matchingTradeGroups.Count > 0)
				{
					var excludedTradeGroups = GetExcludedTradingPartnerRecords(footnoteRule, startDate, preference);

					var tgApplicabilities = matchingTradeGroups.Select(matchingTradeGroup =>
					{
						// TODO: First try and find the existing applicability. If not, then create a new one.
						var tgApplicability = new RefCusApplicability
						{
							ZZT_StartDate = footnoteRule.StartDate ?? startDate,
							ZZT_AdditionalCode = footnoteRule.AdditionalCode,
							ZZT_ZZA_NKTradeGroup = matchingTradeGroup,
							ZZT_ZZA_ZZZ_NKDataGrouping = Constants.CountryCodeTR
						};

						if (excludedTradeGroups.Length > 0)
						{
							tgApplicability.RefCusExcludedTradeGroups = excludedTradeGroups;
						}

						if (footnoteRule.EndDate.HasValue)
						{
							tgApplicability.ZZT_EndDate = footnoteRule.EndDate.Value;
						}

						return tgApplicability;
					}).ToArray();

					var tgRefCusRate = new RefCusRate
					{
						ZZ2_RateFormula = footnoteRule.Formula,
						ZZ2_RateFormulaDerivedFrom = $"{preference}-{footnoteRule.FootnoteCode}",
						ZZ2_StartDate = footnoteRule.StartDate ?? startDate,
						ZZ2_ZY1_NKRateCode = footnoteRule.RateCode,
						ZZ2_ZY1_ZZR_NKRateType = footnoteRule.RateType,
						ZZ2_ZZS_NKPreference = preference,
						ZZ2_ZZS_ZZZ_NKDataGrouping = Constants.CountryCodeTR,
						RefCusApplicabilities = tgApplicabilities,
					};

					if (footnoteRule.EndDate.HasValue)
					{
						tgRefCusRate.ZZ2_EndDate = footnoteRule.EndDate.Value;
					}

					yield return tgRefCusRate;
				}
			}
		}

		static RefCusExcludedTradeGroup[] GetExcludedTradingPartnerRecords(HsnTariffDTYRateFootnoteRule footnoteRule, DateTime startDate,
			string preference)
		{
			var matchingExcludedTradeGroups = new List<string>();

			var excludedTradingPartners =
				(footnoteRule.ExcludingTradingPartnersOverride ?? footnoteRule.ExcludingTradingPartners).ToArray();

			foreach (var excludingTradingPartner in excludedTradingPartners.Where(tp =>
						 tp.Type == HsnTariffDTYRateTradingPartner.HsnTariffDTYRateTradingPartnerType.TradeGroup))
			{
				if (TryGetBestMatchingZZA_TradeGroupFor(footnoteRule.RateCode, excludingTradingPartner.Code, out var bestMatchingTradeGroup))
				{
					matchingExcludedTradeGroups.Add(bestMatchingTradeGroup);
				}
			}

			var tgCountries = excludedTradingPartners.Where(tp =>
				tp.Type == HsnTariffDTYRateTradingPartner.HsnTariffDTYRateTradingPartnerType.Country).ToArray();

			if (tgCountries.Length > 0)
			{
				var tradeGroupCountries = new List<RefCusTradeGroupCountry>();
				foreach (var country in tgCountries)
				{
					var rctgc = new RefCusTradeGroupCountry
					{
						ZZB_Description = RefCusTradeGroupHelper.GetCountryName(country.Code),
						ZZB_RN_NKTradeGroupCountryCode = country.Code
					};
					tradeGroupCountries.Add(rctgc);
				}

				var tgKey = $"EX-{preference}-{footnoteRule.FootnoteCode}" + string.Join("|", tgCountries.Select(t => t.Code).OrderBy(c => c));
				var tgDesc = $"Excluded by {preference}-{footnoteRule.FootnoteCode}";
				var aa = new RefCusTradeGroup
				{
					ZZA_TradeGroup = HsnTariffDTYRateLoader.GetTradingGroupCode(footnoteRule, isExclusion: true),
					//ZZA_Description = tgDesc,
					ZZA_ZZZ_NKDataGrouping = Constants.CountryCodeTR,
					RefCusTradeGroupCountries = tradeGroupCountries.ToArray(),
				};
				matchingExcludedTradeGroups.Add(aa.ZZA_TradeGroup);
			}

			if (!matchingExcludedTradeGroups.Any())
			{
				return Array.Empty<RefCusExcludedTradeGroup>();
			}

			var excludedTradeGroups = matchingExcludedTradeGroups.Select(excludedTradeGroup =>
				new RefCusExcludedTradeGroup
				{
					ZZC_ZZA_NKTradeGroup = excludedTradeGroup,
					ZZC_ZZA_ZZZ_NKDataGrouping = Constants.CountryCodeTR
				}
			).ToArray();

			return excludedTradeGroups;
		}

		static string GetBestMatchingZZS_Preference(HsnTariffDTYRatePreferenceRule preferenceRule)
		{
			var preferenceText = preferenceRule.Preference;
			string result;

			var preferenceTextSearchKey = preferenceText.ToSearchKey();
			if (preferenceTextSearchKey.StartsWith(AllCountriesSearchKey.Value, StringComparison.InvariantCulture))
			{
				result = Constants.Preference.Code.STD;
			}
			else if (preferenceTextSearchKey.StartsWith(OtherCountriesSearchKey.Value, StringComparison.InvariantCulture))
			{
				result = Constants.Preference.Code.OtherCountries;
			}
			else if (RefCusTradeGroupHelper.GetBestMatchRefCusTradeGroupFor(preferenceText) is RefCusTradeGroup refCusTradeGroup)
			{
				result = refCusTradeGroup.ZZA_TradeGroup;
			}
			else
			{
				throw new KeyNotFoundException($"Unable to find a ZZS_Preference for \"{preferenceText}\".");
			}
			return result;
		}

		static bool TryGetBestMatchingZZA_TradeGroupFor(string tariffCode, string ZZS_Preference, out string ZZA_TradeGroup)
		{
			switch (ZZS_Preference)
			{
				case Constants.Preference.Code.STD:
					ZZA_TradeGroup = Constants.TradeGroup.AllCountries;
					break;
				case Constants.Preference.Code.OtherCountries:
					ZZA_TradeGroup = RateLoader.GetOtherCountriesTradingGroupCode(tariffCode);
					break;
				default:
					{
						if (RefCusTradeGroupHelper.GetBestMatchRefCusTradeGroupFor(ZZS_Preference) is RefCusTradeGroup refCusTradeGroup)
						{
							ZZA_TradeGroup = refCusTradeGroup.ZZA_TradeGroup;
						}
						else
						{
							ZZA_TradeGroup = null;
						}

						break;
					}
			}

			return ZZA_TradeGroup != null;
		}

		static readonly Lazy<string> AllCountriesSearchKey = new Lazy<string>(() => Constants.TradeGroup.AllCountries.ToSearchKey());

		static readonly Lazy<string> OtherCountriesSearchKey = new Lazy<string>(() => DtyRateConstants.OtherCountries.ToSearchKey());

		static ILogger GetLogger()
		{
			using (var factory = LoggerFactory.Create(builder => { }))
			{
				return factory.CreateLogger<HsnTariffDTYRateLoader>();
			}
		}

		static readonly ILogger StaticLogger = GetLogger();

		static readonly HsnTariffDTYRateLoader RateLoader = new HsnTariffDTYRateLoader(StaticLogger);
	}
}
