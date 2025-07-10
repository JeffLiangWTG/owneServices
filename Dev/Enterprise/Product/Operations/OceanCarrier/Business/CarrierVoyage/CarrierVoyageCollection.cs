using CargoWise.EntityFramework;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierVoyageCollection : ActiveBusinessObjectCollection<CarrierVoyage>
	{
		public CarrierVoyageCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
