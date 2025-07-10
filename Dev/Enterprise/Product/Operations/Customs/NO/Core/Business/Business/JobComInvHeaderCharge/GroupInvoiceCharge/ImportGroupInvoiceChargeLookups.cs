using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business
{
	public sealed class ImportGroupInvoiceChargeLookups : GroupInvoiceChargeLookups
	{
		public ImportGroupInvoiceChargeLookups(GroupInvoiceCharge groupInvoiceCharge) : base(groupInvoiceCharge)
		{
		}

		public override CodeDescriptionPairList ChargeTypeList => Factory.GetCachedValue<NOInvoiceChargeTypesImport>();
	}
}
