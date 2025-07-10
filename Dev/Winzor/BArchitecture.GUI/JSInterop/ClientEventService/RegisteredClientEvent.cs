using Microsoft.JSInterop;
using WinzorFramework.Extensions;

namespace WinzorFramework.JSInterop;

public sealed class RegisteredClientEvent : IAsyncDisposable
{
	public RegisteredClientEvent(IJSObjectReference jsObjectReference)
	{
		this.jsObjectReference = jsObjectReference;
	}

	IJSObjectReference? jsObjectReference;
	bool disposed;

	public async ValueTask DisposeAsync()
	{
		if (disposed)
		{
			return;
		}
		disposed = true;

		await ExceptionHandlerExtension.HandleJSExceptionAsync(async () =>
		{
			if (jsObjectReference is not null)
			{
				await jsObjectReference.InvokeVoidAsync("unregister");
				await jsObjectReference.DisposeAsync();
				jsObjectReference = null;
			}
		});
	}
}
