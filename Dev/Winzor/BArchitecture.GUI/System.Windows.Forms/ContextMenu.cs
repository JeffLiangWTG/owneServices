using System.Drawing;
using CargoWise.Blazor.Client.Integration.Menus;
using WinzorFramework;

namespace System.Windows.Forms;

public class ContextMenu : Menu
{
	public Control? SourceControl => sourceControl;

	public event EventHandler? Popup;
	public event EventHandler? Collapse;

	internal Control? sourceControl;
	Point position;
#if DEBUG
	internal
#endif
	MenuInterop? menuInterop;

	public ContextMenu()
	{
	}

	public ContextMenu(MenuItem[] menuItems) : base(menuItems)
	{
	}

	public void Show(Control control, Point position, bool isScreenPoint = true)
	{
		control.HasActiveContextMenu = true;

		sourceControl = control;
		this.position = isScreenPoint ? position : control.PointToScreen(position);

		menuInterop = null;

		InvokePopup(new EventArgs());

		menuInterop = new MenuInterop(control, MenuType.ContextMenu, WinzorControlGuid);
		menuInterop.Closed += OnMenuClosed;
		control.InvokeRenderDispatcher(async () => {
			if (menuInterop != null)
			{
				await menuInterop.ShowAsync(MenuItems.ToArray(), this.position);
			}
		});
	}

	protected internal override void OnRootMenuItemChanged()
	{
		// Render the context menu again if a root menu item has changed.
		// Changes to items within SubMenus are otherwise retrieved on every DropDownOpening event.
		if (menuInterop is null)
		{
			base.OnRootMenuItemChanged();
			return;
		}

		if(!isPopup)
		{
			sourceControl?.InvokeRenderDispatcher(() =>
			{
				if (menuInterop is not null)
				{
					return menuInterop.UpdateAsync(MenuItems.ToArray(), position);
				}
				return Task.CompletedTask;
			});
		}

		base.OnRootMenuItemChanged();
	}

	bool isPopup;
	void InvokePopup(EventArgs e)
	{
		try
		{
			isPopup = true;
			Popup?.Invoke(this, e);
		}
		finally
		{
			isPopup = false;
		}
	}

	void OnMenuClosed(object? sender, EventArgs e)
	{
		OnCollapse(EventArgs.Empty);
		if (menuInterop is not null)
		{
			menuInterop.Closed -= OnMenuClosed;
			menuInterop = null;
		}

		if (sourceControl != null)
		{
			sourceControl.HasActiveContextMenu = false;
		}
	}

	public void Show(Control control, Point pos, LeftRightAlignment alignment) => Show(control, pos);

	protected virtual void OnPopup(EventArgs e)
	{
		InvokePopup(e);
	}

	protected virtual void OnCollapse(EventArgs e)
	{
		Collapse?.Invoke(this, e);
	}

	protected internal virtual bool ProcessCmdKey(ref Message msg, Keys keyData, Control control)
	{
		sourceControl = control;
		return ProcessCmdKey(ref msg, keyData);
	}
}
