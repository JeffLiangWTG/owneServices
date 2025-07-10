using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public partial class JobComInvoiceHeader : AutoZAJobComInvoiceHeader
	{
		public new JobComInvoiceHeaderValidation Validation
		{
			get { return (JobComInvoiceHeaderValidation)base.Validation; }
		}

		public new JobComInvoiceHeaderLookups Lookups
		{
			get { return (JobComInvoiceHeaderLookups)base.Lookups; }
		}

		[UniversalCopyCollectionEntity(JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.Constants.JI_JZ)]
		public new JobComInvoiceLineViewCollection JobComInvoiceLines
		{
			get { return (JobComInvoiceLineViewCollection)base.JobComInvoiceLines; }
		}

		public new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
		}

		public new JobComInvChargeCollection<InvoiceCharge> Charges
		{
			get { return (JobComInvChargeCollection<InvoiceCharge>)base.Charges; }
		}

		public new JobComInvApportionedChargeCollection<ApportionedCharge> GroupCharges => (JobComInvApportionedChargeCollection<ApportionedCharge>)base.GroupCharges;

		public new JobComInvoiceLineViewCollection InvoiceLines
		{
			get { return (JobComInvoiceLineViewCollection)base.InvoiceLines; }
		}

		#region Implementation

		protected override Customs.Business.JobComInvoiceHeaderLookups GetNewLookups()
		{
			return new JobComInvoiceHeaderLookups(this);
		}

		protected override BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
		{
			JobComInvoiceLineViewCollection result = null;
			if (JobDeclaration != null)
			{
				result = new JobComInvoiceLineViewCollection(this, JobDeclaration.InvoiceLines);
			}
			return result;
		}

		protected override BaseJobComInvoiceLineViewCollection CreateNewInvoiceLineCollectionWhenDeclarationIsNull()
		{
			InvoiceLineDependentCollection collection = new InvoiceLineDependentCollection(this);
			collection.Load();
			return new JobComInvoiceLineViewCollection(this, collection);
		}

		protected override IJobComInvChargeCollection<BaseInvoiceCharge> CreateNewJobComInvHeaderCharges()
		{
			return new JobComInvChargeCollection<InvoiceCharge>(this);
		}

		protected override IJobComInvApportionedChargeCollection<BaseApportionedCharge> CreateNewApportionedChargeCollection()
		{
			return new JobComInvApportionedChargeCollection<ApportionedCharge>(this);
		}

		#endregion
	}
}
