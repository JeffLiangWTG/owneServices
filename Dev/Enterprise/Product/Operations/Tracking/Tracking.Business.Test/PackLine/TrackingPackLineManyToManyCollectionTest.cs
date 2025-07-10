using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingPackLineManyToManyCollection))]
	public class TrackingPackLineManyToManyCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			TrackingContainer container = Factory.New<TrackingContainer>();
			return container.PackLines;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<TrackingShipment>().OuterPackLines.AddNew();
		}

		public void TestTestingCorrectCollection()
		{
			AssertEquals("Test the correct collection", typeof(TrackingPackLineManyToManyCollection), GetCollectionToTest().GetType());
		}
	}
}
