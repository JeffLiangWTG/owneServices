using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class NX401ImportLicensingMessageGoodsShipment : LicensingMessageGoodsShipment
	{
		public NX401ImportLicensingMessageGoodsShipment(CusTWControllingMessageHeader header) : base(header)
		{
		}

		protected override LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem GenerateLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCore(ZInt sequenceNumeric, CusTWControllingMessageHeader header, JobComInvoiceLine invoiceLine)
		{
			return new NX401ImportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItem(sequenceNumeric, header, invoiceLine);
		}

		protected override bool ConsigneeIdAndTypeCodeIsRequired => true;

		protected override ZDecimal GetItemChargeAmountCore() => ZDecimal.Zero;

		protected override IPartyDetails GetExporterCore() => default;
	}
}
