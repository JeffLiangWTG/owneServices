using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsBOMInventoryPivot))]
	class WhsBOMInventoryPivotTest : WhsBusinessObjectTestCase
	{
		#region TestComponentLine

		public void TestComponentLine()
		{
			var pivot = Factory.New<WhsBOMInventoryPivot>();
			AssertNull(pivot.ComponentLine);

			var orderLine = Factory.New<WhsWorkOrderLine>();
			pivot.WIP_WE_ComponentLine = orderLine.PK;

			AssertEquals(orderLine, pivot.ComponentLine);
		}

		#endregion

		#region TestInventoryLine

		public void TestInventoryLine()
		{
			var pivot = Factory.New<WhsBOMInventoryPivot>();
			AssertNull(pivot.InventoryLine);

			var receiveLine = Factory.New<WhsReceiveLine>();
			pivot.WIP_WE_InventoryLine = receiveLine.PK;

			AssertEquals(receiveLine, pivot.InventoryLine);
		}

		#endregion
	}
}
