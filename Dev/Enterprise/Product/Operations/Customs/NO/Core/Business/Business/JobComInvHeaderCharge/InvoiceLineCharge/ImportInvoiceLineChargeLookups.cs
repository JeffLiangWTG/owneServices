using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business
{
	public sealed class ImportInvoiceLineChargeLookups : InvoiceLineChargeLookups
	{
		public ImportInvoiceLineChargeLookups(InvoiceLineCharge invoiceLineCharge) : base(invoiceLineCharge)
		{
		}

		public override CodeDescriptionPairList ChargeTypeList => Factory.GetCachedValue<NOInvoiceChargeTypesImport>();
	}
}
