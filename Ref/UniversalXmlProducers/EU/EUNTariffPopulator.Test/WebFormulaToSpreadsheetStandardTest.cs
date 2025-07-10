using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffPopulator.Test
{
	[TestFixture]
	public class WebFormulaToSpreadsheetStandardTest
	{
		[TestCase("1.19 EUR / kg/lactic matter + 27.50 EUR / 100 kg", "1.19 EUR KGM P + 27.50 EUR DTN")]
		[TestCase("16.00 % + 582.00 EUR / 1000 kg", "16.00 % + 582.00 EUR TNE")]
		[TestCase("417.53 EurUP / 100 kg", "417.53 EUC DTN")]
		public void GetSpreadsheetStandardFormula(string input, string expectedOutput)
		{
			Assert.AreEqual(expectedOutput, TransformWebFormulaToSpreadsheetStandard.GetSpreadsheetStandardFormula(input));
		}
	}
}
