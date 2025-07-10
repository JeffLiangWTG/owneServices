using CargoWiseNext.Blazor.Components.JsInterop;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWiseNext.Blazor.Components;

public static class ServicesExtension
{
	public static IServiceCollection AddCargoWiseBlazorComponents(this IServiceCollection services) => services
		.AddScoped<IDropDownListInterop, DropDownListInterop>()
		.AddScoped<IDragAndDropJsInterop, DragAndDropJsInterop>()
		.AddScoped<PopoverJsInterop>()
		.AddScoped<IPopoverService, PopoverService>();
}
