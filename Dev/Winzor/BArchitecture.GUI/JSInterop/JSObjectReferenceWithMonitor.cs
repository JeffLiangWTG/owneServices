using Microsoft.JSInterop;
using WinzorFramework.Telemetry;

namespace WinzorFramework.JSInterop;

public class JSObjectReferenceWithMonitor : IJSObjectReference
{
	readonly IJSObjectReference objectReference;
	string? moduleName { get; }

	public JSObjectReferenceWithMonitor(IJSObjectReference objectReference, string? moduleName)
	{
		this.objectReference = objectReference;
		this.moduleName = moduleName;
	}

	public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
	{
		using var activity = TelemetryService.ActivitySource.StartActivity($"JsInvoke_{moduleName}_{identifier}");
		return await objectReference.InvokeAsync<TValue>(identifier, args: args);
	}

	public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
	{
		using var activity = TelemetryService.ActivitySource.StartActivity($"JsInvoke_{moduleName}_{identifier}");
		return await objectReference.InvokeAsync<TValue>(identifier, cancellationToken: cancellationToken, args: args);
	}

	public async ValueTask DisposeAsync()
	{
		await objectReference.DisposeAsync();
	}
}
