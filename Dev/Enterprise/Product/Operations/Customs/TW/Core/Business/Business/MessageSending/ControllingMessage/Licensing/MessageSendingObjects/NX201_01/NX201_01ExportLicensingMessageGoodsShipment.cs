using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class NX201_01ExportLicensingMessageGoodsShipment : ExportLicensingMessageGoodsShipment
	{
		public NX201_01ExportLicensingMessageGoodsShipment(CusTWControllingMessageHeader header) : base(header)
		{
		}

		protected override LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem GenerateLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCore(ZInt sequenceNumeric, CusTWControllingMessageHeader header, JobComInvoiceLine invoiceLine)
		{
			return new NX201_01ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItem(sequenceNumeric, header, invoiceLine);
		}
	}
}
