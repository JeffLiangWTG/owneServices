namespace Enterprise.Customs.SG.V4.Business
{
	public class JobComInvoiceLineViewCollection : TypeSafeJobComInvoiceLineViewCollection
	{
		public JobComInvoiceLineViewCollection(JobComInvoiceHeader invoice, InvoiceLineCompleteCollection completeCollection)
			: base(invoice, completeCollection)
		{
		}

		public JobComInvoiceLineViewCollection(JobComInvoiceHeader invoice, Customs.Business.InvoiceLineDependentCollection completeCollection)
			: base(invoice, completeCollection)
		{
		}
	}
}
