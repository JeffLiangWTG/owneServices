using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingWhsOrderLine))]
	sealed class TrackingWhsOrderLineForNonPersistentBoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return TrackingHelper.Get(Factory.New<WhsOrderLine>());
		}
	}
}
