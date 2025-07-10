using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Business.Test
{
	class JiraExternalEntityLinkTest : TestCaseWithFactory
	{
		public void TestImportProject_ShouldCreateExternalEntityLink()
		{
			var dummyImporter = new DummyJiraEntityImporter(Factory);
			JiraIntegrationTestHelper.AssertImportWasSuccessful(dummyImporter);

			var factory = new BusinessObjectFactory();
			var importedProject = factory.LoadTop1<Project>(new ZQuery());
			var link = factory.LoadTop1<ExternalEntityLink>(new ZQuery(ExternalEntityLinkSchema.EEL_ParentID, importedProject.PK));

			AssertLink("A link should have been created between the Jira project and the WorkProject. SAD!", link, importedProject.PK, "WKP", "SYS", "10008");
		}

		public void TestImportProject_WhenLinkAlreadyExists_ShouldNotImportAgain()
		{
			var dummyImporter = new DummyJiraEntityImporter(Factory);
			JiraIntegrationTestHelper.AssertImportWasSuccessful(dummyImporter);

			var factory = new BusinessObjectFactory();
			var importedProjects = factory.Load<Project>(new ZQuery());
			var links = factory.Load<ExternalEntityLink>(new ZQuery(ExternalEntityLinkSchema.EEL_ParentTableCode, "WKP"));

			AssertEquals("There should be only one project in the database. SAD!", 1, importedProjects.Length);
			AssertEquals("There should be only one external link for Projects in the database. SAD!", 1, links.Length);

			var project = importedProjects.Single();
			var link = links.Single();

			AssertLink("A link should have been created between the Jira project and the WorkProject. SAD!", link, project.PK, "WKP", "SYS", "10008");

			JiraIntegrationTestHelper.AssertImportWasSuccessful(dummyImporter);

			factory = new BusinessObjectFactory();
			importedProjects = factory.Load<Project>(new ZQuery());
			links = factory.Load<ExternalEntityLink>(new ZQuery(ExternalEntityLinkSchema.EEL_ParentTableCode, "WKP"));

			AssertEquals("The project has already been imported, so it shouldn't have been imported again. SAD!", 1, importedProjects.Length);
			AssertEquals("The project has already been imported, so it shouldn't have been imported again. SAD!", project.PK, importedProjects.Single().PK);

			AssertEquals("The project has already been imported, so a new link shouln't have been created for it. SAD!", 1, links.Length);
			AssertEquals("The project has already been imported, so a new link shouln't have been created for it. SAD!", link.PK, links.Single().PK);
		}

		public void TestImportIssue_ShouldCreateExternalEntityLink()
		{
			var dummyImporter = new DummyJiraEntityImporter(Factory);
			JiraIntegrationTestHelper.AssertImportWasSuccessful(dummyImporter);

			var factory = new BusinessObjectFactory();
			var workItem = factory.LoadTop1<WorkItem>(new ZQuery());
			var link = factory.LoadTop1<ExternalEntityLink>(new ZQuery(ExternalEntityLinkSchema.EEL_ParentID, workItem.PK));

			AssertLink("A link should have been created between the Jira issue and the WorkItem. SAD!", link, workItem.PK, "WKI", "SYS", "10054");
		}

		public void TestImportIssue_WhenLinkAlreadyExists_ShouldNotImportAgain()
		{
			var dummyImporter = new DummyJiraEntityImporter(Factory);
			JiraIntegrationTestHelper.AssertImportWasSuccessful(dummyImporter);

			var factory = new BusinessObjectFactory();
			var importedWorksItem = factory.Load<WorkItem>(new ZQuery());
			var links = factory.Load<ExternalEntityLink>(new ZQuery(ExternalEntityLinkSchema.EEL_ParentTableCode, "WKI"));

			AssertEquals("There should be only one work item in the database. SAD!", 1, importedWorksItem.Length);
			AssertEquals("There should be only one external link for work items in the database. SAD!", 1, links.Length);

			var workItem = importedWorksItem.Single();
			var link = links.Single();

			AssertLink("A link should have been created between the Jira issue and the work item. SAD!", link, workItem.PK, "WKI", "SYS", "10054");

			JiraIntegrationTestHelper.AssertImportWasSuccessful(dummyImporter);

			factory = new BusinessObjectFactory();
			importedWorksItem = factory.Load<WorkItem>(new ZQuery());
			links = factory.Load<ExternalEntityLink>(new ZQuery(ExternalEntityLinkSchema.EEL_ParentTableCode, "WKI"));

			AssertEquals("The work item has already been imported, so it shouldn't have been imported again. SAD!", 1, importedWorksItem.Length);
			AssertEquals("The work item has already been imported, so it shouldn't have been imported again. SAD!", workItem.PK, importedWorksItem.Single().PK);

			AssertEquals("The work item has already been imported, so a new link shouln't have been created for it. SAD!", 1, links.Length);
			AssertEquals("The work item has already been imported, so a new link shouln't have been created for it. SAD!", link.PK, links.Single().PK);
		}

		#region Implementation

		void AssertNoLinksInDatabase()
		{
			AssertContainsExactElementsInAnyOrder("There should be no external entity links in the database. SAD!", System.Array.Empty<string>(), Factory.Load<ExternalEntityLink>(new ZQuery()).Select(x => x.EEL_ExternalCode));
		}

		static void AssertLink(string message, ExternalEntityLink link, ZGuid expectedParentId, string expectedParentTableCode, string expectedSystemCode, string expectedJiraId)
		{
			CombineAssertions(message, () =>
			{
				AssertNotNull(link);
				AssertEquals("ParentId", expectedParentId, link.EEL_ParentID);
				AssertEquals("ParentTableCode", expectedParentTableCode, link.EEL_ParentTableCode);
				AssertEquals("System code", expectedSystemCode, link.EEL_SystemCode);
				AssertEquals("Jira ID", expectedJiraId, link.EEL_ExternalCode);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			JiraIntegrationTestHelper.SetJiraUrlsRegistryItem();
			JiraIntegrationTestHelper.AddDefaultOrgProxyClient();
			AssertNoLinksInDatabase();
		}

		#endregion
	}
}
