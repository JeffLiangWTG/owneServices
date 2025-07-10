using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using WinzorFramework;

namespace WinzorFramework.RemoteClientServices
{
	/// <summary>
	/// Provides facilities for synchronous calls to the client services.
	/// Generally asynchronous calls to the client application should be preferred
	/// Will ensure that the call is against a form that is not closing or disposed and will run the message loop if necessary.
	/// </summary>
	public static class CargoWiseClientInvoker
	{
		/// <summary>
		/// Runs a blocking action against the CargoWiseClientServices while waiting for a result
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="action"></param>
		/// <returns></returns>
		public static T Invoke<T>(Func<CargoWiseClientServices, Task<T>> action)
		{
			var result = default(T);
			InvokeCore(async (cws) => result = await action.Invoke(cws));
			return result;
		}

		/// <summary>
		/// Runs a blocking action against the CargoWiseClientServices without providing a result
		/// </summary>
		/// <param name="action"></param>
		public static void Invoke(Func<CargoWiseClientServices, Task> action)
		{
			InvokeCore(action);
		}

		static void InvokeCore(Func<CargoWiseClientServices, Task> action)
		{
			// If we are on a Winzor dispatcher and it has a context we can use the context's form directly
			// Otherwise we will try to find a form on the same dispatcher that is not closing
			// Finally, if we are not on a WinzorDispatcher we can directly block the thread while invoking the form as we will not block the dispatcher
			if (WinzorDispatcher.IsCurrent && WinzorDispatcherAvailable(WinzorDispatcher.Current))
			{
				SynchronizeInvoke(WinzorDispatcher.Current.CurrentContext.Form, action);
			}
			else if (WinzorDispatcher.IsCurrent)
			{
				var form = ZApplication.GetOpenForms().LastOrDefault(f => f.WinzorDispatcher == WinzorDispatcher.Current && !f.IsClosing && !f.IsDisposed && !f.Disposing);
				SynchronizeInvoke(form, action);
			}
			else
			{
				var form = ZApplication.GetOpenForms().LastOrDefault(f => !f.IsClosing && !f.IsDisposed && !f.Disposing);
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
				form?.InvokeRenderDispatcherAsync(async () => await action.Invoke(form?.CargoWiseClientServices)).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
			}
		}

		/// <summary>
		/// synchronized action by Invoking with available form which correctly blocks the call on the WinzorDispatcher by running the message loop
		/// </summary>
		static void SynchronizeInvoke(Form form, Func<CargoWiseClientServices, Task> action)
		{
			if (form == null)
			{
				throw new InvalidOperationException("Form is unavailable");
			}

			ExceptionDispatchInfo? exception = null;
			var cts = new CancellationTokenSource();
			_ = form.InvokeRenderDispatcherAsync(async () =>
			{
				try
				{
					await action.Invoke(form.CargoWiseClientServices);
				}
				catch (Exception e)
				{
					exception = ExceptionDispatchInfo.Capture(e);
				}
				finally
				{
					await cts.CancelAsync();
				}
			});

			WinzorDispatcher.Current.RunMessageLoop(cts);
			cts = null;

			if (exception != null)
			{
				exception.Throw();
			}
		}

		static bool WinzorDispatcherAvailable(WinzorDispatcher winzorDispatcher)
		{
			return WinzorDispatcher.HasCurrentContext && winzorDispatcher.CurrentContext.Form != null
			&& !winzorDispatcher.CurrentContext.Form.IsClosing
			&& !winzorDispatcher.CurrentContext.Form.Disposing
			&& !winzorDispatcher.CurrentContext.Form.IsDisposed;
		}
	}
}
