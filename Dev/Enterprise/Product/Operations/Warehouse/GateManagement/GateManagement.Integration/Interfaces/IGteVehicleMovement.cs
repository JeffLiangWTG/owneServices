using CargoWise.Types;

namespace Enterprise.Warehouse.GateManagement.Integration
{
	public interface IGteVehicleMovement
	{
		ZGuid PK { get; }
		ZString GVM_VehicleRegistration { get; set; }
		string[] FacilityTypes { get; }
		ZGuid GVM_GBK_MainBooking { get; set; }
	}
}
