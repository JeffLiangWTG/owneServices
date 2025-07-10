using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.TW.Business
{
	partial class JobDeclaration
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new JobDeclaration Clone() => (JobDeclaration)base.Clone();

		[ChildEditable(true)]
		public new ICusContainerCollection<CusContainer> CusContainers => (ICusContainerCollection<CusContainer>)base.CusContainers;

		[ChildEditable(false)]
		public new IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader> JobComInvoiceGroupHeaders => (IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)base.JobComInvoiceGroupHeaders;

		public new JobDeclarationValidation Validation => (JobDeclarationValidation)base.Validation;

		public new JobDeclarationLookups Lookups => (JobDeclarationLookups)base.Lookups;

		[ChildEditable(true)]
		public new CusEntryHeaderCollection<CusEntryHeader> CustomsEntryHeaders => (CusEntryHeaderCollection<CusEntryHeader>)base.CustomsEntryHeaders;

		[ChildEditable]
		public new InvoiceHeaderActiveCollection Invoices => (InvoiceHeaderActiveCollection)base.Invoices;

		public new InvoiceLineViewCollection FilteredInvoiceLines => (InvoiceLineViewCollection)base.FilteredInvoiceLines;

		[ChildEditable(true)]
		public new InvoiceLineCompleteCollection InvoiceLines => (InvoiceLineCompleteCollection)base.InvoiceLines;

		[ChildEditable(true)]
		public new BillCollection Bills => (BillCollection)base.Bills;

		public new ActiveCusEntryHeaderCollection ActiveEntryHeaders => (ActiveCusEntryHeaderCollection)base.ActiveEntryHeaders;

		public new TWJobDocAddress SupplierDocumentaryAddress => (TWJobDocAddress)base.SupplierDocumentaryAddress;

		protected override JobDocAddressRequirement GetSupplierDocAddressRequirement()
		{
			return new SupplierAddressRequirement(DocAddressType.SupplierDocumentaryAddress, ContactType.Consignor);
		}

		protected override JobDocAddressRequirement GetSupplierPicDlvAddressRequirement()
		{
			return new SupplierPicDlvAddressRequirement(DocAddressType.SupplierPickupDeliveryAddress, AddressType.PIC);
		}

		public new TWJobDocAddress ImporterDocumentaryAddress => (TWJobDocAddress)base.ImporterDocumentaryAddress;

		protected override JobDocAddressRequirement GetImporterDocumentaryAddressRequirement()
		{
			return new ImporterAddressRequirement(DocAddressType.ImporterDocumentaryAddress, ContactType.Consignee);
		}

		protected override JobDocAddressRequirement GetImporterPicDlvAddressRequirement()
		{
			return new ImporterPicDlvAddressRequirement(DocAddressType.ImporterPickupDeliveryAddress, AddressType.DLV);
		}

		#endregion

		#region Implementation

		#region protected override

		protected override ICusContainerCollection<BaseCusContainer> NewCusContainersCollection() => new BaseCusContainerCollection<CusContainer>(this, Factory);

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);

		protected override Customs.Business.JobDeclarationValidation GetNewValidation()
		{
			JobDeclarationValidation result;
			if (IsImport)
			{
				result = new ImportJobDeclarationValidation(this);
			}
			else if (IsExport)
			{
				result = new ExportJobDeclarationValidation(this);
			}
			else
			{
				result = new JobDeclarationValidation(this);
			}
			return result;
		}

		protected override Customs.Business.JobDeclarationLookups GetNewLookups() => new JobDeclarationLookups(this);

		protected override ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> NewCustomsEntryHeaders() => new CusEntryHeaderCollection<CusEntryHeader>(this, Factory);

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(this);

		protected override IInvoiceLineViewCollection<BaseJobComInvoiceLine> GetNewInvoiceLineViewCollection() => new InvoiceLineViewCollection(this);

		protected override IBillCollection<Customs.Business.Bill, BaseJobDeclaration> CreateNewBillCollection() => new BillCollection(this, Factory);

		protected override Customs.Business.InvoiceLineCompleteCollection GetNewInvoiceLineCompleteCollection() => new InvoiceLineCompleteCollection(this);

		protected override Customs.Business.ActiveCusEntryHeaderCollection GetActiveEntryHeaderCollection() => new ActiveCusEntryHeaderCollection(this);

		public new EntryInstructionProvider CustomsEntryInstructionProvider => (EntryInstructionProvider)base.CustomsEntryInstructionProvider;

		protected override Customs.Business.EntryInstructionProvider GetCustomsEntryInstructionProviderCore() => new EntryInstructionProvider(this, new CusEntryInstructionComparer());

		#endregion

		#endregion

	}
}

