using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.GUI;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.TransportBookings.Module
{
	public class DtbBookingController : DtbBookingControllerShared, IDtbBookingController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.DtbBooking; }
		}

		public void MakeNewBookingsAsQuotes()
		{
			makeNewBookingsAsQuotes = true;
			makeNewBookingsAsMasterBookings = false;
		}

		public void MakeNewBookingsAsMasterBookings(string jobDirection)
		{
			if (!TransportRegistry.Instance.MasterBookingsEnabled.Value)
			{
				ErrorReporter.ReportOnce("Master Bookings not enabled in registry, should not be creating Master Bookings");
			}

			makeNewBookingsAsQuotes = false;
			makeNewBookingsAsMasterBookings = true;
			NewMasterBookingConsolidationJobDirection = jobDirection;
		}

		bool makeNewBookingsAsQuotes;
		bool makeNewBookingsAsMasterBookings;

		protected override void SetupNewBusinessEntity(DtbBooking booking)
		{
			booking.KM_Status = makeNewBookingsAsQuotes ? TransportStatuses.Codes.Quote : TransportStatuses.Codes.Available;
			booking.KM_IsMaster = makeNewBookingsAsMasterBookings;
		}

		public override bool IsFormShownFor(IBusiness businessEntity)
		{
			bool result;

			if (showSingleBookingOnly) // do not check consolidation form (used when openning from the consolidation form)
			{
				result = IsOwnFormShowing(businessEntity);
			}
			else
			{
				result = base.IsFormShownFor(businessEntity);
			}

			return result;
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new DtbBookingConsolidationPlugIn(businessEntity);
		}

		public TransportBookingForm ShowEditFormForSingleBooking(IBusiness businessEntity)
		{
			TransportBookingForm form = null;
			showSingleBookingOnly = true;

			try
			{
				var booking = (DtbBooking)businessEntity;
				form = (TransportBookingForm)ShowEditForm(booking);
			}
			finally
			{
				showSingleBookingOnly = false;
			}

			return form;
		}

		bool showSingleBookingOnly;
	}
}
