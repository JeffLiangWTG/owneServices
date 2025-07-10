using System.Drawing;
using Microsoft.AspNetCore.Components;
using WinzorFramework;
using WinzorFramework.JSInterop;

namespace System.Windows.Forms;

[Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in Splitter.razor")]
public partial class Splitter : Control
{
	public Splitter()
	{
		SetStyle(ControlStyles.Selectable, false);
		TabStop = false;
		Dock = DockStyle.Left;
	}

	string SplitterStyleString
	{
		get
		{
			var cursor = Horizontal ? "cursor:col-resize;" : "cursor:row-resize;";
			return ControlStyleString + cursor;
		}
	}

	protected virtual bool ApplicationDoEventsOnSplitterMove => true; // To allow KSplitter to bypass the Application.DoEvents call

	public override bool UseParentDivForLayout => false;

	public override AnchorStyles Anchor
	{
		get => AnchorStyles.None;
		set { }
	}

	protected override Size DefaultSize => new (defaultWidth, defaultWidth);

	public override DockStyle Dock
	{
		get => base.Dock;
		set
		{
			// Copied from Winforms
			if (!(value == DockStyle.Top || value == DockStyle.Bottom || value == DockStyle.Left || value == DockStyle.Right))
			{
				throw new ArgumentException(SR.SplitterInvalidDockEnum);
			}

			int requestedSize = splitterThickness;

			base.Dock = value;
			switch (Dock)
			{
				case DockStyle.Top:
				case DockStyle.Bottom:
					if (splitterThickness != -1)
					{
						Height = requestedSize;
					}

					break;
				case DockStyle.Left:
				case DockStyle.Right:
					if (splitterThickness != -1)
					{
						Width = requestedSize;
					}

					break;
			}
		}
	}

	bool Horizontal => Dock == DockStyle.Left || Dock == DockStyle.Right;

	public int MinExtra
	{
		get => minExtra;
		set => minExtra = value > 0 ? value : 0;
	}
	int minExtra = 25;

	public int MinSize
	{
		get => minSize;
		set => minSize = value > 0 ? value : 0;
	}
	int minSize = 25;

	public int SplitPosition
	{
		get
		{
			if (splitSize == -1)
			{
				splitSize = CalcSplitSize();
			}

			return splitSize;
		}
		set
		{
			// Adapted from Winforms
			CalcMaxSize();
			var target = FindTarget();
			if (value > maxSize)
			{
				value = maxSize;
			}

			if (value < minSize)
			{
				value = minSize;
			}

			splitSize = value;

			if (target is null)
			{
				splitSize = -1;
				return;
			}

			Rectangle bounds = target.Bounds;
			switch (Dock)
			{
				case DockStyle.Top:
					bounds.Height = value;
					break;
				case DockStyle.Bottom:
					bounds.Y += bounds.Height - splitSize;
					bounds.Height = value;
					break;
				case DockStyle.Left:
					bounds.Width = value;
					break;
				case DockStyle.Right:
					bounds.X += bounds.Width - splitSize;
					bounds.Width = value;
					break;
			}

			target.Bounds = bounds;
			if (ApplicationDoEventsOnSplitterMove)
			{
#pragma warning disable CW1049 // Don't Use Application Do Events Rule - Copied from Winforms Code
				Application.DoEvents();
#pragma warning restore CW1049 // Don't Use Application Do Events Rule - Copied from Winforms Code
			}
			OnSplitterMoved(new SplitterEventArgs(Left, Top, (Left + bounds.Width / 2), (Top + bounds.Height / 2)));
		}
	}
	int splitSize = -1;

	void CalcMaxSize()
	{
		// Copied from Winforms
		var target = FindTarget();
		if (target is not null && Parent is not null)
		{
			int dockWidth = 0, dockHeight = 0;
			foreach (var control in Parent.Controls)
			{
				if (control != target)
				{
					switch (control.Dock)
					{
						case DockStyle.Left:
						case DockStyle.Right:
							dockWidth += control.Width;
							break;
						case DockStyle.Top:
						case DockStyle.Bottom:
							dockHeight += control.Height;
							break;
					}
				}
			}

			if (Horizontal)
			{
				maxSize = Parent.ClientSize.Width - dockWidth - minExtra;
			}
			else
			{
				maxSize = Parent.ClientSize.Height - dockHeight - minExtra;
			}
		}
	}

	void ParentControl_Resized(object? sender, EventArgs e)
	{
		if (FindForm()?.WindowState == FormWindowState.Minimized || Parent == null)
		{
			return;
		}

		var isParentControlSizeShrinked = Horizontal && Parent.Width < parentControlWidth || !Horizontal && Parent.Height < parentControlHeight;
		if (isParentControlSizeShrinked)
		{
			SplitPosition = CalcSplitSize();
		}
		parentControlWidth = Parent.Width;
		parentControlHeight = Parent.Height;
	}

	int CalcSplitSize()
	{
		// Copied from Winforms
		var target = FindTarget();
		if (target is null)
		{
			return -1;
		}

		Rectangle r = target.Bounds;
		switch (Dock)
		{
			case DockStyle.Top:
			case DockStyle.Bottom:
				return r.Height;
			case DockStyle.Left:
			case DockStyle.Right:
				return r.Width;
			default:
				return -1;
		}
	}

	public event SplitterEventHandler? SplitterMoving;

	protected virtual void OnSplitterMoved(SplitterEventArgs args)
	{
		SplitterMoved?.Invoke(this, args);
	}
	public SplitterEventHandler? SplitterMoved;

	public BorderStyle BorderStyle { get; set; } = BorderStyle.None;

	void MoveSplitter(int xOffset, int yOffset)
	{
		// Adapted from Winforms Method GetSplitSize
		var target = FindTarget();
		if (target != null)
		{
			SplitPosition = Dock switch
			{
				DockStyle.Top => target.Height + yOffset,
				DockStyle.Bottom => target.Height - yOffset,
				DockStyle.Left => target.Width + xOffset,
				DockStyle.Right => target.Width - xOffset,
				_ => 0,
			};
		}
	}

	protected override void OnLayout(LayoutEventArgs e)
	{
		base.OnLayout(e);
		if (Parent != null)
		{
			Parent.SizeChanged -= ParentControl_Resized;
			Parent.SizeChanged += ParentControl_Resized;
		}
	}

	protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
	{
		// Copied from Winforms
		if (Horizontal)
		{
			if (width < 1)
			{
				width = 3;
			}

			splitterThickness = width;
		}
		else
		{
			if (height < 1)
			{
				height = 3;
			}

			splitterThickness = height;
		}

		base.SetBoundsCore(x, y, width, height, specified);
	}

	Control? FindTarget()
	{
		// Copied from Winforms
		Control? parent = Parent;
		if (parent is null)
		{
			return null;
		}

		foreach (var target in parent.Controls)
		{
			if (target != this)
			{
				switch (Dock)
				{
					case DockStyle.Top:
						if (target.Bottom == Top)
						{
							return target;
						}
						break;
					case DockStyle.Bottom:
						if (target.Top == Bottom)
						{
							return target;
						}
						break;
					case DockStyle.Left:
						if (target.Right == Left)
						{
							return target;
						}
						break;
					case DockStyle.Right:
						if (target.Left == Right)
						{
							return target;
						}
						break;
				}
			}
		}

		return null;
	}

	async Task BeginSplitterMoveAsync(WinzorFocusInEventArgs args)
	{
		var minSizeBefore = 0;
		var minSizeAfter = 0;
		switch (Dock)
		{
			case DockStyle.Top:
				minSizeBefore = minSize;
				minSizeAfter = minExtra;
				break;
			case DockStyle.Bottom:
				minSizeBefore = minExtra;
				minSizeAfter = minSize;
				break;
			case DockStyle.Left:
				minSizeBefore = minSize;
				minSizeAfter = minExtra;
				break;
			case DockStyle.Right:
				minSizeBefore = minExtra;
				minSizeAfter = minSize;
				break;
		}
		await (GetJSInterop<ISplitterJSInterop>()?.MoveSplitterAsync(SplitterReference, Horizontal, minSizeBefore, minSizeAfter) ?? Task.CompletedTask);
	}

	async Task HandleSplitterMoveAsync(SplitterMovedEventArgs args)
	{
		await InvokeWinzorDispatcherAsync(() =>
		{
			MoveSplitter(args.xOffset, args.yOffset);
		});
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

	protected internal override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		GetJSInterop<ISplitterJSInterop>()?.PreloadInterop();
	}

	protected internal override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			await BeginSplitterMoveAsync(new WinzorFocusInEventArgs());
		}
		await base.OnAfterRenderAsync(firstRender);
	}

	int maxSize;
	int splitterThickness = 3;

	const int defaultWidth = 3;
	int parentControlWidth;
	int parentControlHeight;
}

public delegate void SplitterEventHandler(object? sender, SplitterEventArgs e);
public delegate void SplitterCancelEventHandler(object? sender, SplitterCancelEventArgs e);
