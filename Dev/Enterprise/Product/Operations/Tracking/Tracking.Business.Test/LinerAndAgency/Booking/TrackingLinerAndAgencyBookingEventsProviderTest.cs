namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingLinerAndAgencyBookingEventsProviderTest : ITrackingEventsProviderTest
	{
		#region Overrides

		protected override ITrackingEventsProvider GetNewTestEventsProvider()
		{
			return Factory.NewWithValidTestData<TrackingLinerAndAgencyBooking>();
		}

		#endregion
	}
}
