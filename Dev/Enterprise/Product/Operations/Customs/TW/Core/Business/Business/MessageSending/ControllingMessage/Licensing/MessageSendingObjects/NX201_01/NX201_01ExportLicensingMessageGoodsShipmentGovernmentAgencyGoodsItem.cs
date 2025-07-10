using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class NX201_01ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItem : NX201_01LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem
	{
		public NX201_01ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItem(ZInt sequenceNumeric, CusTWControllingMessageHeader header, JobComInvoiceLine invoiceLine) : base(sequenceNumeric, header, invoiceLine)
		{
		}

		protected override ZString GetCountryCodeCore()
		{
			return ZString.Empty;
		}

		protected override ICommodity GetCommodityCore() => new NX201_01ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity(Header, InvoiceLine);
	}
}
