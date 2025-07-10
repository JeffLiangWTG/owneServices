using System;
using System.Diagnostics;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Winzor.AppServer.Diagnostics;

/// <summary>
/// Custom trace listener to report failures.
/// More about Trace Listeners: https://learn.microsoft.com/en-us/dotnet/framework/debug-trace-profile/trace-listeners
/// - Assert only on debug builds: https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.debug.assert?view=net-8.0
/// - Assert on debug and release builds: https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.trace.assert?view=net-8.0
/// </summary>
public class ExceptionReporterTraceListener : TraceListener
{
	readonly Action<Exception> reportHandler;
	readonly bool propagateError;

	public ExceptionReporterTraceListener(Action<Exception> reportHandler, bool propagateError = false)
	{
		this.reportHandler = reportHandler;
		this.propagateError = propagateError;
	}

	public override void Write(string message)
	{
		/* DO NOTHING */
	}

	public override void WriteLine(string message)
	{
		/* DO NOTHING */
	}

	public override void Fail(string message)
	{
		Fail(message, null);
	}

	public override void Fail(string message, string detailMessage)
	{
		var ex = CreateException(message, detailMessage);

		try
		{
			reportHandler?.Invoke(ex);
		}
		catch (Exception) { /* DO NOTHING */ }

		if (propagateError)
		{
			throw ex;
		}
	}

	Exception CreateException(string message, string detailMessage)
	{
		var ex = new Exception($"Assert failure: '{message}'");
		ex.Data.Add((NoResString)"message", message ?? string.Empty);
		ex.Data.Add("detailMessage", detailMessage ?? string.Empty);
		return ex;
	}
}
