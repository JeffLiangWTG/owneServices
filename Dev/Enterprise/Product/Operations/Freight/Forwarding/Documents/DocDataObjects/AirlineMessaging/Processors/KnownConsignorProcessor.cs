using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.AirlineMessaging.Processors
{
	public class KnownConsignorProcessor : IUniversalShipmentProcessor
	{
		readonly ForwardingConsol consol;

		public KnownConsignorProcessor(ForwardingConsol consol)
		{
			this.consol = consol;
		}

		public void Process(UniversalDataBuss.DataObjects.Universal.Shipment universalShipment)
		{
			var consolHeader = consol.AWBHeader as ConsolExportAWBHeader;
			if (consolHeader == null || universalShipment?.CarrierDocumentsOverride?.AWBHeader == null)
			{
				return;
			}

			// Set KnownConsignor for Shipment node(ForwardingConsol)
			if (!string.IsNullOrEmpty(consolHeader.EH_KnownConsignorCode))
			{
				universalShipment.CarrierDocumentsOverride.AWBHeader.KnownConsignor = consolHeader.EH_KnownConsignorCode;
			}
		}
	}
}
