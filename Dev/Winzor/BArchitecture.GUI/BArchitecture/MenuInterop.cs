using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Blazor.Client.Integration.Menus;
using WinzorFramework.Extensions;
using WinzorFramework.Telemetry;
using DropDownDirection = CargoWise.Blazor.Client.Integration.Menus.DropDownDirection;

namespace WinzorFramework;

public class MenuInterop
{
	public MenuInterop(Control control, MenuType menuType, Guid? menuId)
	{
		this.control = control;
		this.menuType = menuType;
		this.menuId = menuId ?? control.WinzorControlGuid;
	}

	public async Task ShowAsync(IWinzorMenuItem[] menuItems, Point position, ToolStripDropDownDirection dropDownDirection = ToolStripDropDownDirection.BelowRight)
	{
		flatItems.Clear();
		var interopModel = CreateInteropModel(menuItems, position, dropDownDirection);
		if (MenuDisplayer is not null)
		{
			await (MenuDisplayer?.SendShowMenuRequestAsync(interopModel, OnLoadSubMenuAsync, OnMenuClosedAsync) ?? Task.FromResult(MenuShowResultCode.ClientApplicationUnavailable));
		}
	}

	public async Task UpdateAsync(IWinzorMenuItem[] menuItems, Point position, ToolStripDropDownDirection dropDownDirection = ToolStripDropDownDirection.BelowRight)
	{
		flatItems.Clear();
		var interopModel = CreateInteropModel(menuItems, position, dropDownDirection, isUpdate: true);
		if (MenuDisplayer is not null)
		{
			await (MenuDisplayer?.SendShowMenuRequestAsync(interopModel, OnLoadSubMenuAsync, OnMenuClosedAsync) ?? Task.FromResult(MenuShowResultCode.ClientApplicationUnavailable));
		}
	}

	public async Task CloseAsync()
	{
		if (MenuDisplayer is not null)
		{
			await (MenuDisplayer?.SendCloseMenuRequestAsync() ?? Task.FromResult(MenuShowResultCode.ClientApplicationUnavailable));
		}
	}

	internal MenuInteropModel CreateInteropModel(IWinzorMenuItem[] menuItems, Point position, ToolStripDropDownDirection dropDownDirection, bool isUpdate = false)
	{
		var items = GetMenuItems(menuItems).ToArray();
		var menu = new MenuInteropModel(menuId, position.X, position.Y, (DropDownDirection)dropDownDirection, menuType, items, showMnemonicUnderlines: control.FindForm()?.showMnemonicKeys ?? false, isUpdate);
		return menu;
	}

	IMenuDisplayer? MenuDisplayer => control.FindForm()?.CargoWiseClientServices?.MenuDisplayer;

	MenuItemInteropModel[] GetMenuItems(IEnumerable<IWinzorMenuItem> menuItems, Guid? parent = null)
	{
		var items = new List<MenuItemInteropModel>(menuItems.Count());

		foreach (var menuItem in menuItems.Where(m => m is not null && m.Visible))
		{
			menuItem.ShowShortcutString();
			flatItems[menuItem.WinzorControlGuid] = menuItem;
			var item = new MenuItemInteropModel
			{
				ParentId = parent,
				Id = menuItem.WinzorControlGuid,
				Text = menuItem.Text,
				SubMenuItems = menuItem.MenuItems.Length > 0 ? GetMenuItems(menuItem.MenuItems, menuItem.WinzorControlGuid) : null,
				Loadable = menuItem.Loadable && !menuItem.HandleAsSelectable,
				Enabled = menuItem.Enabled,
				IsSeparator = menuItem is ToolStripSeparator || menuItem.Text == "-",
				IsChecked = menuItem.Checked,
				FontStyle = (CargoWise.Blazor.Client.Integration.Messaging.FontStyle)menuItem.Font.Style,
				ToolTipText = menuItem.ToolTipText,
				Image = menuItem.Image?.ToBase64(),
				Clickable = menuItem.Clickable,
			};

			if (menuItem.Image is not null &&
				(menuItem.HoverImage is not null || menuItem.HoverToolTipText is not null))
			{
				item.ImageHoverState = new MenuItemHoverInteropModel
				{
					Image = menuItem.HoverImage?.ToBase64(),
					ToolTipText = menuItem.HoverToolTipText,
				};
			}

			if (!string.IsNullOrWhiteSpace(menuItem.ShortcutString))
			{
				item.ShortcutString = menuItem.ShortcutString;
			}
			else if (menuItem.Shortcut != Shortcut.None)
			{
				item.ShortcutString = TypeDescriptor.GetConverter(typeof(Keys)).ConvertToString((Keys)(int)menuItem.Shortcut);
			}

			items.Add(item);
		}

		return items.ToArray();
	}

	public async Task<MenuItemInteropModel[]> OnLoadSubMenuAsync(SubMenuLoadRequest request)
	{
		using var activity = TelemetryService.ActivitySource.StartActivity($"{nameof(MenuInterop)}.{nameof(OnLoadSubMenuAsync)}");

		var menuItems = Array.Empty<MenuItemInteropModel>();
		if (!flatItems.TryGetValue(request.ItemId, out var menuItem) || menuItem is null || !menuItem.Enabled)
		{
			return menuItems;
		}

		await control.InvokeWinzorDispatcherAsync(() =>
		{
			menuItem.OnSelect();
			menuItems = GetMenuItems(menuItem.MenuItems.Where(m => m.Visible).ToArray(), menuItem.WinzorControlGuid);
		});

		return menuItems;
	}

	public async Task OnMenuClosedAsync(MenuClosedResult result)
	{
		await control.InvokeWinzorDispatcherAsync(() =>
		{
			if (result.ItemId is not null && flatItems.TryGetValue(result.ItemId.Value, out var menuItem) && menuItem is not null && menuItem.Enabled)
			{
				if (result.MenuClosedResultCode == MenuClosedResultCode.SelectionMade)
				{
					menuItem.PerformClick();
				}
				else if (result.MenuClosedResultCode == MenuClosedResultCode.ImageSelectionMade)
				{
					menuItem.OnImageMouseEnter();
					menuItem.PerformClick();
					menuItem.OnImageMouseLeave();
				}
			}

			if (menuType == MenuType.ContextMenu)
			{
				Closed?.Invoke(this, EventArgs.Empty);
			}
		});
	}

	public event EventHandler? Closed;

	readonly Control control;
	readonly MenuType menuType;
	readonly Guid menuId;

	readonly Dictionary<Guid, IWinzorMenuItem> flatItems = new Dictionary<Guid, IWinzorMenuItem>();
}
