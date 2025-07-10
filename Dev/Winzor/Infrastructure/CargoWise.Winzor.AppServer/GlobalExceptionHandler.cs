using System;
using System.Threading.Tasks;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using Microsoft.Extensions.Logging;

namespace CargoWise.Winzor.AppServer;

/// <summary>
/// catch UnobservedTaskException and log error to elastic for debugging.
/// </summary>
public class GlobalExceptionHandler : IDisposable
{
	ILogger<GlobalExceptionHandler> logger;

	public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
	{
		this.logger = logger;
		TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
	}

	public void Dispose()
	{
		TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
	}

#if DEBUG
	public void SetLogger(ILogger<GlobalExceptionHandler> logger)
	{
		this.logger = logger;
	}
#endif

	// Temp code: show error report dialog only once for each type of exception until we have a better solution to reproduce.
	bool isUnobservedTaskEndInvokeDotNetAfterTaskExceptionHandled;
	bool isUnobservedTaskInnerInvokeExceptionHandled;

	void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
	{
		var logLevel = IgnoreUnobservedTaskException(e.Exception.GetInnermostException()) ? LogLevel.Warning : LogLevel.Error;
		logger.Log(logLevel, e.Exception, (NoResString)"An unhandled exception occurred.");

		var innerException = e.Exception.InnerException;
		if (!isUnobservedTaskEndInvokeDotNetAfterTaskExceptionHandled && innerException is NullReferenceException && innerException.TargetSite.DeclaringType.FullName == "Microsoft.JSInterop.Infrastructure.DotNetDispatcher" && innerException.TargetSite.Name == "EndInvokeDotNetAfterTask")
		{
			ErrorReporter.ReportOnce(e.Exception.Message, innerException.TargetSite.DeclaringType.FullName, innerException);
			isUnobservedTaskEndInvokeDotNetAfterTaskExceptionHandled = true;
		}
		if (!isUnobservedTaskInnerInvokeExceptionHandled && innerException is NullReferenceException && innerException.TargetSite.DeclaringType.FullName == "System.Threading.Tasks.ContinuationTaskFromTask" && innerException.TargetSite.Name == "InnerInvoke")
		{
			ErrorReporter.ReportOnce(e.Exception.Message, innerException.TargetSite.DeclaringType.FullName, innerException);
			isUnobservedTaskInnerInvokeExceptionHandled = true;
		}
	}

	bool IgnoreUnobservedTaskException(Exception innerException)
	{
		if (innerException is TaskCanceledException)
		{
			return true;
		}

		if (innerException.GetType().FullName == "Microsoft.JSInterop.JSDisconnectedException")
		{
			return true;
		}

		if (innerException is NullReferenceException && innerException.TargetSite.DeclaringType.FullName == "Microsoft.AspNetCore.Components.Server.Circuits.RemoteJSRuntime" && (innerException.TargetSite.Name == "EndInvokeDotNet" || innerException.TargetSite.Name == "SendByteArray" || innerException.TargetSite.Name == "TransmitStreamAsync"))
		{
			return true;
		}

		if (innerException is NullReferenceException && innerException.TargetSite.DeclaringType.FullName == "Microsoft.AspNetCore.SignalR.ClientProxyExtensions" && innerException.TargetSite.Name == "SendAsync")
		{
			return true;
		}

		if (innerException is NullReferenceException && innerException.TargetSite.DeclaringType.FullName == "Microsoft.JSInterop.Infrastructure.DotNetDispatcher" && innerException.TargetSite.Name == "EndInvokeDotNetAfterTask")
		{
			return true;
		}

		if (innerException is NullReferenceException && innerException.TargetSite.DeclaringType.FullName == "System.Threading.Tasks.ContinuationTaskFromTask" && innerException.TargetSite.Name == "InnerInvoke")
		{
			return true;
		}

		return false;
	}
}
