using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NO.Business;

sealed class InvoiceLineDutyCalculationStrategy(JobComInvoiceLine invLine)
{
	public ZDecimal Calculate()
	{
		var rates = new ApplicableRateLoader(invoiceLine.Factory)
						.LoadRatesForMultipleCriteriaSets(GetTariffAndRateCriteriaSetsForInvoiceLine());

		return rates.Aggregate(ZDecimal.Zero, (total, rate) => total + CalculateAndAddFeesForEntryByRate(rate));
	}

	ZDecimal CalculateAndAddFeesForEntryByRate(RateView rateView)
	{
		var calculationResults = DutyCalculator.CleanFormulaAndCalculate(rateView);

		return calculationResults?.IntermediateResults?
			.WhereNotNull()
			.Aggregate(ZDecimal.Zero, (total, fee) => total + (invoiceLine.FeeRounder?.Round(fee.ParticipatingAmount) ?? fee.ParticipatingAmount))
			?? ZDecimal.Zero;
	}

	IEnumerable<RateLoadTariffCriteriaSet> GetTariffAndRateCriteriaSetsForInvoiceLine()
	{
		if (invoiceLine.UniversalTariff == null)
		{
			return [];
		}

		return GetRateSelectionCriteria()
			.Select(criteria => new RateLoadTariffCriteriaSet(invoiceLine.UniversalTariff, criteria))
			.ToList();
	}

	IEnumerable<IZZRateSelectionCriteria> GetRateSelectionCriteria()
	{
		if (!invoiceLine.CanCalculateDuty())
		{
			yield break;
		}
		yield return invoiceLine.ExciseRateSelectionCriteria;
	}

	readonly JobComInvoiceLine invoiceLine = Argument.NotNull(invLine, nameof(invLine));
	InvoiceLineDutyCalculator DutyCalculator => dutyCalculator ??= new(invoiceLine);
	InvoiceLineDutyCalculator dutyCalculator;
}
