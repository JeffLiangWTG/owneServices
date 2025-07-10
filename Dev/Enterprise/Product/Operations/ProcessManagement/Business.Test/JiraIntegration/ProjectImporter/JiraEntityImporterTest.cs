using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	public class JiraEntityImporterTest : TestCaseWithFactory
	{
		#region Basic Import

		[TestDate(2019, 1, 1)]
		public void TestJiraImport_BasicImport()
		{
			var dummyImporter = new DummyJiraEntityImporter(Factory);
			JiraIntegrationTestHelper.AssertImportWasSuccessful(dummyImporter);

			var factorio = new BusinessObjectFactory();
			var importedProject = factorio.LoadTop1<Project>(new ZQuery()); // only one project should exist in this test run, so getting one project is ok
			var importedIssue = factorio.LoadTop1<WorkItem>(new ZQuery()); // only one WI should exist in this test run, so getting one WI is ok

			CombineAssertions("We've made a Project with correctly transferred values", () =>
			{
				AssertEquals("Summary", "BP - BusinessProject", importedProject.WKP_Summary);
				AssertEquals("Details", "Bibbity Bobb", importedProject.WKP_Details.ToUTF8());
				AssertEquals("Status", ProcessTaskStatusCodeList.Codes.Working, importedProject.WKP_Status);
				AssertCollectionContains("RelatedItems", importedIssue, importedProject.RelatedItems);
				AssertEquals("Project Manager should not exist", "", importedProject.WKP_GS_NKProjectManager);
			});

			CombineAssertions("We've made a Work Item with correctly transferred values", () =>
			{
				AssertEquals("Summary", "AVINA-4: Buge", importedIssue.WKI_Summary);
				JiraIntegrationTestHelper.AssertDescriptionEDocAndHyperlinkAreCorrect(Factory, importedIssue, "AVINA-4", "<p>Description field but I changed it</p>");
				AssertCollectionContains("RelatedItems", importedProject, importedIssue.RelatedItems);
			});
		}

		[TestDate(2019, 1, 1)]
		public void TestJiraImport_WithExistingStaff_ShouldAttachAsProjectManager()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "boberly.lastingtonnamersen@sampleweb.com";
			Factory.Save();

			var dummyImporter = new DummyJiraEntityImporter(Factory);
			JiraIntegrationTestHelper.AssertImportWasSuccessful(dummyImporter);

			var factorio = new BusinessObjectFactory();
			var importedProject = factorio.LoadTop1<Project>(new ZQuery()); // only one project should exist in this test run, so getting one project is ok
			var importedIssue = factorio.LoadTop1<WorkItem>(new ZQuery()); // only one WI should exist in this test run, so getting one WI is ok

			CombineAssertions("We've made a Project with correctly transferred values", () =>
			{
				AssertEquals("Summary", "BP - BusinessProject", importedProject.WKP_Summary);
				AssertEquals("Details", "Bibbity Bobb", importedProject.WKP_Details.ToUTF8());
				AssertEquals("Status", ProcessTaskStatusCodeList.Codes.Working, importedProject.WKP_Status);
				AssertCollectionContains("RelatedItems", importedIssue, importedProject.RelatedItems);
				AssertEquals("Project Manager", staff.GS_Code, importedProject.WKP_GS_NKProjectManager);
			});

			CombineAssertions("We've made a Work Item with correctly transferred values", () =>
			{
				AssertEquals("Summary", "AVINA-4: Buge", importedIssue.WKI_Summary);
				JiraIntegrationTestHelper.AssertDescriptionEDocAndHyperlinkAreCorrect(Factory, importedIssue, "AVINA-4", "<p>Description field but I changed it</p>");
				AssertCollectionContains("RelatedItems", importedProject, importedIssue.RelatedItems);
			});
		}

		public void TestJiraImport_WithIssueTypesSet_ShouldSetWorkItemTypes_FieldOrderDoesNotMatter()
		{
			var map = new IssueTypeMap();

			map.JiraClassificationMap.Add(new IssueTypeMapItem
			{
				JiraEntityName = "Bug",
				SelectionCriterionFieldName = WorkItemSchema.Constants.WKI_ActivitySubtype,
				SelectionCriterionFieldValue = "FIX",
			});

			map.JiraClassificationMap.Add(new IssueTypeMapItem
			{
				JiraEntityName = "Bug",
				SelectionCriterionFieldName = WorkItemSchema.Constants.WKI_ActivityType,
				SelectionCriterionFieldValue = "POX",
			});

			map.JiraClassificationMap.Add(new IssueTypeMapItem
			{
				JiraEntityName = "Hugg",
				SelectionCriterionFieldName = WorkItemSchema.Constants.WKI_Priority,
				SelectionCriterionFieldValue = "BOX",
			});

			ProcessManagementRegistry.Instance.JiraIssueTypesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, map);

			var dummyImporter = new DummyJiraEntityImporter(Factory) { ManyIssuesJSONString = JiraIntegrationTestHelper.JSONForTwoIssues };
			JiraIntegrationTestHelper.AssertImportWasSuccessful(dummyImporter);

			var factorio = new BusinessObjectFactory();
			var importedProject = factorio.LoadTop1<Project>(new ZQuery()); // only one project should exist in this test run, so getting one project is ok
			var importedIssues = factorio.Load<WorkItem>(new ZQuery());
			var importedBug = importedIssues.Where(issue => issue.WKI_Summary.Equals("AVINA-4: Buge")).First();
			var importedHug = importedIssues.Where(issue => issue.WKI_Summary.Equals("AVINA-3: Make Helicarrier")).First();

			AssertNotNull("We should have two distinct issues imported", importedBug);
			AssertNotNull("We should have two distinct issues imported", importedHug);

			CombineAssertions("We've made a Project with correctly transferred values", () =>
			{
				var summary = "BP - BusinessProject";
				AssertEquals("Summary", summary, importedProject.WKP_Summary);
				AssertEquals("Details", "Bibbity Bobb", importedProject.WKP_Details.ToUTF8());
				AssertEquals("Status", ProcessTaskStatusCodeList.Codes.Working, importedProject.WKP_Status);
				AssertCollectionContains("RelatedItems", importedBug, importedProject.RelatedItems);
				AssertCollectionContains("RelatedItems", importedHug, importedProject.RelatedItems);
			});

			CombineAssertions("We've made a Work Item with correctly transferred values", () =>
			{
				AssertEquals("Summary", "AVINA-4: Buge", importedBug.WKI_Summary);
				AssertEquals("Custom Issue Property should be as set in the registry despite it being blanked sometimes", "FIX", importedBug.WKI_ActivitySubtype);
				AssertEquals("Custom Issue Property should be as the other value in the registry", "POX", importedBug.WKI_ActivityType);
				AssertEquals("Custom Issue Property NOT should be set, as in the registry", "", importedBug.WKI_Priority);
			});

			CombineAssertions("We've made a Work Item with correctly transferred values", () =>
			{
				AssertEquals("Summary", "AVINA-3: Make Helicarrier", importedHug.WKI_Summary);
				AssertEquals("Custom Issue Property NOT should be as set in the registry", "", importedHug.WKI_ActivitySubtype);
				AssertEquals("Custom Issue Property NOT should be as the other value in the registry", "", importedHug.WKI_ActivityType);
				AssertEquals("Custom Issue Property should be set as in the registry", "BOX", importedHug.WKI_Priority);
			});
		}

		public void TestJiraImport_WithJiraCustomField_ShouldSetWorkArea()
		{
			var map = new IssueTypeMap();

			map.JiraClassificationMap.Add(new IssueTypeMapItem
			{
				JiraEntityName = "Bug",
				SelectionCriterionFieldName = WorkItemSchema.Constants.WKI_ActivitySubtype,
				SelectionCriterionFieldValue = "FIX",
			});

			map.JiraClassificationMap.Add(new IssueTypeMapItem
			{
				JiraEntityName = "Bug",
				SelectionCriterionFieldName = WorkItemSchema.Constants.WKI_ActivityType,
				SelectionCriterionFieldValue = "POX",
			});

			map.JiraClassificationMap.Add(new IssueTypeMapItem
			{
				JiraEntityName = "Hugg",
				SelectionCriterionFieldName = WorkItemSchema.Constants.WKI_Priority,
				SelectionCriterionFieldValue = "BOX",
			});

			var customFieldMap = new JiraCustomFieldMap();
			customFieldMap.JiraClassificationMap.Add(new JiraCustomFieldMapItem()
			{
				JiraEntityName = "EDI & Services",
				CustomFieldId = "10907",
				SelectionCriterionFieldName = "WKI_WorkItemArea",
				SelectionCriterionFieldValue = "EDS",
			});

			ProcessManagementRegistry.Instance.JiraIssueTypesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, map);
			ProcessManagementRegistry.Instance.JiraCustomFieldsMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customFieldMap);

			var dummyImporter = new DummyJiraEntityImporter(Factory) { ManyIssuesJSONString = JiraIntegrationTestHelper.JSONFourForIssues };
			JiraIntegrationTestHelper.AssertImportWasSuccessful(dummyImporter);

			var factorio = new BusinessObjectFactory();
			var importedProject = factorio.LoadTop1<Project>(new ZQuery()); // only one project should exist in this test run, so getting one project is ok
			var importedIssues = factorio.Load<WorkItem>(new ZQuery());
			var importedBug = importedIssues.Where(issue => issue.WKI_Summary.Equals("AVINA-4: Buge")).First();

			AssertEquals("Wrong area", "EDS", importedBug.WKI_WorkItemArea);
		}

		public void TestJiraImport_WithJiraCustomField_DuplicateConfigIssueTypeShouldBeTaken()
		{
			var map = new IssueTypeMap();

			map.JiraClassificationMap.Add(new IssueTypeMapItem
			{
				JiraEntityName = "Bug",
				SelectionCriterionFieldName = WorkItemSchema.Constants.WKI_ActivitySubtype,
				SelectionCriterionFieldValue = "FIX",
			});

			map.JiraClassificationMap.Add(new IssueTypeMapItem
			{
				JiraEntityName = "Bug",
				SelectionCriterionFieldName = WorkItemSchema.Constants.WKI_ActivityType,
				SelectionCriterionFieldValue = "POX",
			});

			map.JiraClassificationMap.Add(new IssueTypeMapItem
			{
				JiraEntityName = "Hugg",
				SelectionCriterionFieldName = WorkItemSchema.Constants.WKI_Priority,
				SelectionCriterionFieldValue = "BOX",
			});

			map.JiraClassificationMap.Add(new IssueTypeMapItem
			{
				JiraEntityName = "Bug",
				SelectionCriterionFieldName = WorkItemSchema.Constants.WKI_WorkItemArea,
				SelectionCriterionFieldValue = "Bug",
			});

			var customFieldMap = new JiraCustomFieldMap();
			customFieldMap.JiraClassificationMap.Add(new JiraCustomFieldMapItem()
			{
				JiraEntityName = "Bug",
				CustomFieldId = "10907",
				SelectionCriterionFieldName = "WKI_WorkItemArea",
				SelectionCriterionFieldValue = "EDS",
			});

			ProcessManagementRegistry.Instance.JiraIssueTypesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, map);
			ProcessManagementRegistry.Instance.JiraCustomFieldsMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customFieldMap);

			var dummyImporter = new DummyJiraEntityImporter(Factory) { ManyIssuesJSONString = JiraIntegrationTestHelper.JSONFourForIssues };
			JiraIntegrationTestHelper.AssertImportWasSuccessful(dummyImporter);

			var factorio = new BusinessObjectFactory();
			var importedProject = factorio.LoadTop1<Project>(new ZQuery()); // only one project should exist in this test run, so getting one project is ok
			var importedIssues = factorio.Load<WorkItem>(new ZQuery());
			var importedBug = importedIssues.Where(issue => issue.WKI_Summary.Equals("AVINA-4: Buge")).First();

			AssertEquals("Wrong area - issue type should override custom field if same config exists", "Bug", importedBug.WKI_WorkItemArea);
		}

		public void TestImportManyIssues_ShouldCreateIssuesInBatches()
		{
			const int numberOfIssues = 201;
			const int batchSize = 50;

			ProcessManagementRegistry.Instance.JiraIssueImportBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, batchSize);
			var allIssuesJson = JiraIntegrationTestHelper.GetJsonWithSpecifiedNumberOfIssues(numberOfIssues, false);
			var dummyImporter = new DummyJiraEntityImporter(Factory) { ManyIssuesJSONString = allIssuesJson };
			var credentials = new JiraCredentials("Small", "NonSmall");

			AssertEquals(0, Factory.GetDatabaseCount(typeof(WorkItem)));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(Project)));

			dummyImporter.Import(credentials);

			AssertEquals(numberOfIssues, Factory.GetDatabaseCount(typeof(WorkItem)));
			AssertEquals(1, Factory.GetDatabaseCount(typeof(Project)));
			AssertEquals("Should save once for the project, and twice per batch of work items (one for the work items and one for the eConversation).", 11, dummyImporter.NumberOfSaves);
		}

		public void TestImportManyIssues_WhenNumberOfIssuesGreaterThanJiraMaxItemLimit_ShouldDownloadAllIssuesInBatches_LastBatchSizeMatchesMaxBatchSize()
		{
			var importer = new DummyJiraEntityImporterWithMultipleIssueBatchJsonResultsFromJira(Factory);
			importer.OverrideMaxJiraDownloadBatchSize(1);

			importer.AddJsonToReturnForBatchOfIssueRequests(JiraIntegrationTestHelper.JSONForOneIssue);         // number of issues (1) matches batch size (1), so another download will ocurr
			importer.AddJsonToReturnForBatchOfIssueRequests(JiraIntegrationTestHelper.JSONForOneIssue_LongKey); // number of issues (1) matches batch size (1), so another download will ocurr
			importer.AddJsonToReturnForBatchOfIssueRequests(JiraIntegrationTestHelper.JSONForNothing);          // number of issues (0) is less than batch size (1), so loop will exit

			JiraIntegrationTestHelper.AssertImportWasSuccessful(importer);

			var workItems = Factory.Load<WorkItem>(new ZQuery());
			AssertContainsExactElementsInAnyOrder("All batches of issues should be returned. SAD!", new[] { "AVINA-4: Buge", "AVINA-4: VeryLong-VeryVery-Long-SuperDuperLong-WhyIsThisSoLong?-ThisCanNeverHapp" }, workItems.Select(x => x.WKI_Summary));
			AssertEquals("Downloading issues in batches should not require extra Factory.Save()s. SAD!", 2, importer.NumberOfSaves);
			AssertEquals("There should be a Jira web service call for each batch of issues, plus an extra one that doesn't return anything. SAD!", 5, importer.NumberOfWebServiceRequestsExecutedOrSimulated);
		}

		public void TestImportManyIssues_WhenNumberOfIssuesGreaterThanJiraMaxItemLimit_ShouldDownloadAllIssuesInBatches_LastBatchSizeLessThanMaxBatchSize()
		{
			var importer = new DummyJiraEntityImporterWithMultipleIssueBatchJsonResultsFromJira(Factory);
			importer.OverrideMaxJiraDownloadBatchSize(2);

			importer.AddJsonToReturnForBatchOfIssueRequests(JiraIntegrationTestHelper.JSONForTwoIssues);        // number of issues (2) matches batch size (2), so another download will ocurr
			importer.AddJsonToReturnForBatchOfIssueRequests(JiraIntegrationTestHelper.JSONForOneIssue_LongKey); // number of issues (1) is less than batch size (2), so loop will exit
			importer.AddJsonToReturnForBatchOfIssueRequests(JiraIntegrationTestHelper.JSONForNothing);          // this one shouldn't even get called because the loop will stop after the batch with only 1.

			JiraIntegrationTestHelper.AssertImportWasSuccessful(importer);

			var workItems = Factory.Load<WorkItem>(new ZQuery());
			AssertContainsExactElementsInAnyOrder("All batches of issues should be returned. SAD!", new[] { "AVINA-3: Make Helicarrier", "AVINA-4: Buge", "AVINA-4: VeryLong-VeryVery-Long-SuperDuperLong-WhyIsThisSoLong?-ThisCanNeverHapp" }, workItems.Select(x => x.WKI_Summary));
			AssertEquals("Downloading issues in batches should not require extra Factory.Save()s. SAD!", 2, importer.NumberOfSaves);
			AssertEquals("There should be a Jira web service call for each batch of issues, plus an extra one that doesn't return anything. SAD!", 4, importer.NumberOfWebServiceRequestsExecutedOrSimulated);
		}

		public void TestJiraImport_WithProjectCategoriesSet_ShouldSetProjectTypes()
		{
			var map = new ProjectCategoryMap();

			map.JiraClassificationMap.Add(new ProjectCategoryMapItem
			{
				JiraEntityName = "Prooject",
				SelectionCriterionFieldName = WorkProjectSchema.Constants.WKP_Type,
				SelectionCriterionFieldValue = "FOX",
			});

			map.JiraClassificationMap.Add(new ProjectCategoryMapItem
			{
				JiraEntityName = "Prooject",
				SelectionCriterionFieldName = WorkProjectSchema.Constants.WKP_SubType,
				SelectionCriterionFieldValue = "POX",
			});

			map.JiraClassificationMap.Add(new ProjectCategoryMapItem
			{
				JiraEntityName = "Booject",
				SelectionCriterionFieldName = WorkProjectSchema.Constants.WKP_Priority,
				SelectionCriterionFieldValue = "BOX",
			});

			ProcessManagementRegistry.Instance.JiraProjectCategoriesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, map);

			var dummyImporter = new DummyJiraEntityImporter(Factory);
			JiraIntegrationTestHelper.AssertImportWasSuccessful(dummyImporter);

			var factorio = new BusinessObjectFactory();
			var importedProject = factorio.LoadTop1<Project>(new ZQuery()); // only one project should exist in this test run, so getting one project is ok
			var importedIssue = factorio.LoadTop1<WorkItem>(new ZQuery()); // only one WI should exist in this test run, so getting one WI is ok

			CombineAssertions("We've made a Project with correctly transferred values", () =>
			{
				var summary = "BP - BusinessProject";
				AssertEquals("Summary", summary, importedProject.WKP_Summary);
				AssertEquals("Details", "Bibbity Bobb", importedProject.WKP_Details.ToUTF8());
				AssertEquals("Project Category first type", "FOX", importedProject.WKP_Type);
				AssertEquals("Project Category another type", "POX", importedProject.WKP_SubType);
				AssertEquals("Project Category no non-matching types", "", importedProject.WKP_Priority);
			});
		}

		#endregion

		#region Project Client

		public void TestJiraImport_Client_ShouldSetCurrentCompanyOrgProxyMainAddress()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "boberly.lastingtonnamersen@sampleweb.com";

			Factory.Save();

			var dummyImporter = new DummyJiraEntityImporter(Factory);
			JiraIntegrationTestHelper.AssertImportWasSuccessful(dummyImporter);

			var factory = new BusinessObjectFactory();
			var importedProject = factory.LoadTop1<Project>(new ZQuery());
			var clientAddress = importedProject.ClientAddress;

			var orgProxyAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress;
			AssertEquals("The main address of the current company's org proxy should be used as the Client. SAD!", orgProxyAddress.Address1, clientAddress.Address1);
		}

		public void TestJiraImport_Client_ProjectManagerMatchingContact()
		{
			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
			var otherContact = orgProxy.Contacts.AddNew();
			otherContact.OC_ContactName = "Mrs Sullivan";
			otherContact.OC_Email = "somethingelse@sampleweb.com";

			orgProxy.Factory.Save();

			var staff = ProcessMgmtTestHelper.CreateStaff(Factory, "boberly.lastingtonnamersen@sampleweb.com");
			var otherStaff = ProcessMgmtTestHelper.CreateStaff(Factory, "somethingelse@sampleweb.com");

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var dummyImporter = new DummyJiraEntityImporter(Factory);
				JiraIntegrationTestHelper.AssertImportWasSuccessful(dummyImporter);

				var factory = new BusinessObjectFactory();
				var importedProject = factory.LoadTop1<Project>(new ZQuery());
				var clientContact = importedProject.Contact;

				AssertEquals("The contact with an email address matching the project manager should be selected instead of the current user's matching contact, when possible. SAD!", "Jan Michael Vincent", clientContact.OC_ContactName);
			}
		}

		public void TestJiraImport_Client_CurrentUserMatchingContact()
		{
			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
			var contact = orgProxy.Contacts.Cast<OrgContact>().Single();
			contact.OC_Email = "somethingelse@sampleweb.com";

			var otherContact = orgProxy.Contacts.AddNew();
			otherContact.OC_ContactName = "Mrs Sullivan";
			otherContact.OC_Email = "boberly.lastingtonnamersen@sampleweb.com";

			orgProxy.Factory.Save();

			var staff = ProcessMgmtTestHelper.CreateStaff(Factory, "boberly.lastingtonnamersen@sampleweb.com");
			var otherStaff = ProcessMgmtTestHelper.CreateStaff(Factory, "somethingelse@sampleweb.com");

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var dummyImporter = new DummyJiraEntityImporter(Factory);
				JiraIntegrationTestHelper.AssertImportWasSuccessful(dummyImporter);

				var factory = new BusinessObjectFactory();
				var importedProject = factory.LoadTop1<Project>(new ZQuery());
				var clientContact = importedProject.Contact;

				AssertEquals("There wasn't a match on project manager, so the current user's match should have been chosen. SAD!", "Mrs Sullivan", clientContact.OC_ContactName);
			}
		}

		public void TestJiraImport_Client_NoMatchingContact_ShouldSelectFirstContactAlphabetically()
		{
			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
			var contact = orgProxy.Contacts.Cast<OrgContact>().Single();
			contact.OC_Email = "notintheimport@sampleweb.com";

			var otherContact = orgProxy.Contacts.AddNew();
			otherContact.OC_ContactName = "Mrs Sullivan";
			otherContact.OC_Email = "somethingelse@sampleweb.com";

			orgProxy.Factory.Save();

			var staff = ProcessMgmtTestHelper.CreateStaff(Factory, "theresnotimeforthis@sampleweb.com");
			var otherStaff = ProcessMgmtTestHelper.CreateStaff(Factory, "theresnowayofknowing@sampleweb.com");

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var dummyImporter = new DummyJiraEntityImporter(Factory);
				JiraIntegrationTestHelper.AssertImportWasSuccessful(dummyImporter);

				var factory = new BusinessObjectFactory();
				var importedProject = factory.LoadTop1<Project>(new ZQuery());
				var clientContact = importedProject.Contact;

				AssertEquals("Nothing could be matched at all, so the first contact alphabetically should have been chosen. SAD!", "Jan Michael Vincent", clientContact.OC_ContactName);
			}
		}

		public void TestJiraImport_Client_NoMatchingContact_WithNoContactsAtAll_ShouldReturnFailedResult()
		{
			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
			orgProxy.Contacts.RemoveAndDeleteAll();
			orgProxy.Factory.Save();

			var staff = ProcessMgmtTestHelper.CreateStaff(Factory, "theresnotimeforthis@sampleweb.com");
			var otherStaff = ProcessMgmtTestHelper.CreateStaff(Factory, "theresnowayofknowing@sampleweb.com");

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var dummyImporter = new DummyJiraEntityImporter(Factory);
				var result = dummyImporter.Import(new JiraCredentials("small", "nonsmall"));

				AssertEquals(JiraResponseStatus.NoProjectClient, result.Status);
				AssertEquals("Could not find a contact for the Project Client because the Organization Proxy [EDI CUSTOMS BROKERS (EDICUS)] for the current company does not have any contacts. Please add a contact for the Organization and then try the import again.", result.Response);
				AssertEquals(false, result.WasDataSaved);
			}
		}

		#endregion

		#region Comments -> EConversation

		public void TestImportComments()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "janmichaelvincent@xyz.com";
			staff1.GS_FullName = "Jan Michael Vincent";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "mrssullivan@xyz.com";
			staff2.GS_FullName = "Mrs. Sullivan";

			Factory.Save();

			var dummyImporter = new DummyJiraEntityImporter(Factory) { ManyIssuesJSONString = JiraIntegrationTestHelper.JSONForIssueWithComments };
			JiraIntegrationTestHelper.AssertImportWasSuccessful(dummyImporter);

			var factory = new BusinessObjectFactory();
			var workItem = factory.LoadTop1<WorkItem>(new ZQuery());

			AssertContainsExactElementsInAnyOrder("There should be one real participant and one unknown one. SAD!", new[] { "Jan Michael Vincent", "Mrs. Sullivan" }, workItem.Conversation.Participants.Select(x => x.Parent.Name));

			var messageSummaries = new List<string>();

			foreach (var message in workItem.Conversation.Messages.Where(x => !x.Body.Contains("has been added to the conversation")))
			{
				var participant = workItem.Conversation.Participants.Single(x => x.PK == message.JCM_JCP_Participant);
				messageSummaries.Add($"{participant.Parent.Name} said '{message.Body}' on {message.JCM_PostedTimeUtc.ToBestReadableDateTimeString()}.");
			}

			AssertContainsExactElementsInAnyOrder("The correct messages with correct authors and UTC dates should have been saved. SAD!", new[] { "Jan Michael Vincent said 'another thingie' on 04 Feb 2019 00:46.", "Mrs. Sullivan said 'yet another thingie' on 04 Feb 2019 00:53." }, messageSummaries);
		}

		public void TestImportComments_WhenMatchingParticipantNotFound_ShouldUseUnknownGlbStaffAccount()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "janmichaelvincent@xyz.com";
			staff1.GS_FullName = "Jan Michael Vincent";

			Factory.Save();

			var dummyImporter = new DummyJiraEntityImporter(Factory) { ManyIssuesJSONString = JiraIntegrationTestHelper.JSONForIssueWithComments };
			JiraIntegrationTestHelper.AssertImportWasSuccessful(dummyImporter);

			var factory = new BusinessObjectFactory();
			var workItem = factory.LoadTop1<WorkItem>(new ZQuery());

			AssertContainsExactElementsInAnyOrder("There should be one real participant and one unknown one. SAD!", new[] { "Jan Michael Vincent", "Unknown User" }, workItem.Conversation.Participants.Select(x => x.Parent.Name));

			var messageSummaries = new List<string>();

			foreach (var message in workItem.Conversation.Messages.Where(x => !x.Body.Contains("has been added to the conversation")))
			{
				var participant = workItem.Conversation.Participants.Single(x => x.PK == message.JCM_JCP_Participant);
				messageSummaries.Add($"{participant.Parent.Name} said '{message.Body}' on {message.JCM_PostedTimeUtc.ToBestReadableDateTimeString()}.");
			}

			AssertContainsExactElementsInAnyOrder("The 'Unknown User' participant should have been used when the email coulnd't be matched. SAD!", new[] { "Jan Michael Vincent said 'another thingie' on 04 Feb 2019 00:46.", "Unknown User said 'yet another thingie' on 04 Feb 2019 00:53." }, messageSummaries);
		}

		public void TestImportComments_ShouldNotSendEmails()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "janmichaelvincent@xyz.com";
			staff1.GS_FullName = "Jan Michael Vincent";

			Factory.Save();

			var dummyImporter = new DummyJiraEntityImporter(Factory) { ManyIssuesJSONString = JiraIntegrationTestHelper.JSONForIssueWithComments };
			JiraIntegrationTestHelper.AssertImportWasSuccessful(dummyImporter);
			AssertContainsExactElementsInAnyOrder("Importing comments should not send eConversation emails. SAD!", Array.Empty<string>(), Env.AllEmailsCreated.Select(x => x.Body));
		}

		public void TestJiraImport_WithoutComments_ShouldNotCreateJobConversation()
		{
			var dummyImporter = new DummyJiraEntityImporter(Factory);
			JiraIntegrationTestHelper.AssertImportWasSuccessful(dummyImporter);

			var factory = new BusinessObjectFactory();
			var workItem = factory.LoadTop1<WorkItem>(new ZQuery());

			var conversation = factory.LoadTop1<JobConversation>(new ZQuery(JobConversationSchema.JCC_ParentID, workItem.PK));
			AssertNull("No comments were imported, so a JobConversation row should not have been created. SAD!", conversation);
		}

		public void TestJiraImport_WithoutComments_ShouldNotCauseExtraFactorySave()
		{
			var dummyImporter = new DummyJiraEntityImporter(Factory);
			JiraIntegrationTestHelper.AssertImportWasSuccessful(dummyImporter);

			AssertEquals("Since there were no comments to import, it should save once for the project, and once for the batch of work items only. SAD!", 2, dummyImporter.NumberOfSaves);
		}

		#endregion

		#region Issue Attachments

		[TestDate(2019, 1, 1)]
		public void TestImportIssues_WithAttachments_WhenTickedOnProjectSelector_ShouldGetContentAndMetadataFromService()
		{
			ImportIssuesAndAssertAttachmentsCreatedAsEDocs(shouldImportIssueAttachments: true);
		}

		[TestDate(2019, 1, 1)]
		public void TestImportIssues_WithAttachments_WhenUnTickedOnProjectSelector_ShouldNotGetAnythingFromService()
		{
			ImportIssuesAndAssertAttachmentsCreatedAsEDocs(shouldImportIssueAttachments: false);
		}

		void ImportIssuesAndAssertAttachmentsCreatedAsEDocs(bool shouldImportIssueAttachments, JiraIntegrationTestHelper.DummyProgressTracker progressTracker = null)
		{
			var viewModel = new ProjectsToImportViewModel(Factory)
			{
				ShouldImportIssueAttachments = shouldImportIssueAttachments,
				ShouldImportAllProjects = true,
			};

			var importer = new DummyJiraEntityImporter(viewModel, progressTracker ?? new JiraIntegrationTestHelper.DummyProgressTracker())
			{
				ManyIssuesJSONString = JiraIntegrationTestHelper.JSONForIssueWithComments
			};

			var credentials = new JiraCredentials("Admin", "000001");

			importer.IssueAttachmentContentGetter = attachment =>
			{
				if (!shouldImportIssueAttachments)
				{
					Fail("Shouldn't call the web service for attachments if we've opted not to.");
				}

				switch (attachment.ID)
				{
					case "10009":
						return new byte[] { 1, 1, 1 };
					case "10008":
						return new byte[] { 2, 2, 2 };
					case "10001":
						return new byte[] { 3, 3, 3 };
					case "10006":
						return new byte[] { 4, 4, 4 };
					case "10007":
						return new byte[] { 5, 5, 5 };

					default:
						return Array.Empty<byte>();
				}
			};

			AssertEquals(0, Factory.GetDatabaseCount(typeof(WorkItem)));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(Project)));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(StorageMain)));

			importer.Import(credentials);

			AssertEquals(1, Factory.GetDatabaseCount(typeof(WorkItem)));
			AssertEquals(1, Factory.GetDatabaseCount(typeof(Project)));
			AssertEquals(1, Factory.GetDatabaseCount(typeof(StorageMain)));

			if (shouldImportIssueAttachments)
			{
				var workItem = Factory.Load<WorkItem>(new ZQuery()).Single();

				AssertEDocsInDatabase(workItem.PK, "harrharrharr", "txt", new ZDateTime(2019, 2, 11, 3, 7, 0), new byte[] { 1, 1, 1 });
				AssertEDocsInDatabase(workItem.PK, "Microsoft Edge", "lnk", new ZDateTime(2019, 2, 11, 3, 7, 0), new byte[] { 2, 2, 2 });
				AssertEDocsInDatabase(workItem.PK, "pOOOOOOrg", "tga", new ZDateTime(2019, 1, 21, 22, 46, 0), new byte[] { 3, 3, 3 });
				AssertEDocsInDatabase(workItem.PK, "Wireframe - Workflow Details", "tga", ZDateTime.UtcNow, new byte[] { 4, 4, 4 });
				AssertEDocsInDatabase(workItem.PK, "Wireframe - Workflow Details (ba7209e3-1232-49da-abd2-cada0846c7d4)", "tga", ZDateTime.UtcNow, new byte[] { 5, 5, 5 });
			}
		}

		void AssertEDocsInDatabase(ZGuid workItemPK, string expectedFileName, string expectedFileExtension, ZDateTime expectedCreateTime, byte[] expectedContent)
		{
			JiraIntegrationTestHelper.AssertAndGetEDocInDatabase(Factory, workItemPK, expectedFileName, expectedFileExtension, expectedCreateTime, expectedContent);
		}

		public void TestImportComments_ShouldNotRequireExtraWebServiceCalls()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "janmichaelvincent@xyz.com";
			staff1.GS_FullName = "Jan Michael Vincent";

			Factory.Save();

			var dummyImporter = new DummyJiraEntityImporter(Factory) { ManyIssuesJSONString = JiraIntegrationTestHelper.JSONForIssueWithComments };
			JiraIntegrationTestHelper.AssertImportWasSuccessful(dummyImporter);

			AssertEquals("Expected one call for the project, one for project lead, and one for the issues, none for comments. SAD!", 3, dummyImporter.NumberOfWebServiceRequestsExecutedOrSimulated);

			var newJiraQuery = new ManyIssuesJiraQuery("Hello", 0);
			AssertEquals("Should generate a query with *ALL* fields so that comments are included with bodies in plain text (removing this from the query causes them to be jira formatted). Pls don't remove *all ;)", "search?jql=project=Hello&startAt=0&maxResults=-1&fields=*all&expand=renderedFields", newJiraQuery.QueryString);
		}

		public void TestImportIssues_ShouldNotRequireExtraWebServiceCallsToObtainIssueCreator()
		{
			var dummyImporter = new DummyJiraEntityImporter(Factory);
			JiraIntegrationTestHelper.AssertImportWasSuccessful(dummyImporter);

			AssertEquals("Expected one call for the project, one for project lead, one for the issues, and none for the issue creator. SAD!", 3, dummyImporter.NumberOfWebServiceRequestsExecutedOrSimulated);
		}

		#endregion

		#region Issue Description

		[TestDate(2019, 1, 1)]
		public void TestJiraIssueToWorkitem_WithMultilineDescription_ShouldAddEDocContainingHTMLContent()
		{
			var credentials = new JiraCredentials("Admin", "000001");

			JiraDescriptionDecoder GetDocumentFactory(IDocumentFactory docFactory)
			{
				return new JiraDescriptionDecoderForTest(docFactory)
				{
					ReturnBadDocData = false
				};
			}

			var importer = new DummyJiraEntityImporter(Factory)
			{
				DescriptionDecoderToUse = GetDocumentFactory,
				ManyIssuesJSONString = JiraIntegrationTestHelper.JSONForOneIssueWithRenderedFields,
			};

			importer.Import(credentials);

			var workItem = Factory.Load<WorkItem>(new ZQuery()).Single();
			var jiraIssue = JiraToCargoWiseDecoder.DecodeIssueJSONArray(JiraIntegrationTestHelper.JSONForOneIssueWithRenderedFields).First();

			JiraIntegrationTestHelper.AssertDescriptionEDocAndHyperlinkAreCorrect(Factory, workItem, jiraIssue.Code, jiraIssue.Description);
		}

		[TestDate(2019, 1, 1)]
		public void TestJiraIssueToWorkItem_WithFailedHyperlinkCreation_ShouldSupplyFillerDescription()
		{
			using (ObjectFactory.Substitute(Mock.Of<IShowEDocUrlHandler>()))
			{
				var credentials = new JiraCredentials("Admin", "000001");

				JiraDescriptionDecoder GetDocumentFactory(IDocumentFactory docFactory)
				{
					return new JiraDescriptionDecoderForTest(docFactory)
					{
						ReturnBadDocData = false
					};
				}

				var importer = new DummyJiraEntityImporter(Factory)
				{
					DescriptionDecoderToUse = GetDocumentFactory,
					ManyIssuesJSONString = JiraIntegrationTestHelper.JSONForOneIssueWithRenderedFields,
				};

				importer.Import(credentials);

				var workItem = Factory.Load<WorkItem>(new ZQuery()).Single();
				var jiraIssue = JiraToCargoWiseDecoder.DecodeIssueJSONArray(JiraIntegrationTestHelper.JSONForOneIssueWithRenderedFields).First();
				var descriptionDoc = JiraIntegrationTestHelper.AssertAndGetDescriptionEDoc(Factory, workItem.PK, jiraIssue.Code, jiraIssue.Description);
				JiraIntegrationTestHelper.AssertDescriptionDetails("Could not add description hyperlink, please see eDocs tab", workItem);
			}
		}

		[TestDate(2019, 1, 1)]
		public void TestJiraIssueToWorkItem_WithFailedDescriptionConvert_ShouldImportNoDescription()
		{
			var credentials = new JiraCredentials("Admin", "000001");

			JiraDescriptionDecoder GetDocumentFactory(IDocumentFactory docFactory)
			{
				return new JiraDescriptionDecoderForTest(docFactory)
				{
					ReturnBadDocData = true
				};
			}

			var importer = new DummyJiraEntityImporter(Factory)
			{
				DescriptionDecoderToUse = GetDocumentFactory,
				ManyIssuesJSONString = JiraIntegrationTestHelper.JSONForOneIssueWithRenderedFields,
			};

			importer.Import(credentials);

			var workItem = Factory.Load<WorkItem>(new ZQuery()).Single();
			var documentFactory = new DbBackendDocumentFactory(Factory);
			var storageMain = documentFactory.LoadTop1<StorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, workItem.PK));
			AssertEquals("We should have no storageMain row created, since there were no docs to add to the database.", null, storageMain);

			JiraIntegrationTestHelper.AssertDescriptionDetails("Could not convert Jira Description to eDocs Description.", workItem);
		}

		[TestDate(2019, 1, 1)]
		public void TestJiraIssueToWorkItem_WithNoRenderedDescriptionConvert_ShouldFallbackToSingleLineDescription()
		{
			var credentials = new JiraCredentials("Admin", "000001");

			JiraDescriptionDecoder GetDocumentFactory(IDocumentFactory docFactory)
			{
				return new JiraDescriptionDecoderForTest(docFactory)
				{
					ReturnBadDocData = false
				};
			}

			var importer = new DummyJiraEntityImporter(Factory)
			{
				DescriptionDecoderToUse = GetDocumentFactory,
				ManyIssuesJSONString = JiraIntegrationTestHelper.JSONForOneIssueWithoutRenderedFields,
			};

			importer.Import(credentials);

			var workItem = Factory.Load<WorkItem>(new ZQuery()).Single();
			var jiraIssue = JiraToCargoWiseDecoder.DecodeIssueJSONArray(JiraIntegrationTestHelper.JSONForOneIssueWithoutRenderedFields).First();
			var documentFactory = new DbBackendDocumentFactory(Factory);
			var storageMain = documentFactory.LoadTop1<StorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, workItem.PK));
			AssertEquals("We should have no storageMain row created, since there were no docs to add to the database.", null, storageMain);

			JiraIntegrationTestHelper.AssertDescriptionDetails(jiraIssue.Description, workItem);
		}

		#endregion

		#region Progress View

		[TestDate(2019, 1, 1)]
		public void TestProgressViewMessages_ProjectsAndIssues()
		{
			var dummyProgressTracker = new JiraIntegrationTestHelper.DummyProgressTracker();
			var dummyImporter = new DummyJiraEntityImporter(new ProjectsToImportViewModel(Factory) { ShouldImportAllProjects = true }, progressTracker: dummyProgressTracker)
			{
				AllProjectsJSONString = JiraIntegrationTestHelper.JSONForTwoProjects,
				ManyIssuesJSONString = JiraIntegrationTestHelper.JSONForTwoIssues
			};

			JiraIntegrationTestHelper.AssertImportWasSuccessful(dummyImporter);

			var expectedStatusHistory = new[]
			{
				Tuple.Create(@"Beginning download...", 0),
				Tuple.Create(@"Importing Project AVINA (1 of 2)", 0),
				Tuple.Create(@"Importing Project AVINA (1 of 2)
Downloading Issues...", 0),
				Tuple.Create(@"Importing Project AVINA (1 of 2)
Importing Issue AVINA-3 (1 of 2)", 0),
				Tuple.Create(@"Importing Project AVINA (1 of 2)
Importing Issue AVINA-4 (2 of 2)", 0),
				Tuple.Create(@"Importing Project BP (2 of 2)", 50),
				Tuple.Create(@"Importing Project BP (2 of 2)
Downloading Issues...", 50),
			};

			AssertSequencesEqual(expectedStatusHistory, dummyProgressTracker.StatusHistory);
		}

		[TestDate(2019, 1, 1)]
		public void TestProgressViewMessages_Attachments()
		{
			var dummyProgressTracker = new JiraIntegrationTestHelper.DummyProgressTracker();
			ImportIssuesAndAssertAttachmentsCreatedAsEDocs(shouldImportIssueAttachments: true, progressTracker: dummyProgressTracker);

			var expectedStatusHistory = new[]
			{
				Tuple.Create("Beginning download...", 0),
				Tuple.Create(@"Importing Project BP (1 of 1)", 0),
				Tuple.Create(@"Importing Project BP (1 of 1)
Downloading Issues...", 0),
				Tuple.Create(@"Importing Project BP (1 of 1)
Importing Issue AVINA-4 (1 of 1)", 0),
				Tuple.Create(@"Importing Project BP (1 of 1)
Importing Issue AVINA-4 (1 of 1)
Downloading attachment harrharrharr.txt", 0),
				Tuple.Create(@"Importing Project BP (1 of 1)
Importing Issue AVINA-4 (1 of 1)
Downloading attachment Microsoft Edge.ln...", 0),
				Tuple.Create(@"Importing Project BP (1 of 1)
Importing Issue AVINA-4 (1 of 1)
Downloading attachment pOOOOOOrg.tga", 0),
				Tuple.Create(@"Importing Project BP (1 of 1)
Importing Issue AVINA-4 (1 of 1)
Downloading attachment Wireframe - Workf...", 0),
				Tuple.Create(@"Importing Project BP (1 of 1)
Importing Issue AVINA-4 (1 of 1)
Downloading attachment Wireframe - Workf...", 0),
				Tuple.Create(@"Importing Project BP (1 of 1)
Importing Issue AVINA-4 (1 of 1)", 0),
			};

			AssertSequencesEqual(expectedStatusHistory, dummyProgressTracker.StatusHistory);
		}

		[TestDate(2019, 1, 1)]
		public void TestProgressViewMessages_WhenIssuesExceedJiraDownloadLimit_ShouldUpdateForEachBatch()
		{
			var dummyProgressTracker = new JiraIntegrationTestHelper.DummyProgressTracker();
			var dummyImporter = new DummyJiraEntityImporterWithMultipleIssueBatchJsonResultsFromJira(new ProjectsToImportViewModel(Factory) { ShouldImportAllProjects = true }, progressTracker: dummyProgressTracker);
			dummyImporter.OverrideMaxJiraDownloadBatchSize(2);
			dummyImporter.AddJsonToReturnForBatchOfIssueRequests(JiraIntegrationTestHelper.JSONForTwoIssues);
			dummyImporter.AddJsonToReturnForBatchOfIssueRequests(JiraIntegrationTestHelper.JSONForTwoIssues.Replace(@"""id"": ""10054""", @"""id"": ""30054""").Replace(@"""id"": ""10044""", @"""id"": ""30044"""));
			dummyImporter.AddJsonToReturnForBatchOfIssueRequests(JiraIntegrationTestHelper.JSONForOneIssue_LongKey);

			JiraIntegrationTestHelper.AssertImportWasSuccessful(dummyImporter);

			var expectedStatusHistory = new[]
			{
				Tuple.Create(@"Beginning download...", 0),
				Tuple.Create(@"Importing Project BP (1 of 1)", 0),
				Tuple.Create(@"Importing Project BP (1 of 1)
Downloading Issues...", 0),
				Tuple.Create(@"Importing Project BP (1 of 1)
Downloading Issues... (2 received)", 0),
				Tuple.Create(@"Importing Project BP (1 of 1)
Downloading Issues... (4 received)", 0),
				Tuple.Create(@"Importing Project BP (1 of 1)
Importing Issue AVINA-3 (1 of 5)", 0),
				Tuple.Create(@"Importing Project BP (1 of 1)
Importing Issue AVINA-3 (2 of 5)", 0),
				Tuple.Create(@"Importing Project BP (1 of 1)
Importing Issue AVINA-4 (3 of 5)", 0),
				Tuple.Create(@"Importing Project BP (1 of 1)
Importing Issue AVINA-4 (4 of 5)", 0),
				Tuple.Create(@"Importing Project BP (1 of 1)
Importing Issue AVINA-4 (5 of 5)", 0),
			};

			AssertSequencesEqual(expectedStatusHistory, dummyProgressTracker.StatusHistory);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			JiraIntegrationTestHelper.SetJiraUrlsRegistryItem();
			JiraIntegrationTestHelper.AddDefaultOrgProxyClient();
		}

		#endregion
	}
}
