using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingContainerManyToManyCollection))]
	public class TrackingContainerManyToManyCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			TrackingPackLine packLine = Factory.New<TrackingShipment>().OuterPackLines.AddNew();
			return packLine.Containers;
		}

		public void TestTestingCorrectCollection()
		{
			AssertEquals("Test the correct collection", typeof(TrackingContainerManyToManyCollection), GetCollectionToTest().GetType());
		}
	}
}
