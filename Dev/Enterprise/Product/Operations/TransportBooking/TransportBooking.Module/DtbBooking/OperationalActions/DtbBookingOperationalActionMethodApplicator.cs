using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportBookings.Module
{
	public abstract class DtbBookingOperationalActionMethodApplicator : OperationalActionMethodApplicator
	{
		protected DtbBookingOperationalActionMethodApplicator(string name)
			: base(name)
		{
		}

		protected DtbBookingOperationalActionMethodApplicator(string name, BusinessObjectFactory factory)
			: base(name, factory)
		{
		}

		protected LogControllerLink GetBookingIdLink(DtbBooking booking)
		{
			return new LogControllerLink(booking.KM_JobID, ControllerIDs.DtbBooking, booking.PK);
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			if (targets.Any(b => ((DtbBooking)b).BookingIsBeingManagedByAuthorisedCarrierBookingAgent || ((DtbBooking)b).IsSub))
			{
				SomeTargetsAreLockedDown = true;
				log.Notify(OperationalActionLogErrorLevel.Warning, LockedDownBookingCannotBeModifiedErrorMessage);
			}
		}

		protected bool SomeTargetsAreLockedDown { get; set; }

		protected ZString LockedDownBookingCannotBeModifiedErrorMessage
		{
			get
			{
				return Res.GetString("87636a6a-d735-447c-98df-fd985873eb47", "Cannot modify the selected Transport Booking directly, it has been locked down.");
			}
		}
	}
}
