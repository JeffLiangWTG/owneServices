using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public class ModuleGridTaskStatusChanger : IModuleFilterTaskStatusChanger
	{
		public ModuleGridTaskStatusChanger(ZGrid grid)
		{
			if (grid == null)
			{
				throw new ArgumentNullException(nameof(grid), "");
			}

			this.Grid = grid;
		}

		readonly ZGrid Grid;

		#region Menu

		public void Initialise()
		{
			if (!Grid.IsDisposed && Grid.ContextMenu != null)
			{
				Grid.ContextMenu.Popup += new EventHandler(SetUpMenuItem);
			}
		}

#if DEBUG
		internal int CountSetupMenuAccessForTest;
#endif

		void SetUpMenuItem(object sender, EventArgs e)
		{
			var selectedElements = Grid.SelectedElements;

			if (selectedElements.Length == 1)
			{
				Grid.SetupTaskStatusMenuItem(GlbStaff.CurrentUser.GS_FullName, (tasksMenuItem, userTasksMenuItem) =>
				{
					SetupMenu(selectedElements[0], tasksMenuItem, userTasksMenuItem);
				});
			}
			else
			{
				Grid.RemoveExistingTasksMenuItem();
			}
		}

		void SetupMenu(BusinessObject bizo, ZMenuItem allTasksMenuItem, ZMenuItem userTasksMenuItem)
		{
#if DEBUG
			CountSetupMenuAccessForTest++;
#endif
			var workflowProvider = bizo as IWorkflowProvider;

			if (workflowProvider != null)
			{
				var workflowItems = workflowProvider.WorkflowItems;
				var tasks = workflowItems.Tasks;
				tasks.Sort(tasks.GetDefaultOrderComparer());

				foreach (ProcessTask task in tasks)
				{
					AddMenuItemForTask(allTasksMenuItem, userTasksMenuItem, task);
				}

				if (allTasksMenuItem.MenuItems.Count == 0)
				{
					allTasksMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("MasterFiles.ProcessTasks.Tasks.NoTasks", "No tasks for this job")) { Enabled = false });
				}

				if (userTasksMenuItem.MenuItems.Count == 0)
				{
					userTasksMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("MasterFiles.ProcessTasks.UserTasks.NoTasks", "No tasks for {0}", GlbStaff.CurrentUser.GS_FullName)) { Enabled = false });
				}
			}
			else
			{
				var caption = ResString.GetMultilingualString("MasterFiles.ProcessTasks.Tasks.NoWorkflowSupport", "This record does not support Workflow");

				allTasksMenuItem.MenuItems.Add(new ZMenuItem(caption) { Enabled = false });
				userTasksMenuItem.MenuItems.Add(new ZMenuItem(caption) { Enabled = false });
			}
		}

		void AddMenuItemForTask(MenuItem tasksMenuItem, MenuItem userTasksMenuItem, ProcessTask task)
		{
			ZMenuItem taskMenuItem = new ZMenuItem();
			string assignedStaff = task.AssignedStaffMember != null ? task.AssignedStaffMember.GS_FullName : (ZString)Res.GetString("MasterFiles.ProcessTasks.Tasks.Unassigned", "Unassigned");
			MultilingualString taskAsString = (NoResString)string.Format("{0}. {1} - {2} ({3})", task.P9_Sequence, task.P9_Description, assignedStaff, task.StatusDescription);

			taskMenuItem.Checked = task.P9_Status == ProcessTaskStatusCodeList.Codes.Working;
			taskMenuItem.Caption = taskAsString;
			taskMenuItem.Tag = task.PK;
			taskMenuItem.Click += new EventHandler(TaskMenuItem_Click);
			tasksMenuItem.MenuItems.Add(taskMenuItem);

			if (task.P9_GS_NKAssignedStaffMember == GlbStaff.CurrentUser.GS_Code)
			{
				MenuItem clonedMenuItem = (ZMenuItem)taskMenuItem.CloneMenu();
				clonedMenuItem.Tag = taskMenuItem.Tag;
				userTasksMenuItem.MenuItems.Add(clonedMenuItem);
			}
		}

		#endregion

		#region Status Changes

		void TaskMenuItem_Click(object sender, EventArgs e)
		{
			var taskPK = (ZGuid)((MenuItem)sender).Tag;
			var task = new BusinessObjectFactory().Load<ProcessTask>(taskPK);

			if (task == null)
			{
				return;
			}

			switch (task.P9_Status)
			{
				case ProcessTaskStatusCodeList.Codes.Assigned:
					WorkOnAssignedTask(task);
					break;

				case ProcessTaskStatusCodeList.Codes.Working:
					CloseWorkingTask(task);
					break;

				case ProcessTaskStatusCodeList.Codes.Suspended:
					ResumeSuspendedTask(task);
					break;

				default:
					Globals.Message.Show(Res.GetString("48abdbec-e227-4a14-8e3e-07d4e5b5f67e", "To manage this task, please open the Job and assign the task from the 'Workflow' page."));
					break;
			}
		}

		void WorkOnAssignedTask(ProcessTask task)
		{
			DialogResult result = Globals.Message.Show(Res.GetString("7c6a212d-df3f-41de-a291-c24f952c9ab6", "This task is assigned to {0}. Do you want to change the status to Working?", task.P9_GS_NKAssignedStaffMember), Res.GetString("06b66038-b6d3-48c1-b018-9ff55f9c7072", "Working?"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (result == DialogResult.Yes && TryChangeTaskStatus(task, () => { task.P9_Status = ProcessTaskStatusCodeList.Codes.Working; }))
			{
				task.Factory.Save();
			}
		}

		bool TryChangeTaskStatus(ProcessTask task, Action doChangeStatus)
		{
			using (task.SetTemporaryStatusChangeMode(ProcessTaskStatusChangeModeCodeList.Codes.TaskMenu))
			{
				doChangeStatus();
			}

			if (TaskHasValidationError(task))
			{
				ShowValidationErrorMessage(task);
				return false;
			}
			return true;
		}

		void ShowValidationErrorMessage(ProcessTask task)
		{
			using (var messageBox = new ZErrorMessageBox(task))
			{
				ZFormModaliser.ShowMessageBoxWithoutDispose(messageBox);
			}
		}

		bool TaskHasValidationError(ProcessTask task)
		{
			task.Validation.ValidateAll();
			return task.HasErrors;
		}

		void CloseWorkingTask(ProcessTask task)
		{
			DialogResult result;

			using (TaskClosedForm form = new TaskClosedForm(task))
			{
#if DEBUG
				LastShownTaskClosedForm = form;
#endif
				result = ZFormModaliser.ShowDialogWithoutDispose(form);
			}

#if DEBUG
			if (Globals.IsTest)
			{
				result = NextResultForTaskClosedFormTest;
			}
#endif

			if (result == DialogResult.Yes && TryChangeTaskStatus(task, () => CloseWorkingTaskCore(task)))
			{
				task.Factory.Save();
			}

#if DEBUG
			if (Globals.IsTest)
			{
				NextResultForTaskClosedFormTest = DialogResult.None;
			}
#endif
		}

#if DEBUG
		internal TaskClosedForm LastShownTaskClosedForm;
		internal DialogResult NextResultForTaskClosedFormTest;

		internal
#endif
		void CloseWorkingTaskCore(ProcessTask task)
		{
			using (TaskOwnerPasswordEventManager.Manage(task))
			{
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			}
		}

		void ResumeSuspendedTask(ProcessTask task)
		{
			DialogResult result = Globals.Message.Show(Res.GetString("b888af5b-2b2e-4540-bf54-e489926f94cb", "This task is currently suspended by {0}. Do you want to resume the task?", task.P9_GS_NKAssignedStaffMember), Res.GetString("b66715d1-d9d5-4325-8307-cb47e431d61c", "Resume?"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (result == DialogResult.Yes && TryChangeTaskStatus(task, () => { task.P9_Status = ProcessTaskStatusCodeList.Codes.Working; }))
			{
				task.Factory.Save();
			}
		}

		#endregion
	}
}
