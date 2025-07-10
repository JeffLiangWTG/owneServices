using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration.TransportBooking;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.Freight.Forwarding.Business;

public static class TransportBookingHelper
{
	public static bool HasTransportBooking(this IDtbBookingParent parent, DtbBookingDirection direction, bool includeInactiveTBs = false)
	{
		var transportBookings = TransportBookingLoader.GetBookingConsolidations(parent)
		.Where(dtbConsol => dtbConsol.KB_JobDirection == direction.ToString())
		.SelectMany(dtbConsol => dtbConsol.Bookings);

		return includeInactiveTBs ? transportBookings.Any() : transportBookings.Any(booking => !(booking as ICancellable)?.IsCancelled ?? false);
	}

	public static IEnumerable<IDtbBooking> GetTransportBookings(this IDtbBookingParent parent, DtbBookingDirection direction = DtbBookingDirection.None, bool includeInactiveTBs = false)
	{
		var transportBookings = TransportBookingLoader.GetBookingConsolidations(parent)
			.Where(dtbConsol => direction is DtbBookingDirection.None || dtbConsol.KB_JobDirection == direction.ToString())
			.SelectMany(dtbConsol => dtbConsol.Bookings);

		return includeInactiveTBs ? transportBookings : transportBookings.Where(booking => !(booking as ICancellable)?.IsCancelled ?? false);
	}
}
