using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Management;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Services.ServiceHost
{
	static class eAdaptorRequestProcessorCore
	{
		internal static HttpResponseMessage GetPlainTextResponse(string text)
		{
			var response = new HttpResponseMessage();
			response.Content = new StringContent(text);
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
			return response;
		}

		internal delegate IeAdaptorRequestProcessorResult RequestProcessor(SubStreamableStream incomingStream, IeAdaptorConfig eAdaptorConfig, string handlerName);

		internal static HttpResponseMessage ProcessRequestWithExceptionHandling(
			HttpRequestMessage request,
			RequestProcessor requestProcessor,
			IeAdaptorConfig eAdaptorConfig,
			string handlerName = "")
		{
			using (Db.DisposableActionForDbConnection())
			{
				if (!eAdaptorConfig.IsActive && eAdaptorConfig.ThrowIfNotActive)
				{
					return eAdaptorConfig.ResponseWriter.CreateErrorResponse(request, HttpStatusCode.BadRequest, eAdaptorLogs.AdaptorDisabled(eAdaptorConfig.Name));
				}

				try
				{
					using (TrackAndDisposeOfFactories())
					{
						eAdaptorConfig.ContextSetter.SetContext();
						using (var incomingStream = new CargoWise.IO.Shim.SubStreamableStream())
						{
							request.Content.CopyToAsync(incomingStream).GetAwaiter().GetResult();

							var result = requestProcessor(incomingStream, eAdaptorConfig, handlerName);
							using (result.UniversalResponse)
							{
								result.UniversalResponse.Position = 0;
								var response = eAdaptorConfig.ResponseWriter.CreateResponse(result);
								if (response.Content?.Headers != null)
								{
									response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml") { CharSet = Encoding.UTF8.WebName };
								}
								return response;
							}
						}
					}
				}
				catch (HttpException httpException) when (httpException.WebEventCode == WebEventCodes.RuntimeErrorPostTooLarge)
				{
					var runTimeSection = ConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;
					var message = Res.GetString("6292ddeb-fc89-432b-a7cf-b5960bd0bedf", "Maximum request length exceeded. Current limit is {0}KB. Please raise an incident if you believe the limit should be raised.", runTimeSection.MaxRequestLength);
					var error = new HttpError(message);
					return eAdaptorConfig.ResponseWriter.CreateErrorResponse(request, HttpStatusCode.RequestEntityTooLarge, error);
				}
				catch (HttpException httpException) when (httpException.HasInnerExceptionOfType(out COMException comException) && unchecked((uint)comException.HResult) == 0x800703E3)
				{
					// Client has aborted the connection
					return eAdaptorConfig.ResponseWriter.CreateErrorResponse(request, HttpStatusCode.BadRequest, (NoResString)"Abnormal client termination."); // Exception message
				}
				catch (HttpException httpException) when (unchecked((uint)httpException.ErrorCode) == 0x80070040)
				{
					// Client has aborted the connection
					return eAdaptorConfig.ResponseWriter.CreateErrorResponse(request, HttpStatusCode.BadRequest, (NoResString)"Abnormal client termination."); // Exception message
				}
				catch (HttpException httpException) when (httpException.Message.StartsWith((NoResString)"The client disconnected", StringComparison.OrdinalIgnoreCase) && unchecked((uint)httpException.ErrorCode) == 0x80004005)
				{
					// Client has aborted the connection
					return eAdaptorConfig.ResponseWriter.CreateErrorResponse(request, HttpStatusCode.BadRequest, (NoResString)"Abnormal client termination."); // Exception message
				}
				catch (HttpException httpException) when (httpException.HasInnerExceptionOfType(out COMException comException))
				{
					// Something bad on the unmanaged side we were not expecting
					ErrorReporter.ReportOnce("Unexpected COM exception", httpException);
					return eAdaptorConfig.ResponseWriter.CreateErrorResponse(request, HttpStatusCode.InternalServerError, (NoResString)"Unexpected COM exception."); // Exception message
				}
				catch (HttpException httpException) when (httpException.Message.StartsWith((NoResString)"The client is disconnected because the underlying request has been completed.", StringComparison.OrdinalIgnoreCase)) // Exception message
				{
					return eAdaptorConfig.ResponseWriter.CreateErrorResponse(request, HttpStatusCode.BadRequest, (NoResString)"Post request lost input"); // Exception message
				}
				catch (InvalidWebEnvironmentException ex)
				{
					return eAdaptorConfig.ResponseWriter.CreateErrorResponse(request, HttpStatusCode.BadRequest, ex.Message);
				}
				catch (ZConcurrencyCheckFailureException ex) //must be handled before ZCannotSaveException
				{
					return eAdaptorConfig.ResponseWriter.CreateErrorResponse(request, HttpStatusCode.Conflict, ex.Message);
				}
				catch (Exception ex) when (IsServiceUnavailableSqlException(ex, out var dbError))
				{
					return eAdaptorConfig.ResponseWriter.CreateErrorResponse(request, HttpStatusCode.ServiceUnavailable, dbError.GetUserFriendlyMessage(Db.Connection), ex);
				}
				catch (ZCannotSaveException ex)
				{
					return eAdaptorConfig.ResponseWriter.CreateErrorResponse(request, HttpStatusCode.InternalServerError, (NoResString)"Unexpected Save exception.", ex); // Exception message
				}
				catch (ZSaveConcurrencyException saveConcurrencyException)
				{
					return eAdaptorConfig.ResponseWriter.CreateErrorResponse(request, HttpStatusCode.Conflict, new ConcurrencyExceptionHandler(saveConcurrencyException).Info);
				}
				catch (InvalidOperationException ex) when (ex.Message.StartsWith((NoResString)"Timeout expired.  The timeout period elapsed prior to obtaining a connection from the pool.", StringComparison.Ordinal)) // Exception message
				{
					var message = Res.GetString("023AB424-3D34-4801-82DE-835450B09C08", "Maximum number of concurrent connections has been exceeded.");
					return eAdaptorConfig.ResponseWriter.CreateErrorResponse(request, HttpStatusCode.ServiceUnavailable, message);
				}
				catch (InvalidOperationException ex) when (ex.Message.StartsWith((NoResString)"The transaction no longer has an active connection.", StringComparison.Ordinal)) // Exception message
				{
					var message = Res.GetString("CBDB80BD-7545-49A6-8993-0242914A15B0", "This service is temporarily unavailable.");
					return eAdaptorConfig.ResponseWriter.CreateErrorResponse(request, HttpStatusCode.ServiceUnavailable, message);
				}
				catch (TransactionException ex)
				{
					ErrorReporter.ReportOnce("Transaction Exception in eAdaptor", ex);
					return eAdaptorConfig.ResponseWriter.CreateErrorResponse(request, HttpStatusCode.InternalServerError, ex.Message, ex);
				}
				catch (ZSaveException ex)
				{
					var dbErrorType = ex.InnerException.DbErrorType;
					if (ex.GetInnermostException() is TransactionException)
					{
						return eAdaptorConfig.ResponseWriter.CreateErrorResponse(request, HttpStatusCode.InternalServerError, (NoResString)"A write was rolled back by the database. This is likely to be transient and the operation will succeed on retry."); // Exception message
					}
					else if (dbErrorType == DbErrorType.CannotInsertDuplicateUniqueIndexKey)
					{
						var message = Res.GetString("eae613fb-b335-420a-9143-333e6fcaec53", "A Save exception has occurred due to Unique Index Violation '{0}', this may be a transient error and could possibly succeed on retry.", ex.IndexNameIfUniqueIndexViolation);
						return eAdaptorConfig.ResponseWriter.CreateErrorResponse(request, HttpStatusCode.InternalServerError, message, ex); // Exception message
					}
					else if (dbErrorType == DbErrorType.CannotInsertDuplicateConstraintKey
						|| dbErrorType == DbErrorType.InsertConflictedWithForeignKey
						|| dbErrorType == DbErrorType.UpdateConflictedWithForeignKey
						|| dbErrorType == DbErrorType.DeleteConflictedWithForeignKey)
					{
						var message = Res.GetString("3E0DF07B-DD5A-4CCE-9B4D-6B1B864E1A4C", "A save exception has occurred due to a database constraint violation '{0}'.", ex.FriendlyMessage);
						return eAdaptorConfig.ResponseWriter.CreateErrorResponse(request, HttpStatusCode.InternalServerError, message, ex); // Exception message
					}
					else if (dbErrorType == DbErrorType.DeadlockError)
					{
						var message = Res.GetString("cc2c540a-1735-40a5-ba53-dd5824f1cd65", "A save exception has occurred due to deadlock with another operation. Please try again.");
						return eAdaptorConfig.ResponseWriter.CreateErrorResponse(request, HttpStatusCode.InternalServerError, message, ex); // Exception message
					}
					else
					{
						throw;
					}
				}
				catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.ErrorOnUserDefinedRoutineOrAggregate)
				{
					var message = Res.GetString("03c3b87c-3061-4275-991e-669da5ec13df", "Try again later. If this problem persists, please contact your administrator");
					return eAdaptorConfig.ResponseWriter.CreateErrorResponse(request, HttpStatusCode.GatewayTimeout, message, ex);
				}
				catch (UpdateEDIMessageStatusException ex)
				{
					return eAdaptorConfig.ResponseWriter.CreateErrorResponse(request, HttpStatusCode.InternalServerError, ex.Message, ex);
				}
				catch (Exception ex) when (!ex.IsCriticalException() && ex.HasInnerExceptionOfType(out Win32Exception win32Exception))
				{
					if (win32Exception.NativeErrorCode == 10054 || win32Exception.NativeErrorCode == 258) // An existing connection was forcibly closed by the remote host
					{
						var message = Res.GetString("f8332c1e-3683-41ce-979e-156a8f208211", "DB is currently not responsive: if self hosted, contact your administrators; if cloud, raise an incident");
						return eAdaptorConfig.ResponseWriter.CreateErrorResponse(request, HttpStatusCode.ServiceUnavailable, message, ex);
					}
					else
					{
						throw;
					}
				}
			}
		}

		static IDisposable TrackAndDisposeOfFactories()
		{
			var tracking = PersistentFactoryCacheManager.Instance.TrackFactoriesCreatedOnCurrentThread();

			return new DisposableAction(() =>
			{
				try
				{
					foreach (BusinessObjectFactory factory in PersistentFactoryCacheManager.Instance.GetFactoriesCreatedInTrackedRegion())
					{
						factory.DeactivateActiveCollectionsAndCaches();
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ErrorReporter.ReportOnce("TrackAndDisposeOfFactories failed.", ex);
				}
				finally
				{
					tracking.Dispose();
				}
			});
		}

		static bool IsServiceUnavailableSqlException(Exception ex, out DbErrorMatch dbError)
		{
			dbError = null;

			if (ex.IsCriticalException() || !ex.HasInnerExceptionOfType(out SqlException sqlException))
			{
				return false;
			}

			dbError = new DbErrorMatch(sqlException);
			return ServiceUnavalibleSqlErrors.Contains(dbError.ExceptionType);
		}

		static IEnumerable<DbErrorType> ServiceUnavalibleSqlErrors { get; } =
		[
			DbErrorType.TimeoutExpired,
			DbErrorType.LockTimeoutExpired
		];
	}
}
