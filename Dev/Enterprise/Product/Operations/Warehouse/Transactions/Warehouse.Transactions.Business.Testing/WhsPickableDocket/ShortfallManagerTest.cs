using System;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class ShortfallManagerTest : WhsTestCaseWithFactory
	{
		#region TestConstructor

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ShortfallManager(null));
		}

		#endregion

		#region TestDeferMarkingLinesAsShortfallPropertiesChanged

		public void TestDeferMarkingLinesAsShortfallPropertiesChanged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var manager = new ShortfallManager(order);
			AssertEquals("Precondition:", false, manager.IsMarkingLinesAsShortfallPropertiesChangedSuspended);

			using (manager.DeferMarkingLinesAsShortfallPropertiesChanged())
			{
				AssertEquals(true, manager.IsMarkingLinesAsShortfallPropertiesChangedSuspended);

				using (manager.DeferMarkingLinesAsShortfallPropertiesChanged())
				{
					AssertEquals(true, manager.IsMarkingLinesAsShortfallPropertiesChangedSuspended);
				}

				AssertEquals(true, manager.IsMarkingLinesAsShortfallPropertiesChangedSuspended);
			}

			AssertEquals(false, manager.IsMarkingLinesAsShortfallPropertiesChangedSuspended);
		}

		public void TestDeferMarkingLinesAsShortfallPropertiesChanged_MarksExistingMatchingLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			var year = ZDateTime.Today.Year;
			var expiry1 = new ZDate(year, 1, 1);
			var expiry2 = new ZDate(year, 1, 2);
			var packing1 = new ZDate(year, 2, 1);
			var packing2 = new ZDate(year, 2, 2);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, expiry1, packing1, "PA1", "PA2", "PA3",
				"BEK-1", "");
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 1m, expiry1, packing1, "PA1", "PA2", "PA3",
				"BEK-1", "");
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, expiry2, packing1, "PA1", "PA2", "PA3",
				"BEK-1", "");
			var orderLine4 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, expiry1, packing2, "PA1", "PA2", "PA3",
				"BEK-1", "");
			var orderLine5 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, expiry1, packing1, "PAX", "PA2", "PA3",
				"BEK-1", "");
			var orderLine6 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, expiry1, packing1, "PA1", "PAX", "PA3",
				"BEK-1", "");
			var orderLine7 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, expiry1, packing1, "PA1", "PA2", "PAX",
				"BEK-1", "");
			var orderLine8 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, expiry1, packing1, "PA1", "PA2", "PA3",
				"BEK-2", "");
			var orderLine9 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, expiry1, packing1, "PA1", "PA2", "PA3",
				"BEK-1", "");
			var orderLine10 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, expiry1, packing1, "pa1", "pA2", "Pa3",
				"BeK-1", "");
			orderLine1.WE_SerialNumber = "SN1";
			orderLine2.WE_SerialNumber = "SN1";
			orderLine3.WE_SerialNumber = "SN1";
			orderLine4.WE_SerialNumber = "SN1";
			orderLine5.WE_SerialNumber = "SN1";
			orderLine6.WE_SerialNumber = "SN1";
			orderLine7.WE_SerialNumber = "SN1";
			orderLine8.WE_SerialNumber = "SN1";
			orderLine9.WE_SerialNumber = "SN2";
			orderLine10.WE_SerialNumber = "sn1";
			orderLine1.Shortfall.HasProductUnitsOrAttribsChanged = false;
			orderLine2.Shortfall.HasProductUnitsOrAttribsChanged = false;
			orderLine3.Shortfall.HasProductUnitsOrAttribsChanged = false;
			orderLine4.Shortfall.HasProductUnitsOrAttribsChanged = false;
			orderLine5.Shortfall.HasProductUnitsOrAttribsChanged = false;
			orderLine6.Shortfall.HasProductUnitsOrAttribsChanged = false;
			orderLine7.Shortfall.HasProductUnitsOrAttribsChanged = false;
			orderLine8.Shortfall.HasProductUnitsOrAttribsChanged = false;
			orderLine9.Shortfall.HasProductUnitsOrAttribsChanged = false;
			orderLine10.Shortfall.HasProductUnitsOrAttribsChanged = false;

			var manager = new ShortfallManager(order);
			using (manager.DeferMarkingLinesAsShortfallPropertiesChanged())
			{
				manager.AddLineToCheckShortfallPropertiesChangedLater(orderLine10);
				AssertEquals(false, orderLine1.Shortfall.HasProductUnitsOrAttribsChanged);
				AssertEquals(false, orderLine2.Shortfall.HasProductUnitsOrAttribsChanged);
				AssertEquals(false, orderLine3.Shortfall.HasProductUnitsOrAttribsChanged);
				AssertEquals(false, orderLine4.Shortfall.HasProductUnitsOrAttribsChanged);
				AssertEquals(false, orderLine5.Shortfall.HasProductUnitsOrAttribsChanged);
				AssertEquals(false, orderLine6.Shortfall.HasProductUnitsOrAttribsChanged);
				AssertEquals(false, orderLine7.Shortfall.HasProductUnitsOrAttribsChanged);
				AssertEquals(false, orderLine8.Shortfall.HasProductUnitsOrAttribsChanged);
				AssertEquals(false, orderLine9.Shortfall.HasProductUnitsOrAttribsChanged);
				AssertEquals(false, orderLine10.Shortfall.HasProductUnitsOrAttribsChanged);

				using (manager.DeferMarkingLinesAsShortfallPropertiesChanged())
				{
					AssertEquals(false, orderLine1.Shortfall.HasProductUnitsOrAttribsChanged);
					AssertEquals(false, orderLine2.Shortfall.HasProductUnitsOrAttribsChanged);
					AssertEquals(false, orderLine3.Shortfall.HasProductUnitsOrAttribsChanged);
					AssertEquals(false, orderLine4.Shortfall.HasProductUnitsOrAttribsChanged);
					AssertEquals(false, orderLine5.Shortfall.HasProductUnitsOrAttribsChanged);
					AssertEquals(false, orderLine6.Shortfall.HasProductUnitsOrAttribsChanged);
					AssertEquals(false, orderLine7.Shortfall.HasProductUnitsOrAttribsChanged);
					AssertEquals(false, orderLine8.Shortfall.HasProductUnitsOrAttribsChanged);
					AssertEquals(false, orderLine9.Shortfall.HasProductUnitsOrAttribsChanged);
					AssertEquals(false, orderLine10.Shortfall.HasProductUnitsOrAttribsChanged);
				}

				AssertEquals(false, orderLine1.Shortfall.HasProductUnitsOrAttribsChanged);
				AssertEquals(false, orderLine2.Shortfall.HasProductUnitsOrAttribsChanged);
				AssertEquals(false, orderLine3.Shortfall.HasProductUnitsOrAttribsChanged);
				AssertEquals(false, orderLine4.Shortfall.HasProductUnitsOrAttribsChanged);
				AssertEquals(false, orderLine5.Shortfall.HasProductUnitsOrAttribsChanged);
				AssertEquals(false, orderLine6.Shortfall.HasProductUnitsOrAttribsChanged);
				AssertEquals(false, orderLine7.Shortfall.HasProductUnitsOrAttribsChanged);
				AssertEquals(false, orderLine8.Shortfall.HasProductUnitsOrAttribsChanged);
				AssertEquals(false, orderLine9.Shortfall.HasProductUnitsOrAttribsChanged);
				AssertEquals(false, orderLine10.Shortfall.HasProductUnitsOrAttribsChanged);
			}

			AssertEquals(true, orderLine1.Shortfall.HasProductUnitsOrAttribsChanged);
			AssertEquals(false, orderLine2.Shortfall.HasProductUnitsOrAttribsChanged);
			AssertEquals(false, orderLine3.Shortfall.HasProductUnitsOrAttribsChanged);
			AssertEquals(false, orderLine4.Shortfall.HasProductUnitsOrAttribsChanged);
			AssertEquals(false, orderLine5.Shortfall.HasProductUnitsOrAttribsChanged);
			AssertEquals(false, orderLine6.Shortfall.HasProductUnitsOrAttribsChanged);
			AssertEquals(false, orderLine7.Shortfall.HasProductUnitsOrAttribsChanged);
			AssertEquals(false, orderLine8.Shortfall.HasProductUnitsOrAttribsChanged);
			AssertEquals(false, orderLine9.Shortfall.HasProductUnitsOrAttribsChanged);
			AssertEquals(true, orderLine10.Shortfall.HasProductUnitsOrAttribsChanged);
		}

		#endregion

		#region TestGetSameProductLines

		public void TestGetSameProductLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var manager = new ShortfallManager(order);

			var year = ZDateTime.Today.Year;
			var expiry1 = new ZDate(year, 1, 1);
			var expiry2 = new ZDate(year, 1, 2);
			var packing1 = new ZDate(year, 2, 1);
			var packing2 = new ZDate(year, 2, 2);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, expiry1, packing1, "PA1", "PA2", "PA3",
				"BEK-1", "");
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 1m, expiry1, packing1, "PA1", "PA2", "PA3",
				"BEK-1", "");
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, expiry2, packing1, "PA1", "PA2", "PA3",
				"BEK-1", "");
			var orderLine4 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, expiry1, packing2, "PA1", "PA2", "PA3",
				"BEK-1", "");
			var orderLine5 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, expiry1, packing1, "PAX", "PA2", "PA3",
				"BEK-1", "");
			var orderLine6 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, expiry1, packing1, "PA1", "PAX", "PA3",
				"BEK-1", "");
			var orderLine7 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, expiry1, packing1, "PA1", "PA2", "PAX",
				"BEK-1", "");
			var orderLine8 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, expiry1, packing1, "PA1", "PA2", "PA3",
				"BEK-2", "");
			var orderLine9 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, expiry1, packing1, "PA1", "PA2", "PA3",
				"BEK-1", "");
			var orderLine10 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, expiry1, packing1, "pa1", "pA2", "Pa3",
				"BeK-1", "");
			orderLine1.WE_SerialNumber = "SN1";
			orderLine2.WE_SerialNumber = "SN1";
			orderLine3.WE_SerialNumber = "SN1";
			orderLine4.WE_SerialNumber = "SN1";
			orderLine5.WE_SerialNumber = "SN1";
			orderLine6.WE_SerialNumber = "SN1";
			orderLine7.WE_SerialNumber = "SN1";
			orderLine8.WE_SerialNumber = "SN1";
			orderLine9.WE_SerialNumber = "SN2";
			orderLine10.WE_SerialNumber = "sn1";

			AssertContainsExactElementsInAnyOrder(new[] { orderLine1, orderLine10 },
				manager.GetSameProductLines(orderLine1));
		}

		public void TestGetSameProductLines_OrderedPalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var manager = new ShortfallManager(order);

			var year = ZDateTime.Today.Year;
			var expiry1 = new ZDate(year, 1, 1);
			var expiry2 = new ZDate(year, 1, 2);
			var packing1 = new ZDate(year, 2, 1);
			var packing2 = new ZDate(year, 2, 2);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, expiry1, packing1, "PA1", "PA2", "PA3",
				"BEK-1", "", "123");
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, expiry1, packing1, "PA1", "PA2", "PA3",
				"BEK-1", "", "456");
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part2, 1m, expiry1, packing1, "PA1", "PA2", "PA3",
				"BEK-1", "", "123");
			var orderLine4 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, expiry1, packing1, "pa1", "pA2", "Pa3",
				"BeK-1", "", "123");
			var orderLine5 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, expiry1, packing1, "PA1", "PA2", "PA3",
				"BEK-1", "", "");
			var orderLine6 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, expiry1, packing1, "PA1", "pa2", "pa3",
				"BEK-1", "", "");
			var orderLine7 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, expiry2, packing1, "PA1", "PA2", "PA3",
				"BEK-1", "", "");
			var orderLine8 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, expiry1, packing1, "PA1", "PA2", "PA3",
				"BEK-1", "BEK-2", "");

			AssertContainsExactElementsInAnyOrder(new[] { orderLine5, orderLine6, orderLine8 },
				manager.GetSameProductLines(orderLine5)); //empty pallet id 

			AssertContainsExactElementsInAnyOrder(new[] { orderLine1, orderLine4 },
				manager.GetSameProductLines(orderLine1)); // ordered pallet id
		}

		#endregion

		#region TestHeldInventory

		public void TestGetSameProductLines_HeldInventory()
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)) // Only possible with EnableHeldGoodsForOrders
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var manager = new ShortfallManager(order);

				var year = ZDateTime.Today.Year;
				var expiry1 = new ZDate(year, 1, 1);
				var expiry2 = new ZDate(year, 1, 2);
				var packing1 = new ZDate(year, 2, 1);
				var packing2 = new ZDate(year, 2, 2);
				var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m, expiry1, packing1, "PA1", "PA2", "PA3",
					"BEK-1", "");
				var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 2m, expiry1, packing1, "PA1", "PA2", "PA3",
					"BEK-1", "");
				var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 3m, expiry2, packing1, "PA1", "PA2", "PA3",
					"BEK-1", "");
				var orderLine4 = Helper.CreateWhsOrderLine(order, data.Part1, 4m, expiry1, packing2, "PA1", "PA2", "PA3",
					"BEK-1", "");
				var orderLine5 = Helper.CreateWhsOrderLine(order, data.Part1, 5m, expiry1, packing1, "PAX", "PA2", "PA3",
					"BEK-1", "");
				var orderLine6 = Helper.CreateWhsOrderLine(order, data.Part1, 6m, expiry1, packing1, "PA1", "PAX", "PA3",
					"BEK-1", "");
				var orderLine7 = Helper.CreateWhsOrderLine(order, data.Part1, 7m, expiry1, packing1, "PA1", "PA2", "PAX",
					"BEK-1", "");
				var orderLine8 = Helper.CreateWhsOrderLine(order, data.Part1, 8m, expiry1, packing1, "PA1", "PA2", "PA3",
					"BEK-2", "");
				var orderLine9 = Helper.CreateWhsOrderLine(order, data.Part1, 9m, expiry1, packing1, "PA1", "PA2", "PA3",
					"BEK-1", "");
				var orderLine10 = Helper.CreateWhsOrderLine(order, data.Part1, 10m, expiry1, packing1, "pa1", "pA2", "Pa3",
					"BeK-1", "");
				var orderLine11 = Helper.CreateWhsOrderLine(order, data.Part1, 11m, expiry1, packing1, "pa1", "pA2", "Pa3",
					"BeK-1", "");
				var orderLine12 = Helper.CreateWhsOrderLine(order, data.Part1, 12m, expiry1, packing1, "pa1", "pA2", "Pa3",
					"BeK-1", "");

				orderLine1.WE_SerialNumber = "SN1";
				orderLine1.WE_WHC_NKOriginalInventoryHeldCode = "HLD";
				orderLine2.WE_SerialNumber = "SN1";
				orderLine2.WE_WHC_NKOriginalInventoryHeldCode = "HLD";
				orderLine3.WE_SerialNumber = "SN1";
				orderLine3.WE_WHC_NKOriginalInventoryHeldCode = "HLD";
				orderLine4.WE_SerialNumber = "SN1";
				orderLine4.WE_WHC_NKOriginalInventoryHeldCode = "HLD";
				orderLine5.WE_SerialNumber = "SN1";
				orderLine5.WE_WHC_NKOriginalInventoryHeldCode = "HLD";
				orderLine6.WE_SerialNumber = "SN1";
				orderLine6.WE_WHC_NKOriginalInventoryHeldCode = "HLD";
				orderLine7.WE_SerialNumber = "SN1";
				orderLine7.WE_WHC_NKOriginalInventoryHeldCode = "HLD";
				orderLine8.WE_SerialNumber = "SN1";
				orderLine8.WE_WHC_NKOriginalInventoryHeldCode = "HLD";
				orderLine9.WE_SerialNumber = "SN2";
				orderLine9.WE_WHC_NKOriginalInventoryHeldCode = "HLD";
				orderLine10.WE_SerialNumber = "sn1";
				orderLine10.WE_WHC_NKOriginalInventoryHeldCode = "";
				orderLine11.WE_SerialNumber = "sn1";
				orderLine11.WE_WHC_NKOriginalInventoryHeldCode = "DAM";
				orderLine12.WE_SerialNumber = "sn1";
				orderLine12.WE_WHC_NKOriginalInventoryHeldCode = "HLD";

				AssertContainsExactElementsInAnyOrder(new[] { orderLine1, orderLine12 }, manager.GetSameProductLines(orderLine1));
			}
		}

		#endregion

	}
}
