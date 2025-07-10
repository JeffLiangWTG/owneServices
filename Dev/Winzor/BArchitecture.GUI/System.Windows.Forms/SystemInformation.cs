using System.Drawing;

namespace System.Windows.Forms;

public static class SystemInformation
{
	public static int VerticalScrollBarWidth => 17;

	public static int VerticalScrollBarArrowHeight => 0;

	public static int HorizontalScrollBarHeight => 17;

	public static int DoubleClickTime => 0;

	public static int CaptionHeight => 0;

	public static Size BorderSize => new Size(1, 1);

	public static bool HighContrast => false;

	public static bool IsDropShadowEnabled => false;

	public static bool MenuAccessKeysUnderlined => false;
}
