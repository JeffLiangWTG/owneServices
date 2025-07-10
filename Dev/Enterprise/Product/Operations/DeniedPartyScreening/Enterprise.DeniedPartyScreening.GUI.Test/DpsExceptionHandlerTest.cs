using System;
using System.Net;
using System.ServiceModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	public class DpsExceptionHandlerTest : TransactionedTestCase
	{
		public void TestDeniedPartyServiceURL_WhenDeveloperExceptionIsSuppressed()
		{
			SuppressionExceptionCore(true, () =>
			{
				AssertEquals("No developer exception is thrown", 0, ExceptionReporterTestListener.Instance.Count);
			});
		}

		public void TestDeniedPartyServiceURL_WhenDeveloperExceptionIsNotSuppressed()
		{
			SuppressionExceptionCore(false, () =>
			{
				AssertEquals("One developer exception is thrown", 1, ExceptionReporterTestListener.Instance.Count);
			});
		}

		public void TestDeniedPartyServiceURL_WithForbiddenException()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ExceptionReporterTestListener.Instance.Clear();
			AssertEquals("Precondition", 0, ExceptionReporterTestListener.Instance.Count);
			var exception = new Exception("There was no endpoint listening at", new WebException("The remote server returned an error: (403) Forbidden."));
			DpsExceptionHandler.Process(exception);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains($"Error 403 Forbidden. Please contact your network administrator and advise that correct ownership of content is granted for {DpsExceptionHandler.GetServiceUrlsString()}."));
			AssertEquals("No report was created", 0, ExceptionReporterTestListener.Instance.Count);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestDeniedPartyServiceURL_WithNoEndPointListener()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ExceptionReporterTestListener.Instance.Clear();
			AssertEquals("Precondition", 0, ExceptionReporterTestListener.Instance.Count);
			var exception = new EndpointNotFoundException($"There was no endpoint listening at {DpsExceptionHandler.GetServiceUrlsString()} that could accept the message.");
			DpsExceptionHandler.Process(exception);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains($"There was no endpoint listening at {DpsExceptionHandler.GetServiceUrlsString()} that could accept the message. This is often caused by an incorrect address or SOAP action."));
			AssertEquals("No report was created", 0, ExceptionReporterTestListener.Instance.Count);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestDeniedPartyServiceURL_ServiceCouldNotBeActivated()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ExceptionReporterTestListener.Instance.Clear();
			AssertEquals("Precondition", 0, ExceptionReporterTestListener.Instance.Count);
			var exception = new EndpointNotFoundException($"The requested service, '{DpsExceptionHandler.GetServiceUrlsString()}' could not be activated.");
			DpsExceptionHandler.Process(exception);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains($"The requested service, '{DpsExceptionHandler.GetServiceUrlsString()}' could not be activated. Please try the requested operation later."));
			AssertEquals("No report was created", 0, ExceptionReporterTestListener.Instance.Count);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestProcessNetworkExceptions()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ExceptionReporterTestListener.Instance.Clear();
			AssertEquals("Precondition", 0, ExceptionReporterTestListener.Instance.Count);

			var networkErrors = new string[]
			{
				"An error occurred while updating the entries",
				"An error occurred while executing the command definition",
				"An error occurred while receiving the HTTP response",
				"One or more errors occurred",
				"The underlying provider failed on Open"
			};

			Array.ForEach(networkErrors, o =>
			{
				using (new DisposableAction(() =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ExceptionReporterTestListener.Instance.Clear();
				}))
				{
					var exception = new Exception(o);
					DpsExceptionHandler.Process(exception);

					CombineAssertions("Show respective error message but not send the error back to us", () =>
					{
						AssertEquals("No report was created", 0, ExceptionReporterTestListener.Instance.Count);
						AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(@"Unable to access the Denied Party Screening service on the CargoWise server.

Please try again. If the issue persists, please submit an eRequest."));
						AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.Contains(@"Additional Exception Info: Denied Party Screening has encountered errors with each of the service urls:"));
					});
				}
			});
		}

		public void TestProcess407ProxyAuthenticationException()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ExceptionReporterTestListener.Instance.Clear();
			AssertEquals("Precondition", 0, ExceptionReporterTestListener.Instance.Count);

			var exception = new Exception("There was no endpoint listening at", new WebException("The remote server returned an error: (407) Proxy Authentication Required."));
			DpsExceptionHandler.Process(exception);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains($"Error 407 Proxy Authentication Required. Please contact your network administrator and advise that access is required for {DpsExceptionHandler.GetServiceUrlsString()}."));
			AssertEquals("No report was created", 0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestProcessRetryFailureException()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var exception = new AggregateException(new DpsCommunicationException("Additional Message", new Exception("There was no endpoint listening at", new WebException("The remote server returned an error: (407) Proxy Authentication Required."))));
			DpsExceptionHandler.Process(exception, true);
			CombineAssertions(() =>
			{
				AssertContains(System.Environment.NewLine + "Additional Exception Info: Additional Message", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(
					$"Error 407 Proxy Authentication Required. Please contact your network administrator and advise that access is required for {DpsExceptionHandler.GetServiceUrlsString()}.",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No report was created", 0, ExceptionReporterTestListener.Instance.Count);
			});

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			GetDummyUserAndBranch(out var loginName, out var branchPK);
			exception = new AggregateException(new DpsCommunicationException("Additional Message", new Exception("Unknown Error")));

			using (Env.SetTemporaryUserContext(loginName, branchPK, Env.CurrentDepartment.PK))
			{
				DpsExceptionHandler.Process(exception);
			}

			CombineAssertions(() =>
			{
				AssertContains("Additional Message", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("One error report was created", 1, ExceptionReporterTestListener.Instance.Count);
				AssertEquals("Unknown Error", ExceptionReporterTestListener.Instance[0].InnerException.Message);
			});

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestProcess_When_AuthCertNotFoundException_Should_OnlyShowMessageAndNotReport()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var exception = new AuthCertNotFoundException();
			DpsExceptionHandler.Process(exception);

			CombineAssertions(() =>
			{
				AssertEquals("Unable to retrieve Denied Party Status at this time. Please contact your System Administrator for assistance.\r\n\r\nError details: System to system trust certificate not found.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No error report was created", 0, ExceptionReporterTestListener.Instance.Count);
			});
		}

		void SuppressionExceptionCore(bool shouldSuppressException, Action assertExceptionCreated)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ExceptionReporterTestListener.Instance.Clear();
			AssertEquals("Precondition", 0, ExceptionReporterTestListener.Instance.Count);

			GetDummyUserAndBranch(out var loginName, out var branchPK);

			var exception = new Exception("The formatter threw an exception while trying to deserialize the message", new WebException("The formatter threw an exception while trying to deserialize the message."));

			using (Env.SetTemporaryUserContext(loginName, branchPK, Env.CurrentDepartment.PK))
			{
				DpsExceptionHandler.Process(exception, shouldSuppressException);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("The formatter threw an exception while trying to deserialize the message"));
				assertExceptionCreated();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		void GetDummyUserAndBranch(out string loginName, out Guid branchPK)
		{
			loginName = "first.second";
			var factory = new BusinessObjectFactory();
			var testStaff = factory.NewWithValidTestData<GlbStaff>();
			testStaff.GS_LoginName = loginName;

			var company = factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "SDF";
			company.GC_Name = "SDF Company";

			var branch = company.Branches.AddNew();
			branch.GB_Code = "OPI";
			branchPK = branch.PK.ToGuid();

			factory.Save();
		}
	}
}
