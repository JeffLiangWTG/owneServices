using CargoWise.Common;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	class AgencyContainersInfo
	{
		public AgencyContainersInfo(AgencyShipment shipment, bool isVerifiedGrossContainerWeight)
		{
			Shipment = Argument.NotNull(shipment, nameof(shipment));
			IsVGM = isVerifiedGrossContainerWeight && !shipment.IsBillOfLadingStage;
		}

		public AgencyShipment Shipment { get; }
		public bool IsVGM { get; }

		public AgencyShipmentContainerDependentCollection CollectionToImportTo
		{
			get { return IsVGM ? Shipment.RealContainers : Shipment.ShippingContainers; }
		}
	}
}
