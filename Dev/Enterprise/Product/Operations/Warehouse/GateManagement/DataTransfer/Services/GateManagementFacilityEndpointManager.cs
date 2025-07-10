using Enterprise.Warehouse.GateManagement.Integration;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	public abstract class GateManagementFacilityEndpointManager
	{
		public abstract IGateBookingValidationResponse ValidateBooking(IGateBookingValidationRequest request);
	}
}
