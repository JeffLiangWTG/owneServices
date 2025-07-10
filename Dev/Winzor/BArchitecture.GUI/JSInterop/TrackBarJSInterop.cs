using Microsoft.AspNetCore.Components;

namespace WinzorFramework.JSInterop;

public interface ITrackBarJSInterop : IJSInterop
{
	Task UpdateRangeInputStepAsync(ElementReference trackbar, int arrowKeyIncrement, int pageUpDownIncrement);
	Task UpdateTrackBarColorOnDragAsync(ElementReference trackbar);
}

public sealed class TrackBarJSInterop : JSInteropBase, ITrackBarJSInterop
{
	public TrackBarJSInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
		: base(jsRuntime, "/_content/WinzorFramework/js/module/trackbar.js", fileVersionHash)
	{
	}

	public async Task UpdateRangeInputStepAsync(ElementReference trackbar, int arrowKeyIncrement, int pageUpDownIncrement)
	{
		await InvokeJsAsync("updateRangeInputStep", trackbar, arrowKeyIncrement, pageUpDownIncrement);
	}
	public async Task UpdateTrackBarColorOnDragAsync(ElementReference trackbar)
	{
		await InvokeJsAsync("updateTrackBarColorOnDrag", trackbar);
	}
}
