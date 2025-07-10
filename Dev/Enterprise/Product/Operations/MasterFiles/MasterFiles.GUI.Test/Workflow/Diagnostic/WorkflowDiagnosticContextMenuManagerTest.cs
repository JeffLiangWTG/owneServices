using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class WorkflowDiagnosticContextMenuManagerTest : TestCaseWithFactory
	{
		public void TestShow_OnJob()
		{
			var dummyBO = Factory.New<DummyWithWorkflow>();
			dummyBO.Z0_VarCharMax = "J00001000";

			var trigger = dummyBO.WorkflowItems.AddNew();
			trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.FreightLoadedCode;

			AssertEquals("GIVEN job-record, WHEN CanShow is executed, should show", true, WorkflowDiagnosticContextMenuManager.CanShow(trigger));
		}

		public void TestShow_OnTemplate()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = "AAA";

			var templateMilestone = template.WorkflowItems.Milestones.AddNew();
			templateMilestone.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateMilestone.TemplateConditions.TemplateCondition2Value = "\"<Z0_Code>\"==\"DNW\"";

			AssertEquals("GIVEN template-record, WHEN CanShow is executed, should not show", false, WorkflowDiagnosticContextMenuManager.CanShow(templateMilestone));
		}

		public void TestMenuItemsGetAdded()
		{
			var dummyBO = Factory.New<DummyWithWorkflow>();

			var trigger = dummyBO.WorkflowItems.AddNew();
			trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.FreightLoadedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			action.PQ_EmailAddr = "test@test.com";

			using (var actionsGrid = new ZGrid())
			{
				var contextMenuItemsCount = actionsGrid.ContextMenu.MenuItems.Count;
				WorkflowDiagnosticContextMenuManager.AttachToTriggerGrid(actionsGrid);
				AssertEquals("Should be 2 new menu items added.", 2, actionsGrid.ContextMenu.MenuItems.Count - contextMenuItemsCount);

				AssertNotNull(actionsGrid.ContextMenu.MenuItems.FindByText("Workflow Diagnostic"));
				AssertNotNull(actionsGrid.ContextMenu.MenuItems.FindByText("Workflow Template Matches"));
			}
		}
	}
}
