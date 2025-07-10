using System.Collections.Generic;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NO.Business;

sealed class InvoiceLineUniversalRateCalcData(JobComInvoiceLine invLine, RateView rateView)
	: DutyCalculator.InvoiceLineUniversalRateCalcData(invLine, rateView)
{
	protected override IDictionary<string, decimal> GetUnitOfMeasureValueList()
		=> RateCalcUnitOfMeasureAggregator.GetUomQtyDictionaryForInvoiceLine(InvoiceLine as JobComInvoiceLine);
}
