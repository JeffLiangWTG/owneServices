using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace WinzorFramework;

public class WinzorDispatcherSynchronizationContext : SynchronizationContext
{
	public WinzorDispatcherSynchronizationContext(WinzorDispatcher disptacher)
	{
		WinzorDispatcher = disptacher;
	}

	[SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "Implementing SynchronziationContext API")]
	public override void Post(SendOrPostCallback d, object? state)
	{
		if (WinzorDispatcher.IsDisposed)
		{
			return;
		}

		PostAsync(d, state);
	}

	[SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "Implementing SynchronziationContext API")]
	public override void Send(SendOrPostCallback d, object? state)
	{
		if (WinzorDispatcher.IsDisposed)
		{
			return;
		}

		if (WinzorDispatcher.IsCurrent && WinzorDispatcher.Current == WinzorDispatcher)
		{
			d.DynamicInvoke(state);
		}
		else
		{
			PostAsync(d, state).GetAwaiter().GetResult();
		}
	}

	Task PostAsync(SendOrPostCallback d, object? state)
	{
		return WinzorDispatcher.InvokeAsync(() =>
		{
			var context = WinzorDispatcher.HasCurrentContext ? new ServerInitiatedCallbackContext(WinzorDispatcher.CurrentContext) : new ServerInitiatedCallbackContext();
			var withContext = WinzorDispatcher.WithContext(context);
			try
			{
				d.DynamicInvoke(state);
			}
			catch (Exception ex)
			{
				Application.OnThreadException(ex.InnerException ?? ex);
			}
			finally
			{
				withContext.Dispose();
			}
		});
	}

	public WinzorDispatcher WinzorDispatcher { get; }
}
