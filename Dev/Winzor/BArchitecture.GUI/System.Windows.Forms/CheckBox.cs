using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms.ButtonInternal;
using Microsoft.AspNetCore.Components;

namespace System.Windows.Forms;

[DefaultBindingProperty("Checked")]
public partial class CheckBox : ButtonBase
{
	public CheckBox()
	{
		SetAutoSizeMode(AutoSizeMode.GrowAndShrink);
		TextAlign = ContentAlignment.MiddleLeft;
	}

	public override bool UseParentDivForLayout => false;
	public override bool CaptureElementReference => true;
	protected override Size DefaultSize => new Size(104, 24);

	public bool Checked
	{
		get => CheckState != CheckState.Unchecked;
		set
		{
			CheckState = value ? CheckState.Checked : CheckState.Unchecked;
		}
	}

	protected void OnCheckedChanged(EventArgs e)
	{
		CheckedChanged?.Invoke(this, e);
	}

	bool IsDisabled => !Enabled || ReadOnlyAttributeValue;

	public event EventHandler? CheckedChanged;

	protected async Task CheckedChangedAsync(ChangeEventArgs e)
	{
		if (e.Value is null || !(e.Value is bool))
		{
			return;
		}

		await InvokeWinzorDispatcherAsync(() =>
		{
			if (IsDisabled)
			{
				OnIllegalOperations("Checkbox is readonly");
			}
			else
			{
				Checked = (bool)e.Value;
				base.OnClick(EventArgs.Empty);
			}
		});
	}

	protected override void OnClick(EventArgs e)
	{
		Checked = !Checked;
		base.OnClick(e);
	}

	/// <summary>
	///  Raises the <see cref='CheckStateChanged'/> event.
	/// </summary>
	protected void OnCheckStateChanged(EventArgs e)
	{
		if (OwnerDraw)
		{
			Refresh();
		}
		CheckStateChanged?.Invoke(this, e);
	}

	/// <summary>
	///  Raises the <see cref='AppearanceChanged'/> event.
	/// </summary>
	protected void OnAppearanceChanged(EventArgs e)
	{
		AppearanceChanged?.Invoke(this, e);
	}

	public CheckState CheckState
	{
		get => checkState;
		set
		{
			if (!ClientUtils.IsEnumValid(value, (int)value, (int)CheckState.Unchecked, (int)CheckState.Indeterminate))
			{
				throw new InvalidEnumArgumentException("value", (int)value, typeof(CheckState));
			}
			var oldChecked = Checked;
			if (UpdateProperty(ref checkState, value))
			{
				if (oldChecked != Checked)
				{
					OnCheckedChanged(EventArgs.Empty);
				}
				OnCheckStateChanged(EventArgs.Empty);
			}
		}
	}

	CheckState checkState = CheckState.Unchecked;

	internal bool OwnerDraw
	{
		get
		{
			return FlatStyle != FlatStyle.System;
		}
	}

	public event EventHandler? CheckStateChanged;

	public event EventHandler? AppearanceChanged;

	public bool AutoCheck { get; set; }

	ContentAlignment checkAlign = ContentAlignment.MiddleLeft;

	public ContentAlignment CheckAlign
	{
		get => checkAlign;
		set => UpdateProperty(ref checkAlign, value);
	}

	public bool ThreeState { get; set; }

	public Appearance Appearance
	{
		get => appearance;
		set
		{
			if (UpdateProperty(ref appearance, value))
			{
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	Appearance appearance = Appearance.Normal;

	protected override int AutoSizeExtraWidth => 15;
	protected int AutoSizeExtraHeight => 2;

	const int FlatSystemStyleMinimumHeight = 13;

	protected virtual bool ReadOnlyAttributeValue => !Enabled;

	internal override Size GetPreferredSizeCore(Size proposedSize)
	{
		if (base.FlatStyle != FlatStyle.System)
		{
			return base.GetPreferredSizeCore(proposedSize);
		}
		var size = TextRenderer.MeasureText(Text, Font);
		size.Width += AutoSizeExtraWidth;
		size.Height = Math.Max(size.Height + 5, FlatSystemStyleMinimumHeight);

		return size;
	}

	protected internal override bool ProcessMnemonic(char charCode)
	{
		if (UseMnemonic && IsMnemonic(charCode, Text) && CanSelect)
		{
			if (Focus())
			{
				OnClick(EventArgs.Empty);
			}
			return true;
		}
		return false;
	}

	internal override ButtonBaseAdapter CreateStandardAdapter()
	{
		return new CheckBoxStandardAdapter(this);
	}

	/// <summary>
	///  Provides some interesting information for the CheckBox control in
	///  String form.
	/// </summary>
	public override string ToString()
	{
		string? s = base.ToString();
		// We shouldn't need to convert the enum to int
		int checkState = (int)CheckState;
		return s + ", CheckState: " + checkState.ToString(CultureInfo.InvariantCulture);
	}
}
