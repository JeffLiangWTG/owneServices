using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Loaders
{
	public abstract class HsnTariffDTYRatesReader
	{
		protected HsnTariffDTYRatesReader(string inputPath, ILogger logger)
		{
			Logger = Argument.NotNull(logger, nameof(logger));
			DataFileDirectory = inputPath;

			HsnTariffDTYRateDictionary = new Lazy<Dictionary<string, HsnTariffDTYRateRule>>(GetTariffAndExplanations);
		}

		protected HsnTariffDTYRatesReader(ILogger logger)
			: this(Path.Combine(ApplicationConfig.ResPath, DtyRateConstants.DtyRatesSubFolder), logger)
		{ }

		public bool TryGetRule(string tariffCode, out HsnTariffDTYRateRule rule)
		{
			if (!HsnTariffDTYRateDictionary.Value.TryGetValue(tariffCode, out rule))
			{
				return false;
			}

			foreach (var footnoteRule in rule.FootnoteRules)
			{
				var additionalTradeGroups = GetTradeGroups(footnoteRule).ToArray();
				if (additionalTradeGroups.Length == 0)
				{
					continue;
				}

				var additionalTradingPartners = additionalTradeGroups.Where(tg => !tg.Excluded)
					.SelectMany(tg => HsnTariffDTYRateTradingPartner.From(tg.Code));

				if (additionalTradingPartners.Any())
				{
					footnoteRule.TradingPartnersOverride =
						footnoteRule.ExcludingTradingPartners.Where(tg =>
							tg.Type == HsnTariffDTYRateTradingPartner.HsnTariffDTYRateTradingPartnerType
								.TradeGroup);


					footnoteRule.TradingPartnersOverride =
						footnoteRule.TradingPartnersOverride.Union(additionalTradingPartners);
				}

				var additionalExcludingTradingPartners = additionalTradeGroups.Where(tg => tg.Excluded)
					.SelectMany(tg => HsnTariffDTYRateTradingPartner.From(tg.Code));

				if (additionalExcludingTradingPartners.Any())
				{
					footnoteRule.ExcludingTradingPartnersOverride =
						footnoteRule.ExcludingTradingPartners.Where(tg =>
							tg.Type == HsnTariffDTYRateTradingPartner.HsnTariffDTYRateTradingPartnerType
								.TradeGroup);


					footnoteRule.ExcludingTradingPartnersOverride =
						footnoteRule.ExcludingTradingPartnersOverride.Union(additionalExcludingTradingPartners);
				}
			}

			return true;

		}

		public bool TryGetOtherCountryGroupCode(string tariffCode, out string groupCode)
		{
			groupCode = default;
			if (!HsnTariffDTYRateDictionary.Value.ContainsKey(tariffCode))
			{
				return false;
			}

			groupCode = GetOtherCountriesTradingGroupCode();
			return true;
		}

		public IEnumerable<string> TariffCodes => HsnTariffDTYRateDictionary.Value.Keys;

		public TradeGroup[] AdditionalTradeGroups
		{
			get
			{
				if (additionalTradeGroups != null)
				{
					return additionalTradeGroups;
				}

				var additionalTradeGroupList = new HashSet<TradeGroup>
				{
					GetOtherCountriesTradingGroup()
				};

				foreach (var rule in FootnoteExplainer.AllRules)
				{
					additionalTradeGroupList.UnionWith(GetTradeGroups(rule));
				}

				additionalTradeGroups = additionalTradeGroupList.ToArray();
				return additionalTradeGroups;
			}
		}
		TradeGroup[] additionalTradeGroups;

		public virtual DateTime DefaultStartDate => DtyRateConstants.DefaultStartDate;

		public virtual DateTime DefaultEndDate => DtyRateConstants.DefaultEndDate;

		protected abstract string ListFileName { get; }

		protected abstract string ExplanationFileName { get; }

		protected abstract HashSet<string> ListHeaders { get; }

		protected abstract HashSet<string> SearchHeaders { get; }

		protected abstract HashSet<string> PreferenceHeaders { get; }

		protected abstract string TariffCodeHeader { get; }

		protected abstract string FootnoteHeader { get; }

		protected virtual HsnTariffDTYRatePreferenceExplainer PreferenceExplainer =>
			preferenceExplainer ??
			(preferenceExplainer = new HsnTariffDTYRatePreferenceExplainer(ExplanationFilePath, Logger));
		HsnTariffDTYRatePreferenceExplainer preferenceExplainer;

		protected virtual HsnTariffDTYRateFootnoteExplainer FootnoteExplainer =>
			footnoteExplainer ??
			(footnoteExplainer = new HsnTariffDTYRateFootnoteExplainer(ExplanationFilePath, Logger));
		HsnTariffDTYRateFootnoteExplainer footnoteExplainer;

		protected virtual bool PreferenceContainsFootnote => false;

		protected virtual bool IsValidTariffCode(string tariffCode)
		{
			return !string.IsNullOrWhiteSpace(tariffCode);
		}

		protected virtual string GetCellFormat(string header)
		{
			if (PreferenceHeaders.Contains(header))
			{
				return DtyRateConstants.NumericCellFormat;
			}

			switch (header)
			{
				case DtyRateConstants.TariffCode:
					return DtyRateConstants.TariffCellFormat;
				default:
					return null;
			}
		}

		protected ILogger Logger { get; }

		string DataFileDirectory { get; }

		string ListFilePath => Path.Combine(DataFileDirectory, ListFileName);

		string ExplanationFilePath => Path.Combine(DataFileDirectory, ExplanationFileName);

		Lazy<Dictionary<string, HsnTariffDTYRateRule>> HsnTariffDTYRateDictionary { get; }

		#region DTO Implementation

		IEnumerable<TradeGroup> GetTradeGroups(HsnTariffDTYRateFootnoteRule rule)
		{
			var tradingPartners = GetTradeGroup(rule, rule.TradingPartners);
			if (tradingPartners != null)
			{
				yield return tradingPartners;
			}

			var excTradingPartners = GetTradeGroup(rule, rule.ExcludingTradingPartners, isExclusion: true);
			if (excTradingPartners != null)
			{
				yield return excTradingPartners;
			}
		}

		TradeGroup GetTradeGroup(HsnTariffDTYRateFootnoteRule rule, IEnumerable<HsnTariffDTYRateTradingPartner> tradingPartners, bool isExclusion = false)
		{
			var tradeGroupCountries = new List<TradeGroupCountry>();

			foreach (var tradingPartner in tradingPartners)
			{
				if (tradingPartner.Type !=
					HsnTariffDTYRateTradingPartner.HsnTariffDTYRateTradingPartnerType.Country)
				{
					continue;
				}

				var country = RefCusTradeGroupHelper.GetCountry(tradingPartner.Code);
				var tradeGroupCountry = GetTradeGroupCountry(country, rule);
				tradeGroupCountries.Add(tradeGroupCountry);
			}

			if (tradeGroupCountries.Count == 0)
			{
				return null;
			}

			var code = GetTradeGroupCode(rule, isExclusion);
			var description = GetTradeGroupDescription(rule, isExclusion);
			var tradeGroup = new TradeGroup
			{
				Code = code,
				Description = description,
				StartDate = rule.StartDate ?? DefaultStartDate,
				EndDate = rule.EndDate ?? DefaultEndDate,
				Countries = tradeGroupCountries,
				Excluded = isExclusion,
			};

			return tradeGroup;
		}

		TradeGroup GetOtherCountriesTradingGroup()
		{
			var usedCountries = new HashSet<string>();

			foreach (var rule in PreferenceExplainer.AllRules)
			{
				if (rule.Preference.ToSearchKey().StartsWith(DtyRateConstants.OtherCountries.ToSearchKey(), StringComparison.InvariantCultureIgnoreCase))
				{
					continue;
				}

				var existingTradeGroup = RefCusTradeGroupHelper.GetBestMatchRefCusTradeGroupFor(rule.Preference);
				var countriesInTradeGroup = existingTradeGroup
					.RefCusTradeGroupCountries
					.Where(c => c.ZZB_StartDate <= DateTime.UtcNow && c.ZZB_EndDate >= DateTime.UtcNow)
					.Select(c => c.ZZB_RN_NKTradeGroupCountryCode);
				usedCountries.UnionWith(countriesInTradeGroup);
			}

			var otherCountries =
				RefCusTradeGroupHelper.GetBestMatchRefCusTradeGroupFor(Constants.AllCountriesTradeGroup)
					.RefCusTradeGroupCountries
					.Where(ctry => !usedCountries.Contains(ctry.ZZB_RN_NKTradeGroupCountryCode))
					.Select(ctry => new TradeGroupCountry
					{
						Code = ctry.ZZB_RN_NKTradeGroupCountryCode,
						Description = ctry.ZZB_Description,
						StartDate = DefaultStartDate,
						EndDate = DefaultEndDate,
					})
					.ToArray();

			var tradeGroup = new TradeGroup
			{
				Code = GetOtherCountriesTradingGroupCode(),
				Description = GetOtherCountriesTradingGroupDescription(),
				StartDate = DefaultStartDate,
				EndDate = DefaultEndDate,
				Countries = otherCountries
			};

			return tradeGroup;
		}

		TradeGroupCountry GetTradeGroupCountry(RefCusTradeGroupCountry country, HsnTariffDTYRateFootnoteRule rule)
		{
			var tradeGroupCountry = new TradeGroupCountry
			{
				Code = country.ZZB_RN_NKTradeGroupCountryCode,
				Description = country.ZZB_Description,
				StartDate = rule.StartDate ?? DefaultStartDate,
				EndDate = rule.EndDate ?? DefaultEndDate,
			};

			return tradeGroupCountry;
		}

		public static string GetTradeGroupCode(HsnTariffDTYRateFootnoteRule rule, bool isExclusion = false)
		{
			var inExSuffix = isExclusion ? "EX" : "IN";
			var code = string.Join(string.Empty, rule.Section, rule.FootnoteCode, rule.AdditionalCode, inExSuffix);

			if (code.Length > 35)
			{
				var section = rule.Section.Substring(0, rule.Section.Length - code.Length - 35);
				code = string.Join(string.Empty, section, rule.FootnoteCode, rule.AdditionalCode, inExSuffix);
			}

			return code.ToSearchKey();
		}

		string GetTradeGroupDescription(HsnTariffDTYRateFootnoteRule rule, bool isExclusion = false)
		{
			var cleanExcelName = Path.GetFileNameWithoutExtension(ListFileName);
			var inExPrefix = isExclusion ? "Excluded" : "Included";

			var desc = $"{inExPrefix} trading groups for {cleanExcelName}, section: {rule.Section}, footnote: {rule.FootnoteCode}";
			if (!string.IsNullOrWhiteSpace(rule.AdditionalCode))
			{
				desc += $", additional code: {rule.AdditionalCode}";
			}

			return desc;
		}

		string GetOtherCountriesTradingGroupCode()
		{
			var cleanExcelName = Path.GetFileNameWithoutExtension(ListFileName);
			cleanExcelName = cleanExcelName.ToSearchKey();
			if (cleanExcelName.Length > 33)
			{
				cleanExcelName = cleanExcelName.Substring(0, 33);
			}

			return $"{cleanExcelName}DU".ToSearchKey();
		}

		string GetOtherCountriesTradingGroupDescription()
		{
			var cleanExcelName = Path.GetFileNameWithoutExtension(ListFileName);

			return $"{cleanExcelName} other countries trading group";
		}

		#endregion

		#region Implementation

		protected Dictionary<string, string> PreferenceCodeAndHeaderPairs
		{
			get
			{
				if (preferenceHeadersAndCodePairs != null)
				{
					return preferenceHeadersAndCodePairs;
				}

				preferenceHeadersAndCodePairs = new Dictionary<string, string>();
				foreach (var preferenceHeader in PreferenceHeaders)
				{
					foreach (var prefCode in preferenceHeader.SplitBy(","))
					{
						preferenceHeadersAndCodePairs[prefCode] = preferenceHeader;
					}
				}

				return preferenceHeadersAndCodePairs;
			}
		}
		Dictionary<string, string> preferenceHeadersAndCodePairs;

		Dictionary<string, HsnTariffDTYRateRule> GetTariffAndExplanations()
		{
			var hsnTariffDTYRateDictionary = new Dictionary<string, HsnTariffDTYRateRule>();

			var workBook = WorkbookFactory.Create(ListFilePath);
			var sheetCount = workBook.NumberOfSheets;

			for (int sheetIndex = 0; sheetIndex < sheetCount; sheetIndex++)
			{
				var worksheet = workBook.GetSheetAt(sheetIndex);

				ReadRateRulesToDictionary(worksheet, hsnTariffDTYRateDictionary);
			}

			return hsnTariffDTYRateDictionary;
		}

		void ReadRateRulesToDictionary(ISheet worksheet, Dictionary<string, HsnTariffDTYRateRule> hsnTariffDTYRateDictionary)
		{
			var tariffsSearchTable = GetTariffSearchTable(worksheet);

			foreach (var searchRow in tariffsSearchTable)
			{
				var tariffCode = searchRow[TariffCodeHeader].ToPureTariffString();
				if (!IsValidTariffCode(tariffCode))
				{
					continue;
				}

				var preferenceRules = GetPreferenceRules(searchRow);

				var sectionName = tariffsSearchTable.SheetName;
				var footnoteRules = GetFootnoteRules(sectionName, searchRow);

				var tariffRule = new HsnTariffDTYRateRule(tariffCode, preferenceRules, footnoteRules);
				hsnTariffDTYRateDictionary.Add(tariffCode, tariffRule);
			}
		}

		HsnTariffDTYRatePreferenceRule[] GetPreferenceRules(Dictionary<string, string> searchRow)
		{
			var preferenceRules = new List<HsnTariffDTYRatePreferenceRule>(PreferenceCodeAndHeaderPairs.Count);
			foreach (var codeAndHeaderPair in PreferenceCodeAndHeaderPairs)
			{
				var rules = PreferenceExplainer.Explain(codeAndHeaderPair.Key,
					searchRow[codeAndHeaderPair.Value.ToSearchKey()]);
				preferenceRules.AddRange(rules);
			}

			return preferenceRules.ToArray();
		}

		HsnTariffDTYRateFootnoteRule[] GetFootnoteRules(string sectionName, Dictionary<string, string> searchRow)
		{
			var footnoteRules = FootnoteExplainer.Explain(
				sectionName,
				searchRow[FootnoteHeader.ToSearchKey()],
				getRelatedCellValue: column => searchRow.TryGetValue(column, out var formulaValue)
					? formulaValue
					: searchRow[PreferenceCodeAndHeaderPairs[column].ToSearchKey()]);

			if (PreferenceContainsFootnote)
			{
				var preferenceFootnotes = GetPreferenceFootnotes(sectionName, searchRow);
				footnoteRules = footnoteRules.Concat(preferenceFootnotes);
			}

			return footnoteRules.ToArray();
		}

		IEnumerable<HsnTariffDTYRateFootnoteRule> GetPreferenceFootnotes(string sectionName, Dictionary<string, string> searchRow)
		{
			var preferenceHeaders = PreferenceHeaders;
			var preferenceFootnotes = preferenceHeaders
				.Select(preferenceHeader => searchRow[preferenceHeader.ToSearchKey()])
				.SelectMany(footnote => FootnoteExplainer.Explain(sectionName, footnote, true))
				.ToArray();
			return preferenceFootnotes;
		}

		NPOISearchTable GetTariffSearchTable(ISheet worksheet)
		{
			return new HsnTariffDTYRatesTable(worksheet, Logger, ListHeaders, SearchHeaders, GetCellFormat);
		}

		#endregion
	}
}
