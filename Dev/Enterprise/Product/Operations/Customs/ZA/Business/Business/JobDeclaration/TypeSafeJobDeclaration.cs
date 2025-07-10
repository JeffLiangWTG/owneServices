using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public abstract class TypeSafeJobDeclaration : AutoZAJobDeclaration
	{
		#region Constructor

		protected TypeSafeJobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		[ChildEditable]
		[UniversalCopyCollectionEntity(JobComInvoiceHeaderSchema.Constants.TableName, JobComInvoiceHeaderSchema.Constants.JZ_JE)]
		public new InvoiceHeaderActiveCollection Invoices
		{
			get { return (InvoiceHeaderActiveCollection)base.Invoices; }
		}

		[ChildEditable(true)]
		public new CusEntryHeaderCollection<CusEntryHeader> CustomsEntryHeaders
		{
			get
			{
				var result = (CusEntryHeaderCollection<CusEntryHeader>)base.CustomsEntryHeaders;
				RegisterListChangedCalledRefreshBinding(result);
				return result;
			}
		}

		public new JobDeclarationLookups Lookups
		{
			get { return (JobDeclarationLookups)base.Lookups; }
		}

		public new IInvoiceLineViewCollection<JobComInvoiceLine> FilteredInvoiceLines
		{
			get { return (IInvoiceLineViewCollection<JobComInvoiceLine>)base.FilteredInvoiceLines; }
		}

		[ChildEditable(true)]
		public new InvoiceLineCompleteCollection InvoiceLines
		{
			get { return (InvoiceLineCompleteCollection)base.InvoiceLines; }
		}

		[ChildEditable(true)]
		public new CusContainerCollection CusContainers
		{
			get { return (CusContainerCollection)base.CusContainers; }
		}

		[ChildEditable(false)]
		public new IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader> JobComInvoiceGroupHeaders
		{
			get { return (IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)base.JobComInvoiceGroupHeaders; }
		}

		public new ActiveCusEntryHeaderCollection ActiveEntryHeaders
		{
			get { return (ActiveCusEntryHeaderCollection)base.ActiveEntryHeaders; }
		}

		public new JobDeclarationValidation Validation
		{
			get { return (JobDeclarationValidation)base.Validation; }
		}

		public new EntryInstructionProvider CustomsEntryInstructionProvider => (EntryInstructionProvider)base.CustomsEntryInstructionProvider;

		#endregion

		#region Implementation

		#region Overridden 'CreateNew' methods

		JobDeclaration Declaration
		{
			get { return (JobDeclaration)this; }
		}

		protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewInvoiceHeaderCollection()
		{
			return new InvoiceHeaderActiveCollection(Declaration);
		}

		protected override ICusContainerCollection<BaseCusContainer> NewCusContainersCollection()
		{
			return new CusContainerCollection(Declaration, Factory);
		}

		protected override ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> NewCustomsEntryHeaders() => new CusEntryHeaderCollection(Declaration, Factory);

		protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection()
		{
			return new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(Declaration);
		}

		protected override Customs.Business.JobDeclarationLookups GetNewLookups()
		{
			return new JobDeclarationLookups(Declaration);
		}

		protected override Customs.Business.ActiveCusEntryHeaderCollection GetActiveEntryHeaderCollection()
		{
			return new ActiveCusEntryHeaderCollection(Declaration);
		}

		protected override Customs.Business.JobDeclarationValidation GetNewValidation()
		{
			if (Declaration.IsExport)
			{
				return new ExportJobDeclarationValidation(Declaration);
			}
			else if (Declaration.IsExWarehouse)
			{
				return new ExBondJobDeclarationValidation(Declaration);
			}
			else if (Declaration.IsImport)
			{
				return new ImportJobDeclarationValidation(Declaration);
			}
			else
			{
				return new JobDeclarationValidation(Declaration);
			}
		}

		protected override IInvoiceLineViewCollection<BaseJobComInvoiceLine> GetNewInvoiceLineViewCollection()
		{
			return new InvoiceLineViewCollection<JobComInvoiceLine>(Declaration);
		}

		protected override Customs.Business.InvoiceLineCompleteCollection GetNewInvoiceLineCompleteCollection()
		{
			return new InvoiceLineCompleteCollection(Declaration);
		}

		protected override Customs.Business.EntryInstructionProvider GetCustomsEntryInstructionProviderCore()
		{
			return new EntryInstructionProvider(Declaration);
		}

		#endregion

		#endregion
	}
}
