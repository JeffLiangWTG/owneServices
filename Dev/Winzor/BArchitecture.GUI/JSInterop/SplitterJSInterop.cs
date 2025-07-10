using Microsoft.AspNetCore.Components;

namespace WinzorFramework.JSInterop;

public interface ISplitterJSInterop : IJSInterop
{
	Task MoveSplitterAsync(ElementReference splitterReference, bool isHorizontal, int minSizeBefore, int minSizeAfter);
}

public class SplitterJSInterop : JSInteropBase, ISplitterJSInterop
{
	public SplitterJSInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
		: base(jsRuntime, "/_content/WinzorFramework/js/module/splitter.js", fileVersionHash)
	{
	}

	public Task MoveSplitterAsync(ElementReference splitterReference, bool isHorizontal, int minSizeBefore, int minSizeAfter)
	{
		return InvokeJsAsync("moveSplitter", splitterReference, isHorizontal, minSizeBefore, minSizeAfter);
	}
}
