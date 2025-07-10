
namespace Enterprise.Customs.US.Business
{
	partial class InvoiceLineCharge : AutoInvoiceLineCharge
	{
		public new InvoiceLineChargeValidation Validation
		{
			get { return (InvoiceLineChargeValidation)base.Validation; }
		}

		public new InvoiceLineChargeLookups Lookups
		{
			get { return (InvoiceLineChargeLookups)base.Lookups; }
		}

		public new JobComInvoiceLine InvoiceLine
		{
			get { return (JobComInvoiceLine)base.InvoiceLine; }
		}

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new InvoiceLineChargeValidation(this);
		}

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new InvoiceLineChargeLookups(this);
		}
	}
}
