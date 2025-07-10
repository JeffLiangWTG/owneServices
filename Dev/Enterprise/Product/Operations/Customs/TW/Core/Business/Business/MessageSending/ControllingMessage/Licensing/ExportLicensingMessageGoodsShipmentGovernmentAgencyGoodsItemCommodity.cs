using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity : LicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity
	{
		public ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity(CusTWControllingMessageHeader header, JobComInvoiceLine invoiceLine) : base(header, invoiceLine)
		{
		}

		protected override ZString GetCurrentCodeCore() => default;
	}
}
