namespace Enterprise.Customs.SG.V4.Business
{
	public class InvoiceApportionChargeValidation : Customs.Business.BaseApportionedChargeValidation
	{
		public InvoiceApportionChargeValidation(InvoiceApportionCharge invoiceApportionCharge)
			: base(invoiceApportionCharge)
		{
		}

		public InvoiceApportionCharge InvoiceApportionCharge
		{
			get { return Parent; }
		}

		protected new InvoiceApportionCharge Parent
		{
			get { return (InvoiceApportionCharge)base.Parent; }
		}
	}
}
