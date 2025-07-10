using System.Collections.Generic;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Shared
{
	public interface ITransportBookingGUIProvider
	{
		IEnumerable<IDtbBooking> GetBookingsToDeliver(IDtbBookingParent parent, IStmMenuItem menu);
		IEnumerable<IDtbBooking> GetBookingsToDeliver(IDtbBookingParent[] parents, IStmMenuItem menu);
		IEnumerable<IDtbBooking> GetBookingsToDeliver(IDtbBookingParent parents, DtbBookingDirection direction, bool combineContainers);
	}
}
