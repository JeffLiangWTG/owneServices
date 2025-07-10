using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.DutyCalculator.Testing;

[TestedType(typeof(DutyCalculationResult))]
sealed class DutyCalculationResultTest : TestCase
{
	public void TestConstructorArguments()
	{
		AssertExceptionThrown<ArgumentNullException>("When formulaResult is null", () => new DutyCalculationResult(null, new FormulaErrorListener()));
		AssertExceptionThrown<ArgumentNullException>("When errorListener is null", () => new DutyCalculationResult(new RateFormulaResult(0m), null));
	}

	public void TestConstructorCallOnlyWithErrorListener()
	{
		var result = new DutyCalculationResult(new FormulaErrorListener());
		CombineAssertions(() =>
		{
			AssertEquals("Result Amount", 0m, result.ResultAmount);
			AssertEquals("IntermediateResults Count", 0, result.IntermediateResults.Count());
		});
	}

	public void TestProperties()
	{
		var formulaResult = new RateFormulaResult(100m);
		formulaResult.AddIntermediateResult(40m);
		formulaResult.AddIntermediateResult(60m, "DTN");

		var errorListener = new FormulaErrorListener();
		errorListener.Report(FormulaVisitErrorType.CountrySpecificValueNotFound, "SomeErrorMessage");

		var calculationResult = new DutyCalculationResult(formulaResult, errorListener);

		var expectedIntermediateResults = new DutyCalculationIntermediateResult[]
		{
			new DutyCalculationIntermediateResult(40m, 1m, 40m, UniversalReferenceConstants.MethodOfCalculation.Percentage),
			new DutyCalculationIntermediateResult(60m, 1m, 60m, "DTN"),
		};

		var expectedErrors = new ErrorInformation[]
		{
			new ErrorInformation(FormulaVisitErrorType.CountrySpecificValueNotFound, "SomeErrorMessage"),
		};

		AssertDutyCalculationResult(calculationResult, 100m, expectedIntermediateResults, expectedErrors);
	}

	public void TestEmptyFormulaResultConstructor()
	{
		var errorListener = new FormulaErrorListener();
		errorListener.Report(FormulaVisitErrorType.SyntaxError, "InvalidSyntax");

		var calculationResult = new DutyCalculationResult(errorListener);
		var expectedIntermediateResults = Array.Empty<IDutyCalculationIntermediateResult>();

		var expectedErrors = new ErrorInformation[]
		{
			new ErrorInformation(FormulaVisitErrorType.SyntaxError, "InvalidSyntax"),
		};

		AssertDutyCalculationResult(calculationResult, 0m, expectedIntermediateResults, expectedErrors);
	}

	void AssertDutyCalculationResult(IDutyCalculationResult calculationResult, decimal expectedAmount, IDutyCalculationIntermediateResult[] expectedIntermediateResults, ErrorInformation[] expectedErrors)
	{
		AssertEquals("ResultAmount", expectedAmount, calculationResult.ResultAmount);
		RateCalculationVisitorTestBase.AssertIntermediateResults(expectedIntermediateResults, calculationResult.IntermediateResults.ToArray());
		RateCalculationVisitorTestBase.AssertCalculationErrors(expectedErrors, calculationResult.Errors.ToArray());
	}
}
