namespace Enterprise.Tracking.Business.Testing
{
	public class TrackingBookingEventsProviderTest : ITrackingEventsProviderTest
	{
		#region Overrides

		protected override ITrackingEventsProvider GetNewTestEventsProvider()
		{
			return new TrackingBooking(Factory, SiteUser);
		}

		#endregion
	}
}
