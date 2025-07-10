namespace Enterprise.Customs.NZ.Business.Declaration
{
	/// <summary>
	/// InvoiceLine collection dependant on the InvoiceHeader that is used as a sustitute for 
	/// the InvoiceLineCollection (dependant on the JobDeclaration) when there is no JobDeclaration
	/// available against the InvoiceHeader
	/// </summary>
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
