using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ProcessManagement.Business.Test
{
	class ExternalEntityLinkHelperTest : TestCaseWithFactory
	{
		public void TestFilterOutPreviouslyLinkedEntities()
		{
			var dummyToConvertToWorkItem = new DummyExternalEntityLinkable("Bardi", "WKI");
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			ExternalEntityLinkHelper.CreateLink(dummyToConvertToWorkItem, workItem, "AAA");

			var dummyToConvertToProject = new DummyExternalEntityLinkable("Jarule", "WKP");
			var project = ProcessMgmtTestHelper.CreateProject(Factory);
			ExternalEntityLinkHelper.CreateLink(dummyToConvertToProject, project, "BB");

			var results = ExternalEntityLinkHelper.FilterOutPreviouslyLinkedEntities(new[] { dummyToConvertToWorkItem, dummyToConvertToProject }, "AAA", Factory);
			AssertContainsExactElementsInAnyOrder("The work item has already been linked for the specified system, so it should be excluded. SAD!", new[] { "Jarule" }, results.Select(x => x.ID));

			results = ExternalEntityLinkHelper.FilterOutPreviouslyLinkedEntities(new[] { dummyToConvertToWorkItem, dummyToConvertToProject }, "BB", Factory);
			AssertContainsExactElementsInAnyOrder("The project has already been linked for the specified system, so it should be excluded. SAD!", new[] { "Bardi" }, results.Select(x => x.ID));
		}

		public void TestCreateLink()
		{
			var dummyToConvertToWorkItem = new DummyExternalEntityLinkable("Bardi", "WKI");
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			var link = ExternalEntityLinkHelper.CreateLink(dummyToConvertToWorkItem, workItem, "AAA");

			AssertEquals(workItem.PK, link.EEL_ParentID);
			AssertEquals("WKI", link.EEL_ParentTableCode);
			AssertEquals("AAA", link.EEL_SystemCode);
			AssertEquals("Bardi", link.EEL_ExternalCode);
			AssertEquals(workItem.Factory, link.Factory);
		}
	}
}
