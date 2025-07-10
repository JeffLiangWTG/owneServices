using System.Linq;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	public class FormulaExtractorFixture
	{
		[TestCase("3.70 %", "VFD * 0.037")]
		[TestCase("6.000 %", "VFD * 0.060")]
		[TestCase("0 % ", "0")]
		[TestCase("0.000 % ", "0")]
		[TestCase("1.31 EUR KGM P + 22.00 EUR DTN", "1.31 * [KGMP] + 22.00 * [DTN]")]
		[TestCase("96.90 EUR DTN", "96.90 * [DTN]")]
		[TestCase("0 EUR DTN", "0")]
		[TestCase("1.40 % + 13.30 EUR DTN", "VFD * 0.014 + 13.30 * [DTN]")]
		[TestCase("134.00 EUR TNE", "134.00 * [TNE]")]
		[TestCase("12.000 % MIN 0.400 EUR DTN", "MAX(VFD * 0.120, 0.400 * [DTN])")]
		[TestCase("10.000 % MIN 22.000 EUR DTN MAX 56.000 EUR DTN", "MIN(MAX(VFD * 0.100, 22.000 * [DTN]), 56.000 * [DTN])")]
		[TestCase("12.000 % MIN 2.000 EUR DTN G", "MAX(VFD * 0.120, 2.000 * [DTNG])")]
		[TestCase("3.800 % + 7.050 EUR DTN", "VFD * 0.038 + 7.050 * [DTN]")]
		[TestCase("NIHIL", "0")]
		[TestCase("28 EUR NAR", "28 * [NAR]")]
		[TestCase("0.000 % + 20.200 EUR DTN MAX 19.400 % + 9.400 EUR DTN", "MIN(20.200 * [DTN], VFD * 0.194 + 9.400 * [DTN])")]
		[TestCase("4.500 % + 22.550 EUR DTN MAX 9.400 % + 8.250 EUR DTN MAX 35.150 EUR DTN", "MIN(MIN(VFD * 0.045 + 22.550 * [DTN], VFD * 0.094 + 8.250 * [DTN]), 35.150 * [DTN])")]
		[TestCase("0.000 % + 37.220 EUR DTN MAX 18.100 % + 7.000 EUR DTN", "MIN(37.220 * [DTN], VFD * 0.181 + 7.000 * [DTN])")]
		[TestCase("2,765.000 EUR TNE ", "2765.000 * [TNE]")]
		public void NonConditionalRateFormula(string rawRateFormula, string expectedRateFormula)
		{
			const string measureTypeId = "103";
			const string reductionIndicator = "";
			var formulaExtractor = new FormulaExtractor(new MeasuringUnitTransformer(), new Mock<IFormulaExtractor>().Object, new Mock<IConditionFormulaExtractor>().Object);

			var result = formulaExtractor.GetFormula(rawRateFormula, measureTypeId, reductionIndicator);

			Assert.IsNotNull(result);
			Assert.IsNotNull(result.First().Formula);
			Assert.AreEqual(expectedRateFormula, result.First().Formula);
		}
	}
}
