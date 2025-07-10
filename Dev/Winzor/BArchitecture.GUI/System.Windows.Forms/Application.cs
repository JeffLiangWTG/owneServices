using System.Reflection;
using WinzorFramework;

namespace System.Windows.Forms;

public static class Application
{
	public static event EventHandler? Idle
	{
		add => WinzorDispatcher.Current.Idle += value;
		remove => WinzorDispatcher.Current.Idle -= value;
	}

	public static void DoEvents()
	{
		WinzorDispatcher.Current.DoEvents();
	}

	public static FormCollection OpenForms { get; } = new FormCollection();

	internal static Form? GetSuitableContextForm(Func<Form, bool> predicate)
		=> OpenForms.FirstOrDefault(f => f.ShouldRender && f.CargoWiseClientServices is not null && predicate(f));

	public static event ThreadExceptionEventHandler? ThreadException
	{
		add { threadException += value; }
		remove { threadException -= value; }
	}

	// This event is raised when an unhandled exception occurs in the application.
	// Imitates the behavior of the AppDomain.CurrentDomain.UnhandledException event in a WinForms application
	public static event UnhandledExceptionEventHandler? UnhandledException
	{
		add { unhandledException += value; }
		remove { unhandledException -= value; }
	}

	[ThreadStatic]
	static ThreadExceptionEventHandler? threadException;
	
	static UnhandledExceptionEventHandler? unhandledException;

	public static event EventHandler? ApplicationExit;

	public delegate void DeveloperExceptionEventHandler(object? sender, string? message, Exception? ex);

	public static event DeveloperExceptionEventHandler? DeveloperException
	{
		add { developerException += value; }
		remove { developerException -= value; }
	}
	static DeveloperExceptionEventHandler? developerException;

	public static void ReportDeveloperException(string? message, Exception? exception = null)
	{
		developerException?.Invoke(null, message, exception);
	}

	public static void OnThreadException(Exception t)
	{
		try
		{
			threadException?.Invoke(Thread.CurrentThread, new ThreadExceptionEventArgs(t));
		}
		catch (Exception ex)
		{
			// Exceptions can occur during threadException.invoke, e.g. Database upgrade will throw UpgradeInProgressException
			// Handle exceptions here by calling OnUnhandledException as last resort.
			OnUnhandledException(ex);
		}
	}

	/// <summary>
	// Invokes the UnhandledException event handler if it is subscribed, passing the current thread and the exception.
	// This method is called as a last resort when an unhandled exception occurs.
	// After invoking the event handler, it calls the Exit method to shut down the application.
	/// </summary>
	/// <param name="ex">Exception thrown</param>
	internal static void OnUnhandledException(Exception ex)
	{
		try
		{
			unhandledException?.Invoke(Thread.CurrentThread, new UnhandledExceptionEventArgs(ex, true));
		}
		catch
		{
			// If the exception handling for unhandled exceptions throws an exception, then there is nothing we can do.
			// Swallow the exception to prevent it from being reported as an unobserved task exception in places where
			// we are relying on using Application.ThreadException for exception handling in fired and forgotten tasks.
		}
		finally
		{
			Exit();
		}
	}

	public static event EventHandler? ThreadExit;

	internal static void OnThreadExit()
	{
		var relatedOpenForms = OpenForms.Where(form => form.WinzorDispatcher == WinzorDispatcher.Current);
		if (relatedOpenForms.Any())
		{
			foreach (var relatedOpenForm in relatedOpenForms)
			{
				OpenForms.Remove(relatedOpenForm);
			}
		}

		ThreadExit?.Invoke(null, EventArgs.Empty);
	}

	public static void Exit()
	{
		foreach (Form f in OpenForms)
		{
			_ = f.InvokeWinzorDispatcherAsync(() => f.RaiseFormClosedOnAppExit());
		}

		var form = WinzorDispatcher.IsCurrent && WinzorDispatcher.HasCurrentContext ? WinzorDispatcher.Current.CurrentContext.Form : null;
		if (form is not null && form.CargoWiseClientServices is not null)
		{
			_ = form.Invoke(async () =>
			{
				_ = await form.CargoWiseClientServices.LifecycleService.RequestApplicationShutDownAsync();
			});
		}

		ApplicationExit?.Invoke(null, EventArgs.Empty);
	}

	public static string? StartupPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

	public static bool RenderWithVisualStyles => false;

	public static event EventHandler? EnterThreadModal		// To be implemented in WI00826374
	{
		add { }
		remove { }
	}

	public static event EventHandler? LeaveThreadModal		// To be implemented in WI00826374
	{
		add { }
		remove { }
	}
}
