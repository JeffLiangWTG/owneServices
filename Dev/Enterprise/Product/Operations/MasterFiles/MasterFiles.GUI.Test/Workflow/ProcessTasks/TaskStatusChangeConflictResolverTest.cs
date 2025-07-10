using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.GUI
{
	sealed class TaskStatusChangeConflictResolverTest : TestCaseWithFactory
	{
		public void TestExistingTasksDialog()
		{
			Factory.ClearCachedValue<ITaskStatusChangeConflictResolver>(nameof(ITaskStatusChangeConflictResolver));
			Factory.GetCachedValue<ITaskStatusChangeConflictResolver>(nameof(ITaskStatusChangeConflictResolver), () => new TaskStatusChangeConflictResolver());

			OrgOpportunity opp = Factory.New<OrgOpportunity>();
			ProcessTask task1 = opp.WorkflowItems.AddNew();
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			ProcessTask task2 = opp.WorkflowItems.AddNew();
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			ProcessTask task3 = opp.WorkflowItems.AddNew();
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			TaskStatusChangeConflictResolver.NextDialogResultToReturnForTest.Value = DialogResult.Yes;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task2.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, task1.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task3.P9_Status);

			TaskStatusChangeConflictResolver.NextDialogResultToReturnForTest.Value = DialogResult.No;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task2.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, task1.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task3.P9_Status);

			TaskStatusChangeConflictResolver.NextDialogResultToReturnForTest.Value = DialogResult.No;
			TaskStatusChangeConflictResolver.NextTaskToStartForTest.Value = task3;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, task2.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, task1.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task3.P9_Status);
		}

		public void TestExistingTasksDialog_DefaultIfCalledInTransaction()
		{
			Factory.ClearCachedValue<ITaskStatusChangeConflictResolver>(nameof(ITaskStatusChangeConflictResolver));
			Factory.GetCachedValue<ITaskStatusChangeConflictResolver>(nameof(ITaskStatusChangeConflictResolver), () => new TaskStatusChangeConflictResolver());

			OrgOpportunity opp = Factory.New<OrgOpportunity>();
			ProcessTask task1 = opp.WorkflowItems.AddNew();
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			ProcessTask task2 = opp.WorkflowItems.AddNew();
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			ProcessTask task3 = opp.WorkflowItems.AddNew();
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			try
			{
				Db.Connection.BeginTransaction();

				TaskStatusChangeConflictResolver.NextDialogResultToReturnForTest.Value = DialogResult.No;
				task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				AssertEquals("We might have clicked no, but since we were in a transaction we were never given the choice.", ProcessTaskStatusCodeList.Codes.Working, task2.P9_Status);
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}
	}
}
