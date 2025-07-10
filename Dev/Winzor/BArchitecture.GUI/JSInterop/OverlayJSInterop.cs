using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace WinzorFramework.JSInterop;

public interface IOverlayJSInterop : IJSInterop
{
	Task ForceCursorUpdateAsync(ElementReference overlayContainer);
}

public class OverlayJSInterop : JSInteropBase, IOverlayJSInterop
{
	public OverlayJSInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
		: base(jsRuntime, "/_content/WinzorFramework/js/module/overlay.js", fileVersionHash)
	{
	}

	public Task ForceCursorUpdateAsync(ElementReference overlayContainer)
	{
		try
		{
			return InvokeJsAsync("forceCursorUpdate", overlayContainer);
		}
		catch (JSException)
		{
			// This can happen if the screen switches forms quickly. Safe to ignore.
			return Task.CompletedTask;
		}
	}
}
