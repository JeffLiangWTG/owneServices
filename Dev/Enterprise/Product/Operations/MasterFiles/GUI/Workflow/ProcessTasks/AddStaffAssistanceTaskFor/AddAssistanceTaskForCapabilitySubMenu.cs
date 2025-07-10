using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class AddAssistanceTaskForCapabilitySubMenu : ZLazyPopulatingMenuItem
	{
		public AddAssistanceTaskForCapabilitySubMenu(IProcessTask task, Func<IReadOnlyCollection<AddAssistanceTaskForCapabilityMenuItemInfo>> capabilityCollectionGetter, Func<IBusinessObjectCollection> taskCollectionGetter, bool saveAfterActions, bool informUserOnTaskCreation)
			: base(GetName(), MenuItemFunc(task, capabilityCollectionGetter, taskCollectionGetter, saveAfterActions, informUserOnTaskCreation), shouldCache: true)
		{
		}

		static ResourceString GetName()
		{
			return ResString.GetMultilingualString("E7722F48-C954-4BA1-B5ED-05D588D16D17", "Capability");
		}

		static Func<IReadOnlyCollection<ZMenuItem>> MenuItemFunc(IProcessTask task, Func<IReadOnlyCollection<AddAssistanceTaskForCapabilityMenuItemInfo>> capabilityCollectionGetter, Func<IBusinessObjectCollection> taskCollectionGetter, bool saveAfterActions, bool informUserOnTaskCreation)
		{
			return new Func<IReadOnlyCollection<ZMenuItem>>(() =>
			{
				var result = new List<ZMenuItem>();

				foreach (var capabilityInfo in capabilityCollectionGetter())
				{
					result.Add(new AddAssistanceTaskForCapabilityMenuItem(task, capabilityInfo, taskCollectionGetter, saveAfterActions, informUserOnTaskCreation));
				}
				return result;
			});
		}
	}
}
