using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class AddAssistanceTaskForCapabilityMenuItem : ZMenuItem
	{
		public AddAssistanceTaskForCapabilityMenuItem(IProcessTask task, AddAssistanceTaskForCapabilityMenuItemInfo menuInfo, Func<IBusinessObjectCollection> taskCollectionGetter, bool saveAfterActions, bool informUserOnTaskCreation)
			: base(menuInfo.MenuItemDescription)
		{
			this.task = task;
			this.capabilityPK = menuInfo.CapabilityPK;
			this.taskCollectionGetter = taskCollectionGetter;
			this.saveAfterActions = saveAfterActions;
			this.informUserOnTaskCreation = informUserOnTaskCreation;
			this.Click += MenuItemClick;
		}

		readonly bool informUserOnTaskCreation;

		void MenuItemClick(object sender, EventArgs e)
		{
			if (AssistWithThisTaskHelper.TryCreateAssistTaskOrGetExistingOne(task, out IProcessTask assistTask, capabilityPK))
			{
				taskCollectionGetter?.Invoke().Add(assistTask);

				if (saveAfterActions)
				{
					((BusinessObject)assistTask).Factory.Save();
				}

				if (informUserOnTaskCreation)
				{
					Globals.Message.ShowInformation(Res.GetString("7650E280-B84A-4DEB-B4F0-E40B6D09C6BB", "An assistance task for the capability was successfully created."));
				}
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("C95C9852-C698-4FE1-A0E6-1DC186ACDE27", "An appropriate assistance task for this capability already exists."));
			}
		}

		readonly IProcessTask task;
		readonly ZGuid capabilityPK;
		readonly Func<IBusinessObjectCollection> taskCollectionGetter;
		readonly bool saveAfterActions;
	}
}
