using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class NX401ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity : ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity
	{
		public NX401ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity(CusTWControllingMessageHeader header, JobComInvoiceLine invoiceLine) : base(header, invoiceLine)
		{
		}

		protected override ZDecimal GetAdValoremTaxBaseAmountCore() => ZDecimal.Zero;
	}
}
