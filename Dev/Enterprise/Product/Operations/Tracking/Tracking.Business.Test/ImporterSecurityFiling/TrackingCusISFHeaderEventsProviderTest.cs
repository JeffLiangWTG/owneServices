using CargoWise.Types;
using Enterprise.Tracking.Business.ImporterSecurityFiling;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingCusISFHeaderEventsProviderTest : ITrackingEventsProviderTest
	{
		#region Overrides

		protected override ITrackingEventsProvider GetNewTestEventsProvider()
		{
			var result = Factory.NewWithValidTestData<TrackingCusISFHeader>();
			result.FirstTransport.JW_ETD = ZDateTime.Now;
			return result;
		}

		#endregion
	}
}
