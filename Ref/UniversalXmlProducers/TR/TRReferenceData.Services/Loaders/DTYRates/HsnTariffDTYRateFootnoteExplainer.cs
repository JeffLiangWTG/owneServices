using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.TRReferenceData.Services.Common;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Loaders
{
	public class HsnTariffDTYRateFootnoteExplainer : HsnTariffDTYRateExplainer
	{
		public HsnTariffDTYRateFootnoteExplainer(string filePath, ILogger logger)
			: base(filePath, logger)
		{ }

		protected override string SheetName => DtyRateConstants.FootnotesSheetName;

		protected override NPOISearchTable GetSearchTableCore(ISheet sheet) => new Table(sheet, Logger);

		public IEnumerable<HsnTariffDTYRateFootnoteRule> AllRules
		{
			get
			{
				return SearchTable.Find(Table.Headers.TableHeaders.ToArray<string>())
					.Select(row => GetFootnoteRule(row, string.Empty, a => a));
			}
		}

		public IEnumerable<HsnTariffDTYRateFootnoteRule> Explain(string section, string cellValue, bool isPreference = false, Func<string, string> getRelatedCellValue = null)
		{
			var preferenceAndFootnotes = GetValueAndFootnotes(cellValue.ReplaceLineBreaks(), isPreference);

			foreach (var footnote in preferenceAndFootnotes.Footnotes)
			{
				var footnoteRows = SearchTable.Find(
					Table.Headers.TableHeaders.ToArray<string>(),
					(Table.Headers.Section, section),
					(Table.Headers.Footnote, footnote)
				);

				foreach (var footnoteRow in footnoteRows)
				{
					yield return GetFootnoteRule(footnoteRow, preferenceAndFootnotes.Value, getRelatedCellValue);
				}
			}
		}

		static HsnTariffDTYRateFootnoteRule GetFootnoteRule(string[] footnoteRow, string formulaValue, Func<string, string> getRelatedCellValue = null)
		{
			return new HsnTariffDTYRateFootnoteRule(
				section: footnoteRow[0],
				excludingTradingPartners: footnoteRow[1],
				tradingPartners: footnoteRow[2],
				additionalCode: footnoteRow[3],
				footnoteCode: footnoteRow[4],
				rateType: footnoteRow[5],
				rateCode: footnoteRow[6],
				formula: GetFormula(footnoteRow[7], formulaValue, getRelatedCellValue),
				startDate: DateTimeHelper.ParseDate(footnoteRow[8]),
				endDate: DateTimeHelper.ParseDate(footnoteRow[9])
			);
		}

		class Table : NPOISearchTable
		{
			public static class Headers
			{
				public const string Section = "SECTION";
				public const string ExcludingCountryGroup = "Excluding Country/Grouping";
				public const string CountryAndGroup = "Country/Grouping";
				public const string AdditionalCode = "Additional Code";
				public const string Footnote = "Footnote";
				public const string Formula = "Formula";
				public const string RateType = "ZZR_RateType";
				public const string RateCode = "ZY1_RateCode";
				public const string StartDate = "Start Date";
				public const string EndDate = "End Date";

				public static HashSet<string> TableHeaders => new HashSet<string>
				{
					Section,
					ExcludingCountryGroup,
					CountryAndGroup,
					AdditionalCode,
					Footnote,
					RateType,
					RateCode,
					Formula,
					StartDate,
					EndDate,
				};

				public static HashSet<string> SearchHeaders => new HashSet<string> { Section, Footnote };
			}

			public Table(ISheet sheet, ILogger logger) : base(sheet, logger, Headers.TableHeaders, Headers.SearchHeaders) { }
		}
	}

	public class HsnTariffDTYRateFootnoteRule
	{
		public HsnTariffDTYRateFootnoteRule(string section,
			string footnoteCode,
			string excludingTradingPartners,
			string tradingPartners,
			string additionalCode,
			string formula,
			string rateType,
			string rateCode,
			DateTime? startDate,
			DateTime? endDate)
		{
			Section = section;
			FootnoteCode = footnoteCode;
			ExcludingTradingPartners = HsnTariffDTYRateTradingPartner.From(excludingTradingPartners);
			TradingPartners = HsnTariffDTYRateTradingPartner.From(tradingPartners);
			AdditionalCode = additionalCode;
			Formula = formula.ClearWhiteSpaces();
			RateType = rateType;
			RateCode = rateCode;
			StartDate = startDate;
			EndDate = endDate;
		}

		public string Section { get; set; }
		public string FootnoteCode { get; }
		public IEnumerable<HsnTariffDTYRateTradingPartner> ExcludingTradingPartners { get; }
		public IEnumerable<HsnTariffDTYRateTradingPartner> TradingPartners { get; }
		public string AdditionalCode { get; }
		public string Formula { get; }
		public string RateType { get; }
		public string RateCode { get; }
		public DateTime? StartDate { get; }
		public DateTime? EndDate { get; }

		public IEnumerable<HsnTariffDTYRateTradingPartner> ExcludingTradingPartnersOverride { get; set; }

		public IEnumerable<HsnTariffDTYRateTradingPartner> TradingPartnersOverride { get; set; }
	}
}
