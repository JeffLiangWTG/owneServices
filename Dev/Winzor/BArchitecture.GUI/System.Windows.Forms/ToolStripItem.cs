using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms.Layout;
using WinzorFramework;
using WinzorFramework.Extensions;

namespace System.Windows.Forms;

public abstract partial class ToolStripItem : Control, IWinzorMenuItem, IArrangedElement, IRendererControlledElement
{
	protected ToolStripItem()
	{
		Size = DefaultSize;
		CommonProperties.SetAutoSize(this, true);
		AutoToolTip = DefaultAutoToolTip;
	}

	protected ToolStripItem(string? text, Image? image, EventHandler? onClick) : this(text, image, onClick, null)
	{
	}

	protected ToolStripItem(string? text, Image? image, EventHandler? onClick, string? name) : this()
	{
		Text = text;
		Image = image;
		if (onClick != null)
		{
			Click += onClick;
		}
		Name = name;
	}

	public virtual Image? Image
	{
		get => image;
		set
		{
			if (UpdateProperty(ref image, value))
			{
				InvalidateItemLayout(PropertyNames.Image);
			}
		}
	}
	Image? image;

	public int ImageIndex { get; set; }

	protected virtual bool DefaultAutoToolTip => false;

	protected internal virtual void OnParentEnabledChanged(EventArgs e)
	{
		OnEnabledChanged(EventArgs.Empty);
	}

	internal protected virtual void OnOwnerFontChanged(EventArgs e)
	{
		if (HasFontSet)
		{
			OnFontChanged(e);
		}
	}
	public bool AutoToolTip { get; set; }

	ContentAlignment imageAlign = ContentAlignment.MiddleCenter;
	public ContentAlignment ImageAlign
	{
		get => imageAlign;
		set => UpdateProperty(ref imageAlign, value);
	}

	public Color ImageTransparentColor { get; set; }

	public ToolStripItemImageScaling ImageScaling { get; set; } = ToolStripItemImageScaling.SizeToFit;

	public ToolStripItemAlignment Alignment { get; set; } = ToolStripItemAlignment.Left;

	ContentAlignment textAlign = ContentAlignment.MiddleCenter;
	public virtual ContentAlignment TextAlign
	{
		get => textAlign;
		set => UpdateProperty(ref textAlign, value);
	}

	public virtual ToolStripTextDirection TextDirection { get; set; } = ToolStripTextDirection.Horizontal;

	public virtual ToolStripItemDisplayStyle DisplayStyle
	{
		get => displayStyle;
		set
		{
			if (UpdateProperty(ref displayStyle, value))
			{
				InvalidateItemLayout(PropertyNames.DisplayStyle);
			}
		}
	}
	ToolStripItemDisplayStyle displayStyle = ToolStripItemDisplayStyle.ImageAndText;

	public override string? ToolTipText
	{
		get
		{
			if (AutoToolTip && string.IsNullOrEmpty(toolTipText))
			{
				var toolText = Text;
				if (WindowsFormsUtils.ContainsMnemonic(toolText))
				{
					toolText = toolText.WithoutMnemonic();
				}

				return toolText;
			}

			return toolTipText;
		}
		set => toolTipText = value;
	}
	string? toolTipText;

	public virtual bool Pressed
	{
		get => pressed;
		private set => pressed = value;
	}
	bool pressed;

	public bool IsOnOverflow => false;

	public ToolStrip? Owner { get; set; }

	public ToolStrip? GetCurrentParent() => ParentInternal;

	public ToolStripItem? OwnerItem
	{
		get
		{
			if (ParentInternal is not null)
			{
				return (ParentInternal as ToolStripDropDown)?.OwnerItem;
			}

			// parent may be null, but we may be "owned" by a collection.
			return (Owner as ToolStripDropDown)?.OwnerItem;
		}
	}

	protected virtual void OnParentChanged(ToolStrip? oldParent, ToolStrip? newParent)
	{
	}

	public virtual void ShowShortcutString()
	{
	}

	public Rectangle ContentRectangle { get; }

	public virtual bool Selected { get; private set; }

	public override bool Visible
	{
		get
		{
			if (ParentInternal is not null && Available)
			{
				if (ParentInternal.Visible)
				{
					return true;
				}
				if (ParentInternal is ToolStripDropDown toolStripDropDown && toolStripDropDown.OwnerItem is not null && toolStripDropDown.OwnerItem != this)
				{
					return toolStripDropDown.OwnerItem.Visible;
				}
			}

			return false;
		}
		set => SetVisibleCore(value);
	}
	public virtual bool Checked
	{
		get => _checked;
		set => UpdateProperty(ref _checked, value);
	}
	bool _checked;

	public bool Clickable => Enabled && HandlesClick;

	public bool Available
	{
		get => visible;
		set => SetVisibleCore(value);
	}

	bool visible = true;
	protected override void SetVisibleCore(bool vis)
	{
		if (visible != vis)
		{
			if (UpdateProperty(ref visible, vis))
			{
				Unselect();
				Push(false);
				using (new LayoutTransaction(Parent, this, PropertyNames.Visible))
				{
					OnAvailableChanged(EventArgs.Empty);
					OnVisibleChanged(EventArgs.Empty);
				}
			}
		}
	}

	bool IArrangedElement.ParticipatesInLayout => visible;

	internal void Unselect()
	{
		if (Selected)
		{
			Selected = false;
			if (Available)
			{
				Invalidate();
				if (ParentInternal is not null)
				{
					ParentInternal.NotifySelectionChange(this);
				}
			}
		}
	}

	public override void Select()
	{
		if (!CanSelect)
		{
			return;
		}

		if (!Selected)
		{
			Selected = true;
			if (Available)
			{
				Invalidate();
				ParentInternal?.NotifySelectionChange(this);

				if (IsOnDropDown && OwnerItem != null && OwnerItem.IsOnDropDown)
				{
					OwnerItem.Select();
				}
			}
		}
	}

	internal void Push(bool push)
	{
		if (!CanSelect || !Enabled || DesignMode)
		{
			return;
		}

		if (UpdateProperty(ref pressed, push))
		{
			if (Available)
			{
				Invalidate();
			}
		}
	}

	protected virtual void OnAvailableChanged(EventArgs e) => AvailableChanged?.Invoke(this, e);

	public event EventHandler? AvailableChanged;

	public virtual void PerformClick()
	{
		if (Enabled && Available)
		{
			OnClick(new EventArgs());
		}
	}

	protected override void OnClick(EventArgs e)
	{
		Owner?.OnItemClicked(this);
		base.OnClick(e);
	}

	public string StylingClasses(bool horizontalPadding = false)
	{
		static string GetSpacingClasses(Padding padding, string initial, bool horizontal)
		{
			if (padding.All >= 0)
			{
				return $"{initial}-{padding.All}";
			}

			var result = $"{initial}t-{padding.Top} {initial}b-{padding.Bottom}";

			if (horizontal)
			{
				result += $" {initial}r-{padding.Right} {initial}l-{padding.Left}";
			}

			return result;
		}

		var paddingClasses = GetSpacingClasses(Padding, "p", horizontalPadding);
		return paddingClasses;
	}

	public string StylingClassesForButton()
	{
		static string GetSpacingClasses(Padding padding, string initial)
		{
			if (padding.All >= 0)
			{
				return $"{initial}-{padding.All}";
			}

			var result = $"{initial}t-{padding.Top} {initial}b-{padding.Bottom}";

			result += $" {initial}r-{padding.Right} {initial}l-{padding.Left}";

			return result;
		}

		var paddingClasses = GetSpacingClasses(new Padding(0, 0, 1, 0), "p");
		return paddingClasses;
	}

	public virtual new Padding Padding
	{
		get => CommonProperties.GetPadding(this, DefaultPadding);
		set
		{
			if (Padding != value)
			{
				CommonProperties.SetPadding(this, value);
				InvalidateItemLayout(PropertyNames.Padding);
			}
		}
	}
	protected internal override bool ProcessMnemonic(char charCode)
	{
		if (Enabled && IsMnemonic(charCode, Text) && CanProcessMnemonic())
		{
			OnClick(EventArgs.Empty);
			return true;
		}
		return false;
	}

	protected virtual void OnImageMouseEnter() { }
	protected virtual void OnImageMouseLeave() { }

	IWinzorMenuItem[] IWinzorMenuItem.MenuItems => Array.Empty<IWinzorMenuItem>();

	Shortcut IWinzorMenuItem.Shortcut => Shortcut.None;

	string? IWinzorMenuItem.ShortcutString => null;
	bool IWinzorMenuItem.HandleAsSelectable
	{
		get { return false; }
		set { }
	}

	void IWinzorMenuItem.OnSelect()
	{
	}

	void IWinzorMenuItem.OnImageMouseEnter()
	{
		OnImageMouseEnter();
	}

	void IWinzorMenuItem.OnImageMouseLeave()
	{
		OnImageMouseLeave();
	}

	bool IWinzorMenuItem.Loadable => false;

	internal ToolStripItemInternalLayout InternalLayout => toolStripItemInternalLayout ??= new ToolStripItemInternalLayout(this);
	ToolStripItemInternalLayout? toolStripItemInternalLayout;

	protected override Padding DefaultMargin => new Padding(0, 1, 0, 2);

	internal override Size GetPreferredSizeCore(Size proposedSize)
	{
		proposedSize = LayoutUtils.ConvertZeroToUnbounded(proposedSize);
		return InternalLayout.GetPreferredSize(proposedSize - Padding.Size) + Padding.Size;
	}

	internal Size PreferredImageSize
	{
		get
		{
			if ((DisplayStyle & ToolStripItemDisplayStyle.Image) != ToolStripItemDisplayStyle.Image)
			{
				return Size.Empty;
			}

			var image = Image;
			bool usingImageList = ((Owner != null) && (Owner.ImageList != null) && (ImageIndexer.ActualIndex >= 0));

			if (ImageScaling == ToolStripItemImageScaling.SizeToFit)
			{
				var ownerToolStrip = Owner;
				if (ownerToolStrip != null && (image != null || usingImageList))
				{
					return ownerToolStrip.ImageScalingSize;
				}
			}

			var imageSize = Size.Empty;
			if (usingImageList)
			{
				imageSize = Owner?.ImageList?.ImageSize ?? Size.Empty;
			}
			else
			{
				imageSize = (image == null) ? Size.Empty : image.Size;
			}

			return imageSize;
		}
	}

	internal ToolStripItemImageIndexer ImageIndexer => imageIndexer ??= new ToolStripItemImageIndexer(this);
	ToolStripItemImageIndexer? imageIndexer;

	TextImageRelation textImageRelation = TextImageRelation.ImageBeforeText;
	public TextImageRelation TextImageRelation
	{
		get => textImageRelation;
		set => UpdateProperty(ref textImageRelation, value);
	}

	public ToolStripItemOverflow Overflow
	{
		get
		{
			return overflow;
		}
		set
		{
			if (overflow != value)
			{
				overflow = value;
				if (Owner != null)
				{
					LayoutTransaction.DoLayout(Owner, Owner, "Overflow");
				}
			}
		}
	}
	ToolStripItemOverflow overflow = ToolStripItemOverflow.AsNeeded;

	internal ToolStrip? ParentInternal
	{
		get => (Parent is ToolStrip parentToolStrip) ? parentToolStrip : null;
		set
		{
			if (Parent != value)
			{
				var oldParent = Parent is ToolStrip toolStrip ? toolStrip : null;
				AssignParent(value);
				OnParentChanged(oldParent, value);
			}
		}
	}

	public void SetParent(ToolStrip toolstrip)
	{
		ParentInternal = toolstrip;
	}

	public ToolStripItemPlacement Placement => placement;
	internal void SetPlacement(ToolStripItemPlacement value) => placement = value;
	ToolStripItemPlacement placement = ToolStripItemPlacement.None;

	internal protected virtual void SetBounds(Rectangle bounds)
	{
		SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height, BoundsSpecified.All);
		InternalLayout.PerformLayout();
	}

	internal void InvalidateItemLayout(string affectedProperty)
	{
		toolStripItemInternalLayout = null;

		if (Owner != null)
		{
			LayoutTransaction.DoLayout(Owner, this, affectedProperty);
			if (Owner is ToolStripDropDown dropDown && dropDown.Visible && dropDown.OwnerItem is ToolStripDropDownItem dropDownOwnerItem)
			{
				dropDownOwnerItem.ShowDropDown();
			}
		}
	}

	protected override void OnTextChanged(EventArgs e)
	{
		InvalidateItemLayout(PropertyNames.Text);
		base.OnTextChanged(e);
		Owner?.NotifyRenderRequired();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (Owner is not null)
			{
				Owner.Items.Remove(this);
				toolStripItemInternalLayout = null;
			}
		}

		base.Dispose(disposing);
	}

	internal void OnLayoutInternal(LayoutEventArgs e)
	{
		OnLayout(e);
	}

	internal void SetOwner(ToolStrip? newOwner)
	{
		if (Owner != newOwner)
		{
			Owner = newOwner;
			if (newOwner == null)
			{
				this.ParentInternal = null;
			}
		}
	}

	internal bool IsDropdownOpen
	{
		get => isDropdownOpen;
		set
		{
			UpdateProperty(ref isDropdownOpen, value);
		}
	}
	bool isDropdownOpen;

	/// <remarks>
	///  Usually the same as can select, but things like the control box in an MDI window are exceptions
	/// </remarks>
	internal virtual bool CanKeyboardSelect => CanSelect;

	/// <summary>
	///  This is used by ToolStrip to pass on the mouseMessages for ActiveDropDown.
	/// </summary>
	internal bool IsInDesignMode => DesignMode;

	public bool IsOnDropDown
	{
		get
		{
			if (ParentInternal is not null)
			{
				return ParentInternal.IsDropDown;
			}
			else if (Owner is not null && Owner.IsDropDown)
			{
				return true;
			}

			return false;
		}
	}

	/// <summary>
	///  Translates a point from one coordinate system to another
	/// </summary>
	internal Point TranslatePoint(Point fromPoint, ToolStripPointType fromPointType, ToolStripPointType toPointType)
	{
		ToolStrip? parent = ParentInternal;

		parent ??= (IsOnOverflow && Owner is not null) ? Owner.OverflowButton.DropDown : Owner;

		if (parent is null)
		{
			// should not throw here as it's an internal function call.
			return fromPoint;
		}

		if (fromPointType == toPointType)
		{
			return fromPoint;
		}

		Point toPoint = Point.Empty;
		Point currentToolStripItemLocation = Bounds.Location;

		// From: Screen
		// To:      ToolStrip or ToolStripItem
		if (fromPointType == ToolStripPointType.ScreenCoords)
		{
			// Convert ScreenCoords --> ToolStripCoords
			toPoint = parent.PointToClient(fromPoint);

			// Convert ToolStripCoords --> ToolStripItemCoords
			if (toPointType == ToolStripPointType.ToolStripItemCoords)
			{
				toPoint.X += currentToolStripItemLocation.X;
				toPoint.Y += currentToolStripItemLocation.Y;
			}
		}

		// From: ToolStrip or ToolStripItem
		// To:      Screen or ToolStripItem
		else
		{
			// Convert "fromPoint" ToolStripItemCoords --> ToolStripCoords
			if (fromPointType == ToolStripPointType.ToolStripItemCoords)
			{
				fromPoint.X += currentToolStripItemLocation.X;
				fromPoint.Y += currentToolStripItemLocation.Y;
			}

			// At this point, fromPoint is now in ToolStrip coordinates.

			// Convert ToolStripCoords --> ScreenCoords
			if (toPointType == ToolStripPointType.ScreenCoords)
			{
				toPoint = parent.PointToScreen(fromPoint);
			}

			// Convert ToolStripCoords --> ToolStripItemCoords
			else if (toPointType == ToolStripPointType.ToolStripItemCoords)
			{
				fromPoint.X -= currentToolStripItemLocation.X;
				fromPoint.Y -= currentToolStripItemLocation.Y;
				toPoint = fromPoint;
			}
			else
			{
				Debug.Assert((toPointType == ToolStripPointType.ToolStripCoords), "why are we here! - investigate");
				toPoint = fromPoint;
			}
		}

		return toPoint;
	}

	internal ToolStripRenderer? Renderer
	{
		get
		{
			if (Owner is not null)
			{
				return Owner.Renderer;
			}

			return ParentInternal?.Renderer;
		}
	}

	Color IRendererControlledElement.RendererControlledBackColor { get; set; }

	public Image? HoverImage { get; set; }

	public string? HoverToolTipText { get; set; }
}
