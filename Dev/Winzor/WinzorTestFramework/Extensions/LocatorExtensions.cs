#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace WinzorTestFramework;

[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Record")]
[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "This a not resource string context, this is effectively a false positive?")]
public record CSSValue(string Raw) : IEquatable<string>
{
	const double PtToPxConversionFactor = 0.75;

	public double AsPixels()
	{
		if (!Raw.EndsWith("px"))
		{
			throw new InvalidOperationException($"CSS value {Raw} is not a quantity of pixels");
		}

		return double.Parse(Raw.Substring(0, Raw.Length - 2));
	}

	[SuppressMessage("CargoWiseOne", "CW1068:Do Not Use Math.Round", Justification = "<Pending>")]
	public double AsPoints()
	{
		if (!Raw.EndsWith("px"))
		{
			throw new InvalidOperationException($"CSS value {Raw} is not a quantity of pixels, therefore cannot be converted to pt");
		}

		return Math.Round(double.Parse(Raw.Substring(0, Raw.Length - 2)) * PtToPxConversionFactor, 2);
	}

	public virtual bool Equals(string? other) => Equals(Raw, other);

	public virtual bool Equals(CSSValue? other) => other is not null && Equals(Raw, other.Raw);

	public override int GetHashCode()
	{
		return (Raw != null ? Raw.GetHashCode() : 0);
	}
	public override string ToString() => Raw;

	public static implicit operator string(CSSValue value) => value.Raw;
}

public static class LocatorExtensions
{
	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
	public static Task<bool> IsOverflowingAsync(this ILocator element) => element.EvaluateAsync<bool>("e => e.clientWidth < e.scrollWidth || e.clientHeight < e.scrollHeight;");

	public static async Task<string> TryGetAttributeAsync(this ILocator element, string attributeName)
	{
		try
		{
			return await element.GetAttributeAsync(attributeName) ?? string.Empty;
		}
		catch (KeyNotFoundException)
		{
			return string.Empty;
		}
	}

	public static async Task<CSSValue> GetComputedStyleAsync(this ILocator element, string? pseudoElement, string property) =>
		new((await element.EvaluateAsync<string>($"e => window.getComputedStyle(e, '{pseudoElement}').getPropertyValue('{property}')")).Trim());

	public static async Task<CSSValue> GetComputedStyleAsync(this ILocator element, string property) => await element.GetComputedStyleAsync(null, property);

	public static string GetStyleMessage(this ILocator element, string property) => $"{element} {{ {property} }}";
}
