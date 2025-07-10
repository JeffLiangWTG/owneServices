using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.US.Business
{
	partial class JobComInvoiceHeader : AutoJobComInvoiceHeader
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
		}

		public new JobComInvoiceHeaderLookups Lookups
		{
			get { return (JobComInvoiceHeaderLookups)base.Lookups; }
		}

		public new JobComInvoiceHeaderValidation Validation
		{
			get { return (JobComInvoiceHeaderValidation)base.Validation; }
		}

		public new JobComInvoiceLineViewCollection JobComInvoiceLines
		{
			get { return (JobComInvoiceLineViewCollection)base.JobComInvoiceLines; }
		}

		public new JobComInvoiceLineViewCollection InvoiceLines
		{
			get { return (JobComInvoiceLineViewCollection)base.InvoiceLines; }
		}

		[ChildEditable(true)]
		public new InvoiceChargeCollection Charges
		{
			get { return (InvoiceChargeCollection)base.Charges; }
		}

		[ChildEditable(true)]
		public new InvoiceApportionChargeCollection GroupCharges => (InvoiceApportionChargeCollection)base.GroupCharges;

		public new JobComInvoiceGroupHeader Master
		{
			get { return (JobComInvoiceGroupHeader)base.Master; }
		}

		public new JobComInvoiceGroupHeader GroupHeader
		{
			get { return (JobComInvoiceGroupHeader)base.GroupHeader; }
		}

		public new Bill Bill
		{
			get { return (Bill)base.Bill; }
		}

		#endregion

		#region Implementation

		#region Overridden 'CreateNew' methods

		protected override Customs.Business.JobComInvoiceHeaderLookups GetNewLookups()
		{
			return new JobComInvoiceHeaderLookups(this);
		}

		protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation()
		{
			var declaration = JobDeclaration;

			if (declaration != null && (declaration.IsRecon || declaration.IsDrawback))
			{
				return new EmptyInvoiceHeaderValidation(this);
			}
			else
			{
				return new JobComInvoiceHeaderValidation(this);
			}
		}

		protected override BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
		{
			JobComInvoiceLineViewCollection result = null;
			if (JobDeclaration != null)
			{
				result = new JobComInvoiceLineViewCollection(this, JobDeclaration.InvoiceLines);

				if (JobDeclaration.IsRecon)
				{
					this.RegisterEditableChildObject(result);
				}
			}
			return result;
		}

		protected override BaseJobComInvoiceLineViewCollection CreateNewInvoiceLineCollectionWhenDeclarationIsNull()
		{
			InvoiceLineDependentCollection collection = new InvoiceLineDependentCollection(this);
			collection.Load();
			JobComInvoiceLineViewCollection result = new JobComInvoiceLineViewCollection(this, collection);

			return result;
		}

		protected override IJobComInvChargeCollection<BaseInvoiceCharge> CreateNewJobComInvHeaderCharges()
		{
			return new InvoiceChargeCollection(this);
		}

		protected override IJobComInvApportionedChargeCollection<BaseApportionedCharge> CreateNewApportionedChargeCollection()
		{
			return new InvoiceApportionChargeCollection(this);
		}

		#endregion

		#endregion
	}
}
