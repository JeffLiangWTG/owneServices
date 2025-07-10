using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.PL.Business;

public class EntryLineDutyCalculator : EU.Business.EntryLineDutyCalculator
{
	public EntryLineDutyCalculator(CusEntryLine entity, RateCalculationVisitorMode rateCalculationVisitorMode)
		: base(entity, rateCalculationVisitorMode, GetNewUniversalRateCalcData)
	{
	}

	static IUniversalRateCalcData GetNewUniversalRateCalcData(Customs.Business.CusEntryLine entryLine, RateView rateView) => new UniversalRateCalcData((CusEntryLine)entryLine, rateView);

	protected override IRateCalculationVisitorCreator GetRateCalculationVisitorCreator(RateView rateView)
	{
		var baseFormulaCreator = base.GetRateCalculationVisitorCreator(rateView);
		return rateView.RateCode == EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts
			? new A00RateCalculationVisitorCreator(baseFormulaCreator)
			: baseFormulaCreator;
	}
}
