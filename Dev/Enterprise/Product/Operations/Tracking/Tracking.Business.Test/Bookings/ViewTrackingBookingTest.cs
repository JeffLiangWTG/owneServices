using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Freight.QuotedBookings.Business.Test;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(ViewTrackingBooking))]
	sealed class ViewTrackingBookingTest : ViewQuotedBookingTest
	{
		[ExpectNoExceptions]
		public void TestLastMilestoneProperties()
		{
			var booking = GetNewBusinessObject() as ViewTrackingBooking;
			AssertNotNull("Booking", booking);
			AssertNotNull("Milestones", booking.TrackingBooking.Milestones);
			AssertNull("LastMilestone", booking.TrackingBooking.Milestones.LastMilestone);
			var label = new ZDateTimeLabel();
			label.BindTo = "TrackingBooking+Milestones+LastMilestone+DisplayDate";
			label.Bind(booking);

			var notificationControl = label;
			var notifications = notificationControl.Notifications;
		}

		[HttpContextEnabledTest]
		public void TestScheduleChooser_Booking()
		{
			var helper = new TestHelper(Factory);
			AssertNotNull(helper.TestSiteUser);
			AssertEquals(helper.TestSiteUser, WebEnv.AppInstance.SiteUser);

			var viewBooking = Factory.New<ViewTrackingBooking>();
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			viewBooking.VB_JS = booking.Booking.PK;

			AssertType(typeof(TrackingScheduleChooser), viewBooking.TrackingBooking.QuotedBooking.ScheduleChooser);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			ViewQuotedBooking viewTrackingBooking = Factory.New<ViewTrackingBooking>();
			viewTrackingBooking.VB_TH = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted).PK;
			return viewTrackingBooking;
		}

		protected override bool IsDeleteSupported()
		{
			return false;
		}

		#endregion
	}
}
