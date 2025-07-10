using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace WinzorFramework.JSInterop;

public interface IPopupJSInterop : IJSInterop
{
	Task<IJSObjectReference> AttachPopupAsync(ElementReference popupElement, ElementReference anchorElement);
}

public sealed class PopupJSInterop : JSInteropBase, IPopupJSInterop
{
	public PopupJSInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
		: base(jsRuntime, "/_content/WinzorFramework/js/module/popup.js", fileVersionHash)
	{
	}

	public async Task<IJSObjectReference> AttachPopupAsync(ElementReference popupElement, ElementReference anchorElement)
	{
		var jsObjectReference = await InvokeJsAsync<IJSObjectReference>("attachPopup", popupElement, anchorElement);
		return jsObjectReference;
	}
}
