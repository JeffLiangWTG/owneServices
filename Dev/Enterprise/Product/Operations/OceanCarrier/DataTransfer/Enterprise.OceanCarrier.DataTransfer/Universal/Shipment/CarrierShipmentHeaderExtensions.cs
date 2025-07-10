using System.Linq;
using Enterprise.Core;
using Enterprise.OceanCarrier.Business;

namespace Enterprise.OceanCarrier.DataTransfer.Universal.Shipment
{
	public static class CarrierShipmentHeaderExtensions
	{
		public static CarrierShipmentCargo GetMatchedContainerList(this CarrierShipmentHeader carrierShipmentHeader, string containerNumber)
		{
			if (carrierShipmentHeader.Cargoes.Count == 0)
			{
				return null;
			}

			return carrierShipmentHeader.Cargoes
				.FirstOrDefault(x => x.CSC_EquipmentNo == containerNumber && x.CSC_CargoType == Constants.ContainerModes.Containerised);
		}

		public static CarrierShipmentCargo GetMatchedBreakBulkList(this CarrierShipmentHeader carrierShipmentHeader, string referenceNumber)
		{
			if (carrierShipmentHeader.Cargoes.Count == 0)
			{
				return null;
			}

			return carrierShipmentHeader
				.Cargoes
				.FirstOrDefault(x => x.CSC_IdentificationReference == referenceNumber && x.CSC_CargoType == Constants.ContainerModes.BreakBulk);
		}
	}
}
