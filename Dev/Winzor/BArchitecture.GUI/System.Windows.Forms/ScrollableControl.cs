using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms;

public class ScrollableControl : Control, IArrangedElement
{
	public ScrollableControl() : base()
	{
		SetStyle(ControlStyles.ContainerControl, true);
	}

	VScrollProperties? verticalScroll;
	HScrollProperties? horizontalScroll;

	public DockPaddingEdges DockPadding { get; } = new DockPaddingEdges();

	public Point AutoScrollPosition { get; set; }

	public VScrollProperties VerticalScroll => verticalScroll ??= new VScrollProperties(this);

	public HScrollProperties HorizontalScroll => horizontalScroll ??= new HScrollProperties(this);

	protected internal override string ControlStyleString
	{
		get => $"{base.ControlStyleString}{ScrollStyleString}";
	}

	protected virtual internal string ScrollStyleString
	{
		get
		{
			var result = (VerticalScroll._visible, HorizontalScroll._visible) switch
			{
				(true, true) => "overflow:auto;",
				(true, false) => "overflow-y:auto;overflow-x:hidden;",
				(false, true) => "overflow-x:auto;overflow-y:hidden;",
				(false, false) => "overflow:hidden;"
			};

			return result;
		}
	}

	protected bool HScroll { get; set; }

	protected bool VScroll { get; set; }

	Size scrollMargin = Size.Empty;

	Size requestedScrollMargin = Size.Empty;

	public Size AutoScrollMinSize
	{
		get => userAutoScrollMinSize;
		set
		{
			if (UpdateProperty(ref userAutoScrollMinSize, value))
			{
				AutoScroll = true;
				PerformLayout();
			}
		}
	}
	Size userAutoScrollMinSize = Size.Empty;

	bool autoScroll;
	public virtual bool AutoScroll
	{
		get => autoScroll;
		set
		{
			if (UpdateProperty(ref autoScroll, value))
			{
				LayoutTransaction.DoLayout(this, this, PropertyNames.AutoScroll);
			}
		}
	}

	public override Rectangle DisplayRectangle
	{
		get
		{
			var rect = ClientRectangle;
			if (!displayRect.IsEmpty)
			{
				rect.X = displayRect.X;
				rect.Y = displayRect.Y;
				if (HScroll)
				{
					rect.Width = displayRect.Width;
				}

				if (VScroll)
				{
					rect.Height = displayRect.Height;
				}
			}

			return LayoutUtils.DeflateRect(rect, Padding);
		}
	}
	Rectangle displayRect = Rectangle.Empty;

	Rectangle IArrangedElement.DisplayRectangle
	{
		get
		{
			Rectangle displayRectangle = DisplayRectangle;
			// Controls anchored the bottom of their container may disappear (be scrunched)
			// when scrolling is used.
			if (AutoScrollMinSize.Width != 0 && AutoScrollMinSize.Height != 0)
			{
				displayRectangle.Width = Math.Max(displayRectangle.Width, AutoScrollMinSize.Width);
				displayRectangle.Height = Math.Max(displayRectangle.Height, AutoScrollMinSize.Height);
			}
			return displayRectangle;
		}
	}

	Rectangle GetDisplayRectInternal()
	{
		if (displayRect.IsEmpty)
		{
			displayRect = ClientRectangle;
		}

		if (!AutoScroll && HorizontalScroll._visible)
		{
			displayRect = new Rectangle(displayRect.X, displayRect.Y, HorizontalScroll.Maximum, displayRect.Height);
		}

		if (!AutoScroll && VerticalScroll._visible)
		{
			displayRect = new Rectangle(displayRect.X, displayRect.Y, displayRect.Width, VerticalScroll.Maximum);
		}

		return displayRect;
	}

	/// <summary>
	///  Forces the layout of any docked or anchored child controls.
	/// </summary>
	protected override void OnLayout(LayoutEventArgs levent)
	{
		// We get into a problem when you change the docking of a control
		// with autosizing on. Since the control (affectedControl) has
		// already had the dock property changed, adjustFormScrollbars
		// treats it as a docked control. However, since base.onLayout
		// hasn't been called yet, the bounds of the control haven't been
		// changed.
		//
		// We can't just call base.onLayout() once in this case, since
		// adjusting the scrollbars COULD adjust the display area, and
		// thus require a new layout. The result is that when you
		// affect a control's layout, we are forced to layout twice. There
		// isn't any noticeable flicker, but this could be a perf problem...
		if (levent is not null && levent.AffectedControl is not null && AutoScroll)
		{
			base.OnLayout(levent);
		}

		AdjustFormScrollbars(AutoScroll);
		base.OnLayout(levent!);
	}

	bool SetVisibleScrollbars(bool horiz, bool vert)
	{
		var needLayout = false;

		if ((!horiz && HScroll)
						|| (horiz && !HScroll)
						|| (!vert && VScroll)
						|| (vert && !VScroll))
		{
			needLayout = true;
		}

		if (needLayout)
		{
			var x = displayRect.X;
			var y = displayRect.Y;
			if (!horiz)
			{
				x = 0;
			}

			if (!vert)
			{
				y = 0;
			}

			HScroll = horiz;
			VScroll = vert;

			if (horiz)
			{
				HorizontalScroll._visible = true;
			}
			else
			{
				ResetScrollProperties(HorizontalScroll);
			}

			if (vert)
			{
				VerticalScroll._visible = true;
			}
			else
			{
				ResetScrollProperties(VerticalScroll);
			}
			Invalidate();
		}

		return needLayout;
	}

	static void ResetScrollProperties(ScrollProperties scrollProperties)
	{
		// Set only these two values as when the ScrollBars are not visible ...
		// there is no meaning of the "value" property.
		scrollProperties._visible = false;
		scrollProperties.Value = 0;
	}

	protected virtual void AdjustFormScrollbars(bool displayScrollbars)
	{
		var needLayout = false;
		var display = GetDisplayRectInternal();
		if (!displayScrollbars && (HScroll || VScroll))
		{
			needLayout = SetVisibleScrollbars(false, false);
		}

		if (!displayScrollbars)
		{
			var client = ClientRectangle;
			display.Width = client.Width;
			display.Height = client.Height;
		}
		else
		{
			needLayout |= ApplyScrollbarChanges(display);
		}

		if (needLayout)
		{
			LayoutTransaction.DoLayout(this, this, PropertyNames.DisplayRectangle);
		}
	}

	bool ApplyScrollbarChanges(Rectangle display)
	{
		var needLayout = false;
		var needHscroll = false;
		var needVscroll = false;
		var currentClient = ClientRectangle;
		var fullClient = currentClient;
		var minClient = fullClient;

		if (HScroll)
		{
			fullClient.Height += SystemInformation.HorizontalScrollBarHeight;
		}
		else
		{
			minClient.Height -= SystemInformation.HorizontalScrollBarHeight;
		}

		if (VScroll)
		{
			fullClient.Width += SystemInformation.VerticalScrollBarWidth;
		}
		else
		{
			minClient.Width -= SystemInformation.VerticalScrollBarWidth;
		}

		var maxX = minClient.Width;
		var maxY = minClient.Height;

		if (Controls.Count != 0)
		{
			scrollMargin = requestedScrollMargin;

			if (DockPadding is not null)
			{
				scrollMargin.Height += Padding.Bottom;
				scrollMargin.Width += Padding.Right;
			}

			for (int i = 0; i < Controls.Count; i++)
			{
				var current = Controls[i];
				if (current is not null && current.ControlVisible)
				{
					switch (current.Dock)
					{
						case DockStyle.Bottom:
							scrollMargin.Height += current.Size.Height;
							break;
						case DockStyle.Right:
							scrollMargin.Width += current.Size.Width;
							break;
					}
				}
			}
		}

		if (!userAutoScrollMinSize.IsEmpty)
		{
			maxX = userAutoScrollMinSize.Width + scrollMargin.Width;
			maxY = userAutoScrollMinSize.Height + scrollMargin.Height;
			needHscroll = true;
			needVscroll = true;
		}

		var defaultLayoutEngine = (LayoutEngine == DefaultLayout.Instance);
		if (!defaultLayoutEngine && CommonProperties.HasLayoutBounds(this))
		{
			var layoutBounds = CommonProperties.GetLayoutBounds(this);

			if (layoutBounds.Width > maxX)
			{
				needHscroll = true;
				maxX = layoutBounds.Width;
			}

			if (layoutBounds.Height > maxY)
			{
				needVscroll = true;
				maxY = layoutBounds.Height;
			}
		}
		else if (Controls.Count != 0)
		{
			// Compute the dimensions of the display rect
			for (var i = 0; i < Controls.Count; i++)
			{
				var watchHoriz = true;
				var watchVert = true;

				var current = Controls[i];

				// Same logic as the margin calc - you need to see if the
				// control *will* be visible...
				if (current is not null && current.Visible)
				{
					if (defaultLayoutEngine)
					{
						var richCurrent = current;

						switch (richCurrent.Dock)
						{
							case DockStyle.Top:
								watchHoriz = false;
								break;
							case DockStyle.Left:
								watchVert = false;
								break;
							case DockStyle.Bottom:
							case DockStyle.Fill:
							case DockStyle.Right:
								watchHoriz = false;
								watchVert = false;
								break;
							default:
								AnchorStyles anchor = richCurrent.Anchor;
								if ((anchor & AnchorStyles.Right) == AnchorStyles.Right)
								{
									watchHoriz = false;
								}

								if ((anchor & AnchorStyles.Left) != AnchorStyles.Left)
								{
									watchHoriz = false;
								}

								if ((anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom)
								{
									watchVert = false;
								}

								if ((anchor & AnchorStyles.Top) != AnchorStyles.Top)
								{
									watchVert = false;
								}

								break;
						}
					}

					if (watchHoriz || watchVert)
					{
						var bounds = current.Bounds;
						var ctlRight = -display.X + bounds.X + bounds.Width + scrollMargin.Width;
						var ctlBottom = -display.Y + bounds.Y + bounds.Height + scrollMargin.Height;

						if (!defaultLayoutEngine)
						{
							ctlRight += current.Margin.Right;
							ctlBottom += current.Margin.Bottom;
						}

						if (ctlRight > maxX && watchHoriz)
						{
							needHscroll = true;
							maxX = ctlRight;
						}

						if (ctlBottom > maxY && watchVert)
						{
							needVscroll = true;
							maxY = ctlBottom;
						}
					}
				}
			}
		}

		// Check maxX/maxY against the clientRect, we must compare it to the
		// clientRect without any scrollbars, and then we can check it against
		// the clientRect with the "new" scrollbars. This will make the
		// scrollbars show and hide themselves correctly at the boundaries.
		if (maxX <= fullClient.Width)
		{
			needHscroll = false;
		}

		if (maxY <= fullClient.Height)
		{
			needVscroll = false;
		}

		var clientToBe = fullClient;
		if (needHscroll)
		{
			clientToBe.Height -= SystemInformation.HorizontalScrollBarHeight;
		}

		if (needVscroll)
		{
			clientToBe.Width -= SystemInformation.VerticalScrollBarWidth;
		}

		if (needHscroll && maxY > clientToBe.Height)
		{
			needVscroll = true;
		}

		if (needVscroll && maxX > clientToBe.Width)
		{
			needHscroll = true;
		}

		if (!needHscroll)
		{
			maxX = clientToBe.Width;
		}

		if (!needVscroll)
		{
			maxY = clientToBe.Height;
		}

		needLayout = (SetVisibleScrollbars(needHscroll, needVscroll) || needLayout);

		if (HScroll || VScroll)
		{
			needLayout = (SetDisplayRectangleSize(maxX, maxY) || needLayout);
		}
		else
		{
			SetDisplayRectangleSize(maxX, maxY);
		}

		// this is to mimic UpdateBounds() called by WmWindowPosChanged eventually
		UpdateBounds(Left, Top, Width, Height);

		return needLayout;
	}

	bool SetDisplayRectangleSize(int width, int height)
	{
		var needLayout = false;
		if (displayRect.Width != width || displayRect.Height != height)
		{
			displayRect.Width = width;
			displayRect.Height = height;
			needLayout = true;
		}

		var minX = ClientRectangle.Width - width;
		var minY = ClientRectangle.Height - height;
		if (minX > 0)
		{
			minX = 0;
		}

		if (minY > 0)
		{
			minY = 0;
		}

		var x = displayRect.X;
		var y = displayRect.Y;

		if (!HScroll)
		{
			x = 0;
		}

		if (!VScroll)
		{
			y = 0;
		}

		if (x < minX)
		{
			x = minX;
		}

		if (y < minY)
		{
			y = minY;
		}

		SetDisplayRectLocation(x, y);
		return needLayout;
	}

	protected unsafe void SetDisplayRectLocation(int x, int y)
	{
		var client = ClientRectangle;
		// The DisplayRect property modifies
		// the returned rect to include padding. We don't want to
		// include this padding in our adjustment of the DisplayRect
		// because it interferes with the scrolling.
		var displayRectangle = displayRect;
		var minX = Math.Min(client.Width - displayRectangle.Width, 0);
		var minY = Math.Min(client.Height - displayRectangle.Height, 0);

		if (x > 0)
		{
			x = 0;
		}

		if (y > 0)
		{
			y = 0;
		}

		if (x < minX)
		{
			x = minX;
		}

		if (y < minY)
		{
			y = minY;
		}

		displayRect.X = x;
		displayRect.Y = y;

		for (var i = 0; i < Controls.Count; i++)
		{
			var ctl = Controls[i];
			if (ctl is not null && ctl.IsHandleCreated)
			{
				ctl.UpdateBounds();
			}
		}
	}

	internal override void AdjustWindowRectEx(ref Interop.RECT rect)
	{
		if (VScroll || VerticalScroll.Visible)
		{
			rect.right += SystemInformation.VerticalScrollBarWidth;
		}
		if (HScroll || HorizontalScroll.Visible)
		{
			rect.bottom += SystemInformation.HorizontalScrollBarHeight;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	protected override void OnVisibleChanged(EventArgs e)
	{
		if (Visible)
		{
			// When the page becomes visible, we need to call OnLayout to adjust the scrollbars.
			LayoutTransaction.DoLayout(this, this, PropertyNames.Visible);
		}

		base.OnVisibleChanged(e);
	}

	public class DockPaddingEdges
	{
		public int All { get; set; }
		public int Bottom { get; set; }
		public int Left { get; set; }
		public int Right { get; set; }
		public int Top { get; set; }
	}
}
