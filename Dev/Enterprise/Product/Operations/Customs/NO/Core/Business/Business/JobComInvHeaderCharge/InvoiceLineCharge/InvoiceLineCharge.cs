using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NO.Business
{
	public class InvoiceLineCharge : Customs.Business.BaseInvoiceLineCharge
	{
		public InvoiceLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new InvoiceLineCharge Clone() => (InvoiceLineCharge)base.Clone();

		public new InvoiceLineChargeValidation Validation => (InvoiceLineChargeValidation)base.Validation;

		public new InvoiceLineChargeLookups Lookups => (InvoiceLineChargeLookups)base.Lookups;

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceLineChargeValidation(this);

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups() => this switch
		{
			{ IsImport: true } => new ImportInvoiceLineChargeLookups(this),
			{ IsExport: true } => new ExportInvoiceLineChargeLookups(this),
			_ => new InvoiceLineChargeLookups(this),
		};

		bool IsImport => InvoiceLine?.IsImport ?? ZBool.False;
		bool IsExport => InvoiceLine?.IsExport ?? ZBool.False;
	}
}
