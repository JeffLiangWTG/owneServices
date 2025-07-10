using System.Drawing;
using System.Windows.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WinzorFramework.Extensions;

namespace WinzorFramework.JSInterop;

public interface IGridJSInterop : IJSInterop
{
	Task InitializeGridEventsAsync(ElementReference grid, DotNetObjectReference<DataGrid> dotNet);
	Task MultiSelectionRowsAsync(DotNetObjectReference<DataGrid> grid, WebMouseEventArgs args, int lastSelectedRow, ElementReference dataGrid);
	Task ReorderColumnAsync(WebMouseEventArgs args, ElementReference column, Color dragMaskBackColor);
	Task ResizeColumnAsync(DotNetObjectReference<DataGrid> grid, WebMouseEventArgs args, ElementReference column);
	Task SetRowDragDataAsync(ElementReference grid, Dictionary<int, string> data);
	Task SetScrollLeftAsync(ElementReference dataGrid, int scrollLeft);
	Task SetScrollTopAsync(ElementReference dataGrid, int scrollTop);
	Task SelectAllAsync(ElementReference dataGrid);
}

public sealed class GridJSInterop : JSInteropBase, IGridJSInterop
{
	public GridJSInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
		: base(jsRuntime, "/_content/WinzorFramework/js/module/grid.js", fileVersionHash)
	{
	}

	public async Task InitializeGridEventsAsync(ElementReference grid, DotNetObjectReference<DataGrid> dotNet)
	{
		await InvokeJsAsync("initializeGridEvents", grid, dotNet);
	}

	public async Task ResizeColumnAsync(DotNetObjectReference<DataGrid> grid, WebMouseEventArgs args, ElementReference column)
	{
		await InvokeJsAsync("resizeColumn", grid, args, column);
	}

	public async Task ReorderColumnAsync(WebMouseEventArgs args, ElementReference column, Color dragMaskBackColor)
	{
		await InvokeJsAsync("reorderColumn", args, column, dragMaskBackColor.GetColorStyleValue());
	}

	public async Task SetRowDragDataAsync(ElementReference grid, Dictionary<int, string> data)
	{
		await InvokeJsAsync("setRowDragData", grid, data);
	}

	public async Task MultiSelectionRowsAsync(DotNetObjectReference<DataGrid> grid, WebMouseEventArgs args, int lastSelectedRow, ElementReference dataGrid)
	{
		await InvokeJsAsync("multiSelectionRows", grid, args, lastSelectedRow, dataGrid);
	}

	public async Task SetScrollTopAsync(ElementReference dataGrid, int scrollTop)
	{
		await InvokeJsAsync("setScrollTop", dataGrid, scrollTop);
	}

	public async Task SetScrollLeftAsync(ElementReference dataGrid, int scrollLeft)
	{
		await InvokeJsAsync("setScrollLeft", dataGrid, scrollLeft);
	}

	public async Task SelectAllAsync(ElementReference dataGrid)
	{
		await InvokeJsAsync("selectAll", dataGrid);
	}
}
