using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.GUI
{
	public static class AttachLoadListToConsolGUIHelper
	{
		public static void ShowPopupModuleAndAttachSelected(ForwardingConsol forwardingConsol)
		{
			var factory = new BusinessObjectFactory();
			List<IHVLVOriginLoadList> loadListsInNewFactory = null;
			var virtualFindBox = new VirtualFindBox();
			var moduleDecisionProvider = ObjectFactory.Get<IModuleDecisionProvider>("AttachLoadListToConsolModuleDecisionProvider", forwardingConsol, virtualFindBox);
			var logger = new SimpleLogger();
			var consolAllocator = ObjectFactory.Get<IHVLVOriginLoadListConsolAllocator>(nameof(IHVLVOriginLoadListConsolAllocator), logger);

			using (var filterModule = ZModuleFactory.Instance.Create(ModuleIDs.HVLVOriginLoadList) as ZFilterModule)
			{
				if (filterModule == null || moduleDecisionProvider == null || forwardingConsol == null)
				{
					return;
				}

				filterModule.OverrideModuleDecisionProvider(moduleDecisionProvider);

				using (var embeddedModulePopup = new EmbeddedModulePopup(filterModule))
				{
					embeddedModulePopup.Selected += (sender, args) =>
					{
						loadListsInNewFactory = new List<IHVLVOriginLoadList>();
						foreach (var loadList in args?.SelectedBusinessObjects)
						{
							loadListsInNewFactory.Add((IHVLVOriginLoadList)factory.ImportFromAnotherFactory(loadList));
						}
					};
					virtualFindBox.Initialize(moduleDecisionProvider.List, embeddedModulePopup);
					ZFormModaliser.ShowDialogAndDispose(embeddedModulePopup);
				}
			}

			if (loadListsInNewFactory?.Count > 0)
			{
				var loadListPKs = loadListsInNewFactory.Select(o => o.PK).ToList();
				var consolInNewFactory = (ForwardingConsol)factory.ImportFromAnotherFactory(forwardingConsol);

				if (!consolAllocator.TryAcquireApplicationLocks(loadListPKs,
					(lockedPKs) =>
					{
						var loadListsWithAppLock = loadListsInNewFactory.Where(o => lockedPKs.Any(x => x == o.PK));
						var success = consolAllocator.TryAttachToConsol(consolInNewFactory, loadListsWithAppLock, out _);
						var loggerContent = logger.ToString().Trim();

						if (success)
						{
							if (!loggerContent.IsNullOrEmpty())
							{
								Globals.Message.ShowWarning(loggerContent, AttachLoadListToConsolWarningCaption);
							}
							factory.Save();
						}
						else
						{
							Globals.Message.ShowError(loggerContent, AttachLoadListToConsolErrorCaption);
						}
					},

					out var attachLoadListToConsolConcurrentErrorMessage))
				{
					Globals.Message.ShowError(attachLoadListToConsolConcurrentErrorMessage, AttachLoadListToConsolErrorCaption);
				}
			}
		}

		static string AttachLoadListToConsolErrorCaption => Res.GetString("5f72cfb3-2445-41b0-b4b3-8e0176347bca", "Unable to complete action");
		public static string AttachLoadListToConsolWarningCaption => Res.GetString("29d991d9-f471-4adf-a805-82b29e1ea373", "Action completed with warning");
	}
}
