
namespace Enterprise.Customs.US.Business
{
	partial class InvoiceLineApportionCharge : AutoInvoiceLineApportionCharge
	{
		public new InvoiceLineApportionChargeValidation Validation
		{
			get { return (InvoiceLineApportionChargeValidation)base.Validation; }
		}

		public new InvoiceLineApportionChargeLookups Lookups
		{
			get { return (InvoiceLineApportionChargeLookups)base.Lookups; }
		}

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new InvoiceLineApportionChargeValidation(this);
		}

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new InvoiceLineApportionChargeLookups(this);
		}
	}
}
