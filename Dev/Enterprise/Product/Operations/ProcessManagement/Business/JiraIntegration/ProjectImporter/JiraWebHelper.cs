using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using CargoWise.Common;
using Newtonsoft.Json.Linq;
using static Enterprise.ProcessManagement.Business.JiraConstants;

namespace Enterprise.ProcessManagement.Business
{
	#region SuppressResourceStringsCheckRegion

	// TODO: remove this and refactor the JiraServiceProvider to use an interface instead
	public class JiraWebHelper
	{
		public static class FailedJiraRequestReasons
		{
			public const string BadCredentialsStatusCode = "Response status code does not indicate success: 401";
			public const string InsufficientPermissions = "The remote server returned an error: (403) Forbidden";

			public const string InvalidRequestStatusCode = "Response status code does not indicate success: 404 ()"; //  yes seriously, this is how they tell us a request is invalid
			public const string InvalidRequestedIDStatusCode = "Response status code does not indicate success: 400 ()"; // a 400 is the result of an ID being incorrect or not being found but is also only returned by the http client when the jql format is wrong, sometimes

			public static class FailedServerCallReasons
			{
				public const string ProtocolInvalidFormat = "An invalid request URI was provided. The request URI must either be an absolute URI or BaseAddress must be set";
				public const string ProtocolWrong = "Response status code does not indicate success: 404 (Not Found)"; // http:// instead of https://
				public const string BadUrlFormat = "The remote server returned an error: (503) Server Unavailable"; // the '.net' is wrong, or the '.atlassian' is wrong, or the structure of the whole thing is wrong (eg not Name.atlassian.net)
			}

			public const string TimeoutStatusCode = "504";
		}

		public virtual JiraResult SendJiraRequestAndGetResponse(Uri baseUri, JiraCredentials credentials, JiraQuery jiraQuery, bool includesUserTypedText, string jiraApiVersion = JiraAPIVersions.JiraRequestAPI_3)
		{
			var request = GetJiraRequestTemplate(credentials);
			var result = new JiraResult(JiraResponseStatus.Success, "");

			var uri = GetJiraRequestUri(baseUri, jiraQuery.QueryString, jiraApiVersion);

			try
			{
				result.Response = GetResultOfRequest(request, uri);
			}
			catch (AggregateException ex) when (!ex.IsCriticalException())
			{
				var innerException = ex.InnerException;

				while (innerException.InnerException != null)
				{
					innerException = innerException.InnerException;
				}
				if (!MissingQueryValues(result, request, uri, credentials, jiraQuery, jiraApiVersion))
				{
					result = DetectAndHandleException(innerException, jiraQuery, includesUserTypedText);
				}
			}
			catch (HttpRequestException ex) when (!ex.IsCriticalException())
			{
				if (!MissingQueryValues(result, request, uri, credentials, jiraQuery, jiraApiVersion))
				{
					result = DetectAndHandleException(ex, jiraQuery, includesUserTypedText);
				}
			}

			return result;
		}

		public bool MissingQueryValues(JiraResult result, HttpClient request, Uri jiraRequestUri, JiraCredentials credentials, JiraQuery jiraQuery, string jiraApiVersion = JiraAPIVersions.JiraRequestAPI_3)
		{
			if (jiraQuery is ManyIssuesJiraQuery)
			{
				var checkStatuses = GetFullResultOfRequest(request, jiraRequestUri);
				if (!checkStatuses.IsSuccessStatusCode)
				{
					var errorMessageResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(checkStatuses.Content.ReadAsStringAsync().Result) as JObject;
					var errorMessageList = (JArray)errorMessageResponse["errorMessages"];
					if (errorMessageList.Count == 0)
					{
						return false;
					}
					var errorMessageForUser = new CargoWise.Types.ZStringBuilder(Res.GetString("fe2c6a34-02c8-4985-90a6-5fb93e440d2f", "The import failed with the following error messages:") + System.Environment.NewLine);
					foreach (var errorMessage in errorMessageList.WhereNotNull())
					{
						var errorMessageString = (string)errorMessage;
						errorMessageForUser.Append(errorMessageString + System.Environment.NewLine);
					}
					errorMessageForUser.Append(System.Environment.NewLine + Res.GetString("de614f9a-59ac-49e1-89b8-ef2d30a263cc", "Please check your values in the Jira Registry and ensure you have selected the correct instance of Jira. If you still receive errors, please raise an incident."));
					errorMessageForUser.Append(Res.GetString("71c939ce-ac8b-469f-9fad-08f0b31cf0b4", "URL: {0}", jiraRequestUri));
					result.Status = JiraResponseStatus.GenericFailure;
					result.Response = errorMessageForUser.ToStringWithNewLineBetweenAppends();
					return true;
				}
			}
			return false;
		}
		JiraResult DetectAndHandleException(Exception ex, JiraQuery failedQuery, bool includesUserTypedText)
		{
			var message = ex.Message;
			var result = new JiraResult() { Status = JiraResponseStatus.Success, Response = message };

			if (message.Contains(FailedJiraRequestReasons.BadCredentialsStatusCode))
			{
				result.Status = JiraResponseStatus.BadCredentials;
			}
			else if (message.Contains(FailedJiraRequestReasons.InvalidRequestStatusCode)
				|| message.Contains(FailedJiraRequestReasons.InvalidRequestedIDStatusCode))
			{
				result.Status = JiraResponseStatus.InvalidRequest;

				if (!includesUserTypedText)
				{
					ErrorReporter.ReportOnce("JiraImporterTool|InvalidRequestDueToAQuery", "Jira API Request failed on the following query due to a bad request exception:\r\n'" + failedQuery.QueryString + "'\r\n", ex);
				}
			}
			else if (message.Contains(FailedJiraRequestReasons.FailedServerCallReasons.ProtocolInvalidFormat)
				|| message.Contains(FailedJiraRequestReasons.FailedServerCallReasons.ProtocolWrong)
				|| message.Contains(FailedJiraRequestReasons.FailedServerCallReasons.BadUrlFormat))
			{
				result.Status = JiraResponseStatus.FailedServerCall;
			}
			else if (message.Contains(FailedJiraRequestReasons.InsufficientPermissions))
			{
				result.Status = JiraResponseStatus.InsufficientPermissions;
			}
			else if (message.Contains(FailedJiraRequestReasons.TimeoutStatusCode))
			{
				result.Status = JiraResponseStatus.Timeout;
			}
			else
			{
				result.Status = JiraResponseStatus.GenericFailure;
			}

			if (ex.GetType() != typeof(HttpRequestException) && result.Status == JiraResponseStatus.Success)
			{
				throw ex;
			}

			return result;
		}

		public string GetJiraApiVersion(Uri baseUri)
		{
			var request = new HttpClient();

			var requestFromCloud = RequestJiraInstanceServerInfo(baseUri, request, JiraAPIVersions.JiraRequestAPI_3);
			if (!requestFromCloud.IsNullOrEmpty())
			{
				return JiraAPIVersions.JiraRequestAPI_3;
			}

			return RequestJiraInstanceServerInfo(baseUri, request, JiraAPIVersions.JiraRequestAPI_2);
		}

		public string RequestJiraInstanceServerInfo(Uri baseUri, HttpClient request, string jiraApiVersion)
		{
			var uri = GetJiraRequestUri(baseUri, "serverInfo", jiraApiVersion);

			var response = GetFullResultOfRequest(request, uri);
			if (!response.IsSuccessStatusCode)
			{
				return null;
			}

			var jsonBody = response.Content.ReadAsStringAsync().Result;
			try
			{
				var deserializedJsonBody = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonBody) as JObject;
				var deploymentType = (string)deserializedJsonBody["deploymentType"];

				if (deploymentType.Equals("Cloud"))
				{
					return JiraAPIVersions.JiraRequestAPI_3;
				}
				else if (deploymentType.Equals("Server"))
				{
					return JiraAPIVersions.JiraRequestAPI_2;
				}
			}
			catch
			{
				// Account for the 200 (success) response but the return value is HTML instead of JSON (incorrect URL)
				// Could catch other cases e.g. if the "deploymentType" field is missing in the JSON body
				return null;
			}

			return null;    // If something else went wrong e.g. deploymentType is not equal to cloud or server
		}

		protected virtual string GetResultOfRequest(HttpClient request, Uri uri)
		{
			return request.GetStringAsync(uri).Result;
		}

		protected virtual HttpResponseMessage GetFullResultOfRequest(HttpClient request, Uri uri)
		{
			return request.GetAsync(uri).Result;
		}

		public static byte[] DownloadFile(JiraCredentials credentials, Uri uri)
		{
			var request = GetJiraRequestTemplate(credentials);

			try
			{
				using (var response = request.GetAsync(uri).Result)
				using (var streamToReadFrom = response.Content.ReadAsStreamAsync().Result)
				{
					return streamToReadFrom.ReadFully();
				}
			}
			catch (HttpRequestException ex) when (!ex.IsCriticalException()) // TODO: figure out the exact exceptions we expect here. Candidates are 400 for a bad query, 404 for bad permissions or the object not existing
			{
				return null;
			}
		}

		static HttpClient GetJiraRequestTemplate(JiraCredentials credentials)
		{
			var request = new HttpClient();
			var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials.Username + ":" + credentials.Password));
			request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth); // never-seen constant

			return request;
		}

		public Uri GetJiraRequestUri(Uri baseUri, string apiQuery, string jiraApiVersion = JiraAPIVersions.JiraRequestAPI_3)
		{
			return GetJiraRequestUriCore(baseUri, apiQuery, jiraApiVersion);
		}

		protected virtual Uri GetJiraRequestUriCore(Uri baseUri, string apiQuery, string jiraApiVersion = JiraAPIVersions.JiraRequestAPI_3)
		{
			if (baseUri.AbsolutePath.EndsWith("/"))
			{
				return new Uri(baseUri + jiraApiVersion + apiQuery);
			}
			return new Uri(baseUri + "/" + jiraApiVersion + apiQuery);
		}
	}

	#endregion
}
