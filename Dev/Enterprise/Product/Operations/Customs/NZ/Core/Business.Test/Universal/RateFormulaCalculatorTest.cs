using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators;
using Moq;

namespace Enterprise.Customs.NZ.Business.Testing
{
	sealed class RateFormulaCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculateDuty()
		{
			var dutyRateParametersMock = new Mock<IHaveTheParametersRequiredToCalculateDuty>();
			dutyRateParametersMock.Setup(x => x.DateForDutyRate).Returns(ZDateTime.Today);
			dutyRateParametersMock.Setup(x => x.ValueForDuty).Returns(20M);
			dutyRateParametersMock.Setup(x => x.StatUQ).Returns("KG");
			dutyRateParametersMock.Setup(x => x.StatQty).Returns(5M);
			var visitor = new RateFormulaCalculativeVisitor(new Universal.UniversalRateCalcDataWrapper(new RateCalcData(dutyRateParametersMock.Object), new Universal.FormulaErrorListener()));

			var formula = "0.700240*[KG]";
			var dutyAmount = RateFormulaCalculator.CalculateDuty(visitor, formula, omitVFDCalculation: false, omitUOMCalculation: false);
			AssertCalculateResult(3.5012M, dutyAmount, 0M, visitor.VFDRate, 0.700240M, visitor.UOMRate, "KG", visitor.UOMCode, formula);

			formula = "5.000000*(VFD/100)";
			dutyAmount = RateFormulaCalculator.CalculateDuty(visitor, formula, omitVFDCalculation: false, omitUOMCalculation: false);
			AssertCalculateResult(1M, dutyAmount, 5M, visitor.VFDRate, 0M, visitor.UOMRate, "", visitor.UOMCode, formula);

			formula = "(5.000000*(VFD/100))+(0.700240*[KG])";
			dutyAmount = RateFormulaCalculator.CalculateDuty(visitor, formula, omitVFDCalculation: false, omitUOMCalculation: false);
			AssertCalculateResult(4.5012M, dutyAmount, 5M, visitor.VFDRate, 0.700240M, visitor.UOMRate, "KG", visitor.UOMCode, formula);

			dutyAmount = RateFormulaCalculator.CalculateDuty(visitor, formula, omitVFDCalculation: false, omitUOMCalculation: true);
			AssertCalculateResult(1M, dutyAmount, 5M, visitor.VFDRate, 0M, visitor.UOMRate, "KG", visitor.UOMCode, formula);

			dutyAmount = RateFormulaCalculator.CalculateDuty(visitor, formula, omitVFDCalculation: true, omitUOMCalculation: false);
			AssertCalculateResult(3.5012M, dutyAmount, 0M, visitor.VFDRate, 0.700240M, visitor.UOMRate, "KG", visitor.UOMCode, formula);

			dutyAmount = RateFormulaCalculator.CalculateDuty(visitor, formula, omitVFDCalculation: true, omitUOMCalculation: true);
			AssertCalculateResult(0M, dutyAmount, 0M, visitor.VFDRate, 0M, visitor.UOMRate, "KG", visitor.UOMCode, formula);

			formula = "(0.700240*[KG])-(5.000000*(VFD/100))";
			dutyAmount = RateFormulaCalculator.CalculateDuty(visitor, formula, omitVFDCalculation: false, omitUOMCalculation: false);
			AssertCalculateResult(2.5012M, dutyAmount, 5M, visitor.VFDRate, 0.700240M, visitor.UOMRate, "KG", visitor.UOMCode, formula);

			formula = "0.5*CU1";
			dutyAmount = RateFormulaCalculator.CalculateDuty(visitor, formula, omitVFDCalculation: false, omitUOMCalculation: false);
			AssertCalculateResult(2.5M, dutyAmount, 0M, visitor.VFDRate, 0M, visitor.UOMRate, string.Empty, visitor.UOMCode, formula);
		}

		void AssertCalculateResult(decimal expectedDutyAmout, decimal actualDutyAmout, decimal expectedVFDRate, decimal actualVFDRate, decimal expectedUOMRate, decimal actualUOMRate, string expectedUOMCode, string actualUOMCode, string formula)
		{
			CombineAssertions(formula, () =>
			{
				AssertEquals("Duty amount:", expectedDutyAmout, actualDutyAmout);
				AssertEquals("VFD Rate:", expectedVFDRate, actualVFDRate);
				AssertEquals("UOM Rate:", expectedUOMRate, actualUOMRate);
				AssertEquals("UOM Code:", expectedUOMCode, actualUOMCode);
			});
		}
	}
}
