namespace Enterprise.Customs.NZ.Business.Declaration
{
	/// <summary>
	/// BusinessObjectCollectionView used on [InvoiceHeader.JobComInvoiceLines] showing a subset from 
	/// the full collection against the JobDeclaration. Will also accept a parent collection from the
	/// InvoiceHeader for the case where there is no JobDeclaration available.
	/// </summary>
	public class JobComInvoiceLineViewCollection : Customs.Business.BaseJobComInvoiceLineViewCollection
		, Integration.Customs.NZ.IJobComInvoiceLineViewCollection
	{
		public JobComInvoiceLineViewCollection(JobComInvoiceHeader parent, Customs.Business.InvoiceLineCompleteCollection collectionFromDeclaration)
			: base(parent, collectionFromDeclaration)
		{
		}

		public JobComInvoiceLineViewCollection(JobComInvoiceHeader parent, InvoiceLineDependentCollection collectionFromInvoiceHeader)
			: base(parent, collectionFromInvoiceHeader)
		{
		}

		public new virtual JobComInvoiceLine this[int index]
		{
			get { return (JobComInvoiceLine)Elements[index]; }
		}

		Integration.Customs.NZ.IJobComInvoiceLine Integration.Customs.NZ.IJobComInvoiceLineViewCollection.this[int index] => this[index];

		public virtual new JobComInvoiceLine AddNew()
		{
			return (JobComInvoiceLine)base.AddNew();
		}
	}
}
