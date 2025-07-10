using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.Freight.OnlineSailingSchedules.Exceptions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	public abstract class AdjustableServiceRequestManager : IServiceRequestManager
	{
		protected AdjustableServiceRequestManager(INotifications notifications)
		{
			this.notifications = Argument.NotNull(notifications, nameof(notifications));
		}

		readonly INotifications notifications;
		const int MaximumSuppressionLevel = 16;

		public TimeSpan RequestTimeout => TimeSpan.FromSeconds(TimeoutInSeconds);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		public bool IsSuppressed()
		{
			return DateTime.UtcNow <= SuppressUntilUtc;
		}

		public void OnSuccessfulRequest()
		{
			int currentSuppressionLevel = SuppressionLevel;

			if (currentSuppressionLevel != 0)
			{
				SuppressionLevel = 0;
				SuppressUntilUtc = DateTime.MinValue;
			}
		}

		public void OnTimedOutRequest()
		{
			int suppressionLevel = SuppressionLevel;

			suppressionLevel = suppressionLevel <= 0
				? 1
				: Math.Min(suppressionLevel * 2, MaximumSuppressionLevel);

			SuppressionLevel = suppressionLevel;

			var suppressUntil = ZDateTime.UtcNow.AddMinutes(suppressionLevel);
			SuppressUntilUtc = suppressUntil;
		}

		void HandleHttpRequestException(Exception exception, Uri uri)
		{
			if (exception == null || exception.InnerException == null)
			{
				notifications.AddError(ResString.GetMultilingualString("bd22fc20-6a21-4fd2-8d03-d94b437dcfc9", "Unknown exception has happened. Please try again."));
				return;
			}
			var ex = exception.InnerException;
			var exceptionErrorKey = $"{ExceptionErrorKey}-{ex.GetType()}";
			if (ex.InnerException is WebException webException)
			{
				if (webException.Message.Contains((NoResString)"The remote server returned an error: (407) Proxy Authentication Required."))
				{
					if (!IsAutoInitiatedServiceRequest)
					{
						notifications.AddError(Res.GetString("3299a9d8-4963-46e6-8260-3b540b4c136b", "Please configure your proxy server to be able to use Global Sailing Schedules."));
					}

					return;
				}

				if (webException.Message.Contains((NoResString)"The underlying connection was closed: Could not establish trust relationship for the SSL/TLS secure channel."))
				{
					if (webException.InnerException is AuthenticationException authenticationException && authenticationException.Message.Contains((NoResString)"The remote certificate is invalid according to the validation procedure."))
					{
						if (!IsAutoInitiatedServiceRequest)
						{
							notifications.AddError(Res.GetString("856a6ff8-15e7-4bdd-90a5-f9fa8151f98f", "Please check that your certificate is installed and enabled or check your proxy server settings to be able to use Global Sailing Schedules."));
						}

						return;
					}
				}
				else if (webException.Status == WebExceptionStatus.SecureChannelFailure)
				{
					if (!IsAutoInitiatedServiceRequest)
					{
						notifications.AddError(Res.GetString("4b26b126-06ab-4073-b7e7-cf6b25493a3f", "Could not create SSL/TLS secure channel. Please try again later."));
					}

					return;
				}
				else if (webException.Status == WebExceptionStatus.ServerProtocolViolation)
				{
					if (!IsAutoInitiatedServiceRequest)
					{
						notifications.AddError(Res.GetString("5638c1c5-8a58-4595-b06e-fe8bf2581c18", "The server committed a protocol violation. Please try again later."));
					}

					return;
				}

				if (webException.Status == WebExceptionStatus.ConnectFailure
					|| webException.Status == WebExceptionStatus.NameResolutionFailure
					|| webException.Status == WebExceptionStatus.ConnectionClosed)
				{
					if (!IsAutoInitiatedServiceRequest)
					{
						notifications.AddError(Res.GetString("c43db7ba-6bee-4d13-91af-5d70d2ed4d14", "Unable to connect to Global Sailing Schedules. Please check that connection URL specified in the registry is correct."));
					}

					return;
				}

				if (webException.Status == WebExceptionStatus.ProtocolError && webException.Message.Contains((NoResString)"(403) Forbidden"))
				{
					if (!IsAutoInitiatedServiceRequest)
					{
						notifications.AddError(Res.GetString("ad9c70e7-6ccb-4a96-b82e-25d14d26c8c9",
							"Unable to connect to Global Sailing Schedules Server. Please check your connection. Firewalls/Proxies may restrict your connection."));
					}

					return;
				}

				exceptionErrorKey += $"-{webException.Status}";
				webException.Data.Add(nameof(webException.Status), webException.Status);
			}

			ErrorReporter.ReportOnce(exceptionErrorKey, GetErrorReportMessage(uri, exception), exception);
			notifications.AddError(ExceptionErrorMessage);
		}

		public void HandleException(Exception exception, Uri uri = null)
		{
			if (exception == null)
			{
				notifications.AddError(ResString.GetMultilingualString("f3bf5e05-bf75-4d54-a5af-87af1e8c02ef", "Unknown exception has happened. Please try again."));
				return;
			}

			var ex = exception.InnerException;
			if (ex is HttpRequestException)
			{
				HandleHttpRequestException(exception, uri);
				return;
			}

			if (ex is TooManyRecordsReturnedException)
			{
				notifications.AddError(ResString.GetMultilingualString("491C3C6D-A8B6-4B69-B100-F6A851A80F98", "Too many records were returned. Please modify the filters to narrow down your search."));
				return;
			}

			if (ex is UnauthorizedException)
			{
				notifications.AddError(ResString.GetMultilingualString("7a4acf6b-730e-4748-a2b7-d06165e48cef", "{0}. Please contact support.", ex.Message));
				return;
			}

			if (ex is TaskCanceledException taskCanceledException && !taskCanceledException.CancellationToken.IsCancellationRequested)
			{
				if (IsAutoInitiatedServiceRequest)
				{
					OnTimedOutRequest();
				}

				notifications.AddError(ResString.GetMultilingualString("4E6C4C05-86CC-4A0A-913B-2705B3AB71A8", "The server is taking too long to respond. This may be caused either by a poor Internet speed or there are too many records that match your search criteria. Please try again later or modify the filters to narrow down your search."));
				return;
			}

			if (ex is BadRequestException badRequestException)
			{
				var reportException = badRequestException.ProblemMessages.Count <= 0;

				foreach (var errorMessage in badRequestException.ProblemMessages)
				{
					if (ErrorMessageCatalog.ErrorMessageCollection.ContainsKey(errorMessage))
					{
						notifications.AddError(ErrorMessageCatalog.GetErrorMessage(errorMessage));
					}
					else
					{
						reportException = true;
					}
				}

				if (!reportException)
				{
					return;
				}
			}

			notifications.AddError(ExceptionErrorMessage);

			if (ex is InternalServerErrorException)
			{
				return;
			}

			ErrorReporter.ReportOnce(ExceptionErrorKey + "-" + ex?.GetType(), GetErrorReportMessage(uri, exception), exception);
		}

		string GetErrorReportMessage(Uri uri, Exception ex)
		{
			var messageLines = new List<string>
			{
				ExceptionErrorMessage,
				$@"Exception Details: {ex}" ,
				$"Url: {uri}",
				$"ServicePointManager.SecurityProtocol: {ServicePointManager.SecurityProtocol}"
			};

			return string.Join(System.Environment.NewLine, messageLines);
		}

		protected virtual bool IsAutoInitiatedServiceRequest => false;

		protected abstract string ExceptionErrorKey { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		protected abstract int TimeoutInSeconds { get; }
		protected internal abstract int SuppressionLevel { get; set; }
		protected internal abstract ZDateTime SuppressUntilUtc { get; set; }
		protected internal abstract string ExceptionErrorMessage { get; }
	}
}
