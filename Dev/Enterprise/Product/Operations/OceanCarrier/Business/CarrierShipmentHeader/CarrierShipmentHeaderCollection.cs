using CargoWise.EntityFramework;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierShipmentHeaderCollection : ActiveBusinessObjectCollection<CarrierShipmentHeader>
	{
		public CarrierShipmentHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
