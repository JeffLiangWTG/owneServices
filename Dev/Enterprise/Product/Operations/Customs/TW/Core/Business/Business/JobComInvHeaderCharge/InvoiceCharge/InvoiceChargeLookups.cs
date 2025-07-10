namespace Enterprise.Customs.TW.Business
{
	public class InvoiceChargeLookups : Customs.Business.JobComInvHeaderChargeLookups
	{
		public InvoiceChargeLookups(InvoiceCharge invoiceCharge)
			: base(invoiceCharge)
		{
		}

		public new InvoiceCharge Parent
		{
			get { return (InvoiceCharge)base.Parent; }
		}
	}
}
