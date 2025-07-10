namespace Enterprise.Customs.NO.Business
{
	public abstract class InvoiceChargeLookups : Customs.Business.JobComInvHeaderChargeLookups
	{
		protected InvoiceChargeLookups(InvoiceCharge invoiceCharge)
			: base(invoiceCharge)
		{
		}
	}
}
