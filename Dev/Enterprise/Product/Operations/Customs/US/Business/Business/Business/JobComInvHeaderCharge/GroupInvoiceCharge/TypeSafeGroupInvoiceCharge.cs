
namespace Enterprise.Customs.US.Business
{
	partial class GroupInvoiceCharge : AutoGroupInvoiceCharge
	{
		public new GroupInvoiceChargeValidation Validation
		{
			get { return (GroupInvoiceChargeValidation)base.Validation; }
		}

		public new GroupInvoiceChargeLookups Lookups
		{
			get { return (GroupInvoiceChargeLookups)base.Lookups; }
		}

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new GroupInvoiceChargeValidation(this);
		}

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new GroupInvoiceChargeLookups(this);
		}
	}
}
