using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingWhsOrderEventsProviderTest : ITrackingEventsProviderTest
	{
		#region Overrides

		protected override ITrackingEventsProvider GetNewTestEventsProvider()
		{
			return TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrder>());
		}

		#endregion
	}
}
