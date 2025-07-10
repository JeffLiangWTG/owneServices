using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingOrderLineCollection))]
	sealed class TrackingOrderLineCollectionBOTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(TrackingOrderLine));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TrackingOrderLineCollection(Factory);
		}
	}
}
