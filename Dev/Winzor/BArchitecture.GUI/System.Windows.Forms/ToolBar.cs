using System.Drawing;

namespace System.Windows.Forms;

public partial class ToolBar : Control
{
	public ToolBar()
	{
		buttonsCollection = new ToolBarButtonCollection();
		Dock = DockStyle.Top;
	}

	public ToolBarButtonCollection Buttons
	{
		get
		{
			return buttonsCollection;
		}
	}

	public Size ButtonSize { get; set; }

	public ToolBarAppearance Appearance { get; set; }

	public ImageList? ImageList { get; set; }

	public bool Wrappable { get; set; }

	protected virtual void OnButtonClick(ToolBarButtonClickEventArgs e)
	{
		if (!Enabled)
		{
			return;
		}
		ButtonClick?.Invoke(this, e);
	}

	protected virtual void OnButtonDropDown(ToolBarButtonClickEventArgs e)
	{
	}

	public bool Divider { get; set; }

	public bool DropDownArrows { get; set; }

	public bool ShowToolTips { get; set; }

	public event ToolBarButtonClickEventHandler? ButtonClick;

	readonly ToolBarButtonCollection buttonsCollection;

	protected override Size DefaultSize => new Size(100, 22);
}
