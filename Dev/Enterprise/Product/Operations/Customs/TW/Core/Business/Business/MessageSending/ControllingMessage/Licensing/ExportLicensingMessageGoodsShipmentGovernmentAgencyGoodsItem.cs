using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItem : LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem
	{
		public ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItem(ZInt sequenceNumeric, CusTWControllingMessageHeader header, JobComInvoiceLine invoiceLine) : base(sequenceNumeric, header, invoiceLine)
		{
		}

		protected override ICommodity GetCommodityCore() => new ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity(Header, InvoiceLine);
	}
}
