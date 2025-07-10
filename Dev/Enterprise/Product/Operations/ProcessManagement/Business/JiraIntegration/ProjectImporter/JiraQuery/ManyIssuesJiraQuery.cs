using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Enterprise.ProcessManagement.Business
{
	public class ManyIssuesJiraQuery : JiraQuery
	{
		public ManyIssuesJiraQuery(string projectKey, int batchNumber, string alreadyAssembledIssueStatusQuery = "", string customIssueQuery = "")
		{
			ProjectKey = ProjectJiraQuery.GetCorrectedProjectKey(projectKey);
			BatchNumber = batchNumber;
			IssueStatusCondition = (string.IsNullOrEmpty(alreadyAssembledIssueStatusQuery) ? string.Empty : AndQueryPart) + alreadyAssembledIssueStatusQuery;
			CustomIssueQueryFull = customIssueQuery + (string.IsNullOrEmpty(customIssueQuery) ? string.Empty : AndQueryPart);
		}

		string ProjectKey { get; }
		int BatchNumber { get; }
		string IssueStatusCondition { get; }
		string CustomIssueQueryFull { get; }

		public override string QueryString => AssembleQuery(JquQueryPart, CustomIssueQueryFull, IssueQueryByProjectQueryPart, ProjectKey, IssueStatusCondition, GetStartAtQueryString(), UnlimitedRequestResults, IncludeAllFields, ExpandRenderedFields);

		string GetStartAtQueryString()
		{
			return StartAtQueryPart + (BatchNumber * JiraDownloadBatchSize);
		}

		#region SuppressResourceStringsCheckRegion

		const string IssueQueryByProjectQueryPart = "project=";
		const string IssueQueryByStatusQueryPart = "status+in+({0})";
		const string IssueQueryByNotStatusQueryPart = "status+not+in+({0})";

		const string UnlimitedRequestResults = "&maxResults=-1";
		const string IncludeAllFields = "&fields=*all";
		const string ExpandRenderedFields = "&expand=renderedFields";

		const string StartAtQueryPart = "&startAt=";
		const string JquQueryPart = "search?jql=";
		const string AndQueryPart = "+and+";

		#endregion

		public const int JiraDownloadBatchSize = 100; // This is set by Jira (at their pleasure, without notice or updates to documentation). You can lower it but please don't exceed 100.

		#region Issue Status Query Helper Methods

		public static string GetIssueStatusesQueryString(IssueStatusModifiers issueStatusesModifier)
		{
			var relevantIssueStatuses = GetRelevantIssueStatuses(issueStatusesModifier);

			if (!relevantIssueStatuses.Any())
			{
				return string.Empty; // found no issue statuses to query, don't add condition for issue status
			}

			var formattedStatusList = FormatIssueStatusesForQuery(relevantIssueStatuses);

			switch (issueStatusesModifier)
			{
				case IssueStatusModifiers.None:
				case IssueStatusModifiers.Completed | IssueStatusModifiers.Unstarted | IssueStatusModifiers.WorkInProgress:
					return "";

				case IssueStatusModifiers.Completed:
				case IssueStatusModifiers.WorkInProgress:
				case IssueStatusModifiers.Completed | IssueStatusModifiers.WorkInProgress:
					return string.Format(CultureInfo.InvariantCulture, IssueQueryByStatusQueryPart, formattedStatusList);

				case IssueStatusModifiers.Unstarted:
				case IssueStatusModifiers.Completed | IssueStatusModifiers.Unstarted:
				case IssueStatusModifiers.Unstarted | IssueStatusModifiers.WorkInProgress:
					return string.Format(CultureInfo.InvariantCulture, IssueQueryByNotStatusQueryPart, formattedStatusList);

				default:
					throw new InvalidOperationException("A default case in a switch statement was reached when it should never be reached.");
			}
		}

		static string[] GetRelevantIssueStatuses(IssueStatusModifiers issueStatusesModifier)
		{
			var statusesToQuery = new List<string>();

			switch (issueStatusesModifier)
			{
				case IssueStatusModifiers.None:
				case IssueStatusModifiers.Completed | IssueStatusModifiers.Unstarted | IssueStatusModifiers.WorkInProgress:
					break;

				case IssueStatusModifiers.Completed:
				case IssueStatusModifiers.Unstarted | IssueStatusModifiers.WorkInProgress:
					statusesToQuery.AddRange(ProcessManagementRegistry.Instance.CompletedIssueStatusTypesMapping.Value);
					break;

				case IssueStatusModifiers.Unstarted:
				case IssueStatusModifiers.Completed | IssueStatusModifiers.WorkInProgress:
					statusesToQuery.AddRange(ProcessManagementRegistry.Instance.CompletedIssueStatusTypesMapping.Value);
					statusesToQuery.AddRange(ProcessManagementRegistry.Instance.WorkInProgressIssueStatusTypesMapping.Value);
					break;

				case IssueStatusModifiers.Completed | IssueStatusModifiers.Unstarted:
				case IssueStatusModifiers.WorkInProgress:
					statusesToQuery.AddRange(ProcessManagementRegistry.Instance.WorkInProgressIssueStatusTypesMapping.Value);
					break;

				default:
					throw new InvalidOperationException("A default case in a switch statement was reached when it should never be reached.");
			}

			return statusesToQuery.ToArray();
		}

		static string FormatIssueStatusesForQuery(string[] issueStatuses)
		{
			return string.Join(",", issueStatuses.Select(status => $"\"{status}\"")); // necessary constant string, it shan't be translated
		}

		#endregion
	}
}
