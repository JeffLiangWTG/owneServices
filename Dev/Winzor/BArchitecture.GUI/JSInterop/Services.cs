using Microsoft.Extensions.DependencyInjection;

namespace WinzorFramework.JSInterop;

public static class Services
{
	public static void AddJSInteropServices(this IServiceCollection services)
	{
		services.AddScoped<IClientEventServiceJSInterop, ClientEventServiceJSInterop>();
		services.AddScoped<IClipboardJSInterop, ClipboardJSInterop>();
		services.AddScoped<IElementJSInterop, ElementJSInterop>();
		services.AddScoped<IFileServiceJSInterop, FileServiceJSInterop>();
		services.AddScoped<IFormJSInterop, FormJSInterop>();
		services.AddScoped<IGridJSInterop, GridJSInterop>();
		services.AddScoped<IListViewJSInterop, ListViewJSInterop>();
		services.AddScoped<IMonthCalendarJSInterop, MonthCalendarJSInterop>();
		services.AddScoped<IOverlayJSInterop, OverlayJSInterop>();
		services.AddScoped<IPopupJSInterop, PopupJSInterop>();
		services.AddScoped<IRichTextBoxJSInterop, RichTextBoxJSInterop>();
		services.AddScoped<ISplitterJSInterop, SplitterJSInterop>();
		services.AddScoped<ITextBoxJSInterop, TextBoxJSInterop>();
		services.AddScoped<ITrackBarJSInterop, TrackBarJSInterop>();
		services.AddScoped<ITreeViewJSInterop, TreeViewJSInterop>();
		services.AddScoped<IWindowJSInterop, WindowJSInterop>();
	}
}
