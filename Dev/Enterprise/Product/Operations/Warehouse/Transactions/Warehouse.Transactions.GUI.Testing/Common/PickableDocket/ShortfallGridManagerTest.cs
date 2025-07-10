using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class ShortfallGridManagerTest : WhsGuiTestCaseWithFactory
	{
		#region TestOnOrderLineChanged_DoesNotUseDeletedRow

		public void TestOnOrderLineChanged_DoesNotUseDeletedRow()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 15m);

			var userControl = new DummyPickableDocketLinesControl();
			userControl.LineUpdateHitCounts[orderLine1.PK] = 0; // Setup key
			userControl.LineUpdateHitCounts[orderLine2.PK] = 0; // Setup key

			using (var form = GetNewForm(order, userControl))
			{
				var grid = userControl.LinesGrid;
				form.Show();

				grid.SelectSingleElement(orderLine1);
				orderLine1.WE_TransactionQuantity = 12m;
				orderLine1.Delete();

				grid.SelectSingleElement(orderLine2);
				AssertEquals(0, userControl.LineUpdateHitCounts[orderLine1.PK]);
			}
		}

		#endregion

		#region TestShortfall

		public void TestShortfall()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var poke1 = orderLine1.WE_ShortfallQuantityCached;
			var poke2 = orderLine2.WE_ShortfallQuantityCached;

			var userControl = new DummyPickableDocketLinesControl();
			userControl.LineUpdateHitCounts[orderLine1.PK] = 0; // Setup key
			userControl.LineUpdateHitCounts[orderLine2.PK] = 0; // Setup key

			using (var form = GetNewForm(order, userControl))
			{
				var grid = userControl.LinesGrid;
				form.Controls.Add(userControl);
				form.Show();

				grid.SelectSingleElement(orderLine1);
				grid.Focus();
				AssertEquals("Precondition - Should not fire Shortfall Calc.", 0, userControl.LineUpdateHitCounts[orderLine1.PK]);

				grid.SelectSingleElement(orderLine2);
				AssertEquals(0, userControl.LineUpdateHitCounts[orderLine1.PK]);

				orderLine2.WE_TransactionQuantity = 12m;
				AssertEquals(0, userControl.LineUpdateHitCounts[orderLine2.PK]);

				grid.SelectSingleElement(orderLine1);
				AssertEquals(1, userControl.LineUpdateHitCounts[orderLine2.PK]);

				orderLine1.WE_ExpiryDate = new ZDate(2012, 1, 1);
				grid.SelectSingleElement(orderLine2);
				AssertEquals(1, userControl.LineUpdateHitCounts[orderLine1.PK]);

				orderLine2.WE_PackingDate = new ZDate(2012, 1, 1);
				grid.SelectSingleElement(orderLine1);
				AssertEquals(3, userControl.LineUpdateHitCounts[orderLine2.PK]);

				orderLine1.WE_PartAttrib1 = "PA1";
				grid.SelectSingleElement(orderLine2);
				AssertEquals(2, userControl.LineUpdateHitCounts[orderLine1.PK]);

				orderLine2.WE_PartAttrib2 = "PA2";
				grid.SelectSingleElement(orderLine1);
				AssertEquals(4, userControl.LineUpdateHitCounts[orderLine2.PK]);

				orderLine1.WE_PartAttrib3 = "PA3";
				grid.SelectSingleElement(orderLine2);
				AssertEquals(3, userControl.LineUpdateHitCounts[orderLine1.PK]);

				orderLine2.WE_OP = ZGuid.NewZGuid();
				grid.SelectSingleElement(orderLine1);
				AssertEquals(5, userControl.LineUpdateHitCounts[orderLine2.PK]);

				orderLine1.WE_LineNo = 10;
				grid.SelectSingleElement(orderLine2);
				AssertEquals(3, userControl.LineUpdateHitCounts[orderLine1.PK]);
			}
		}

		#endregion

		#region TestShortfallIsUpdatedOnFormOpenAndWhenChangingGridRow

		public void TestShortfallIsUpdatedOnFormOpenAndWhenChangingGridRow()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateMultiWarehouseClientProductInventory(TestDataForInventory.StockType.WithoutBondEntryKeys);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", data.Notify);
			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var line2 = Helper.CreateWhsOrderLine(order, data.Part2, 150m); // this should create a shortfall of 50 of Part2

			var userControl = new DummyPickableDocketLinesControl();
			using (var form = GetNewForm(order, userControl))
			{
				form.Show();

				// test that the shortfall values + validation is run when an existing Order is opened.

				AssertHasWarning("Warnings should exist when the form is first opened.", line2.WE_ShortfallQuantityCachedInfo, "Shortfall: Only 100 unit(s) currently available");
				AssertEquals("Shortfall Qty should be updated when the form is first opened.", 50m, line2.WE_ShortfallQuantityCached);

				// test that changing between rows updates the previous row's shortfall qty + validation

				var pokeCache1 = line1.WE_ShortfallQuantityCached;
				line1.WE_TransactionQuantity = 175;
				AssertEquals("Precondition", 0m, line1.WE_ShortfallQuantityCached);
				AssertNoWarnings("Precondition", line1.WE_ShortfallQuantityCachedInfo);

				userControl.LinesGrid.ListManager.Position = 0;
				userControl.LinesGrid.ListManager.Position = 1; // this simulates the user moving from Row0 to Row1
				AssertEquals("Shortfall Qty should be updated when changing row.", 75m, line1.WE_ShortfallQuantityCached);
				AssertHasWarning("Shortfall Warning should be updated when changing row.", line1.WE_ShortfallQuantityCachedInfo, "Shortfall: Only 100 unit(s) currently available");

				// test that leaving the grid updates the currently selected row's shortfall qty + validation (as no row change will occur)

				userControl.LinesGrid.Focus();
				var pokeCache2 = line2.WE_ShortfallQuantityCached;
				line1.WE_TransactionQuantity = 1;
				line2.WE_TransactionQuantity = 1;
				AssertHasWarning("Precondition", line2.WE_ShortfallQuantityCachedInfo, "Shortfall: Only 100 unit(s) currently available");

				userControl.FocusOnRandomButton(); // simulate user clicking out of the grid.
				AssertEquals("Shortfall Qty should be updated when leaving the grid.", 0m, line1.WE_ShortfallQuantityCached);
				AssertNoWarnings("Shortfall Warning should be updated when leaving the grid.", line1.WE_ShortfallQuantityCachedInfo);
				AssertEquals("Shortfall Qty should be updated when leaving the grid.", 0m, line2.WE_ShortfallQuantityCached);
				AssertNoWarnings("Shortfall Warning should be updated when leaving the grid.", line2.WE_ShortfallQuantityCachedInfo);
			}
		}

		#endregion

		#region TestShortfallCalculationIsSuspendedAndResumedWhenChangingGridRow

		public void TestShortfallCalculationIsSuspendedAndResumedWhenChangingGridRow()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateMultiWarehouseClientProductInventory(TestDataForInventory.StockType.WithoutBondEntryKeys);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, data.Notify);
			var orderline1 = Helper.CreateWhsOrderLine(order, data.Part1, 110); //shortfall of 10
			var orderline2 = Helper.CreateWhsOrderLine(order, data.Part2, 150); //shortfall of 50

			var userControl = new DummyPickableDocketLinesControl();
			using (var form = GetNewForm(order, userControl))
			{
				form.Show();
				var grid = userControl.LinesGrid;
				grid.ListManager.Position = 0;

				grid.Focus();
				AssertEquals("Entering grid should suspend shortfall calculation on current row.", 0m, orderline1.WE_ShortfallQuantityCached);

				grid.ListManager.Position = 1; // move to next row in grid
				AssertEquals("Moving row should resume shortfall calculation on previous row.", 10m, orderline1.WE_ShortfallQuantityCached);
				AssertEquals("Updating first row will update all rows need to update.", 50m, orderline2.WE_ShortfallQuantityCached);

				orderline2.WE_TransactionQuantity = 160;
				userControl.FocusOnRandomButton(); // leave grid
				AssertEquals("Leaving grid should resume shortfall calculation on previous row.", 60m, orderline2.WE_ShortfallQuantityCached);
			}
		}

		#endregion

		#region Implementation

		ZForm GetNewForm(WhsOrder order, DummyPickableDocketLinesControl userControl)
		{
			var form = new ZForm(order);
			form.Controls.Add(userControl);
			return form;
		}

		class DummyPickableDocketLinesControl : PickableDocketLinesGridUserControl
		{
			public DummyPickableDocketLinesControl()
			{
				Controls.Add(TestButton);
			}

			readonly ZButton TestButton = new ZButton();

			public void FocusOnRandomButton()
			{
				TestButton.Focus();
			}

			protected override IShortfallGridManagerUpdateStrategy ShortfallGridManagerUpdateStrategy
			{
				get { return UpdateStrategy; }
			}

			readonly DummyShortfallUpdateStrategy UpdateStrategy = new DummyShortfallUpdateStrategy();

			public Dictionary<ZGuid, int> LineUpdateHitCounts
			{
				get { return UpdateStrategy.LineUpdateHitCounts; }
			}
		}

		class DummyShortfallUpdateStrategy : IShortfallGridManagerUpdateStrategy
		{
			public void Update(WhsPickableDocketLine line)
			{
				if (LineUpdateHitCounts.ContainsKey(line.PK))
				{
					LineUpdateHitCounts[line.PK]++;
				}
				else
				{
					LineUpdateHitCounts[line.PK] = 1;
				}

				line.Validation.ValidateWE_ShortfallQuantityCached();
			}

			public Dictionary<ZGuid, int> LineUpdateHitCounts = new Dictionary<ZGuid, int>();
		}

		#endregion
	}
}
