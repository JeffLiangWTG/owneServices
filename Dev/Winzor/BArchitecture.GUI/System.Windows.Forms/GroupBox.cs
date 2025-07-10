using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms;

public partial class GroupBox : Control
{
	public GroupBox() : base()
	{
		SetStyle(ControlStyles.ContainerControl, true);
		SetStyle(ControlStyles.Selectable, false);
		TabStop = false;
	}
	protected override Size DefaultSize => new Size(200, 100);

	int fontHeight = -1;
	Font? cachedFont;

	public FlatStyle FlatStyle { get; set; } = FlatStyle.Standard;

	public AutoSizeMode AutoSizeMode
	{
		get => GetAutoSizeMode();
		set
		{
			if (GetAutoSizeMode() != value)
			{
				SetAutoSizeMode(value);
				if (Parent is not null)
				{
					// DefaultLayout does not keep anchor information until it needs to.  When
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

	public bool UseCompatibleTextRendering { get; set; }

	protected virtual bool BoldLegend => false;

	internal override Size GetPreferredSizeCore(Size proposedSize)
	{
		// Translating 0,0 from ClientSize to actual Size tells us how much space is required for the borders.
		Size borderSize = SizeFromClientSize(Size.Empty);
		Size totalPadding = borderSize + new Size(0, fontHeight) + Padding.Size;

		Size prefSize = LayoutEngine.GetPreferredSize(this, proposedSize - totalPadding);
		return prefSize + totalPadding;
	}

	/// <summary>
	///  Set the default Padding to 3 so that it is consistent with Everett
	/// </summary>
	protected override Padding DefaultPadding => new Padding(3);

	/// <summary>
	///  Gets a rectangle that represents the dimensions of the <see cref="GroupBox"/>
	/// </summary>
	public override Rectangle DisplayRectangle
	{
		get
		{
			Size size = ClientSize;

			if (fontHeight == -1)
			{
				fontHeight = FontHeight;
				cachedFont = Font;
			}
			else if (!ReferenceEquals(cachedFont, Font))
			{
				// Must also cache font identity here because we need to provide an accurate DisplayRectangle
				// picture even before the OnFontChanged event bubbles through.
				fontHeight = FontHeight;
				cachedFont = Font;
			}

			// For efficiency, so that we don't need to read property store four times
			Padding padding = Padding;
			return new Rectangle(
				padding.Left,
				fontHeight + padding.Top,
				Math.Max(size.Width - padding.Horizontal, 0),
				Math.Max(size.Height - fontHeight - padding.Vertical, 0));
		}
	}

	[AllowNull]
	public override string Text
	{
		get => base.Text;
		set
		{
			base.Text = value;
		}
	}

	protected override void OnFontChanged(EventArgs e)
	{
		fontHeight = -1;
		cachedFont = null;
		Invalidate();
		base.OnFontChanged(e);
	}

	protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
	{
		if (factor.Width != 1F && factor.Height != 1F)
		{
			// Make sure when we're scaling by non-unity to clear the font cache
			// as the font has likely changed, but we don't know it yet as OnFontChanged has yet to
			// be called on us by our parent.
			fontHeight = -1;
			cachedFont = null;
		}

		base.ScaleControl(factor, specified);
	}

	/// <summary>
	///  Returns a string representation for this control.
	/// </summary>
	public override string ToString() => $"{base.ToString()}, Text: {Text}";
}
