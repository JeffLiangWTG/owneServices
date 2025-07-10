using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class ChargeDeductionWrapper : IChargeDeduction
{
	public ChargeDeductionWrapper(InvoiceLineCharge invoiceLineCharge, int sequenceNumeric)
	{
		this.invoiceLineCharge = invoiceLineCharge;
		SequenceNumeric = sequenceNumeric;
	}
	readonly InvoiceLineCharge invoiceLineCharge;

	public int SequenceNumeric { get; }

	public string ChargesTypeCode => invoiceLineCharge.J7_ChargeType;

	public decimal OtherChargeDeductionAmount => invoiceLineCharge.J7_Amount;
}
