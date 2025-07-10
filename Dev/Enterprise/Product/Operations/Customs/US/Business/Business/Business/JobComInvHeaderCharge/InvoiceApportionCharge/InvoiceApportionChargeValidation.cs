namespace Enterprise.Customs.US.Business
{
	public class InvoiceApportionChargeValidation : Customs.Business.BaseApportionedChargeValidation
	{
		public InvoiceApportionChargeValidation(InvoiceApportionCharge invoiceApportionCharge)
			: base(invoiceApportionCharge)
		{
		}

		protected override bool IsCIFComponentUsed
		{
			get { return true; }
		}
	}
}
