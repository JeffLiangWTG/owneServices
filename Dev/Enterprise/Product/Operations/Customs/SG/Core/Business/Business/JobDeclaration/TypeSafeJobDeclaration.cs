using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.SG.V4.Business.TypeSafe
{
	public abstract class TypeSafeJobDeclaration : AutoJobDeclaration
	{
		#region Constructor

		protected TypeSafeJobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		[ChildEditable(true)]
		public new CusContainerCollection CusContainers => (CusContainerCollection)base.CusContainers;

		[ChildEditable(false)]
		public new IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader> JobComInvoiceGroupHeaders => (IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)base.JobComInvoiceGroupHeaders;

		public new JobDeclarationValidation Validation => (JobDeclarationValidation)base.Validation;

		public new JobDeclarationLookups Lookups => (JobDeclarationLookups)base.Lookups;

		[ChildEditable(true)]
		public new ICusEntryHeaderCollection<CusEntryHeader> CustomsEntryHeaders => (ICusEntryHeaderCollection<CusEntryHeader>)base.CustomsEntryHeaders;

		[ChildEditable]
		public new InvoiceHeaderActiveCollection Invoices => (InvoiceHeaderActiveCollection)base.Invoices;

		public new InvoiceLineViewCollection FilteredInvoiceLines => (InvoiceLineViewCollection)base.FilteredInvoiceLines;

		[ChildEditable(true)]
		public new InvoiceLineCompleteCollection InvoiceLines => (InvoiceLineCompleteCollection)base.InvoiceLines;

		[ChildEditable(true)]
		public new BillCollection<Bill, JobDeclaration> Bills => (BillCollection<Bill, JobDeclaration>)base.Bills;

		#endregion

		#region Implementation

		#region Overridden 'CreateNew' methods

		public JobDeclaration Declaration => (JobDeclaration)this;

		protected override ICusContainerCollection<BaseCusContainer> NewCusContainersCollection() => new CusContainerCollection(Declaration, Factory);

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(Declaration);

		protected override Customs.Business.JobDeclarationValidation GetNewValidation() => new JobDeclarationValidation(Declaration);

		protected override Customs.Business.JobDeclarationLookups GetNewLookups() => new JobDeclarationLookups(Declaration);

		protected override ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> NewCustomsEntryHeaders() => new CusEntryHeaderCollection<CusEntryHeader>(this, Factory);

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(Declaration);

		protected override IInvoiceLineViewCollection<BaseJobComInvoiceLine> GetNewInvoiceLineViewCollection() => new InvoiceLineViewCollection(Declaration);

		protected override IBillCollection<Customs.Business.Bill, BaseJobDeclaration> CreateNewBillCollection() => new BillCollection<Bill, JobDeclaration>(Declaration, Factory);

		protected override Customs.Business.InvoiceLineCompleteCollection GetNewInvoiceLineCompleteCollection() => new InvoiceLineCompleteCollection(Declaration);

		#endregion

		#endregion
	}
}

