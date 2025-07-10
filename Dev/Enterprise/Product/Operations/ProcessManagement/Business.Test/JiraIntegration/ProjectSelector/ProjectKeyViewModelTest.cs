using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(ProjectKeyViewModel))]
	class ProjectKeyViewModelTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => ProcessMgmtTestHelper.GetProjectsToImportViewModel(Factory).SpecificProjectsToImport.AddNew();
	}
}
