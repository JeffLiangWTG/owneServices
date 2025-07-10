using CargoWise.Blazor.Client.Integration.Menus;
using Microsoft.AspNetCore.Components;
using WinzorFramework.JSInterop;

namespace WinzorFramework;

public static class MenuDisplayerExtensions
{
	public static async Task ShowClipboardContextMenuAsync(this IMenuDisplayer menuDisplayer, WebMouseEventArgs args, IClipboardJSInterop? interop, ElementReference element)
	{
		var availableActions = await (interop?.GetAvailableActionsAsync(element) ?? Task.FromResult(new AvailableClipboardActions(false, false, false)));

		var menuId = Guid.NewGuid();
		var cutItem = new MenuItemInteropModel()
		{
			ParentId = menuId,
			Id = Guid.NewGuid(),
			Text = "Cut",
			ShortcutString = "Ctrl+X",
			Enabled = availableActions.AllowCut,
		};
		var copyItem = new MenuItemInteropModel()
		{
			ParentId = menuId,
			Id = Guid.NewGuid(),
			Text = "Copy",
			ShortcutString = "Ctrl+C",
			Enabled = availableActions.AllowCopy,
		};
		var pasteItem = new MenuItemInteropModel()
		{
			ParentId = menuId,
			Id = Guid.NewGuid(),
			Text = "Paste",
			ShortcutString = "Ctrl+V",
			Enabled = availableActions.AllowPaste,
		};
		var menuItems = new MenuItemInteropModel[] { cutItem, copyItem, pasteItem };

		async Task OnMenuClosedAsync(MenuClosedResult result)
		{
			if (result.MenuClosedResultCode == MenuClosedResultCode.SelectionMade)
			{
				var selectedItem = result.ItemId;
				if (selectedItem is not null)
				{
					if (selectedItem == cutItem.Id)
					{
						await (interop?.CutAsync(element) ?? Task.CompletedTask);
					}
					else if (selectedItem == copyItem.Id)
					{
						await (interop?.CopyAsync(element) ?? Task.CompletedTask);
					}
					else if (selectedItem == pasteItem.Id)
					{
						await (interop?.PasteAsync(element) ?? Task.CompletedTask);
					}
				}
			}
		}

		var menu = new MenuInteropModel(menuId, (int)args.ClientX, (int)args.ClientY, DropDownDirection.BelowLeft, MenuType.ContextMenu, menuItems);
		var result = await menuDisplayer.SendShowMenuRequestAsync(menu, (_) => Task.FromResult(Array.Empty<MenuItemInteropModel>()), OnMenuClosedAsync);
	}
}
