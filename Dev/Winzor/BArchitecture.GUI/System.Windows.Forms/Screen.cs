using System.Drawing;

namespace System.Windows.Forms;

public class Screen
{
	Screen(Rectangle bounds)
	{
		Bounds = bounds;
	}

	public static Screen[] AllScreens => new[] { PrimaryScreen };

	public static Screen PrimaryScreen => new Screen(new Rectangle(0, 0, 3840, 2160));

	public int BitsPerPixel { get; }

	public Rectangle Bounds { get; }

	public Rectangle WorkingArea => Bounds;

	public static Rectangle GetWorkingArea(Control ctl) => Rectangle.Empty;

	public static Screen FromControl(Control control) => throw new NotImplementedException();
}
