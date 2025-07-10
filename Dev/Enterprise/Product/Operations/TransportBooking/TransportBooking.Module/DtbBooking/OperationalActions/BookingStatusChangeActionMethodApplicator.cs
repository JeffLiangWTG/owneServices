using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportBookings.Business;

namespace Enterprise.TransportBookings.Module
{
	public abstract class BookingStatusChangeActionMethodApplicator : DtbBookingOperationalActionMethodApplicator
	{
		protected BookingStatusChangeActionMethodApplicator(string name)
			: base(name)
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
				ChangeTheStatus(booking, log);
			}
		}

		protected abstract IEnumerable<ZString> StatusesToChangeFrom { get; }
		protected abstract ZString StatusToChangeTo { get; }

		void ChangeTheStatus(DtbBooking booking, IOperationalActionSectionLog log)
		{
			var bookingLink = GetBookingIdLink(booking);

			if (StatusesToChangeFrom.Contains(booking.KM_Status))
			{
				booking.KM_Status = StatusToChangeTo;
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, "{0} - {1}", bookingLink, Res.GetString("8a6e9157-ee68-42d3-8db8-0b59c9ecc4da", "The action '{0}' was successfully processed.", Name));
			}
			else
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "{0} - {1}", bookingLink,
					Res.GetString("d1375286-a5e0-40d7-b13e-5905c164ec5a", "The Booking cannot be made {0} because the status is {1}. Only Bookings with a status of {2} can be processed.",
					booking.Lookups.BindToLists.Statuses.GetDescriptionFromCode(StatusToChangeTo),
					booking.StatusDescription,
					string.Join(", ", StatusesToChangeFrom.Select(s => booking.Lookups.BindToLists.Statuses.GetDescriptionFromCode(s)))));
			}
		}
	}
}
