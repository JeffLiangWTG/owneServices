using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingMAWBHeaderCollection))]
	public class TrackingMAWBCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TrackingMAWBHeaderCollection(Factory);
		}

		public void TestTestingCorrectCollection()
		{
			AssertEquals("Test the correct collection", typeof(TrackingMAWBHeaderCollection), GetCollectionToTest().GetType());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<TrackingMAWBHeader>();
		}
	}
}
