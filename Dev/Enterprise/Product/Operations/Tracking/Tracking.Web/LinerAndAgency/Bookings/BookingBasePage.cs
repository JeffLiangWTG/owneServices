using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Web.LinerAndAgency
{
	public abstract class BookingBasePage : LinerAndAgencyBasePage
	{
		protected override string NotFoundLabelText
		{
			get { return Res.GetString("efadcabc-5874-4dbc-abea-a62485c0d8a8", "Booking was not found in the database or you do not have rights to access it."); }
		}

		#region DataSource

		protected override BusinessObject GetNewDataSource()
		{
			TrackingLinerAndAgencyBooking result = null;
			ZGuid bookingPK = GetGuidFromParameter("Ref");

			if (SiteUser != null && SiteUser.LoggedInOrganisation != null && !bookingPK.IsEmpty)
			{
				var filter = new ZDBOnlyQuery(typeof(TrackingLinerAndAgencyBooking));
				filter.AddToFilter(JobShipmentSchema.PK, bookingPK);
				if (!SiteUser.IsShipmentQuickViewUser)
				{
					filter.AddToFilter(OrgRestrictionFilterFactory.Instance.GetFilter<TrackingLinerAndAgencyBooking>());
				}
				filter.IgnoreActiveFilter = true;
				result = Factory.LoadTop1<TrackingLinerAndAgencyBooking>(filter);
			}
			return result;
		}

		public TrackingLinerAndAgencyBooking Booking
		{
			get { return DataSource as TrackingLinerAndAgencyBooking; }
		}

		#endregion

		#region WebInterfacesHelper

		protected override LinerAndAgencyBaseWebInterfacesHelper GetNewWebInterfacesHelper()
		{
			return new LinerAndAgencyBookingWebInterfacesHelper(Booking);
		}

		protected LinerAndAgencyBookingWebInterfacesHelper BookingWebInterfacesHelper
		{
			get { return WebInterfacesHelper as LinerAndAgencyBookingWebInterfacesHelper; }
		}

		#endregion
	}
}
