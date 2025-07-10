using CargoWise.Types;
using Enterprise.Customs.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DutyCalculator.Testing;

[TestedType(typeof(RateFormulaResult))]
sealed class RateFormulaResultTest : TestCase
{
	public void TestResultAmount()
	{
		var rateFormulaResult = new RateFormulaResult(12m);
		AssertEquals(nameof(RateFormulaResult.ResultAmount), 12m, rateFormulaResult.ResultAmount);
	}

	public void TestIntermediateResults()
	{
		var rateFormulaResult = new RateFormulaResult(10m);
		AssertEquals("[PRE-Condition] No intermediate results", 0, rateFormulaResult.IntermediateResults.Count);

		rateFormulaResult.AddIntermediateResult(12);
		AssertRateFormulaResult(12, 1);

		rateFormulaResult.AddIntermediateResult(15);
		AssertRateFormulaResult(15, 2);

		rateFormulaResult.AddIntermediateResult(25, "KG");
		AssertRateFormulaResult(25, 3, "KG");
		void AssertRateFormulaResult(decimal expectedAmount, int expectedNumberOfRecords, string expectedMethodOfCalculation = "%")
		{
			AssertEquals($"Number of Records when Record is added with expectedAmount {expectedAmount}", expectedNumberOfRecords, rateFormulaResult.IntermediateResults.Count);
			CombineAssertions($"Added New rate with Amount {expectedAmount}", () =>
			{
				var intermediateResult = rateFormulaResult.IntermediateResults.ToArray()[expectedNumberOfRecords - 1];
				AssertEquals("Method Of Calculation", expectedMethodOfCalculation, intermediateResult?.MethodOfCalculation);
				AssertEquals("Amount", expectedAmount, intermediateResult?.Amount);
				AssertEquals("Base Value", expectedAmount, intermediateResult?.BaseValue);
				AssertEquals("Rate", 1m, intermediateResult?.Rate);
			});
		}
	}

	public void TestAddIntermediateResultsRange()
	{
		var rateFormulaResult = new RateFormulaResult(10m);
		AssertEquals("[PRE-Condition] No intermediate results", 0, rateFormulaResult.IntermediateResults.Count);

		var rangeOne = new[] { Mock.Of<IDutyCalculationIntermediateResult>(), Mock.Of<IDutyCalculationIntermediateResult>(), };
		rateFormulaResult.AddIntermediateResultsRange(rangeOne);
		AssertEquals("When 2 Results are added", 2, rateFormulaResult.IntermediateResults.Count);

		var rangeTwo = new[]
		{
			Mock.Of<IDutyCalculationIntermediateResult>(),
			Mock.Of<IDutyCalculationIntermediateResult>(),
			Mock.Of<IDutyCalculationIntermediateResult>()
		};
		rateFormulaResult.AddIntermediateResultsRange(rangeTwo);
		AssertEquals("When 3 more Results are added", 5, rateFormulaResult.IntermediateResults.Count);
	}

	public void TestAddIntermediateResultsRangeWithIndex()
	{
		var rateFormulaResult = new RateFormulaResult(10m);
		AssertEquals("[PRE-Condition] No intermediate results", 0, rateFormulaResult.IntermediateResults.Count);

		rateFormulaResult.AddIntermediateResult(11);
		var rangeOne = new[] { Mock.Of<IDutyCalculationIntermediateResult>(d => d.Amount == 15) };
		rateFormulaResult.AddIntermediateResultsRangeWithIndex(0, rangeOne);
		AssertContainsExactElementsInExactOrder("When RangeOne is added at index 0", new ZDecimal[] { 15, 11 }, rateFormulaResult.IntermediateResults.Select(r => r.Amount));

		var rangeTwo = new[]
		{
			Mock.Of<IDutyCalculationIntermediateResult>(d => d.Amount == 25),
			Mock.Of<IDutyCalculationIntermediateResult>(d => d.Amount == 55)
		};
		rateFormulaResult.AddIntermediateResultsRangeWithIndex(1, rangeTwo);
		AssertContainsExactElementsInExactOrder("When RangeTwo is added at index 1", new ZDecimal[] { 15, 25, 55, 11 }, rateFormulaResult.IntermediateResults.Select(r => r.Amount));
	}
}
