using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	[SetGlobalsIsWeb]
	sealed class TrackingWhsReceiveEventsProviderTest : ITrackingEventsProviderTest
	{
		#region Overrides

		protected override ITrackingEventsProvider GetNewTestEventsProvider()
		{
			return TrackingHelper.Get(Factory.NewWithValidTestData<WhsReceive>());
		}

		#endregion
	}
}
