using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business
{
	public sealed class ImportInvoiceChargeLookups : InvoiceChargeLookups
	{
		public ImportInvoiceChargeLookups(InvoiceCharge invoiceCharge) : base(invoiceCharge)
		{
		}

		public override CodeDescriptionPairList ChargeTypeList => Factory.GetCachedValue<NOInvoiceChargeTypesImport>();
	}
}
