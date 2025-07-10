namespace Enterprise.Customs.ZA.Business
{
	public class JobComInvoiceLineViewCollection : Customs.Business.BaseJobComInvoiceLineViewCollection
	{
		public JobComInvoiceLineViewCollection(JobComInvoiceHeader invoiceHeader, InvoiceLineCompleteCollection completeCollection)
			: base(invoiceHeader, completeCollection)
		{
		}

		public JobComInvoiceLineViewCollection(JobComInvoiceHeader invoiceHeader, Customs.Business.InvoiceLineDependentCollection completeCollection)
			: base(invoiceHeader, completeCollection)
		{
		}

		public new JobComInvoiceLine AddNew()
		{
			return (JobComInvoiceLine)base.AddNew();
		}

		public new JobComInvoiceLine this[int index]
		{
			get { return (JobComInvoiceLine)base[index]; }
		}
	}
}
