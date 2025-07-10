using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using NUnit.Framework;
using static Enterprise.ProcessManagement.Business.JiraConstants;
using static Enterprise.ProcessManagement.Business.Test.JiraWebHelperTest;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(ProjectsToImportViewModel))]
	class ProjectsToImportViewModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestShouldImportAllProjects_WhenTrue_ProjectKeyCollectionShouldBeReadOnly()
		{
			var viewModel = ProcessMgmtTestHelper.GetProjectsToImportViewModel(Factory);

			AssertEquals(false, viewModel.SpecificProjectsToImport.ReadOnly);

			viewModel.ShouldImportAllProjects = true;
			AssertEquals(true, viewModel.SpecificProjectsToImport.ReadOnly);

			viewModel.ShouldImportAllProjects = false;
			AssertEquals(false, viewModel.SpecificProjectsToImport.ReadOnly);
		}

		public void TestIssueStatusModifier()
		{
			var viewModel = ProcessMgmtTestHelper.GetProjectsToImportViewModel(Factory);

			viewModel.ShouldImportCompletedIssues = false;
			viewModel.ShouldImportInProgressIssues = false;
			viewModel.ShouldImportUnStartedIssues = false;

			AssertEquals(IssueStatusModifiers.None, viewModel.IssueStatusModifier);

			viewModel.ShouldImportCompletedIssues = true;
			viewModel.ShouldImportInProgressIssues = false;
			viewModel.ShouldImportUnStartedIssues = false;

			AssertEquals(IssueStatusModifiers.Completed, viewModel.IssueStatusModifier);

			viewModel.ShouldImportCompletedIssues = false;
			viewModel.ShouldImportInProgressIssues = false;
			viewModel.ShouldImportUnStartedIssues = true;

			AssertEquals(IssueStatusModifiers.Unstarted, viewModel.IssueStatusModifier);

			viewModel.ShouldImportCompletedIssues = true;
			viewModel.ShouldImportInProgressIssues = false;
			viewModel.ShouldImportUnStartedIssues = true;

			AssertEquals(IssueStatusModifiers.Completed | IssueStatusModifiers.Unstarted, viewModel.IssueStatusModifier);

			viewModel.ShouldImportCompletedIssues = false;
			viewModel.ShouldImportInProgressIssues = true;
			viewModel.ShouldImportUnStartedIssues = false;

			AssertEquals(IssueStatusModifiers.WorkInProgress, viewModel.IssueStatusModifier);

			viewModel.ShouldImportCompletedIssues = true;
			viewModel.ShouldImportInProgressIssues = true;
			viewModel.ShouldImportUnStartedIssues = false;

			AssertEquals(IssueStatusModifiers.Completed | IssueStatusModifiers.WorkInProgress, viewModel.IssueStatusModifier);

			viewModel.ShouldImportCompletedIssues = false;
			viewModel.ShouldImportInProgressIssues = true;
			viewModel.ShouldImportUnStartedIssues = true;

			AssertEquals(IssueStatusModifiers.Unstarted | IssueStatusModifiers.WorkInProgress, viewModel.IssueStatusModifier);

			viewModel.ShouldImportCompletedIssues = true;
			viewModel.ShouldImportInProgressIssues = true;
			viewModel.ShouldImportUnStartedIssues = true;

			AssertEquals(IssueStatusModifiers.Completed | IssueStatusModifiers.Unstarted | IssueStatusModifiers.WorkInProgress, viewModel.IssueStatusModifier);
		}

		public void TestImportReportsCorrectResults_Success()
		{
			var importResult = new JiraResult(JiraResponseStatus.Success, "Boi");
			var expected = new ProjectImportReport(true, ProjectImportReport.Success());
			AssertImportCreatesCorrectReport(importResult, expected);
		}

		public void TestImportReportsCorrectResultsForJiraCloud_Success()
		{
			var importResult = new JiraResult(JiraResponseStatus.Success, "Boi");
			var expected = new ProjectImportReport(true, ProjectImportReport.Success(JiraAPIVersions.JiraRequestAPI_3));
			AssertImportCreatesCorrectReport(importResult, expected);
		}

		public void TestImportReportsCorrectResultsForJiraServer_Success()
		{
			var importResult = new JiraResult(JiraResponseStatus.Success, "Boi");
			var expected = new ProjectImportReport(true, ProjectImportReport.Success(JiraAPIVersions.JiraRequestAPI_2));
			AssertImportCreatesCorrectReport(importResult, expected, JiraAPIVersions.JiraRequestAPI_2);
		}

		public void TestImportReportsCorrectResults_InvalidRequest()
		{
			var importResult = new JiraResult(JiraResponseStatus.InvalidRequest, "Boi");
			var expected = new ProjectImportReport(false, ProjectImportReport.InvalidRequest);
			AssertImportCreatesCorrectReport(importResult, expected);
		}

		public void TestImportReportsCorrectResults_InvalidRequestWithCustomQuery()
		{
			var importResult = new JiraResult(JiraResponseStatus.InvalidRequestWithCustomQuery, "Boi");
			var expected = new ProjectImportReport(false, ProjectImportReport.InvalidRequestWithCustomQuery);
			AssertImportCreatesCorrectReport(importResult, expected);
		}

		public void TestImportReportsCorrectResults_BadCredentials()
		{
			var importResult = new JiraResult(JiraResponseStatus.BadCredentials, "Boi");
			var expected = new ProjectImportReport(false, ProjectImportReport.BadCredentials);
			AssertImportCreatesCorrectReport(importResult, expected);
		}

		public void TestImportReportsCorrectResults_InsufficientPermissions()
		{
			var importResult = new JiraResult(JiraResponseStatus.InsufficientPermissions, "Boi");
			var expected = new ProjectImportReport(false, ProjectImportReport.InsufficientPermissions);
			AssertImportCreatesCorrectReport(importResult, expected);
		}

		public void TestImportReportsCorrectResults_FailedServerCall()
		{
			JiraIntegrationTestHelper.SetJiraUrlsRegistryItem(Tuple.Create("ABC", "https://sampleweb.atlassian.net"));
			var importResult = new JiraResult(JiraResponseStatus.FailedServerCall, "Boi");
			var expected = new ProjectImportReport(false, ProjectImportReport.FailedServerCall(new Uri("https://sampleweb.atlassian.net")));
			AssertImportCreatesCorrectReport(importResult, expected);
		}

		public void TestImportReportCorrectResults_FailedServerCall_ContainsCorrectPath()
		{
			var uri = new Uri("https://sampleweb.atlassian.net/");
			var result = ProjectImportReport.FailedServerCall(uri);
			AssertContains(ProcessManagementRegistry.Instance.JiraSiteUrls.Inner.Location, result);
		}

		public void TestImportReportsCorrectResults_GenericFailure_WithNoGivenResponse()
		{
			var importResult = new JiraResult(JiraResponseStatus.GenericFailure, "");
			var expected = new ProjectImportReport(true, ProjectImportReport.UnspecifiedFailure);
			AssertImportCreatesCorrectReport(importResult, expected);
		}

		public void TestImportReportsCorrectResults_GenericFailure_WithACustoResponse()
		{
			var importResult = new JiraResult(JiraResponseStatus.GenericFailure, "Boi");
			var expected = new ProjectImportReport(true, "Boi");
			AssertImportCreatesCorrectReport(importResult, expected);
		}

		public void TestImportReportsCorrectResults_WhenSomeDataWasSaved()
		{
			var importResult = new JiraResult(JiraResponseStatus.Success, "Boi", wasDataSaved: true);
			var expected = new ProjectImportReport(true, ProjectImportReport.Success());
			AssertImportCreatesCorrectReport(importResult, expected);
		}

		public void TestImportReportsCorrectResults_NoClient()
		{
			var importResult = new JiraResult(JiraResponseStatus.NoProjectClient, "Boi");
			var expected = new ProjectImportReport(true, "Boi");
			AssertImportCreatesCorrectReport(importResult, expected);
		}

		void AssertImportCreatesCorrectReport(JiraResult desiredImportResult, ProjectImportReport expectedReport, string jiraApiVersion = JiraAPIVersions.JiraRequestAPI_3)
		{
			var viewModel = new DummyProjectsToImportViewModel(Factory, jiraApiVersion);
			viewModel.ImporterToImportWith = new DummyJiraEntityImporter(viewModel) { NeverActuallyImport = true, ImportResult = desiredImportResult };

			var dummyProgressForm = new JiraIntegrationTestHelper.DummyProgressTracker();
			var credentials = new JiraCredentials("WhoAmI?", "NoneOfYourBusiness!");
			viewModel.SetCredentials_ForTest(credentials);
			var actualResult = viewModel.ImportJiraContentAndReportResult(dummyProgressForm);

			AssertEquals("We should have a correct message for our import since that's the result we received", expectedReport, actualResult);
		}

		public void TestJiraUrls_ShouldUseListFromRegistryItem()
		{
			JiraIntegrationTestHelper.SetJiraUrlsRegistryItem(Tuple.Create("ABC", "https://first.com"), Tuple.Create("123", "https://second.com"));
			var viewModel = new ProjectsToImportViewModel(Factory);

			AssertEquals(@"ABC - https://first.com
123 - https://second.com", viewModel.JiraUrls.ElementsAsString);
		}

		public void TestJiraSystemCode_WhenOnlyOneCodeDefinedInRegistry_ShouldBeSelectedByDefault()
		{
			JiraIntegrationTestHelper.SetJiraUrlsRegistryItem(Tuple.Create("ABC", "https://first.com"), Tuple.Create("123", "https://second.com"));
			var viewModel = new ProjectsToImportViewModel(Factory);

			AssertEquals("Multiple codes are defined in the registry, so let the user decide. SAD!", ZString.Empty, viewModel.JiraSystemCode);

			JiraIntegrationTestHelper.SetJiraUrlsRegistryItem(Tuple.Create("ABC", "https://first.com"));
			viewModel = new ProjectsToImportViewModel(Factory);

			AssertEquals("Only one code is defined in the registry, so it should be automatically selected as a convenience to the user. SAD!", "ABC", viewModel.JiraSystemCode);
		}

		public void TestSelectedUri()
		{
			JiraIntegrationTestHelper.SetJiraUrlsRegistryItem(Tuple.Create("ABC", "https://first.com"), Tuple.Create("123", "https://second.com"));
			var viewModel = new ProjectsToImportViewModel(Factory);
			AssertNull(viewModel.SelectedUri);

			viewModel.JiraSystemCode = "ABC";
			AssertEquals("https://first.com/", viewModel.SelectedUri.ToString());

			viewModel.JiraSystemCode = "123";
			AssertEquals("https://second.com/", viewModel.SelectedUri.ToString());
		}

		public void TestJiraApiVersion()
		{
			JiraIntegrationTestHelper.SetJiraUrlsRegistryItem(Tuple.Create("TST", "https://google.com/"), Tuple.Create("CLD", JiraIntegrationTestHelper.DummyJiraLinks.DummyCloudBaseUri), Tuple.Create("SRV", JiraIntegrationTestHelper.DummyJiraLinks.DummyServerBaseUri));
			var viewModel = new ProjectsToImportViewModel(Factory);
			AssertNullOrEmpty(viewModel.JiraApiVersion);

			viewModel.JiraSystemCode = "CLD";
			var dummyJiraWebHelper = new DummyJiraWebHelperToTestRequestJiraInstanceServerInfo();
			var checkJiraWebVersion = viewModel.CheckJiraApiVersion(dummyJiraWebHelper);

			AssertEquals(true, checkJiraWebVersion);
			AssertEquals(viewModel.JiraApiVersion, JiraAPIVersions.JiraRequestAPI_3);

			viewModel.JiraSystemCode = "SRV";
			checkJiraWebVersion = viewModel.CheckJiraApiVersion(dummyJiraWebHelper);

			AssertEquals(true, checkJiraWebVersion);
			AssertEquals(viewModel.JiraApiVersion, JiraAPIVersions.JiraRequestAPI_2);

			viewModel.JiraSystemCode = "TST";
			checkJiraWebVersion = viewModel.CheckJiraApiVersion(dummyJiraWebHelper);

			AssertEquals(false, checkJiraWebVersion);
			AssertNullOrEmpty(viewModel.JiraApiVersion);
		}

		public void TestJiraApiVersionReportsCorrectResults_FailedServerCall()
		{
			JiraIntegrationTestHelper.SetJiraUrlsRegistryItem(Tuple.Create("ABC", "https://error1.com"));

			var mock = new Mock<DummyProjectsToImportViewModel>(Factory, JiraAPIVersions.JiraRequestAPI_3);
			mock.CallBase = true;
			var viewModel = mock.Object;
			var dummyProgressForm = new JiraIntegrationTestHelper.DummyProgressTracker();

			mock.Setup(m => m.CheckJiraApiVersion(It.IsAny<JiraWebHelper>()))
				.Throws(new Exception("On no Something bad happened!"));

			var actualResult = viewModel.ImportJiraContentAndReportResult(dummyProgressForm);

			AssertEquals(false, actualResult.ShouldCloseImportForm);
			AssertEquals(actualResult.ProjectReportMessage, "On no Something bad happened!");

			mock.Setup(m => m.CheckJiraApiVersion(It.IsAny<JiraWebHelper>()))
				.Throws(new AggregateException("I aggregate errors!", new AggregateException("OH no!", new Exception("Something bad happened!"))));

			actualResult = viewModel.ImportJiraContentAndReportResult(dummyProgressForm);

			AssertEquals(false, actualResult.ShouldCloseImportForm);
			AssertEquals(actualResult.ProjectReportMessage, "OH no!" + System.Environment.NewLine + "Something bad happened!");
		}
	}
}
