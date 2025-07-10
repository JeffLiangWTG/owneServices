using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI
{
	[TestedType(typeof(ExistingWorkingTaskForm))]
	sealed class ExistingWorkingTaskFormTest : ZFormBasherTest
	{
		public void TestOpenJobButton()
		{
			OrgOpportunity opportunityWithWorkingTask = Factory.NewWithValidTestData<OrgOpportunity>();
			ProcessTask workingTask = opportunityWithWorkingTask.WorkflowItems.AddNew();
			workingTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			workingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			OrgOpportunity opportunityWithSuspendedTask = Factory.NewWithValidTestData<OrgOpportunity>();
			ProcessTask suspendedTask = opportunityWithSuspendedTask.WorkflowItems.AddNew();
			suspendedTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			suspendedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			ProcessTask standAloneTask = Factory.NewWithValidTestData<ProcessTask>();
			standAloneTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			standAloneTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			Factory.Save();

			using (ExistingWorkingTaskForm form = new ExistingWorkingTaskForm(workingTask))
			{
				form.Show();
				AssertEquals(3, form.TasksGrid.ListManager.List.Count);

				int standAloneTaskRow = form.TasksGrid.ListManager.List.IndexOf(standAloneTask);
				int suspendedTaskRow = form.TasksGrid.ListManager.List.IndexOf(suspendedTask);

				form.TasksGrid.ListManager.Position = standAloneTaskRow;
				form.OpenJobButton.PerformClick();
				AssertEquals("This is a stand-alone or template task.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.TasksGrid.ListManager.Position = suspendedTaskRow;
				form.OpenJobButton.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(typeof(OpportunityForm), form.LastControllerForTest.LastShownForm.GetType());
				form.LastControllerForTest.LastShownForm.Dispose();
			}
		}

		public void TestContinueExistingWorkButton()
		{
			using (ExistingWorkingTaskForm form = new ExistingWorkingTaskForm(GetTaskForTests()))
			{
				form.Show();
				AssertEquals(true, form.Visible);
				form.ContinueExistingWorkButton.PerformClick();
				AssertEquals(false, form.Visible);
				AssertEquals(DialogResult.No, form.DialogResult);
			}
		}

		public void TestSuspendExistingTaskButton()
		{
			using (ExistingWorkingTaskForm form = new ExistingWorkingTaskForm(GetTaskForTests()))
			{
				form.Show();
				AssertEquals(true, form.Visible);
				form.SuspendExistingTaskButton.PerformClick();
				AssertEquals(false, form.Visible);
				AssertEquals(DialogResult.Yes, form.DialogResult);
			}
		}

		public void TestCancelButton()
		{
			using (ExistingWorkingTaskForm form = new ExistingWorkingTaskForm(GetTaskForTests()))
			{
				form.Show();
				AssertEquals(true, form.Visible);
				AssertEquals(form.CancelButton, form.CancelButtonX);

				form.CancelButtonX.PerformClick();
				AssertEquals(false, form.Visible);
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new ExistingWorkingTaskForm(GetTaskForTests());
		}

		ProcessTask GetTaskForTests()
		{
			OrgOpportunity opportunityWithWorkingTask = Factory.NewWithValidTestData<OrgOpportunity>();
			ProcessTask workingTask = opportunityWithWorkingTask.WorkflowItems.AddNew();
			workingTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			workingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			OrgOpportunity opportunityWithSuspendedTask = Factory.NewWithValidTestData<OrgOpportunity>();
			ProcessTask suspendedTask = opportunityWithSuspendedTask.WorkflowItems.AddNew();
			suspendedTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			suspendedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();
			return workingTask;
		}
	}
}
