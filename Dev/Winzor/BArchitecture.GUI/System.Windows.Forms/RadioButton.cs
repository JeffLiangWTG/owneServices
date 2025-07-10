using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms.ButtonInternal;

namespace System.Windows.Forms;

public partial class RadioButton : ButtonBase
{
	public override bool UseParentDivForLayout => false;

	const int FlatSystemStylePaddingWidth = 19;
	const int FlatSystemStyleMinimumHeight = 13;

	public RadioButton() : base()
	{
		TextAlign = ContentAlignment.MiddleLeft;
		CheckAlign = ContentAlignment.MiddleLeft;
		TabStop = false;
		SetAutoSizeMode(AutoSizeMode.GrowAndShrink);
	}

	// Used to see if we need to iterate through the auto checked items and modify their tab stops.
	bool firstFocus = true;

	public bool Checked
	{
		get => @checked;
		set
		{
			if (UpdateProperty(ref @checked, value))
			{
				PerformAutoUpdates(false);
				OnCheckedChanged(EventArgs.Empty);
			}
		}
	}
	bool @checked;

	protected virtual void OnCheckedChanged(EventArgs e)
	{
		CheckedChanged?.Invoke(this, e);
	}

	public event EventHandler? CheckedChanged;

	public ContentAlignment CheckAlign { get; set; }

	bool autoCheck = true;
	public bool AutoCheck
	{
		get
		{
			return autoCheck;
		}
		set
		{
			if (autoCheck != value)
			{
				autoCheck = value;
				PerformAutoUpdates(false);
			}
		}
	}

	/// <summary>
	///  Deriving classes can override this to configure a default size for their control.
	///  This is more efficient than setting the size in the control's constructor.
	/// </summary>
	protected override Size DefaultSize
	{
		get
		{
			return new Size(104, 24);
		}
	}

	void PerformAutoUpdates(bool tabbedInto)
	{
		if (!autoCheck)
		{
			return;
		}

		if (firstFocus)
		{
			WipeTabStops(tabbedInto);
		}

		TabStop = Checked;
		if (!Checked)
		{
			return;
		}

		if (Parent == null)
		{
			return;
		}

		ControlCollection controls = Parent.Controls;
		for (int i = 0; i < controls.Count; i++)
		{
			Control control = controls[i];
			if (control != this && control is RadioButton)
			{
				RadioButton radioButton = (RadioButton)control;
				if (radioButton.autoCheck && radioButton.Checked)
				{
					radioButton.Checked = false;
				}
			}
		}
	}

	/// <summary>
	///  Removes tab stops from all radio buttons, other than the one that currently has the focus.
	/// </summary>
	void WipeTabStops(bool tabbedInto)
	{
		if (Parent is not { } parent)
		{
			return;
		}

		var children = parent.Controls;
		for (var i = 0; i < children.Count; i++)
		{
			if (children[i] is RadioButton button)
			{
				if (!tabbedInto)
				{
					button.firstFocus = false;
				}

				if (button.autoCheck)
				{
					button.TabStop = false;
				}
			}
		}
	}

	public Appearance Appearance { get; set; }

	protected override void OnClick(EventArgs e)
	{
		if (AutoCheck && Enabled)
		{
			Checked = true;
		}
		base.OnClick(e);
	}

	public void PerformClick()
	{
		if (CanSelect)
		{
			OnClick(EventArgs.Empty);
		}
	}

	protected internal override bool ProcessMnemonic(char charCode)
	{
		if (UseMnemonic && IsMnemonic(charCode, Text) && CanSelect)
		{
			PerformClick();
			return true;
		}

		return false;
	}

	protected override void OnEnter(EventArgs e)
	{
		if (Control.MouseButtons == MouseButtons.None)
		{
			if (((FindForm()?.LastFormKeyData ?? Keys.None) & Keys.Tab) != Keys.Tab)
			{
				// Entered the radio button by using arrow keys.
				if (!ValidationCancelled)
				{
					OnClick(e);
				}
			}
			else
			{
				// Entered the radio button by pressing Tab
				PerformAutoUpdates(true);

				// Reset the TabStop so we can come back later. PerformAutoUpdates will set TabStop to false.
				TabStop = true;
			}
		}

		base.OnEnter(e);
	}

	internal override Size GetPreferredSizeCore(Size proposedConstraints)
	{
		if (FlatStyle != FlatStyle.System)
		{
			return base.GetPreferredSizeCore(proposedConstraints);
		}

		Size textSize = TextRenderer.MeasureText(Text, Font);
		Size size = SizeFromClientSize(textSize);
		size.Width += FlatSystemStylePaddingWidth;
		size.Height = DpiHelper.IsScalingRequirementMet ? Math.Max(size.Height + 5, FlatSystemStyleMinimumHeight) : size.Height + 5; // ensure minimum height to avoid truncation of RadioButton circle or text
		return size;
	}

	internal override ButtonBaseAdapter CreateFlatAdapter()
	{
		return new RadioButtonFlatAdapter(this);
	}

	internal override ButtonBaseAdapter CreatePopupAdapter()
	{
		return new RadioButtonPopupAdapter(this);
	}

	internal override ButtonBaseAdapter CreateStandardAdapter()
	{
		return new RadioButtonStandardAdapter(this);
	}

	[DefaultValue(false)]
	public new bool TabStop
	{
		get => base.TabStop;
		set => base.TabStop = value;
	}

	/// <summary>
	///  Returns a string representation for this control.
	/// </summary>
	public override string ToString()
	{
		string s = base.ToString() ?? "";
		return s + ", Checked: " + Checked.ToString();
	}
}
