
namespace Enterprise.Customs.US.Business
{
	partial class InvoiceApportionCharge : AutoInvoiceApportionCharge
	{
		public new InvoiceApportionChargeValidation Validation
		{
			get { return (InvoiceApportionChargeValidation)base.Validation; }
		}

		public new InvoiceApportionChargeLookups Lookups
		{
			get { return (InvoiceApportionChargeLookups)base.Lookups; }
		}

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new InvoiceApportionChargeValidation(this);
		}

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new InvoiceApportionChargeLookups(this);
		}
	}
}
