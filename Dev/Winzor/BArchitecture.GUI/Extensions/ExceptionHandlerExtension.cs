using System.Windows.Forms;
using Microsoft.JSInterop;

namespace WinzorFramework.Extensions;

public static class ExceptionHandlerExtension
{
	public static async Task HandleJSExceptionAsync(Func<Task> action)
	{
		try
		{
			await action();
		}
		catch (JSDisconnectedException)
		{
			// Invoking a JS method after the form has been closed and the circuit is no longer available
			// Ignore.
		}
		catch (ObjectDisposedException)
		{
			// Ignore JS method after caller has been disposed
		}
		catch (TaskCanceledException)
		{
			// Ignore JS task after control has been disposed
			// https://github.com/dotnet/aspnetcore/issues/21384
			// Net.9 might address this issue then we can revisit
		}
	}
}
