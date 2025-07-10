using System.Drawing;

namespace System.Windows.Forms;

public class PaintEventArgs : EventArgs
{
	public PaintEventArgs(BGraphics graphics, Rectangle clipRect)
	{
		Graphics = graphics;
		ClipRectangle = clipRect;
	}

	public PaintEventArgs(Control parentControl)
	{
		Graphics = new BGraphics();
		ClipRectangle = new Rectangle(Point.Empty, parentControl.Size);
	}

	public BGraphics Graphics { get; }

	public Rectangle ClipRectangle { get; }
}
