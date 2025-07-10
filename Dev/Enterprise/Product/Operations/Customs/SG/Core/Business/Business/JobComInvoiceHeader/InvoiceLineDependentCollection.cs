namespace Enterprise.Customs.SG.V4.Business
{
	public class InvoiceLineDependentCollection : Customs.Business.InvoiceLineDependentCollection
	{
		public InvoiceLineDependentCollection(JobComInvoiceHeader invoice)
			: base(invoice)
		{
		}

		public new JobComInvoiceLine AddNew()
		{
			return (JobComInvoiceLine)base.AddNew();
		}

		public new JobComInvoiceLine this[int index]
		{
			get { return (JobComInvoiceLine)Elements[index]; }
		}
	}
}
