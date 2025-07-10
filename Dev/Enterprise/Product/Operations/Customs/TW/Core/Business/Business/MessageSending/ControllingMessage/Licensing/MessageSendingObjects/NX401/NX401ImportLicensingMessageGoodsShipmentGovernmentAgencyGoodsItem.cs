using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class NX401ImportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItem : LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem
	{
		public NX401ImportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItem(ZInt sequenceNumeric, CusTWControllingMessageHeader header, JobComInvoiceLine invoiceLine) : base(sequenceNumeric, header, invoiceLine)
		{
		}

		protected override ICommodity GetCommodityCore() => new NX401ImportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity(Header, InvoiceLine);
	}
}
