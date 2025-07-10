using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NO.Business
{
	public class InvoiceLineApportionCharge : Customs.Business.BaseInvoiceLineApportionedCharge
	{
		public InvoiceLineApportionCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new InvoiceLineApportionCharge Clone() => (InvoiceLineApportionCharge)base.Clone();

		public new InvoiceLineApportionChargeValidation Validation => (InvoiceLineApportionChargeValidation)base.Validation;

		public new InvoiceLineApportionChargeLookups Lookups => (InvoiceLineApportionChargeLookups)base.Lookups;

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceLineApportionChargeValidation(this);

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups() => new InvoiceLineApportionChargeLookups(this);
	}
}
