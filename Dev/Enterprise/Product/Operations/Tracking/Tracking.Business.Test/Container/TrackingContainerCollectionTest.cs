using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingContainerCollection))]
	public class TrackingContainerCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			TrackingConsol trackingConsol = Factory.New<TrackingConsol>();
			return trackingConsol.Containers;
		}

		public void TestTestingCorrectCollection()
		{
			AssertEquals("Test the correct collection", typeof(TrackingContainerCollection), GetCollectionToTest().GetType());
		}
	}
}
