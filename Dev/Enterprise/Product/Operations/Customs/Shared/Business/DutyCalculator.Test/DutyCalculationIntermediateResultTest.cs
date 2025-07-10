using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DutyCalculator.Testing;

[TestedType(typeof(DutyCalculationIntermediateResult))]
sealed class DutyCalculationIntermediateResultTest : TestCase
{
	public void TestAdjustedRate()
	{
		var testResult = new DutyCalculationIntermediateResult(0m, 1234.5678426m, 0m, "KGM");
		AssertEquals("KGM Method Of Calculation => AdjustedRate", 1234.567843m, testResult.AdjustedRate);

		testResult.MethodOfCalculation = "%" ;
		AssertEquals("% Method Of Calculation => AdjustedRate", 123456.78426m, testResult.AdjustedRate);
	}

	public void TestParticipatingExpression()
	{
		var testResult = new DutyCalculationIntermediateResult(0m, 0m, 0m, ZString.Empty);
		AssertEquals("ParticipatingExpression default value", true, testResult.ParticipatingExpression);

		testResult.ParticipatingExpression = false;
		AssertEquals("ParticipatingExpression value as set", false, testResult.ParticipatingExpression);
	}

	public void TestParticipatingAmount()
	{
		var testResult = new DutyCalculationIntermediateResult(12345.67m, 0m, 0m, "DTN");
		AssertEquals("ParticipatingExpression => Amount", 12345.67m, testResult.Amount);
		AssertEquals("ParticipatingExpression => ParticipatingAmount", testResult.Amount, testResult.ParticipatingAmount);

		testResult.ParticipatingExpression = false;
		AssertEquals("Non-ParticipatingExpression => Amount", 12345.67m, testResult.Amount);
		AssertEquals("Non-ParticipatingExpression => ParticipatingAmount", 0m, testResult.ParticipatingAmount);
	}

	public void TestSetRateAndMethodOfCalculation()
	{
		AssertRateAndMethodOfCalculation(ZString.Empty, 50m, 50m);
		AssertRateAndMethodOfCalculation("KGM", 234.5m, 234.5m);
		AssertRateAndMethodOfCalculation(UniversalReferenceConstants.MethodOfCalculation.Percentage, 75.3m, 0.753m);
	}

	void AssertRateAndMethodOfCalculation(ZString settingMoc, ZDecimal settingAdjustedRate, ZDecimal expectedRate)
	{
		var testResult = new DutyCalculationIntermediateResult(0m, 0m, 0m, ZString.Empty);
		testResult.SetRateAndMethodOfCalculation(settingMoc, settingAdjustedRate);

		CombineAssertions($"MoC = '{settingMoc}', Adjusted Rate = {settingAdjustedRate}", () =>
		{
			AssertEquals("Rate", expectedRate, testResult.Rate);
			AssertEquals("MethodOfCalculation", settingMoc, testResult.MethodOfCalculation);
			AssertEquals("AdjustedRate", settingAdjustedRate, testResult.AdjustedRate);
		});
	}
}
