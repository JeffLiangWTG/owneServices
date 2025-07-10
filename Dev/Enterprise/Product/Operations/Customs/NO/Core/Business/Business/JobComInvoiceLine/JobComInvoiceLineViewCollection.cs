namespace Enterprise.Customs.NO.Business
{
	public class JobComInvoiceLineViewCollection : Customs.Business.BaseJobComInvoiceLineViewCollection
	{
		public JobComInvoiceLineViewCollection(JobComInvoiceHeader invoice, InvoiceLineCompleteCollection completeCollection)
			: base(invoice, completeCollection)
		{
		}

		public new JobComInvoiceLine this[int index] => (JobComInvoiceLine)Elements[index];

		public new JobComInvoiceLine AddNew() => (JobComInvoiceLine)base.AddNew();

		protected new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

		protected new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;
	}
}
