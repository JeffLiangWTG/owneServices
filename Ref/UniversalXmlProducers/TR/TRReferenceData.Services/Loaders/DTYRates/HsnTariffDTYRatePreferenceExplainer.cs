using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.TRReferenceData.Services.Common;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Loaders
{
	public class HsnTariffDTYRatePreferenceExplainer : HsnTariffDTYRateExplainer
	{
		public HsnTariffDTYRatePreferenceExplainer(string filePath, ILogger logger)
			: base(filePath, logger)
		{ }

		protected override string SheetName => DtyRateConstants.PreferencesSheetName;

		protected override NPOISearchTable GetSearchTableCore(ISheet sheet) => new Table(sheet, Logger);

		public IEnumerable<HsnTariffDTYRatePreferenceRule> AllRules
		{
			get
			{
				return SearchTable.Find(Table.Headers.TableHeaders.ToArray<string>())
					.SelectMany(row => GetPreferenceRule(row, string.Empty));
			}
		}

		public IEnumerable<HsnTariffDTYRatePreferenceRule> Explain(string codeInExcel, string formulaValue)
		{
			var searchRows = SearchTable.Find(
				Table.Headers.TableHeaders.ToArray<string>(),
				(Table.Headers.CodeInExcel, codeInExcel)
			);
			if (!searchRows.Any())
			{
				Logger.LogError("Unable to find a rule for Code In Excel: {}", codeInExcel);
				return null;
			}

			var searchRow = searchRows.Single();

			return GetPreferenceRule(searchRow, formulaValue);
		}

		static IEnumerable<HsnTariffDTYRatePreferenceRule> GetPreferenceRule(string[] row, string formulaValue)
		{
			var formulaCellText = row[2];
			var formulaAndFootnotes = GetValueAndFootnotes(formulaCellText, true);
			var formula = GetFormula(formulaAndFootnotes.Value, formulaValue);

			var preferenceCellText = row[1];
			var preferences = preferenceCellText.SplitBy();

			return preferences.Select(preference => new HsnTariffDTYRatePreferenceRule(row[0],
				preference,
				formula,
				row[3],
				row[4],
				DateTimeHelper.ParseDate(row[5]),
				DateTimeHelper.ParseDate(row[6]),
				formulaAndFootnotes.Footnotes));
		}

		class Table : NPOISearchTable
		{
			public static class Headers
			{
				public const string CodeInExcel = "Code in Excel";
				public const string ZZS_Preference = "[ZZS_Preference]/Trade Group";
				public const string Formula = "Formula";
				public const string RateType = "ZZR_RateType";
				public const string RateCode = "ZY1_RateCode";
				public const string StartDate = "Start Date";
				public const string EndDate = "End Date";

				public static HashSet<string> TableHeaders => new HashSet<string>
				{
					CodeInExcel,
					ZZS_Preference,
					Formula,
					RateType,
					RateCode,
					StartDate,
					EndDate,
				};

				public static HashSet<string> SearchHeaders => new HashSet<string> { CodeInExcel, ZZS_Preference };
			}

			public Table(ISheet sheet, ILogger logger) : base(sheet, logger, Headers.TableHeaders, Headers.SearchHeaders) { }
		}
	}

	public class HsnTariffDTYRatePreferenceRule
	{
		public HsnTariffDTYRatePreferenceRule(string codeInExcel,
			string preference,
			string formula,
			string rateType,
			string rateCode,
			DateTime? startDate,
			DateTime? endDate,
			IEnumerable<string> footnotes)
		{
			CodeInExcel = codeInExcel;
			Preference = preference;
			Formula = formula.ClearWhiteSpaces();
			RateType = rateType;
			RateCode = rateCode;
			StartDate = startDate;
			EndDate = endDate;
			Footnotes = footnotes;
		}

		public string CodeInExcel { get; }

		public string Preference { get; }

		public string Formula { get; }

		public string RateType { get; }

		public string RateCode { get; }

		public DateTime? StartDate { get; set; }

		public DateTime? EndDate { get; set; }

		public IEnumerable<string> Footnotes { get; }
	}
}
