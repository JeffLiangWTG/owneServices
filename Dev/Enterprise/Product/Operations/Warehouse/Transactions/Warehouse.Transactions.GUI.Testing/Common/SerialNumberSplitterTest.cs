using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NotificationType = CargoWise.EntityFramework.NotificationType;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class SerialNumberSplitterTest : WhsGuiTestCaseWithFactory
	{
		#region TestAddSplitSerialProductLinesMenuItemAndHookEvents

		public void TestAddSplitSerialProductLinesMenuItemAndHookEvents()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var collection = new DummyBusinessObjectCollection(Factory);

			var quantityOneLine = GetNewLine(collection, 1m, true);
			var noSerialLine = GetNewLine(collection, 2m, false);
			var splittableLine1 = GetNewLine(collection, 2m, true);
			var splittableLine2 = GetNewLine(collection, 2m, true);
			var splittableLine3 = GetNewLine(collection, 3m, true);

			using (var form = new ZForm())
			{
				var grid = new ZGrid();
				grid.DataSource = collection;
				form.Controls.Add(grid);
				form.Show();
				AssertEquals(5, grid.List.Count);

				var notifications = new TestNotificationBuffer();
				SerialNumberSplitter.AddSplitSerialProductLinesMenuItemAndHookEvents(grid, notifications);
				var splitSerialMenuItem = grid.ContextMenu.MenuItems.FindByText("S&plit Lines with Serial Numbers");
				AssertNotNull(splitSerialMenuItem);

				grid.ContextMenu.ShowPopupMenu();
				AssertEquals(false, splitSerialMenuItem.Enabled);

				grid.SelectSingleElement(quantityOneLine);
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals(true, splitSerialMenuItem.Enabled);

				splitSerialMenuItem.PerformClick();
				AssertEquals(5, grid.List.Count);
				AssertEquals(0, quantityOneLine.SplitWhenSerialNumberExistsHitCount);
				AssertEquals(NotificationType.Error, notifications.LastEvent.Type);
				AssertEquals("Line must have a quantity greater than 1 to split.", notifications.LastEvent.Message);
				notifications.Clear(); // clean up

				grid.SelectSingleElement(noSerialLine);
				splitSerialMenuItem.PerformClick();
				AssertEquals(5, grid.List.Count);
				AssertEquals(0, noSerialLine.SplitWhenSerialNumberExistsHitCount);
				AssertEquals(NotificationType.Error, notifications.LastEvent.Type);
				AssertEquals("Line has no Serial Number attributes, unable to split.", notifications.LastEvent.Message);
				notifications.Clear(); // clean up

				grid.SelectElements(quantityOneLine, noSerialLine);
				splitSerialMenuItem.PerformClick();
				AssertEquals(5, grid.List.Count);
				AssertEquals(0, quantityOneLine.SplitWhenSerialNumberExistsHitCount);
				AssertEquals(0, noSerialLine.SplitWhenSerialNumberExistsHitCount);
				AssertEquals(NotificationType.Error, notifications.LastEvent.Type);
				AssertEquals("No lines were split because they are not valid Serial Numbered Products.", notifications.LastEvent.Message);
				notifications.Clear(); // clean up

				grid.SelectElements(quantityOneLine, noSerialLine, splittableLine1);
				splitSerialMenuItem.PerformClick();
				AssertEquals(5, grid.List.Count);
				AssertEquals(0, quantityOneLine.SplitWhenSerialNumberExistsHitCount);
				AssertEquals(0, noSerialLine.SplitWhenSerialNumberExistsHitCount);
				AssertEquals(1, splittableLine1.SplitWhenSerialNumberExistsHitCount);
				AssertEquals(NotificationType.Warning, notifications.LastEvent.Type);
				AssertEquals("Some lines were not split because they are not valid Serial Numbered Products.", notifications.LastEvent.Message);
				notifications.Clear(); // clean up

				grid.SelectElements(splittableLine2, splittableLine3);
				splitSerialMenuItem.PerformClick();
				AssertEquals(5, grid.List.Count);
				AssertEquals(1, splittableLine2.SplitWhenSerialNumberExistsHitCount);
				AssertEquals(1, splittableLine3.SplitWhenSerialNumberExistsHitCount);
				AssertNull(notifications.LastEvent);
				notifications.Clear(); // clean up

				grid.UnSelectAll();
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals(false, splitSerialMenuItem.Enabled);
				grid.DataSource = null; // clean up
			}
		}

		DummyBusinessObjectForSerials GetNewLine(DummyBusinessObjectCollection collection, ZDecimal units, bool isSplittableLine)
		{
			var line = collection.AddNew();
			line.Units = units;
			line.IsSplittableProduct = isSplittableLine;

			return line;
		}

		#endregion

		#region TestSplitSerialProductLinesMenuItemShowsErrorOnFinalisedPick

		public void TestSplitSerialProductLinesMenuItemShowsErrorOnFinalisedPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "Reference1", ZDateTimeOffset.Today);
			for (int i = 0; i < 5; i++)
			{
				var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "");
				inventory.WI_SerialNumber = "SN:0" + i;
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			pick.PickOrdersWithAllocationMock(order);
			pick.AutoAllocateItemsWithMock();
			order.FinaliseDocket();
			AssertEquals("Order should be finalised", true, order.IsFinalised);

			pick.FinalisePick();
			AssertEquals("Pick should be finalised", true, pick.IsFinalised);

			using (var form = new ZForm())
			{
				orderLine.ReleaseLines[0].Quantity = 2m; // line must have a quantity greater than 1 to split.
				var grid = new ZGrid();
				grid.DataSource = orderLine.ReleaseLines;
				form.Controls.Add(grid);
				form.Show();

				var notifications = new TestNotificationBuffer();
				SerialNumberSplitter.AddSplitSerialProductLinesMenuItemAndHookEvents(grid, notifications);
				var splitSerialMenuItem = grid.ContextMenu.MenuItems.FindByText("S&plit Lines with Serial Numbers");
				AssertNotNull(splitSerialMenuItem);

				grid.SelectSingleElement(orderLine.ReleaseLines[0]);
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("Split menu should be enabled.", true, splitSerialMenuItem.Enabled);
				AssertEquals("There should be 5 rows.", 5, orderLine.ReleaseLines.Count);

				splitSerialMenuItem.PerformClick();
				AssertEquals("Still there should be 5 rows.", 5, orderLine.ReleaseLines.Count);
				AssertEquals(NotificationType.Error, notifications.LastEvent.Type);
				AssertEquals("Splitting is not allowed on Finalized Jobs.", notifications.LastEvent.Message);
			}
		}

		#endregion

		#region TestSplitSerialProductLinesMenuItemShowsErrorWhenSerialLinesHaveError

		public void TestSplitSerialProductLinesMenuItemShowsErrorWhenSerialLinesHaveError()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "Reference1", ZDateTimeOffset.Today);
			for (int i = 0; i < 5; i++)
			{
				var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "");
				inventory.WI_SerialNumber = "SN:0" + i;
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);

			using (var form = new ZForm())
			{
				orderLine.ReleaseLines[0].Quantity = -2m;
				var grid = new ZGrid();
				grid.DataSource = orderLine.ReleaseLines;
				form.Controls.Add(grid);
				form.Show();

				var notifications = new TestNotificationBuffer();
				SerialNumberSplitter.AddSplitSerialProductLinesMenuItemAndHookEvents(grid, notifications);
				var splitSerialMenuItem = grid.ContextMenu.MenuItems.FindByText("S&plit Lines with Serial Numbers");
				AssertNotNull(splitSerialMenuItem);

				grid.SelectSingleElement(orderLine.ReleaseLines[0]);
				grid.ContextMenu.ShowPopupMenu();
				AssertEquals("Split menu should be enabled.", true, splitSerialMenuItem.Enabled);
				AssertEquals("There should be 5 rows.", 5, orderLine.ReleaseLines.Count);

				splitSerialMenuItem.PerformClick();
				AssertEquals("Still there should be 5 rows.", 5, orderLine.ReleaseLines.Count);
				AssertEquals(NotificationType.Error, notifications.LastEvent.Type);
				AssertEquals("No lines were split as some lines have Errors.", notifications.LastEvent.Message);
			}
		}

		#endregion

		#region Implementation

		#region class DummyBusinessObjectCollection

		class DummyBusinessObjectCollection : BusinessObjectCollection<DummyBusinessObjectForSerials>
		{
			public DummyBusinessObjectCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}

		#endregion

		#region class DummyBusinessObjectForSerials

		class DummyBusinessObjectForSerials : DummyBusinessObject, ISerialSplittableLine
		{
			public DummyBusinessObjectForSerials(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool IsFinalised { get; set; }
			public ZDecimal Units { get; set; }
			public bool IsSplittableProduct { get; set; }

			public void SplitWhenSerialNumberExists()
			{
				SplitWhenSerialNumberExistsHitCount++;
			}

			public int SplitWhenSerialNumberExistsHitCount { get; private set; }
		}

		#endregion

		#endregion
	}
}
