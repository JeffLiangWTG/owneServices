using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business
{
	public sealed class ExportInvoiceChargeLookups : InvoiceChargeLookups
	{
		public ExportInvoiceChargeLookups(InvoiceCharge invoiceCharge) : base(invoiceCharge)
		{
		}

		public override CodeDescriptionPairList ChargeTypeList => Factory.GetCachedValue<NOInvoiceChargeTypesExport>();
	}
}
