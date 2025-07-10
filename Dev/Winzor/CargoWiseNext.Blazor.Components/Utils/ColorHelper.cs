using System.Drawing;
using SysColor = System.Drawing.Color;

namespace CargoWiseNext.Blazor.Components.Utils;

public static class ColorHelper
{
	public static byte GetAlpha(double value)
	{
		return (byte)((value * 255.0).EnsureRange(255));
	}

	public static SysColor SetAlpha(this SysColor color, double alpha)
	{
		return SysColor.FromArgb(GetAlpha(alpha), color.R, color.G, color.B);
	}

	public static SysColor ConvertToColor(this string hex)
	{
		return ColorTranslator.FromHtml(hex);
	}

	public static SysColor Lighten(this SysColor color)
	{
		return color.Lighten(0.075);
	}

	public static SysColor Lighten(this SysColor color, double amount)
	{
		var h = color.GetHue();
		var s = color.GetSaturation();
		var l = color.GetBrightness();
		var a = color.A;
		l = (float)Math.Max(0, Math.Min(1, l + amount));
		return HSLToColor(h, s, l, a);
	}

	public static SysColor Darken(this SysColor color)
	{
		return color.Darken(0.075);
	}

	public static SysColor Darken(this SysColor color, double amount)
	{
		return Lighten(color, -amount);
	}

	static SysColor HSLToColor(float h, float s, float l, int a)
	{
		double c = (1 - Math.Abs(2 * l - 1)) * s;
		double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
		double m = l - c / 2;

		double r = 0, g = 0, b = 0;

		if (0 <= h && h < 60)
		{
			r = c;
			g = x;
			b = 0;
		}
		else if (60 <= h && h < 120)
		{
			r = x;
			g = c;
			b = 0;
		}
		else if (120 <= h && h < 180)
		{
			r = 0;
			g = c;
			b = x;
		}
		else if (180 <= h && h < 240)
		{
			r = 0;
			g = x;
			b = c;
		}
		else if (240 <= h && h < 300)
		{
			r = x;
			g = 0;
			b = c;
		}
		else if (300 <= h && h < 360)
		{
			r = c;
			g = 0;
			b = x;
		}

		int rInt = (int)((r + m) * 255);
		int gInt = (int)((g + m) * 255);
		int bInt = (int)((b + m) * 255);

		return SysColor.FromArgb(a,rInt, gInt, bInt);
	}
}

