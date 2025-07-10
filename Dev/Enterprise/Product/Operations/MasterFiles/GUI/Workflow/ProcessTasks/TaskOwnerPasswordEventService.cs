using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	public class TaskOwnerPasswordEventService : IService
	{
		public void UnhookPasswordRequestEvent()
		{
			if (taskOwnerPasswordEventManagers != null)
			{
				foreach (TaskOwnerPasswordEventManager manager in taskOwnerPasswordEventManagers)
				{
					manager.Dispose();
				}
				taskOwnerPasswordEventManagers = null;
			}
		}

		public void HookPasswordRequestEvent(ProcessTaskCollection tasks)
		{
			List<TaskOwnerPasswordEventManager> managerList = new List<TaskOwnerPasswordEventManager>(tasks.Count);
			foreach (ProcessTask task in tasks)
			{
				managerList.Add(TaskOwnerPasswordEventManager.Manage(task));
			}
			taskOwnerPasswordEventManagers = managerList.ToArray();
		}

		TaskOwnerPasswordEventManager[] taskOwnerPasswordEventManagers;
	}
}
