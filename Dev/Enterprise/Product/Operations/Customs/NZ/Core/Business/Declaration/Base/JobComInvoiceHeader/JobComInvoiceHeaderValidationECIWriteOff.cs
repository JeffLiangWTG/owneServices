namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class JobComInvoiceHeaderValidationECIWriteOff : JobComInvoiceHeaderValidation
	{
		public JobComInvoiceHeaderValidationECIWriteOff(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		protected override void CheckJZ_Calc_BalanceCore()
		{
		}

		protected override void CheckJZ_RX_NKInvoice_Currency()
		{
		}

		protected override void CheckJZ_Calc_CIFAmount()
		{
		}

		protected override void CheckJZ_CU_RelatedHouseBill()
		{
		}
	}
}
