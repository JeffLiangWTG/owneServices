using System.Collections.Generic;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NO.Business;

sealed class EntryLineUniversalRateCalcData(Customs.Business.CusEntryLine entryLine, RateView rateView) : DutyCalculator.EntryLineUniversalRateCalcData(entryLine, rateView)
{
	protected override IDictionary<string, decimal> GetUnitOfMeasureValueList() => RateCalcUnitOfMeasureAggregator.GetUomQtyDictionary(EntryLine);
}
