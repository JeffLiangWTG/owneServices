namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingCartageEventsProviderTest : ITrackingEventsProviderTest
	{
		#region Overrides

		protected override ITrackingEventsProvider GetNewTestEventsProvider()
		{
			return Factory.NewWithValidTestData<TrackingCartage>();
		}

		#endregion
	}
}
