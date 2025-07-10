using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffPopulator
{
	public static class TransformWebFormulaToSpreadsheetStandard
	{
		public static string GetSpreadsheetStandardFormula(string formula)
		{
			Argument.NotNull(formula, nameof(formula));
			formula = formula.ToUpper();
			formula = new Regex(DTNRegex).Replace(formula, " DTN ");
			formula = new Regex(EUCRegex).Replace(formula, " EUC ");
			formula = new Regex(KGMPRegex).Replace(formula, " KGM P ");
			formula = new Regex(TNERegex).Replace(formula, " TNE ");
			return formula.Trim();
		}

		static string DTNRegex = @"\s?\/\s?100\s?KG";
		static string TNERegex = @"\s?\/\s?1000\s?KG\s?";
		static string KGMPRegex = @"\s?\/\sKG/LACTIC MATTER\s?";
		static string EUCRegex = @"\s?EURUP\s?";
	}
}
