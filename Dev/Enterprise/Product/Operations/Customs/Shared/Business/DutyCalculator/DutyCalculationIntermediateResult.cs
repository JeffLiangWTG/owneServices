using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DutyCalculator;

public class DutyCalculationIntermediateResult : IDutyCalculationIntermediateResult
{
	public DutyCalculationIntermediateResult(ZDecimal amount, ZDecimal rate, ZDecimal baseValue, ZString methodOfCalculation)
	{
		Amount = amount;
		Rate = rate;
		BaseValue = baseValue;
		MethodOfCalculation = methodOfCalculation;
	}

	public ZDecimal Amount { get; set; }

	public ZString MethodOfCalculation { get; set; }

	public ZDecimal Rate { get; set; }

	public ZDecimal BaseValue { get; set; }

	public ZString MethodOfPayment { get; set; }

	public bool ParticipatingExpression { get; set; } = true;

	public ZString OriginalMeursingExpression { get ; set; }

	public int TaxRateDecimalPrecision { get; set; } = 6;

	public ZDecimal AdjustedRate => GetAdjustedRate();

	public ZDecimal ParticipatingAmount => ParticipatingExpression ? Amount : ZDecimal.Zero;

	public void SetRateAndMethodOfCalculation(ZString methodOfCalculation, ZDecimal adjustedRate)
	{
		MethodOfCalculation = methodOfCalculation;
		Rate = (MethodOfCalculation == UniversalReferenceConstants.MethodOfCalculation.Percentage)
			? (ZDecimal)(adjustedRate / 100m)
			: adjustedRate;
	}

	ZDecimal GetAdjustedRate()
	{
		var rate = MethodOfCalculation == UniversalReferenceConstants.MethodOfCalculation.Percentage ? (ZDecimal)(Rate * 100) : Rate;
		return rate.Round(TaxRateDecimalPrecision);
	}
}
