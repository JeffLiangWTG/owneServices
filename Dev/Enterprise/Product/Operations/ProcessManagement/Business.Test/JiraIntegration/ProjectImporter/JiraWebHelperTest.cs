using System;
using System.Net;
using System.Net.Http;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using static Enterprise.ProcessManagement.Business.JiraConstants;

namespace Enterprise.ProcessManagement.Business.Test
{
	public class JiraWebHelperTest : TestCaseWithFactory
	{
		public void TestWebHelper_ReturnsApiVersion3ForCloudInstance()
		{
			var dummyWebHelper = new DummyJiraWebHelperToTestRequestJiraInstanceServerInfo();
			var jiraApiVersion = dummyWebHelper.GetJiraApiVersion(new Uri(JiraIntegrationTestHelper.DummyJiraLinks.DummyCloudBaseUri));
			AssertEquals(JiraAPIVersions.JiraRequestAPI_3, jiraApiVersion);
		}

		public void TestWebHelper_ReturnsApiVersion2ForServerInstance()
		{
			var dummyWebHelper = new DummyJiraWebHelperToTestRequestJiraInstanceServerInfo();
			var jiraApiVersion = dummyWebHelper.GetJiraApiVersion(new Uri(JiraIntegrationTestHelper.DummyJiraLinks.DummyServerBaseUri));
			AssertEquals(JiraAPIVersions.JiraRequestAPI_2, jiraApiVersion);
		}

		public void TestWebHelper_ReturnsNullForInvalidUrl()
		{
			var dummyWebHelper = new DummyJiraWebHelperToTestRequestJiraInstanceServerInfo();
			var jiraApiVersion = dummyWebHelper.GetJiraApiVersion(new Uri("https://www.google.com/"));
			AssertNullOrEmpty(jiraApiVersion);
		}

		public void TestWebHelper_HandlesMissingValuesInApiResponse()
		{
			var jiraResult = new JiraResult(JiraResponseStatus.Success, "");
			var dummyWebHelper = GetDummyWebHelper("Response status code does not indicate success: 404 ()");
			dummyWebHelper.MissingQueryValues(jiraResult, new HttpClient(), new Uri("https://www.google.com/"), new JiraCredentials("", ""), new ManyIssuesJiraQuery("Hello", 0));
			AssertEquals(JiraResponseStatus.GenericFailure, jiraResult.Status);
			AssertContains("The value 'Cancele' does not exist for the field 'status'.", jiraResult.Response);
			AssertContains("The value 'Potato' does not exist for the field 'status'.", jiraResult.Response);
			AssertContains("Please check your values in the Jira Registry", jiraResult.Response);
			AssertContains("https://www.google.com/", jiraResult.Response);
		}

		public void TestWebHelper_HandlesTrailingSlash()
		{
			var baseUrlWithoutTrailingSlash = "https://sampleweb.atlassian.net";
			var baseUrlWithTrailingSlash = "https://sampleweb.atlassian.net/";
			var expectedUrl = "https://sampleweb.atlassian.net/rest/api/3/";
			AssertEquals(expectedUrl, new JiraWebHelper().GetJiraRequestUri(new Uri(baseUrlWithoutTrailingSlash), ""));
			AssertEquals(expectedUrl, new JiraWebHelper().GetJiraRequestUri(new Uri(baseUrlWithTrailingSlash), ""));
		}

		public void TestWebHelper_UsesRegistryURLValue()
		{
			var sampleUrl = "https://sampleweb.atlassian.net";
			AssertEquals(sampleUrl + "/rest/api/3/", new JiraWebHelper().GetJiraRequestUri(new Uri(sampleUrl), ""));
		}

		public void TestWebHelper_ShouldHandleBadRequest()
		{
			var response = new JiraResult(JiraResponseStatus.Success, "");
			var jiraRequest = new AllProjectsJiraQuery();
			var dummyWebHelper = GetDummyWebHelper("Response status code does not indicate success: 404 ()");

			AssertNoExceptionThrown("Bad Request exceptions are handled masterfully", () => response = dummyWebHelper.SendJiraRequestAndGetResponse(new Uri("https://sampleweb.atlassian.net"), new JiraCredentials("", ""), jiraRequest, false));

			AssertEquals(JiraResponseStatus.InvalidRequest, response.Status);
			AssertEquals("We should also get an error report in this case, as urgent work must be done to remedy it.", "Jira API Request failed on the following query due to a bad request exception:\r\n'" + jiraRequest.QueryString + "'\r\n", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestWebHelper_ShouldHandleGeneralHttpExceptions()
		{
			var response = new JiraResult(JiraResponseStatus.Success, "");
			var dummyWebHelper = GetDummyWebHelper("oWo");

			AssertNoExceptionThrown("Generic exceptions are handled gracefully", () => response = dummyWebHelper.SendJiraRequestAndGetResponse(new Uri("https://sampleweb.atlassian.net"), new JiraCredentials("", ""), new AllProjectsJiraQuery(), false));

			AssertEquals(JiraResponseStatus.GenericFailure, response.Status);
		}

		public void TestWebHelper_ShouldHandleBadCredentials()
		{
			var response = new JiraResult(JiraResponseStatus.Success, "");
			var dummyWebHelper = GetDummyWebHelper("Response status code does not indicate success: 401");

			AssertNoExceptionThrown("Bad Credentials exceptions are handled serenely", () => response = dummyWebHelper.SendJiraRequestAndGetResponse(new Uri("https://sampleweb.atlassian.net"), new JiraCredentials("", ""), new AllProjectsJiraQuery(), false));

			AssertEquals(JiraResponseStatus.BadCredentials, response.Status);
		}

		public void TestWebHelper_ShouldHandleInsufficientPermissions()
		{
			var response = new JiraResult(JiraResponseStatus.Success, "");
			var dummyWebHelper = GetDummyWebHelper("The remote server returned an error: (403) Forbidden.");

			AssertNoExceptionThrown("Insufficient Permissions exceptions are handled serenely", () => response = dummyWebHelper.SendJiraRequestAndGetResponse(new Uri("https://sampleweb.atlassian.net"), new JiraCredentials("", ""), new AllProjectsJiraQuery(), false));

			AssertEquals(JiraResponseStatus.InsufficientPermissions, response.Status);
		}

		public void TestWebHelper_ShouldHandleInvalidUrlProtocol()
		{
			var response = new JiraResult(JiraResponseStatus.Success, "");
			var dummyWebHelper = GetDummyWebHelper("An invalid request URI was provided. The request URI must either be an absolute URI or BaseAddress must be set.");

			AssertNoExceptionThrown("Invalid Protocol exceptions are handled serenely", () => response = dummyWebHelper.SendJiraRequestAndGetResponse(new Uri("https://sampleweb.atlassian.net"), new JiraCredentials("", ""), new AllProjectsJiraQuery(), false));

			AssertEquals(JiraResponseStatus.FailedServerCall, response.Status);
		}

		public void TestWebHelper_ShouldHandleBadDomain()
		{
			var response = new JiraResult(JiraResponseStatus.Success, "");
			var dummyWebHelper = GetDummyWebHelper("Response status code does not indicate success: 404 (Not Found)");

			AssertNoExceptionThrown("Bad Doman exceptions are handled serenely", () => response = dummyWebHelper.SendJiraRequestAndGetResponse(new Uri("https://sampleweb.atlassian.net"), new JiraCredentials("", ""), new AllProjectsJiraQuery(), false));

			AssertEquals(JiraResponseStatus.FailedServerCall, response.Status);
		}

		public void TestWebHelper_ShouldHandleBadURLFormat()
		{
			var response = new JiraResult(JiraResponseStatus.Success, "");
			var jiraRequest = new AllProjectsJiraQuery();
			var dummyWebHelper = GetDummyWebHelper("The remote server returned an error: (503) Server Unavailable.");

			AssertNoExceptionThrown("Exceptions caused by bad URL formats are handled adroitly", () => response = dummyWebHelper.SendJiraRequestAndGetResponse(new Uri("https://sampleweb.atlassian.net"), new JiraCredentials("", ""), jiraRequest, false));

			AssertEquals(JiraResponseStatus.FailedServerCall, response.Status);
		}

		public void TestWebHelper_ShouldHandleTimeouts()
		{
			var response = new JiraResult(JiraResponseStatus.Success, "");
			var jiraRequest = new AllProjectsJiraQuery();
			var dummyWebHelper = GetDummyWebHelper("The remote server returned an error: (504) Server Unavailable.");

			AssertNoExceptionThrown("Bad Request exceptions are handled adeptly", () => response = dummyWebHelper.SendJiraRequestAndGetResponse(new Uri("https://sampleweb.atlassian.net"), new JiraCredentials("", ""), jiraRequest, false));

			AssertEquals(JiraResponseStatus.Timeout, response.Status);
		}

		public void TestWebHelper_ShouldHandleUserTypoesInProjectCodes_ShouldNotSendErrorReports()
		{
			var response = new JiraResult(JiraResponseStatus.Success, "");
			var dummyWebHelper = GetDummyWebHelper("Response status code does not indicate success: 404 ()");

			AssertNoExceptionThrown("Exceptions caused by user-typed code don't kill us", () => response = dummyWebHelper.SendJiraRequestAndGetResponse(new Uri("https://sampleweb.atlassian.net"), new JiraCredentials("", ""), new AllProjectsJiraQuery(), true));

			AssertEquals("At this point an InvalidRequestWithCustomQuery does not exist, so expect a normal InvalidRequest", JiraResponseStatus.InvalidRequest, response.Status);
			AssertEquals("We should have no errors reported in this API call error case since the failed query is not our fault", string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestWebHelper_ShouldHandleUserTypoesInCustomJQL_ShouldNotSendErrorReports()
		{
			var response = new JiraResult(JiraResponseStatus.Success, "");
			var dummyWebHelper = GetDummyWebHelper("Response status code does not indicate success: 400 ()");

			AssertNoExceptionThrown("Exceptions caused by user-typed code don't kill us", () => response = dummyWebHelper.SendJiraRequestAndGetResponse(new Uri("https://sampleweb.atlassian.net"), new JiraCredentials("", ""), new AllProjectsJiraQuery(), true));

			AssertEquals("At this point an InvalidRequestWithCustomQuery does not exist, so expect a normal InvalidRequest", JiraResponseStatus.InvalidRequest, response.Status);
			AssertEquals("We should have no errors reported in this API call error case since the failed query is not our fault", string.Empty, ErrorReporter.LastMessageReported);
		}

		#region Implementation

		public DummyJiraWebHelper GetDummyWebHelper(string messageToThrowAsException) => new DummyJiraWebHelper(messageToThrowAsException);

		public class DummyJiraWebHelper : JiraWebHelper
		{
			public DummyJiraWebHelper(string messageToThrowAsException)
			{
				ExceptionMessage = messageToThrowAsException;
			}

			string ExceptionMessage { get; }

			protected override Uri GetJiraRequestUriCore(Uri baseUri, string apiQuery, string jiraApiVersion = JiraAPIVersions.JiraRequestAPI_3)
			{
				return new Uri("https://www.google.com");
			}

			protected override string GetResultOfRequest(HttpClient request, Uri uri)
			{
				throw new HttpRequestException(ExceptionMessage);
			}

			protected override HttpResponseMessage GetFullResultOfRequest(HttpClient request, Uri uri)
			{
				var serializedString =
@"{
	""errorMessages"": [
		""The value 'Cancele' does not exist for the field 'status'."",
		""The value 'Potato' does not exist for the field 'status'."",
    ],
    ""warningMessages"": []
}";
				return new HttpResponseMessage(HttpStatusCode.NotFound)
				{
					Content = new StringContent(serializedString, System.Text.Encoding.UTF8, "application/json")
				};
			}
		}

		public class DummyJiraWebHelperToTestRequestJiraInstanceServerInfo : JiraWebHelper
		{
			public DummyJiraWebHelperToTestRequestJiraInstanceServerInfo()
			{
			}

			protected override HttpResponseMessage GetFullResultOfRequest(HttpClient request, Uri uri)
			{
				var successfulResponseForCloud =
@"{
	""baseUrl"": ""test.com"",
	""versionNumbers"": [
		1001,
		1002
    ],
    ""deploymentType"": ""Cloud""
}";

				var successfulResponseForServer =
@"{
	""baseUrl"": ""test.com"",
	""versionNumbers"": [
		1001,
		1002
    ],
    ""deploymentType"": ""Server""
}";
				var serializedString = String.Empty;
				switch (uri.AbsoluteUri)
				{
					case JiraIntegrationTestHelper.DummyJiraLinks.DummyCloudQuery:
						serializedString = successfulResponseForCloud;
						break;
					case JiraIntegrationTestHelper.DummyJiraLinks.DummyServerQuery:
						serializedString = successfulResponseForServer;
						break;
					default:
						serializedString = "<html>";
						break;
				}

				return new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(serializedString, System.Text.Encoding.UTF8, "application/json")
				};
			}
		}

		#endregion
	}
}
