using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class NX401ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItem : ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItem
	{
		public NX401ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItem(ZInt sequenceNumeric, CusTWControllingMessageHeader header, JobComInvoiceLine invoiceLine) : base(sequenceNumeric, header, invoiceLine)
		{
		}

		protected override ICommodity GetCommodityCore() => new NX401ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity(Header, InvoiceLine);
	}
}
