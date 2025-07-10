using System;
using System.Windows.Forms;
using CargoWise.Data;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class TaskOwnerPasswordEventManager : IDisposable
	{
		public static TaskOwnerPasswordEventManager Manage(ProcessTask task)
		{
			return new TaskOwnerPasswordEventManager(task);
		}

		TaskOwnerPasswordEventManager(ProcessTask task)
		{
			this.task = task;
			task.TaskOwnerPasswordRequested += new EventHandler<ProcessTask.PasswordRequestEventArgs>(task_TaskOwnerPasswordRequested);
		}

		void task_TaskOwnerPasswordRequested(object sender, ProcessTask.PasswordRequestEventArgs e)
		{
			if (Db.Connection.AppTransactionCount < 1)
			{
				using (TaskOwnerPasswordRequestForm dialog = new TaskOwnerPasswordRequestForm())
				{
					dialog.LoginName = e.loginName;
					DialogResult result = ZFormModaliser.ShowDialogWithoutDispose(dialog);
					if (result == DialogResult.OK)
					{
						e.IsValidPassword = dialog.IsValidPassword;
					}
				}
			}
		}

		public void Dispose()
		{
			task.TaskOwnerPasswordRequested -= new EventHandler<ProcessTask.PasswordRequestEventArgs>(task_TaskOwnerPasswordRequested);
		}

		public readonly ProcessTask task;
	}
}
