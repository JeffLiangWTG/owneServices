using System;
using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.Freight.Business
{
	public interface IAirlineTrackingEventProvider
	{
		IEnumerable<BookingConfirmation> LoadBookingConfirmations();
		IEnumerable<ActualEvent> LoadActualEvents();
		void UpdateShipmentDeliveredTime();
		bool MatchAndUpdateRoutingLeg(BookingConfirmation bookingConfirmation, Func<bool> needUpdate);
		bool MatchAndUpdateRoutingLeg(ActualEvent actualEvent, Func<bool> needUpdate);
		void PropagateTransportEventToConsol(IStmALog log);
	}
}
