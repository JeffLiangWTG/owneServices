using System.Linq;
using Enterprise.Integration.TransportBooking;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportBookings.GUI
{
	class OpenedBookingFormCounter : IOpenedBookingFormCounter
	{
		public int AlreadyOpenedBookingFormCount(IDtbBookingConsolidation consolidation, IDtbBooking excludingBooking = null)
		{
			int count = 0;

			if (consolidation != null)
			{
				if (OpenedFormCache.GetInstance().Contains(consolidation.PK.ToGuid(), ControllerIDs.DtbBookingConsolidation.ToString()))
				{
					count++;
				}

				count += consolidation.Bookings.Count(b =>
					(excludingBooking == null || b.PK != excludingBooking.PK)
					&& OpenedFormCache.GetInstance().Contains(b.PK.ToGuid(), ControllerIDs.DtbBooking.ToString()));
			}

			return count;
		}
	}
}
