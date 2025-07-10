

namespace Enterprise.Customs.TW.Business
{
	public partial class InvoiceCharge : AutoInvoiceCharge
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new InvoiceCharge Clone()
		{
			return (InvoiceCharge)base.Clone();
		}

		public new InvoiceChargeLookups Lookups
		{
			get { return (InvoiceChargeLookups)base.Lookups; }
		}

		public new InvoiceChargeValidation Validation
		{
			get { return (InvoiceChargeValidation)base.Validation; }
		}

		#endregion

		#region Implementation

		#region protected override

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new InvoiceChargeLookups(this);
		}

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new InvoiceChargeValidation(this);
		}

		#endregion

		#endregion
	}
}
