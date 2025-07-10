using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportBookings.Business;

namespace Enterprise.TransportBookings.Module
{
	public class RemoveBookingHeldStatusActionMethodApplicator : DtbBookingOperationalActionMethodApplicator
	{
		public RemoveBookingHeldStatusActionMethodApplicator()
			: base(Res.GetString("1d366311-ef2e-4f4f-9e50-485f249af68a", "Remove Held Status"))
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] bookings)
		{
			base.ApplyCore(log, bookings);
			if (SomeTargetsAreLockedDown)
			{
				SomeTargetsAreLockedDown = false;
				return;
			}

			log.SetSectionProgressMax(bookings.Length);

			foreach (DtbBooking booking in bookings)
			{
				log.BumpSectionProgress();
				RemoveHeldStatusFromBooking(booking, log);
			}
		}

		void RemoveHeldStatusFromBooking(DtbBooking booking, IOperationalActionSectionLog log)
		{
			var bookingLink = GetBookingIdLink(booking);

			if (booking.IsHeld)
			{
				booking.UpdateStatus();
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, "{0} - {1}", bookingLink,
					Res.GetString("457aafe6-4c85-4793-8f63-dfbdef24a57d", "The action 'Remove Held Status' was successfully processed."));
			}
			else
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "{0} - {1}", bookingLink,
					Res.GetString("0fb74f6a-4581-4e35-bd34-062e5ed99800", "The booking status must be 'Held' to make this change."));
			}
		}
	}
}
