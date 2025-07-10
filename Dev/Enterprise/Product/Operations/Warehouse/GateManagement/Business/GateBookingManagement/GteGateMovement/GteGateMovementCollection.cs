using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.GateManagement.Business
{
	public class GteGateMovementCollection : ActiveBusinessObjectCollection<GteGateMovement>
	{
		public GteGateMovementCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GteGateMovementCollection(GteVehicleMovement vehicleMovement) : base(vehicleMovement.Factory, vehicleMovement, null, GteGateMovementSchema.GGM_GVM_VehicleMovement)
		{
		}

		public GteGateMovementCollection(GteGateMovementBooking gateMovementBooking) : base(gateMovementBooking.Factory, gateMovementBooking, null, GteGateMovementSchema.GGM_GBM_MovementBooking)
		{
		}
	}
}
