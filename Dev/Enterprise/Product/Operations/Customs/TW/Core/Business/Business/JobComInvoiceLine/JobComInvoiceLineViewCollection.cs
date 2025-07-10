namespace Enterprise.Customs.TW.Business
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
