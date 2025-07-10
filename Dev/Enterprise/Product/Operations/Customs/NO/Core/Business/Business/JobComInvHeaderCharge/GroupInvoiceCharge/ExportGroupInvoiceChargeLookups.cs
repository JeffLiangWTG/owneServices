using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business
{
	public sealed class ExportGroupInvoiceChargeLookups : GroupInvoiceChargeLookups
	{
		public ExportGroupInvoiceChargeLookups(GroupInvoiceCharge groupInvoiceCharge) : base(groupInvoiceCharge)
		{
		}

		public override CodeDescriptionPairList ChargeTypeList => Factory.GetCachedValue<NOInvoiceChargeTypesExport>();
	}
}
