using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingBillOfLadingCollection))]
	sealed class TrackingBillOfLadingCollectionTest : ActiveBusinessObjectCollectionTestCase<TrackingBillOfLadingCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(TrackingBillOfLading));
		}

		protected override TrackingBillOfLadingCollection GetCollectionToTest()
		{
			return new TrackingBillOfLadingCollection(Factory);
		}
	}
}
