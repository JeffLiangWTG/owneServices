using System;

namespace Enterprise.eTail.Integration
{
	public interface ILastMileCarrierBookingService
	{
		ILastMileCarrierBookingResponseCollection BookLastMileCarrier(Guid entityPK, bool saveToeDocs = true);
		ILastMileCarrierBookingResponseCollection CancelBooking(Guid entityPK);
	}
}
