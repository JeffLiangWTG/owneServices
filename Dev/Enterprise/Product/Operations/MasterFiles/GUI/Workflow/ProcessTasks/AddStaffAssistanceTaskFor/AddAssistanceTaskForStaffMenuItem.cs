using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class AddAssistanceTaskForStaffMenuItem : ZMenuItem
	{
		public AddAssistanceTaskForStaffMenuItem(IProcessTask task, AddAssistanceTaskForStaffMenuItemInfo menuInfo, Func<IBusinessObjectCollection> taskCollectionGetter, bool saveAfterActions, bool informUserOnTaskCreation)
			: base(menuInfo.MenuItemDescription)
		{
			this.task = task;
			this.userCode = menuInfo.UserCode;
			this.taskCollectionGetter = taskCollectionGetter;
			this.saveAfterActions = saveAfterActions;
			this.informUserOnTaskCreation = informUserOnTaskCreation;
			this.Click += MenuItemClick;
		}

		readonly bool informUserOnTaskCreation;

		void MenuItemClick(object sender, EventArgs e)
		{
			if (AssistWithThisTaskHelper.TryCreateAssistTaskOrGetExistingOne(task, out IProcessTask assistTask, userCode, allowUsingAppropriateCapabilityTask: false))
			{
				taskCollectionGetter?.Invoke().Add(assistTask);

				if (saveAfterActions)
				{
					((BusinessObject)assistTask).Factory.Save();
				}

				if (informUserOnTaskCreation)
				{
					Globals.Message.ShowInformation(Res.GetString("46D14323-5A3F-41F6-80D3-B38F2D1A6262", "An assistance task for the user was successfully created."));
				}
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("E30EADBC-C632-4F46-A488-9FCD7FF83BC0", "An appropriate assistance task for this user already exists."));
			}
		}

		readonly IProcessTask task;
		readonly string userCode;
		readonly Func<IBusinessObjectCollection> taskCollectionGetter;
		readonly bool saveAfterActions;
	}
}
