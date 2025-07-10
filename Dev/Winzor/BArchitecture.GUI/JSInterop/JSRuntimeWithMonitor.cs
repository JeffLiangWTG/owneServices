using Microsoft.JSInterop;
using WinzorFramework.Telemetry;

namespace WinzorFramework.JSInterop;

public interface IJSRuntimeWithMonitor : IJSRuntime
{
}

public class JSRuntimeWithMonitor : IJSRuntimeWithMonitor
{
	readonly IJSRuntime jsRuntime;

	public JSRuntimeWithMonitor(IJSRuntime jsRuntime)
	{
		this.jsRuntime = jsRuntime;
	}

	public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
	{
		if (typeof(TValue) == typeof(IJSObjectReference))
		{
			return (TValue)await InvokeJSObjectReferenceAsync(identifier, args);
		}

		using var activity = TelemetryService.ActivitySource.StartActivity($"JsInvoke_{identifier}");
		return await jsRuntime.InvokeAsync<TValue>(identifier, args);
	}

	public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
	{
		if (typeof(TValue) == typeof(IJSObjectReference))
		{
			return (TValue)await InvokeJSObjectReferenceAsync(identifier, args, cancellationToken);
		}

		using var activity = TelemetryService.ActivitySource.StartActivity($"JsInvoke_{identifier}");
		return await jsRuntime.InvokeAsync<TValue>(identifier, cancellationToken, args);
	}

	async ValueTask<IJSObjectReference> InvokeJSObjectReferenceAsync(string identifier, object?[]? args, CancellationToken? cancellationToken = null)
	{
		IJSObjectReference objectReference;
		var activityName = $"JsInvoke_{identifier}";
		string? moduleName = null;
		if (identifier == "import")
		{
			moduleName = $"{(args?[0] as string)?.Split('/').Last().Split('?').First()}";
			activityName += "_" + moduleName;
		}
		using var activity = TelemetryService.ActivitySource.StartActivity(activityName);
		if (cancellationToken.HasValue)
		{
			objectReference = await jsRuntime.InvokeAsync<IJSObjectReference>(identifier, cancellationToken.Value, args);
		}
		else
		{
			objectReference = await jsRuntime.InvokeAsync<IJSObjectReference>(identifier, args);
		}
		return new JSObjectReferenceWithMonitor(objectReference, moduleName);
	}
}
