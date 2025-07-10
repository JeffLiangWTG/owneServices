using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ProcessManagement.Business.Test
{
	public class JiraQueryTest : TestCaseWithFactory
	{
		#region New Query

		public void TestGeneratesAllProjectQuery()
		{
			var newJiraQuery = new AllProjectsJiraQuery();
			AssertEquals("Should generate a query to request all Jira Projects", "project/?expand=description,lead,issueTypes,url,projectKeys", newJiraQuery.QueryString);
		}

		public void TestGeneratesOneProjectQuery()
		{
			var newJiraQuery = new OneProjectJiraQuery("Ceasar");
			AssertEquals("Should generate a query to request all Jira Projects", "project/Ceasar/?expand=description,lead,issueTypes,url,projectKeys", newJiraQuery.QueryString);
		}

		public void TestGeneratesManyIssuesQuery()
		{
			var newJiraQuery = new ManyIssuesJiraQuery("Hello", 0);
			AssertEquals("Should generate a query to request all Jira Projects, with *ALL* fields so that we don't miss anything. Pls don't remove *all ;)", "search?jql=project=Hello&startAt=0&maxResults=-1&fields=*all&expand=renderedFields", newJiraQuery.QueryString);
		}

		public void TestGeneratesManyIssuesQuery_NoProjectCodeRegisters()
		{
			var newJiraQuery = new ManyIssuesJiraQuery(string.Empty, 0);
			AssertEquals("Should generate a query to request all Jira Projects, with *ALL* fields so that we don't miss anything. Pls don't remove *all ;)", "search?jql=project=NoProject&startAt=0&maxResults=-1&fields=*all&expand=renderedFields", newJiraQuery.QueryString);
		}

		public void TestGeneratesManyIssuesQuery_WithStatusImport()
		{
			var newJiraQuery = new ManyIssuesJiraQuery("Henlo", 0, alreadyAssembledIssueStatusQuery: "status+in+(\"Done\")");
			AssertEquals("Should generate a query to request all Jira Projects", "search?jql=project=Henlo+and+status+in+(\"Done\")&startAt=0&maxResults=-1&fields=*all&expand=renderedFields", newJiraQuery.QueryString);
		}

		public void TestGeneratesManyIssuesWithCustomQueryQuery()
		{
			var newJiraQuery = new ManyIssuesJiraQuery("Hello", 0, customIssueQuery: "description=\"Yello\"");
			AssertEquals("Should generate a query to request all Jira Projects", "search?jql=description=\"Yello\"+and+project=Hello&startAt=0&maxResults=-1&fields=*all&expand=renderedFields", newJiraQuery.QueryString);
		}

		public void TestGeneratesManyIssuesWithCustomQueryQuery_NoCustomQueryStillMakesUsableQuery()
		{
			var newJiraQuery = new ManyIssuesJiraQuery("Henlo", 0, alreadyAssembledIssueStatusQuery: "status+in+(\"Done\")", customIssueQuery: "description=\"also me\"");
			AssertEquals("Should generate a query to request all Jira Projects", "search?jql=description=\"also me\"+and+project=Henlo+and+status+in+(\"Done\")&startAt=0&maxResults=-1&fields=*all&expand=renderedFields", newJiraQuery.QueryString);
		}

		public void TestManyIssuesQuery_StartAt()
		{
			var query = new ManyIssuesJiraQuery("Wsup", 0);
			AssertEquals("The first batch should start at 0. SAD!", "search?jql=project=Wsup&startAt=0&maxResults=-1&fields=*all&expand=renderedFields", query.QueryString);

			query = new ManyIssuesJiraQuery("Wsup", 1);
			AssertEquals("The query should skip 100 for each batch, because that's the max results that Jira will return. SAD!", "search?jql=project=Wsup&startAt=100&maxResults=-1&fields=*all&expand=renderedFields", query.QueryString);

			query = new ManyIssuesJiraQuery("Wsup", 2);
			AssertEquals("The query should skip 100 for each batch, because that's the max results that Jira will return. SAD!", "search?jql=project=Wsup&startAt=200&maxResults=-1&fields=*all&expand=renderedFields", query.QueryString);
		}

		public void TestGeneratesOneStaffQuery()
		{
			var newJiraQuery = new OneUserJiraQuery("Ulysses");
			AssertEquals("Should generate a query to request all Jira Projects", "user?accountId=Ulysses", newJiraQuery.QueryString);
		}

		#endregion

		#region Issue Status Assembly

		public void TestIssueStatusQueryAssembler_NoIssueStatuses()
		{
			SetDefaultIssueStatusRegistryValues();

			var issueQuery = ManyIssuesJiraQuery.GetIssueStatusesQueryString(IssueStatusModifiers.None);
			AssertEquals("Should make an issue status query with only WIP statuses", "", issueQuery);
		}

		public void TestIssueStatusQueryAssembler_CompletedIssueStatuses()
		{
			SetDefaultIssueStatusRegistryValues();

			var issueQuery = ManyIssuesJiraQuery.GetIssueStatusesQueryString(IssueStatusModifiers.Completed);
			AssertEquals("Should make an issue status query with only WIP statuses", "status+in+(\"Comp1\",\"Comp2\",\"Comp3\")", issueQuery);
		}

		public void TestIssueStatusQueryAssembler_UnstartedIssueStatuses()
		{
			SetDefaultIssueStatusRegistryValues();

			var issueQuery = ManyIssuesJiraQuery.GetIssueStatusesQueryString(IssueStatusModifiers.Unstarted);
			AssertEquals("Should make an issue status query with only WIP statuses", "status+not+in+(\"Comp1\",\"Comp2\",\"Comp3\",\"WIP1\",\"WIP2\")", issueQuery);
		}

		public void TestIssueStatusQueryAssembler_WIPIssueStatuses()
		{
			SetDefaultIssueStatusRegistryValues();

			var issueQuery = ManyIssuesJiraQuery.GetIssueStatusesQueryString(IssueStatusModifiers.WorkInProgress);
			AssertEquals("Should make an issue status query with only WIP statuses", "status+in+(\"WIP1\",\"WIP2\")", issueQuery);
		}

		public void TestIssueStatusQueryAssembler_CompletedAndUnstartedIssueStatuses()
		{
			SetDefaultIssueStatusRegistryValues();

			var issueQuery = ManyIssuesJiraQuery.GetIssueStatusesQueryString(IssueStatusModifiers.Completed | IssueStatusModifiers.Unstarted);
			AssertEquals("Should make an issue status query with only WIP statuses", "status+not+in+(\"WIP1\",\"WIP2\")", issueQuery);
		}

		public void TestIssueStatusQueryAssembler_CompletedAndWIPIssueStatuses()
		{
			SetDefaultIssueStatusRegistryValues();

			var issueQuery = ManyIssuesJiraQuery.GetIssueStatusesQueryString(IssueStatusModifiers.Completed | IssueStatusModifiers.WorkInProgress);
			AssertEquals("Should make an issue status query with only WIP statuses", "status+in+(\"Comp1\",\"Comp2\",\"Comp3\",\"WIP1\",\"WIP2\")", issueQuery);
		}

		public void TestIssueStatusQueryAssembler_UnstartedAndWIPIssueStatuses()
		{
			SetDefaultIssueStatusRegistryValues();

			var issueQuery = ManyIssuesJiraQuery.GetIssueStatusesQueryString(IssueStatusModifiers.Unstarted | IssueStatusModifiers.WorkInProgress);
			AssertEquals("Should make an issue status query with only WIP statuses", "status+not+in+(\"Comp1\",\"Comp2\",\"Comp3\")", issueQuery);
		}

		public void TestIssueStatusQueryAssembler_AllIssueStatuses()
		{
			SetDefaultIssueStatusRegistryValues();

			var issueQuery = ManyIssuesJiraQuery.GetIssueStatusesQueryString(IssueStatusModifiers.Completed | IssueStatusModifiers.Unstarted | IssueStatusModifiers.WorkInProgress);
			AssertEquals("Should make an issue status query with only WIP statuses", "", issueQuery);
		}

		static void SetDefaultIssueStatusRegistryValues()
		{
			ProcessManagementRegistry.Instance.CompletedIssueStatusTypesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { "Comp1", "Comp2", "Comp3" });
			ProcessManagementRegistry.Instance.WorkInProgressIssueStatusTypesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { "WIP1", "WIP2" });
		}

		#endregion
	}
}
