using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class PickOrdersUserControlTest : WhsGuiTestCaseWithFactory
	{
		#region TestControlVisibilityModifiedWhenIsCartonisedSet

		public void TestControlVisibilityModifiedWhenIsCartonisedSet()
		{
			var pick = Factory.New<WhsPick>();
			using (var form = new ZForm(pick))
			{
				var control = new PickOrdersUserControl();
				form.Controls.Add(control);

				form.Show();
				AssertEquals("Precondition", true, control.PickOrdersModuleButtonGrid.ShowNewButton);
				AssertEquals("Precondition", true, control.PickOrdersModuleButtonGrid.ShowEditButton);
				AssertEquals("Precondition", true, control.PickOrdersModuleButtonGrid.ShowAttachButton);
				AssertEquals("Precondition", true, control.PickOrdersModuleButtonGrid.ShowDetachButton);
				AssertEquals("Precondition", true, control.PickOrdersModuleButtonGrid.ShowAutoPickButton);
				AssertEquals("Precondition", true, control.PickOrdersModuleButtonGrid.ShowCancelPickButton);
				AssertEquals("Precondition", true, control.PickOrdersModuleButtonGrid.ShowReleaseButton);

				pick.WP_IsCartonised = true;
				AssertEquals("Should still be shown after WP_IsCartonised is set.", true, control.PickOrdersModuleButtonGrid.ShowDetachButton);
				AssertEquals("Should no longer be shown after WP_IsCartonised is set.", false, control.PickOrdersModuleButtonGrid.ShowNewButton);
				AssertEquals("Should no longer be shown after WP_IsCartonised is set.", false, control.PickOrdersModuleButtonGrid.ShowEditButton);
				AssertEquals("Should no longer be shown after WP_IsCartonised is set.", false, control.PickOrdersModuleButtonGrid.ShowAttachButton);
				AssertEquals("Should no longer be shown after WP_IsCartonised is set.", false, control.PickOrdersModuleButtonGrid.ShowAutoPickButton);
				AssertEquals("Should no longer be shown after WP_IsCartonised is set.", false, control.PickOrdersModuleButtonGrid.ShowCancelPickButton);
				AssertEquals("Should still be shown after WP_IsCartonised is set.", true, control.PickOrdersModuleButtonGrid.ShowReleaseButton);
			}
		}

		#endregion

		#region TestControlVisibilityModifiedWhenTaskPlanningStatusChanged

		public void TestControlVisibilityModifiedWhenTaskPlanningStatusChanged()
		{
			var pick = Factory.New<WhsPick>();
			using (var form = new ZForm(pick))
			{
				var control = new PickOrdersUserControl();
				form.Controls.Add(control);

				form.Show();
				AssertNullOrEmpty("Precondition", pick.WP_TaskPlanningStatus);
				AssertEquals("Precondition", true, control.PickOrdersModuleButtonGrid.ShowNewButton);
				AssertEquals("Precondition", true, control.PickOrdersModuleButtonGrid.ShowEditButton);
				AssertEquals("Precondition", true, control.PickOrdersModuleButtonGrid.ShowAttachButton);
				AssertEquals("Precondition", true, control.PickOrdersModuleButtonGrid.ShowDetachButton);
				AssertEquals("Precondition", true, control.PickOrdersModuleButtonGrid.ShowAutoPickButton);
				AssertEquals("Precondition", true, control.PickOrdersModuleButtonGrid.ShowCancelPickButton);
				AssertEquals("Precondition", true, control.PickOrdersModuleButtonGrid.ShowReleaseButton);

				pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				AssertEquals("Should no longer be shown after WP_TaskPlanningStatus set to Ready or Planned.", false, control.PickOrdersModuleButtonGrid.ShowDetachButton);
				AssertEquals("Should no longer be shown after WP_TaskPlanningStatus set to Ready or Planned.", false, control.PickOrdersModuleButtonGrid.ShowNewButton);
				AssertEquals("Should no longer be shown after WP_TaskPlanningStatus set to Ready or Planned.", false, control.PickOrdersModuleButtonGrid.ShowEditButton);
				AssertEquals("Should no longer be shown after WP_TaskPlanningStatus set to Ready or Planned.", false, control.PickOrdersModuleButtonGrid.ShowAttachButton);
				AssertEquals("Should no longer be shown after WP_TaskPlanningStatus set to Ready or Planned.", false, control.PickOrdersModuleButtonGrid.ShowAutoPickButton);
				AssertEquals("Should no longer be shown after WP_TaskPlanningStatus set to Ready or Planned.", false, control.PickOrdersModuleButtonGrid.ShowCancelPickButton);
				AssertEquals("Should shown after WP_TaskPlanningStatus set to Ready or Planned.", true, control.PickOrdersModuleButtonGrid.ShowReleaseButton);

				pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
				AssertEquals("Should shown after WP_TaskPlanningStatus set to Not Ready.", true, control.PickOrdersModuleButtonGrid.ShowDetachButton);
				AssertEquals("Should shown after WP_TaskPlanningStatus set to Not Ready.", true, control.PickOrdersModuleButtonGrid.ShowNewButton);
				AssertEquals("Should shown after WP_TaskPlanningStatus set to Not Ready.", true, control.PickOrdersModuleButtonGrid.ShowEditButton);
				AssertEquals("Should shown after WP_TaskPlanningStatus set to Not Ready.", true, control.PickOrdersModuleButtonGrid.ShowAttachButton);
				AssertEquals("Should shown after WP_TaskPlanningStatus set to Not Ready.", true, control.PickOrdersModuleButtonGrid.ShowAutoPickButton);
				AssertEquals("Should shown after WP_TaskPlanningStatus set to Not Ready.", true, control.PickOrdersModuleButtonGrid.ShowCancelPickButton);
				AssertEquals("Should shown after WP_TaskPlanningStatus set to Not Ready.", true, control.PickOrdersModuleButtonGrid.ShowReleaseButton);

				pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
				AssertEquals("Should no longer be shown after WP_TaskPlanningStatus set to Ready or Planned.", false, control.PickOrdersModuleButtonGrid.ShowDetachButton);
				AssertEquals("Should no longer be shown after WP_TaskPlanningStatus set to Ready or Planned.", false, control.PickOrdersModuleButtonGrid.ShowNewButton);
				AssertEquals("Should no longer be shown after WP_TaskPlanningStatus set to Ready or Planned.", false, control.PickOrdersModuleButtonGrid.ShowEditButton);
				AssertEquals("Should no longer be shown after WP_TaskPlanningStatus set to Ready or Planned.", false, control.PickOrdersModuleButtonGrid.ShowAttachButton);
				AssertEquals("Should no longer be shown after WP_TaskPlanningStatus set to Ready or Planned.", false, control.PickOrdersModuleButtonGrid.ShowAutoPickButton);
				AssertEquals("Should no longer be shown after WP_TaskPlanningStatus set to Ready or Planned.", false, control.PickOrdersModuleButtonGrid.ShowCancelPickButton);
				AssertEquals("Should shown after WP_TaskPlanningStatus set to Ready or Planned.", true, control.PickOrdersModuleButtonGrid.ShowReleaseButton);
			}
		}

		#endregion

		#region TestOrdersGridModuleId

		public void TestOrdersGridModuleId_WhsOrderPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			TestOrdersGridModuleIdCore(pick, ModuleIDs.WhsOrder);
		}

		public void TestOrdersGridModuleId_WhsWorkOrderPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.Orders.Add(workOrder);
			TestOrdersGridModuleIdCore(pick, ModuleIDs.WhsWorkOrder);
		}

		public void TestOrdersGridModuleId_WhsDynamicWorkOrderPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var workOrder = Helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.Orders.Add(workOrder);
			TestOrdersGridModuleIdCore(pick, ModuleIDs.WhsDynamicWorkOrder);
		}

		void TestOrdersGridModuleIdCore(WhsPick pick, ModuleIdentifier expectedModuleId)
		{
			using (var form = new ZForm(pick))
			{
				var control = new PickOrdersUserControl();
				form.Controls.Add(control);
				form.Show();
				AssertEquals("ModuleId is correct.", expectedModuleId, control.PickOrdersModuleButtonGrid.ModuleID);
			}
		}

		#endregion
	}
}
