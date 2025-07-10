using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace System.Drawing;

public class BGraphics : IDisposable
{
	public SizeF MeasureString(string text, Font font)
	{
		if (string.IsNullOrEmpty(text))
		{
			return SizeF.Empty;
		}

		text = ReplaceNewLines(text);
		return AdjustHeight(TextRenderer.MeasureText(text, font));
	}

	public SizeF MeasureString(string text, Font font, int width)
	{
		if (string.IsNullOrEmpty(text))
		{
			return SizeF.Empty;
		}

		text = ReplaceNewLines(text);
		return AdjustHeight(TextRenderer.MeasureText(text, font, width));
	}

	public SizeF MeasureString(string text, Font font, Size size)
	{
		if (string.IsNullOrEmpty(text))
		{
			return SizeF.Empty;
		}

		text = ReplaceNewLines(text);
		var result = TextRenderer.MeasureText(text, font, size, TextFormatFlags.WordBreak);
		result = AdjustHeight(result);
		result.Height = Math.Min(result.Height, size.Height);
		result.Width = Math.Min(result.Width, size.Width);

		return result;
	}

	Size AdjustHeight(Size size)
	{
		//After testing, the ratio between the results of TextRender.MeasureText and Graphics.MeasureText (GDI code) is 1.1.
		size.Height = (int)(size.Height * 1.1);
		return size;
	}

	public static string ReplaceNewLines(string text)
	{
		return Regex.Replace(text, "(?<!\r)\n", "\r\n");
	}

	public void DrawIconUnstretched(Icon icon, Rectangle targetRect)
	{
	}

	public void DrawImageUnscaled(Image image, int x, int y)
	{
	}

	public void DrawImageUnscaled(Image image, Rectangle rect)
	{
	}

	public void DrawLine(Pen pen, Point pt1, Point pt2)
	{
	}

	public void FillRectangle(Brush brush, Rectangle rect)
	{
	}

	public float DpiX { get; }

	public float DpiY { get; }

	public void Dispose()
	{
	}

	public GraphicsUnit PageUnit { get; set; }
}
