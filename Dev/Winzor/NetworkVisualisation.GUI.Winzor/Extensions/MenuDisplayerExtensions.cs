using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Microsoft.AspNetCore.Components;
using WinzorFramework.JSInterop;

namespace CargoWise.NetworkVisualisation.GUI.Extensions;

public static class MenuDisplayerExtensions
{
	/// <summary>
	/// Displays a context menu with specified network action menu items.
	/// </summary>
	/// <param name="menuDisplayer">The menu displayer instance.</param>
	/// <param name="args">The mouse event arguments containing the position where the context menu should appear.</param>
	/// <param name="menuItems">The collection of network action menu items to display in the context menu.</param>
	/// <param name="onItemSelected">The callback function to invoke when a menu item is selected.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public static async Task ShowContextMenuAsync(this IMenuDisplayer menuDisplayer, WebMouseEventArgs args, IEnumerable<NetworkActionMenuItem> menuItems, Func<INetworkAction, WebMouseEventArgs, Task> onItemSelected)
	{
		var items = new Dictionary<Guid, NetworkActionMenuItem>();

		IEnumerable<MenuItemInteropModel> GetMenuItems(IEnumerable<NetworkActionMenuItem> menuItems, Guid? parent)
		{
			foreach (var menuItem in menuItems)
			{
				var itemId = Guid.NewGuid();
				if (menuItem is null)
				{
					yield return new MenuItemInteropModel() { ParentId = parent, Id = itemId, IsSeparator = true };
					continue;
				}

				items[itemId] = menuItem;

				yield return new MenuItemInteropModel()
				{
					ParentId = parent,
					Id = itemId,
					Text = menuItem.Name,
					Enabled = menuItem.Enabled,
					IsChecked = menuItem.Ticked,
					Loadable = menuItem.Items.Any(),
				};
			}
		}

		Task<MenuItemInteropModel[]> OnSubMenuRequested(SubMenuLoadRequest request)
		{
			var itemId = request.ItemId;
			if (items.ContainsKey(itemId))
			{
				var item = items[itemId];
				return Task.FromResult(GetMenuItems(item.Items, itemId).ToArray());
			}
			return Task.FromResult(Array.Empty<MenuItemInteropModel>());
		}

		async Task OnMenuClosedAsync(MenuClosedResult result)
		{
			if (result.MenuClosedResultCode == MenuClosedResultCode.SelectionMade)
			{
				await onItemSelected(items[result.ItemId.Value].Action, args);
			}
		}

		var menuId = Guid.NewGuid();
		var menu = new MenuInteropModel(menuId, (int)args.ClientX, (int)args.ClientY, DropDownDirection.BelowRight, MenuType.ContextMenu, GetMenuItems(menuItems, menuId).ToArray());
		var result = await menuDisplayer.SendShowMenuRequestAsync(menu, OnSubMenuRequested, OnMenuClosedAsync);
	}

	/// <summary>
	/// Displays a ribbon context menu with specified ribbon button view models.
	/// </summary>
	/// <param name="menuDisplayer">The menu displayer instance.</param>
	/// <param name="x">The X-coordinate where the context menu should appear.</param>
	/// <param name="y">The Y-coordinate where the context menu should appear.</param>
	/// <param name="menuItems">The collection of ribbon button view models to display in the context menu.</param>
	/// <param name="onItemSelected">The callback function to invoke when a menu item is selected.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public static async Task ShowRibbonContextMenuAsync(this IMenuDisplayer menuDisplayer, double x, double y, IEnumerable<RibbonButtonViewModel> menuItems, Func<INetworkAction, Task> onItemSelected)
	{
		var items = new Dictionary<Guid, RibbonButtonViewModel>();

		IEnumerable<MenuItemInteropModel> GetMenuItems(IEnumerable<RibbonButtonViewModel> menuItems, Guid? parent)
		{
			foreach (var menuItem in menuItems)
			{
				var itemId = Guid.NewGuid();

				items[itemId] = menuItem;

				yield return new MenuItemInteropModel()
				{
					ParentId = parent,
					Id = itemId,
					Text = menuItem.Label,
					Description = menuItem.Tooltip,
					Image = menuItem.Image64,
					IsChecked = menuItem.IsChecked
				};
			}
		}

		async Task OnMenuClosedAsync(MenuClosedResult result)
		{
			if (result.MenuClosedResultCode == MenuClosedResultCode.SelectionMade)
			{
				await onItemSelected(items[result.ItemId.Value].Action);
			}
		}

		var menuId = Guid.NewGuid();
		var menu = new MenuInteropModel(menuId, (int)x, (int)y, DropDownDirection.BelowRight, MenuType.ContextMenu, GetMenuItems(menuItems, menuId).ToArray());
		var result = await menuDisplayer.SendShowMenuRequestAsync(menu, (_) => Task.FromResult(Array.Empty<MenuItemInteropModel>()), OnMenuClosedAsync);
	}

	/// <summary>
	/// Displays a clipboard context menu.
	/// </summary>
	/// <param name="menuDisplayer">The menu displayer instance.</param>
	/// <param name="args">The mouse event arguments containing the position where the context menu should appear.</param>
	/// <param name="clipboardInterop">The clipboard interop instance for interacting with the clipboard.</param>
	/// <param name="element">The element reference where the context menu is attached.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public static async Task ShowClipboardContextMenuAsync(this IMenuDisplayer menuDisplayer, WebMouseEventArgs args, IClipboardJSInterop clipboardInterop, ElementReference element)
		=> await WinzorFramework.MenuDisplayerExtensions.ShowClipboardContextMenuAsync(menuDisplayer, args, clipboardInterop, element);
}
