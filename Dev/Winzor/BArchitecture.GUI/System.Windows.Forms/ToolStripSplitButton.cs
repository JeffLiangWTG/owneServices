using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace System.Windows.Forms;

public partial class ToolStripSplitButton : ToolStripDropDownItem
{
	protected override bool DefaultAutoToolTip => true;

	public override bool UseParentDivForLayout => false;

	public int DropDownButtonWidth { get; set; }

	public void PerformButtonClick()
	{
		OnButtonClick(EventArgs.Empty);
	}

	[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in ToolStripSplitButton.razor")]
	Task OnButtonClickAsync(WebMouseEventArgs e)
	{
		return InvokeWinzorDispatcherAsync(() => OnButtonClick(EventArgs.Empty));
	}

	protected virtual void OnButtonClick(EventArgs e)
	{
		if (!Enabled)
		{
			return;
		}

		Owner?.OnItemClicked(this);
		ButtonClick?.Invoke(this, e);
	}

	public event EventHandler? ButtonClick;

	internal int SplitterWidth
	{
		get => splitterWidth;
		set
		{
			if (value < 0)
			{
				splitterWidth = 0;
			}
			else
			{
				splitterWidth = value;
			}
		}
	}
	int splitterWidth = 1;

	public override Size GetPreferredSize(Size constrainingSize)
	{
		var preferredSize = base.GetPreferredSize(constrainingSize);
		preferredSize.Width += DropDownButtonWidth + SplitterWidth + Padding.Horizontal;
		return preferredSize;
	}

	public bool ButtonPressed
	{
		get
		{
			return SplitButtonButton.Pressed;
		}
	}

	public bool ButtonSelected
	{
		get
		{
			return SplitButtonButton.Selected || DropDownButtonPressed;
		}
	}

	public bool DropDownButtonPressed
	{
		get
		{
			return DropDown.Visible;
		}
	}

	public bool DropDownButtonSelected
	{
		get
		{
			return Selected;
		}
	}

	protected internal override bool ProcessMnemonic(char charCode)
	{
		if (CanProcessMnemonic() && IsMnemonic(charCode, Text))
		{
			PerformButtonClick();
			return true;
		}

		return base.ProcessMnemonic(charCode);
	}

	ToolStripSplitButtonButton? splitButtonButton;

	/// <summary>
	///  Just used as a convenience to help manage layout
	/// </summary>
	ToolStripSplitButtonButton SplitButtonButton
	{
		get
		{
			splitButtonButton ??= new ToolStripSplitButtonButton(this);

			splitButtonButton.Image = Image;
			splitButtonButton.Text = Text;
			splitButtonButton.BackColor = BackColor;
			splitButtonButton.ForeColor = ForeColor;
			splitButtonButton.Font = Font;
			splitButtonButton.ImageAlign = ImageAlign;
			splitButtonButton.TextAlign = TextAlign;
			splitButtonButton.TextImageRelation = TextImageRelation;
			return splitButtonButton;
		}
	}

	/// <summary>
	///  This class represents the item to the left of the dropdown [ A |v]  (e.g the "A")
	///  It exists so that we can use our existing methods for text and image layout
	///  and have a place to stick certain state information like pushed and selected
	///  Note since this is NOT an actual item hosted on the ToolStrip - it won't get things
	///  like MouseOver, won't be laid out by the ToolStrip, etc etc.  This is purely internal
	///  convenience.
	/// </summary>
	class ToolStripSplitButtonButton : ToolStripButton
	{
		readonly ToolStripSplitButton owner;

		public ToolStripSplitButtonButton(ToolStripSplitButton owner)
		{
			this.owner = owner;
		}

		public override bool Enabled
		{
			get
			{
				return owner.Enabled;
			}
			set
			{
				// do nothing
			}
		}

		public override ToolStripItemDisplayStyle DisplayStyle
		{
			get
			{
				return owner.DisplayStyle;
			}
			set
			{
				// do nothing
			}
		}

		public override Padding Padding
		{
			get
			{
				return owner.Padding;
			}
			set
			{
				// do nothing
			}
		}

		public override ToolStripTextDirection TextDirection
		{
			get
			{
				return owner.TextDirection;
			}
		}

		public override Image? Image
		{
			get
			{
				if ((owner.DisplayStyle & ToolStripItemDisplayStyle.Image) == ToolStripItemDisplayStyle.Image)
				{
					return owner.Image;
				}
				else
				{
					return null;
				}
			}
			set
			{
				// do nothing
			}
		}

		public override bool Selected
		{
			get
			{
				if (owner is not null)
				{
					return owner.Selected;
				}

				return base.Selected;
			}
		}

		[AllowNull]
		public override string Text
		{
			get
			{
				if ((owner.DisplayStyle & ToolStripItemDisplayStyle.Text) == ToolStripItemDisplayStyle.Text)
				{
					return owner.Text;
				}
				else
				{
					return string.Empty;
				}
			}
			set
			{
				// do nothing
			}
		}
	}
}
