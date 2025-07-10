using Microsoft.AspNetCore.Components;

namespace WinzorFramework.JSInterop;

public interface IClipboardJSInterop : IJSInterop
{
	Task CopyAsync(ElementReference? element = null);
	Task CutAsync(ElementReference? element = null);
	Task PasteAsync(ElementReference? element = null);
	Task PastePlainTextAsync(ElementReference? element = null);
	Task<AvailableClipboardActions> GetAvailableActionsAsync(ElementReference? element = null);
}

public sealed class ClipboardJSInterop : JSInteropBase, IClipboardJSInterop
{
	public ClipboardJSInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
		: base(jsRuntime, "/_content/WinzorFramework/js/module/clipboard.js", fileVersionHash)
	{
	}

	public async Task CopyAsync(ElementReference? element = null)
	{
		await InvokeJsAsync("clipboard.copy", element);
	}

	public async Task CutAsync(ElementReference? element = null)
	{
		await InvokeJsAsync("clipboard.cut", element);
	}

	public async Task PasteAsync(ElementReference? element = null)
	{
		await InvokeJsAsync("clipboard.paste", element);
	}

	public async Task PastePlainTextAsync(ElementReference? element = null)
	{
		await InvokeJsAsync("clipboard.pastePlainText", element);
	}

	public async Task<AvailableClipboardActions> GetAvailableActionsAsync(ElementReference? element = null)
	{
		return await InvokeJsAsync<AvailableClipboardActions>("clipboard.getSupportedActions", element);
	}
}

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
public record AvailableClipboardActions(bool AllowCut, bool AllowCopy, bool AllowPaste);
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter

