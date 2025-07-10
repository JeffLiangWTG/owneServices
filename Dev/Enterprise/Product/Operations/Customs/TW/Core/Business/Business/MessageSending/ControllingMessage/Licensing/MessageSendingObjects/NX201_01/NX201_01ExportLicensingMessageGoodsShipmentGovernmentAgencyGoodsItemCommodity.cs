using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class NX201_01ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity : ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity
	{
		public NX201_01ExportLicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity(CusTWControllingMessageHeader header, JobComInvoiceLine invoiceLine) : base(header, invoiceLine)
		{
		}

		protected override ZDecimal GetUnitPriceAmountCore()
		{
			return ZDecimal.Zero;
		}

		protected override IEnumerable<IAdditionalDocument> GetAdditionalDocuments(JobComInvoiceLine invoiceLine)
		{
			return Enumerable.Empty<IAdditionalDocument>();
		}
	}
}
