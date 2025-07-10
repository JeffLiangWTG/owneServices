
namespace Enterprise.Customs.US.Business
{
	public class InvoiceLineChargeLookups : ChargeLookups
	{
		public InvoiceLineChargeLookups(InvoiceLineCharge invoiceLineCharge)
			: base(invoiceLineCharge)
		{
		}

		public new InvoiceLineCharge Parent
		{
			get { return (InvoiceLineCharge)base.Parent; }
		}
	}
}
