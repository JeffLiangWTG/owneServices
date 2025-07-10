using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DutyCalculator;

sealed class DutyCalculationResult : IDutyCalculationResult
{
	public DutyCalculationResult(RateFormulaResult formulaResult, FormulaErrorListener errorListener)
	{
		this.formulaResult = Argument.NotNull(formulaResult, nameof(formulaResult));
		this.errorListener = Argument.NotNull(errorListener, nameof(errorListener));
	}
	readonly RateFormulaResult formulaResult;
	readonly FormulaErrorListener errorListener;

	public DutyCalculationResult(FormulaErrorListener errorListener) : this(new RateFormulaResult(0m), errorListener)
	{
	}

	public decimal ResultAmount => formulaResult.ResultAmount;

	public IEnumerable<IDutyCalculationIntermediateResult> IntermediateResults => formulaResult.IntermediateResults;

	public IEnumerable<ErrorInformation> Errors => errorListener.Errors;
}
