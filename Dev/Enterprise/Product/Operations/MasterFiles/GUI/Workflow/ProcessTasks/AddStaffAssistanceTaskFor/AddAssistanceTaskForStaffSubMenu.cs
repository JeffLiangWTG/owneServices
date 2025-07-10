using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class AddAssistanceTaskForStaffSubMenu : ZLazyPopulatingMenuItem
	{
		public AddAssistanceTaskForStaffSubMenu(IProcessTask task, Func<IReadOnlyCollection<AddAssistanceTaskForStaffMenuItemInfo>> staffCollectionGetter, Func<IBusinessObjectCollection> taskCollectionGetter, bool saveAfterActions, bool informUserOnTaskCreation)
			: base(GetName(), MenuItemFunc(task, staffCollectionGetter, taskCollectionGetter, saveAfterActions, informUserOnTaskCreation), shouldCache: true)
		{
		}

		static ResourceString GetName()
		{
			return ResString.GetMultilingualString("EBB21187-A1AC-4E78-984B-A5EA33540670", "Staff");
		}

		static Func<IReadOnlyCollection<ZMenuItem>> MenuItemFunc(IProcessTask task, Func<IReadOnlyCollection<AddAssistanceTaskForStaffMenuItemInfo>> staffCollectionGetter, Func<IBusinessObjectCollection> taskCollectionGetter, bool saveAfterActions, bool informUserOnTaskCreation)
		{
			return new Func<IReadOnlyCollection<ZMenuItem>>(() =>
			{
				var result = new List<ZMenuItem>();

				foreach (var staffInfo in staffCollectionGetter())
				{
					result.Add(new AddAssistanceTaskForStaffMenuItem(task, staffInfo, taskCollectionGetter, saveAfterActions, informUserOnTaskCreation));
				}
				return result;
			});
		}
	}
}
