namespace Enterprise.Tracking.Business.Testing
{
	public class TrackingContainerEventsProviderTest : ITrackingEventsProviderTest
	{
		#region Overrides

		protected override ITrackingEventsProvider GetNewTestEventsProvider()
		{
			return Factory.NewWithValidTestData<TrackingContainer>();
		}

		#endregion
	}
}
