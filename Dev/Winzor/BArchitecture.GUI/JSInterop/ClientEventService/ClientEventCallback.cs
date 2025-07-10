using System.Diagnostics;
using Microsoft.JSInterop;

namespace WinzorFramework.JSInterop;

public sealed class ClientEventCallback : IDisposable
{
	public ClientEventCallback(Func<Task> callback)
	{
		this.callback = callback;
		ObjectReference = DotNetObjectReference.Create(this);
	}

	public DotNetObjectReference<ClientEventCallback> ObjectReference { get; }

	public ActivitySource? ActivitySource { get; set; }
	public string? EventType { get; set; }

	[JSInvokable]
	public async Task InvokeCallbackAsync(object args)
	{
		using (var activity = ActivitySource?.StartActivity($"ClientEventCallback_{EventType}", ActivityKind.Server))
		{
			await callback();
		}
	}

	public void Dispose()
	{
		ObjectReference?.Dispose();
	}

	readonly Func<Task> callback;
}

public sealed class ClientEventCallback<T> : IDisposable
{
	public ClientEventCallback(Func<T, Task> callback)
	{
		this.callback = callback;
		ObjectReference = DotNetObjectReference.Create(this);
	}

	public DotNetObjectReference<ClientEventCallback<T>> ObjectReference { get; }

	public ActivitySource? ActivitySource { get; set; }
	public string? EventType { get; set; }

	[JSInvokable]
	public async Task InvokeCallbackAsync(T args)
	{
		using (var activity = ActivitySource?.StartActivity($"ClientEventCallback_{EventType}", ActivityKind.Server))
		{
			await callback(args);
		}
	}

	public void Dispose()
	{
		ObjectReference?.Dispose();
	}

	readonly Func<T, Task> callback;
}
