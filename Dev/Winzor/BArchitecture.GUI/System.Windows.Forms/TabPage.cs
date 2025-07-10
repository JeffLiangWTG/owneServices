using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using WinzorFramework.Extensions;

namespace System.Windows.Forms;

public partial class TabPage : Panel
{
	public TabPage()
	{
	}

	public TabPage(string text)
		: this()
	{
		Text = text;
	}

	public override bool UseParentDivForLayout => false;

	protected internal override bool StopLeaveOnNoActiveControl => true;

	public int ImageIndex
	{
		get => imageIndex;
		set
		{
			if (UpdateProperty(ref imageIndex, value))
			{
				Parent?.NotifyRenderRequired();
				RecalculateTabWidth();
			}
		}
	}

	int imageIndex = -1;

	public int IconIndex
	{
		get => iconIndex;
		set
		{
			if (UpdateProperty(ref iconIndex, value))
			{
				Parent?.NotifyRenderRequired();
			}
		}
	}

	int iconIndex = -1;

	public Color? CaptionBackgroundColor
	{
		get => captionBackgroundColor;
		set
		{
			if (UpdateProperty(ref captionBackgroundColor, value))
			{
				Parent?.NotifyRenderRequired();
			}
		}
	}

	Color? captionBackgroundColor;

	public string CaptionBackgroundStyleString
	{
		get
		{
			if (CaptionBackgroundColor is Color background)
			{
				return $"background-color: rgb({background.R}, {background.G}, {background.B})";
			}
			return string.Empty;
		}
	}

	public string? ImageKey { get; set; }

	public int RectLeft {  get; set; }

	public bool UseVisualStyleBackColor { get; set; }

	protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
	{
		if (Parent is TabControl && Parent.IsHandleCreated)
		{
			var r = Parent.DisplayRectangle;

			// LayoutEngines send BoundsSpecified.None so they can know they are the ones causing the size change
			// in the subsequent InitLayout. We need to be careful preserve a None.
			base.SetBoundsCore(r.X, r.Y, r.Width, r.Height, specified == BoundsSpecified.None ? BoundsSpecified.None : BoundsSpecified.All);
		}
		else
		{
			base.SetBoundsCore(x, y, width, height, specified);
		}
	}

	/// <summary>
	///  This is an internal method called by the TabControl to fire the Leave event when TabControl leave occurs.
	/// </summary>
	internal void FireLeave(EventArgs e)
	{
		leaveFired = true;
		OnLeave(e);
	}

	/// <summary>
	///  This is an internal method called by the TabControl to fire the Enter event when TabControl leave occurs.
	/// </summary>
	internal void FireEnter(EventArgs e)
	{
		enterFired = true;
		OnEnter(e);
	}

	/// <summary>
	///  Actually goes and fires the OnEnter event. Inheriting controls should use this to know
	///  when the event is fired [this is preferable to adding an event handler on yourself for
	///  this event]. They should, however, remember to call base.OnEnter(e); to ensure the event
	///  i still fired to external listeners
	///  This listener is overidden so that we can fire SAME ENTER and LEAVE events on the TabPage.
	///  TabPage should fire enter when the focus is on the TabPage and not when the control
	///  within the TabPage gets Focused.
	/// </summary>
	protected override void OnEnter(EventArgs e)
	{
		if (Parent is TabControl)
		{
			if (enterFired)
			{
				base.OnEnter(e);
			}

			enterFired = false;
		}
	}
	bool enterFired;

	/// <summary>
	///  Actually goes and fires the OnLeave event. Inheriting controls should use this to know
	///  when the event is fired [this is preferable to adding an event handler on yourself for
	///  this event]. They should, however, remember to call base.OnLeave(e); to ensure the event
	///  is still fired to external listeners
	///  This listener is overidden so that we can fire same enter and leave events on the TabPage.
	///  TabPage should fire enter when the focus is on the TabPage and not when the control within
	///  the TabPage gets Focused.
	///  Similary the Leave should fire when the TabControl (and hence the TabPage) loses focus.
	/// </summary>
	protected override void OnLeave(EventArgs e)
	{
		if (Parent is TabControl)
		{
			if (leaveFired)
			{
				base.OnLeave(e);
			}

			leaveFired = false;
		}
	}
	bool leaveFired;

	protected override void OnTextChanged(EventArgs e)
	{
		base.OnTextChanged(e);
		(Parent as TabControl)?.NotifyRenderRequired();
		RecalculateTabWidth();
	}

	void RecalculateTabWidth()
	{
		var imageWidth = 0;
		if (ImageIndex != -1)
		{
			imageWidth = ((TabControl?)Parent)?.ImageList?.Images[ImageIndex]?.Width ?? 0;

			if (imageWidth > 0 && Text.Length > 0)
			{
				//Add gap between icon and text.
				imageWidth += 4;
			}
		}

		DynamicTabWidth = TextRenderer.MeasureText(Text, Font).Width + imageWidth + 8;
	}

	internal int DynamicTabWidth; //Generated tab width for normal TabDrawMode.

	[AllowNull]
	public override string Text
	{
		get => base.Text;
		set
		{
			base.Text = value;
			UpdateParent();
		}
	}

	void UpdateParent()
	{
		if (Parent is TabControl parent)
		{
			parent.UpdateTab(this);
		}
	}

	public string TabPageStyleString(bool useBackColor)
	{
		var backgroundColorStyleString = this.BackgroundColorStyleString();
		if (useBackColor)
		{
			return ControlStyleString.Replace(backgroundColorStyleString, string.Empty);
		}
		return ControlStyleString;
	}
}
