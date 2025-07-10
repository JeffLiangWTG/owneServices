using System.ComponentModel;

namespace System.Windows.Forms;

public class ScrollBar : Control
{
	public int Value { get; set; }

	public int Maximum { get; set; } = 100;

	[DefaultValue(false)]
	public new bool TabStop
	{
		get => base.TabStop;
		set => base.TabStop = value;
	}
}
