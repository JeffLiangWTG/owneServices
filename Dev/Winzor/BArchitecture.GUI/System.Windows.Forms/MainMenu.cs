using System.ComponentModel;
using CargoWise.Blazor.Client.Integration.Menus;

namespace System.Windows.Forms;

public partial class MainMenu : Menu
{
	public MainMenu()
	{
	}

	public MainMenu(IContainer container)
	{
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (form is not null)
			{
				form.Menu = null;
			}
		}

		base.Dispose(disposing);
	}

	public MainMenu(MenuItem[] items) : base(items)
	{
	}

	protected internal override void OnRootMenuItemChanged()
	{
		// Render the main menu again if a root menu item has changed.
		// Changes to items within SubMenus are otherwise retrieved on every DropDownOpening event.
		form?.RenderMainMenu();
		base.OnRootMenuItemChanged();
	}

	public Form? GetForm() => form;
	internal Form? form;

	internal IMenuDisplayer? GetMenuDisplayer() => GetForm()?.CargoWiseClientServices?.MenuDisplayer;

	internal void ShowDropDown(Guid menuItemId)
	{
		var menuDisplayer = GetMenuDisplayer();
		if (form == null || menuDisplayer == null)
		{
			return;
		}

		form.InvokeRenderDispatcher(async () => await menuDisplayer.SendShowDropDownRequestAsync(menuItemId));
	}
}
