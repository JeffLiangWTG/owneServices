using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffPopulator;
using NUnit.Framework;

namespace EUNTariffPopulator.Test
{
	[TestFixture]
	public class FormulaExtractorTest
	{
		[TestCase("3.70 %", "VFD * 0.037", "A00")]
		[TestCase("6.000 %", "VFD * 0.060", "A00")]
		[TestCase("0 % ", "0", "A00")]
		[TestCase("0.000 % ", "0", "A00")]
		[TestCase("1.31 EUR / KG/LACTIC MATTER + 22.00 EUR / 100 KG", "1.31 * [KGMP] + 22.00 * [DTN]", "A00")]
		[TestCase("96.90 EUR / 100 kg", "96.90 * [DTN]", "A00")]
		[TestCase("0 EUR / 1000 kg", "0", "A00")]
		[TestCase("1.40 % + 13.30 EUR / 100 kg", "VFD * 0.014 + 13.30 * [DTN]", "A00")]
		[TestCase("134.00 EUR / 1000 kg", "134.00 * [TNE]", "A00")]
		[TestCase("12.000 % MIN 0.400 EUR DTN", "MAX(VFD * 0.120, 0.400 * [DTN])", "A00")]
		public void SingleFormulaExtractorTester(string input, string rateFormula, string ratecode)
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			Assert.AreEqual(new List<Tuple<string, string>>() { new Tuple<string, string>(rateFormula, ratecode) }, FormulaExtractor.GetFormula(TransformWebFormulaToSpreadsheetStandard.GetSpreadsheetStandardFormula(input)));
		}

		[Test]
		public void SingleFormulaFromConditionExtractorTester()
		{
			const string inputFormula = "Cond:  V 112.60 EUR / 100 kg:8.80 %;V 110.30 EUR / 100 kg:8.80 % + 2.30 EUR / 100 kg;V 108.10 EUR / 100 kg:8.80 % + 4.50 EUR / 100 kg;V 105.80 EUR / 100 kg:8.80 % + 6.80 EUR / 100 kg;V 103.60 EUR / 100 kg:8.80 % + 9.00 EUR / 100 kg;V 0 EUR / 100 kg:8.80 % + 29.80 EUR / 100 kg";
			var expectedValues = new List<Tuple<string, string>>() { new Tuple<string, string>("VFD * 0.088 + if (VFD/[DTN] > 112.60,0,(if (VFD/[DTN] > 110.30,2.30 * [DTN],(if (VFD/[DTN] > 108.10,4.50 * [DTN],(if (VFD/[DTN] > 105.80,6.80 * [DTN],(if (VFD/[DTN] > 103.60,9.00 * [DTN],(29.80 * [DTN]))))))))))", "A00") };
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			Assert.AreEqual(expectedValues, FormulaExtractor.GetConditionRateFormula(TransformWebFormulaToSpreadsheetStandard.GetSpreadsheetStandardFormula(inputFormula)));
		}

		[Test]
		public void MultipleFormulaExtractorTester()
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			Assert.AreEqual(FormulaExtractor.GetFormula("9.00 % + EA(1)"), new List<Tuple<string, string>>()
			{
				new Tuple<string, string>("VFD * 0.09", "A00")
				, new Tuple<string, string>("[EA(1)]", "A20")
			});
			Assert.AreEqual(FormulaExtractor.GetFormula("0 % + EA(1) MAX 18.70 % +ADSZ(1)"), new List<Tuple<string, string>>()
			{
				new Tuple<string, string>("0", "A00")
				, new Tuple<string, string>("MIN([EA(1)], VFD * 0.187)", "A20")
				, new Tuple<string, string>("[ADSZ(1)]", "A20")
			});
		}
	}
}
