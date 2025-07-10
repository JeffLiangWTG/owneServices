using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business
{
	[TestedType(typeof(TrackingLegacyOrderCollection))]
	sealed class TrackingLegacyOrderCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(TrackingOrder));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TrackingLegacyOrderCollection(Factory);
		}
	}
}
