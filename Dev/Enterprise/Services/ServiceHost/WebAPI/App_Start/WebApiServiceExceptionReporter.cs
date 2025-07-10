using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Http.ExceptionHandling;
using CargoWise.Common;
using CargoWise.Data;
using CargoWiseOne.WebInfrastructure;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Common;

namespace Enterprise.Services.ServiceHost
{
	public class WebApiServiceExceptionReporter : ExceptionLogger
	{
		public override Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
		{
			try
			{
				using (Db.DisposableActionForDbConnection())
				{
					var exception = context.Exception;
					if (ContainsDatabaseUpgradeException(exception))
					{
						WebUpgradeManager.NotifyUpgradeRequired();
					}
					else if (!ShouldIgnoreException(context))
					{
						ExceptionReporter.Instance.ReportException(string.Empty, exception);
					}
					return Task.CompletedTask;
				}
			}
			catch (Exception ex)
			{
				KillProcessIfOutOfMemoryException(ex);
				throw;
			}
			finally
			{
					var exception = context.Exception;
					KillProcessIfOutOfMemoryException(exception);
			}
		}

		void KillProcessIfOutOfMemoryException(Exception exception)
		{
			if (exception is OutOfMemoryException && WebConfigurationManager.AppSettings["KillProcessOnOutOfMemory"] != "false" && !Globals.IsTest)
			{
				Process.GetCurrentProcess().Kill();
			}
		}

		static bool ShouldIgnoreException(ExceptionLoggerContext context)
		{
			return context.Exception.IsIISInternalCommunicationErrorWhenClientConnected()
					|| IsClientDisconnectedError(context.Exception)
					|| IsServerInternalError(context.Exception)
					|| IsServerInternalError((context.Exception as AggregateException)?.Flatten().InnerException)
					|| IsNetworkException(context.Exception)
					|| IsUrlParseException(context)
				;
		}

		static bool IsClientDisconnectedError(Exception exception)
		{
			const int ERROR_OPERATION_ABORTED = unchecked((int)0x800703E3);
			return (exception is IOException && exception.InnerException is HttpException httpException && httpException.HResult == ERROR_OPERATION_ABORTED)
				|| (exception is HttpException && exception.Message.Equals((NoResString)"The client disconnected.", StringComparison.Ordinal));
		}

		static bool IsServerInternalError(Exception exception)
		{
			const int httpInternalServerError = 500;
			var handledHttpHResultCodes = new[]
			{
				unchecked((int)0x80070057),
				unchecked((int)0x800703E3),
				unchecked((int)0x800704CD),
				unchecked((int)0x80072746),
				unchecked((int)0x80070016),
				unchecked((int)0x80070006),
				unchecked((int)0x80070040),
			};

			return exception is HttpException httpException
					&& httpException.GetHttpCode() == httpInternalServerError
					&& (handledHttpHResultCodes.Contains(httpException.HResult) || handledHttpHResultCodes.Contains(httpException.InnerException?.HResult ?? 0));
		}

		static bool IsNetworkException(Exception exception)
		{
			return exception.GetFirstOccurrenceOfException<SqlException>() is SqlException sqlException && (sqlException.Number == 121 || sqlException.Number == 53);
		}

		static bool IsUrlParseException(ExceptionLoggerContext context)
		{
			return
				context.CatchBlock == ExceptionCatchBlocks.HttpControllerDispatcher &&
				context.Exception is ArgumentException &&
				context.Exception.Message.StartsWith((NoResString)"The key is invalid JQuery syntax because it is missing a closing bracket");
		}

		static bool ContainsDatabaseUpgradeException(Exception exception) => exception.FlattenInnerExceptions().Any(e => e is DatabaseUpgradeException);
	}
}
