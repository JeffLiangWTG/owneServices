using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class DutyCalculatorHelperTest : TestCaseWithFactory
	{
		public void TestCalculate()
		{
			var entryLine = Factory.NewWithValidTestData<CusEntryLine>();
			entryLine.CL_CustomsValue = 22m;
			var universalRateCalcData = new EntryLineUniversalRate(entryLine);

			CombineAssertions(() =>
			{
				AssertExceptionThrown<System.ArgumentNullException>("universalRateCalcData should not be null", () => DutyCalculatorHelper.Calculate(null, "VFD * 0.9", 2, "", 0, false, false));
				AssertEquals("rateFormula: VFD * 0.9", 19.80m, DutyCalculatorHelper.Calculate(universalRateCalcData, "VFD * 0.9", 2, "", 0, false, false));
				AssertEquals("rateFormula: VFD * 0.8", 17.60m, DutyCalculatorHelper.Calculate(universalRateCalcData, "VFD * 0.8", 2, "", 0, false, false));
				AssertEquals("dutyDecimalPlaces: 0 => Round(17.6)", 18m, DutyCalculatorHelper.Calculate(universalRateCalcData, "VFD * 0.8", 0, "", 0, false, false));
				AssertEquals("shouldTruncate: true => Math.Truncate(17.6)", 17m, DutyCalculatorHelper.Calculate(universalRateCalcData, "VFD * 0.8", 2, "", 0, true, false));
				AssertEquals("Has formulaSpecificQuestion = (60m)*0.1", 6.00m, DutyCalculatorHelper.Calculate(universalRateCalcData, @"({DECIMAL(6,3):""User Enter Value""})*0.1", 2, "User Enter Value", 60m, false, false));

				entryLine.CL_CustomsValue = -22m;
				universalRateCalcData = new EntryLineUniversalRate(entryLine);
				AssertEquals("canBeNegative : false", 0m, DutyCalculatorHelper.Calculate(universalRateCalcData, "VFD * 0.8", 2, "", 0, false, false));
				AssertEquals("canBeNegative : true", -17.60m, DutyCalculatorHelper.Calculate(universalRateCalcData, "VFD * 0.8", 2, "", 0, false, true));
				AssertEquals("dutyDecimalPlaces: 0 => Round(-17.6)", -18m, DutyCalculatorHelper.Calculate(universalRateCalcData, "VFD * 0.8", 0, "", 0, false, true));
				AssertEquals("shouldTruncate: true => Math.Truncate(-17.6)", -17m, DutyCalculatorHelper.Calculate(universalRateCalcData, "VFD * 0.8", 2, "", 0, true, true));
			});
		}
	}
}
