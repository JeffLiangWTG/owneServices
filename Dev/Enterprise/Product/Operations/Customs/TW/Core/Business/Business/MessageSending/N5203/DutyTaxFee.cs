using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5203
{
	public class DutyTaxFee : IDutyTaxFee
	{
		public DutyTaxFee(CusEntryHeader entryHeader)
		{
			this.entryHeader = entryHeader;
		}

		readonly CusEntryHeader entryHeader;

		public ZString DutyExemptionWaiverNote => ZString.Empty;

		public ZString DutyMemoPrinted => ZString.Empty;

		public ZString DutyMethodCode => entryHeader.Declaration?.JE_PaymentMethod ?? ZString.Empty;

		public ZDecimal TotalDutyTaxFeeAmount => ZDecimal.Zero;

		public ZString PaymentObligationGuaranteeReferenceID => ZString.Empty;

		public ZDecimal TotalCashDutyTaxFeeAmount => ZDecimal.Zero;

		public ZDecimal TotalNonCashDutyTaxFeeAmount => ZDecimal.Zero;
	}
}
