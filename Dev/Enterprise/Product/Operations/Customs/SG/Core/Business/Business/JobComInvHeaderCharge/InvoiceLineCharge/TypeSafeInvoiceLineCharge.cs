using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public abstract class TypeSafeInvoiceLineCharge : AutoInvoiceLineCharge
	{
		protected TypeSafeInvoiceLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		InvoiceLineCharge invoiceLineCharge
		{
			get { return (InvoiceLineCharge)this; }
		}

		public new InvoiceLineChargeValidation Validation
		{
			get { return (InvoiceLineChargeValidation)base.Validation; }
		}

		public new InvoiceLineChargeLookups Lookups
		{
			get { return (InvoiceLineChargeLookups)base.Lookups; }
		}

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new InvoiceLineChargeValidation(invoiceLineCharge);
		}

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new InvoiceLineChargeLookups(invoiceLineCharge);
		}
	}
}
