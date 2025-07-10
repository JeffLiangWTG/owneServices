using System.Collections.Generic;
using Enterprise.Integration.TransportBooking;

namespace Enterprise.TransportBookings.Shared
{
	public interface IDtbDeliveryManager
	{
		IEnumerable<IDtbBooking> DeliverTransportBooking();
	}
}
