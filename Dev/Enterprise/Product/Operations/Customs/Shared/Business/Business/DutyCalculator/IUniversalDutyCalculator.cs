using Enterprise.Customs.Universal;

namespace Enterprise.Customs.Business
{
	public interface IUniversalDutyCalculator
	{
		IDutyCalculationResult CleanFormulaAndCalculate(RateView rateView);
	}
}
