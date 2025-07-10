using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NO.Business;

class EntryLineDutyCalculator : UniversalDutyCalculator<CusEntryLine, EntryLineUniversalRateCalcData>
{
	public EntryLineDutyCalculator(CusEntryLine entity)
		: base(entity, RateCalculationVisitorMode.Default, CreateUniversalRateCalcData)
	{
	}

	protected override IRateCalculationVisitorCreator GetRateCalculationVisitorCreator(RateView rateView)
		=> new RateCalculationVisitorWithCombinedMultiplicationSubResults.Creator();

	static IUniversalRateCalcData CreateUniversalRateCalcData(CusEntryLine entryLine, RateView rateView)
		=> new EntryLineUniversalRateCalcData(entryLine, rateView);
}
