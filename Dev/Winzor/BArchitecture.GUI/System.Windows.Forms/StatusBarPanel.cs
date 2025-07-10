using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace System.Windows.Forms;

public partial class StatusBarPanel : Control, ISupportInitialize
{
	public override bool UseParentDivForLayout => false;

	public StatusBarPanelStyle Style { get; set; }

	public StatusBarPanelBorderStyle BorderStyle { get; set; }

	public new StatusBarPanelAutoSize AutoSize { get; set; }

	public int MinWidth { get; set; }

	protected override Size DefaultSize => new Size(100, 20);

	public Icon? Icon { get; set; }

	public string text = string.Empty;

	object? userData;

	public override Color BackColor
	{
		get
		{
			if (ShouldSerializeBackColor())
			{
				return base.BackColor;
			}

			return SystemColors.Control;
		}
		set => base.BackColor = value;
	}

	public new object? Tag
	{
		get
		{
			return userData;
		}
		set
		{
			userData = value;
		}
	}

	public HorizontalAlignment Alignment
	{
		get
		{
			return alignment;
		}

		set
		{
			if (!ClientUtils.IsEnumValid(value, (int)value, (int)HorizontalAlignment.Left, (int)HorizontalAlignment.Center))
			{
				throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(HorizontalAlignment));
			}
			if (alignment != value)
			{
				alignment = value;
				Realize();
			}
		}
	}

	[AllowNull]
	public override string Text
	{
		get => text;
		set
		{
			if (UpdateProperty(ref text, value ?? string.Empty))
			{
				OnTextChanged(EventArgs.Empty);
			}
			this.Realize();
		}
	}

	string toolTipText = string.Empty;

	public override string? ToolTipText
	{
		get => toolTipText;
		set
		{
			if (UpdateProperty(ref toolTipText, value ?? string.Empty))
			{
				OnTextChanged(EventArgs.Empty);
			}
		}
	}

	public void BeginInit()
	{
	}

	public void EndInit()
	{
	}

	public HorizontalAlignment alignment = HorizontalAlignment.Left;

	public StatusBar? parent;

	internal void Realize()
	{
		if (Created)
		{
			string text;
			string sendText;

			if (this.text == null)
			{
				text = string.Empty;
			}
			else
			{
				text = this.text;
			}

			HorizontalAlignment align = alignment;

			// Translate the alignment for Rtl apps
			if (parent?.RightToLeft == RightToLeft.Yes)
			{
				switch (align)
				{
					case HorizontalAlignment.Left:
						align = HorizontalAlignment.Right;
						break;
					case HorizontalAlignment.Right:
						align = HorizontalAlignment.Left;
						break;
				}
			}

			switch (align)
			{
				case HorizontalAlignment.Center:
					sendText = "\t" + text;
					break;
				case HorizontalAlignment.Right:
					sendText = "\t\t" + text;
					break;
				default:
					sendText = text;
					break;
			}
		}
	}

	/// <summary>
	///  Retrieves a string that contains information about the
	///  panel.
	/// </summary>
	public override string ToString()
	{
		return "StatusBarPanel: {" + Text + "}";
	}
}
