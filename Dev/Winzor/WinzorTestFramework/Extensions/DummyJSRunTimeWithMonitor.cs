#nullable enable
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using WinzorFramework.JSInterop;

namespace WinzorTestFramework;

public class DummyJSRunTimeWithMonitor : IJSRuntimeWithMonitor
{
	readonly IJSRuntime jsRuntime;

	public DummyJSRunTimeWithMonitor(IJSRuntime jSRuntime)
	{
		this.jsRuntime = jSRuntime;
	}

#pragma warning disable SA1011 // Closing square brackets should be spaced correctly
	public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => await jsRuntime.InvokeAsync<TValue>(identifier, args);

	public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => await jsRuntime.InvokeAsync<TValue>(identifier, cancellationToken, args);
#pragma warning restore SA1011 // Closing square brackets should be spaced correctly
}
