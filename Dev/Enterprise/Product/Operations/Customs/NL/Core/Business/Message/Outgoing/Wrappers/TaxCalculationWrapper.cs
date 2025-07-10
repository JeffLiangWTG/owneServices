using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class TaxCalculationWrapper : ITaxCalculation
{
	public TaxCalculationWrapper(JobComInvoiceLine jobComInvoiceLine)
	{
		invoiceLine = jobComInvoiceLine;
	}
	readonly JobComInvoiceLine invoiceLine;

	public decimal TaxAssessedAmount => decimal.Zero;

	public string QuotaOrderID => invoiceLine.JI_ConcessionOrder;

	public string DutyRegimeCode => invoiceLine.JI_PrimaryPreference;

	public IReadOnlyCollection<IDutyTaxFee> DutyTaxFees => dutyTaxFees ??= invoiceLine.CusEntryLine.Fees.Cast<CusEntryLineFee>().Select((x, index) => new DutyTaxFeeWrapper(x, index + 1)).ToArray();
	IReadOnlyCollection<IDutyTaxFee> dutyTaxFees;
}
