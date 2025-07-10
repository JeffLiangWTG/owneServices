using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Bookings
{
	public class BookingNextSailingConfirmation : ZPageConfirmation
	{
		public BookingNextSailingConfirmation(ZPage page)
			: base(page)
		{
		}

		TrackingBooking Booking => (TrackingBooking)base.DataSource;
		TrackingScheduleChooser ScheduleChooser => (TrackingScheduleChooser)Booking?.QuotedBooking?.ScheduleChooser;

		protected override ConfirmationTypes ConfirmationType => ConfirmationTypes.None;

		protected override bool GetRequired() => ScheduleChooser?.HasNextSailing ?? false;

		string SailingText => ScheduleChooser?.SailingText;

		protected override string GetTitle() => Res.GetString("622ec331-8297-4096-b1b4-10fbaea78818", "Automatically populate {0}?", SailingText);

		protected override string GetConfirmationMessage() => Res.GetString("17ada775-42f0-47fd-bc6e-ef2a460541bb", "Would you like to populate the latest {0} schedule?", SailingText);

		protected override string GetUserResponseHolderID() => "NextSailingConfirmation";

		protected override void HandleResponseCore()
		{
			base.HandleResponseCore();

			if (YesButtonClicked)
			{
				ScheduleChooser?.SetNextSailing();
			}
			else
			{
				ScheduleChooser?.ClearNextSailing();
			}

			ResponseHolder.Value = string.Empty;
		}

		public override bool AlwaysRegisterConfirmationScript => IsRequired;
	}
}
