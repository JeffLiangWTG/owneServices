using System;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.OnlineSailingSchedules.Exceptions;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Freight.OnlineSailingSchedules.Testing
{
	public class AutoInitiatedServiceRequestManagerTest : AdjustableServiceRequestManagerTest
	{
		protected override AdjustableServiceRequestManager CreateManager()
		{
			return new AutoInitiatedServiceRequestManager(Notifications);
		}

		[TestDate(2016, 10, 22)]
		public override void TestTaskCanceledException_CallOnTimeOutRequest()
		{
			var manager = CreateManager();
			var ex = new TaskCanceledException();

			var utcNow = ZDateTime.UtcNow;

			manager.SuppressionLevel = 0;
			manager.SuppressUntilUtc = utcNow.AddMinutes(-10);
			manager.HandleException(new RequestHandlingException(null, null, ex));
			AssertEquals(1, manager.SuppressionLevel);
			AssertEquals(1, (manager.SuppressUntilUtc - utcNow).Minutes);
		}

		protected override bool IsAutoInitiatedServiceRequestTest => true;
	}

	public class UserInitiatedServiceRequestManagerTest : AdjustableServiceRequestManagerTest
	{
		protected override AdjustableServiceRequestManager CreateManager()
		{
			return new UserInitiatedServiceRequestManager(Notifications);
		}
	}

	public abstract class AdjustableServiceRequestManagerTest : TestCaseWithFactory
	{
		public void TestRequestTimeout()
		{
			var manager = CreateManager();
			var expectedRequestTimeout = IsAutoInitiatedServiceRequestTest ? 5 : 20;
			AssertEquals(TimeSpan.FromSeconds(expectedRequestTimeout), manager.RequestTimeout);
		}

		public void TestIsSuppressed()
		{
			var manager = CreateManager();

			manager.SuppressUntilUtc = ZDateTime.UtcNow.AddMinutes(-1);
			AssertEquals(false, manager.IsSuppressed());

			manager.SuppressUntilUtc = ZDateTime.UtcNow.AddMinutes(1);
			AssertEquals(true, manager.IsSuppressed());
		}

		public void TestOnSuccessfullRequest_SuppressionLevelShouldBeResetToZero()
		{
			var manager = CreateManager();
			manager.SuppressionLevel = 2;
			manager.OnSuccessfulRequest();
			AssertEquals(0, manager.SuppressionLevel);

			manager.OnSuccessfulRequest();
			AssertEquals(0, manager.SuppressionLevel);
		}

		[TestDate(2016, 10, 22)]
		public void TestOnTimedOutRequest_IncreaseSuppressionLevel_SetNewSuppressUntilUTC()
		{
			var manager = CreateManager();
			var utcNow = ZDateTime.UtcNow;

			manager.SuppressionLevel = 0;
			manager.SuppressUntilUtc = utcNow.AddMinutes(-10);
			manager.OnTimedOutRequest();
			AssertEquals(1, manager.SuppressionLevel);
			AssertEquals(1, (manager.SuppressUntilUtc - utcNow).Minutes);

			manager.OnTimedOutRequest();
			AssertEquals(2, manager.SuppressionLevel);
			AssertEquals(2, (manager.SuppressUntilUtc - utcNow).Minutes);

			manager.OnTimedOutRequest();
			AssertEquals(4, manager.SuppressionLevel);
			AssertEquals(4, (manager.SuppressUntilUtc - utcNow).Minutes);

			manager.OnTimedOutRequest();
			AssertEquals(8, manager.SuppressionLevel);
			AssertEquals(8, (manager.SuppressUntilUtc - utcNow).Minutes);

			manager.OnTimedOutRequest();
			AssertEquals(16, manager.SuppressionLevel);
			AssertEquals(16, (manager.SuppressUntilUtc - utcNow).Minutes);

			manager.OnTimedOutRequest();
			AssertEquals("Maximum Suppression Level", 16, manager.SuppressionLevel);
			AssertEquals("Maximum Suppression Time", 16, (manager.SuppressUntilUtc - utcNow).Minutes);
		}

		[TestDate(2016, 10, 22)]
		public virtual void TestTaskCanceledException_CallOnTimeOutRequest()
		{
			var manager = CreateManager();
			var ex = new TaskCanceledException();

			var utcNow = ZDateTime.UtcNow;

			manager.SuppressionLevel = 0;
			manager.SuppressUntilUtc = utcNow.AddMinutes(-10);
			manager.HandleException(new RequestHandlingException(null, null, ex));
			AssertEquals(0, manager.SuppressionLevel);
			AssertEquals(-10, (manager.SuppressUntilUtc - utcNow).Minutes);
		}

		public void TestHandleException_BadRequestException()
		{
			var exception = new BadRequestException("Unknown error", Array.Empty<string>(), "12345");
			var notification = GetManagerExceptionErrorMessage();
			HandleExceptionTest(exception, notification, true);
		}

		public void TestHandleException_UnauthorizedException()
		{
			var exception = new UnauthorizedException("Authentication parameters are not provided");
			var notification = "Authentication parameters are not provided. Please contact support.";
			HandleExceptionTest(exception, notification, false);
		}

		public void TestHandleException_TooManyRecordsReturnedException()
		{
			var exception = new TooManyRecordsReturnedException();
			var notification = "Too many records were returned. Please modify the filters to narrow down your search.";
			HandleExceptionTest(exception, notification, false);
		}

		public void TestHandleException_NumberOfRecordsReturnedLimitException()
		{
			var errorMessage = "More than 500 records returned.";
			var exception = new BadRequestException(errorMessage, new[] { errorMessage }, "12345");
			var notification = "Too many records were returned. Please modify the filters to narrow down your search.";
			HandleExceptionTest(exception, notification, false);
		}

		public void TestHandleException_MultipleErrorMessages()
		{
			var loadingUnlocoError = "Port of Loading UNLOCO is invalid. It must be 5 characters long.";
			var dischargeUnlocoError = "Port of Discharge UNLOCO is invalid. It must be 5 characters long.";
			var exception = new BadRequestException($"{loadingUnlocoError}{System.Environment.NewLine}{dischargeUnlocoError}", new[] { loadingUnlocoError, dischargeUnlocoError }, "12345");
			var notification = $"Origin UNLOCO is invalid. It must be 5 characters long.{System.Environment.NewLine}Destination UNLOCO is invalid. It must be 5 characters long.";
			HandleExceptionTest(exception, notification, false);
		}

		public void TestHandleException_MultipleErrorMessages_OneUnknown()
		{
			var loadingUnlocoError = "Port of Loading UNLOCO is invalid. It must be 5 characters long.";
			var unknownError = "Unknown error";
			var exception = new BadRequestException($"{loadingUnlocoError}{System.Environment.NewLine}{unknownError}", new[] { loadingUnlocoError, unknownError }, "12345");
			var notigicationMessage = $"Origin UNLOCO is invalid. It must be 5 characters long.{System.Environment.NewLine}{GetManagerExceptionErrorMessage()}";
			var manager = CreateManager();
			manager.HandleException(new RequestHandlingException(null, null, exception));

			AssertEquals(notigicationMessage, Notifications.AsString.Trim('\r', '\n'));

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			Assert(ErrorReporter.LastMessageReported.StartsWith(GetManagerExceptionErrorMessage()));
			ErrorReporter.Clear();
		}

		public void TestHandleException_ErrorCatalog()
		{
			var errorMessages = ErrorMessageCatalog.ErrorMessageCollection.Keys;
			foreach (var errorMessage in errorMessages)
			{
				Notifications.Clear();
				ErrorReporter.Clear();
				var exception = new BadRequestException(errorMessage, new[] { errorMessage }, "12345");
				var notification = ErrorMessageCatalog.GetErrorMessage(errorMessage);
				HandleExceptionTest(exception, notification, false);
			}
		}

		public void TestHandleException_InternalServerErrorException()
		{
			var exception = new InternalServerErrorException("Detailed error message of internal server error exception");
			var notification = GetManagerExceptionErrorMessage();
			HandleExceptionTest(exception, notification, false);
		}

		public void TestHandleException_TaskCanceledException()
		{
			var exception = new AggregateException(new TaskCanceledException());
			var notification = GetManagerExceptionErrorMessage();
			HandleExceptionTest(exception, notification, true);
		}

		public void TestHandleException_ProxyAuthenticationRequired()
		{
			var exception = new HttpRequestException(string.Empty, new WebException("The remote server returned an error: (407) Proxy Authentication Required."));
			var notification = IsAutoInitiatedServiceRequestTest ? "" : "Please configure your proxy server to be able to use Global Sailing Schedules.";
			HandleExceptionTest(exception, notification, false);
		}

		public void TestHandleException_InvalidRemoteCertificate()
		{
			var exception = new HttpRequestException(string.Empty,
				new WebException("The underlying connection was closed: Could not establish trust relationship for the SSL/TLS secure channel.",
				new AuthenticationException("The remote certificate is invalid according to the validation procedure.")));
			var notification = IsAutoInitiatedServiceRequestTest ? "" : "Please check that your certificate is installed and enabled or check your proxy server settings to be able to use Global Sailing Schedules.";
			HandleExceptionTest(exception, notification, false);
		}

		public void TestHandleException_NameResolutionFailure()
		{
			var exception = new HttpRequestException(string.Empty, new WebException(string.Empty, WebExceptionStatus.NameResolutionFailure));
			var notification = IsAutoInitiatedServiceRequestTest ? "" : "Unable to connect to Global Sailing Schedules. Please check that connection URL specified in the registry is correct.";
			HandleExceptionTest(exception, notification, false);
		}

		public void TestHandleException_ConnectFailure()
		{
			var exception = new HttpRequestException(string.Empty, new WebException(string.Empty, WebExceptionStatus.ConnectFailure));
			var notification = IsAutoInitiatedServiceRequestTest ? "" : "Unable to connect to Global Sailing Schedules. Please check that connection URL specified in the registry is correct.";
			HandleExceptionTest(exception, notification, false);
		}

		public void TestHandleException_ConnectionClosed()
		{
			var exception = new HttpRequestException(string.Empty, new WebException(string.Empty, WebExceptionStatus.ConnectionClosed));
			var notification = IsAutoInitiatedServiceRequestTest ? "" : "Unable to connect to Global Sailing Schedules. Please check that connection URL specified in the registry is correct.";
			HandleExceptionTest(exception, notification, false);
		}

		public void TestHandleException_UnknownWebException()
		{
			var exception = new HttpRequestException(string.Empty, new WebException(string.Empty, WebExceptionStatus.UnknownError));
			var notification = GetManagerExceptionErrorMessage();
			HandleExceptionTest(exception, notification, true);
		}

		public void TestHandleException_UnknownException()
		{
			var exception = new Exception("Unknown exception");
			var notification = GetManagerExceptionErrorMessage();
			HandleExceptionTest(exception, notification, true);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			CleanupTimeoutSettings();

			Notifications.Clear();
			ErrorReporter.Clear();
		}

		protected abstract AdjustableServiceRequestManager CreateManager();

		protected NotificationBuffer Notifications
		{
			get { return notifications ?? (notifications = new NotificationBuffer()); }
		}
		NotificationBuffer notifications;

		protected virtual bool IsAutoInitiatedServiceRequestTest => false;

		void HandleExceptionTest(Exception exception, string message, bool errorReport)
		{
			var manager = CreateManager();
			manager.HandleException(new RequestHandlingException(null, null, exception));

			AssertEquals(message, Notifications.AsString.Trim('\r', '\n'));

			if (errorReport)
			{
				AssertEquals(1, ErrorReporter.TotalErrorCount);
				Assert(ErrorReporter.LastMessageReported.StartsWith(message));
				ErrorReporter.Clear();
			}
			else
			{
				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}
		}

		string GetManagerExceptionErrorMessage()
		{
			var manager = CreateManager();
			return manager.ExceptionErrorMessage;
		}

		void CleanupTimeoutSettings()
		{
			TimeoutSettings.Instance.OnlineSailingSchedulesAutoInitiatedSuppressionLevel = 0;
			TimeoutSettings.Instance.OnlineSailingSchedulesAutoInitiatedSuppressUntilUtc = DateTime.MinValue;
			TimeoutSettings.Instance.OnlineSailingSchedulesUserInitiatedSuppressionLevel = 0;
			TimeoutSettings.Instance.OnlineSailingSchedulesUserInitiatedSuppressUntilUtc = DateTime.MinValue;
		}

		#endregion
	}
}
