using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingCFSShipmentsCollection))]
	sealed class TrackingCFSShipmentsCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TrackingCFSShipmentsCollection(Factory);
		}

		public void TestTestingCorrectCollection()
		{
			AssertEquals("Test the correct collection", typeof(TrackingCFSShipmentsCollection), GetCollectionToTest().GetType());
		}
	}
}
