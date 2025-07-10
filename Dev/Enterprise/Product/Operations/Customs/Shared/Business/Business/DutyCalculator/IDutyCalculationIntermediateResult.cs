using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface IDutyCalculationIntermediateResult
	{
		ZDecimal Amount { get; set; }

		ZString MethodOfCalculation { get; set; }

		ZDecimal Rate { get; set; }

		ZDecimal BaseValue { get; set; }

		ZString MethodOfPayment { get; set; }

		bool ParticipatingExpression { get; set; }

		ZString OriginalMeursingExpression { get; set; }

		int TaxRateDecimalPrecision { get; set; }

		ZDecimal AdjustedRate { get; }

		ZDecimal ParticipatingAmount { get; }

		void SetRateAndMethodOfCalculation(ZString methodOfCalculation, ZDecimal adjustedRate);
	}
}
