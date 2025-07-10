using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class NX201_01ImportLicensingMessageGoodsShipment : LicensingMessageGoodsShipment
	{
		public NX201_01ImportLicensingMessageGoodsShipment(CusTWControllingMessageHeader header) : base(header)
		{
		}

		protected override IPartyDetails GetBuyerCore() => default;

		protected override IPartyDetails GetConsigneeCore() => default;

		protected override LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem GenerateLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCore(ZInt sequenceNumeric, CusTWControllingMessageHeader header, JobComInvoiceLine invoiceLine)
		{
			return new NX201_01LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem(sequenceNumeric, header, invoiceLine);
		}
	}
}
