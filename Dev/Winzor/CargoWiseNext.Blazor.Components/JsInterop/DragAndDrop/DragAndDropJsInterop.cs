using Microsoft.JSInterop;

namespace CargoWiseNext.Blazor.Components;

public class DragAndDropJsInterop : IDragAndDropJsInterop
{
	const string ModulePath = "./_content/CargoWiseNext.Blazor.Components/js/dragAndDrop.js";

	readonly Lazy<Task<IJSObjectReference>> module;

	public DragAndDropJsInterop(IJSRuntime jsRuntime)
	{
		module = new Lazy<Task<IJSObjectReference>>(async () => await jsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath));
	}

	public async ValueTask InitAsync(string dropAreaClassname, string dragOverClassname, DotNetObjectReference<IDragAndDropInvokables> invokablesRef)
	{
		var module = await this.module.Value;
		await module.InvokeVoidAsync("dragAndDrop.init", dropAreaClassname, dragOverClassname, invokablesRef);
	}

	public async ValueTask TeardownAsync()
	{
		var module = await this.module.Value;
		await module.InvokeVoidAsync("dragAndDrop.teardown");
	}
}
