using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class NX401ImportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity : LicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity
	{
		public NX401ImportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity(CusTWControllingMessageHeader header, JobComInvoiceLine invoiceLine) : base(header, invoiceLine)
		{
		}

		protected override ZDecimal GetItemChargeAmountCore() => ZDecimal.Zero;
	}
}
