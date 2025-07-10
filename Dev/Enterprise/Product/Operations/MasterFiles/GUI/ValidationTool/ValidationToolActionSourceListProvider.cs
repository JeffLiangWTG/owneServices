using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI;

sealed class ValidationToolActionSourceListProvider : IValidationToolActionSourceListProvider
{
	ICodeDescriptionPairList IValidationToolActionSourceListProvider.GetActionSourceList(ControllerID controllerId, string countryCode)
	{
		var result = new CodeDescriptionPairList();

		if (controllerId is null)
		{
			return result;
		}

		var controller = ZControllerFactory.CreateWithCountry(controllerId, countryCode);
		if (controller is null)
		{
			return result;
		}

		var typeOfTopLevelBusinessObject = controller.TypeOfTopLevelBusinessObject;
		if (typeOfTopLevelBusinessObject is null)
		{
			return result;
		}

		var nullBusinessObject = controller.Factory.New(typeOfTopLevelBusinessObject);
		using var form = (ZForm)((ZControllerInternals)controller).GetForm(nullBusinessObject);
		if (form is null)
		{
			return result;
		}

		BuildAllPluginMenuItems();

		MustNullBusinessObjectPostSetupSinceFormHasIssuesWithNulls();

		BuildDocumentsMenuItem();

		foreach (var menuItem in form.Menu?.MenuItems.OfType<ZMenuItem>() ?? [])
		{
			result.AddRange(GetActionSourceList(menuItem));
		}

		return result;

		void BuildAllPluginMenuItems()
		{
			ZFormPlugInStrategy.SetupPlugInsBeforeLoad(form);
		}

		void MustNullBusinessObjectPostSetupSinceFormHasIssuesWithNulls()
		{
			nullBusinessObject.IsNull = true;
		}

		void BuildDocumentsMenuItem()
		{
			if (form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn) is not { } docDataPlugIn)
			{
				return;
			}

			EnsureNoChangesBeforeDocumentsMenuItemSetup();
			docDataPlugIn.OnMenuShown();
		}

		void EnsureNoChangesBeforeDocumentsMenuItemSetup()
		{
			nullBusinessObject.HasChanges = false;
			foreach (var child in ((IBusiness)nullBusinessObject).Children.OfType<BusinessObject>())
			{
				child.HasChanges = false;
			}
		}
	}

	internal CodeDescriptionPairList GetActionSourceList(ZMenuItem menu)
	{
		var result = new CodeDescriptionPairList();
		foreach (var item in GetAllMenuItems(menu.MenuItems))
		{
			result.AddPair(item.ActionSourceCode, GetActionSourcePath(item));
		}

		return result;
	}

	static ZString GetActionSourcePath(ZMenuItem menuItem)
	{
		Stack<ZString> paths = new();
		while (menuItem != null)
		{
			paths.Push(KMenuItem.StripAcceleratorKeys(menuItem.Text));
			menuItem = menuItem.Parent as ZMenuItem;
		}

		return ZString.Join(" > ", paths.ToArray());
	}

	static IEnumerable<ZMenuItem> GetAllMenuItems(Menu.MenuItemCollection items)
	{
		foreach (var menuItem in items.OfType<ZMenuItem>())
		{
			if (menuItem.Text != ZMenuItem.Separator && menuItem.MenuItems.Count == 0)
			{
				yield return menuItem;
			}

			foreach (var subMenuItem in GetAllMenuItems(menuItem.MenuItems))
			{
				yield return subMenuItem;
			}
		}
	}
}
