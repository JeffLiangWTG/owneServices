using Enterprise.Integration.TransportBooking;

namespace Enterprise.TransportBookings.Shared
{
	public interface IOpenedBookingFormCounter
	{
		int AlreadyOpenedBookingFormCount(IDtbBookingConsolidation consolidation, IDtbBooking excludingBooking = null);
	}
}

