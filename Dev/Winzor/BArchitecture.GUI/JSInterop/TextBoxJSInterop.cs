using Microsoft.AspNetCore.Components;

namespace WinzorFramework.JSInterop;

public interface ITextBoxJSInterop : ITextBoxBaseJSInterop
{
	Task FocusAsync(ElementReference reference);
	Task<int[]> GetSelectionAsync(ElementReference reference);
	Task RaiseWarningAsync(ElementReference reference, bool capsKeyPressed);
	Task SelectAllAsync(ElementReference reference);
	Task SetTextContentAsync(ElementReference reference, string text);
	Task SetTextAndSelectionAsync(ElementReference reference, string text, int start, int end);
	Task ValidateTextOnKeyPressAsync(ElementReference reference, string matchExpression, Dictionary<char, char>? replacementCharacters = null);
}

public sealed class TextBoxJSInterop : JSInteropBase, ITextBoxJSInterop
{
	public TextBoxJSInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
		: base(jsRuntime, "/_content/WinzorFramework/js/module/textbox.js", fileVersionHash)
	{
	}

	public async Task SetSelectionAsync(ElementReference reference, int start, int end)
	{
		await InvokeJsAsync("setSelection", reference, start, end);
	}

	public async Task SetTextContentAsync(ElementReference reference, string text)
	{
		await InvokeJsAsync(SetTextContentIdentifier, reference, text);
	}

	public async Task SelectAllAsync(ElementReference reference)
	{
		await InvokeJsAsync("selectAll", reference);
	}

	public async Task SetTextAndSelectionAsync(ElementReference reference, string text, int start, int end)
	{
		await InvokeJsAsync("setTextAndSelection", reference, text, start, end);
	}

	public async Task FocusAsync(ElementReference reference)
	{
		await InvokeJsAsync("focus", reference);
	}

	public async Task<int[]> GetSelectionAsync(ElementReference reference)
	{
		return await InvokeJsAsync<int[]>("getSelection", reference);
	}

	public async Task RaiseWarningAsync(ElementReference reference, bool capsKeyPressed)
	{
		await InvokeJsAsync("raiseWarning", reference, capsKeyPressed);
	}

	public const string SetTextContentIdentifier = "setTextContent";

	public async Task ValidateTextOnKeyPressAsync(ElementReference reference, string matchExpression, Dictionary<char, char>? replacementCharacters = null)
	{
		await InvokeJsAsync("validateTextOnKeyPress", reference, matchExpression, replacementCharacters);
	}

	public async Task ScrollToCaretAsync(ElementReference reference, int start, int end, int textLength)
	{
		await InvokeJsAsync("scrollToCaret", reference);
	}
}
