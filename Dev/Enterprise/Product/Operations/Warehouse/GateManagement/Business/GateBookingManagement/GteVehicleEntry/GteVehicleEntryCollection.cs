using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.GateManagement.Business
{
	public class GteVehicleEntryCollection : ActiveBusinessObjectCollection<GteVehicleEntry>
	{
		public GteVehicleEntryCollection(GteVehicleMovement vehicleMovement)
			: base(vehicleMovement.Factory, vehicleMovement, null, GteVehicleEntrySchema.GVE_GVM_VehicleMovement)
		{
		}

		public GteVehicleEntryCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
