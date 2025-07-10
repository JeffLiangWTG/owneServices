using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace WinzorFramework.Extensions;

public static class ElementReferenceExtension
{
	internal static string SupressFocusEventDataAttribute = "data-server-initiated-focus";

	public static async Task<bool> TryFocusOnClientAsync(this ElementReference element, IJSRuntime jsRuntime)
	{
		return await jsRuntime.InvokeAsync<bool>("tryFocusFromServer", !default(ElementReference).Equals(element) ? element : null);
	}

	public static async Task ScrollToAsync(this ElementReference element, IJSRuntime? jsRuntime, double x, double y, bool animated = false)
	{
		if (jsRuntime is not null)
		{
			await jsRuntime.InvokeVoidAsync("scrollElementTo", element, x, y, animated);
		}
	}

	/// <summary>
	/// Invokes client side javascript to scroll the element into view
	/// Will fallback to scrollIntoView if scrollIntoViewIfNeeded is unavailable
	/// </summary>
	/// <param name="element"></param>
	/// <param name="jsRuntime"></param>
	public static async Task ScrollElementIntoViewIfNeededAsync(this ElementReference element, IJSRuntime jsRuntime)
	{
		if (jsRuntime is not null)
		{
			await jsRuntime.InvokeVoidAsync("scrollIntoViewIfNeeded", element);
		}
	}

	public static ElementReference? ElementReferenceOrNull(this ElementReference element) => !default(ElementReference).Equals(element) ? element : null;
}
