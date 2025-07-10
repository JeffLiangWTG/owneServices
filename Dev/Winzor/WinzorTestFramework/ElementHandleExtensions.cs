#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace WinzorTestFramework;

public static class ElementHandleExtensions
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
	public static Task<bool> IsOverflowingAsync(this IElementHandle element) => element.EvaluateAsync<bool>("e => e.clientWidth < e.scrollWidth || e.clientHeight < e.scrollHeight;");

	public static async Task<string> TryGetAttributeAsync(this IElementHandle element, string attributeName)
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

	public static async Task<CSSValue> GetComputedStyleAsync(this IElementHandle element, string? pseudoElement, string property) =>
		new ((await element.EvaluateAsync<string>($"e => window.getComputedStyle(e, '{pseudoElement}').getPropertyValue('{property}')")).Trim());

	public static async Task<CSSValue> GetComputedStyleAsync(this IElementHandle element, string property) => await element.GetComputedStyleAsync(null, property);

	public static async Task<IReadOnlyList<IElementHandle>> GetChildrenAsync(this IElementHandle element) => await element.QuerySelectorAllAsync("> *");
}
