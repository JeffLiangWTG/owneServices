namespace Enterprise.Customs.US.Business
{
	public class InvoiceLineApportionChargeCollection : Common.JobComInvApportionedChargeCollection<InvoiceLineApportionCharge>
	{
		public InvoiceLineApportionChargeCollection(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public new InvoiceLineApportionCharge this[string chargeName] => base[chargeName];

		protected new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;
	}
}
