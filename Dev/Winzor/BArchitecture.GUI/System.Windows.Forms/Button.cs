using System.Drawing;
using System.Windows.Forms.ButtonInternal;
using System.Windows.Forms.Layout;
using WinzorFramework.Extensions;

namespace System.Windows.Forms;

public partial class Button : ButtonBase, IButtonControl
{
	public override bool UseParentDivForLayout => false;

	public DialogResult DialogResult { get; set; }

	public AutoSizeMode AutoSizeMode
	{
		get
		{
			return GetAutoSizeMode();
		}
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

	public virtual void NotifyDefault(bool value)
	{
		if (IsDefault != value)
		{
			IsDefault = value;
		}
	}

	/// <summary>
	///  Generates a <see cref='Control.Click'/> event for a button.
	/// </summary>
	public void PerformClick()
	{
		if (CanSelect)
		{
			OnClick(EventArgs.Empty);
		}
	}

	internal override Size GetPreferredSizeCore(Size proposedConstraints)
	{
		if (FlatStyle != FlatStyle.System)
		{
			Size prefSize = base.GetPreferredSizeCore(proposedConstraints);
			return AutoSizeMode == AutoSizeMode.GrowAndShrink ? prefSize : LayoutUtils.UnionSizes(prefSize, Size);
		}
		Size requiredSize = TextRenderer.MeasureText(Text, Font);
		requiredSize.Width += AutoSizeExtraWidth;

		Size paddedSize = requiredSize + Padding.Size;
		return AutoSizeMode == AutoSizeMode.GrowAndShrink ? paddedSize : LayoutUtils.UnionSizes(paddedSize, Size);
	}

	protected override void OnClick(EventArgs e)
	{
		if (Enabled)
		{
			var form = FindForm();
			if (form is not null && DialogResult is not DialogResult.None)
			{
				form.DialogResult = DialogResult;
			}

			base.OnClick(e);
		}
	}

	protected internal override bool ProcessMnemonic(char charCode)
	{
		if (UseMnemonic && CanProcessMnemonic() && IsMnemonic(charCode, Text))
		{
			PerformClick();
			return true;
		}

		return base.ProcessMnemonic(charCode);
	}

	protected override async Task OnClickCoreAsync(WebMouseEventArgs args)
	{
		await base.OnClickCoreAsync(args);
		if (args.IsMouseInitiated())
		{
			await OnMouseUpAsync(args);
		}
	}

	internal override ButtonBaseAdapter CreateFlatAdapter()
	{
		return new ButtonFlatAdapter(this);
	}

	internal override ButtonBaseAdapter CreatePopupAdapter()
	{
		return new ButtonPopupAdapter(this);
	}

	internal override ButtonBaseAdapter CreateStandardAdapter()
	{
		return new ButtonStandardAdapter(this);
	}
}
