using System;
using System.Collections.Generic;
using static Enterprise.ProcessManagement.Business.JiraConstants;

namespace Enterprise.ProcessManagement.Business
{
	public class ProjectImportReport
	{
		public ProjectImportReport(bool closeImportForm, string importReportMessage)
		{
			ShouldCloseImportForm = closeImportForm;
			ProjectReportMessage = importReportMessage;
		}

		public bool ShouldCloseImportForm { get; set; }
		public string ProjectReportMessage { get; set; }

		#region Implementation

		public override bool Equals(object obj)
		{
			var otherObject = (ProjectImportReport)obj;

			return otherObject != null
				&& ShouldCloseImportForm.Equals(otherObject.ShouldCloseImportForm)
				&& ProjectReportMessage.Equals(otherObject.ProjectReportMessage);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = 157926300;
				hashCode = hashCode * -1521134295 + ShouldCloseImportForm.GetHashCode();
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ProjectReportMessage);
				return hashCode;
			}
		}

		#endregion

		#region Import Reports

		public static string Success(string jiraApiVersion = JiraAPIVersions.JiraRequestAPI_3) =>
			jiraApiVersion.Equals(JiraAPIVersions.JiraRequestAPI_3)
			? Res.GetString("260E9F8D-E78F-4676-B6EA-07E47AE9DA1B", "Data Import from Atlassian Cloud is complete")
			: Res.GetString("d0d03200-ad1a-4cb9-9f44-c7a601995d6f", "Data Import from JIRA Self Hosted Server is complete");
		public static string PartiallySavedImport => Res.GetString("7E3D00AF-4BE8-4E5D-A5BF-749169C45648", "The Import process resulted in saved data. Please consult the Project and Work Item modules to determine what data was loaded, and what could not be retrieved.");
		public static string InvalidRequest => Res.GetString("9D52E6C6-6385-47C5-8530-773B54B4AB57", "The server request was unsuccessful due to an invalid query. Please make sure that you have entered the correct Jira URL and the Project Keys provided are valid. If you are importing all projects or if errors still occur, please raise an incident.");
		public static string InvalidRequestWithCustomQuery => Res.GetString("382DDADF-C830-4EA8-B0DF-05E701214990", "The server request was unsuccessful due to an invalid query. Please make sure that you have entered the correct Jira URL and that all custom JQL queries are correct. If you are importing all projects or if errors still occur, please raise an incident.");
		public static string BadCredentials => Res.GetString("B543D0BF-4CF6-4EC7-81DD-4B1F04B2575E", "An invalid username and password combination was provided. Please enter valid credentials and try again.");
		public static string InsufficientPermissions => Res.GetString("9CE9434F-6FA5-4CA7-B3CC-FE7A547E625E", "The Jira server reports that the requested resource cannot be viewed by the account whose credentials were provided. Please contact your Jira account administrator and try again.");
		public static string FailedServerCall(Uri uri) => Res.GetString("53663EEE-A6B8-4C97-95DB-D7FF864554FC", "A connection could not be established with the '{0}' Jira domain. Please validate you have entered the URL correctly in the system registry at:\r\n\t{1}", uri, ProcessManagementRegistry.Instance.JiraSiteUrls.Inner.Location);
		public static string IncorrectJiraUrl => Res.GetString("9b8c4203-c3ac-4d76-8b5b-8bf7da529455", "Data Import was unsuccessful. Please check the URL you have entered from JIRA is correct. If your entered the correct URL from JIRA and this error is still occurring, please raise an incident.");
		public static string UnspecifiedFailure => Res.GetString("F6CCC064-2897-4E9C-A5DE-2ED3981E2C08", "Something unexpected occurred during the import process, and it was terminated.");

		#endregion
	}
}
