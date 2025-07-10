using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingEventsCollection))]
	sealed class TrackingEventsCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestFetchStrategy()
		{
			var events = new TrackingEventsCollection(Factory);

			AssertType<TrackingEventsFetchStrategy>(events.FetchStrategy);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new TrackingEventsCollection(Factory);
	}
}
