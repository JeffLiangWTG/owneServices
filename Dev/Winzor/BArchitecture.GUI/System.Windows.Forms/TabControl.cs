using System.Drawing;
using System.Windows.Forms.Layout;
using Microsoft.AspNetCore.Components;
using WinzorFramework;
using WinzorFramework.Extensions;
using WinzorFramework.Telemetry;

namespace System.Windows.Forms;

public partial class TabControl : Control
{
	/// <summary>
	///  Constructs a TabBase object, usually as the base class for a TabStrip or TabControl.
	/// </summary>
	public TabControl()
	{
		tabCollection = new TabPageCollection(this);
	}

	int tabControlNavigationHeight = 20;

	/// <summary>
	///  Deriving classes can override this to configure a default size for their control.
	///  This is more efficient than setting the size in the control's constructor.
	/// </summary>
	protected override Size DefaultSize => new Size(200, 100);

	public override bool UseParentDivForLayout => false;

	int selectedIndex = -1;

	protected override Control.ControlCollection CreateControlsInstance() => new ControlCollection(this);

	public TabPageCollection TabPages => tabCollection;

	readonly TabPageCollection tabCollection;

	protected virtual string TabControlAlignment { get; } = string.Empty;

	protected virtual string TabControlButtonStyleString { get; } = string.Empty;

	protected virtual string TabControlNavigationStyleString { get; } = string.Empty;

	protected virtual string TabControlTabPageStyleString { get; } = string.Empty;

	public override Color BackColor
	{
		get
		{
			return SystemColors.Control;
		}
	}

	void RemoveTabPage(int index)
	{
		if (index < 0 || index >= tabPages.Count)
		{
			throw new ArgumentOutOfRangeException(nameof(index), index, string.Format(SR.InvalidArgument, nameof(index), index));
		}

		tabPages.RemoveAt(index);

		if (index < SelectedIndex)
		{
			// The page we removed was before our current selection, so we need to update selected index to reflect the change
			selectedIndex--;
		}

		cachedDisplayRect = Rectangle.Empty;
	}

	void AddTabPage(TabPage tabPage)
	{
		tabPages.Add(tabPage);
		ApplyItemSize();
	}

	/// <summary>
	///  Returns the number of tabs in the strip
	/// </summary>
	public int TabCount => tabPages.Count;

	/// <summary>
	///  Allows the user to specify the name of the tabpage in Tabcontrol.TabPageCollection to be shown.
	/// </summary>
	public void SelectTab(string tabPageName)
	{
		ArgumentNullException.ThrowIfNull(tabPageName);

		var tabPage = TabPages[tabPageName]!;
		SelectTab(tabPage);
	}

	/// <summary>
	///  Allows the user to specify the tabpage in Tabcontrol.TabPageCollection  to be shown.
	/// </summary>
	public void SelectTab(TabPage tabPage)
	{
		ArgumentNullException.ThrowIfNull(tabPage);

		var index = FindTabPage(tabPage);
		SelectTab(index);
	}

	public void SelectTab(int index)
	{
		var tabPage = GetTabPage(index);

		if (tabPage is not null)
		{
			SelectedTab = tabPage;
		}
	}

	internal TabPage GetTabPage(int index)
	{
		if (index < 0 || index >= tabPages.Count)
		{
			throw new ArgumentOutOfRangeException(nameof(index), index, string.Format(SR.InvalidArgument, nameof(index), index));
		}

		return tabPages[index];
	}

	void SetTabPage(int index, TabPage value)
	{
		if (index < 0 || index >= tabPages.Count)
		{
			throw new ArgumentOutOfRangeException(nameof(index), index, string.Format(SR.InvalidArgument, nameof(index), index));
		}

		ArgumentNullException.ThrowIfNull(value);
		tabPages[index] = value;
		SelectTab(index);
	}

	int lastSelectedIndex;
	bool selectFirstControl;

	/// <summary>
	///  The index of the currently selected tab in the strip, if there
	///  is one.  If the value is -1, there is currently no selection.  If the
	///  value is 0 or greater, than the value is the index of the currently
	///  selected tab.
	/// </summary>
	public int SelectedIndex
	{
		get => (IsHandleCreated && (selectedIndex == -1 || selectedIndex >= TabCount)) ? 0 : selectedIndex;
		set
		{
			if (value < -1)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, string.Format(SR.InvalidLowBoundArgumentEx, nameof(SelectedIndex), value, -1));
			}

			if (SelectedIndex != value)
			{
				if (IsHandleCreated)
				{
					if (!selectFirstControl)
					{
						lastSelectedIndex = SelectedIndex;
						selectedIndex = value;
						if (TabSelChanging())
						{
							return;
						}

						selectFirstControl = true;
						if (TabSelChange())
						{
							selectFirstControl = false;
							return;
						}
						else
						{
							selectFirstControl = false;
						}
					}
				}
				else
				{
					selectedIndex = value;
				}
			}
		}
	}

	/// <summary>
	///  Actually goes and fires the OnLeave event.  Inheriting controls
	///  should use this to know when the event is fired [this is preferable to
	///  adding an event handler on yourself for this event].  They should,
	///  however, remember to call base.OnLeave(e); to ensure the event is
	///  still fired to external listeners
	///  This listener is overridden so that we can fire SAME ENTER and LEAVE
	///  events on the TabPage.
	///  TabPage should fire enter when the focus is on the TABPAGE and not when the control
	///  within the TabPage gets Focused.
	///  Similarly the Leave event should fire when the TabControl (and hence the TabPage) looses
	///  Focus. To be Backward compatible we have added new bool which can be set to true
	///  to the get the NEW SANE ENTER-LEAVE EVENTS ON THE TABPAGE.
	/// </summary>
	protected override void OnEnter(EventArgs e)
	{
		base.OnEnter(e);
		if (SelectedTab != null)
		{
			SelectedTab.FireEnter(e);
		}
	}

	protected override bool WantArrowKeys => true;

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Left && SelectedIndex > 0)
		{
			selectedIndex = SelectedIndex - 1;
			this.UpdateUISelection();
			if (StartingIndexOfVisibleTabs > SelectedIndex)
			{
				StartingIndexOfVisibleTabs--;
				NotifyRenderRequired();
			}
			var tabPage = GetTabPage(selectedIndex);
			RegisterAfterRenderAction(async () =>
			{
				if (tabPage.IsElementReferenceCaptured && JSRuntime is not null)
				{
					await tabPage.ElementReference.TryFocusOnClientAsync(JSRuntime);
				}
			});
		}
		else if (e.KeyCode == Keys.Right && selectedIndex < TabPages.Count - 1)
		{
			selectedIndex = SelectedIndex + 1;
			this.UpdateUISelection();
			var lastVisibleTabIndex = StartingIndexOfVisibleTabs + CalculateNumberOfVisibleTabs();
			lastVisibleTabIndex -= LastTabPartiallyVisible ? 1 : 0;
			if (lastVisibleTabIndex <= SelectedIndex)
			{
				StartingIndexOfVisibleTabs++;
				NotifyRenderRequired();
			}
			var tabPage = GetTabPage(selectedIndex);
			RegisterAfterRenderAction(async () =>
			{
				if (tabPage.IsElementReferenceCaptured && JSRuntime is not null)
				{
					await tabPage.ElementReference.TryFocusOnClientAsync(JSRuntime);
				}
			});
		}
		base.OnKeyDown(e);
	}

	/// <summary>
	///  Actually goes and fires the OnLeave event.  Inheriting controls
	///  should use this to know when the event is fired [this is preferable to
	///  adding an event handler on yourself for this event].  They should,
	///  however, remember to call base.OnLeave(e); to ensure the event is
	///  still fired to external listeners
	///  This listener is overridden so that we can fire SAME ENTER and LEAVE
	///  events on the TabPage.
	///  TabPage should fire enter when the focus is on the TABPAGE and not when the control
	///  within the TabPage gets Focused.
	///  Similarly the Leave event  should fire when the TabControl (and hence the TabPage) looses
	///  Focus. To be Backward compatible we have added new bool which can be set to true
	///  to the get the NEW SANE ENTER-LEAVE EVENTS ON THE TABPAGE.
	/// </summary>
	protected override void OnLeave(EventArgs e)
	{
		if (SelectedTab != null)
		{
			SelectedTab.FireLeave(e);
		}
		base.OnLeave(e);
	}

	bool TabSelChange()
	{
		TabControlCancelEventArgs tcc = new TabControlCancelEventArgs(SelectedTab, SelectedIndex, false, TabControlAction.Selecting);
		OnSelecting(tcc);
		if (!tcc.Cancel)
		{
			OnSelected(new TabControlEventArgs(SelectedTab, SelectedIndex, TabControlAction.Selected));
			OnSelectedIndexChanged(EventArgs.Empty);
		}
		else
		{
			// user Cancelled the Selection of the new Tab.
			selectedIndex = lastSelectedIndex;
			UpdateTabSelection(true);
		}
		return tcc.Cancel;
	}

	bool TabSelChanging()
	{
		IContainerControl? c = GetContainerControl();
		if (c != null && !DesignMode)
		{
			if (c is ContainerControl)
			{
				((ContainerControl)c).SetActiveControl(this);
			}
			else
			{
				c.ActiveControl = this;
			}
		}
		// Fire DeSelecting .... on the current Selected Index...
		// Set the return value to a global
		// if 'cancelled' return from here else..
		// fire Deselected.
		var deSelectIndex = lastSelectedIndex < 0 || lastSelectedIndex > tabPages.Count - 1 ? 0 : lastSelectedIndex;
		TabControlCancelEventArgs tcc = new TabControlCancelEventArgs(tabPages[deSelectIndex], deSelectIndex, false, TabControlAction.Deselecting);
		OnDeselecting(tcc);
		if (!tcc.Cancel)
		{
			OnDeselected(new TabControlEventArgs(tabPages[deSelectIndex], deSelectIndex, TabControlAction.Deselected));
		}
		return tcc.Cancel;
	}

	/// <summary>
	///  This is a notification that the handle has been created.
	///  We do some work here to configure the handle.
	///  Overriders should call base.OnHandleCreated()
	/// </summary>
	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		cachedDisplayRect = Rectangle.Empty;

		ResizePages();

		if (selectedIndex != -1)
		{
			SelectedIndex = selectedIndex;
		}

		UpdateTabSelection(false);
	}

	/// <summary>
	///  The selection to the given tab, provided it .equals a tab in the
	///  list.  The return value is the index of the tab that was selected,
	///  or -1 if no tab was selected.
	/// </summary>
	public TabPage? SelectedTab
	{
		get
		{
			int index = SelectedIndex;
			var localTabPages = tabPages;

			if (index == -1 || index >= localTabPages.Count)
			{
				return null;
			}

			return localTabPages[index];
		}
		set
		{
			int index = FindTabPage(value);
			SelectedIndex = index;
		}
	}

	int FindTabPage(TabPage? tabPage)
	{
		if (tabPages is not null)
		{
			for (int i = 0; i < tabPages.Count; i++)
			{
				if (tabPages[i].Equals(tabPage))
				{
					return i;
				}
			}
		}

		return -1;
	}

	/// <summary>
	///  Actually goes and fires the onSelectedIndexChanged event.  Inheriting controls
	///  should use this to know when the event is fired [this is preferable to
	///  adding an event handler on yourself for this event].  They should,
	///  however, remember to call base.onSelectedIndexChanged(e); to ensure the event is
	///  still fired to external listeners
	/// </summary>
	protected virtual void OnSelectedIndexChanged(EventArgs e)
	{
		cachedDisplayRect = Rectangle.Empty;
		UpdateTabSelection(Focused);
		SelectedIndexChanged?.Invoke(this, e);
	}

	protected virtual void OnSelecting(TabControlCancelEventArgs e)
	{
		Selecting?.Invoke(this, e);
	}

	protected virtual void OnSelected(TabControlEventArgs e)
	{
		Selected?.Invoke(this, e);
		if (SelectedTab != null)
		{
			SelectedTab.FireEnter(EventArgs.Empty);
		}
	}

	protected virtual void OnDeselecting(TabControlCancelEventArgs e)
	{
		Deselecting?.Invoke(this, e);
	}

	protected virtual void OnDeselected(TabControlEventArgs e)
	{
		if (selectedIndex == -1)
		{
			return;
		}
		Deselected?.Invoke(this, e);
		if (SelectedTab != null)
		{
			if (isTabChanged)
			{
				SelectedTab.FireLeave(EventArgs.Empty);
			}
			else
			{
				tabPages[nextTabPageIndex]?.FireLeave(EventArgs.Empty);
			}
		}
	}

	public event EventHandler? SelectedIndexChanged;

	public event EventHandler? RightToLeftLayoutChanged;

	public event TabControlEventHandler? Selected;

	public event TabControlCancelEventHandler? Selecting;

	public event TabControlEventHandler? Deselected;

	public event TabControlCancelEventHandler? Deselecting;

	int nextTabPageIndex;

	bool isTabChanged;

	protected Task TabChangedAsync(TabPage tabPage)
	{
		return InvokeWinzorDispatcherAsync(() =>
		{
			using var activity = TelemetryService.ActivitySource.StartActivity($"{GetType().Name}.TabChanged");

			nextTabPageIndex = tabPages.IndexOf(tabPage);

			if (selectedIndex == nextTabPageIndex)
			{
				return;
			}

			isTabChanged = true;
			UpdateUISelection();

			var visibleTabs = GetVisibleTabs();
			if (visibleTabs.Count == 0)
			{
				return;
			}

			while (visibleTabs[^1] == tabPage && LastTabPartiallyVisible)
			{
				StartingIndexOfVisibleTabs++;
				NotifyRenderRequired();
				visibleTabs = GetVisibleTabs();
			}
		});
	}

	/// <summary>
	///  Retrieves the bounding rectangle for the given tab in the tab strip.
	/// </summary>
	public Rectangle GetTabRect(int index)
	{
		if (index < 0 || index >= tabPages.Count)
		{
			throw new ArgumentOutOfRangeException(nameof(index), index, string.Format(SR.InvalidArgument, nameof(index), index));
		}

		return GetTabRect(tabPages[index]);
	}

	Rectangle GetTabRect(TabPage tabPage)
	{
		if (!itemSize.IsEmpty && SizeMode == TabSizeMode.Fixed)
		{
			return new Rectangle(new Point(tabPage.RectLeft, 0), ItemSize);
		}

		return new Rectangle(tabPage.RectLeft, 0, tabPage.DynamicTabWidth, tabControlNavigationHeight);
	}

	/// <summary>
	///  Returns the imageList the control points at.  This is where tabs that have imageIndex
	///  set will get there images from.
	/// </summary>
	public ImageList? ImageList { get; set; }

	[Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in TabControl.razor")]
	string ImageStyleString => $"width:{ImageList?.ImageSize.Width}px;height:{ImageList?.ImageSize.Height}px;";

	/// <summary>
	///  Indicates whether tooltips are being shown for tabs that have tooltips set on
	///  them.
	/// </summary>
	public bool ShowToolTips { get; set; }

	/// <summary>
	///  By default, tabs will automatically size themselves to fit their icon, if any, and their label.
	///  However, the tab size can be explicitly set by setting this property.
	/// </summary>
	public Size ItemSize
	{
		get
		{
			if (itemSize.IsEmpty)
			{
				if (IsHandleCreated && TabPages.Count > 0)
				{
					return GetTabRect(0).Size;
				}
				return Size.Empty;
			}
			return itemSize;
		}
		set
		{
			if (itemSize != value)
			{
				itemSize = value;
				tabControlNavigationHeight = value.Height;
				Invalidate();
			}
		}
	}

	Size itemSize;

	/// <summary>
	///  The number of rows currently being displayed in
	///  the tab strip.  This is most commonly used when the Multline property
	///  is 'true' and you want to know how many rows the tabs are currently
	///  taking up.
	/// </summary>
	public int RowCount { get; } = 1;

	/// <summary>
	///  This is used for international applications where the language
	///  is written from RightToLeft. When this property is true,
	//      and the RightToLeft is true, mirroring will be turned on on the form, and
	///  control placement and text will be from right to left.
	/// </summary>
	public virtual bool RightToLeftLayout
	{
		get
		{
			return rightToLeftLayout;
		}

		set
		{
			if (value != rightToLeftLayout)
			{
				rightToLeftLayout = value;
				using (new LayoutTransaction(this, this, PropertyNames.RightToLeftLayout))
				{
					OnRightToLeftLayoutChanged(EventArgs.Empty);
				}
			}
		}
	}

	bool rightToLeftLayout;

	/// <summary>
	///  The amount of padding around the items in the individual tabs.
	///  You can specify both horizontal and vertical padding.
	/// </summary>
	public new Point Padding { get; set; }

	/// <summary>
	///  Indicates if there can be more than one row of tabs.  By default [when
	///  this property is false], if there are more tabs than available display
	///  space, arrows are shown to let the user navigate between the extra
	///  tabs, but only one row is shown.  If this property is set to true, then
	///  Windows spills extra tabs over on to second rows.
	/// </summary>
	public bool Multiline { get; set; }

	/// <summary>
	///  Returns on what area of the control the tabs reside on (A TabAlignment value).
	///  The possibilities are Top (the default), Bottom, Left, and Right.  When alignment
	///  is left or right, the Multiline property is ignored and Multiline is implicitly on.
	///  If the alignment is anything other than top, TabAppearance.FlatButtons degenerates
	///  to TabAppearance.Buttons.
	/// </summary>
	public TabAlignment Alignment { get; set; }

	/// <summary>
	///  Indicates whether the tabs in the tabstrip look like regular tabs, or if they look
	///  like buttons as seen in the Windows 95 taskbar.
	///  If the alignment is anything other than top, TabAppearance.FlatButtons degenerates
	///  to TabAppearance.Buttons.
	/// </summary>
	public TabAppearance Appearance { get; set; }

	/// <summary>
	///  By default, tabs are big enough to display their text, and any space
	///  on the right of the strip is left as such.  However, you can also
	///  set it such that the tabs are stretched to fill out the right extent
	///  of the strip, if necessary, or you can set it such that all tabs
	///  the same width.
	/// </summary>
	public TabSizeMode SizeMode { get; set; }

	/// <summary>
	///  The drawing mode of the tabs in the tab strip.  This will indicate
	/// </summary>
	public TabDrawMode DrawMode { get; set; }

	/// <summary>
	///  Indicates whether the tabs visually change when the mouse passes over them.
	/// </summary>
	public bool HotTrack { get; set; }

	public event DrawItemEventHandler? DrawItem;

	void UpdateUISelection()
	{
		if (TabSelChanging())
		{
			return;
		}

		if (ValidationCancelled)
		{
			return;
		}

		if (isTabChanged)
		{
			selectedIndex = nextTabPageIndex;
			isTabChanged = false;
		}

		if (TabSelChange())
		{
			return;
		}
	}

	/// <summary>
	///  Set the panel selections appropriately
	/// </summary>
	protected void UpdateTabSelection(bool updateFocus)
	{
		if (IsHandleCreated)
		{
			int index = SelectedIndex;

			if (index != -1 && index < tabPages.Count)
			{
				tabPages[index].Bounds = DisplayRectangle;

				// After changing the Bounds of TabPages, we need to
				// make TabPages Redraw.
				// Use Invalidate directly here has no performance
				// issue, since ReSize is calling low frequence.
				tabPages[index].Invalidate();

				tabPages[index].Visible = true;

				if (updateFocus)
				{
					if (!Focused || selectFirstControl)
					{
						bool selectNext = tabPages[index].SelectNextControl(null, true, true, false, false);

						if (selectNext)
						{
							if (!ContainsFocus)
							{
								IContainerControl? c = GetContainerControl();
								if (c is not null)
								{
									while (c.ActiveControl is ContainerControl)
									{
										c = (IContainerControl)c.ActiveControl;
									}

									c.ActiveControl?.Focus();
								}
							}
						}
						else
						{
							IContainerControl? c = GetContainerControl();
							if (c is not null && !DesignMode)
							{
								if (c is ContainerControl)
								{
									((ContainerControl)c).SetActiveControl(this);
								}
								else
								{
									c.ActiveControl = this;
								}
							}
						}
					}
				}
			}

			totalTabWidth = 0;
			for (int i = 0; i < tabPages.Count; i++)
			{
				tabPages[i].RectLeft = i == 0 ? 0 : tabPages[i - 1].RectLeft + tabPages[i - 1].DynamicTabWidth;
				if (i != SelectedIndex)
				{
					tabPages[i].Visible = false;
				}
				totalTabWidth += GetTabRect(i).Width;
			}
		}
	}

	void ResizePages()
	{
		var rect = DisplayRectangle;

		var pages = GetTabPages();

		for (int i = 0; i < pages.Length; i++)
		{
			pages[i].Bounds = rect;
		}
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		cachedDisplayRect = Rectangle.Empty;
		UpdateTabSelection(false);
	}

	protected virtual void OnRightToLeftLayoutChanged(EventArgs e)
	{
		if (GetAnyDisposingInHierarchy())
		{
			return;
		}

		if (RightToLeft == RightToLeft.Yes)
		{
			RecreateHandle();
		}

		RightToLeftLayoutChanged?.Invoke(this, e);
	}

	bool ShowTabSlider
	{
		get
		{
			return totalTabWidth > Size.Width;
		}
	}

	protected internal int StartingIndexOfVisibleTabs { get; set; }

	protected internal int NumberOfVisibleTabs { get; private set; }

	protected internal bool LastTabPartiallyVisible { get; private set; }

	int totalTabWidth;

	int CalculateNumberOfVisibleTabs()
	{
		var visibleTabsCount = 0;
		var totalVisibleTabsWidth = TabSlider.SLIDER_WIDTH;
		for (var i = StartingIndexOfVisibleTabs; i < tabPages.Count && totalVisibleTabsWidth < Size.Width; i++)
		{
			visibleTabsCount++;
			totalVisibleTabsWidth += GetTabRect(i).Width;
		}

		LastTabPartiallyVisible = totalVisibleTabsWidth > Size.Width;
		NumberOfVisibleTabs = visibleTabsCount;
		return visibleTabsCount;
	}

	public List<TabPage> GetVisibleTabs()
	{
		if (!ShowTabSlider)
		{
			return tabPages.GetSnapshot().ToList();
		}
		return tabPages.GetSnapshot().ToList()
			.GetRange(StartingIndexOfVisibleTabs, CalculateNumberOfVisibleTabs());
	}

	const int WinFormsDisplayRectangleInset = 4;

	/// <summary>
	///  The rectangle that represents the Area of the tab strip not
	///  taken up by the tabs, borders, or anything else owned by the Tab.  This
	///  is typically the rectangle you want to use to place the individual
	///  children of the tab strip.
	/// </summary>
	public override Rectangle DisplayRectangle
	{
		get
		{
			// Set the cached display rect to Rectangle.Empty whenever we do anything to change it.
			if (!cachedDisplayRect.IsEmpty)
			{
				return cachedDisplayRect;
			}

			var rect = Bounds;
			rect.Height -= tabControlNavigationHeight + WinFormsDisplayRectangleInset * 2 - 1;
			rect.Width -= WinFormsDisplayRectangleInset * 2;
			rect.Y = tabControlNavigationHeight + WinFormsDisplayRectangleInset;
			rect.X = WinFormsDisplayRectangleInset;

			cachedDisplayRect = rect;
			return cachedDisplayRect;
		}
	}
	Rectangle cachedDisplayRect = Rectangle.Empty;

	internal TabPage[] GetTabPages() => tabPages.ToArray();

	readonly WrappedList<TabPage> tabPages = new WrappedList<TabPage>();

	/// <summary>
	///  This private property is set by the TabPageCollection when the user calls "InsertItem".
	///  The problem is when InsertItem is called then we add this item to the ControlsCollection (in addition to the TabPageCollection)
	///  to keep both the collections is sync. But the controlCollection.Add is overridden to again ADD the item to the TabPageCollection.
	///  So we keep this flag in order to avoid repeated addition (only during insert)
	///  When the Add ends ... we reset this flag.
	/// </summary>
	bool InsertingItem { get; set; }

	internal void ApplyItemSize()
	{
		cachedDisplayRect = Rectangle.Empty;
	}

	protected void RemoveAll()
	{
		Controls.Clear();
		tabPages.Clear();
	}

	/// <summary>
	///  This function is used by the Insert Logic to insert a tabPage in the current TabPage in the TabPageCollection.
	/// </summary>
	void InsertItem(int index, TabPage tabPage)
	{
		if (index < 0 || index > tabPages.Count)
		{
			throw new ArgumentOutOfRangeException(nameof(index), index, string.Format(SR.InvalidArgument, nameof(index), index));
		}

		ArgumentNullException.ThrowIfNull(tabPage);

		tabPages.Insert(index, tabPage);
		ApplyItemSize();
	}

	public void SetMinTabWidth(int minTabWidth) => WinzorMinTabWidth = minTabWidth;

	public int WinzorMinTabWidth;

	public override ElementReference ElementReference
	{
		get => SelectedTab?.ElementReference ?? base.ElementReference;
	}

	internal void UpdateTab(TabPage tabPage)
	{
		// It's possible that changes to this TabPage will change the DisplayRectangle of the
		// TabControl, so invalidate and resize the size of this page.
		cachedDisplayRect = Rectangle.Empty;
		UpdateTabSelection(false);
	}

	/// <summary>
	///  Collection of controls...
	/// </summary>
	public new class ControlCollection : Control.ControlCollection
	{
		public ControlCollection(TabControl owner)
			: base(owner)
		{
			this.owner = owner;
		}

		public override void Add(Control value)
		{
			if (!(value is TabPage))
			{
				throw new ArgumentException(string.Format(SR.TabControlInvalidTabPageType, value.GetType().Name));
			}

			TabPage tabPage = (TabPage)value;

			// See InsertingItem property
			if (!owner.InsertingItem)
			{
				owner.AddTabPage(tabPage);
			}

			base.Add(tabPage);
			tabPage.Visible = false;

			if (owner.IsHandleCreated)
			{
				tabPage.Bounds = owner.DisplayRectangle;
			}

			owner.ApplyItemSize();
			owner.UpdateTabSelection(false);
		}

		public override void Remove(Control? value)
		{
			base.Remove(value);
			if (!(value is TabPage))
			{
				return;
			}

			int index = owner.FindTabPage((TabPage)value);
			int curSelectedIndex = owner.SelectedIndex;
			if (index != -1)
			{
				owner.RemoveTabPage(index);
				if (index == curSelectedIndex)
				{
					// Always select the first tabPage is the Selected TabPage is removed.
					owner.SelectedIndex = 0;
				}
			}

			owner.UpdateTabSelection(false);
		}

		readonly TabControl owner;
	}
}
