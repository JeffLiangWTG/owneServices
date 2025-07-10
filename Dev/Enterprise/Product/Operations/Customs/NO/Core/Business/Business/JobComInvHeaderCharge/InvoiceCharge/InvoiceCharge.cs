using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NO.Business
{
	public class InvoiceCharge : Customs.Business.BaseInvoiceCharge
	{
		public InvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new InvoiceCharge Clone() => (InvoiceCharge)base.Clone();

		public new InvoiceChargeLookups Lookups => (InvoiceChargeLookups)base.Lookups;

		public new InvoiceChargeValidation Validation => (InvoiceChargeValidation)base.Validation;

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups() => IsImport ? new ImportInvoiceChargeLookups(this) : new ExportInvoiceChargeLookups(this);

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceChargeValidation(this);

		protected override bool GetJ7_IsDutiable_ReadOnly() => J7_ChargeType.ToString() switch
		{
			Common.CustomsChargeTypeList.Codes.OverseasFreight or
			Common.CustomsChargeTypeList.Codes.OverseasInsurance => true,
			_ => base.GetJ7_IsDutiable_ReadOnly()
		};

		public ZBool IsImport => Invoice?.IsImport ?? ZBool.False;
	}
}
