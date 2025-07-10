using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ProcessManagement.Business.Test
{
	class ProjectsToImportViewModelValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCredentials_ShouldBeMandatory()
		{
			var viewModel = ProcessMgmtTestHelper.GetProjectsToImportViewModel(Factory);

			viewModel.Validation.ValidateAll();

			AssertMandatoryValidationError(viewModel.JiraUserNameInfo, isExpectingError: true);
			AssertMandatoryValidationError(viewModel.JiraAuthTokenInfo, isExpectingError: true);

			viewModel.JiraUserName = "Homer.Simpson";
			viewModel.JiraAuthToken = "FlancrestEnterprises";

			AssertMandatoryValidationError(viewModel.JiraUserNameInfo, isExpectingError: false);
			AssertMandatoryValidationError(viewModel.JiraAuthTokenInfo, isExpectingError: false);
		}

		public void TestJiraSystemCode_ShouldBeMandatory()
		{
			JiraIntegrationTestHelper.SetJiraUrlsRegistryItem(Tuple.Create("ABC", "https://first.com"), Tuple.Create("123", "https://second.com"));

			var viewModel = ProcessMgmtTestHelper.GetProjectsToImportViewModel(Factory);
			viewModel.Validation.ValidateAll();

			AssertHasError(viewModel.JiraSystemCodeInfo, "Please enter a Jira URL.");

			viewModel.JiraSystemCode = "ABC";
			AssertNoErrors(viewModel.JiraSystemCodeInfo);
		}

		public void TestJiraSystemCode_ShouldUseListValidation()
		{
			JiraIntegrationTestHelper.SetJiraUrlsRegistryItem(Tuple.Create("ABC", "https://first.com"));
			var viewModel = ProcessMgmtTestHelper.GetProjectsToImportViewModel(Factory);
			viewModel.JiraSystemCode = "XYZ";
			AssertHasError(viewModel.JiraSystemCodeInfo, "Enter a valid Jira URL.");

			viewModel.JiraSystemCode = "ABC";
			AssertNoErrors(viewModel.JiraSystemCodeInfo);
		}
	}
}
