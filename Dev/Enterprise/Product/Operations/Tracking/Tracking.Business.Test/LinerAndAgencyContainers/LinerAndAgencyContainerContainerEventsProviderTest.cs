namespace Enterprise.Tracking.Business.Testing
{
	sealed class LinerAndAgencyContainerContainerEventsProviderTest : ITrackingEventsProviderTest
	{
		#region Overrides

		protected override ITrackingEventsProvider GetNewTestEventsProvider()
		{
			return Factory.NewWithValidTestData<LinerAndAgencyContainer>();
		}

		#endregion
	}
}
