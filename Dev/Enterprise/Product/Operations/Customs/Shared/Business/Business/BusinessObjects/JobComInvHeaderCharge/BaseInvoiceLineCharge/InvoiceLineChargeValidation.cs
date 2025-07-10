namespace Enterprise.Customs.Business
{
	public class InvoiceLineChargeValidation : JobComInvHeaderChargeValidation
	{
		public InvoiceLineChargeValidation(BaseInvoiceLineCharge invoiceLineCharge) : base(invoiceLineCharge)
		{
			this.InvoiceLineCharge = invoiceLineCharge;
		}

		protected readonly BaseInvoiceLineCharge InvoiceLineCharge;

		#region Overriden Checks

		protected override void CheckJ7_RX_NKCurrency()
		{
			base.CheckJ7_RX_NKCurrency();
			ValidateCurrency();
		}

		#endregion
	}
}
