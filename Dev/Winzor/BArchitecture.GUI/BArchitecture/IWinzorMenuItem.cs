using System.Drawing;
using System.Windows.Forms;

namespace WinzorFramework;

public interface IWinzorMenuItem
{
	public Guid WinzorControlGuid { get; }

	public bool Visible { get; }

	public bool Checked { get; }

	public string Text { get; }

	public IWinzorMenuItem[] MenuItems { get; }

	public void PerformClick();

	public void OnSelect();

	public void OnImageMouseEnter();

	public void OnImageMouseLeave();

	public bool Loadable { get; }

	public bool Enabled { get; }

	public Image? Image { get; }

	public Font Font { get; }

	public Shortcut Shortcut { get; }

	public string? ShortcutString { get; }

	public void ShowShortcutString();

	public bool HandleAsSelectable { get; set; }

	public string? ToolTipText { get; set; }

	public bool Clickable { get; }

	public Image? HoverImage { get; set; }

	public string? HoverToolTipText { get; set; }
}
