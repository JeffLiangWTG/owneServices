using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public class TaskOwnerPasswordEventManagerTest : TestCaseWithFactory
	{
		[UseSnapshotProtection(true)]
		public void TestManage()
		{
			SetupDummyStandaloneWorkflowTaskTypes();
			GlbStaff newStaff = new BusinessObjectFactory().NewWithValidTestData<GlbStaff>();
			newStaff.GS_LoginName = "new.user";
			newStaff.GS_Code = "NEW";
			newStaff.StaffPlainTextPassword = "test123";
			newStaff.Factory.Save();

			ProcessTask task = Factory.New<ProcessTask>();
			task.P9_Type = "RVW";
			task.P9_GS_NKAssignedStaffMember = "NEW";

			using (TaskOwnerPasswordEventManager manager = TaskOwnerPasswordEventManager.Manage(task))
			{
				AssertNull("Precondition", ZFormModaliser.LastFormShownDialogForTest);

				SetTaskOwnerPasswordDialogResult(DialogResult.Cancel, "test123");
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				AssertEquals(typeof(TaskOwnerPasswordRequestForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("Should be cancelled", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

				ZFormModaliser.LastFormShownDialogForTest = null;
				SetTaskOwnerPasswordDialogResult(DialogResult.OK, "test12390");
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				AssertEquals(typeof(TaskOwnerPasswordRequestForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("Incorrect password", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

				ZFormModaliser.LastFormShownDialogForTest = null;
				SetTaskOwnerPasswordDialogResult(DialogResult.OK, "test123");
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				AssertEquals(typeof(TaskOwnerPasswordRequestForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("Should be closed", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
			}

			ZFormModaliser.LastFormShownDialogForTest = null;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertNull("Event unhooked on disposed", ZFormModaliser.LastFormShownDialogForTest);
		}

		public static bool IsManaged(ProcessTask task)
		{
			MulticastDelegate eventHandlers = (MulticastDelegate)typeof(ProcessTask).GetField("TaskOwnerPasswordRequested", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(task);
			if (eventHandlers != null)
			{
				Delegate[] invocationList = eventHandlers.GetInvocationList();
				foreach (Delegate handler in invocationList)
				{
					if (handler.Method.Name == "task_TaskOwnerPasswordRequested"
						&& handler.Target.GetType() == typeof(TaskOwnerPasswordEventManager))
					{
						return true;
					}
				}
			}

			return false;
		}

		void SetTaskOwnerPasswordDialogResult(DialogResult result, string password)
		{
			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(delegate(object dialogOrForm)
			{
				TaskOwnerPasswordRequestForm dialog = (TaskOwnerPasswordRequestForm)dialogOrForm;
				dialog.Show();
				dialog.SetPasswordTextForTest(password);
				dialog.AcceptButton.PerformClick();
			});
			ZFormModaliser.ResultToReturnFromShowDialog = result;
		}

		void SetupDummyStandaloneWorkflowTaskTypes()
		{
			CategorisedWorkflowTaskTypesCollection categorisedTaskTypesCollection = new CategorisedWorkflowTaskTypesCollection();
			CategorisedWorkflowTaskTypes categorisedTaskTypes = categorisedTaskTypesCollection.AddNew();
			categorisedTaskTypes.Code = "STA";
			WorkflowTaskType codingTask = categorisedTaskTypes.TaskTypes.AddNew();
			codingTask.Code = "COD";
			WorkflowTaskType reviewTask = categorisedTaskTypes.TaskTypes.AddNew();
			reviewTask.Code = "RVW";
			reviewTask.CanCloseTaskNotAssignedToSelf = false;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypesCollection);
			Env.Security.WorkflowTasksCloseTaskNotAssignedToSelf.IsAllowed = false;
		}
	}
}
