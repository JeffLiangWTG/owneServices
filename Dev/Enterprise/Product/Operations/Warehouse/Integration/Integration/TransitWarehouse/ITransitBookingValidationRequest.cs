namespace Enterprise.Warehouse.Integration
{
	public interface ITransitBookingValidationRequest
	{
		ITransitBookingInfo GetBooking(IGateBookingRequest request);
	}
}
