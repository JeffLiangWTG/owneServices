using SysColor = System.Drawing.Color;

namespace CargoWiseNext.Blazor.Components;

public static class SysColorExtensions
{
	public static string ToRgb(this SysColor color) => $"rgb({color.R}, {color.G}, {color.B})";
	public static string ToRgba(this SysColor color) => @$"rgba({color.R}, {color.G}, {color.B}, {color.A.ToDecimalAlpha():0.###})";
	public static decimal ToDecimalAlpha(this byte alpha) => alpha / 255m;
}
