using CargoWise.Types;

namespace Enterprise.Warehouse.GateManagement.Integration.Interfaces
{
	public interface IGteGateMovement
	{
		ZGuid GGM_GBM_MovementBooking { get; set; }
		ZGuid GGM_GVM_VehicleMovement {  get; set; }
	}
}
