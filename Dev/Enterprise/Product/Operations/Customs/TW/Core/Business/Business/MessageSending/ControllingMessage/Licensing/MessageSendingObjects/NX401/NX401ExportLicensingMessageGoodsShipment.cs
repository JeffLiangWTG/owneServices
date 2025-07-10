using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class NX401ExportLicensingMessageGoodsShipment : LicensingMessageGoodsShipment
	{
		public NX401ExportLicensingMessageGoodsShipment(CusTWControllingMessageHeader header) : base(header)
		{
		}

		protected override IConsignment GetConsignmentCore() => new NX401ExportLicensingMessageGoodsShipmentConsignment(Header);

		protected override LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem GenerateLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCore(ZInt sequenceNumeric, CusTWControllingMessageHeader header, JobComInvoiceLine invoiceLine)
		{
			return new NX401ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItem(sequenceNumeric, header, invoiceLine);
		}

		protected override IPartyDetails GetSellerCore() => default;
	}
}
