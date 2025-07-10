using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.DutyCalculator.Testing;

public abstract class RateCalculationVisitorTestBase : TestCase
{
	public static void AssertCalculation(string assertionMessage, IDutyCalculationResult calculationResult, decimal expectedFinalResultAmount, IDutyCalculationIntermediateResult[] expectedIntermediateResults, ErrorInformation[] expectedErrors = null)
	{
		AssertNotNull(assertionMessage, calculationResult);

		CombineAssertions(assertionMessage, () =>
		{
			AssertEquals("Final Result Amount", expectedFinalResultAmount, calculationResult.ResultAmount);
			AssertIntermediateResults(expectedIntermediateResults, calculationResult.IntermediateResults.ToArray());
			AssertCalculationErrors(expectedErrors ?? Array.Empty<ErrorInformation>(), calculationResult.Errors.ToArray());
		});
	}

	public static void AssertIntermediateResults(IDutyCalculationIntermediateResult[] expectedIntermediateResults, IDutyCalculationIntermediateResult[] actualIntermediateResults)
	{
		AssertEquals("Intermediate Result count", expectedIntermediateResults.Length, actualIntermediateResults.Length);

		for (int i = 0; i < Math.Min(expectedIntermediateResults.Length, actualIntermediateResults.Length); i++)
		{
			AssertEquals($"IntermediateResult[{i}].Amount", expectedIntermediateResults[i].Amount, actualIntermediateResults[i].Amount);
			AssertEquals($"IntermediateResult[{i}].Rate", expectedIntermediateResults[i].Rate, actualIntermediateResults[i].Rate);
			AssertEquals($"IntermediateResult[{i}].BaseValue", expectedIntermediateResults[i].BaseValue, actualIntermediateResults[i].BaseValue);
			AssertEquals($"IntermediateResult[{i}].MethodOfCalculation", expectedIntermediateResults[i].MethodOfCalculation, actualIntermediateResults[i].MethodOfCalculation);
			AssertEquals($"IntermediateResult[{i}].AdjustedRate", expectedIntermediateResults[i].AdjustedRate, actualIntermediateResults[i].AdjustedRate);
			AssertEquals($"IntermediateResult[{i}].ParticipatingExpression", expectedIntermediateResults[i].ParticipatingExpression, actualIntermediateResults[i].ParticipatingExpression);
			AssertEquals($"IntermediateResult[{i}].ParticipatingAmount", expectedIntermediateResults[i].ParticipatingAmount, actualIntermediateResults[i].ParticipatingAmount);
			AssertResultCalculatedProperties(actualIntermediateResults[i]);
		}
	}

	public static void AssertCalculationErrors(ErrorInformation[] expectedErrors, ErrorInformation[] actualErrors)
	{
		AssertEquals("Calculation Error count", expectedErrors.Length, actualErrors.Length);

		for (int i = 0; i < Math.Min(expectedErrors.Length, actualErrors.Length); i++)
		{
			AssertEquals($"Errors[{i}]", expectedErrors[i], actualErrors[i]);
		}
	}

	protected void CalculateAndAssert(string formulaForTesting,
		decimal expectedFinalResultAmount,
		IDutyCalculationIntermediateResult[] expectedResults,
		RateCalcDataForTesting testingData,
		IRateCalculationVisitorCreator formulaVisitorCreator,
		ErrorInformation[] expectedErrors = null)
	{
		var assertionMessage = $"Calculation Result\r\nFormula = {formulaForTesting}\r\nRateCalcData = {testingData}\r\n";
		var calculationResult = RateCalculationVisitor.CalculateDuties(testingData, formulaForTesting, formulaVisitorCreator);
		AssertCalculation(assertionMessage, calculationResult, expectedFinalResultAmount, GetApplicableExpectedResults(expectedResults), expectedErrors);
	}

	protected virtual RateCalcDataForTesting GetTestingData() => new RateCalcDataForTesting();

	protected abstract IDutyCalculationIntermediateResult[] GetApplicableExpectedResults(IDutyCalculationIntermediateResult[] allExpressionResults);

	static void AssertResultCalculatedProperties(IDutyCalculationIntermediateResult intermediateResult)
	{
		var exactExpectedRate = (intermediateResult.MethodOfCalculation == UniversalReferenceConstants.MethodOfCalculation.Percentage)
			? (ZDecimal)(intermediateResult.Rate * 100)
			: intermediateResult.Rate;

		var expectedRoundRate = exactExpectedRate.Round(intermediateResult.TaxRateDecimalPrecision);

		AssertEquals("AdjustedRate", expectedRoundRate, intermediateResult.AdjustedRate);

		if (intermediateResult.ParticipatingExpression)
		{
			AssertEquals("Participating Expression => ParticipatingAmount<=>Amount", intermediateResult.Amount, intermediateResult.ParticipatingAmount);
		}
		else
		{
			AssertEquals("Non-Participating Expression => ParticipatingAmount", 0m, intermediateResult.ParticipatingAmount);
		}
	}
}
