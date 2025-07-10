using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Workflow.GUI.Test
{
	public class ValidateCommunicationModesMenuItemProviderTest : TestCaseWithFactory
	{
		public void TestAddItemToGrid()
		{
			var provider = new ValidateCommunicationModesMenuItemProvider();
			var dummyBO = Factory.New<DummyWithWorkflow>();

			var trigger = dummyBO.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

			using (var form = new ZForm())
			using (var actionsGrid = new ZGrid())
			{
				var column = new ZTextBoxColumnStyleInfo();
				column.ColumnName = "P9_Sequence";
				actionsGrid.ColumnStyles.Add(column);
				form.Controls.Add(actionsGrid);
				actionsGrid.SetDataBinding(dummyBO.WorkflowItems.Triggers, "");
				provider.AddItemToGrid(actionsGrid);

				var menuItem = actionsGrid.ContextMenu.MenuItems.FindByText("Validate Communication Modes");
				AssertNotNull(menuItem);
				AssertNoExceptionThrown(menuItem.PerformClick);
				AssertHasWarningContaining(action.PQ_Calc_TriggerPartyInfo, "has no matching Communication Modes");
			}
		}
	}
}
