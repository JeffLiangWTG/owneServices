using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class AddAssistanceTaskForMenuItem : ZMenuItem
	{
		public AddAssistanceTaskForMenuItem(IProcessTask task,
			Func<IReadOnlyCollection<AddAssistanceTaskForStaffMenuItemInfo>> staffCollectionGetter, Func<IReadOnlyCollection<AddAssistanceTaskForCapabilityMenuItemInfo>> capabilityCollectionGetter,
			Func<IBusinessObjectCollection> taskCollectionGetter = null,
			bool saveAfterActions = false, bool informUserOnTaskCreation = false)
				: base(GetName())
		{
			MenuItems.Add(new AddAssistanceTaskForStaffSubMenu(task, staffCollectionGetter, taskCollectionGetter, saveAfterActions, informUserOnTaskCreation));
			MenuItems.Add(new AddAssistanceTaskForCapabilitySubMenu(task, capabilityCollectionGetter, taskCollectionGetter, saveAfterActions, informUserOnTaskCreation));
		}

		static ResourceString GetName()
		{
			return ResString.GetMultilingualString("A95A8EB4-416A-4802-8135-E29A4D309408", "Add Assistance Task For");
		}

		public static bool ShouldAddMenuItem(IProcessTask task)
		{
			return AssistWithThisTaskHelper.CanBeUsedForTargetTask(task);
		}
	}
}
