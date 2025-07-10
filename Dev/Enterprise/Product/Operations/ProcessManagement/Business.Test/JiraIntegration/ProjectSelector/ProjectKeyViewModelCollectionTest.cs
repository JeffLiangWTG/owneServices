using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(ProjectKeyViewModelCollection))]
	class ProjectKeyViewModelCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ProjectKeyViewModelCollection>
	{
		protected override ProjectKeyViewModelCollection GetCollectionToTest() => ProcessMgmtTestHelper.GetProjectsToImportViewModel(Factory).SpecificProjectsToImport;

		protected override BusinessObject GetNewElementToAddToTheCollection() => ProcessMgmtTestHelper.GetProjectsToImportViewModel(Factory).SpecificProjectsToImport.AddNew();
	}
}
