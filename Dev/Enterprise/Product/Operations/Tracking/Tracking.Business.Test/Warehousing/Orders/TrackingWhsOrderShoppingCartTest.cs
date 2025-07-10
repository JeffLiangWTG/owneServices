using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Module;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingWhsOrderShoppingCart))]
	sealed class TrackingWhsOrderShoppingCartTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new TrackingWhsOrderShoppingCart(new InventoryFilterBusinessObject(), new TrackingInventorySummaryCollection(Factory));
		}
	}
}
