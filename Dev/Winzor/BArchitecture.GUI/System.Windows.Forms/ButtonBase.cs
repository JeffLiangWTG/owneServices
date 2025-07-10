using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms.ButtonInternal;
using System.Windows.Forms.Layout;
using WinzorFramework.Extensions;

namespace System.Windows.Forms;

public class ButtonBase : Control
{
	public ButtonBase()
	{
		FlatAppearance = new FlatButtonAppearance(this);
	}

	protected override Size DefaultSize => new Size(75, 23);

	public bool AutoEllipsis { get; set; }

	public FlatStyle FlatStyle { get; set; } = FlatStyle.Standard;

	public FlatButtonAppearance FlatAppearance { get; init; }

	internal void FlatAppearanceUpdated()
	{
		NotifyRenderRequired();
	}

	public Image? Image
	{
		get
		{
			if (image?.IsDisposed() is true)
			{
				image = null;
			}

			return image;
		}
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

	public ImageList? ImageList { get; set; }

	ContentAlignment imageAlign = ContentAlignment.MiddleCenter;

	public ContentAlignment ImageAlign
	{
		get => imageAlign;
		set => UpdateProperty(ref imageAlign, value);
	}

	public TextImageRelation TextImageRelation { get; set; }

	ContentAlignment textAlign = ContentAlignment.MiddleCenter;

	public ContentAlignment TextAlign
	{
		get => textAlign;
		set => UpdateProperty(ref textAlign, value);
	}

	public bool UseCompatibleTextRendering { get; set; }

	bool isDefault;

	protected internal bool IsDefault
	{
		get => isDefault;
		set
		{
			UpdateProperty(ref isDefault, value);
		}
	}

	public bool UseMnemonic
	{
		get => useMnemonic;
		set
		{
			if (UpdateProperty(ref useMnemonic, value))
			{
				LayoutTransaction.DoLayoutIf(AutoSize, Parent, this, PropertyNames.Text);
			}
		}
	}
	bool useMnemonic = true;

	[AllowNull]
	public override string Text
	{
		get => base.Text;
		set
		{
			base.Text = value;
		}
	}

	// This allows the user to disable visual styles for the button so that it inherits its background color
	bool enableVisualStyleBackground = true;

	bool isEnableVisualStyleBackgroundSet;

	public bool UseVisualStyleBackColor
	{
		get
		{
			if (isEnableVisualStyleBackgroundSet || ((RawBackColor.IsEmpty) && (BackColor == SystemColors.Control)))
			{
				return enableVisualStyleBackground;
			}
			else
			{
				return false;
			}
		}
		set
		{
			if (isEnableVisualStyleBackgroundSet && value == enableVisualStyleBackground)
			{
				return;
			}

			isEnableVisualStyleBackgroundSet = true;
			enableVisualStyleBackground = value;
			Invalidate();
		}
	}

	public override Color BackColor
	{
		get => base.BackColor;
		set
		{
			if (DesignMode)
			{
				if (value != Color.Empty)
				{
					UseVisualStyleBackColor = false;
				}
			}
			else
			{
				UseVisualStyleBackColor = false;
			}

			base.BackColor = value;
		}
	}

	internal override Size GetPreferredSizeCore(Size proposedConstraints)
	{
		Size prefSize = Adapter.GetPreferredSizeCore(proposedConstraints);
		return LayoutUtils.UnionSizes(prefSize + Padding.Size, MinimumSize);
	}

	internal ButtonBaseAdapter? _adapter;
	internal FlatStyle _cachedAdapterType;

	internal ButtonBaseAdapter Adapter
	{
		get
		{
			if (_adapter == null || FlatStyle != _cachedAdapterType)
			{
				switch (FlatStyle)
				{
					case FlatStyle.Standard:
					case FlatStyle.System:
						_adapter = CreateStandardAdapter();
						break;
					case FlatStyle.Popup:
						_adapter = CreatePopupAdapter();
						break;
					case FlatStyle.Flat:
						_adapter = CreateFlatAdapter();
						break;
					default:
						Debug.Fail("Unsupported FlatStyle: '" + FlatStyle + '"');
						break;
				}
				_cachedAdapterType = FlatStyle;
			}
			return _adapter;
		}
	}

	internal virtual ButtonBaseAdapter CreateFlatAdapter()
	{
		Debug.Fail("Derived classes need to provide a meaningful implementation.");
		return null;
	}

	internal virtual ButtonBaseAdapter CreatePopupAdapter()
	{
		Debug.Fail("Derived classes need to provide a meaningful implementation.");
		return null;
	}

	internal virtual ButtonBaseAdapter CreateStandardAdapter()
	{
		Debug.Fail("Derived classes need to provide a meaningful implementation.");
		return null;
	}

	public string InnerDivStyleString { get; set; } = string.Empty;

	public int ImageIndex { get; set; }
}
