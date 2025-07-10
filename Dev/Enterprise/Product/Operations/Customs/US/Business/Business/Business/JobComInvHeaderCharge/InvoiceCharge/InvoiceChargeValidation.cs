namespace Enterprise.Customs.US.Business
{
	public class InvoiceChargeValidation : Customs.Business.BaseInvoiceChargeValidation
	{
		public InvoiceChargeValidation(InvoiceCharge invoiceCharge)
			: base(invoiceCharge)
		{
		}

		public InvoiceCharge InvoiceCharge
		{
			get { return Parent; }
		}

		protected new InvoiceCharge Parent
		{
			get { return (InvoiceCharge)base.Parent; }
		}

		protected override bool ShouldCheckExRates
		{
			get { return base.ShouldCheckExRates && !ValuationDatesChanged; }
		}

		bool ValuationDatesChanged
		{
			get { return Parent.Invoice != null && Parent.Invoice.JobDeclaration != null && Parent.Invoice.JobDeclaration.ValuationDatesChanged; }
		}

		protected override void CheckJ7_AdjustedCharge()
		{
			base.CheckJ7_AdjustedCharge();

			Parent.ValidateAdjustedChargeHasAnotherChargeToAdjust();
		}

		protected override bool IsCIFComponentUsed
		{
			get { return true; }
		}
	}
}
