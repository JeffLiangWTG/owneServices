using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms.Automation;
using System.Windows.Forms.Layout;
using WinzorFramework.Extensions;
using static Interop;

namespace System.Windows.Forms;

public partial class Label : Control
{
	bool SelfSizing => CommonProperties.ShouldSelfSize(this);

	public event EventHandler? TextAlignChanged;

	public override bool UseParentDivForLayout => false;

	LayoutUtils.MeasureTextCache? textMeasurementCache;
	int requestedHeight;
	int requestedWidth;

	/*private int _requestedHeight;
	private int _requestedWidth;*/

	/// <summary>
	///  This method is required because the Label constructor needs to know if the control is
	///  OwnerDraw but it should not call the virtual property because if a derived class has
	///  overridden the method, the derived class version will be called (before the derived
	///  class constructor is called).
	/// </summary>
	bool IsOwnerDraw() => FlatStyle != FlatStyle.System;

	AutomationLiveSetting liveSetting;

	public Label()
	{
		// this class overrides GetPreferredSizeCore, let Control automatically cache the result
		SetExtendedState(ExtendedStates.UserPreferredSizeCache, true);

		SetStyle(ControlStyles.UserPaint |
				ControlStyles.SupportsTransparentBackColor |
				ControlStyles.OptimizedDoubleBuffer, IsOwnerDraw());

		SetStyle(ControlStyles.FixedHeight |
				ControlStyles.Selectable, false);

		SetStyle(ControlStyles.ResizeRedraw, true);

		CommonProperties.SetSelfAutoSizeInDefaultLayout(this, true);

		TabStop = false;

		requestedHeight = Height;
		requestedWidth = Width;
	}

	protected override Padding DefaultMargin => new Padding(3, 0, 3, 0);

	protected override Size DefaultSize => new Size(100, AutoSize ? PreferredSize.Height : 23);

	Size GetBordersAndPadding()
	{
		var bordersAndPadding = Padding.Size;

		if (UseCompatibleTextRendering)
		{
			if (BorderStyle != BorderStyle.None)
			{
				bordersAndPadding.Height += 6; // taken from Everett.PreferredHeight
				bordersAndPadding.Width += 2; // taken from Everett.PreferredWidth
			}
			else
			{
				bordersAndPadding.Height += 3; // taken from Everett.PreferredHeight
			}
		}
		else
		{
			bordersAndPadding += SizeFromClientSize(Size.Empty);
			if (BorderStyle == BorderStyle.Fixed3D)
			{
				bordersAndPadding += new Size(2, 2);
			}
		}
		return bordersAndPadding;
	}

	public override bool AutoSize
	{
		get => base.AutoSize;
		set
		{
			if (AutoSize != value)
			{
				base.AutoSize = value;
				AdjustSize();
			}
		}
	}

	/// <summary>
	///  Updates the control in response to events that could affect either
	///  the size of the control, or the size of the text within it.
	/// </summary>
	internal void AdjustSize()
	{
		if (!SelfSizing)
		{
			return;
		}

		// the rest is here for RTM compat.

		// If width and/or height are constrained by anchoring, don't adjust control size
		// to fit around text, since this will cause us to lose the original anchored size.
		if (!AutoSize &&
			((Anchor & (AnchorStyles.Left | AnchorStyles.Right)) == (AnchorStyles.Left | AnchorStyles.Right) ||
			(Anchor & (AnchorStyles.Top | AnchorStyles.Bottom)) == (AnchorStyles.Top | AnchorStyles.Bottom)))
		{
			return;
		}

		// Resize control to fit around current text

		int saveHeight = requestedHeight;
		int saveWidth = requestedWidth;
		try
		{
			Size preferredSize = (AutoSize) ? PreferredSize : new Size(saveWidth, saveHeight);
			Size = preferredSize;
		}
		finally
		{
			requestedHeight = saveHeight;
			requestedWidth = saveWidth;
		}
	}

	/// <summary>
	///  Overrides Control.setBoundsCore to enforce autoSize.
	/// </summary>

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);
		AdjustTextPosition();
	}

	protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
	{
		if ((specified & BoundsSpecified.Height) != BoundsSpecified.None)
		{
			requestedHeight = height;
		}

		if ((specified & BoundsSpecified.Width) != BoundsSpecified.None)
		{
			requestedWidth = width;
		}

		if (AutoSize && SelfSizing)
		{
			Size preferredSize = PreferredSize;
			width = preferredSize.Width;
			height = preferredSize.Height;
		}

		base.SetBoundsCore(x, y, width, height, specified);

		Debug.Assert(!AutoSize || (AutoSize && !SelfSizing) || Size == PreferredSize,
			"It is SetBoundsCore's responsibility to ensure Size = PreferredSize when AutoSize is true.");
	}

	public virtual ContentAlignment TextAlign
	{
		get => textAlign;
		set
		{
			if (UpdateProperty(ref textAlign, value))
			{
				AdjustTextPosition();
				OnTextAlignChanged(EventArgs.Empty);
			}
		}
	}
	ContentAlignment textAlign = ContentAlignment.TopLeft;

	protected void OnTextAlignChanged(EventArgs e)
	{
		TextAlignChanged?.Invoke(this, e);
	}

	void AdjustTextPosition()
	{
		var textSize = TextRenderer.MeasureText(Text, Font, Size, TextFormatFlags.WordBreak);

		// Winforms is creating a Padding by defalut when a text is left align within a Lable.
		//https://stackoverflow.com/questions/12035548/default-padding-for-label
		var leftSpacing = TextRenderer.GetEmptyCharWidth(Font);
		if (Font.Name == "Wingdings")
		{
			leftSpacing *= 0.25;
		}

		if ((TextAlign & LayoutUtils.AnyCenter) != 0)
		{
			textSize.Width -= TextRenderer.GetWidthAdjustment(Font);
		}
		var left = TextAlign switch
		{
			_ when (Size.Width < textSize.Width) => Padding.Left,
			_ when ((TextAlign & LayoutUtils.AnyLeft) != 0) => Padding.Left + Math.Floor(leftSpacing),
			_ when (TextAlign & LayoutUtils.AnyCenter) != 0 => (Size.Width + Padding.Left - textSize.Width) / 2f,
			_ => Size.Width - textSize.Width - Padding.Right,
		};
		var top = TextAlign switch
		{
			_ when (TextAlign & LayoutUtils.AnyTop) != 0 => Padding.Top,
			_ when (TextAlign & LayoutUtils.AnyMiddle) != 0
				=> (Size.Height > textSize.Height) ? (Size.Height + Padding.Top - textSize.Height) / 2f : 0,
			_ => Size.Height - textSize.Height - Padding.Bottom
		};
		CssTextAlign = TextAlign switch
		{
			_ when (TextAlign & LayoutUtils.AnyLeft) != 0 => "left",
			_ when (TextAlign & LayoutUtils.AnyCenter) != 0 => "center",
			_ => "right"
		};
		TextPositionsStyleString = $"position: absolute; text-align: {CssTextAlign}; left: {left}px; top: {top}px;";
	}

	protected virtual string TextFontStyleString { get => this.FontStyleString(); }

	protected virtual string TextPositionsStyleString { get; set; } = string.Empty;

	protected virtual string TextStyleString { get => TextPositionsStyleString + TextFontStyleString; }

	protected virtual string CssTextAlign { get; set; } = string.Empty;

	public BorderStyle BorderStyle
	{
		get => borderStyle;
		set
		{
			if (UpdateProperty(ref borderStyle, value))
			{
				if (AutoSize)
				{
					AdjustSize();
				}

				// Border style might have changed ClientSize
				UpdateBounds(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);
			}
		}
	}
	BorderStyle borderStyle = BorderStyle.None;

	internal override void AdjustWindowRectEx(ref RECT rect)
	{
		base.AdjustWindowRectEx(ref rect);

		if (BorderStyle == BorderStyle.FixedSingle)
		{
			// div has 1px border
			rect = new RECT(rect.left - 1, rect.top - 1, rect.right + 1, rect.bottom + 1);
		}
		else if (BorderStyle == BorderStyle.Fixed3D)
		{
			// In WinForms, this sets the SS.INSET flag, which doesn't adjust ClientSize.

			// That said, the call to UpdateBounds() will adjust the client area based on the window handle, which returns a size slightly smaller than clientSize.
			// In Winzor, we don't do any of that and the control does not move or change size

			// Currently in the Winzor case, the border style is 0.5px, which is iconvenient, but we'll round to 1 to avoid drawing on the border
			rect = new RECT(rect.left - 1, rect.top - 1, rect.right + 1, rect.bottom + 1);
		}
	}

	public FlatStyle FlatStyle { get; set; }

	public Image? Image
	{
		get => image;
		set
		{
			ImageWidth = value?.Width ?? 0;
			ImageHeight = value?.Height ?? 0;
			UpdateProperty(ref image, value);
		}
	}
	Image? image;
	internal int ImageWidth { get; private set; }
	internal int ImageHeight { get; private set; }

	public ContentAlignment ImageAlign { get; set; }

	public int ImageIndex
	{
		get => imageIndex;
		set => UpdateProperty(ref imageIndex, value);
	}
	int imageIndex = -1;

	public ImageList? ImageList { get; set; }

	public string? ImageKey { get; set; }

	public bool AutoEllipsis { get; set; }

	public bool UseMnemonic
	{
		get => useMnemonic;
		set
		{
			if (UpdateProperty(ref useMnemonic, value))
			{
				MeasureTextCache.InvalidateCache();

				using (LayoutTransaction.CreateTransactionIf(AutoSize, Parent, this, PropertyNames.Text))
				{
					AdjustSize();
				}
			}
		}
	}
	bool useMnemonic = true;

	public bool UseCompatibleTextRendering { get; set; }

	protected override void OnParentChanged(EventArgs e)
	{
		base.OnParentChanged(e);
		if (SelfSizing)
		{
			// In the case of SelfSizing
			// we don't know what size to be until we're parented
			AdjustSize();
		}
	}

	internal LayoutUtils.MeasureTextCache MeasureTextCache
		=> textMeasurementCache ??= new LayoutUtils.MeasureTextCache();

	[DefaultValue(AutomationLiveSetting.Off)]
	public AutomationLiveSetting LiveSetting
	{
		get => liveSetting;
		set
		{
			liveSetting = value;
		}
	}

	protected override void OnTextChanged(EventArgs e)
	{
		using (LayoutTransaction.CreateTransactionIf(AutoSize, Parent, this, PropertyNames.Text))
		{
			MeasureTextCache.InvalidateCache();
			base.OnTextChanged(e);
			AdjustSize();
			AdjustTextPosition();
			Invalidate();
		}

		if (IsAccessibilityObjectCreated && LiveSetting != AutomationLiveSetting.Off)
		{
			AccessibilityObject.RaiseLiveRegionChanged();
		}
	}

	protected override void OnFontChanged(EventArgs e)
	{
		MeasureTextCache.InvalidateCache();
		base.OnFontChanged(e);
		AdjustSize();
		AdjustTextPosition();
		Invalidate();
	}

	/// <summary>Retrieves the size of a rectangular area into which a control can be fitted.</summary>
	/// <param name="proposedSize">The custom-sized area for a control.</param>
	/// <returns>An ordered pair of type <see cref="T:System.Drawing.Size" /> representing the width and height of a rectangle.</returns>
	public override Size GetPreferredSize(Size proposedSize)
	{
		//make sure the behavior is consistent with GetPreferredSizeCore
		if (proposedSize.Width == 1)
		{
			proposedSize.Width = 0;
		}

		if (proposedSize.Height == 1)
		{
			proposedSize.Height = 0;
		}
		return base.GetPreferredSize(proposedSize);
	}

	internal override Size GetPreferredSizeCore(Size proposedConstraints)
	{
		Size bordersAndPadding = GetBordersAndPadding();

		// Subtract border area from constraints
		proposedConstraints -= bordersAndPadding;

		// keep positive
		proposedConstraints = LayoutUtils.UnionSizes(proposedConstraints, Size.Empty);

		var flags = ((FlatStyle != FlatStyle.System) ? CreateTextFormatFlags(proposedConstraints) : TextFormatFlags.Default);
		Size requiredSize;
		if (string.IsNullOrEmpty(Text))
		{
			requiredSize = TextRenderer.MeasureText("0", Font, proposedConstraints, flags);
			requiredSize.Width = 0;
		}
		else
		{
			requiredSize = MeasureTextCache.GetTextSize(Text, Font, proposedConstraints, flags);
		}
		requiredSize += bordersAndPadding;

		return requiredSize;
	}

	internal virtual TextFormatFlags CreateTextFormatFlags(Size constrainingSize)
	{
		TextFormatFlags textFormatFlags = ControlPaint.CreateTextFormatFlags(this, TextAlign, AutoEllipsis, UseMnemonic);
		if (!MeasureTextCache.TextRequiresWordBreak(Text, Font, constrainingSize, textFormatFlags))
		{
			textFormatFlags &= ~(TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak);
		}
		return textFormatFlags;
	}

	public virtual int PreferredHeight => PreferredSize.Height;
	public virtual int PreferredWidth => PreferredSize.Width;

	[AllowNull]
	public override string Text
	{
		get => base.Text;
		set
		{
			base.Text = value;
		}
	}

	[DefaultValue(false)]
	public new bool TabStop
	{
		get => base.TabStop;
		set => base.TabStop = value;
	}

	protected override void OnMouseDoubleClick(MouseEventArgs e)
	{
		if (!string.IsNullOrEmpty(Text))
		{
			Clipboard.SetText(Text);
		}
	}

	/// <summary>
	/// Returns a string representation for this control.
	/// </summary>
	public override string ToString()
	{
		return $"{base.ToString()}, Text: {Text ?? string.Empty}";
	}
}
