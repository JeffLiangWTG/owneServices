using Microsoft.JSInterop;

namespace CargoWiseNext.Blazor.Components;

public class PopoverJsInterop
{
	readonly Lazy<Task<IJSObjectReference>> module;

	public PopoverJsInterop(IJSRuntime jsRuntime)
	{
		module = new Lazy<Task<IJSObjectReference>>(async () => await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/CargoWiseNext.Blazor.Components/js/popover.js"));
	}

	protected async Task InvokeJsAsync(string identifier, params object?[] args)
		=> await (await module.Value).InvokeVoidAsync(identifier, args: args);

	public async Task ShowAsync(string id)
	{
		await InvokeJsAsync("show", id);
	}

	public async Task HideAsync(string id)
	{
		await InvokeJsAsync("hide", id);
	}

	public async Task ToggleAsync(string id)
	{
		await InvokeJsAsync("toggle", id);
	}

	public async Task InitAsync(string id)
	{
		await InvokeJsAsync("init", id);
	}
}
