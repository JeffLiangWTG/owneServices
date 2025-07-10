using System;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class BOMMenuItemTest : WhsGuiTestCaseWithFactory
	{
		#region TestExpandAndCollapseAllLines

		public void TestExpandAndCollapseAllLines()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			WhsWorkOrderLine bikeLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 2m);

			using (WorkOrderEntryForm form = new WorkOrderEntryForm(workOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Precondition", 1, workOrder.Lines.Count);

				MenuItem bomMenuItem = form.BomMenuItem;
				MethodInfo onClickMethod = bomMenuItem.GetType().GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic);

				onClickMethod.Invoke(bomMenuItem, new object[] { EventArgs.Empty });
				AssertEquals(14, workOrder.Lines.Count);

				onClickMethod.Invoke(bomMenuItem, new object[] { EventArgs.Empty });
				AssertEquals(1, workOrder.Lines.Count);
				AssertCollectionContains(bikeLine, workOrder.Lines);
			}
		}

		#endregion

		#region TestExpandAndCollapseSelectedLines

		public void TestExpandAndCollapseSelectedLines()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);

			var bikeLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 2m);
			var bikePolishLine = data.BOM.Lines.BikePolish(workOrder);

			var bikeWheelLine = data.BOM.Lines.BikeWheel(workOrder);
			var wheelRimLine = data.BOM.Lines.WheelRim(workOrder);
			var wheelTyreLine = data.BOM.Lines.WheelTyre(workOrder);
			var wheelPolishLine = data.BOM.Lines.WheelPolish(workOrder);

			var bikeEngineLine = data.BOM.Lines.BikeEngine(workOrder);
			var engineBlockLine = data.BOM.Lines.EngineBlock(workOrder);
			var enginePistonLine = data.BOM.Lines.EnginePiston(workOrder);
			var enginePolishLine = data.BOM.Lines.EnginePolish(workOrder);

			var pistonHeadLine = data.BOM.Lines.PistonHead(workOrder);
			var pistonCrankLine = data.BOM.Lines.PistonCrank(workOrder);
			var pistonRingLine = data.BOM.Lines.PistonRing(workOrder);

			using (var form = new WorkOrderEntryForm(workOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals(1, workOrder.Lines.Count);

				MenuItem bomMenuItem = form.BomMenuItemFromGrid;
				MethodInfo onClickMethod = bomMenuItem.GetType().GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic);

				// expand the top level (Bike) line
				form.LinesGrid.Select(0);
				onClickMethod.Invoke(bomMenuItem, new object[] { EventArgs.Empty });
				AssertEquals(14, workOrder.Lines.Count);

				// collapse the Wheel line
				int wheelRowIndex = form.LinesGrid.ListManager.List.IndexOf(data.BOM.Lines.BikeWheel(workOrder));
				form.LinesGrid.Select(wheelRowIndex);
				onClickMethod.Invoke(bomMenuItem, new object[] { EventArgs.Empty });
				AssertEquals(11, workOrder.Lines.Count);

				// bike
				AssertCollectionContains(bikeLine, workOrder.Lines);
				// wheel -- should be collapsed thus Rim + Tyre + WheelPolish not visible
				AssertCollectionContains(bikeWheelLine, workOrder.Lines);
				AssertCollectionContains(bikePolishLine, workOrder.Lines);
				// engine
				AssertCollectionContains(bikeEngineLine, workOrder.Lines);
				AssertCollectionContains(engineBlockLine, workOrder.Lines);
				AssertCollectionContains(enginePistonLine, workOrder.Lines);
				AssertCollectionContains(enginePolishLine, workOrder.Lines);
				// piston
				AssertCollectionContains(pistonHeadLine, workOrder.Lines);
				AssertCollectionContains(pistonCrankLine, workOrder.Lines);
				AssertCollectionContains(pistonRingLine, workOrder.Lines);
			}
		}

		#endregion

		#region TestUpdateCheckedAndEnabledStateForAllLines

		public void TestUpdateCheckedAndEnabledStateForAllLines()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);

			using (WorkOrderEntryForm form = new WorkOrderEntryForm(workOrder, new NotificationSubscriberGuiHelper()))
			{
				form.BomMenuItem.UpdateCheckedAndEnabledStateForAllLines();
				AssertEquals(false, form.BomMenuItem.Checked);
				AssertEquals("No lines on WorkOrder, BOM menu item should be disabled.", false, form.BomMenuItem.Enabled);

				// add a bike to the workOrder
				WhsWorkOrderLine bikeLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 2m);
				workOrder.BOM.ExpandAllLines();
				form.BomMenuItem.UpdateCheckedAndEnabledStateForAllLines();
				AssertEquals("All Lines are expanded, BOM menu item should be checked.", true, form.BomMenuItem.Checked);

				workOrder.BOM.CollapseAllLines();
				form.BomMenuItem.UpdateCheckedAndEnabledStateForAllLines();
				AssertEquals("All Lines are collapsed, BOM menu item should be unchecked.", false, form.BomMenuItem.Checked);

				data.BOM.Lines.BikeWheel(workOrder).BOM.ToggleExpansion(true);
				form.BomMenuItem.UpdateCheckedAndEnabledStateForAllLines();
				AssertEquals("Some Lines are collapsed, BOM menu item should be unchecked.", false, form.BomMenuItem.Checked);
			}
		}

		#endregion

		#region TestUpdateCheckedAndEnabledStateForSelectedLines

		public void TestUpdateCheckedAndEnabledStateForSelectedLines()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			WhsWorkOrderLine bikeLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.Bike, 2m);

			WhsWorkOrderLine bikeWheelLine = data.BOM.Lines.BikeWheel(workOrder);
			WhsWorkOrderLine wheelRimLine = data.BOM.Lines.WheelRim(workOrder);
			WhsWorkOrderLine wheelTyreLine = data.BOM.Lines.WheelTyre(workOrder);

			workOrder.BOM.ExpandAllLines();
			using (WorkOrderEntryForm form = new WorkOrderEntryForm(workOrder, new NotificationSubscriberGuiHelper()))
			{
				form.BomMenuItem.UpdateCheckedAndEnabledStateForSelectedLines(Array.Empty<WhsWorkOrderLine>());
				AssertEquals(false, form.BomMenuItem.Checked);
				AssertEquals("No lines are selected, BOM menu item should be disabled.", false, form.BomMenuItem.Enabled);
				AssertEquals("No lines are selected, BOM menu item should be unchecked.", false, form.BomMenuItem.Checked);

				form.BomMenuItem.UpdateCheckedAndEnabledStateForSelectedLines(new WhsWorkOrderLine[] { bikeWheelLine, wheelRimLine });
				AssertEquals("Multiple Lines are selected, BOM menu item should be disabled.", false, form.BomMenuItem.Enabled);
				AssertEquals("Multiple Lines are selected, BOM menu item should be unchecked.", false, form.BomMenuItem.Checked);

				form.BomMenuItem.UpdateCheckedAndEnabledStateForSelectedLines(new WhsWorkOrderLine[] { wheelRimLine });
				AssertEquals("Selected line is not a BOM product, BOM menu item should be disabled.", false, form.BomMenuItem.Enabled);
				AssertEquals("Selected line is not a BOM product, BOM menu item should be unchecked.", false, form.BomMenuItem.Checked);

				form.BomMenuItem.UpdateCheckedAndEnabledStateForSelectedLines(new WhsWorkOrderLine[] { bikeWheelLine });
				AssertEquals("Selected line is valid, BOM menu item should be enabled.", true, form.BomMenuItem.Enabled);
				AssertEquals("Selected line is already expanded, BOM menu item should be checked.", true, form.BomMenuItem.Checked);

				data.BOM.Lines.BikeWheel(workOrder).BOM.ToggleExpansion(false);
				form.BomMenuItem.UpdateCheckedAndEnabledStateForSelectedLines(new WhsWorkOrderLine[] { bikeWheelLine });
				AssertEquals("Selected line is valid, BOM menu item should be enabled.", true, form.BomMenuItem.Enabled);
				AssertEquals("Selected line is collapsed, BOM menu item should be unchecked.", false, form.BomMenuItem.Checked);
			}
		}

		#endregion
	}
}
