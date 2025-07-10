using Enterprise.Customs.DataTransfer.Universal;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class ShipmentDataObjectWriterHelper : UniversalCommonHelper
	{
		public ShipmentDataObjectWriterHelper(Shipment shipmentBO, Trip parentBO) : base(shipmentBO.Factory)
		{
			this.thisShipmentBO = shipmentBO;
			this.tripBO = parentBO;
		}

		public readonly Shipment thisShipmentBO;
		public readonly Trip tripBO;
	}
}
