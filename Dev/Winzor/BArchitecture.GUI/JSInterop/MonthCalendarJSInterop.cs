using System.Windows.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace WinzorFramework.JSInterop;

public interface IMonthCalendarJSInterop : IJSInterop
{
	Task ShowCalendarAsync(ElementReference element, DotNetObjectReference<MonthCalendar> dotnet);
}

public class MonthCalendarJSInterop : JSInteropBase, IMonthCalendarJSInterop
{
	public MonthCalendarJSInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
		: base(jsRuntime, "/_content/WinzorFramework/js/module/monthCalendar.js", fileVersionHash)
	{
	}

	public async Task ShowCalendarAsync(ElementReference element, DotNetObjectReference<MonthCalendar> dotnet)
	{
		await InvokeJsAsync("showCalendar", element, dotnet);
	}
}
