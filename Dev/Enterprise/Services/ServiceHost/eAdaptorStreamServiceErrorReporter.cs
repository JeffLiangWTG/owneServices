using System;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Management;
using CargoWise.Common;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.ServiceHost
{
	static class eAdaptorStreamServiceErrorReporter
	{
		internal static void ReportUnhandledException(Exception exception)
		{
			Argument.NotNull(exception, "Exception to be handled and potentially reported");
			if (!ShouldIgnoreErrorReporting(exception))
			{
				ErrorReporter.ReportOnce("Unhandled exception in eAdaptor streamed service", exception);
			}
		}

		static bool ShouldIgnoreErrorReporting(Exception exception)
		{
			return (exception is HttpException && ((HttpException)exception).WebEventCode == WebEventCodes.RuntimeErrorPostTooLarge) ||
				(exception is HttpException && exception.HasInnerExceptionOfType(out COMException comException) && unchecked((uint)comException.HResult) == 0x800703E3) || // Client has aborted the connection
				(exception is HttpException && unchecked((uint)((HttpException)exception).ErrorCode) == 0x80070040) || // Client has aborted the connection
				(exception is HttpException && exception.HasInnerExceptionOfType(out COMException _)) || // Something bad on the unmanaged side we were not expecting
				(exception is HttpException && exception.Message.StartsWith((NoResString)"The client is disconnected because the underlying request has been completed.", StringComparison.OrdinalIgnoreCase)) || // Exception message))
				(exception is InvalidOperationException && exception.Message.StartsWith((NoResString)"Timeout expired.  The timeout period elapsed prior to obtaining a connection from the pool.", StringComparison.Ordinal)) || // Exception message))
				(exception is InvalidOperationException && exception.Message.StartsWith((NoResString)"The transaction no longer has an active connection.", StringComparison.Ordinal)) || // Exception message
				(exception is ArgumentException && exception.Message.StartsWith((NoResString)"Invalid Request XML.", StringComparison.Ordinal)) || // Exception message
				(exception is MessageHandlerException);
		}
	}
}
