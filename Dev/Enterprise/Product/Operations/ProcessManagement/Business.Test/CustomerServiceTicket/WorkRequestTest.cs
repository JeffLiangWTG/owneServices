using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(WorkRequest))]
	class WorkRequestTest : EnterpriseBusinessObjectTestCase
	{
		#region Grrr

		public void TestUpdateClientShouldUpdateOrganisation()
		{
			var request = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "Request X");

			AssertEquals("Precondition", ZGuid.Empty, request.OrganisationPK);

			var client = Factory.NewWithValidTestData<OrgContact>();
			request.WKR_OC_Client = client.PK;

			AssertEquals("WHEN setting client then Organisation should be updated", client.ParentOrg.PK, request.OrganisationPK);

			var currentOrganizationPK = request.OrganisationPK;
			request.WKR_OC_Client = ZGuid.NewZGuid();
			AssertEquals("WHEN setting client to invalid then Organisation should not be updated", currentOrganizationPK, request.OrganisationPK);
		}

		#endregion

		#region Properties

		public void TestJobNumber_ShouldBeCreatedOnSave()
		{
			var request1 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "Dinglebop");
			var request2 = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "Schleem");

			request1.WKR_OC_Client = Factory.NewWithValidTestData<OrgContact>().PK;
			request2.WKR_OC_Client = Factory.NewWithValidTestData<OrgContact>().PK;

			AssertEquals("", request1.WKR_RequestNumber);
			AssertEquals("", request2.WKR_RequestNumber);

			Factory.Save();

			AssertEquals("CST00000001", request1.WKR_RequestNumber);
			AssertEquals("CST00000002", request2.WKR_RequestNumber);
		}

		public void TestHumanReadableNames()
		{
			var request = ProcessMgmtTestHelper.CreateWorkRequest(Factory, "Fleeb");
			AssertEquals("Customer Service Ticket - Fleeb", request.HumanReadableName);
			AssertEquals("Fleeb", request.HumanReadableShortcutName);

			Factory.Save();

			AssertEquals("Customer Service Ticket - CST00000001 - Fleeb", request.HumanReadableName);
			AssertEquals("CST00000001 - Fleeb", request.HumanReadableShortcutName);

			request.WKR_RequestNumber = ZString.Empty;
			AssertEquals("Customer Service Ticket - Fleeb", request.HumanReadableName);
			AssertEquals("Fleeb", request.HumanReadableShortcutName);

			request.WKR_Summary = ZString.Empty;
			request.WKR_RequestNumber = "CST00000001";
			AssertEquals("Customer Service Ticket - CST00000001", request.HumanReadableName);
			AssertEquals("CST00000001", request.HumanReadableShortcutName);

			request.WKR_Summary = ZString.Empty;
			request.WKR_RequestNumber = ZString.Empty;
			AssertEquals("Customer Service Ticket", request.HumanReadableName);
			AssertEquals(ZString.Empty, request.HumanReadableShortcutName);
		}

		public void TestSelectionCriterionCaption()
		{
			var workRequest = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var testString1 = "TestSTring1";
			var testString2 = "TestSTring2";
			var testString3 = "TestSTring3";
			var testString4 = "TestSTring4";
			var testString5 = "TestSTring5";

			ProcessManagementRegistry.Instance.SelectionCriterion1Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ZString(testString1));
			ProcessManagementRegistry.Instance.SelectionCriterion2Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ZString(testString2));
			ProcessManagementRegistry.Instance.SelectionCriterion3Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ZString(testString3));
			ProcessManagementRegistry.Instance.SelectionCriterion4Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ZString(testString4));
			ProcessManagementRegistry.Instance.SelectionCriterion5Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ZString(testString5));

			CombineAssertions("", () =>
			{
				AssertEquals("The property SelectionCriterion1Caption should have the value of the registry item SelectionCriterion1Caption.", workRequest.SelectionCriterion1Caption, testString1);
				AssertEquals("The property SelectionCriterion2Caption should have the value of the registry item SelectionCriterion2Caption.", workRequest.SelectionCriterion2Caption, testString2);
				AssertEquals("The property SelectionCriterion3Caption should have the value of the registry item SelectionCriterion3Caption.", workRequest.SelectionCriterion3Caption, testString3);
				AssertEquals("The property SelectionCriterion4Caption should have the value of the registry item SelectionCriterion4Caption.", workRequest.SelectionCriterion4Caption, testString4);
				AssertEquals("The property SelectionCriterion5Caption should have the value of the registry item SelectionCriterion5Caption.", workRequest.SelectionCriterion5Caption, testString5);
			}
			);
		}

		public void TestHasIncompleteWorkItems()
		{
			var workRequest = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var workItem1 = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			var workItem2 = ProcessMgmtTestHelper.CreateWorkItem(Factory);

			Factory.Save();

			AssertEquals(false, workRequest.HasIncompleteWorkItems);

			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest, workItem1);
			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest, workItem2);

			AssertEquals(false, workRequest.HasIncompleteWorkItems);

			var task1 = MasterFilesTestHelper.CreateTask(workItem1);
			AssertEquals(true, workRequest.HasIncompleteWorkItems);

			var task2 = MasterFilesTestHelper.CreateTask(workItem2);
			AssertEquals(true, workRequest.HasIncompleteWorkItems);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(true, workRequest.HasIncompleteWorkItems);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(false, workRequest.HasIncompleteWorkItems);
		}

		public void TestWereAllWorkItemsPreviouslyCompleted()
		{
			var workRequest = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest, workItem);

			var workRequestTask = MasterFilesTestHelper.CreateTask(workRequest);
			var workItemTask = MasterFilesTestHelper.CreateTask(workItem);

			Factory.Save();

			AssertEquals(false, workRequest.WereAllWorkItemsPreviouslyCompleted);

			workRequestTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(false, workRequest.WereAllWorkItemsPreviouslyCompleted);

			workItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(true, workRequest.WereAllWorkItemsPreviouslyCompleted);

			var newWorkItemTask = MasterFilesTestHelper.CreateTask(workItem);
			Factory.Save();

			AssertEquals(true, workRequest.WereAllWorkItemsPreviouslyCompleted);

			newWorkItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(true, workRequest.WereAllWorkItemsPreviouslyCompleted);
		}

		public void TestHasAttachedWorkItemsAndAllAreCancelled()
		{
			var workRequest = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var workItem1 = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			var workItem2 = ProcessMgmtTestHelper.CreateWorkItem(Factory);

			Factory.Save();

			AssertEquals(false, workRequest.HasAttachedWorkItemsAndAllAreCancelled);

			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest, workItem1);
			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest, workItem2);

			AssertEquals(false, workRequest.HasAttachedWorkItemsAndAllAreCancelled);

			var task1 = MasterFilesTestHelper.CreateTask(workItem1);
			var task2 = MasterFilesTestHelper.CreateTask(workItem2);
			AssertEquals(false, workRequest.HasAttachedWorkItemsAndAllAreCancelled);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(false, workRequest.HasAttachedWorkItemsAndAllAreCancelled);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(false, workRequest.HasAttachedWorkItemsAndAllAreCancelled);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(true, workRequest.HasAttachedWorkItemsAndAllAreCancelled);
		}

		public void TestHasAttachedWorkItemsAndAllAreCompleted()
		{
			var workRequest = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var workItem1 = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			var workItem2 = ProcessMgmtTestHelper.CreateWorkItem(Factory);

			Factory.Save();

			AssertEquals(false, workRequest.HasAttachedWorkItemsAndAllAreCompleted);

			workRequest.RelatedItems.Add(workItem1);
			workRequest.RelatedItems.Add(workItem2);

			AssertEquals(true, workRequest.HasAttachedWorkItemsAndAllAreCompleted);

			var task1 = MasterFilesTestHelper.CreateTask(workItem1);
			var task2 = MasterFilesTestHelper.CreateTask(workItem2);
			Factory.Save();

			AssertEquals(false, workRequest.HasAttachedWorkItemsAndAllAreCompleted);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(false, workRequest.HasAttachedWorkItemsAndAllAreCompleted);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(true, workRequest.HasAttachedWorkItemsAndAllAreCompleted);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			AssertEquals(false, workRequest.HasAttachedWorkItemsAndAllAreCompleted);
		}

		public void TestHasAttachedWorkItems()
		{
			var workRequest = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var workItem1 = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			var workItem2 = ProcessMgmtTestHelper.CreateWorkItem(Factory);

			Factory.Save();

			AssertEquals(false, workRequest.HasAttachedWorkItems);

			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest, workItem1);
			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest, workItem2);

			AssertEquals(true, workRequest.HasAttachedWorkItems);
		}

		public void TestWorkflowMacroTypes()
		{
			var workRequest = ProcessMgmtTestHelper.CreateWorkRequest(Factory);

			CombineAssertions("", () =>
			{
				AssertEquals("HasIncompleteWorkItems should be a ZBool", typeof(ZBool), workRequest.HasIncompleteWorkItems.GetType());
				AssertEquals("WereAllWorkItemsPreviouslyCompleted should be a ZBool", typeof(ZBool), workRequest.WereAllWorkItemsPreviouslyCompleted.GetType());
				AssertEquals("HasAttachedWorkItemsAndAllAreCancelled should be a ZBool", typeof(ZBool), workRequest.HasAttachedWorkItemsAndAllAreCancelled.GetType());
				AssertEquals("HasAttachedWorkItems should be a ZBool", typeof(ZBool), workRequest.HasAttachedWorkItems.GetType());
			});
		}

		public void TestStatus_ShouldBeReadOnly()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);

			AssertEquals("Status is maintained by the system. It must never be human-editable.", true, ticket.WKR_StatusInfo.ReadOnly);
		}

		#endregion

		#region Related Items

		public void TestSupportedRelatedItemModules_ShouldIncludeWorkItemOnly()
		{
			var workRequest = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			AssertContainsExactElementsInAnyOrder(new[] { ModuleIDs.WorkItem }, workRequest.SupportedRelatedItemModules.Select(x => x.ModuleID));
			AssertEquals(true, workRequest.SupportedRelatedItemModules.Single().AllowNew);
		}

		public void TestRelatedItems()
		{
			var relatedOpenWorkItem = Factory.NewWithValidTestData<WorkItem>();
			relatedOpenWorkItem.WKI_Summary = "Jan Quadrant Vincent 16";

			var relatedClosedWorkItem = Factory.NewWithValidTestData<WorkItem>();
			relatedClosedWorkItem.WKI_Summary = "Ants-In-My-Eyes Johnson";

			var unrelatedWorkItem = Factory.NewWithValidTestData<WorkItem>();
			unrelatedWorkItem.WKI_Summary = "Mr Sneezy 3D";

			MasterFilesTestHelper.CreateTask(relatedOpenWorkItem);
			MasterFilesTestHelper.CreateTask(unrelatedWorkItem);

			Factory.Save();

			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, relatedOpenWorkItem.WKI_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, relatedClosedWorkItem.WKI_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, unrelatedWorkItem.WKI_Status);

			var workRequest = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest, relatedOpenWorkItem);
			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest, relatedClosedWorkItem);

			IEnumerable<ZString> GetRelatedItemDescriptions() => workRequest.RelatedItems.Cast<IWorkTaskRelatedItem>().Select(x => x.ItemDescription);
			IEnumerable<ZString> GetFilteredRelatedItemDescriptions() => workRequest.FilteredRelatedItems.Cast<IWorkTaskRelatedItem>().Select(x => x.ItemDescription);

			AssertEquals(false, workRequest.ShowOnlyNonClosedItems);
			AssertContainsExactElementsInAnyOrder(new[] { "Jan Quadrant Vincent 16", "Ants-In-My-Eyes Johnson" }, GetRelatedItemDescriptions());
			AssertContainsExactElementsInAnyOrder(new[] { "Jan Quadrant Vincent 16", "Ants-In-My-Eyes Johnson" }, GetFilteredRelatedItemDescriptions());

			workRequest.ShowOnlyNonClosedItems = true;
			AssertContainsExactElementsInAnyOrder(new[] { "Jan Quadrant Vincent 16", "Ants-In-My-Eyes Johnson" }, GetRelatedItemDescriptions());
			AssertContainsExactElementsInAnyOrder(new[] { "Jan Quadrant Vincent 16" }, GetFilteredRelatedItemDescriptions());
		}

		public void TestAttachAndDetachWorkItem_ShouldCreateLogs()
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			workItem.WKI_Summary = "Little Bits";
			workItem.WKI_WorkItemNumber = "WI12345678";
			var workRequest = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			workRequest.WKR_RequestNumber = "WR87654321";
			Factory.Save();

			workRequest.RelatedItems.Add(workItem);

			IEnumerable<StmALog> GetLogs(EnterpriseBusinessObject parent) => parent.Logs.GetAllLogs().Cast<StmALog>().Where(x => x.Event.SE_Code == AutoEvents.Attached.Code || x.Event.SE_Code == AutoEvents.Detached.Code);

			var expectedAttachedLogText = "WI12345678 attached to WR87654321";
			AssertContainsExactElementsInAnyOrder(new[] { expectedAttachedLogText }, GetLogs(workRequest).Select(x => x.SL_Reference));
			AssertContainsExactElementsInAnyOrder(new[] { expectedAttachedLogText }, GetLogs(workItem).Select(x => x.SL_Reference));

			workRequest.RelatedItems.Remove(workItem);
			Factory.Save();

			var expectedDetachedLogText = "WI12345678 detached from WR87654321";
			AssertContainsExactElementsInAnyOrder(new[] { expectedAttachedLogText, expectedDetachedLogText }, GetLogs(workRequest).Select(x => x.SL_Reference));
			AssertContainsExactElementsInAnyOrder(new[] { expectedAttachedLogText, expectedDetachedLogText }, GetLogs(workItem).Select(x => x.SL_Reference));
		}

		public void TestIWorkTaskRelatedItemMembers()
		{
			var workRequest = Factory.NewWithValidTestData<WorkRequest>();
			workRequest.WKR_Summary = "Last Will and Testameow";
			var client = ProcessMgmtTestHelper.CreateOrganizationAndContact(Factory, "Weekend at Dead Cat Lady's House 2", "Mrs Sullivan");
			workRequest.WKR_OC_Client = client.PK;

			var relatedItem = (IWorkTaskRelatedItem)workRequest;
			AssertEquals(string.Empty, relatedItem.AssignedStaffCode);
			AssertEquals(workRequest.Client.OrganisationCode, relatedItem.ClientCode);
			AssertEquals("Weekend at Dead Cat Lady's House 2", relatedItem.ClientName);
			AssertEquals(ControllerIDs.CustomerServiceTicket, relatedItem.ControllerID);
			AssertEquals(string.Empty, relatedItem.Criticality);
			AssertEquals(false, relatedItem.IsClosedOrCancelled);
			AssertEquals("Last Will and Testameow", relatedItem.ItemDescription);
			AssertEquals(workRequest.WKR_RequestNumber, relatedItem.Number);
			AssertEquals(string.Empty, relatedItem.Source);
			AssertEquals("Open", relatedItem.StatusDescription);
			AssertEquals("Customer Service Ticket", relatedItem.Type);

			Factory.Save();
			AssertEquals(true, relatedItem.IsClosedOrCancelled);

			var task = MasterFilesTestHelper.CreateTask(workRequest);
			Factory.Save();

			AssertEquals(false, relatedItem.IsClosedOrCancelled);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			AssertEquals(true, relatedItem.IsClosedOrCancelled);
		}

		public void TestStatusDescription()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);

			AssertEquals("Open", ticket.StatusDescription);

			ticket.WKR_Status = TicketStatusList.Codes.Working;
			AssertEquals("In progress", ticket.StatusDescription);

			ticket.WKR_Status = TicketStatusList.Codes.Closed;
			AssertEquals("Closed as completed", ticket.StatusDescription);

			ticket.WKR_Status = TicketStatusList.Codes.Cancelled;
			AssertEquals("Closed as canceled", ticket.StatusDescription);
		}

		public void TestRelatedItems_ShouldBeIncludedInBusinessObjectsWithRelatedEventsProperty()
		{
			var relatedOpenWorkItem = Factory.NewWithValidTestData<WorkItem>();
			relatedOpenWorkItem.WKI_Summary = "Help me my PW doesn't open";

			var relatedClosedWorkItem = Factory.NewWithValidTestData<WorkItem>();
			relatedClosedWorkItem.WKI_Summary = "Ants-In-My-Eyes Johnson";

			var unrelatedWorkItem = Factory.NewWithValidTestData<WorkItem>();
			unrelatedWorkItem.WKI_Summary = "Help me my CW1 doesn't open";

			MasterFilesTestHelper.CreateTask(relatedOpenWorkItem);
			MasterFilesTestHelper.CreateTask(unrelatedWorkItem);

			Factory.Save();

			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, relatedOpenWorkItem.WKI_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, relatedClosedWorkItem.WKI_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, unrelatedWorkItem.WKI_Status);

			var workRequest = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest, relatedOpenWorkItem);
			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest, relatedClosedWorkItem);

			AssertContainsExactElementsInAnyOrder("We should have all related WIs, and no unrelated WIs, within our BusinessObjectWithRelatedEvents property, but instead...", new BusinessObject[] { relatedOpenWorkItem, relatedClosedWorkItem }, workRequest.BusinessObjectsWithRelatedEvents);
		}

		public void TestLogsFilterQuery_ShouldIncludeRelatedItemEvents()
		{
			var relatedOpenWorkItem = Factory.NewWithValidTestData<WorkItem>();
			relatedOpenWorkItem.WKI_Summary = "Help me my PW doesn't open";

			var relatedClosedWorkItem = Factory.NewWithValidTestData<WorkItem>();
			relatedClosedWorkItem.WKI_Summary = "Ants-In-My-Eyes Johnson";

			var unrelatedWorkItem = Factory.NewWithValidTestData<WorkItem>();
			unrelatedWorkItem.WKI_Summary = "Help me my CW1 doesn't open";

			MasterFilesTestHelper.CreateTask(relatedOpenWorkItem);
			MasterFilesTestHelper.CreateTask(unrelatedWorkItem);

			Factory.Save();

			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, relatedOpenWorkItem.WKI_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, relatedClosedWorkItem.WKI_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, unrelatedWorkItem.WKI_Status);

			var workRequest = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest, relatedOpenWorkItem);
			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest, relatedClosedWorkItem);

			var openWorkItemLogs = relatedOpenWorkItem.Logs.Find(log => !log.IsNull).ToArray();
			var closedWorkItemLogs = relatedClosedWorkItem.Logs.Find(log => !log.IsNull).ToArray();
			var workRequestLogs = workRequest.Logs.Find(log => !log.IsNull).ToArray();

			var expectedLogs = workRequestLogs.Concat(openWorkItemLogs).Concat(closedWorkItemLogs);

			var filterController = new ZStmALogFilterBusinessObject(workRequest);
			filterController.LogsToShow = LogsToShow.All;

			var filter = filterController[ZStmALogFilterBusinessObject.Schema.ShowFor];
			var workRequestFilteredLogs = Factory.Load<StmALog>(filter.Query);

			AssertContainsExactElementsInAnyOrder("We should have exactly these logs in our queried logs filter collection, but instead... we didn't!", expectedLogs, workRequestFilteredLogs);
		}

		public void TestRelatedEventsAreSupportedTypesOnly()
		{
			var workRequest = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			// add types to exclude here!
			var excludedTypes = Array.Empty<Type>();
			var includedTypes = workRequest.SupportedRelatedItemModules.Where(item => !excludedTypes.Contains(item.GetType())).ToArray();

			foreach (var supportedItem in includedTypes)
			{
				var relatedItem = Factory.New(supportedItem.BusinessObjectType);
				relatedItem.FillWithValidTestData();

				AssertNoExceptionThrown(() => workRequest.RelatedItems.Add(relatedItem));
				AssertNoExceptionThrown(Factory.Save);
				AssertCollectionContains($@"Work requests should only contain supported BusinessObjectsWithRelatedEvents types, but {relatedItem.GetType().ToString()} is not supported!
Please either include this bizo type in the WorkRequest.SupportedTypesForRelatedEvents list, or exclude it in this test.
Note: Don't just include your new type in the SupportedTypesForRelatedEvents list, as it can have deep repercussions!",
					relatedItem, workRequest.BusinessObjectsWithRelatedEvents);

				AssertNoExceptionThrown(() => workRequest.RelatedItems.Remove(relatedItem));
				AssertNoExceptionThrown(Factory.Save);
			}

			Assert("There should always be some number of related item modules!", includedTypes.Length != 0);
		}

		public void TestRelatedEventsIncludeWorkItemInheritors()
		{
			var workItemInheritor = Factory.NewWithValidTestData<DummyWorkItem>();
			workItemInheritor.WKI_Summary = "Help me my EDIprod doesn't open";

			Factory.Save();

			var workRequest = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			ProcessMgmtTestHelper.CreateWorkItemRequestLink(workRequest, workItemInheritor);

			AssertNotEquals("Our dummy WI is not a direct WorkItem", typeof(WorkItem), workItemInheritor.GetType());
			AssertCollectionContains("We should have our WorkItem-inheriting dummy class included in the list of related bizos for a WorkRequest, but instead...", workItemInheritor, workRequest.BusinessObjectsWithRelatedEvents);
		}

		#endregion

		#region Custom Fields

		[TestedType(typeof(WorkRequest))]
		class CustomFieldsTest : TestICustomFieldProvider
		{
		}

		#endregion

		#region eConversation

		public void TestConversation()
		{
			var request = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			AssertNull("You have to save the parent before you can create a conversation for it", request.Conversation);
			Factory.Save();

			var conversation = request.Conversation;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedRequest = newFactory.Load<WorkRequest>(request.PK);
			var loadedConversation = loadedRequest.Conversation;

			AssertEquals("A new conversation should not be created for the same work request. SAD!", conversation.PK, loadedConversation.PK);
		}

		public void TestAddConversationMessageAndSave_ShouldSendNotificationEmailsToParticipants()
		{
			var request = ProcessMgmtTestHelper.CreateWorkRequest(Factory, clientEmail: "cows@tv.com");
			var resource1 = ProcessMgmtTestHelper.CreateStaff(Factory, emailAddress: "horses@tv.com");
			var resource2 = ProcessMgmtTestHelper.CreateStaff(Factory, emailAddress: "cats@tv.com");

			Factory.Save();

			request.Conversation.Participants.AddNewParticipant(resource1);
			request.Conversation.Participants.AddNewParticipant(resource2);

			Factory.Save();

			request.Conversation.AddMessageFromCurrentUser("Cows don't look like cows on TV. You gotta use horses.", isInternal: false);

			AssertEquals(0, Env.AllEmailsCreated.Count());

			Factory.Save();

			var emails = Env.AllEmailsCreated.ToArray();

			AssertContainsExactElementsInAnyOrder(new[] { "cows@tv.com", "horses@tv.com", "cats@tv.com" }, emails.SelectMany(email => email.Recipients.Cast<RecipientDef>()).Select(r => r.Email));
			AssertEquals(3, Env.AllEmailsCreated.Count());

			const string bodyWithHtmlEscapedChars = "Cows don&#39;t look like cows on TV. You gotta use horses.";

			AssertContains(bodyWithHtmlEscapedChars, emails[0].Body);
			AssertContains(bodyWithHtmlEscapedChars, emails[1].Body);
			AssertContains(bodyWithHtmlEscapedChars, emails[2].Body);
		}

		public void TestAddConversationMessage_ShouldRegisterClientAsParticipant()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory, clientEmail: "CW@hunrath.com");
			Factory.Save();

			ticket.Conversation.AddMessageFromCurrentUser("Them Kaptar Critters", isInternal: false);
			Factory.Save();

			AssertEquals(1, Env.AllEmailsCreated.Count());
			AssertContains("Them Kaptar Critters", Env.AllEmailsCreated.Single().Body);
			AssertEquals("CW@hunrath.com", Env.AllEmailsCreated.Single().Recipients.Cast<RecipientDef>().Single().Email);

			Env.ClearAllEmailsCreated();

			var otherContact = ticket.Client.ParentOrg.Contacts.AddNew();
			otherContact.FillWithValidTestData();
			otherContact.OC_Email = "farley@hunrath.com";

			ticket.WKR_OC_Client = otherContact.PK;

			ticket.Conversation.AddMessageFromCurrentUser("We all lost everything", isInternal: false);
			Factory.Save();

			AssertEquals(1, Env.AllEmailsCreated.Count());
			AssertContains("We all lost everything", Env.AllEmailsCreated.Single().Body);
			AssertEquals("farley@hunrath.com", Env.AllEmailsCreated.Single().Recipients.Cast<RecipientDef>().Single().Email);

			Env.ClearAllEmailsCreated();

			ticket.WKR_OC_Client = ZGuid.Empty;

			ticket.Conversation.AddMessageFromCurrentUser("Shorah", isInternal: false);
			AssertExceptionThrown<ZSaveException>(Factory.Save);
			AssertEquals(0, Env.AllEmailsCreated.Count());
		}

		public void TestAddExternalConversationMessage_WhenNoParticipants_ShouldUseRegistryFallback()
		{
			MasterFilesTestHelper.ClearWorkflowTables();

			var registryNotificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			var registryNotificationGroupStaff = ProcessMgmtTestHelper.CreateStaff(Factory, "RegistryNotificationGroupStaff@johnson.com", groups: new[] { registryNotificationGroup });

			Factory.Save();
			ProcessManagementRegistry.Instance.CustomerServiceTicketNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryNotificationGroup.PK.ToGuid());

			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory, clientEmail: "Client@Gazorpazorp.com");

			Factory.Save();

			ticket.Conversation.AddMessageFromCurrentUser("I hope our prices aren't too low!", isInternal: false);

			Factory.Save();
			Env.ClearAllEmailsCreated();

			ProcessMgmtTestHelper.AddExternalEConversationMessageFromClient(ticket, "Mmmm enchiladas!");
			ProcessMgmtTestHelper.AddExternalEConversationMessageFromClient(ticket, "Another message from client!");
			AssertEquals(0, Env.AllEmailsCreated.Count());

			MasterFilesTestHelper.RunLogWalker();

			CombineAssertions("WHEN a client sends two messages, THEN the messages should generate two emails (one for each recipient).", () =>
			{
				AssertContainsExactElementsInAnyOrder("For now we are also sending a notification to the client when they write their own messages. This will likely change. The important thing here is we DO fall back to the notification group because we have no participants.",
					new[] { "RegistryNotificationGroupStaff@johnson.com", "Client@Gazorpazorp.com" }, Env.AllEmailsCreated.SelectMany(email => email.Recipients.Cast<RecipientDef>()).Select(r => r.Email));
				AssertEquals("Emails created.", 2, Env.AllEmailsCreated.Count());
			});
		}

		public void TestAddExternalConversationMessage_WhenParticipantExists_ShouldNotUseRegistryFallback()
		{
			MasterFilesTestHelper.ClearWorkflowTables();

			var registryNotificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			var registryNotificationGroupStaff = ProcessMgmtTestHelper.CreateStaff(Factory, "RegistryNotificationGroupStaff@johnson.com", groups: new[] { registryNotificationGroup });
			var participantStaff = ProcessMgmtTestHelper.CreateStaff(Factory, "Staff@Gazorpazorp.com");

			ProcessManagementRegistry.Instance.CustomerServiceTicketNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryNotificationGroup.PK.ToGuid());

			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory, clientEmail: "Client@Gazorpian.com");

			Factory.Save();

			ticket.Conversation.Participants.AddNewParticipant(participantStaff);
			ticket.Conversation.AddMessageFromCurrentUser("I hope our prices aren't too low!", isInternal: false);

			Factory.Save();
			Env.ClearAllEmailsCreated();

			ProcessMgmtTestHelper.AddExternalEConversationMessageFromClient(ticket, "Mmmm enchiladas!");
			ProcessMgmtTestHelper.AddExternalEConversationMessageFromClient(ticket, "Another message from client!");
			AssertEquals(0, Env.AllEmailsCreated.Count());

			MasterFilesTestHelper.RunLogWalker();

			CombineAssertions("WHEN a client sends two messages, THEN the messages should generate two emails (one for each recipient).", () =>
			{
				AssertContainsExactElementsInAnyOrder("For now we are also sending a notification to the client when they write their own messages. This will likely change. The important thing here is we DON'T fall back to the notification group because we have a participant.",
					new[] { "Staff@Gazorpazorp.com", "Client@Gazorpian.com" }, Env.AllEmailsCreated.SelectMany(email => email.Recipients.Cast<RecipientDef>()).Select(r => r.Email));
				AssertEquals("Emails created.", 2, Env.AllEmailsCreated.Count());
			});
		}

		public void TestAddConversationMessageFromStaff_WhenNoSubscribedStaff_ShouldOnlySendEmailToClientAndNotUseRegistryFallback()
		{
			AssertAddConversationMessageFromStaff_WhenNoSubscribedParticipants_ShouldNotUseRegistryFallback(isInternal: false);
		}

		public void TestAddInternalConversationMessageFromStaff_WhenNoSubscribedStaff_ShouldNotSendEmailToAnyoneAndNotUseRegistryFallback()
		{
			AssertAddConversationMessageFromStaff_WhenNoSubscribedParticipants_ShouldNotUseRegistryFallback(isInternal: true);
		}

		void AssertAddConversationMessageFromStaff_WhenNoSubscribedParticipants_ShouldNotUseRegistryFallback(bool isInternal)
		{
			MasterFilesTestHelper.ClearWorkflowTables();

			var registryNotificationGroup = Factory.NewWithValidTestData<GlbGroup>();
			var registryNotificationGroupStaff = ProcessMgmtTestHelper.CreateStaff(Factory, "RegistryNotificationGroupStaff@ryan.com", groups: new[] { registryNotificationGroup });
			var participantStaff = ProcessMgmtTestHelper.CreateStaff(Factory, "Staff@ryan.com");

			ProcessManagementRegistry.Instance.CustomerServiceTicketNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryNotificationGroup.PK.ToGuid());

			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory, clientEmail: "Client@ryan.com");

			Factory.Save();

			var jobConversationParticipant = ticket.Conversation.Participants.AddNewParticipant(participantStaff);
			jobConversationParticipant.JCP_IsSubscribed = false;
			ticket.Conversation.AddMessageFromCurrentUser("Haskell is a cool programming language!", isInternal);
			ticket.Conversation.AddMessageFromCurrentUser("But it doesn't have arrays.", isInternal);

			Factory.Save();

			CombineAssertions("WHEN we add two messages from staff...", () =>
			{
				if (!isInternal)
				{
					AssertContainsExactElementsInAnyOrder("We should only send the message to the client.",
					new[] { "Client@ryan.com" }, Env.AllEmailsCreated.SelectMany(email => email.Recipients.Cast<RecipientDef>()).Select(r => r.Email));
				}
				else
				{
					AssertEquals("We should not send any emails when it is an internal message.", 0, Env.AllEmailsCreated.Count());
				}
			});
		}

		public void TestConversationMessageNotification_ShouldUseExternalLinksForContacts_AndInternalLinksForStaff()
		{
			var staff = ProcessMgmtTestHelper.CreateStaff(Factory, "sleep@data.com");
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);

			Factory.Save();

			ticket.Conversation.Participants.AddNewParticipant(staff);
			ticket.Conversation.AddMessageFromCurrentUser("Sleep... Data.", isInternal: false);

			Factory.Save();

			var emails = Env.AllEmailsCreated.ToArray();

			AssertEquals(2, emails.Length);

			var emailToStaff = emails.Single(e => e.Recipients.Cast<RecipientDef>().Any(r => r.Email == "sleep@data.com"));
			var emailToContact = emails.Single(e => e.Recipients.Cast<RecipientDef>().Any(r => r.Email == ticket.Client.OC_Email));

			var expectedInternalUrl = FormattableString.Invariant($"edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=CustomerServiceTicket&BusinessEntityPK={ticket.PK}");
			var expectedExternalUrl = FormattableString.Invariant($"{GlowRegistry.Instance.GlowPortalsUri.Value}TKT/Desktop#/formFlow/c02c315d-c12a-4abb-aec7-e8d3b57a2dba/{ticket.PK}");

			AssertContains(expectedInternalUrl, emailToStaff.Body);
			AssertContains(expectedExternalUrl, emailToContact.Body);

			AssertNotContains(expectedExternalUrl, emailToStaff.Body);
			AssertNotContains(expectedInternalUrl, emailToContact.Body);
		}

		public void TestConversationMessageNotification_WhenGlowPortalsUriDoesNotEndWithSlash_ShouldStillCreateValidUri()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://interslice.com/Portals");

			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);

			Factory.Save();

			ticket.Conversation.AddMessageFromCurrentUser("Sleep... Data.", isInternal: false);
			Factory.Save();

			var email = Env.AllEmailsCreated.Single();
			var expectedExternalUrl = "https://interslice.com/Portals/TKT/Desktop#/formFlow/c02c315d-c12a-4abb-aec7-e8d3b57a2dba/" + ticket.PK;
			AssertContains("The missing slash from the registry item should be added so that this uri will work. SAD!", expectedExternalUrl, email.Body);
		}

		public void TestShouldUseThisProviderForHyperlink_ShouldBeTrueForNullParticipant()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);

			AssertEquals(true, ((IConversationParentHyperlinkProvider)ticket).ShouldUseThisProviderForHyperlink(null));
		}

		#endregion

		#region JobInvoicing

		public void TestInvoicingSupporter()
		{
			var request = Factory.New<WorkRequest>();
			AssertType(typeof(WorkRequestInvoicingSupporter), request.InvoicingSupporter);
		}

		#endregion

		#region Status Calculation

		public void TestStatusCalculation_ShouldUpdateWhenSavingTicket()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);

			AssertEquals(TicketStatusList.Codes.Open, ticket.WKR_Status);

			Factory.Save();
			AssertEquals(TicketStatusList.Codes.Closed, ticket.WKR_Status);
		}

		public void TestStatusCalculation_ShouldConsiderTaskStatus()
		{
			CreateTicketAndEnsureTicketStatusChangesToWorking(ProcessTaskStatusCodeList.Codes.Working);
			CreateTicketAndEnsureTicketStatusChangesToWorking(ProcessTaskStatusCodeList.Codes.Suspended);
			CreateTicketAndEnsureTicketStatusChangesToWorking(ProcessTaskStatusCodeList.Codes.Closed);

			CreateTicketAndEnsureTicketStatusChangesToWorking(ProcessTaskStatusCodeList.Codes.Open, shouldChangeToWorking: false);
			CreateTicketAndEnsureTicketStatusChangesToWorking(ProcessTaskStatusCodeList.Codes.Assigned, shouldChangeToWorking: false);
			CreateTicketAndEnsureTicketStatusChangesToWorking(ProcessTaskStatusCodeList.Codes.Cancelled, shouldChangeToWorking: false);
		}

		void CreateTicketAndEnsureTicketStatusChangesToWorking(string taskStatus, bool shouldChangeToWorking = true)
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var task1 = MasterFilesTestHelper.CreateTask(ticket);
			var task2 = MasterFilesTestHelper.CreateTask(ticket);

			AssertEquals(TicketStatusList.Codes.Open, ticket.WKR_Status);

			task1.P9_Status = taskStatus;
			Factory.Save();

			if (shouldChangeToWorking)
			{
				AssertEquals(TicketStatusList.Codes.Working, ticket.WKR_Status);
			}
			else
			{
				AssertEquals(TicketStatusList.Codes.Open, ticket.WKR_Status);
			}

			MasterFilesTestHelper.SetAllTasksStatus(ticket, ProcessTaskStatusCodeList.Codes.Closed);
			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Closed, ticket.WKR_Status);
		}

		public void TestStatusCalculation_ShouldConsiderAttachedWorkItems()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var workItem_complete = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			var workItem_incomplete = ProcessMgmtTestHelper.CreateWorkItem(Factory);

			MasterFilesTestHelper.CreateTask(workItem_incomplete, GlbStaff.CurrentUser.GS_Code);

			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Closed, ticket.WKR_Status);

			ticket.RelatedItems.Add(workItem_complete);
			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Closed, ticket.WKR_Status);

			ticket.RelatedItems.Add(workItem_incomplete);
			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Working, ticket.WKR_Status);

			MasterFilesTestHelper.SetAllTasksStatus(workItem_incomplete, ProcessTaskStatusCodeList.Codes.Closed);
			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Closed, ticket.WKR_Status);

			MasterFilesTestHelper.SetAllTasksStatus(workItem_incomplete, ProcessTaskStatusCodeList.Codes.Suspended);
			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Working, ticket.WKR_Status);

			ticket.RelatedItems.Remove(workItem_incomplete);
			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Closed, ticket.WKR_Status);
		}

		public void TestStatusCalculation_ShouldConsiderAttachedWorkItems_AndTaskStatus_WhenClosingWorkItemTasksFirst()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			var workItemTask = MasterFilesTestHelper.CreateTask(workItem);

			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Closed, ticket.WKR_Status);

			var task1 = MasterFilesTestHelper.CreateTask(ticket);
			var task2 = MasterFilesTestHelper.CreateTask(ticket);

			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Open, ticket.WKR_Status);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Working, ticket.WKR_Status);

			ticket.RelatedItems.Add(workItem);
			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Working, ticket.WKR_Status);

			MasterFilesTestHelper.SetAllTasksStatus(workItem, ProcessTaskStatusCodeList.Codes.Closed);
			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Working, ticket.WKR_Status);

			MasterFilesTestHelper.SetAllTasksStatus(ticket, ProcessTaskStatusCodeList.Codes.Closed);
			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Closed, ticket.WKR_Status);
		}

		public void TestStatusCalculation_ShouldConsiderAttachedWorkItems_AndTaskStatus_WhenClosingTicketTasksFirst()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			var workItemTask = MasterFilesTestHelper.CreateTask(workItem);

			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Closed, ticket.WKR_Status);

			ticket.RelatedItems.Add(workItem);
			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Working, ticket.WKR_Status);

			var task1 = MasterFilesTestHelper.CreateTask(ticket);
			var task2 = MasterFilesTestHelper.CreateTask(ticket);

			AssertEquals(TicketStatusList.Codes.Working, ticket.WKR_Status);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Working, ticket.WKR_Status);

			MasterFilesTestHelper.SetAllTasksStatus(ticket, ProcessTaskStatusCodeList.Codes.Closed);
			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Working, ticket.WKR_Status);

			MasterFilesTestHelper.SetAllTasksStatus(workItem, ProcessTaskStatusCodeList.Codes.Closed);
			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Closed, ticket.WKR_Status);
		}

		public void TestStatusCalculation_WhenSetToCancelled_DontChangeItUnderAnyCircumstance()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);

			ticket.WKR_Status = TicketStatusList.Codes.Cancelled;

			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Cancelled, ticket.WKR_Status);

			MasterFilesTestHelper.CreateTask(ticket, status: ProcessTaskStatusCodeList.Codes.Working);
			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Cancelled, ticket.WKR_Status);

			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			MasterFilesTestHelper.CreateTask(workItem, status: ProcessTaskStatusCodeList.Codes.Suspended);

			ticket.RelatedItems.Add(workItem);
			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Cancelled, ticket.WKR_Status);
		}

		public void TestStatusCalculation_WhenTicketTaskDeleted()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var task1 = MasterFilesTestHelper.CreateTask(ticket);
			var task2 = MasterFilesTestHelper.CreateTask(ticket);

			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Open, ticket.WKR_Status);

			task1.Delete();
			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Open, ticket.WKR_Status);

			task2.Delete();
			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Closed, ticket.WKR_Status);
		}

		public void TestStatusCalculation_WhenWorkItemTaskDeleted()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);

			ticket.RelatedItems.Add(workItem);

			var task1 = MasterFilesTestHelper.CreateTask(workItem);
			var task2 = MasterFilesTestHelper.CreateTask(workItem);

			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Working, ticket.WKR_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, workItem.WKI_Status);

			task1.Delete();
			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Working, ticket.WKR_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, workItem.WKI_Status);

			task2.Delete();
			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Closed, ticket.WKR_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, workItem.WKI_Status);
		}

		#endregion

		#region Cancel

		public void TestCancel_NotCancellingAttachedWorkItems_ShouldCancelTicketTasksOnly()
		{
			CreateAndCancelTicket_AssertingExpectedStatuses(shouldCancelAttachedWorkItems: false);
		}

		public void TestCancel_AlsoCancellingAttachedWorkItems_ShouldCancelTicketTasksAndWorkItemTasks()
		{
			CreateAndCancelTicket_AssertingExpectedStatuses(shouldCancelAttachedWorkItems: true);
		}

		void CreateAndCancelTicket_AssertingExpectedStatuses(bool shouldCancelAttachedWorkItems)
		{
			var staff1 = ProcessMgmtTestHelper.CreateStaff(Factory);
			var staff2 = ProcessMgmtTestHelper.CreateStaff(Factory);

			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var workItem1 = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			var workItem2 = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			var workItem3 = ProcessMgmtTestHelper.CreateWorkItem(Factory);

			ticket.RelatedItems.Add(workItem1);
			ticket.RelatedItems.Add(workItem2);

			var ticketTask_working = MasterFilesTestHelper.CreateTask(ticket, staff1.GS_Code, status: ProcessTaskStatusCodeList.Codes.Working);
			var ticketTask_suspended = MasterFilesTestHelper.CreateTask(ticket, status: ProcessTaskStatusCodeList.Codes.Suspended);
			var ticketTask_closed = MasterFilesTestHelper.CreateTask(ticket, status: ProcessTaskStatusCodeList.Codes.Closed);
			var ticketTask_assigned = MasterFilesTestHelper.CreateTask(ticket, status: ProcessTaskStatusCodeList.Codes.Assigned);
			var ticketTask_open = MasterFilesTestHelper.CreateTask(ticket, status: ProcessTaskStatusCodeList.Codes.Open);
			var ticketTask_cancelled = MasterFilesTestHelper.CreateTask(ticket, status: ProcessTaskStatusCodeList.Codes.Cancelled);

			var workItem1Task_working = MasterFilesTestHelper.CreateTask(workItem1, staff2.GS_Code, status: ProcessTaskStatusCodeList.Codes.Working);
			var workItem1Task_suspended = MasterFilesTestHelper.CreateTask(workItem1, status: ProcessTaskStatusCodeList.Codes.Suspended);
			var workItem1Task_closed = MasterFilesTestHelper.CreateTask(workItem1, status: ProcessTaskStatusCodeList.Codes.Closed);
			var workItem1Task_assigned = MasterFilesTestHelper.CreateTask(workItem1, status: ProcessTaskStatusCodeList.Codes.Assigned);
			var workItem1Task_open = MasterFilesTestHelper.CreateTask(workItem1, status: ProcessTaskStatusCodeList.Codes.Open);
			var workItem1Task_cancelled = MasterFilesTestHelper.CreateTask(workItem1, status: ProcessTaskStatusCodeList.Codes.Cancelled);

			var workItem2Task = MasterFilesTestHelper.CreateTask(workItem2);
			var workItem3Task = MasterFilesTestHelper.CreateTask(workItem3);

			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Working, ticket.WKR_Status);

			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, ticketTask_working.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, ticketTask_suspended.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, ticketTask_closed.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, ticketTask_assigned.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, ticketTask_open.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, ticketTask_cancelled.P9_Status);

			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, workItem1Task_working.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, workItem1Task_suspended.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, workItem1Task_closed.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, workItem1Task_assigned.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, workItem1Task_open.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, workItem1Task_cancelled.P9_Status);

			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, workItem2Task.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, workItem3Task.P9_Status);

			ticket.Cancel(shouldCancelAttachedWorkItems);

			AssertEquals(TicketStatusList.Codes.Cancelled, ticket.WKR_Status);

			AssertEquals("Task that started out with WRK status should be closed instead of cancelled since there was some time recording to track.", ProcessTaskStatusCodeList.Codes.Closed, ticketTask_working.P9_Status);
			AssertEquals("Task that started out with SUS status should be closed instead of cancelled since there was some time recording to track.", ProcessTaskStatusCodeList.Codes.Closed, ticketTask_suspended.P9_Status);
			AssertEquals("Already closed task shouldn't be affected", ProcessTaskStatusCodeList.Codes.Closed, ticketTask_closed.P9_Status);
			AssertEquals("Un-started task should be cancelled", ProcessTaskStatusCodeList.Codes.Cancelled, ticketTask_assigned.P9_Status);
			AssertEquals("Un-started task should be cancelled", ProcessTaskStatusCodeList.Codes.Cancelled, ticketTask_open.P9_Status);
			AssertEquals("Already cancelled task should still be cancelled", ProcessTaskStatusCodeList.Codes.Cancelled, ticketTask_cancelled.P9_Status);

			if (shouldCancelAttachedWorkItems)
			{
				AssertEquals("Work Item task that started out with WRK status should be closed instead of cancelled since there was some time recording to track.", ProcessTaskStatusCodeList.Codes.Closed, workItem1Task_working.P9_Status);
				AssertEquals("Work Item task that started out with SUS status should be closed instead of cancelled since there was some time recording to track.", ProcessTaskStatusCodeList.Codes.Closed, workItem1Task_suspended.P9_Status);
				AssertEquals("Already closed task shouldn't be affected", ProcessTaskStatusCodeList.Codes.Closed, workItem1Task_closed.P9_Status);
				AssertEquals("Un-started task should be cancelled", ProcessTaskStatusCodeList.Codes.Cancelled, workItem1Task_assigned.P9_Status);
				AssertEquals("Un-started task should be cancelled", ProcessTaskStatusCodeList.Codes.Cancelled, workItem1Task_open.P9_Status);
				AssertEquals("Already cancelled task should still be cancelled", ProcessTaskStatusCodeList.Codes.Cancelled, workItem1Task_cancelled.P9_Status);

				AssertEquals("Un-started task in another attached WI should also be cancelled", ProcessTaskStatusCodeList.Codes.Cancelled, workItem2Task.P9_Status);
				AssertEquals("Work item #3 isn't attached to the ticket so its task status should be unaffected", ProcessTaskStatusCodeList.Codes.Assigned, workItem3Task.P9_Status);
			}
			else
			{
				AssertEquals(ProcessTaskStatusCodeList.Codes.Working, workItem1Task_working.P9_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, workItem1Task_suspended.P9_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, workItem1Task_closed.P9_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, workItem1Task_assigned.P9_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Open, workItem1Task_open.P9_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, workItem1Task_cancelled.P9_Status);

				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, workItem2Task.P9_Status);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, workItem3Task.P9_Status);
			}

			Factory.Save();
			AssertEquals("The ticket has been cancelled, so should remain that way after saving.", TicketStatusList.Codes.Cancelled, ticket.WKR_Status);
		}

		public void TestUnCancel_ShouldJustSetStatus()
		{
			var staff1 = ProcessMgmtTestHelper.CreateStaff(Factory);
			var staff2 = ProcessMgmtTestHelper.CreateStaff(Factory);

			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var ticketTask = MasterFilesTestHelper.CreateTask(ticket, staff1.GS_Code, status: ProcessTaskStatusCodeList.Codes.Working);
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			var workItemTask = MasterFilesTestHelper.CreateTask(workItem, staff2.GS_Code, status: ProcessTaskStatusCodeList.Codes.Working);

			ticket.RelatedItems.Add(workItem);

			Factory.Save();

			AssertEquals(TicketStatusList.Codes.Working, ticket.WKR_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, ticketTask.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, workItem.WKI_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, workItemTask.P9_Status);

			ticket.Cancel(shouldCancelAttachedWorkItems: true);

			AssertEquals(TicketStatusList.Codes.Cancelled, ticket.WKR_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, ticketTask.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, workItem.WKI_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, workItemTask.P9_Status);

			ticket.UnCancel();

			AssertEquals(TicketStatusList.Codes.Closed, ticket.WKR_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, ticketTask.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, workItem.WKI_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, workItemTask.P9_Status);
		}

		public void TestCancelledEvent_AsRaisedThroughGlowPortal_EventDetailsShouldBeHumanReadable()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);

			AssertEventDetails("Ticket Canceled by Davey Baby", Tuple.Create("NAM", "Davey Baby"), Tuple.Create("TYP", "Ticket"));
			AssertEventDetails("Ticket Canceled", Tuple.Create("NAM", ""), Tuple.Create("TYP", "Ticket"));
			AssertEventDetails("Ticket Canceled", Tuple.Create("TYP", "Ticket"));

			AssertEventDetails("Canceled by Davey Baby", Tuple.Create("NAM", "Davey Baby"), Tuple.Create("TYP", ""));
			AssertEventDetails("Canceled by Davey Baby", Tuple.Create("NAM", "Davey Baby"));

			AssertEventDetails("Canceled", Tuple.Create("NAM", ""), Tuple.Create("TYP", ""));

			void AssertEventDetails(string expectedEventDetails, params Tuple<string, string>[] referenceParameters)
			{
				var @event = ticket.GetLogs().AddNew(AutoEvents.Cancelled, referenceParameters.Select(tuple => new KeyValuePair<string, string>(tuple.Item1, tuple.Item2)).ToArray());

				AssertEquals(expectedEventDetails, @event.DisplayEventReference);
			}
		}

		public void TestCancel_ShouldIgnoreTaskCancellationValidationAndCancelAllTasks()
		{
			var staff1 = ProcessMgmtTestHelper.CreateStaff(Factory);
			var staff2 = ProcessMgmtTestHelper.CreateStaff(Factory);

			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(Factory);
			var ticketTask = MasterFilesTestHelper.CreateTask(ticket, staff1.GS_Code, status: ProcessTaskStatusCodeList.Codes.Assigned);
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(Factory);
			var workItemTask = MasterFilesTestHelper.CreateTask(workItem, staff2.GS_Code, status: ProcessTaskStatusCodeList.Codes.Assigned);

			ticket.RelatedItems.Add(workItem);

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var workItemTaskTypes = categorisedTaskTypes.AddNew();
			workItemTaskTypes.Code = "WKI";
			var wkiTaskType = workItemTaskTypes.TaskTypes.AddNew();
			wkiTaskType.Code = "UDF";
			wkiTaskType.CanCancelTask = false;
			var ticketTaskTypes = categorisedTaskTypes.AddNew();
			ticketTaskTypes.Code = "CST";
			var wkrTaskType = ticketTaskTypes.TaskTypes.AddNew();
			wkrTaskType.Code = "UDF";
			wkrTaskType.CanCancelTask = false;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);
			Env.Security.WorkflowTasksCanCancelAllTasks.IsAllowed = false;

			Factory.Save();

			ticket.Cancel(shouldCancelAttachedWorkItems: true);
			ticketTask.Validation.ValidateP9_Status();

			AssertNoErrors("Validation on task status should be disabled when cancelling a customer service ticket", ticketTask.P9_StatusInfo);
			AssertNoErrors("Validation on task status should be disabled when cancelling the linked work item", workItemTask.P9_StatusInfo);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return ProcessMgmtTestHelper.CreateWorkRequest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return ProcessMgmtTestHelper.CreateWorkRequest(factory);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return ProcessMgmtTestHelper.CreateWorkRequest(Factory);
		}

		public class DummyWorkItem : WorkItem
		{
			public DummyWorkItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}
		}

		#endregion
	}
}
