using CargoWise.EntityFramework;
using Enterprise.Integration.TransportBooking;

namespace Enterprise.TransportBookings.Shared
{
	public interface IDtbBookingCollection : IActiveBusinessObjectCollection
	{
		new IDtbBooking this[int index] { get; }
	}
}
