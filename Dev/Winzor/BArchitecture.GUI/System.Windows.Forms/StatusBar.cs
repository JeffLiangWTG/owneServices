using System.ComponentModel;
using System.Drawing;
using System.Globalization;

namespace System.Windows.Forms;

public partial class StatusBar : Control
{
	public StatusBar()
	{
		SetStyle(ControlStyles.Selectable, false);
		Dock = DockStyle.Bottom;
		TabStop = false;
	}

	protected override Size DefaultSize => new Size(100, 22);

	public override bool UseParentDivForLayout => false;

	public bool SizingGrip { get; set; }

	public bool ShowPanels { get; set; }

	public string StatusBarBorderStyleString { get; set; } = string.Empty;

	public StatusBarPanelCollection Panels { get; } = new StatusBarPanelCollection();

	[DefaultValue(false)]
	public new bool TabStop
	{
		get => base.TabStop;
		set => base.TabStop = value;
	}

	[DefaultValue(DockStyle.Bottom)]
	public override DockStyle Dock
	{
		get => base.Dock;
		set => base.Dock = value;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			foreach (var panel in Panels)
			{
				panel.Dispose();
			}
		}
		base.Dispose(disposing);
	}

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

	event StatusBarDrawItemEventHandler? drawItem;
	public event StatusBarDrawItemEventHandler? DrawItem
	{
		add
		{
			drawItem += value;
		}
		remove
		{
			drawItem -= value;
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		drawItem?.Invoke(this, new StatusBarDrawItemEventArgs());
	}

	/// <summary>
	///  Returns a string representation for this control.
	/// </summary>
	public override string ToString()
	{
		string s = base.ToString() ?? string.Empty;
		if (Panels != null)
		{
			s += ", Panels.Count: " + Panels.Count.ToString(CultureInfo.CurrentCulture);
			if (Panels.Count > 0)
			{
				s += ", Panels[0]: " + Panels[0].ToString();
			}
		}
		return s;
	}
}
