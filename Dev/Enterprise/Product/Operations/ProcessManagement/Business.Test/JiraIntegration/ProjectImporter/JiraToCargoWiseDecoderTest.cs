using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	public class JiraToCargoWiseDecoderTest : TestCaseWithFactory
	{
		#region JSON Deserialisation

		public void TestJiraProjectDecoder_ShouldNotifyUserOfASubstringOfAnErrorWhenGivenInvalidJSON()
		{
			var jiraResult = JiraIntegrationTestHelper.EmptyJiraResult();
			var htmlString = "<!DOCTYPE html><html lang=\"en\" class=\"aui-responsive\"><!DOCTYPE html><html lang=\"en\" class=\"aui-responsive\"><!DOCTYPE html><html lang=\"en\" class=\"aui-responsive\">";

			AssertNoExceptionThrown(() => JiraToCargoWiseDecoder.DecodeProjectJSONArray(htmlString, jiraResult));
			AssertEquals(JiraResponseStatus.GenericFailure, jiraResult.Status);
			AssertContains("The information returned wasn't in the expected format and can't be converted into Projects and Work Items. Please ensure the Jira URL is correct. Please contact Atlassian support for help determining the correct URL to use.", jiraResult.Response);
			AssertContains("<!DOCTYPE html><html lang=\"en\" class=\"aui-responsive\"><!DOCTYPE html><html lang=\"en\" class=\"aui-responsive\"><!DOCTYPE html><html lang=\"en\" c", jiraResult.Response);
			AssertNotContains(htmlString, jiraResult.Response);
		}

		public void TestJiraProjectDecoder_ShouldNotifyUserOfAnErrorWhenGivenInvalidJSON()
		{
			var jiraResult = JiraIntegrationTestHelper.EmptyJiraResult();
			var htmlString = "<!DOCTYPE html><html lang=\"en\" class=\"aui-responsive\"><!DOCTYPE html>";

			AssertNoExceptionThrown(() => JiraToCargoWiseDecoder.DecodeProjectJSONArray(htmlString, jiraResult));
			AssertEquals(JiraResponseStatus.GenericFailure, jiraResult.Status);
			AssertContains("The information returned wasn't in the expected format and can't be converted into Projects and Work Items. Please ensure the Jira URL is correct. Please contact Atlassian support for help determining the correct URL to use.", jiraResult.Response);
			AssertContains(htmlString, jiraResult.Response);
		}

		#region Projects

		public void TestJiraProjectDecoder_ShouldCreateValidJiraProject()
		{
			var jiraResult = JiraIntegrationTestHelper.EmptyJiraResult();
			var createdJiraProject = JiraToCargoWiseDecoder.DecodeProjectJSONArray(JiraIntegrationTestHelper.JSONForOneProject, jiraResult).First();

			AssertNotNull(createdJiraProject);
			JiraIntegrationTestHelper.AssertJiraResultIsSuccessfulAndNoResponseIsSet(jiraResult);
			CombineAssertions("Given valid JSON, we can create a valid JiraProject entity", () =>
			{
				AssertEquals("ID", "10008", createdJiraProject.ID);
				AssertEquals("Code", "BP", createdJiraProject.Code);
				AssertEquals("Description", "Bibbity Bobb", createdJiraProject.Description);
				AssertEquals("Project Lead ID", "11235", createdJiraProject.ProjectLeadID);
				AssertEquals("Email, which is set later", null, createdJiraProject.ProjectLeadEmail);
				AssertEquals("Name", "BusinessProject", createdJiraProject.Name);
			});
		}

		public void TestJiraProjectDecoder_ShouldCreateValidJiraProjects_WhenSuppliedWithJSONArray()
		{
			var jiraResult = JiraIntegrationTestHelper.EmptyJiraResult();
			var createdJiraProject = JiraToCargoWiseDecoder.DecodeProjectJSONArray(JiraIntegrationTestHelper.JSONForTwoProjects, jiraResult);

			AssertNotNull(createdJiraProject);
			JiraIntegrationTestHelper.AssertJiraResultIsSuccessfulAndNoResponseIsSet(jiraResult);
			AssertEquals("We gave info enough for two projects, and received two projects", 2, createdJiraProject.Length);

			var firstProject = createdJiraProject[0];
			var secondProject = createdJiraProject[1];

			CombineAssertions("Given valid JSON, we can create a valid JiraProject entity", () =>
			{
				AssertEquals("ID", "10001", firstProject.ID);
				AssertEquals("Code", "AVINA", firstProject.Code);
				AssertEquals("Description", "hello I am descriptive", firstProject.Description);
				AssertEquals("Project Lead ID", "11235", firstProject.ProjectLeadID);
				AssertEquals("Email, which is set later", null, firstProject.ProjectLeadEmail);
				AssertEquals("Name", "Avengers Initiative", firstProject.Name);

				AssertEquals("ID", "10008", secondProject.ID);
				AssertEquals("Code", "BP", secondProject.Code);
				AssertEquals("Description", "Business-handling project of the business-most ordinance", secondProject.Description);
				AssertEquals("Project Lead ID", "11235", secondProject.ProjectLeadID);
				AssertEquals("Email, which is set later", null, secondProject.ProjectLeadEmail);
				AssertEquals("Name", "Business Project", secondProject.Name);
			});
		}

		public void TestJiraProjectDecoder_ShouldDoNothing_WhenSuppliedWithEmptyArray()
		{
			var jiraResult = JiraIntegrationTestHelper.EmptyJiraResult();
			JiraProject[] createdJiraProjects;
			JiraIntegrationTestHelper.AssertJiraResultIsSuccessfulAndNoResponseIsSet(jiraResult);
			AssertNoExceptionThrown("Attempting to decode an empty JSON array is ok!", () => createdJiraProjects = JiraToCargoWiseDecoder.DecodeProjectJSONArray(JiraIntegrationTestHelper.JSONForNothing, jiraResult));
			createdJiraProjects = JiraToCargoWiseDecoder.DecodeProjectJSONArray(JiraIntegrationTestHelper.JSONForNothing, jiraResult); // otherwise compiler doesn't understand what we're doing
			JiraIntegrationTestHelper.AssertJiraResultIsSuccessfulAndNoResponseIsSet(jiraResult);
			AssertEquals("Invalid JQueries should create empty arrays instead of exceptions", 0, createdJiraProjects.Length);
		}

		#endregion Projects

		#region Issues

		public void TestJiraIssueDecoder_ShouldCreateValidIssue()
		{
			var createdJiraIssues = JiraToCargoWiseDecoder.DecodeIssueJSONArray(JiraIntegrationTestHelper.JSONForOneIssue);

			AssertNotNull(createdJiraIssues);
			AssertEquals("Jira Issues come in paginated format, but we can decode it and recognise even single issues", 1, createdJiraIssues.Length);

			var createdJiraIssue = createdJiraIssues.First();
			CombineAssertions("Given valid JSON, we can create a valid JiraIssue entity", () =>
			{
				AssertEquals("ID", "10054", createdJiraIssue.ID);
				AssertEquals("Code", "AVINA-4", createdJiraIssue.Code);
				AssertEquals("Project Code", "AVINA", createdJiraIssue.ProjectCode);
				AssertEquals("Description", "<p>Description field but I changed it</p>", createdJiraIssue.Description);
				AssertEquals("Summary", "Buge", createdJiraIssue.Summary);
				AssertEquals("Creator ID", "20000", createdJiraIssue.Creator?.AccountID);
			});
		}

		public void TestJiraIssueDecoder_ShouldCreateValidJiraIssues_WhenSuppliedWithJSONArray()
		{
			var createdJiraIssues = JiraToCargoWiseDecoder.DecodeIssueJSONArray(JiraIntegrationTestHelper.JSONFourForIssues);

			AssertNotNull(createdJiraIssues);
			AssertEquals("Jira Issues come in paginated format, and we can recognise the response of multiple issues", 4, createdJiraIssues.Length);

			var issueOne = createdJiraIssues[0];
			var issueTwo = createdJiraIssues[1];
			var issueThree = createdJiraIssues[2];
			var issueFour = createdJiraIssues[3];

			CombineAssertions("We extracted the first issue from the JSON issue array", () =>
			{
				AssertEquals("ID", "10054", issueOne.ID);
				AssertEquals("Code", "AVINA-4", issueOne.Code);
				AssertEquals("Project Code", "AVINA", issueOne.ProjectCode);
				AssertEquals("Description", "Description field but I changed it", issueOne.Description);
				AssertEquals("Summary", "Buge", issueOne.Summary);
				AssertEquals("Creator ID", "11235", issueOne.Creator?.AccountID);
				AssertEquals("customfield_10907", "EDI & Services", issueOne.CustomFields.FirstOrDefault(_ => _.Id == "10907" && _.Key == "value")?.Value);
			});

			CombineAssertions("We extracted the second issue from the JSON issue array", () =>
			{
				AssertEquals("ID", "10044", issueTwo.ID);
				AssertEquals("Code", "AVINA-3", issueTwo.Code);
				AssertEquals("Project Code", "AVINA", issueTwo.ProjectCode);
				AssertEquals("Description", "They're really neat, make em shiny", issueTwo.Description);
				AssertEquals("Summary", "Make Helicarrier", issueTwo.Summary);
				AssertEquals("Creator ID", "11235", issueTwo.Creator?.AccountID);
			});

			CombineAssertions("We extracted the third issue from the JSON issue array", () =>
			{
				AssertEquals("ID", "10043", issueThree.ID);
				AssertEquals("Code", "AVINA-2", issueThree.Code);
				AssertEquals("Project Code", "AVINA", issueThree.ProjectCode);
				AssertEquals("Description", "Do the thing", issueThree.Description);
				AssertEquals("Summary", "Recruit Thor", issueThree.Summary);
				AssertEquals("Creator ID", "11235", issueThree.Creator?.AccountID);
			});

			CombineAssertions("We extracted the final issue from the JSON issue array", () =>
			{
				AssertEquals("ID", "10003", issueFour.ID);
				AssertEquals("Code", "AVINA-1", issueFour.Code);
				AssertEquals("Project Code", "AVINA", issueFour.ProjectCode);
				AssertEquals("Description", "Gimmie cheeseburger Obadiah", issueFour.Description);
				AssertEquals("Summary", "Get Iron Man Dude", issueFour.Summary);
				AssertEquals("Creator ID", "11235", issueFour.Creator?.AccountID);
			});
		}

		public void TestJiraIssueDecoder_ShouldNotExplodeViolentlyOnInvalidJSON()
		{
			JiraIssue[] createdJiraIssues;

			AssertNoExceptionThrown("Attempting to decode an empty JSON array is ok!", () => createdJiraIssues = JiraToCargoWiseDecoder.DecodeIssueJSONArray(JiraIntegrationTestHelper.JSONForNothing));
			createdJiraIssues = JiraToCargoWiseDecoder.DecodeIssueJSONArray(JiraIntegrationTestHelper.JSONForNothing); // otherwise compiler doesn't understand what we're doing
			AssertEquals("Invalid JQueries should create empty arrays instead of exceptions", 0, createdJiraIssues.Length);
		}

		#endregion Issues

		#region Users

		public void TestJiraUserDecoder_ShouldCreateValidJiraUser()
		{
			var createdJiraUser = JiraToCargoWiseDecoder.GetUserFromJSON(JiraIntegrationTestHelper.JSONForOneUser);

			AssertNotNull(createdJiraUser);

			CombineAssertions("Given valid JSON, we can create a valid JiraUser entity", () =>
			{
				AssertEquals("ID", "11235", createdJiraUser.AccountID);
				AssertEquals("Code", "boberly.lastingtonnamersen", createdJiraUser.Key);
				AssertEquals("Name, which is different to Display Name", "Boberly.LastingtonNamersen", createdJiraUser.Name);
				AssertEquals("Email", "boberly.lastingtonnamersen@sampleweb.com", createdJiraUser.Email);
				AssertEquals("Display Name", "Boberly Namersen", createdJiraUser.DisplayName);
				AssertEquals("Summary", true, createdJiraUser.IsActive);
			});
		}

		public void TestJiraUserDecoder_ShouldNotDetonateProfuselyOnInvalidJSON()
		{
			var createdJiraUser = JiraToCargoWiseDecoder.GetUserFromJSON(JiraIntegrationTestHelper.JSONForNothing);

			AssertNull(createdJiraUser);
		}

		#endregion Users

		#endregion JSON Deserialisation

		#region JiraEntity to Bizo conversion

		public void TestJiraProjectToProject()
		{
			var jiraProject = JiraToCargoWiseDecoder.DecodeProjectJSONArray(JiraIntegrationTestHelper.JSONForOneProject, JiraIntegrationTestHelper.EmptyJiraResult()).First();
			var convertedProject = JiraToCargoWiseDecoder.GetProjectFromJiraProject(jiraProject, Factory);

			// Set client manually... this will be done by the importer when the decoder is used in production.
			var contact = ProcessMgmtTestHelper.CreateOrganizationAndContact(Factory);
			convertedProject.WKP_OA_ClientAddress = contact.Header.MainAddress.PK;
			convertedProject.WKP_OC_Contact = contact.PK;

			AssertDecodedObjectExistsWithNoErrors(convertedProject);

			var factorio = new BusinessObjectFactory();
			var loadedConvert = factorio.Load<Project>(convertedProject.PK);

			CombineAssertions("We've made a Project with correctly transferred values", () =>
			{
				AssertEquals("Summary", "BP - BusinessProject", loadedConvert.WKP_Summary.ToString());
				AssertEquals("Details", "Bibbity Bobb", loadedConvert.WKP_Details.ToUTF8());
				AssertEquals("Status", ProcessTaskStatusCodeList.Codes.Working, loadedConvert.WKP_Status);
			});
		}

		#region JiraIssue to WorkItem

		[TestDate(2019, 1, 1)]
		public void TestJiraIssueToWorkItem()
		{
			var jiraIssue = JiraToCargoWiseDecoder.DecodeIssueJSONArray(JiraIntegrationTestHelper.JSONForOneIssue).First();
			var convertedIssue = JiraToCargoWiseDecoder.GetWorkItemFromJiraIssue(jiraIssue, Factory);

			AssertDecodedObjectExistsWithNoErrors(convertedIssue);
			AssertNullOrEmpty("We should not have a workItem summary yet because it's created during the docCreation phase", convertedIssue.WKI_Details.ToUTF8().ToString());

			JiraToCargoWiseDecoder.SetWorkItemDescription(convertedIssue, "Gibberish to show that this works");
			Factory.Save();

			var factorio = new BusinessObjectFactory();
			var loadedConvert = factorio.Load<WorkItem>(convertedIssue.PK);

			CombineAssertions("We've made a Work Item with correctly transferred values", () =>
			{
				AssertEquals("Summary", "AVINA-4: Buge", loadedConvert.WKI_Summary); // change the summary we get here to include the jiraIssue Code and the jiraIssue Summary with a colon space in between
				AssertEquals("Details", "Gibberish to show that this works", loadedConvert.WKI_Details.ToUTF8().ToString());
			});
		}

		[TestDate(2019, 1, 1)]
		public void TestJiraIssueCustomField_NoProductAreaSet()
		{
			var jiraIssue = JiraToCargoWiseDecoder.DecodeIssueJSONArray(JiraIntegrationTestHelper.JSONForOneIssue).First();
			var convertedIssue = JiraToCargoWiseDecoder.GetWorkItemFromJiraIssue(jiraIssue, Factory);

			AssertEquals("Product Area should be default as it is not set via any method", convertedIssue.WKI_WorkItemArea, "");

			AssertDecodedObjectExistsWithNoErrors(convertedIssue);
		}

		[TestDate(2019, 1, 1)]
		public void TestJiraIssueCustomField_AreaToProductArea_WithFullSave()
		{
			var jiraIssue = JiraToCargoWiseDecoder.DecodeIssueJSONArray(JiraIntegrationTestHelper.JSONFourForIssues).First();

			var referenceJiraCustomField = jiraIssue.CustomFields.FirstOrDefault(_ => _.Id == "10907" && _.Key == "value");
			AssertNotNull("The expected reference custom field value is missing in the test data :(", referenceJiraCustomField);

			var convertedIssue = JiraToCargoWiseDecoder.GetWorkItemFromJiraIssue(jiraIssue, Factory, CreateCustomFieldMapping());

			AssertEquals("Product Area is not correctly set from custom field", convertedIssue.WKI_WorkItemArea, "EDS");

			// No work item areas do exist in Odyssey - therefore set it to UDF to pass the "no errors" check.
			convertedIssue.WKI_WorkItemArea = "UDF";

			AssertDecodedObjectExistsWithNoErrors(convertedIssue);
			AssertNullOrEmpty("We should not have a workItem summary yet because it's created during the docCreation phase", convertedIssue.WKI_Details.ToUTF8().ToString());

			JiraToCargoWiseDecoder.SetWorkItemDescription(convertedIssue, "Gibberish to show that this works");
			Factory.Save();

			var factorio = new BusinessObjectFactory();
			var loadedConvert = factorio.Load<WorkItem>(convertedIssue.PK);

			CombineAssertions("We've made a Work Item with correctly transferred values", () =>
			{
				AssertEquals("Summary", "AVINA-4: Buge", loadedConvert.WKI_Summary); // change the summary we get here to include the jiraIssue Code and the jiraIssue Summary with a colon space in between
				AssertEquals("Details", "Gibberish to show that this works", loadedConvert.WKI_Details.ToUTF8().ToString());
			});
		}

		[TestDate(2019, 1, 1)]
		public void TestJiraIssueToWorkItem_LongSummaryIsTruncated()
		{
			var jiraIssue = JiraToCargoWiseDecoder.DecodeIssueJSONArray(JiraIntegrationTestHelper.JSONForOneIssue_LongKey).First();
			var convertedIssue = JiraToCargoWiseDecoder.GetWorkItemFromJiraIssue(jiraIssue, Factory);

			AssertDecodedObjectExistsWithNoErrors(convertedIssue);
			AssertNullOrEmpty("We should not have a workItem summary yet because it's created during the docCreation phase", convertedIssue.WKI_Details.ToUTF8().ToString());

			var factorio = new BusinessObjectFactory();
			var loadedConvert = factorio.Load<WorkItem>(convertedIssue.PK);

			CombineAssertions("We've made a Work Item with correctly transferred values", () =>
			{
				AssertEquals("Summary, truncated to 80 chars", "AVINA-4: VeryLong-VeryVery-Long-SuperDuperLong-WhyIsThisSoLong?-ThisCanNeverHapp", loadedConvert.WKI_Summary); // change the summary we get here to include the jiraIssue Code and the jiraIssue Summary with a colon space in between, that's then truncated
				AssertEquals("Summary truncated to WKI_Summary.MaxLength", AutoWorkItem.Schema.WKI_SummaryMaxLength, loadedConvert.WKI_Summary.Length);
			});
		}

		IEnumerable<JiraCustomFieldMapItem> CreateCustomFieldMapping()
		{
			yield return new JiraCustomFieldMapItem()
			{
				JiraEntityName = "EDI & Services",
				CustomFieldId = "10907",
				SelectionCriterionFieldName = "WKI_WorkItemArea",
				SelectionCriterionFieldValue = "EDS",
			};
		}

		void AssertDecodedObjectExistsWithNoErrors(BusinessObject jiraObject)
		{
			AssertNotNull("We actually made a thing", jiraObject);
			AssertNoExceptionThrown("We made a thing that saves properly", () => Factory.Save());
			AssertNoErrors("We made a thing that doesn't cause horrible errors", jiraObject);
		}

		#endregion JiraIssue to WorkItem

		#endregion JiraEntity to Bizo conversion
	}
}
