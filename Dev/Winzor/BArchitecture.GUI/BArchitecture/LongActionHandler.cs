using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using Microsoft.JSInterop;
using WinzorFramework.Extensions;
using WinzorFramework.Telemetry;

namespace WinzorFramework;

public sealed class LongActionHandler : IAsyncDisposable
{
	public LongActionHandler(Control control)
	{
		form = control.FindForm();

		notRespondingCancellationToken = new CancellationTokenSource();
		notRespondingTask = Task.Run(async () =>
		{
			await Task.Delay(1000, notRespondingCancellationToken.Token);
			if (!notRespondingCancellationToken.IsCancellationRequested && form != null)
			{
				unsetNotResponding = await form.SetNotRespondingAsync();
			}
		}, notRespondingCancellationToken.Token);
	}

	[SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks")]
	public async ValueTask DisposeAsync()
	{
		using var activity = TelemetryService.ActivitySource.StartActivity($"{nameof(LongActionHandler)}.{nameof(DisposeAsync)}");
		await notRespondingCancellationToken.CancelAsync();
		await ExceptionHandlerExtension.HandleJSExceptionAsync(async () => await notRespondingTask);
		if (unsetNotResponding != null && form is not null && !form.IsClosing && !form.IsDisposed)
		{
			await ExceptionHandlerExtension.HandleJSExceptionAsync(async () => await unsetNotResponding.DisposeAsync());
		}
	}

	IAsyncDisposable? unsetNotResponding;
	readonly Form? form;
	readonly CancellationTokenSource notRespondingCancellationToken;
	readonly Task notRespondingTask;
}
