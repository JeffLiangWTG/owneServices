using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5203
{
	public class CustomsValuation : ICustomsValuation
	{
		public CustomsValuation(CusEntryHeader entryHeader)
		{
			this.entryHeader = entryHeader;
		}

		readonly CusEntryHeader entryHeader;

		public ZDecimal ExitToEntryChargeAmount => entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency;

		public ZDecimal FreightChargeAmount => entryHeader.CH_TotalInternationalFreightAmountInInvoiceCurrency;

		public ZDecimal OtherChargeDeductionAmount => ZDecimal.Zero;

		public ZString PartyRelationshipCode => ZString.Empty;

		public ZDecimal OtherChargeAmount => entryHeader.CH_TotalAdditionsInInvoiceCurrency;

		public ZDecimal OtherDeductionAmount => entryHeader.CH_TotalDeductionsInInvoiceCurrency;

		ZDecimal ICustomsValuation.InvoiceAmount => ZDecimal.Zero;

		ZDecimal ICustomsValuation.TotalDutyTaxFeeAmount => ZDecimal.Zero;
	}
}
