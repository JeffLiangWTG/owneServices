using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.DutyCalculator;

[CodeAlive("This can be used by any country which do not require country specific implementation of InvoiceLineUniversalRateCalcData")]
public class InvoiceLineUniversalDutyCalculator : UniversalDutyCalculator<BaseJobComInvoiceLine, InvoiceLineUniversalRateCalcData>
{
	public InvoiceLineUniversalDutyCalculator(BaseJobComInvoiceLine entity, RateCalculationVisitorMode rateCalculationVisitorMode)
		: base(entity, rateCalculationVisitorMode, CreateUniversalRateCalcData)
	{
	}

	static IUniversalRateCalcData CreateUniversalRateCalcData(BaseJobComInvoiceLine invoiceLine, RateView rateView)
		=> new InvoiceLineUniversalRateCalcData(invoiceLine, rateView);
}
