using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	public class ViewTrackingBooking : ViewQuotedBooking
	{
		public ViewTrackingBooking(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public TrackingBooking TrackingBooking => trackingBooking ?? (trackingBooking = new TrackingBooking(VB_JS, VB_TH, Factory, WebEnv.AppInstance?.SiteUser as TrackingSiteUser));
		TrackingBooking trackingBooking;
	}
}
