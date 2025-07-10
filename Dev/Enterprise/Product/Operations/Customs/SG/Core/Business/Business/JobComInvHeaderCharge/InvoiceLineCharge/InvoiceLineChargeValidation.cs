
namespace Enterprise.Customs.SG.V4.Business
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
	}
}
