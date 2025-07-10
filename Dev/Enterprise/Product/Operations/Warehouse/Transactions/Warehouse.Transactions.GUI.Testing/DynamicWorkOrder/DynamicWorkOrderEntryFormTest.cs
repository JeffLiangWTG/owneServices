using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	[TestedType(typeof(DynamicWorkOrderEntryForm))]
	public class DynamicWorkOrderEntryFormTest : ZFormBasherTest
	{
		#region TestHandleSaveException_ShowsMessageWhenTriggersFail

		public void TestHandleSaveException_ShowsMessageWhenTriggersFail()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;
			helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			Factory.Save();

			var order = helper.CreateWhsDynamicWorkOrderWithLine(data.Org1, data.Whs1, data.Part1, 7m);
			order.Lines[0].CustomsData.WB_IsMainInwardsProcessedItem = true;
			GUITestHelper.AssertHandleSaveException_ShowsMessageWhenTriggersFail(() => new DynamicWorkOrderEntryForm(order));
		}

		#endregion

		#region TestSubTypeVisibility

		public void TestSubTypeVisibility_GroupBoxes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area = helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var dynamicWorkOrder = helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);
			dynamicWorkOrder.WD_DocketSubType = DynamicWorkOrderType.Codes.Assemble;
			Factory.Save();

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			using (var form = new DynamicWorkOrderEntryFormForTest(dynamicWorkOrder))
			{
				form.Show();

				var detailTabControl = GUITestHelper.FindControl<ZTemplateTabControl>(form.DynamicWorkOrderEntryControl.Controls, "DetailTabControl");
				var parentLinesGroupBox = GUITestHelper.FindControl<ZGroupBox>(detailTabControl.Controls, "ParentLinesGroupBox");
				var componentLinesGroupBox = GUITestHelper.FindControl<ZGroupBox>(detailTabControl.Controls, "ComponentLinesGroupBox");

				AssertEquals("ParentLines always visible.", true, parentLinesGroupBox.Visible);
				AssertEquals("Assembly Work Orders do not hide control.", true, componentLinesGroupBox.Visible);

				dynamicWorkOrder.WD_DocketSubType = DynamicWorkOrderType.Codes.Disassemble;
				AssertEquals("ParentLines always visible.", true, parentLinesGroupBox.Visible);
				AssertEquals("Disassembly Work Orders hide control.", false, componentLinesGroupBox.Visible);

				dynamicWorkOrder.WD_DocketSubType = DynamicWorkOrderType.Codes.Assemble;
				AssertEquals("ParentLines always visible.", true, parentLinesGroupBox.Visible);
				AssertEquals("Assembly Work Orders do not hide control.", true, componentLinesGroupBox.Visible);

				// Switch to Lines Tab
				var linesTabPage = GUITestHelper.FindControl<ZTabPage>(detailTabControl.Controls, "LinesTabPage");
				detailTabControl.SelectedTab = linesTabPage;

				var linesTabParentLinesGroupBox = GUITestHelper.FindControl<ZGroupBox>(linesTabPage.Controls, "ParentLinesGroupBox");
				var linesTabComponentLinesGroupBox = GUITestHelper.FindControl<ZGroupBox>(linesTabPage.Controls, "ComponentLinesGroupBox");
				AssertEquals("ParentLines always visible.", true, linesTabParentLinesGroupBox.Visible);
				AssertEquals("Assembly Work Orders do not hide control.", true, linesTabComponentLinesGroupBox.Visible);

				dynamicWorkOrder.WD_DocketSubType = DynamicWorkOrderType.Codes.Disassemble;
				AssertEquals("ParentLines always visible.", true, linesTabParentLinesGroupBox.Visible);
				AssertEquals("Disassembly Work Orders hide control.", false, linesTabComponentLinesGroupBox.Visible);

				dynamicWorkOrder.WD_DocketSubType = DynamicWorkOrderType.Codes.Assemble;
				AssertEquals("ParentLines always visible.", true, linesTabParentLinesGroupBox.Visible);
				AssertEquals("Assembly Work Orders do not hide control.", true, linesTabComponentLinesGroupBox.Visible);
			}
		}

		public void TestSubTypeVisibility_QuantityMetColumn()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area = helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var dynamicWorkOrder = helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);
			dynamicWorkOrder.WD_DocketSubType = DynamicWorkOrderType.Codes.Assemble;
			Factory.Save();

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			using (var form = new DynamicWorkOrderEntryFormForTest(dynamicWorkOrder))
			{
				form.Show();

				var detailTabControl = GUITestHelper.FindControl<ZTemplateTabControl>(form.DynamicWorkOrderEntryControl.Controls, "DetailTabControl");
				var parentLinesGridControl = GUITestHelper.FindControl<DynamicWorkOrderParentLinesGridUserControl>(detailTabControl.Controls, "ParentLinesGridControl");

				AssertEquals("ParentLines always visible.", true, parentLinesGridControl.Visible);
				AssertEquals("Quantity Met only visible for disassembly.", true, parentLinesGridControl.LinesGrid.GetColumnStyle("SumOfUnitsMet").IsUnavailable);

				dynamicWorkOrder.WD_DocketSubType = DynamicWorkOrderType.Codes.Disassemble;
				AssertEquals("ParentLines always visible.", true, parentLinesGridControl.Visible);
				AssertEquals("Quantity Met only visible for disassembly.", false, parentLinesGridControl.LinesGrid.GetColumnStyle("SumOfUnitsMet").IsUnavailable);

				dynamicWorkOrder.WD_DocketSubType = DynamicWorkOrderType.Codes.Assemble;
				AssertEquals("ParentLines always visible.", true, parentLinesGridControl.Visible);
				AssertEquals("Quantity Met only visible for disassembly.", true, parentLinesGridControl.LinesGrid.GetColumnStyle("SumOfUnitsMet").IsUnavailable);

				// Switch to Lines Tab
				var linesTabPage = GUITestHelper.FindControl<ZTabPage>(detailTabControl.Controls, "LinesTabPage");
				detailTabControl.SelectedTab = linesTabPage;

				var linesTabParentLinesGroupBox = GUITestHelper.FindControl<ZGroupBox>(linesTabPage.Controls, "ParentLinesGroupBox");
				var linesTabComponentLinesGroupBox = GUITestHelper.FindControl<ZGroupBox>(linesTabPage.Controls, "ComponentLinesGroupBox");
				AssertEquals("ParentLines always visible.", true, linesTabParentLinesGroupBox.Visible);
				AssertEquals("Assembly Work Orders do not hide control.", true, linesTabComponentLinesGroupBox.Visible);

				dynamicWorkOrder.WD_DocketSubType = DynamicWorkOrderType.Codes.Disassemble;
				AssertEquals("ParentLines always visible.", true, linesTabParentLinesGroupBox.Visible);
				AssertEquals("Disassembly Work Orders hide control.", false, linesTabComponentLinesGroupBox.Visible);

				dynamicWorkOrder.WD_DocketSubType = DynamicWorkOrderType.Codes.Assemble;
				AssertEquals("ParentLines always visible.", true, linesTabParentLinesGroupBox.Visible);
				AssertEquals("Assembly Work Orders do not hide control.", true, linesTabComponentLinesGroupBox.Visible);
			}
		}
		#endregion

		#region TestPickButton

		public void TestPickButton()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area = helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var setup = DynamicWorkOrderTestSetup.CreateDynamicWorkOrderForTest(helper, data.Org1, data.Whs1);
			Factory.Save();

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			using (var form = new DynamicWorkOrderEntryFormForTest(setup.WorkOrder))
			{
				form.Show();

				AssertEquals("Precondition.", false, setup.WorkOrder.IsAttachedToPickButNotFinalised);
				form.PickButton.PerformClick();
				AssertEquals("Should have created pick.", true, setup.WorkOrder.IsAttachedToPickButNotFinalised);
			}
		}

		#endregion

		#region TestFinaliseButton

		public void TestFinaliseButton()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area = helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;
			Factory.Save();

			var setup = DynamicWorkOrderTestSetup.CreateDynamicWorkOrderForTest(helper, data.Org1, data.Whs1);
			setup.ReceiveStockForWorkOrder();
			Factory.Save();

			helper.CreatePickNew(setup.WorkOrder);

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			using (var form = new DynamicWorkOrderEntryFormForTest(setup.WorkOrder))
			{
				form.Show();

				AssertEquals("Precondition.", false, setup.WorkOrder.IsFinalised);

				form.FinalizeButton.PerformClick();
				AssertEquals("Should have finalized the work order.", true, setup.WorkOrder.IsFinalised);
				AssertEquals("Should have finalized the pick.", true, setup.WorkOrder.Pick.IsFinalised);

				var receive = setup.WorkOrder.Receive;
				AssertNotNull("Should have created the receive.", receive);

				Factory.Save();

				var receiveController = ZControllerFactory.Create(ControllerIDs.WhsReceive);
				var lastForm = receiveController.GetOpenedForm(receive);
				AssertNotNull(lastForm);
				lastForm.Dispose();
			}
		}

		#endregion

		#region TestFinaliseButtonIsDisabledForNonPickedWorkOrder

		public void TestFinaliseButtonIsDisabledForNonPickedWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			using (var form = (DynamicWorkOrderEntryFormForTest)GetFormToBashCore())
			{
				var docket = (WhsDynamicWorkOrder)form.BusinessEntity;
				docket.WD_OH_Client = data.Org1.PK;
				docket.WD_WW_Whs = data.Whs1.PK;
				docket.WD_RequiredDate = ZDateTimeOffset.Now;
				docket.ConsigneeAddressPK = data.Org1.MainAddress.PK;
				Factory.Save();

				form.Show();
				AssertEquals("Docket has no Pick, Finalise Button should be enabled.", false, form.FinalizeButton.Enabled);

				var pick = Factory.New<WhsPick>();
				docket.WD_WP = pick.PK;
				AssertEquals("Precondition", true, docket.IsAttachedToPickButNotFinalised);
				AssertEquals(true, form.FinalizeButton.Enabled);

				docket.WD_FinalisedDate = ZDateTimeOffset.Now;
				AssertEquals("Docket is already finalised, Finalise Button should be disabled.", false, form.FinalizeButton.Enabled);
			}
		}

		#endregion

		#region TestWorkflowTabpage

		public void TestWorkflowTabpage()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var tabControl = form.FindAll<ZTemplateTabControl>().First();
				AssertNotEquals("WorkflowTabPage Visible", -1, tabControl.TabPages.IndexOf(tabControl.GetTabPage("WorkflowTabPage")));
			}
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var result = new DynamicWorkOrderEntryFormForTest(Factory.New<WhsDynamicWorkOrder>());
			result.ControllerID = ControllerIDs.WhsDynamicWorkOrder;
			return result;
		}

		#region class DynamicWorkOrderEntryFormForTest

		class DynamicWorkOrderEntryFormForTest : DynamicWorkOrderEntryForm
		{
			public DynamicWorkOrderEntryFormForTest(WhsDynamicWorkOrder docket)
				: base(docket)
			{
			}

			public new ZButton PickButton => base.PickButton;

			public new ZButton FinalizeButton => base.FinalizeButton;

			public new DynamicWorkOrderEntryUserControl DynamicWorkOrderEntryControl => base.DynamicWorkOrderEntryControl;
		}

		#endregion

		#endregion
	}
}
