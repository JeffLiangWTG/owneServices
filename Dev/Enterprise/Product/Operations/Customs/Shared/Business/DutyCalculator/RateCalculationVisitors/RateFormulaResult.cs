using Enterprise.Customs.Business;

namespace Enterprise.Customs.DutyCalculator;

public sealed class RateFormulaResult
{
	public RateFormulaResult(decimal resultAmount)
	{
		ResultAmount = resultAmount;
	}

	public decimal ResultAmount { get; set; }

	public IReadOnlyCollection<IDutyCalculationIntermediateResult> IntermediateResults => intermediateResults;

	public void AddIntermediateResult(decimal amount, string methodOfCalculation = UniversalReferenceConstants.MethodOfCalculation.Percentage)
		=> intermediateResults.Add(new DutyCalculationIntermediateResult(amount, 1m, amount, methodOfCalculation));

	public void AddIntermediateResultsRange(IEnumerable<IDutyCalculationIntermediateResult> results)
		=> intermediateResults.AddRange(results);

	public void AddIntermediateResultsRangeWithIndex(int index, IEnumerable<IDutyCalculationIntermediateResult> results)
		=> intermediateResults.InsertRange(index, results);

	readonly List<IDutyCalculationIntermediateResult> intermediateResults = new();
}
