using System.Collections.Generic;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class NX401ExportLicensingMessageGoodsShipmentConsignment : ExportLicensingMessageGoodsShipmentConsignment
	{
		public NX401ExportLicensingMessageGoodsShipmentConsignment(CusTWControllingMessageHeader header) : base(header)
		{
		}

		protected override ITransportMeans GetBorderTransportMeansCore()
		{
			return new NX401ExportGoodsShipmentConsignmentBorderTransportMeans(Declaration);
		}

		protected override IEnumerable<ITransportContractDocument> GetTransportContractDocumentsCore()
		{
			return default;
		}
	}
}
