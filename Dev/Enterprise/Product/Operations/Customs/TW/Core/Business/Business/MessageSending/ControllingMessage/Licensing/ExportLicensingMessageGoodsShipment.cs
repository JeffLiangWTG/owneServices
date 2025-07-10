using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class ExportLicensingMessageGoodsShipment : LicensingMessageGoodsShipment
	{
		public ExportLicensingMessageGoodsShipment(CusTWControllingMessageHeader header) : base(header)
		{
		}

		protected override ZDateTime GetExitDateTimeCore() => ZDateTime.Empty;

		protected override ZDecimal GetItemChargeAmountCore() => Declaration.EntryHeader?.CH_TotalCustomsValueInLocalCurrency ?? default;

		protected override IPartyDetails GetSellerCore() => default;

		protected override IConsignment GetConsignmentCore() => new ExportLicensingMessageGoodsShipmentConsignment(Header);

		protected override LicensingMessageGoodsShipmentGovernmentAgencyGoodsItem GenerateLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCore(ZInt sequenceNumeric, CusTWControllingMessageHeader header, JobComInvoiceLine invoiceLine)
		{
			return new ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItem(sequenceNumeric, header, invoiceLine);
		}
	}
}
