namespace System.Windows.Forms;

public partial class Menu : Control
{
	/// <summary>
	///  Used by findMenuItem
	/// </summary>
	public const int FindHandle = 0;
	/// <summary>
	///  Used by findMenuItem
	/// </summary>
	public const int FindShortcut = 1;

	public event EventHandler<MenuItemEventArgs>? ItemAdded;
	public event EventHandler<MenuItemEventArgs>? ItemRemoved;
	public event EventHandler<MenuItemEventArgs>? ItemChanged;

	public Menu()
	{
		MenuItems = new MenuItemCollection(this);
	}

	protected Menu(MenuItem[] items) : this()
	{
		if (items != null)
		{
			MenuItems.AddRange(items);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			for (int i = 0; i < MenuItems.Count; i++)
			{
				var item = MenuItems[i];
				item.Parent = null;
				item.Dispose();
			}

			MenuItems.Clear();
		}

		base.Dispose(disposing);
	}

	/// <summary>
	///  Returns the MainMenu item that contains this menu.  The MainMenu
	///  is at the top of this menu's parent chain.
	///  Returns null if this menu is not contained in a MainMenu.
	///  This can occur if it's contained in a ContextMenu or if it isn't
	///  currently contained in any menu at all.
	/// </summary>
	public MainMenu? GetMainMenu()
	{
		var menu = this;
		while (menu is not null && menu is not MainMenu)
		{
			if (menu is MenuItem menuItem)
			{
				menu = menuItem.Parent;
			}
			else
			{
				menu = (Menu?)menu.Parent;
			}
		}
		return (MainMenu?)menu;
	}

	protected internal virtual void OnItemAdded(MenuItemEventArgs e)
	{
		if (ReferenceEquals(e.Item?.Parent, this))
		{
			OnRootMenuItemChanged();
			e.Item.ItemChanged += OnRootMenuItemChangedInternal;
		}
		ItemAdded?.Invoke(this, e);
	}

	protected internal virtual void OnItemRemoved(MenuItemEventArgs e)
	{
		if (ReferenceEquals(e.Item?.Parent, this))
		{
			OnRootMenuItemChanged();
			e.Item.ItemChanged -= OnRootMenuItemChangedInternal;
		}
		ItemRemoved?.Invoke(this, e);
	}

	protected internal virtual void OnItemChanged(MenuItemEventArgs e)
	{
		ItemChanged?.Invoke(this, e);
	}

	void OnRootMenuItemChangedInternal(object? sender, MenuItemEventArgs e)
	{
		OnRootMenuItemChanged();
	}

	protected internal virtual void OnRootMenuItemChanged()
	{
	}

	/// <summary>
	///  Returns the ContextMenu that contains this menu.  The ContextMenu
	///  is at the top of this menu's parent chain.
	///  Returns null if this menu is not contained in a ContextMenu.
	///  This can occur if it's contained in a MainMenu or if it isn't
	///  currently contained in any menu at all.
	/// </summary>
	public ContextMenu? GetContextMenu()
	{
		Menu? menuT;
		for (menuT = this; !(menuT is ContextMenu);)
		{
			if (!(menuT is MenuItem))
			{
				return null;
			}

			menuT = ((MenuItem)menuT).Parent;
		}
		return (ContextMenu?)menuT;
	}

	public virtual bool IsParent => MenuItems.Any();

	public MenuItemCollection MenuItems { get; }

	/// <summary>
	///  Sets this menu to be an identical copy of another menu.
	/// </summary>
	protected internal void CloneMenu(Menu menuSrc)
	{
		if (menuSrc == null)
		{
			throw new ArgumentNullException(nameof(menuSrc));
		}

		int count = menuSrc.MenuItems.Count;
		var newItems = new MenuItem[count];
		for (int i = 0; i < count; i++)
		{
			newItems[i] = menuSrc.MenuItems[i].CloneMenu();
		}
		MenuItems.Clear();
		if (newItems != null)
		{
			MenuItems.AddRange(newItems);
		}
	}

	protected internal new bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		MenuItem? item = FindMenuItem(FindShortcut, (int)keyData);
		return item != null && item.ShortcutClick();
	}

	protected internal override bool ProcessMnemonic(char charCode)
	{
		for (var i = 0; i < MenuItems.Count; i++)
		{
			if (MenuItems[i].ProcessMnemonic(charCode))
			{
				return true;
			}
		}

		return base.ProcessMnemonic(charCode);
	}

	public MenuItem? FindMenuItem(int type, IntPtr value)
	{
		for (int i = 0; i < MenuItems.Count; i++)
		{
			MenuItem? item = MenuItems[i];
			switch (type)
			{
				case FindHandle:
					break;
				case FindShortcut:
					if (item.Shortcut == (Shortcut)(int)value)
					{
						return item;
					}

					break;
			}
			item = item.FindMenuItem(type, value);
			if (item != null)
			{
				return item;
			}
		}
		return null;
	}

	public new string Name = string.Empty;
}
