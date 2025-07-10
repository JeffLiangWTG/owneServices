using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;
using System;
using System.Globalization;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Loaders
{
	public abstract class HsnTariffDTYRateExplainer
	{
		protected HsnTariffDTYRateExplainer(string filePath, ILogger logger)
		{
			var sheet = GetSheet(filePath);
			Logger = Argument.NotNull(logger, nameof(logger));
			SearchTable = GetSearchTable(sheet);
		}

		protected ILogger Logger { get; }

		protected NPOISearchTable SearchTable { get; }

		protected abstract string SheetName { get; }

		NPOISearchTable GetSearchTable(ISheet sheet) => GetSearchTableCore(sheet);

		protected abstract NPOISearchTable GetSearchTableCore(ISheet sheet);

		ISheet GetSheet(string filePath)
		{
			return WorkbookFactory.Create(filePath).GetSheet(SheetName);
		}

		public static (string Value, string[] Footnotes) GetValueAndFootnotes(string cellValue, bool isPreference = false)
		{
			if (!isPreference)
			{
				return (null, GetNumbers(cellValue));
			}

			if (PreferenceFootnoteRegex.Match(cellValue) is Match match && match.Success)
			{
				return (match.Groups[1].Value, GetNumbers(match.Groups[2].Value));
			}

			return (cellValue, Array.Empty<string>());
		}

		public static string GetFormula(string formulaTemplate, string formulaValue, Func<string, string> getRelatedCellValue = null)
		{
			if (decimal.TryParse(formulaValue, out var result) && result == 0)
			{
				return decimal.Zero.ToString(CultureInfo.InvariantCulture);
			}

			var formulaRelatedCell = FormulaRelatedCellRegex.Match(formulaTemplate);
			if (formulaRelatedCell.Success)
			{
				var relatedCellPart = RelatedColumnRegex.Match(formulaRelatedCell.Groups[1].Value);
				if (relatedCellPart.Success)
				{
					var relatedCell = relatedCellPart.Groups[1].Value;
					formulaValue = getRelatedCellValue(relatedCell);
				}
				return FormulaRelatedCellRegex.Replace(formulaTemplate, formulaValue);
			}
			return formulaTemplate;
		}

		static readonly Regex FormulaRelatedCellRegex = new Regex(@"\W?number\s+in\s+the\s+((\w+\s)+)cell\W?");

		static readonly Regex RelatedColumnRegex = new Regex(@"(\w+)\s+column");

		static readonly Regex PreferenceFootnoteRegex = new Regex(@"(.*?)((\(\d{1,2}\))+)$");

		static readonly Regex FootnoteCodeSeparatorRegex = new Regex(@"\W+");

		static string[] GetNumbers(string input) => FootnoteCodeSeparatorRegex.Split(input).Where(split => !string.IsNullOrWhiteSpace(split)).ToArray<string>();
	}
}
