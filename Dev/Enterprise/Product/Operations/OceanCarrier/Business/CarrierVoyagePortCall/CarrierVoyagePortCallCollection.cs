using CargoWise.EntityFramework;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierVoyagePortCallCollection : ActiveBusinessObjectCollection<CarrierVoyagePortCall>
	{
		public CarrierVoyagePortCallCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
