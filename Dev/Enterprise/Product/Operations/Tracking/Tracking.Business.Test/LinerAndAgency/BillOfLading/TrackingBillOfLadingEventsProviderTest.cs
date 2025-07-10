namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingBillOfLadingEventsProviderTest : ITrackingEventsProviderTest
	{
		#region Overrides

		protected override ITrackingEventsProvider GetNewTestEventsProvider()
		{
			return Factory.NewWithValidTestData<TrackingBillOfLading>();
		}

		#endregion
	}
}
