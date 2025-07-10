namespace Enterprise.Tracking.Business.Testing
{
	sealed class CFSShipmentEventsProviderTest : ITrackingEventsProviderTest
	{
		#region Overrides

		protected override ITrackingEventsProvider GetNewTestEventsProvider()
		{
			var result = Factory.NewWithValidTestData<TrackingCFSShipment>();
			result.SiteUser = SiteUser;
			return result;
		}

		#endregion
	}
}
