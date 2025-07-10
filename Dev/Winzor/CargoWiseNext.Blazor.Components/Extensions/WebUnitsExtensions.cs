using System.Globalization;

namespace CargoWiseNext.Blazor.Components;

public static class WebUnitsExtensions
{
	public static string ToPx(this int val) => $"{val}px";
	public static string ToPx(this int? val) => val != null ? val.Value.ToPx() : string.Empty;
	public static string ToPx(this long val) => $"{val}px";
	public static string ToPx(this long? val) => val != null ? val.Value.ToPx() : string.Empty;
	public static string ToPx(this double val) => $"{val.ToString("0.##", CultureInfo.InvariantCulture)}px";
	public static string ToPx(this double? val) => val != null ? val.Value.ToPx() : string.Empty;
}

