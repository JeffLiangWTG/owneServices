using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using WinzorFramework;

namespace System.Windows.Forms;

public class MenuItem : Menu, IWinzorMenuItem
{
	public MenuItem()
	{
	}

	public MenuItem(string text)
	{
		Text = text;
	}

	public MenuItem(string text, EventHandler? onClick)
	{
		Text = text;
		Click += onClick;
	}

	public MenuItem(string text, MenuItem[] items) : base(items)
	{
		Text = text;
	}

	public MenuItem(string text, EventHandler? onClick, Shortcut shortcut)
	{
		Text = text;
		Click += onClick;
		Shortcut = shortcut;
	}

	public MenuItem(MenuMerge mergeType, int mergeOrder, Shortcut shortcut, string text, EventHandler? onClick, EventHandler? onPopup, EventHandler? onSelect, MenuItem[] items) : base(items)
	{
		Text = text;
		Click += onClick;
		Popup += onPopup;
		Select += onSelect;
		Shortcut = shortcut;
	}

	public int Index
	{
		get
		{
			if (Parent != null)
			{
				for (int i = 0; i < Parent.MenuItems.Count; i++)
				{
					if (Parent.MenuItems[i] == this)
					{
						return i;
					}
				}
			}

			return -1;
		}
		set
		{
			int oldIndex = Index;
			if (oldIndex >= 0)
			{
				if (value < 0 || value >= Parent?.MenuItems.Count)
				{
					throw new ArgumentOutOfRangeException(nameof(value), string.Format(SR.InvalidArgument, nameof(Index), value));
				}

				if (value != oldIndex)
				{
					var menu = Parent;

					menu?.MenuItems.RemoveAt(oldIndex);
					menu?.MenuItems.Add(value, this);
				}
			}
		}
	}

	public new Menu? Parent { get; internal set; }

	public bool DefaultItem { get; set; }

	public Shortcut Shortcut { get; set; }

	public string? ShortcutString { get; set; }

	public bool ShowShortcut { get; set; }

	public bool HandleAsSelectable { get; set; }

	bool isChecked;
	public bool Checked
	{
		get => isChecked;
		set
		{
			if (isChecked != value)
			{
				isChecked = value;
				OnItemChanged(new MenuItemEventArgs(this));
			}
		}
	}

	public bool Clickable => Enabled && HandlesClick;

	public bool RadioCheck { get; set; }

	public MenuMerge MergeType { get; set; }
	public int MergeOrder { get; set; }

	public bool OwnerDraw { get; set; }

	public void PerformClick()
	{
		OnClick(new EventArgs());
	}

	public virtual void PerformSelect()
	{
		OnSelect(new EventArgs());
	}

	internal virtual bool ShortcutClick()
	{
		if (Parent is MenuItem parent)
		{
			if (!parent.ShortcutClick() || Parent != parent)
			{
				return false;
			}
		}

		if (MenuItems.Count > 0)
		{
			OnPopup(EventArgs.Empty);
		}
		else
		{
			OnClick(EventArgs.Empty);
		}

		return true;
	}

	protected internal override bool ProcessMnemonic(char charCode)
	{
		if (CanProcessMnemonic() && IsMnemonic(charCode, Text))
		{
			if (MenuItems.Count > 0)
			{
				OnPopup(EventArgs.Empty);
				ShowDropDown();
			}
			else
			{
				OnClick(EventArgs.Empty);
			}
			return true;
		}

		return base.ProcessMnemonic(charCode);
	}

	internal override bool CanProcessMnemonic()
	{
		if (MenuItems.Count == 0)
		{
			return GetMainMenu()?.GetForm()?.CurrentKeyEvent == KeyEventType.KeyDown && Enabled && Visible;
		}

		var mainMenu = GetMainMenu();
		return mainMenu?.GetForm() != null && mainMenu?.GetMenuDisplayer() != null && mainMenu?.GetForm()?.CurrentKeyEvent == KeyEventType.KeyDown && Enabled && Visible;
	}

	void ShowDropDown()
	{
		GetMainMenu()?.ShowDropDown(WinzorControlGuid);
	}

	protected virtual void OnInitMenuPopup(EventArgs e)
	{
	}

	public virtual void ShowShortcutString()
	{
	}

	protected virtual void OnPopup(EventArgs e)
	{
		Popup?.Invoke(this, e);
	}

	protected virtual void OnSelect(EventArgs e)
	{
		Select?.Invoke(this, e);
	}

	public void CallOnClick(EventArgs e)
	{
		OnClick(e);
	}

	public void CallOnPopup(EventArgs e)
	{
		OnPopup(e);
	}

	public bool HandlesPopup => Popup != null;

	public event EventHandler? Popup;

	public new event EventHandler? Select;

	[SuppressMessage("CargoWiseOne", "CW1100:DoNotUseMenuItemOrKMenuItem", Justification = "Baseline")]
	public virtual MenuItem CloneMenu()
	{
		var clone = new MenuItem();
		clone.CloneMenu(this);
		return clone;
	}

	protected void CloneMenu(MenuItem itemSrc)
	{
		base.CloneMenu(itemSrc);
		Text = itemSrc.Text;
		Popup = itemSrc.Popup;
		Select = itemSrc.Select;
		MergeType = itemSrc.MergeType;
		MergeOrder = itemSrc.MergeOrder;
		Shortcut = itemSrc.Shortcut;
		ShowShortcut = itemSrc.ShowShortcut;
		Enabled = itemSrc.Enabled;
		CopyClickEventHandler(itemSrc);
	}

	protected void InvokeOnForm(Form form, Func<Task> func)
	{
		form.InvokeRenderDispatcher(func);
	}

	IWinzorMenuItem[] IWinzorMenuItem.MenuItems => MenuItems.ToArray();

	void IWinzorMenuItem.OnSelect()
	{
		OnSelect(EventArgs.Empty);
		OnPopup(EventArgs.Empty);
	}

	bool IWinzorMenuItem.Loadable => Select is not null || (MenuItems.Count > 0 && Popup is not null);

	void IWinzorMenuItem.OnImageMouseEnter() { }

	void IWinzorMenuItem.OnImageMouseLeave() { }

	public Image? Image { get; set; }

	public override bool Enabled
	{
		get => base.Enabled;
		set
		{
			if (base.Enabled != value)
			{
				base.Enabled = value;
				OnItemChanged(new MenuItemEventArgs(this));
			}
		}
	}

	[AllowNull]
	public override string Text
	{
		get => base.Text;
		set
		{
			if (base.Text != value)
			{
				base.Text = value;
				OnItemChanged(new MenuItemEventArgs(this));
			}
		}
	}

	bool visible = true;

	public override bool Visible
	{
		get => visible;
		set
		{
			if (UpdateProperty(ref visible, value))
			{
				OnItemChanged(new MenuItemEventArgs(this));
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			foreach (var menuItem in MenuItems)
			{
				//This check fixes an issue where a menu item containing itself will cause an infinite loop.
				if (menuItem != this)
				{
					menuItem.Dispose();
				}
			}
			Parent?.MenuItems.Remove(this);
		}
		base.Dispose(disposing);
	}

	public Image? HoverImage { get; set; }

	public string? HoverToolTipText { get; set; }
}
