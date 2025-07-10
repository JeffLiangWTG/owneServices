using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business
{
	[TestedType(typeof(TrackingOrderCollection))]
	sealed class TrackingOrderCollectionTest : ActiveBusinessObjectCollectionTestCase<TrackingOrderCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(TrackingOrder));
		}

		protected override TrackingOrderCollection GetCollectionToTest()
		{
			return new TrackingOrderCollection(Factory);
		}
	}
}
