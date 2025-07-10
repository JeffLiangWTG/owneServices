namespace Enterprise.Customs.US.Business
{
	public class InvoiceLineChargeValidation : Customs.Business.InvoiceLineChargeValidation
	{
		public InvoiceLineChargeValidation(InvoiceLineCharge invoiceLineCharge)
			: base(invoiceLineCharge)
		{
		}

		public new InvoiceLineCharge InvoiceLineCharge
		{
			get { return Parent; }
		}

		protected new InvoiceLineCharge Parent
		{
			get { return (InvoiceLineCharge)base.Parent; }
		}

		protected override bool ShouldCheckExRates
		{
			get { return base.ShouldCheckExRates && !ValuationDatesChanged; }
		}

		bool ValuationDatesChanged
		{
			get { return Parent.InvoiceLine != null && Parent.InvoiceLine.Declaration != null && Parent.InvoiceLine.Declaration.ValuationDatesChanged; }
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
