namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingShipmentEventsProviderTest : ITrackingEventsProviderTest
	{
		#region Overrides

		protected override ITrackingEventsProvider GetNewTestEventsProvider()
		{
			var result = Factory.NewWithValidTestData<TrackingShipment>();
			result.SiteUser = SiteUser;
			return result;
		}

		#endregion
	}
}
