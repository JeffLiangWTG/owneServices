using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms.Layout;
using WinzorFramework;

namespace System.Windows.Forms;

public partial class Panel : ScrollableControl
{
	BorderStyle _borderStyle = BorderStyle.None;

	public Panel()
	{
		TabStop = false;
		SetExtendedState(ExtendedStates.UserPreferredSizeCache, true);
		SetStyle(ControlStyles.Selectable, false);
	}

	protected override EventAttribute EventAttributes => EventAttribute.Click;

	protected bool Draggable { get; set; }

	protected override string ClassName => "panel";

	protected override Size DefaultSize => new Size(200, 100);

	protected internal override string ControlStyleString => base.ControlStyleString + GetBorderStyleString();

	public override bool CaptureElementReference => true;

	string GetBorderStyleString()
	{
		var result = string.Empty;
		switch (BorderStyle)
		{
			case BorderStyle.FixedSingle:
				result = "border: 1px solid black;";
				break;
			case BorderStyle.Fixed3D:
				result = "border: inset 2px;";
				break;
		}

		return result;
	}

	public virtual AutoSizeMode AutoSizeMode
	{
		get => GetAutoSizeMode();
		set
		{
			if (GetAutoSizeMode() != value)
			{
				SetAutoSizeMode(value);
				if (Parent is not null)
				{
					// DefaultLayout does not keep anchor information until it needs to. When
					// AutoSize became a common property, we could no longer blindly call into
					// DefaultLayout, so now we do a special InitLayout just for DefaultLayout.
					if (Parent.LayoutEngine == DefaultLayout.Instance)
					{
						Parent.LayoutEngine.InitLayout(this, BoundsSpecified.Size);
					}

					LayoutTransaction.DoLayout(Parent, this, PropertyNames.AutoSize);
				}
			}
		}
	}

	public BorderStyle BorderStyle
	{
		get => _borderStyle;
		set
		{
			if (UpdateProperty(ref _borderStyle, value))
			{
				UpdateStyles();

				// Border style might have changed ClientSize
				UpdateBounds(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);
			}
		}
	}

	public void ScrollControlIntoView(Control activeControl)
	{
	}

	internal override void AdjustWindowRectEx(ref Interop.RECT rect)
	{
		base.AdjustWindowRectEx(ref rect);

		if (BorderStyle == BorderStyle.FixedSingle)
		{
			// div has 1px border
			rect = new Interop.RECT(rect.left - 1, rect.top - 1, rect.right + 1, rect.bottom + 1);
		}
		else if (BorderStyle == BorderStyle.Fixed3D)
		{
			// div has 1px border, 1px shadow
			rect = new Interop.RECT(rect.left - 2, rect.top - 2, rect.right + 2, rect.bottom + 2);
		}
	}

	internal override Size GetPreferredSizeCore(Size proposedSize)
	{
		var size = SizeFromClientSize(Size.Empty) + Padding.Size;
		return LayoutEngine.GetPreferredSize(this, proposedSize - size) + size;
	}

	[DefaultValue(false)]
	public new bool TabStop
	{
		get => base.TabStop;
		set => base.TabStop = value;
	}
}
