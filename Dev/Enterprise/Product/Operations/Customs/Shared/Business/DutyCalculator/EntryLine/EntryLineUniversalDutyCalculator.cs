using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.DutyCalculator;

[CodeAlive("This can be used by any country which do not require country specific implementation of EntryLineUniversalRateCalcData")]
public class EntryLineUniversalDutyCalculator : UniversalDutyCalculator<CusEntryLine, EntryLineUniversalRateCalcData>
{
	public EntryLineUniversalDutyCalculator(CusEntryLine entryLine, RateCalculationVisitorMode rateCalculationVisitorMode)
		: base(entryLine, rateCalculationVisitorMode, CreateUniversalRateCalcData)
	{
	}

	static IUniversalRateCalcData CreateUniversalRateCalcData(CusEntryLine entryLine, RateView rateView)
		=> new EntryLineUniversalRateCalcData(entryLine, rateView);
}
