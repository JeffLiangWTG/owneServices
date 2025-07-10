using Enterprise.Customs.Business;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingDeclarationEventsProviderTest : ITrackingEventsProviderTest
	{
		#region Overrides

		protected override ITrackingEventsProvider GetNewTestEventsProvider()
		{
			return new TrackingDeclaration(Factory.NewWithValidTestData<BaseJobDeclaration>(), SiteUser);
		}

		#endregion
	}
}
