namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingOrderEventsProviderTest : ITrackingEventsProviderTest
	{
		#region Overrides

		protected override ITrackingEventsProvider GetNewTestEventsProvider()
		{
			return Factory.NewWithValidTestData<TrackingOrder>();
		}

		#endregion
	}
}
