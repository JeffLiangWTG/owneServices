using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.Bookings;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class BookingNextSailingConfirmationTest : ZPageConfirmationTest
	{
		public override void TestHandleResponse()
		{
			var sailing = Factory.NewWithValidTestData<JobSailing>();
			var scheduleChooser = (TrackingScheduleChooser)TestBooking.QuotedBooking.ScheduleChooser;
			AssertEquals(ZGuid.Empty, TestBooking.QuotedBooking.Booking.JS_JX);

			scheduleChooser.NextSailingPKForTesting = sailing.PK;
			TestConfirmation.ResponseHolder.Value = "Yes";
			TestConfirmation.HandleResponse();
			AssertEquals(sailing.PK, TestBooking.QuotedBooking.Booking.JS_JX);
			AssertEquals(string.Empty, TestConfirmation.ResponseHolder.Value);

			TestBooking.QuotedBooking.Booking.JS_JX = ZGuid.Empty;
			scheduleChooser.NextSailingPKForTesting = sailing.PK;
			TestConfirmation.ResponseHolder.Value = "No";
			TestConfirmation.HandleResponse();
			AssertEquals(ZGuid.Empty, TestBooking.QuotedBooking.Booking.JS_JX);
			AssertEquals(string.Empty, TestConfirmation.ResponseHolder.Value);
		}

		public override void TestIsRequired()
		{
			AssertEquals(false, TestConfirmation.IsRequired);

			var scheduleChooser = (TrackingScheduleChooser)TestBooking.QuotedBooking.ScheduleChooser;
			scheduleChooser.NextSailingPKForTesting = new ZGuid("1bcd1b02-2339-4ed3-8fc1-dacdc930eb05");
			AssertEquals(true, scheduleChooser.HasNextSailing);
			AssertEquals(true, TestConfirmation.IsRequired);
			AssertEquals(true, TestConfirmation.AlwaysRegisterConfirmationScript);

			scheduleChooser.ClearNextSailing();
			AssertEquals(false, TestConfirmation.IsRequired);
			AssertEquals(false, TestConfirmation.AlwaysRegisterConfirmationScript);
		}

		protected override string GetExpectedConfirmationMessage() => "Would you like to populate the latest Sailing schedule?";

		protected override string GetExpectedTitle() => "Automatically populate Sailing?";

		protected override string GetExpectedUserReponseHolderID() => "NextSailingConfirmation";

		protected override ZPageConfirmation GetNewPageConfirmationForTesting() => new BookingNextSailingConfirmation(TestConfirmationPage);

		protected override void SetUp()
		{
			base.SetUp();

			TestSiteUser = new TrackingSiteUser();
			TestBooking = new TrackingBooking(Factory, TestSiteUser);
			TestBooking.QuotedBooking.TransportMode = TransportModes.Sea;
			TestConfirmationPage.SetDataSource(TestBooking);
		}

		TrackingBooking TestBooking;
		TrackingSiteUser TestSiteUser;
	}
}
