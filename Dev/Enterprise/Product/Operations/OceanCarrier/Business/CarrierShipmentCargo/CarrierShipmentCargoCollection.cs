using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierShipmentCargoCollection : ActiveBusinessObjectCollection<CarrierShipmentCargo>
	{
		public CarrierShipmentCargoCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CarrierShipmentCargoCollection(CarrierShipmentHeader carrierShipmentHeader)
			: base(carrierShipmentHeader.Factory, carrierShipmentHeader, new ZQuery(), CarrierShipmentCargoSchema.CSC_CSH_CarrierShipment)
		{
		}
	}
}
