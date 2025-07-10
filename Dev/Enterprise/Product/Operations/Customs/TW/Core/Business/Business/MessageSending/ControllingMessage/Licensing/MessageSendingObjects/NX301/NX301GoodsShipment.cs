using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class NX301GoodsShipment : LicensingMessageGoodsShipment
	{
		public NX301GoodsShipment(CusTWControllingMessageHeader header) : base(header)
		{
		}

		protected override LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem GenerateLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCore(ZInt sequenceNumeric, CusTWControllingMessageHeader header, JobComInvoiceLine invoiceLine)
		{
			return new NX301GoodsShipmentGovernmentAgencyGoodsItem(sequenceNumeric, header, invoiceLine);
		}
	}
}
