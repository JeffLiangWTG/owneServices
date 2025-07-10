using System;
using System.Threading.Tasks;
using CargoWise.Blazor.Client.Integration.Menus;

namespace WinzorTestFramework;

public class TestMenuDisplayer : IMenuDisplayer
{
	public Task<MenuShowResultCode> SendShowMenuRequestAsync(MenuInteropModel menu, Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>> loadSubMenuCallbackAsync, Func<MenuClosedResult, Task> menuClosedCallbackAsync)
	{
		Menu = menu;
		LoadSubMenuCallback = loadSubMenuCallbackAsync;
		MenuClosedCallback = menuClosedCallbackAsync;
		ShowMenuRequestCount++;
		return Task.FromResult(MenuShowResultCode.Shown);
	}

	public Task<MenuShowResultCode> SendCloseMenuRequestAsync()
	{
		return Task.FromResult(MenuShowResultCode.Closed);
	}

	public Task<MenuDropDownResultCode> SendShowDropDownRequestAsync(Guid menuItemId)
	{
		return Task.FromResult(MenuDropDownResultCode.Shown);
	}

	public MenuInteropModel Menu { get; private set; }
	public Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>> LoadSubMenuCallback { get; private set; }
	public Func<MenuClosedResult, Task> MenuClosedCallback { get; private set; }
	public int ShowMenuRequestCount { get; private set; }
}
