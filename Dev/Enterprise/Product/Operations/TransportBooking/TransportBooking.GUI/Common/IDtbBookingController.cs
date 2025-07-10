using CargoWise.EntityFramework;

namespace Enterprise.TransportBookings.GUI
{
	public interface IDtbBookingController
	{
		TransportBookingForm ShowEditFormForSingleBooking(IBusiness businessEntity);
	}
}
