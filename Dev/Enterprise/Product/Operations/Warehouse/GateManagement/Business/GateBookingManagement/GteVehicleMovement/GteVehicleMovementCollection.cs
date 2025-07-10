using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.GateManagement.Business
{
	public class GteVehicleMovementCollection : ActiveBusinessObjectCollection<GteVehicleMovement>
	{
		public GteVehicleMovementCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
