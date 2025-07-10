using System.ComponentModel;
using System.Drawing;
using Microsoft.AspNetCore.Components;
using WinzorFramework;
using WinzorFramework.JSInterop;

namespace System.Windows.Forms;

[Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in MonthCalendar.razor")]
public partial class SplitContainer : ContainerControl, ISupportInitialize
{
	Rectangle _splitterRect;
	public SplitContainer()
	{
		_splitterRect = new Rectangle();
		Panel1 = new SplitterPanel(this);
		Panel2 = new SplitterPanel(this);

		Controls.Add(Panel1);
		Controls.Add(Panel2);

		UpdateSplitter();
	}

	public override bool UseParentDivForLayout => false;

	// Refer to IsContainerControl property on Control for more details.
	internal override bool IsContainerControl => true;

	public Orientation Orientation
	{
		get => orientation;
		set
		{
			if (UpdateProperty(ref orientation, value))
			{
				splitDistance = 0;
				SplitterDistance = SplitterDistanceInternal;
				UpdateSplitter();
			}
		}
	}
	Orientation orientation = Orientation.Vertical;

	public int SplitterDistance
	{
		get => splitDistance;
		set
		{
			if (value != SplitterDistance)
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(SplitterDistance), string.Format(SR.InvalidLowBoundArgument, "SplitterDistance", value, 0));
				}

				try
				{
					setSplitterDistance = true;

					if (Orientation == Orientation.Vertical)
					{
						if (value < Panel1MinSize)
						{
							value = Panel1MinSize;
						}

						if (value + SplitterWidthInternal > Width - Panel2MinSize)
						{
							value = Width - Panel2MinSize - SplitterWidthInternal;
						}

						if (value < 0)
						{
							throw new InvalidOperationException(SR.SplitterDistanceNotAllowed);
						}

						splitDistance = value;
						splitterDistance = value;
						Panel1.WidthInternal = SplitterDistance;
					}
					else
					{
						if (value < Panel1MinSize)
						{
							value = Panel1MinSize;
						}

						if (value + SplitterWidthInternal > Height - Panel2MinSize)
						{
							value = Height - Panel2MinSize - SplitterWidthInternal;
						}

						if (value < 0)
						{
							throw new InvalidOperationException(SR.SplitterDistanceNotAllowed);
						}

						splitDistance = value;
						splitterDistance = value;
						Panel1.HeightInternal = SplitterDistance;
					}

					switch (fixedPanel)
					{
						case FixedPanel.Panel1:
							panelSize = SplitterDistance;
							break;
						case FixedPanel.Panel2:
							if (Orientation == Orientation.Vertical)
							{
								panelSize = Width - SplitterDistance - SplitterWidthInternal;
							}
							else
							{
								panelSize = Height - SplitterDistance - SplitterWidthInternal;
							}

							break;
					}

					UpdateSplitter();
				}
				finally
				{
					setSplitterDistance = false;
				}

				OnSplitterMoved(new SplitterEventArgs(splitterRectangle.X + splitterRectangle.Width / 2, splitterRectangle.Y + splitterRectangle.Height / 2, splitterRectangle.X, splitterRectangle.Y));
			}
		}
	}
	int splitDistance = 50;
	bool setSplitterDistance;

	int SplitterDistanceInternal
	{
		get
		{
			return splitterDistance;
		}
		set
		{
			SplitterDistance = value;
		}
	}
	int splitterDistance = 50;

	public bool IsSplitterFixed { get; set; }

	public int Panel1MinSize
	{
		get => panel1MinSize;
		set
		{
			newPanel1MinSize = value;
			if (value != Panel1MinSize && !initializing)
			{
				ApplyPanel1MinSize(value);
			}
		}
	}
	int panel1MinSize = 25;
	int newPanel1MinSize = 25;

	public int Panel2MinSize
	{
		get => panel2MinSize;
		set
		{
			newPanel2MinSize = value;
			if (value != Panel2MinSize && !initializing)
			{
				ApplyPanel2MinSize(value);
			}
		}
	}
	int panel2MinSize = 25;
	int newPanel2MinSize = 25;

	public int SplitterWidth
	{
		get => splitterWidth;
		set
		{
			newSplitterWidth = value;
			if (value != SplitterWidth && !initializing)
			{
				ApplySplitterWidth(value);
			}
		}
	}
	int splitterWidth = 4;
	int newSplitterWidth = 4;

	void ApplySplitterWidth(int value)
	{
		if (value < 1)
		{
			throw new ArgumentOutOfRangeException(nameof(value), value, string.Format(SR.InvalidLowBoundArgumentEx, nameof(SplitterWidth), value, 1));
		}

		if (Orientation == Orientation.Vertical)
		{
			if (DesignMode && value + Panel1MinSize + Panel2MinSize > Width)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, string.Format(SR.InvalidArgument, nameof(SplitterWidth), value));
			}
		}
		else if (Orientation == Orientation.Horizontal)
		{
			if (DesignMode && value + Panel1MinSize + Panel2MinSize > Height)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, string.Format(SR.InvalidArgument, nameof(SplitterWidth), value));
			}
		}

		UpdateProperty(ref splitterWidth, value);
		UpdateSplitter();
	}

	public FixedPanel FixedPanel
	{
		get => fixedPanel;
		set
		{
			if (UpdateProperty(ref fixedPanel, value))
			{
				switch (fixedPanel)
				{
					case FixedPanel.Panel2:
						if (Orientation == Orientation.Vertical)
						{
							panelSize = Width - SplitterDistanceInternal - SplitterWidthInternal;
						}
						else
						{
							panelSize = Height - SplitterDistanceInternal - SplitterWidthInternal;
						}

						break;
					default:
						panelSize = SplitterDistanceInternal;
						break;
				}
			}
		}
	}
	FixedPanel fixedPanel = FixedPanel.None;
	int panelSize;

	public SplitterPanel Panel1 { get; init; }

	public SplitterPanel Panel2 { get; init; }

	public bool Panel1Collapsed
	{
		get => Panel1.Collapsed;
		set
		{
			if (value != Panel1.Collapsed)
			{
				if (value && Panel2.Collapsed)
				{
					CollapsePanel(Panel2, false);
				}
				CollapsePanel(Panel1, value);
			}
		}
	}

	public bool Panel2Collapsed
	{
		get => Panel2.Collapsed;
		set
		{
			if (value != Panel2.Collapsed)
			{
				if (value && Panel1.Collapsed)
				{
					CollapsePanel(Panel1, false);
				}
				CollapsePanel(Panel2, value);
			}
		}
	}

	bool IsSplitterMovable
	{
		get
		{
			if (Orientation == Orientation.Vertical)
			{
				return (Width >= Panel1MinSize + SplitterWidthInternal + Panel2MinSize);
			}
			else
			{
				return (Height >= Panel1MinSize + SplitterWidthInternal + Panel2MinSize);
			}
		}
	}

	void CollapsePanel(SplitterPanel panel, bool collapsing)
	{
		panel.Collapsed = collapsing;
		panel.Visible = !collapsing;
		UpdateSplitter();
	}

	void UpdateSplitter()
	{
		Panel1.SuspendLayout();
		Panel2.SuspendLayout();
		if (Orientation == Orientation.Vertical)
		{
			bool isRTL = RightToLeft == RightToLeft.Yes;

			//NO PANEL FIXED !!
			if (!CollapsedMode)
			{
				Panel1.HeightInternal = Height;
				Panel1.WidthInternal = splitterDistance; //Default splitter distance from left or top.
				Panel2.Size = new Size(Width - splitterDistance - SplitterWidthInternal, Height);

				if (!isRTL)
				{
					Panel1.Location = new Point(0, 0);
					Panel2.Location = new Point(splitterDistance + SplitterWidthInternal, 0);
				}
				else
				{
					Panel1.Location = new Point(Width - splitterDistance, 0);
					Panel2.Location = new Point(0, 0);
				}

				SetSplitterRect(vertical: true);
				if (!resizeCalled)
				{
					ratioWidth = (Width / (double)(Panel1.Width) > 0) ? Width / (double)(Panel1.Width) : ratioWidth;
				}
			}
			else
			{
				if (Panel1Collapsed)
				{
					Panel2.Size = Size;
					Panel2.Location = new Point(0, 0);
				}
				else if (Panel2Collapsed)
				{
					Panel1.Size = Size;
					Panel1.Location = new Point(0, 0);
				}

				// Update Ratio when the splitContainer is in CollapsedMode.
				if (!resizeCalled)
				{
					ratioWidth = (Width / (double)(splitterDistance) > 0) ? Width / (double)(splitterDistance) : ratioWidth;
				}
			}
		}
		else
		{
			//NO PANEL FIXED !!
			if (!CollapsedMode)
			{
				Panel1.Location = new Point(0, 0);
				Panel1.WidthInternal = Width;

				Panel1.HeightInternal = SplitterDistanceInternal; //Default splitter distance from left or top.
				int panel2Start = splitterDistance + SplitterWidthInternal;
				Panel2.Size = new Size(Width, Height - panel2Start);
				Panel2.Location = new Point(0, panel2Start);

				SetSplitterRect(vertical: false);

				if (!resizeCalled)
				{
					ratioHeight = (Height / (double)(Panel1.Height) > 0) ? Height / (double)(Panel1.Height) : ratioHeight;
				}
			}
			else
			{
				if (Panel1Collapsed)
				{
					Panel2.Size = Size;
					Panel2.Location = new Point(0, 0);
				}
				else if (Panel2Collapsed)
				{
					Panel1.Size = Size;
					Panel1.Location = new Point(0, 0);
				}

				// Update Ratio when the splitContainer is in CollapsedMode.
				if (!resizeCalled)
				{
					ratioHeight = (Height / (double)(splitterDistance) > 0) ? Height / (double)(splitterDistance) : ratioHeight;
				}
			}
		}

		Panel1.ResumeLayout();
		Panel2.ResumeLayout();
	}

	void ResizeSplitContainer()
	{
		Panel1.SuspendLayout();
		Panel2.SuspendLayout();

		if (Width == 0)
		{
			// Set the correct Width iif the WIDTH has changed to ZERO.
			Panel1.Size = new Size(0, Panel1.Height);
			Panel2.Size = new Size(0, Panel2.Height);
		}
		else if (Height == 0)
		{
			// Set the correct Height iif the HEIGHT has changed to ZERO.
			Panel1.Size = new Size(Panel1.Width, 0);
			Panel2.Size = new Size(Panel2.Width, 0);
		}
		else
		{
			if (Orientation == Orientation.Vertical)
			{
				// If no panel is collapsed then do the default ...
				if (!CollapsedMode)
				{
					if (FixedPanel == FixedPanel.Panel1)
					{
						Panel1.Size = new Size(panelSize, Height);
						Panel2.Size = new Size(Math.Max(Width - panelSize - SplitterWidthInternal, Panel2MinSize), Height);
					}

					if (FixedPanel == FixedPanel.Panel2)
					{
						Panel2.Size = new Size(panelSize, Height);
						splitterDistance = Math.Max(Width - panelSize - SplitterWidthInternal, Panel1MinSize);
						Panel1.WidthInternal = splitterDistance;
						Panel1.HeightInternal = Height;
					}

					if (FixedPanel == FixedPanel.None)
					{
						if (ratioWidth != 0.0)
						{
							splitterDistance = Math.Max((int)(Math.Floor(Width / ratioWidth)), Panel1MinSize);
						}

						Panel1.WidthInternal = splitterDistance; //Default splitter distance from left or top.
						Panel1.HeightInternal = Height;
						Panel2.Size = new Size(Math.Max(Width - splitterDistance - SplitterWidthInternal, Panel2MinSize), Height);
					}

					if (RightToLeft == RightToLeft.No)
					{
						Panel2.Location = new Point(Panel1.Width + SplitterWidthInternal, 0);
					}
					else
					{
						Panel1.Location = new Point(Width - Panel1.Width, 0);
					}

					SetSplitterRect(true);
				}
				else
				{
					if (Panel1Collapsed)
					{
						Panel2.Size = Size;
						Panel2.Location = new Point(0, 0);
					}
					else if (Panel2Collapsed)
					{
						Panel1.Size = Size;
						Panel1.Location = new Point(0, 0);
					}
				}
			}
			else if (Orientation == Orientation.Horizontal)
			{
				// If no panel is collapsed then do the default ...
				if (!CollapsedMode)
				{
					if (FixedPanel == FixedPanel.Panel1)
					{
						//Default splitter distance from left or top.
						Panel1.Size = new Size(Width, panelSize);
						int panel2Start = panelSize + SplitterWidthInternal;
						Panel2.Size = new Size(Width, Math.Max(Height - panel2Start, Panel2MinSize));
						Panel2.Location = new Point(0, panel2Start);
					}

					if (FixedPanel == FixedPanel.Panel2)
					{
						Panel2.Size = new Size(Width, panelSize);
						splitterDistance = Math.Max(Height - Panel2.Height - SplitterWidthInternal, Panel1MinSize);
						Panel1.HeightInternal = splitterDistance;
						Panel1.WidthInternal = Width;
						int panel2Start = splitterDistance + SplitterWidthInternal;
						Panel2.Location = new Point(0, panel2Start);
					}

					if (FixedPanel == FixedPanel.None)
					{
						//NO PANEL FIXED !!
						if (ratioHeight != 0.0)
						{
							splitterDistance = Math.Max((int)(Math.Floor(Height / ratioHeight)), Panel1MinSize);
						}

						Panel1.HeightInternal = splitterDistance; //Default splitter distance from left or top.
						Panel1.WidthInternal = Width;
						int panel2Start = splitterDistance + SplitterWidthInternal;
						Panel2.Size = new Size(Width, Math.Max(Height - panel2Start, Panel2MinSize));
						Panel2.Location = new Point(0, panel2Start);
					}

					SetSplitterRect(false);
				}
				else
				{
					if (Panel1Collapsed)
					{
						Panel2.Size = Size;
						Panel2.Location = new Point(0, 0);
					}
					else if (Panel2Collapsed)
					{
						Panel1.Size = Size;
						Panel1.Location = new Point(0, 0);
					}
				}
			}

			try
			{
				resizeCalled = true;
				ApplySplitterDistance();
			}
			finally
			{
				resizeCalled = false;
			}
		}

		Panel1.ResumeLayout();
		Panel2.ResumeLayout();
	}

	void ApplySplitterDistance()
	{
		using (new Layout.LayoutTransaction(this, this, "SplitterDistance", false))
		{
			SplitterDistanceInternal = splitterDistance;
		}

		if (Orientation == Orientation.Vertical)
		{
			if (RightToLeft == RightToLeft.No)
			{
				splitterRectangle.X = Location.X + SplitterDistanceInternal;
			}
			else
			{
				splitterRectangle.X = Right - SplitterDistanceInternal - SplitterWidthInternal;
			}
		}
		else
		{
			splitterRectangle.Y = Location.Y + SplitterDistanceInternal;
		}
		NotifyRenderRequired();
	}

	bool CollapsedMode => Panel1Collapsed || Panel2Collapsed;

	/// <summary>
	///  We need to have a internal Property for the SplitterWidth which returns zero if we are in collapsed mode.
	///  This property is used to Layout SplitContainer.
	/// </summary>
	int SplitterWidthInternal => CollapsedMode ? 0 : splitterWidth;

	Rectangle splitterRectangle;

	void SetSplitterRect(bool vertical)
	{
		if (vertical)
		{
			splitterRectangle = new Rectangle(
				(RightToLeft == RightToLeft.Yes) ? Width - SplitterDistance - SplitterWidth : Location.X + SplitterDistance,
				Location.Y,
				SplitterWidth,
				Height);
		}
		else
		{
			splitterRectangle = new Rectangle(
				Location.X,
				Location.Y + SplitterDistance,
				Width,
				SplitterWidth);
		}
		NotifyRenderRequired();
	}

	public void OnSplitterMoved(SplitterEventArgs e)
	{
		SplitterMoved?.Invoke(this, e);
	}
	public void OnSplitterMoving(SplitterCancelEventArgs e)
	{
		SplitterMoving?.Invoke(this, e);
	}
	public event SplitterEventHandler? SplitterMoved;
	public event SplitterCancelEventHandler? SplitterMoving;

	protected override Size DefaultSize => new Size(150, 100);

	protected override void OnLayout(LayoutEventArgs e)
	{
		SetInnerMostBorder(this);

		if (IsSplitterMovable && !setSplitterDistance)
		{
			ResizeSplitContainer();
		}

		base.OnLayout(e);
	}

	public Rectangle SplitterRectangle
	{
		get
		{
			Rectangle r = _splitterRect;
			r.X = _splitterRect.X - Left;
			r.Y = _splitterRect.Y - Top;
			return r;
		}
	}

	// FixedPanel.None requires us to keep the Width/Height Ratio Depending on SplitContainer.Orientation
	double ratioWidth;
	double ratioHeight;
	bool resizeCalled;

	public new DockStyle Dock
	{
		get => base.Dock;
		set
		{
			base.Dock = value;
			if (Parent is SplitterPanel splitterPanel)
			{
				SplitContainer sc = splitterPanel.Owner;
				sc.SetInnerMostBorder(sc);
			}

			ResizeSplitContainer();
		}
	}

	void ApplyPanel1MinSize(int value)
	{
		if (value < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(value), value, string.Format(SR.InvalidLowBoundArgument, nameof(Panel1MinSize), value));
		}

		if (Orientation == Orientation.Vertical)
		{
			if (DesignMode && Width != DefaultSize.Width && value + Panel2MinSize + SplitterWidth > Width)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, string.Format(SR.InvalidArgument, nameof(Panel1MinSize), value));
			}
		}
		else if (Orientation == Orientation.Horizontal)
		{
			if (DesignMode && Height != DefaultSize.Height && value + Panel2MinSize + SplitterWidth > Height)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, string.Format(SR.InvalidArgument, nameof(Panel1MinSize), value));
			}
		}

		UpdateProperty(ref panel1MinSize, value);
		if (value > SplitterDistanceInternal)
		{
			SplitterDistanceInternal = value;  //Set the Splitter Distance to the end of Panel1
		}
	}

	void ApplyPanel2MinSize(int value)
	{
		if (value < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(value), value, string.Format(SR.InvalidLowBoundArgument, nameof(Panel2MinSize), value, 0));
		}

		if (Orientation == Orientation.Vertical)
		{
			if (DesignMode && Width != DefaultSize.Width && value + Panel1MinSize + SplitterWidth > Width)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, string.Format(SR.InvalidArgument, nameof(Panel2MinSize), value.ToString()));
			}
		}
		else if (Orientation == Orientation.Horizontal)
		{
			if (DesignMode && Height != DefaultSize.Height && value + Panel1MinSize + SplitterWidth > Height)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, string.Format(SR.InvalidArgument, nameof(Panel2MinSize), value));
			}
		}

		UpdateProperty(ref panel2MinSize, value);
		if (value > Panel2.Width)
		{
			SplitterDistanceInternal = Panel2.Width + SplitterWidthInternal;  //Set the Splitter Distance to the start of Panel2
		}
	}

	protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
	{
		// If we are changing Height, check if its greater than minimum else ... make it equal to the minimum
		if ((specified & BoundsSpecified.Height) != BoundsSpecified.None && Orientation == Orientation.Horizontal)
		{
			if (height < Panel1MinSize + SplitterWidthInternal + Panel2MinSize)
			{
				height = Panel1MinSize + SplitterWidthInternal + Panel2MinSize;
			}
		}

		// If we are changing Width, check if its greater than minimum else ... make it equal to the minimum
		if ((specified & BoundsSpecified.Width) != BoundsSpecified.None && Orientation == Orientation.Vertical)
		{
			if (width < Panel1MinSize + SplitterWidthInternal + Panel2MinSize)
			{
				width = Panel1MinSize + SplitterWidthInternal + Panel2MinSize;
			}
		}

		base.SetBoundsCore(x, y, width, height, specified);

		SetSplitterRect(Orientation == Orientation.Vertical);
	}

	bool selectNextControl;

	/// <summary>Activates a child control. Optionally specifies the direction in the tab order to select the control from.</summary>
	/// <param name="directed">
	///   <see langword="true" /> to specify the direction of the control to select; otherwise, <see langword="false" />.</param>
	/// <param name="forward">
	///   <see langword="true" /> to move forward in the tab order; <see langword="false" /> to move backward in the tab order.</param>
	protected override void Select(bool directed, bool forward)
	{
		if (selectNextControl)
		{
			return;
		}
		if (Panel1.Controls.Count > 0 || Panel2.Controls.Count > 0 || TabStop)
		{
			SelectNextControlInContainer(this, forward, tabStopOnly: true, nested: true, wrap: false);
		}
		else
		{
			try
			{
				selectNextControl = true;
				while (Parent != null)
				{
					if (Parent.SelectNextControl(this, forward, tabStopOnly: true, nested: true, Parent.Parent == null))
					{
						break;
					}
					Parent = Parent.Parent;
				}
			}
			finally
			{
				selectNextControl = false;
			}
		}
	}

	bool SelectNextControlInContainer(Control? ctl, bool forward, bool tabStopOnly, bool nested, bool wrap)
	{
		if (!Contains(ctl) || (!nested && ctl!.Parent != this))
		{
			ctl = null;
		}

		SplitterPanel? firstPanel = null;
		do
		{
			ctl = GetNextControl(ctl, forward);
			if (ctl is SplitterPanel panel && panel.Visible)
			{
				if (firstPanel != null)
				{
					break;
				}
				firstPanel = panel;
			}
			if (!forward && firstPanel != null && ctl!.Parent != firstPanel)
			{
				ctl = firstPanel;
				break;
			}
			if (ctl == null)
			{
				break;
			}
			if (ctl.CanSelect && ctl.TabStop)
			{
				if (ctl is SplitContainer)
				{
					((SplitContainer)ctl).Select(forward, forward);
				}
				else
				{
					SelectNextActiveControl(ctl, forward, tabStopOnly, nested, wrap);
				}
				return true;
			}
		} while (ctl != null);

		if (ctl != null && TabStop)
		{
			var c = Parent?.GetContainerControl();
			if (c != null)
			{
				if (!(c is ContainerControl containerControl))
				{
					c.ActiveControl = this;
				}
				else
				{
					containerControl.SetActiveControl(this);
				}
			}
			SetActiveControl(null);
			return true;
		}

		var selected = SelectNextControlInPanel(ctl, forward, tabStopOnly, nested, wrap);
		if (!selected)
		{
			if (Parent != null)
			{
				try
				{
					selectNextControl = true;
					Parent.SelectNextControl(this, forward, tabStopOnly: true, nested: true, wrap: true);
				}
				finally
				{
					selectNextControl = false;
				}
			}
		}
		return false;
	}

	bool SelectNextControlInPanel(Control? ctl, bool forward, bool tabStopOnly, bool nested, bool wrap)
	{
		if (!Contains(ctl) || (!nested && ctl!.Parent != this))
		{
			ctl = null;
		}

		do
		{
			ctl = GetNextControl(ctl, forward);
			if (ctl == null || (ctl is SplitterPanel && ctl.Visible))
			{
				break;
			}
			if (ctl.CanSelect && (!tabStopOnly || ctl.TabStop))
			{
				if (ctl is SplitContainer)
				{
					((SplitContainer)ctl).Select(forward, forward);
				}
				else
				{
					SelectNextActiveControl(ctl, forward, tabStopOnly, nested, wrap);
				}
				return true;
			}
		} while (ctl != null);
		return false;
	}

	static void SelectNextActiveControl(Control ctl, bool forward, bool tabStopOnly, bool nested, bool wrap)
	{
		if (ctl is ContainerControl containerControl)
		{
			var correctParentActiveControl = true;
			if (containerControl.Parent != null)
			{
				IContainerControl? c = containerControl.Parent?.GetContainerControl();
				if (c != null)
				{
					c.ActiveControl = containerControl;
					correctParentActiveControl = c.ActiveControl == containerControl;
				}
			}
			if (correctParentActiveControl)
			{
				ctl.SelectNextControl(null, forward, tabStopOnly, nested, wrap);
			}
		}
		else
		{
			ctl.Select();
		}
	}

	public BorderStyle BorderStyle
	{
		get => borderStyle;
		set
		{
			if (UpdateProperty(ref borderStyle, value))
			{
				SetInnerMostBorder(this);
				if (Parent is SplitterPanel splitterPanel)
				{
					SplitContainer sc = splitterPanel.Owner;
					sc.SetInnerMostBorder(sc);
				}
			}
		}
	}
	BorderStyle borderStyle = BorderStyle.None;

	void SetInnerMostBorder(SplitContainer sc)
	{
		foreach (Control ctl in sc.Controls)
		{
			bool foundChildSplitContainer = false;
			if (ctl is SplitterPanel)
			{
				foreach (Control c in ctl.Controls)
				{
					if (c is SplitContainer c1 && c1.Dock == DockStyle.Fill)
					{
						// We need to Overlay borders
						// if the Children have matching BorderStyles ...
						if (c1.BorderStyle != BorderStyle)
						{
							break;
						}

								((SplitterPanel)ctl).BorderStyle = BorderStyle.None;
						SetInnerMostBorder(c1);
						foundChildSplitContainer = true;
					}
				}

				if (!foundChildSplitContainer)
				{
					((SplitterPanel)ctl).BorderStyle = BorderStyle;
				}
			}
		}
	}

	async Task BeginSplitterMoveAsync(WinzorFocusInEventArgs args)
	{
		if (!IsSplitterFixed && IsSplitterMovable)
		{
			var minSizeAfter = Panel2MinSize;
			if (Orientation == Orientation.Vertical)
			{
				minSizeAfter = Math.Min(Panel2MinSize, Width - Panel1MinSize - SplitterWidthInternal);
			}
			else
			{
				minSizeAfter = Math.Min(Panel2MinSize, Height - Panel1MinSize - SplitterWidthInternal);
			}
			await (GetJSInterop<ISplitterJSInterop>()?.MoveSplitterAsync(SplitterReference, Orientation != Orientation.Horizontal, Panel1MinSize, minSizeAfter) ?? Task.CompletedTask);
		}
	}

	async Task HandleSplitterMoveAsync(SplitterMovedEventArgs args)
	{
		await InvokeWinzorDispatcherAsync(() =>
		{
			SplitMove(args.xOffset, args.yOffset);
		});
	}

	void SplitMove(int xOffset, int yOffset)
	{
		// Adapted from Winforms - the value we get from JS is already a delta
		int size = GetSplitterDistance(xOffset, yOffset);
		if (Orientation == Orientation.Vertical)
		{
			if (size + SplitterWidthInternal <= Width - Panel2MinSize)
			{
				splitterDistance = size;
			}
		}
		else
		{
			if (size + SplitterWidthInternal <= Height - Panel2MinSize)
			{
				splitterDistance = size;
			}
		}
		ApplySplitterDistance();
	}

	int GetSplitterDistance(int xDelta, int yDelta)
	{
		// Adapted from Winforms - the value we get from JS is already a delta
		int delta;
		if (Orientation == Orientation.Vertical)
		{
			delta = xDelta;
		}
		else
		{
			delta = yDelta;
		}

		// Negative delta - moving to the left
		// Positive delta - moving to the right

		int size = 0;
		switch (Orientation)
		{
			case Orientation.Vertical:
				if (RightToLeft == RightToLeft.No)
				{
					size = Math.Max(Panel1.Width + delta, borderSize);
				}
				else
				{
					// In RTL negative delta actually means increasing the size....
					size = Math.Max(Panel1.Width - delta, borderSize);
				}

				break;
			case Orientation.Horizontal:
				size = Math.Max(Panel1.Height + delta, borderSize);
				break;
		}

		if (Orientation == Orientation.Vertical)
		{
			return Math.Max(Math.Min(size, Width - Panel2MinSize - SplitterWidthInternal), Panel1MinSize);
		}
		else
		{
			return Math.Max(Math.Min(size, Height - Panel2MinSize - SplitterWidthInternal), Panel1MinSize);
		}
	}

	public void BeginInit()
	{
		initializing = true;
	}
	bool initializing;

	public void EndInit()
	{
		initializing = false;

		// validate and apply new value
		if (newPanel1MinSize != panel1MinSize)
		{
			ApplyPanel1MinSize(newPanel1MinSize);
		}
		if (newPanel2MinSize != panel2MinSize)
		{
			ApplyPanel2MinSize(newPanel2MinSize);
		}
		if (newSplitterWidth != splitterWidth)
		{
			ApplySplitterWidth(newSplitterWidth);
		}
	}

	readonly int borderSize;

	string SplitterStyleString
	{
		get
		{
			var cursor = string.Empty;
			if (IsSplitterFixed || !IsSplitterMovable)
			{
				cursor = "cursor:default;";
			}
			else
			{
				if (Orientation != Orientation.Horizontal)
				{
					cursor = "cursor:ew-resize;";
				}
				else
				{
					cursor = "cursor:ns-resize;";
				}
			}

			return $"position:absolute;width:{splitterRectangle.Width}px;height:{splitterRectangle.Height}px;top:{splitterRectangle.Top - Location.Y}px;left:{splitterRectangle.Left - Location.X}px;" + cursor;
		}
	}

	ElementReference splitterReference;

	ElementReference SplitterReference
	{
		get
		{
			return splitterReference;
		}
		set
		{
			splitterReference = value;
		}
	}

	public new bool TabStop = true;
}
