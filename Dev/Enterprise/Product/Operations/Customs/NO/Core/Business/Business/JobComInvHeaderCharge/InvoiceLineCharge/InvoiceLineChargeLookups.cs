namespace Enterprise.Customs.NO.Business
{
	public class InvoiceLineChargeLookups : Customs.Business.JobComInvHeaderChargeLookups
	{
		public InvoiceLineChargeLookups(InvoiceLineCharge invoiceLineCharge)
			: base(invoiceLineCharge)
		{
		}

		protected new InvoiceLineCharge Parent => (InvoiceLineCharge)base.Parent;
	}
}
