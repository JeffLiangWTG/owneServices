using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	public class ConditionFormulaExtractorFixture
	{
		[TestCase(@"Cond:  V 2,112.60 EUR DTN:8.80 %;V 110.30 EUR DTN:8.80 % + 2.30 EUR DTN;V 108.10 EUR DTN:8.80 % + 4.50 EUR DTN;V 105.80 EUR DTN:8.80 % + 6.80 EUR DTN;V 103.60 EUR DTN:8.80 % + 9.00 EUR DTN;V 0 EUR DTN:8.80 % + 29.80 EUR DTN"
			, @"If(VFD/[DTN] >= 2112.60, VFD * 0.088, If(VFD/[DTN] >= 110.30, VFD * 0.088 + 2.30 * [DTN], If(VFD/[DTN] >= 108.10, VFD * 0.088 + 4.50 * [DTN], If(VFD/[DTN] >= 105.80, VFD * 0.088 + 6.80 * [DTN], If(VFD/[DTN] >= 103.60, VFD * 0.088 + 9.00 * [DTN], VFD * 0.088 + 29.80 * [DTN])))))"
			, "A00"
			, TestName = "Rate Formula 1")]
		[TestCase(@"Cond:  V 45.700 EUR/DTN(01):9.000 % ; V 44.800 EUR/DTN(01):11.200 % + 0.900 EUR DTN ; V 43.900 EUR/DTN(01):11.200 % + 1.800 EUR DTN ; V 43.000 EUR/DTN(01):11.200 % + 2.700 EUR DTN ; V 42.000 EUR/DTN(01):11.200 % + 3.700 EUR DTN ; V 0.000 EUR/DTN(01):11.200 % + 23.800 EUR DTN"
			, @"If(VFD/[DTN] >= 45.700, VFD * 0.090, If(VFD/[DTN] >= 44.800, VFD * 0.112 + 0.900 * [DTN], If(VFD/[DTN] >= 43.900, VFD * 0.112 + 1.800 * [DTN], If(VFD/[DTN] >= 43.000, VFD * 0.112 + 2.700 * [DTN], If(VFD/[DTN] >= 42.000, VFD * 0.112 + 3.700 * [DTN], VFD * 0.112 + 23.800 * [DTN])))))"
			, "A00"
			, TestName = "Rate Formula 2")]
		[TestCase(@"Cond: B cert: Y - 086(27):; B(07):; V 72.600 EUR / DTN(01):0.000 % ; V 71.100 EUR / DTN(01):0.000 % + 1.500 EUR DTN ; V 69.700 EUR / DTN(01):0.000 % + 2.900 EUR DTN ; V 68.200 EUR / DTN(01):0.000 % + 4.400 EUR DTN ; V 66.800 EUR / DTN(01):0.000 % + 5.800 EUR DTN ; V 0.000 EUR / DTN(01):0.000 % + 29.800 EUR DTN "
			, @"If(VFD/[DTN] >= 72.600, 0, If(VFD/[DTN] >= 71.100, 1.500 * [DTN], If(VFD/[DTN] >= 69.700, 2.900 * [DTN], If(VFD/[DTN] >= 68.200, 4.400 * [DTN], If(VFD/[DTN] >= 66.800, 5.800 * [DTN], 29.800 * [DTN])))))"
			, "A00"
			, TestName = "Rate Formula 3")]
		[TestCase(@"Cond:  B cert: N-990 (27):; B (08):; V 48.100 EUR/DTN(01):0.000 % ; V 47.100 EUR/DTN(01):0.000 % ; V 46.200 EUR/DTN(01):0.000 % ; V 3,445.200 EUR/DTN(01):0.000 % ; V 44.300 EUR/DTN(01):0.000 % ; V 35.000 EUR/DTN(01):0.000 % ; V 34.300 EUR/DTN(01):0.000 % + 0.700 EUR DTN ; V 33.600 EUR/DTN(01):0.000 % + 1.400 EUR DTN ; V 32.900 EUR/DTN(01):0.000 % + 2.100 EUR DTN ; V 32.200 EUR/DTN(01):0.000 % + 2.800 EUR DTN ; V 0.000 EUR/DTN(01):0.000 % + 37.800 EUR DTN "
			, @"If(VFD/[DTN] >= 48.100, 0, If(VFD/[DTN] >= 47.100, 0, If(VFD/[DTN] >= 46.200, 0, If(VFD/[DTN] >= 3445.200, 0, If(VFD/[DTN] >= 44.300, 0, If(VFD/[DTN] >= 35.000, 0, If(VFD/[DTN] >= 34.300, 0.700 * [DTN], If(VFD/[DTN] >= 33.600, 1.400 * [DTN], If(VFD/[DTN] >= 32.900, 2.100 * [DTN], If(VFD/[DTN] >= 32.200, 2.800 * [DTN], 37.800 * [DTN]))))))))))"
			, "A00"
			, TestName = "Rate Formula 4")]
		[TestCase(@"Cond:  A cert: D-008 (01):39.900 % ; A (01):54.900 %",
			@"If(has(""CERT"",""D008""),VFD * 0.399,VFD * 0.549)"
			, "A30"
			, TestName = "Rate Formula 5")]
		[TestCase(@"Cond:  A cert: D-017 (01):0.000 % ; A cert: D-018 (01):36.200 % ; A (01):36.200 % ",
			@"If(has(""CERT"",""D017""),0,If(has(""CERT"",""D018""),VFD * 0.362,VFD * 0.362))"
			, "A30"
			, TestName = "Rate Formula 6")]
		[TestCase(@"Cond:  A cert: D-020 (28):; A (08):; F 1,873.000 EUR/TNE(01):0.000 EUR TNE ;F 1,378.220 EUR/TNE(11):1,873.000 EUR TNE ; F 0.000 EUR/TNE(01):35.900 % ",
			@"If(VFD/[TNE] > 1873.000,0.000,(If(VFD/[TNE] < 1378.220,VFD * 0.359,(1873.000 - VFD/[TNE]) * [TNE])))"
			, "A30"
			, TestName = "Rate Formula 7")]
		[TestCase(@"Cond:  A cert: D-008 (01):68.600 EUR TNE I ; A (01):172.200 EUR TNE I",
			@"If(has(""CERT"",""D008""),68.600 * [TNEI],172.200 * [TNEI])",
			"A30"
			, TestName = "Rate Formula 8")]
		[TestCase("Cond:  F 325.000 EUR/MIL(01):0.000 EUR MIL ; F 0.000 EUR/MIL(11):325.000 EUR MIL",
			"If(VFD/[MIL] > 325.000,0.000,(325.000 - VFD/[MIL]) * [MIL])",
			"A30"
			, TestName = "Rate Formula 9")]
		[TestCase(@"Cond:  L 118.080 EUR/DTN(01):0.000 EUR DTN ; L 78.720 EUR/DTN(01):35.424 EUR DTN - 0.000 % ; L 52.480 EUR/DTN(01):51.168 EUR DTN - 50.000 % ; L 32.800 EUR/DTN(01):61.664 EUR DTN - 70.000 % ; L 0.000 EUR/DTN(01):68.224 EUR DTN - 90.000 %"
			, @"If(CIF/[DTN] >= 118.080, 0, If(CIF/[DTN] >= 78.720, 35.424 * [DTN]- CIF * 0.000, If(CIF/[DTN] >= 52.480, 51.168 * [DTN]- CIF * 0.500, If(CIF/[DTN] >= 32.800, 61.664 * [DTN]- CIF * 0.700, 68.224 * [DTN]- CIF * 0.900))))"
			, "A00"
			, TestName = "Rate Formula 10")]
		[TestCase(@"Cond:  V 42.500 EUR/HLT(01):0.000 % + 20.600 EUR DTN ; V 41.700 EUR/HLT(01):0.000 % + 0.800 EUR HLT + 20.600 EUR DTN ; V 40.800 EUR/HLT(01):0.000 % + 1.700 EUR HLT + 20.600 EUR DTN ; V 40.000 EUR/HLT(01):0.000 % + 2.500 EUR HLT + 20.600 EUR DTN ; V 39.100 EUR/HLT(01):0.000 % + 3.400 EUR HLT + 20.600 EUR DTN ; V 0.000 EUR/HLT(01):0.000 % + 27.000 EUR HLT + 20.600 EUR DTN"
			, @"If(VFD/[HLT] >= 42.500, 20.600 * [DTN], If(VFD/[HLT] >= 41.700, 0.800 * [HLT] + 20.600 * [DTN], If(VFD/[HLT] >= 40.800, 1.700 * [HLT] + 20.600 * [DTN], If(VFD/[HLT] >= 40.000, 2.500 * [HLT] + 20.600 * [DTN], If(VFD/[HLT] >= 39.100, 3.400 * [HLT] + 20.600 * [DTN], 27.000 * [HLT] + 20.600 * [DTN])))))"
			, "A00"
			, TestName = "Rate Formula 11")]
		[TestCase(@"Cond:  A cert: D-008 (01):0.075 ENC ENP ; A (01):0.192 ENC ENP"
			, @"If(has(""CERT"",""D008""),0.075 * [ENP],0.192 * [ENP])"
			, "A00"
			, TestName = "Rate Formula 12")]
		[TestCase("Cond:  A cert: D-008 (34):; A (14):; F 1,346.000 EUR/TNE(01):0.000 EUR TNE ; F 0.000 EUR/TNE(11):1,346.000 EUR TNE"
			, "If(VFD/[TNE] > 1346.000,0.000,(1346.000 - VFD/[TNE]) * [TNE])"
			, "A30"
			, TestName = "Rate Formula 13")]
		public void ConditionalRateFormula(string rawRateFormula, string expectedRateFormula, string rateCode)
		{
			var conditionFormulaExtractor = new ConditionFormulaExtractor(new MeasuringUnitTransformer());

			var result = conditionFormulaExtractor.GetFormula(rawRateFormula, rateCode);
			Assert.AreEqual(expectedRateFormula, result.Formula);
		}
	}
}
