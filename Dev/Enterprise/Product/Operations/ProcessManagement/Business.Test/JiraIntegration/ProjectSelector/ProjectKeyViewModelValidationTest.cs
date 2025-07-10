using CargoWise.EntityFramework.Testing;

namespace Enterprise.ProcessManagement.Business.Test
{
	class ProjectKeyViewModelValidationTest : BusinessObjectValidationTestCase
	{
		public void TestKey_ShouldBeMandatory_WhilstSpecificProjectsAreBeingImported()
		{
			var viewModel = ProcessMgmtTestHelper.GetProjectsToImportViewModel(Factory);
			var projectKey = viewModel.SpecificProjectsToImport.AddNew();

			viewModel.ShouldImportAllProjects = true;

			projectKey.Validation.ValidateAll();

			AssertMandatoryValidationError(projectKey.ProjectKeyInfo, isExpectingError: false);

			viewModel.ShouldImportAllProjects = false;
			AssertMandatoryValidationError(projectKey.ProjectKeyInfo, isExpectingError: true);

			projectKey.ProjectKey = "Creed";
			AssertMandatoryValidationError(projectKey.ProjectKeyInfo, isExpectingError: false);
		}
	}
}
