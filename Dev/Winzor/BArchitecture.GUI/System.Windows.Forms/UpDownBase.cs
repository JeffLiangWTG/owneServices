using System.Drawing;

namespace System.Windows.Forms;

public abstract class UpDownBase : ContainerControl
{
	const int DefaultControlWidth = 120;

	public bool ReadOnly
	{
		get => readOnly;
		set => UpdateProperty(ref readOnly, value);
	}
	bool readOnly;

	public HorizontalAlignment TextAlign { get; set; }

	public abstract void DownButton();

	public abstract void UpButton();

	protected override Size DefaultSize => new Size(DefaultControlWidth, PreferredHeight);

	public int PreferredHeight => FontHeight;

	protected override bool AllowTextChangeFromClient => base.AllowTextChangeFromClient && !ReadOnly;
}
