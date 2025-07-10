using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business
{
	public sealed class ExportInvoiceLineChargeLookups : InvoiceLineChargeLookups
	{
		public ExportInvoiceLineChargeLookups(InvoiceLineCharge invoiceLineCharge) : base(invoiceLineCharge)
		{
		}

		public override CodeDescriptionPairList ChargeTypeList => Factory.GetCachedValue<NOInvoiceChargeTypesExport>();
	}
}
