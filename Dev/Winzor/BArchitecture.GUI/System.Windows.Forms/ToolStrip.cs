using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms.Layout;
using static System.Windows.Forms.WindowsFormsUtils;

namespace System.Windows.Forms;

[SuppressMessage("CargoWiseOne", "CW1093:DoNotUseSystemWindowsFormsToolStripControls", Justification = "Third party code")]
public partial class ToolStrip : ScrollableControl, IArrangedElement, IRendererControlledElement
{
	public ToolStrip()
	{
		Font = new Font("Segoe UI", 9);
		SuspendLayout();
		TabStop = false;
		SetStyle(ControlStyles.Selectable, false);
		SetExtendedState(ExtendedStates.UserPreferredSizeCache, true);
		layoutEngine = new ToolStripSplitStackLayout(this);
		Dock = DefaultDock;
		AutoSize = true;
		CausesValidation = false;
		SetAutoSizeMode(AutoSizeMode.GrowAndShrink);
		ShowItemToolTips = DefaultShowItemToolTips;
		//add a weak ref link in ToolstripManager
		ToolStripManager.ToolStrips.Add(this);
		ResumeLayout(true);
	}

	internal bool IsInDesignMode
	{
		get
		{
			return DesignMode;
		}
	}

	public override bool UseParentDivForLayout => false;

	public bool AllowMerge { get; set; }

	public override AnchorStyles Anchor
	{
		get => base.Anchor;
		set
		{
			// the base calls SetDock, which causes an OnDockChanged to be called
			// which forces two layouts of the parent.
			using (new LayoutTransaction(this, this, PropertyNames.Anchor))
			{
				base.Anchor = value;
			}
		}
	}

	public virtual ToolStripDropDownDirection DefaultDropDownDirection { get; set; }
	protected override ControlCollection CreateControlsInstance() => new ReadOnlyControlCollection(this, true);

	protected internal override Task OnInitializedAsync()
	{
		OnPaintBackground(new PaintEventArgs(this));
		OnPaint(new PaintEventArgs(this));

		return Task.CompletedTask;
	}

	public ToolStripGripStyle GripStyle
	{
		get => toolStripGripStyle;
		set
		{
			if (toolStripGripStyle != value)
			{
				toolStripGripStyle = value;
				Grip.Visible = toolStripGripStyle == ToolStripGripStyle.Visible;
				LayoutTransaction.DoLayout(this, this, PropertyNames.GripStyle);
			}
		}
	}
	ToolStripGripStyle toolStripGripStyle = ToolStripGripStyle.Visible;

	public override string ToString()
	{
		StringBuilder sb = new StringBuilder(base.ToString());
		sb.Append(", Name: ");
		sb.Append(Name);
		sb.Append(", Items: ").Append(Items.Count);
		return sb.ToString();
	}

	public ToolStripLayoutStyle LayoutStyle
	{
		get
		{
			if (layoutStyle == ToolStripLayoutStyle.StackWithOverflow)
			{
				switch (Orientation)
				{
					case Orientation.Horizontal:
						return ToolStripLayoutStyle.HorizontalStackWithOverflow;
					case Orientation.Vertical:
						return ToolStripLayoutStyle.VerticalStackWithOverflow;
				}
			}
			return layoutStyle;
		}
		set
		{
			if (layoutStyle != value)
			{
				layoutStyle = value;

				switch (value)
				{
					case ToolStripLayoutStyle.Flow:
						if (!(layoutEngine is FlowLayout))
						{
							layoutEngine = FlowLayout.Instance;
						}
						// Orientation really only applies to split stack layout (which swaps based on Dock, ToolStripPanel location)
						UpdateOrientation(Orientation.Horizontal);
						break;
					case ToolStripLayoutStyle.Table:

						throw new NotImplementedException("Table Tool Strip Layout Not Implemented In Winzor");
					case ToolStripLayoutStyle.StackWithOverflow:
					case ToolStripLayoutStyle.HorizontalStackWithOverflow:
					case ToolStripLayoutStyle.VerticalStackWithOverflow:
					default:

						if (value != ToolStripLayoutStyle.StackWithOverflow)
						{
							UpdateOrientation((value == ToolStripLayoutStyle.VerticalStackWithOverflow) ? Orientation.Vertical : Orientation.Horizontal);
						}
						else
						{
							UpdateLayoutStyle(Dock);
						}
						if (!(layoutEngine is ToolStripSplitStackLayout))
						{
							layoutEngine = new ToolStripSplitStackLayout(this);
						}
						break;
				}

				using (LayoutTransaction.CreateTransactionIf(IsHandleCreated, this, this, PropertyNames.LayoutStyle))
				{
					LayoutSettings = CreateLayoutSettings(layoutStyle);
				}
			}
		}
	}
	ToolStripLayoutStyle layoutStyle = ToolStripLayoutStyle.StackWithOverflow;

	void UpdateLayoutStyle(DockStyle newDock)
	{
		if (!IsInToolStripPanel && layoutStyle != ToolStripLayoutStyle.HorizontalStackWithOverflow && layoutStyle != ToolStripLayoutStyle.VerticalStackWithOverflow)
		{
			using (new LayoutTransaction(this, this, PropertyNames.Orientation))
			{
				//
				//  We want the ToolStrip to size appropriately when the dock has switched.
				//
				if (newDock == DockStyle.Left || newDock == DockStyle.Right)
				{
					UpdateOrientation(Orientation.Vertical);
				}
				else
				{
					UpdateOrientation(Orientation.Horizontal);
				}
			}

			if (Parent != null)
			{
				LayoutTransaction.DoLayout(Parent, this, PropertyNames.Orientation);
			}
		}
	}

	public LayoutSettings? LayoutSettings { get; set; }

	protected virtual LayoutSettings? CreateLayoutSettings(ToolStripLayoutStyle layoutStyle)
	{
		switch (layoutStyle)
		{
			case ToolStripLayoutStyle.Flow:
				return new FlowLayoutSettings(this);
			case ToolStripLayoutStyle.Table:
				throw new NotImplementedException("Table Tool Strip Layout Not Implemented In Winzor");
			default:
				return null;
		}
	}

	public Orientation Orientation => orientation;
	Orientation orientation = Orientation.Horizontal;

	void UpdateOrientation(Orientation newOrientation)
	{
		if (newOrientation != orientation)
		{
			// snap our last dimensions before switching over.
			// use specifed bounds so that if something is docked or anchored we dont take the extra stretching
			// effects into account.
			Size size = CommonProperties.GetSpecifiedBounds(this).Size;
			orientation = newOrientation;
			// since the Grip affects the DisplayRectangle, we need to re-adjust the size
			SetupGrip();
		}
	}

	public virtual ToolStripTextDirection TextDirection { get; set; } = ToolStripTextDirection.Horizontal;

	public Size ImageScalingSize { get; set; } = new Size(16, 16);

	public ToolStripRenderMode RenderMode
	{
		get
		{
			if (userDefaultRenderer)
			{
				return ToolStripRenderMode.ManagerRenderMode;
			}

			if (renderer is not null && !renderer.IsAutoGenerated)
			{
				return ToolStripRenderMode.Custom;
			}

			if (currentRendererType == ToolStripManager.s_professionalRendererType)
			{
				return ToolStripRenderMode.Professional;
			}

			if (currentRendererType == ToolStripManager.s_systemRendererType)
			{
				return ToolStripRenderMode.System;
			}

			return ToolStripRenderMode.Custom;
		}
		set
		{
			if (value == ToolStripRenderMode.Custom)
			{
				throw new NotSupportedException();
			}

			if (value == ToolStripRenderMode.ManagerRenderMode)
			{
				if (!userDefaultRenderer)
				{
					userDefaultRenderer = true;
					OnRendererChanged(EventArgs.Empty);
				}
			}
			else
			{
				userDefaultRenderer = false;
				Renderer = ToolStripManager.CreateRenderer(value);
			}
		}
	}

	public ImageList? ImageList { get; set; }

	public virtual ToolStripItemCollection Items => toolStripItemCollection ??= new ToolStripItemCollection(this, true);
	ToolStripItemCollection? toolStripItemCollection;

	IList<Control> IArrangedElement.Children => Items.ToList<Control>();

	protected internal virtual void OnItemAdded(ToolStripItemEventArgs e)
	{
		DoLayoutIfHandleCreated(e);
	}

	internal void OnItemClicked(ToolStripItem item)
	{
		ItemClicked?.Invoke(this, new ToolStripItemClickedEventArgs(item));
	}

	public event ToolStripItemClickedEventHandler? ItemClicked;

	protected internal virtual ToolStripItem CreateDefaultItem(string? text, Image? image, EventHandler? onClick) => text == "-" ? new ToolStripSeparator() : new ToolStripButton(text, image, onClick);

	public override LayoutEngine LayoutEngine => layoutEngine;
	LayoutEngine layoutEngine;

	internal override Size GetPreferredSizeCore(Size proposedSize)
	{
		// We act like a container control

		// Translating 0,0 from ClientSize to actual Size tells us how much space
		// is required for the borders.
		if (proposedSize.Width == 1)
		{
			proposedSize.Width = int.MaxValue;
		}
		if (proposedSize.Height == 1)
		{
			proposedSize.Height = int.MaxValue;
		}

		Padding padding = Padding;
		Size prefSize = LayoutEngine.GetPreferredSize(this, proposedSize - padding.Size);
		Padding newPadding = Padding;

		// as a side effect of some of the layouts, we can change the padding.
		// if this happens, we need to clear the cache.
		if (padding != newPadding)
		{
			CommonProperties.xClearPreferredSizeCache(this);
		}
		return prefSize + newPadding.Size;
	}

	// returns true when entered into menu mode through this toolstrip/menustrip
	// this is only really supported for menustrip active event, but to prevent casting everywhere...
	internal virtual bool KeyboardActive { get; set; }

	public bool CanOverflow
	{
		get => canOverflow;
		set
		{
			if (canOverflow != value)
			{
				canOverflow = value;
				InvalidateLayout();
			}
		}
	}
	bool canOverflow = true;

	public new bool CausesValidation
	{
		get
		{
			// By default: CausesValidation is false for a ToolStrip
			// we want people to be able to use menus without validating
			// their controls.
			return base.CausesValidation;
		}
		set => base.CausesValidation = value;
	}

	public new event EventHandler? CausesValidationChanged;

	void InvalidateLayout()
	{
		if (IsHandleCreated)
		{
			LayoutTransaction.DoLayout(this, this, null);
		}
	}

	public ToolStripOverflowButton OverflowButton => toolStripOverflowButton ??= DefaultOverflowButton();
	ToolStripOverflowButton? toolStripOverflowButton;

	ToolStripOverflowButton DefaultOverflowButton()
	{
		var toolStripOverflowButton = new ToolStripOverflowButton(this)
		{
			Overflow = ToolStripItemOverflow.Never,
			ParentInternal = this,
			Alignment = ToolStripItemAlignment.Right
		};
		toolStripOverflowButton.Size = toolStripOverflowButton.GetPreferredSize(DisplayRectangle.Size - Padding.Size);
		return toolStripOverflowButton;
	}

	internal static Size GetPreferredSizeHorizontal(IArrangedElement container, Size proposedConstraints)
	{
		Size maxSize = Size.Empty;
		ToolStrip toolStrip = (ToolStrip)container;

		// ensure preferred size respects default size as a minimum.
		Size defaultSize = toolStrip.DefaultSize - toolStrip.Padding.Size;
		maxSize.Height = Math.Max(0, defaultSize.Height);

		bool requiresOverflow = false;
		bool foundItemParticipatingInLayout = false;

		for (int j = 0; j < toolStrip.Items.Count; j++)
		{
			ToolStripItem item = toolStrip.Items[j];

			if (((IArrangedElement)item).ParticipatesInLayout)
			{
				foundItemParticipatingInLayout = true;
				if (item.Overflow != ToolStripItemOverflow.Always)
				{
					Padding itemMargin = item.Margin;
					Size prefItemSize = GetPreferredItemSize(item);
					maxSize.Width += itemMargin.Horizontal + prefItemSize.Width;
					maxSize.Height = Math.Max(maxSize.Height, itemMargin.Vertical + prefItemSize.Height);
				}
				else
				{
					requiresOverflow = true;
				}
			}
		}

		if (toolStrip.Items.Count == 0 || (!foundItemParticipatingInLayout))
		{
			// if there are no items there, create something anyways.
			maxSize = defaultSize;
		}

		if (requiresOverflow)
		{
			// add in the width of the overflow button
			ToolStripOverflowButton overflowItem = toolStrip.OverflowButton;
			Padding overflowItemMargin = overflowItem.Margin;

			maxSize.Width += overflowItemMargin.Horizontal + overflowItem.Bounds.Width;
		}
		else
		{
			maxSize.Width += 2;  //add Padding of 2 Pixels to the right if not Overflow.
		}

		if (toolStrip.GripStyle == ToolStripGripStyle.Visible)
		{
			// add in the grip width
			Padding gripMargin = toolStrip.GripMargin;
			maxSize.Width += gripMargin.Horizontal + toolStrip.Grip.GripThickness;
		}

		maxSize = LayoutUtils.IntersectSizes(maxSize, proposedConstraints);
		return maxSize;
	}

	internal static Size GetPreferredSizeVertical(IArrangedElement container, Size proposedConstraints)
	{
		Size maxSize = Size.Empty;
		bool requiresOverflow = false;
		ToolStrip toolStrip = (ToolStrip)container;

		bool foundItemParticipatingInLayout = false;

		for (int j = 0; j < toolStrip.Items.Count; j++)
		{
			ToolStripItem item = toolStrip.Items[j];

			if (((IArrangedElement)item).ParticipatesInLayout)
			{
				foundItemParticipatingInLayout = true;
				if (item.Overflow != ToolStripItemOverflow.Always)
				{
					Size preferredSize = GetPreferredItemSize(item);
					Padding itemMargin = item.Margin;
					maxSize.Height += itemMargin.Vertical + preferredSize.Height;
					maxSize.Width = Math.Max(maxSize.Width, itemMargin.Horizontal + preferredSize.Width);
				}
				else
				{
					requiresOverflow = true;
				}
			}
		}

		if (toolStrip.Items.Count == 0 || !foundItemParticipatingInLayout)
		{
			// if there are no items there, create something anyways.
			maxSize = LayoutUtils.FlipSize(toolStrip.DefaultSize);
		}

		if (requiresOverflow)
		{
			// add in the width of the overflow button
			ToolStripOverflowButton overflowItem = toolStrip.OverflowButton;
			Padding overflowItemMargin = overflowItem.Margin;
			maxSize.Height += overflowItemMargin.Vertical + overflowItem.Bounds.Height;
		}
		else
		{
			maxSize.Height += 2;  //add Padding to the bottom if not Overflow.
		}

		if (toolStrip.GripStyle == ToolStripGripStyle.Visible)
		{
			// add in the grip width
			Padding gripMargin = toolStrip.GripMargin;
			maxSize.Height += gripMargin.Vertical + toolStrip.Grip.GripThickness;
		}

		// note here the difference in vertical - we want the strings to fit perfectly so we're not going to constrain by the specified size.
		if (toolStrip.Size != maxSize)
		{
			CommonProperties.xClearPreferredSizeCache(toolStrip);
		}
		return maxSize;
	}

	static Size GetPreferredItemSize(ToolStripItem item)
	{
		return item.AutoSize ? item.GetPreferredSize(Size.Empty) : item.Size;
	}

	internal ToolStripGrip Grip => toolStripGrip ??= DefaultToolStripGrip();
	ToolStripGrip? toolStripGrip;

	ToolStripGrip DefaultToolStripGrip() => new ToolStripGrip
	{
		Overflow = ToolStripItemOverflow.Never,
		Visible = toolStripGripStyle == ToolStripGripStyle.Visible,
		AutoSize = false,
		ParentInternal = this,
		Margin = new Padding(2),
	};

	void SetupGrip()
	{
		Rectangle gripRectangle = Rectangle.Empty;
		Rectangle displayRect = DisplayRectangle;

		if (Orientation == Orientation.Horizontal)
		{
			// the display rectangle already knows about the padding and the grip rectangle width
			// so place it relative to that.
			gripRectangle.X = Math.Max(0, displayRect.X - Grip.GripThickness);
			gripRectangle.Y = Math.Max(0, displayRect.Top - Grip.Margin.Top);
			gripRectangle.Width = Grip.GripThickness;
			gripRectangle.Height = displayRect.Height;
			if (RightToLeft == RightToLeft.Yes)
			{
				gripRectangle.X = ClientRectangle.Right - gripRectangle.Width - Grip.Margin.Horizontal;
				gripRectangle.X += Grip.Margin.Left;
			}
			else
			{
				gripRectangle.X -= Grip.Margin.Right;
			}
		}
		else
		{
			// vertical split stack mode
			gripRectangle.X = displayRect.Left;
			gripRectangle.Y = displayRect.Top - (Grip.GripThickness + Grip.Margin.Bottom);
			gripRectangle.Width = displayRect.Width;
			gripRectangle.Height = Grip.GripThickness;
		}

		if (Grip.Bounds != gripRectangle)
		{
			Grip.SetBounds(gripRectangle);
		}
	}

	public ToolStripGripDisplayStyle GripDisplayStyle
	{
		get
		{
			return (LayoutStyle == ToolStripLayoutStyle.HorizontalStackWithOverflow) ? ToolStripGripDisplayStyle.Vertical
																 : ToolStripGripDisplayStyle.Horizontal;
		}
	}

	public Padding GripMargin
	{
		get => Grip.Margin;
		set => Grip.Margin = value;
	}

	/// <summary>
	///  The boundaries of the grip on the ToolStrip.  If it is invisible - returns Rectangle.Empty.
	/// </summary>
	public Rectangle GripRectangle
	{
		get
		{
			return (GripStyle == ToolStripGripStyle.Visible) ? Grip.Bounds : Rectangle.Empty;
		}
	}

	internal bool IsInToolStripPanel => false;

	internal bool LayoutRequired { get; set; }

	internal ToolStripItem? GetSelectedItem()
	{
		for (var i = 0; i < DisplayedItems.Count; i++)
		{
			if (DisplayedItems[i].Selected)
			{
				return DisplayedItems[i];
			}
		}
		return null;
	}

	protected override bool IsInputKey(Keys keyData)
	{
		var item = GetSelectedItem();
		if (item != null)
		{
			return true;
		}
		return base.IsInputKey(keyData);
	}

	protected override bool IsInputChar(char charCode)
	{
		var item = GetSelectedItem();
		if (item != null)
		{
			return true;
		}
		return base.IsInputChar(charCode);
	}

	protected override void OnEnabledChanged(EventArgs e)
	{
		base.OnEnabledChanged(e);

		// notify items that the parent has changed
		for (var i = 0; i < Items.Count; i++)
		{
			if (Items[i] != null && Items[i].ParentInternal == this)
			{
				Items[i].OnParentEnabledChanged(e);
			}
		}
	}

	public event EventHandler? EventLayoutStyleChanged;

	protected virtual void OnLayoutStyleChanged(EventArgs e)
	{
		EventLayoutStyleChanged?.Invoke(this, e);
	}

	protected override void OnFontChanged(EventArgs e)
	{
		base.OnFontChanged(e);
		for (var i = 0; i < Items.Count; i++)
		{
			Items[i].OnOwnerFontChanged(e);
		}
	}

	protected override void OnLayout(LayoutEventArgs e)
	{
		LayoutRequired = false;

		// we need to do this to prevent autosizing to happen while we're reparenting.
		ToolStripOverflow? overflow = GetOverflow();
		if (overflow != null)
		{
			overflow.SuspendLayout();
			toolStripOverflowButton!.Size = toolStripOverflowButton.GetPreferredSize(DisplayRectangle.Size - Padding.Size);
		}

		for (int j = 0; j < Items.Count; j++)
		{
			Items[j].OnLayoutInternal(e);
		}

		base.OnLayout(e);
		SetDisplayedItems();
		OnLayoutCompleted(EventArgs.Empty);
		Invalidate();

		if (overflow != null)
		{
			overflow.ResumeLayout();
		}
	}

	protected virtual void OnLayoutCompleted(EventArgs e)
	{
		LayoutCompleted?.Invoke(this, e);
	}
	public EventHandler? LayoutCompleted;

	protected virtual DockStyle DefaultDock => DockStyle.Top;

	internal ToolStripOverflow? GetOverflow() => toolStripOverflowButton is null || !toolStripOverflowButton.HasDropDown ? null : toolStripOverflowButton.DropDown as ToolStripOverflow;

	protected virtual void SetDisplayedItems()
	{
		DisplayedItems.Clear();
		OverflowItems.Clear();
		HasVisibleItems = false;

		Size biggestItemSize = Size.Empty; // used in determining OnPaint caching.

		if (LayoutEngine is ToolStripSplitStackLayout)
		{
			if (ToolStripGripStyle.Visible == GripStyle)
			{
				DisplayedItems.Add(Grip);
				SetupGrip();
			}

			// For splitstack layout we re-arrange the items in the displayed items
			// collection so that we can easily tab through them in natural order
			var displayRect = DisplayRectangle;
			int lastRightAlignedItem = -1;

			for (int pass = 0; pass < 2; pass++)
			{
				int j = 0;

				if (pass == 1 /*add right aligned items*/)
				{
					j = lastRightAlignedItem;
				}

				// add items to the DisplayedItem collection.
				// in pass 0, we go forward adding the head (left) aligned items
				// in pass 1, we go backward starting from the last (right) aligned item we found
				for (; j >= 0 && j < Items.Count; j = (pass == 0) ? j + 1 : j - 1)
				{
					ToolStripItem item = Items[j];
					ToolStripItemPlacement placement = item.Placement;
					if (((IArrangedElement)item).ParticipatesInLayout)
					{
						if (placement == ToolStripItemPlacement.Main)
						{
							bool addItem = false;
							if (pass == 0)
							{ // Align.Left items
								addItem = (item.Alignment == ToolStripItemAlignment.Left);
								if (!addItem)
								{
									// stash away this index so we don't have to iterate through the whole list again.
									lastRightAlignedItem = j;
								}
							}
							else if (pass == 1)
							{
								// Align.Right items
								addItem = (item.Alignment == ToolStripItemAlignment.Right);
							}

							if (addItem)
							{
								HasVisibleItems = true;
								biggestItemSize = LayoutUtils.UnionSizes(biggestItemSize, item.Bounds.Size);
								DisplayedItems.Add(item);
							}
						}
						else if (placement == ToolStripItemPlacement.Overflow && !(item is ToolStripSeparator))
						{
							OverflowItems.Add(item);
						}
					}
					else
					{
						item.SetPlacement(ToolStripItemPlacement.None);
					}
				}
			}

			ToolStripOverflow? overflow = GetOverflow();
			if (overflow is not null)
			{
				overflow.LayoutRequired = true;
			}

			if (OverflowItems.Count == 0)
			{
				OverflowButton.Visible = false;
			}
			else if (CanOverflow)
			{
				DisplayedItems.Add(OverflowButton);
			}
		}
		else
		{
			// NOT a SplitStack layout.  We don't change the order of the displayed items collection
			// for custom keyboard handling override GetNextItem.
			Rectangle clientBounds = ClientRectangle;

			// for all other layout managers, we ignore overflow placement
			for (int j = 0; j < Items.Count; j++)
			{
				ToolStripItem item = Items[j];
				if (((IArrangedElement)item).ParticipatesInLayout)
				{
					item.ParentInternal = this;

					bool boundsCheck = !IsDropDown;
					bool intersects = item.Bounds.IntersectsWith(clientBounds);

					bool verticallyContained = clientBounds.Contains(clientBounds.X, item.Bounds.Top) &&
											clientBounds.Contains(clientBounds.X, item.Bounds.Bottom);
					if (!verticallyContained)
					{
						//allContained = false;
					}

					if (!boundsCheck || intersects)
					{
						HasVisibleItems = true;
						biggestItemSize = LayoutUtils.UnionSizes(biggestItemSize, item.Bounds.Size);
						DisplayedItems.Add(item);
						item.SetPlacement(ToolStripItemPlacement.Main);
					}
				}
				else
				{
					item.SetPlacement(ToolStripItemPlacement.None);
				}
			}
		}
	}

	protected internal virtual ToolStripItemCollection DisplayedItems => displayedItems ??= new ToolStripItemCollection(this, false);
	ToolStripItemCollection? displayedItems;

	internal ToolStripItemCollection OverflowItems => overflowItems ??= new ToolStripItemCollection(this, false);
	ToolStripItemCollection? overflowItems;

	internal bool HasVisibleItems
	{
		get
		{
			if (!IsHandleCreated)
			{
				foreach (ToolStripItem item in Items)
				{
					if (((IArrangedElement)item).ParticipatesInLayout)
					{
						return hasVisibleItems = true;
					}
				}
				return hasVisibleItems = false;
			}
			return hasVisibleItems;
		}
		set
		{
			hasVisibleItems = value;
		}
	}
	bool hasVisibleItems;

	public bool IsDropDown => this is ToolStripDropDown;

	public override Rectangle DisplayRectangle
	{
		get
		{
			Rectangle rect = base.DisplayRectangle;

			if ((LayoutEngine is ToolStripSplitStackLayout) && (GripStyle == ToolStripGripStyle.Visible))
			{
				if (Orientation == Orientation.Horizontal)
				{
					int gripwidth = Grip.GripThickness + Grip.Margin.Horizontal;
					rect.Width -= gripwidth;
					// in RTL.No we need to shift the rectangle
					rect.X += (RightToLeft == RightToLeft.No) ? gripwidth : 0;
				}
				else
				{ // Vertical Grip placement
					int gripheight = Grip.GripThickness + Grip.Margin.Vertical;
					rect.Y += gripheight;
					rect.Height -= gripheight;
				}
			}
			return rect;
		}
	}

	internal void NotifySelectionChange(ToolStripItem item)
	{
		if (item is null)
		{
			ClearAllSelections();
		}
		else if (item.Selected)
		{
			ClearAllSelectionsExcept(item);
		}
	}

	void ClearAllSelections()
	{
		ClearAllSelectionsExcept(null);
	}

	void ClearAllSelectionsExcept(ToolStripItem? item)
	{
		Rectangle regionRect = (item is null) ? Rectangle.Empty : item.Bounds;

		for (int i = 0; i < DisplayedItems.Count; i++)
		{
			if (DisplayedItems[i] == item)
			{
				continue;
			}
			else if (item is not null && DisplayedItems[i].Pressed)
			{
				if (DisplayedItems[i] is ToolStripDropDownItem dropDownItem && dropDownItem.HasDropDownItems)
				{
					dropDownItem.AutoHide(item);
				}
			}

			bool invalidate = false;
			if (DisplayedItems[i].Selected)
			{
				DisplayedItems[i].Unselect();
				invalidate = true;
			}

			if (invalidate)
			{
				NotifyRenderRequired();
			}
		}
	}

	internal void DoLayoutIfHandleCreated(ToolStripItemEventArgs e)
	{
		if (IsHandleCreated)
		{
			LayoutTransaction.DoLayout(this, e.Item, PropertyNames.Items);
			Invalidate();
			// Adding this item may have added it to the overflow
			// However, we can't check if it's in OverflowItems, because
			// it gets added there in Layout, and layout might be suspended.
			if (CanOverflow && OverflowButton.HasDropDown)
			{
				if (DeferOverflowDropDownLayout())
				{
					CommonProperties.xClearPreferredSizeCache(OverflowButton.DropDown);
					OverflowButton.DropDown.LayoutRequired = true;
				}
				else
				{
					LayoutTransaction.DoLayout(OverflowButton.DropDown, e.Item, PropertyNames.Items);
					OverflowButton.DropDown.Invalidate();
				}
			}
		}
		else
		{
			// next time we fetch the preferred size, recalc it.
			CommonProperties.xClearPreferredSizeCache(this);
			LayoutRequired = true;
			if (CanOverflow && OverflowButton.HasDropDown)
			{
				OverflowButton.DropDown.LayoutRequired = true;
			}
		}
	}

	bool DeferOverflowDropDownLayout()
	{
		return IsLayoutSuspended
				|| !OverflowButton.DropDown.Visible
				|| !OverflowButton.DropDown.IsHandleCreated;
	}

	public bool Stretch { get; set; }

	protected override Size DefaultSize => new Size(100, 25);

	// Internal so that it's not a public API.
	internal virtual void ChangeSelection(ToolStripItem? nextItem)
	{
		if (nextItem is not null)
		{
			ToolStripControlHost? controlHost = nextItem as ToolStripControlHost;
			// if we contain focus, we should set focus to ourselves
			// so we get the focus off the thing that's currently focused
			// e.g. go from a text box to a toolstrip button
			if (ContainsFocus && !Focused)
			{
				Focus();
			}

			if (controlHost is not null)
			{
				controlHost.Control.Select();
				controlHost.Control.Focus();
			}

			nextItem.Select();

			if (nextItem is ToolStripMenuItem tsNextItem && !IsDropDown)
			{
				// only toplevel menus auto expand when the selection changes.
				tsNextItem.HandleAutoExpansion();
			}
		}
	}

	protected override void Select(bool directed, bool forward)
	{
		bool correctParentActiveControl = true;
		if (Parent is not null)
		{
			IContainerControl? c = Parent.GetContainerControl();

			if (c is not null)
			{
				c.ActiveControl = this;
				correctParentActiveControl = (c.ActiveControl == this);
			}
		}

		if (directed && correctParentActiveControl)
		{
			SelectNextToolStripItem(null, forward);
		}
	}

	internal ToolStripItem? SelectNextToolStripItem(ToolStripItem? start, bool forward)
	{
		ToolStripItem? nextItem = GetNextItem(start, (forward) ? ArrowDirection.Right : ArrowDirection.Left, /*RTLAware=*/true);
		ChangeSelection(nextItem);
		return nextItem;
	}

	internal virtual ToolStripItem? GetNextItem(ToolStripItem? start, ArrowDirection direction, bool rtlAware)
	{
		if (rtlAware && RightToLeft == RightToLeft.Yes)
		{
			if (direction == ArrowDirection.Right)
			{
				direction = ArrowDirection.Left;
			}
			else if (direction == ArrowDirection.Left)
			{
				direction = ArrowDirection.Right;
			}
		}

		return GetNextItem(start, direction);
	}

	/// <summary>
	///  Gets the next item from the given start item in the direction specified.
	///  - This function wraps if at the end
	///  - This function will only surf the items in the current container
	///  - Overriding this function will change the tab ordering and accessible child ordering.
	/// </summary>
	public virtual ToolStripItem? GetNextItem(ToolStripItem? start, ArrowDirection direction)
	{
		switch (direction)
		{
			case ArrowDirection.Right:
				return GetNextItemHorizontal(start, forward: true);
			case ArrowDirection.Left:
				bool forward = LastKeyData == Keys.Tab || TabStop;
				return GetNextItemHorizontal(start, forward);
			case ArrowDirection.Down:
				return GetNextItemVertical(start, down: true);
			case ArrowDirection.Up:
				return GetNextItemVertical(start, down: false);
			default:
				throw new InvalidEnumArgumentException(nameof(direction), (int)direction, typeof(ArrowDirection));
		}
	}

	/// <remarks>
	///  Helper function for GetNextItem - do not directly call this.
	/// </remarks>
	ToolStripItem? GetNextItemHorizontal(ToolStripItem? start, bool forward)
	{
		if (DisplayedItems.Count <= 0)
		{
			return null;
		}

		ToolStripDropDown? dropDown = this as ToolStripDropDown;

		if (start is null)
		{
			// The navigation should be consistent when navigating in forward and
			// backward direction entering the toolstrip, it means that the first
			// toolstrip item should be selected irrespectively TAB or SHIFT+TAB
			// is pressed.
			start = GetStartItem(forward, dropDown is not null);
		}

		int current = DisplayedItems.IndexOf(start);
		if (current == -1)
		{
			return null;
		}

		int count = DisplayedItems.Count;

		do
		{
			if (forward)
			{
				current = ++current % count;
			}
			else
			{  // provide negative wrap if necessary
				current = (--current < 0) ? count + current : current;
			}

			if (dropDown?.OwnerItem is not null && dropDown.OwnerItem.IsInDesignMode)
			{
				return DisplayedItems[current];
			}

			if (DisplayedItems[current].CanKeyboardSelect)
			{
				return DisplayedItems[current];
			}
		}
		while (DisplayedItems[current] != start);

		return null;
	}

	ToolStripItem GetStartItem(bool forward, bool isDropDown)
	{
		if (forward)
		{
			return DisplayedItems[DisplayedItems.Count - 1];
		}

		if (!isDropDown)
		{
			// For the drop-down up-directed loop should be preserved.
			// So if the current item is topmost, then the bottom item should be selected on up-key press.
			return DisplayedItems[DisplayedItems.Count > 1 ? 1 : 0];
		}

		return DisplayedItems[0];
	}

	/// <remarks>
	///  Helper function for GetNextItem - do not directly call this.
	/// </remarks>
	ToolStripItem? GetNextItemVertical(ToolStripItem? selectedItem, bool down)
	{
		ToolStripItem? tanWinner = null;
		ToolStripItem? hypotenuseWinner = null;

		double minHypotenuse = double.MaxValue;
		double minTan = double.MaxValue;
		double hypotenuseOfTanWinner = double.MaxValue;
		double tanOfHypotenuseWinner = double.MaxValue;

		if (selectedItem is null)
		{
			return GetNextItemHorizontal(selectedItem, down);
		}

		if (this is ToolStripDropDown dropDown && dropDown.OwnerItem is not null && (dropDown.OwnerItem.IsInDesignMode || (dropDown.OwnerItem.Owner is not null && dropDown.OwnerItem.Owner.IsInDesignMode)))
		{
			return GetNextItemHorizontal(selectedItem, down);
		}

		Point midPointOfCurrent = new Point(selectedItem.Bounds.X + selectedItem.Width / 2,
												selectedItem.Bounds.Y + selectedItem.Height / 2);

		for (int i = 0; i < DisplayedItems.Count; i++)
		{
			ToolStripItem otherItem = DisplayedItems[i];
			if (otherItem == selectedItem || !otherItem.CanKeyboardSelect)
			{
				continue;
			}

			if (!down && otherItem.Bounds.Bottom > selectedItem.Bounds.Top)
			{
				// if we are going up the other control has to be above
				continue;
			}
			else if (down && otherItem.Bounds.Top < selectedItem.Bounds.Bottom)
			{
				// if we are going down the other control has to be below
				continue;
			}

			//[ otherControl ]
			//       *
			Point otherItemMidLocation = new Point(otherItem.Bounds.X + otherItem.Width / 2, (down) ? otherItem.Bounds.Top : otherItem.Bounds.Bottom);
			int oppositeSide = otherItemMidLocation.X - midPointOfCurrent.X;
			int adjacentSide = otherItemMidLocation.Y - midPointOfCurrent.Y;

			// use pythagorean theorem to calculate the length of the distance
			// between the middle of the current control in question and it's adjacent
			// objects.
			double hypotenuse = Math.Sqrt(adjacentSide * adjacentSide + oppositeSide * oppositeSide);

			if (adjacentSide != 0)
			{ // avoid divide by zero - we don't do layered controls
			  //    _[o]
			  //    |/
			  //   [s]
			  //   get the angle between s and o by taking the arctan.
			  //   PERF consider using approximation instead
				double tan = Math.Abs(Math.Atan(oppositeSide / adjacentSide));

				// we want the thing with the smallest angle and smallest distance between midpoints
				minTan = Math.Min(minTan, tan);
				minHypotenuse = Math.Min(minHypotenuse, hypotenuse);

				if (minTan == tan && !double.IsNaN(minTan))
				{
					tanWinner = otherItem;
					hypotenuseOfTanWinner = hypotenuse;
				}

				if (minHypotenuse == hypotenuse)
				{
					hypotenuseWinner = otherItem;
					tanOfHypotenuseWinner = tan;
				}
			}
		}

		if ((tanWinner is null) || (hypotenuseWinner is null))
		{
			return (GetNextItemHorizontal(null, down));
		}

		// often times the guy with the best angle will be the guy with the closest hypotenuse.
		// however in layouts where things are more randomly spaced, this is not necessarily the case.
		if (tanOfHypotenuseWinner == minTan)
		{
			// if the angles match up, such as in the case of items of the same width in vertical flow
			// then pick the closest one.
			return hypotenuseWinner;
		}

		if ((!down && tanWinner.Bounds.Bottom <= hypotenuseWinner.Bounds.Top)
		  || (down && tanWinner.Bounds.Top > hypotenuseWinner.Bounds.Bottom))
		{
			// we prefer the case where the angle is smaller than
			// the case where the hypotenuse is smaller.  The only
			// scenarios where that is not the case is when the hypotenuse
			// winner is clearly closer than the angle winner.

			//   [a.winner]                       |       [s]
			//                                    |         [h.winner]
			//       [h.winner]                   |
			//     [s]                            |    [a.winner]
			return hypotenuseWinner;
		}

		return tanWinner;
	}

	protected virtual bool DefaultShowItemToolTips => true;

	public bool ShowItemToolTips { get; set; }

	bool userDefaultRenderer = true;
	Type currentRendererType = typeof(Type);
	ToolStripRenderer? renderer;

	/// <summary>
	///  The renderer is used to paint the hwndless ToolStrip items.  If someone wanted to
	///  change the "Hot" look of all of their buttons to be a green triangle, they should
	///  create a class that derives from ToolStripRenderer, assign it to this property and call
	///  invalidate.
	/// </summary>
	[AllowNull]
	public ToolStripRenderer Renderer
	{
		get
		{
			if (IsDropDown)
			{
				// PERF: since this is called a lot we don't want to make it virtual
				var dropDown = (ToolStripDropDown)this;
				if (dropDown is ToolStripOverflow || dropDown.IsAutoGenerated)
				{
					if (dropDown.OwnerToolStrip is not null)
					{
						return dropDown.OwnerToolStrip.Renderer;
					}
				}
			}

			if (RenderMode == ToolStripRenderMode.ManagerRenderMode)
			{
				return ToolStripManager.Renderer;
			}

			// always return a valid renderer so our paint code
			// doesn't have to be bogged down by checks for null.

			userDefaultRenderer = false;
			if (renderer is null)
			{
				renderer = ToolStripManager.CreateRenderer(RenderMode);
			}

			return renderer;
		}
		set
		{
			// if the value happens to be null, the next get
			// will autogenerate a new ToolStripRenderer.
			if (renderer != value)
			{
				userDefaultRenderer = (value is null);
				renderer = value;
				currentRendererType = (renderer is not null) ? renderer.GetType() : typeof(Type);
				OnRendererChanged(EventArgs.Empty);
			}
		}
	}

	public event EventHandler? RendererChanged;

	void InitializeRenderer(ToolStripRenderer renderer)
	{
		// wrap this in a LayoutTransaction so that if they change sizes
		// in this method we've suspended layout.
		using (LayoutTransaction.CreateTransactionIf(AutoSize, this, this, PropertyNames.Renderer))
		{
			renderer.Initialize(this);
			for (int i = 0; i < Items.Count; i++)
			{
				renderer.InitializeItem(Items[i]);
			}
		}

		Invalidate(Controls.Count > 0);
	}

	protected virtual void OnRendererChanged(EventArgs e)
	{
		InitializeRenderer(Renderer);
		RendererChanged?.Invoke(this, e);
	}

	internal void InvalidateTextItems()
	{
		using (new LayoutTransaction(this, this, "ShowKeyboardFocusCues", /*PerformLayout=*/Visible))
		{
			for (int j = 0; j < DisplayedItems.Count; j++)
			{
				if (((DisplayedItems[j].DisplayStyle & ToolStripItemDisplayStyle.Text) == ToolStripItemDisplayStyle.Text))
				{
					DisplayedItems[j].InvalidateItemLayout("ShowKeyboardFocusCues");
				}
			}
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);

		foreach (var item in DisplayedItems)
		{
			item.OnBeforeRender();
		}

		BGraphics toolstripGraphics = e.Graphics;
		Renderer.DrawToolStripBorder(new ToolStripRenderEventArgs(toolstripGraphics, this));
	}

	protected override void OnPaintBackground(PaintEventArgs e)
	{
		Renderer.DrawToolStripBackground(new ToolStripRenderEventArgs(e.Graphics, this));
	}

	Color IRendererControlledElement.RendererControlledBackColor { get; set; }

	[DefaultValue(false)]
	public new bool TabStop
	{
		get => base.TabStop;
		set => base.TabStop = value;
	}

	internal virtual ToolStrip? GetToplevelOwnerToolStrip()
	{
		return this;
	}

	protected internal override bool ProcessMnemonic(char charCode)
	{
		// menus and toolbars only take focus on ALT
		if (!CanProcessMnemonic())
		{
			return false;
		}
		if (Focused || ContainsFocus || ModifierKeys == Keys.Alt)
		{
			return ProcessMnemonicInternal(charCode);
		}

		// do not call base, as we dont want to walk through the controls collection and reprocess everything
		// we should have processed in the displayed items collection.
		return false;
	}

	bool ProcessMnemonicInternal(char charCode)
	{
		if (!CanProcessMnemonic())
		{  // Checking again for security...
			return false;
		}
		// at this point we assume we can process mnemonics as process mnemonic has filtered for use.
		var startingItem = GetSelectedItem();
		var startIndex = 0;
		if (startingItem != null)
		{
			startIndex = DisplayedItems.IndexOf(startingItem);
		}
		startIndex = Math.Max(0, startIndex);

		ToolStripItem? firstMatch = null;
		var foundMenuItem = false;
		var index = startIndex;

		// PASS1, iterate through the real mnemonics
		for (var i = 0; i < DisplayedItems.Count; i++)
		{
			var currentItem = DisplayedItems[index];

			index = (index + 1) % DisplayedItems.Count;
			if (string.IsNullOrEmpty(currentItem.Text) || !currentItem.Enabled)
			{
				continue;
			}

			// Only items which display text should be processed
			if ((currentItem.DisplayStyle & ToolStripItemDisplayStyle.Text) != ToolStripItemDisplayStyle.Text)
			{
				continue;
			}

			// keep track whether we've found a menu item - we'll have to do a 
			// second pass for fake mnemonics in that case.
			foundMenuItem = (foundMenuItem || (currentItem is ToolStripMenuItem));

			// In WinForms, the mnemonic key event is dispatched to the handle of the drop down directly.
			// In Winzor, it comes via the form and propagates through the controls hierarchy. We need to
			// check if the item has an open drop down menu so we can let it handle the mnemonic key.
			if (currentItem is ToolStripDropDownItem currentParentItem
				&& currentParentItem.DropDown.Visible
				&& currentParentItem.DropDown.ProcessMnemonic(charCode))
			{
				// The open dropdown was able to handle the mnemonic key
				return true;
			}
			else if (IsMnemonic(charCode, currentItem.Text))
			{
				if (firstMatch == null)
				{
					firstMatch = currentItem;
				}
				else
				{
					// we've found a second match - we should only change selection. 
					if (firstMatch == startingItem)
					{
						// change the selection to be the second match as the first is already selected
						ProcessDuplicateMnemonic(currentItem, charCode);
					}
					else
					{
						ProcessDuplicateMnemonic(firstMatch, charCode);
					}
					// we've found two mnemonics, just return.
					return true;
				}
			}
		}
		// We've found a singular match.
		if (firstMatch != null)
		{
			return firstMatch.ProcessMnemonic(charCode);
		}

		if (!foundMenuItem)
		{
			return false;
		}

		index = startIndex;

		// Key presses should change selection if mnemonic not present
		// If we haven't found a mnemonic, cycle through the menu items and check if we match.

		// Iterate through the pseudo mnemonics
		for (var i = 0; i < DisplayedItems.Count; i++)
		{
			var currentItem = DisplayedItems[index];
			index = (index + 1) % DisplayedItems.Count;

			// Menu items only
			if (!(currentItem is ToolStripMenuItem) || string.IsNullOrEmpty(currentItem.Text) || !currentItem.Enabled)
			{
				continue;
			}
			// Only items which display text should be processed
			if ((currentItem.DisplayStyle & ToolStripItemDisplayStyle.Text) != ToolStripItemDisplayStyle.Text)
			{
				continue;
			}

			if (ToolStrip.IsPseudoMnemonic(charCode, currentItem.Text))
			{
				if (firstMatch == null)
				{
					firstMatch = currentItem;
				}
				else
				{
					// we've found a second match - we should only change selection. 
					if (firstMatch == startingItem)
					{
						// change the selection to be the second match as the first is already selected
						ProcessDuplicateMnemonic(currentItem, charCode);
					}
					else
					{
						ProcessDuplicateMnemonic(firstMatch, charCode);
					}
					// we've found two mnemonics, just return.
					return true;
				}
			}
		}

		if (firstMatch != null)
		{
			return firstMatch.ProcessMnemonic(charCode);
		}

		// do not call base, as we dont want to walk through the controls collection and reprocess everything
		// we should have processed in the displayed items collection.
		return false;
	}

	internal virtual void ProcessDuplicateMnemonic(ToolStripItem item, char charCode)
	{
		if (!CanProcessMnemonic())
		{
			// Checking again for security...
			return;
		}

		if (item != null)
		{
			item.Select();
		}
	}

	static bool IsPseudoMnemonic(char charCode, string text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			if (!WindowsFormsUtils.ContainsMnemonic(text))
			{
				char charToCompare = Char.ToUpper(charCode, CultureInfo.CurrentCulture);
				char firstLetter = Char.ToUpper(text[0], CultureInfo.CurrentCulture);
				if (firstLetter == charToCompare || (Char.ToLower(charCode, CultureInfo.CurrentCulture) == Char.ToLower(text[0], CultureInfo.CurrentCulture)))
				{
					return true;
				}
			}
		}
		return false;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			foreach (var item in Items)
			{
				item.Dispose();
			}
			Items.Clear();
			toolStripOverflowButton?.Dispose();
		}
		base.Dispose(disposing);
	}
}
