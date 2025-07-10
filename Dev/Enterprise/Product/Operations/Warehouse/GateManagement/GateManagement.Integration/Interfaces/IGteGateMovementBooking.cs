using CargoWise.Types;

namespace Enterprise.Warehouse.GateManagement.Integration.Interfaces
{
	public interface IGteGateMovementBooking
	{
		ZGuid PK { get; }
		ZGuid GBM_GBK_Booking { get; set; }
		ZString GBM_BookingReferenceNumber { get; }
		ZString GBM_SourceReferenceNumber { get; }
		ZString GBM_UnitNumber { get; }
		ZBool GBM_IsPickup { get; }
		ZGuid GBM_RC_UnitType { get; }
		ZGuid GBM_FacilityJobId { get; }
	}
}
