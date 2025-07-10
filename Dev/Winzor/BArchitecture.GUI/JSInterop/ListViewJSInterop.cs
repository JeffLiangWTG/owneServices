using System.Windows.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace WinzorFramework.JSInterop;

public interface IListViewJSInterop : IJSInterop
{
	Task ResizeColumnAsync(DotNetObjectReference<ListView> listView, WebMouseEventArgs args, ElementReference column);
}

public sealed class ListViewJSInterop : JSInteropBase, IListViewJSInterop
{
	public ListViewJSInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
		: base(jsRuntime, "/_content/WinzorFramework/js/module/listView.js", fileVersionHash)
	{
	}

	public async Task ResizeColumnAsync(DotNetObjectReference<ListView> listView, WebMouseEventArgs args, ElementReference column)
	{
		await InvokeJsAsync("resizeColumn", listView, args, column);
	}
}
