using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms;

public class NotifyIcon : Control
{
	public NotifyIcon()
	{
	}

	public NotifyIcon(IContainer container)
	{
	}

	public Icon? Icon;

	public event EventHandler? BalloonTipClicked;

	public event EventHandler? BalloonTipClosed;

	public void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon)
	{
	}
}
