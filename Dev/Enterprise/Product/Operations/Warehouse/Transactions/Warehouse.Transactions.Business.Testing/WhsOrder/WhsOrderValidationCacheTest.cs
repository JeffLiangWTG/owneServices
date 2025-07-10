using System;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsOrderValidationCacheTest : WhsTestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new WhsOrderValidationCache(null));
		}

		#region TestLinesContainHeldInventory

		public void TestLinesContainHeldInventory_NoHeldLines()
		{
			var order = SetupOrderLines(false, false);

			var cache = new WhsOrderValidationCache(order.Lines);
			AssertEquals(cache.LinesContainHeldInventory(), false);
		}

		public void TestLinesContainHeldInventory_SingleHeldLine()
		{
			var order = SetupOrderLines(false, true);

			var cache = new WhsOrderValidationCache(order.Lines);
			AssertEquals(cache.LinesContainHeldInventory(), true);
		}

		#endregion

		#region TestLinesContainAvailableInventory

		public void TestLinesContainAvailableInventory_NoAvailableLines()
		{
			var order = SetupOrderLines(true, true);

			var cache = new WhsOrderValidationCache(order.Lines);
			AssertEquals(cache.LinesContainAvailableInventory(), false);
		}

		public void TestLinesContainAvailableInventory_SingleAvailableLine()
		{
			var order = SetupOrderLines(false, true);

			var cache = new WhsOrderValidationCache(order.Lines);
			AssertEquals(cache.LinesContainAvailableInventory(), true);
		}

		#endregion

		public WhsOrder SetupOrderLines(bool firstLineContainsHeldInventory, bool secondLineContainsHeldInventory)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			if (firstLineContainsHeldInventory)
			{
				orderLine1.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Held;
			}
			if (secondLineContainsHeldInventory)
			{
				orderLine2.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Held;
			}

			return order;
		}
	}
}
