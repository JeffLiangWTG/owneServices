using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NO.Business;

class InvoiceLineDutyCalculator : UniversalDutyCalculator<JobComInvoiceLine, InvoiceLineUniversalRateCalcData>
{
	public InvoiceLineDutyCalculator(JobComInvoiceLine invoiceLine)
		: base(invoiceLine, RateCalculationVisitorMode.Default, CreateUniversalRateCalcData)
	{
	}

	protected override IRateCalculationVisitorCreator GetRateCalculationVisitorCreator(RateView rateView)
		=> new RateCalculationVisitorWithCombinedMultiplicationSubResults.Creator();

	static IUniversalRateCalcData CreateUniversalRateCalcData(JobComInvoiceLine invoiceLine, RateView rateView)
		=> new InvoiceLineUniversalRateCalcData(invoiceLine, rateView);
}
