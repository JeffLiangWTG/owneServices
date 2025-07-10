using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	public static class ProcessMgmtTestHelper
	{
		public static void EnableProductivityWise()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
		}

		public static void EnableBufferManagement()
		{
			ObjectFactory.Get<IBMTestHelper>().EnableBMSInRegistry();
		}

		public static void SetDummySelectionCriteriaValues()
		{
			SetSelectionCritieriaRegistryValues(1, Tuple.Create("ENT", "CargoWise Two"), Tuple.Create("GLW", "GLOW"));
			SetSelectionCritieriaRegistryValues(2, Tuple.Create("PAV", "Productivity Acceleration and Visualisation Engine"), Tuple.Create("PER", "Performance and Deployment"));
			SetSelectionCritieriaRegistryValues(3, Tuple.Create("BUF", "Buffer Management"), Tuple.Create("APP", "Application Deployment"));
			SetSelectionCritieriaRegistryValues(4, Tuple.Create("PRD", "Product Enhancement"), Tuple.Create("FIX", "Defect Fix"));
			SetSelectionCritieriaRegistryValues(5, Tuple.Create("ALP", "Alpha"), Tuple.Create("GPR", "GPR or is GP1?? There's no way of knowing."));
		}

		public static void SetMrsSullivanRelatedSelectionCriteriaValues()
		{
			SetSelectionCritieriaRegistryValues(1, Tuple.Create("AAA", "Mrs Sullivan always planned to leave everything to her cats."));
			SetSelectionCritieriaRegistryValues(2, Tuple.Create("BBB", "But sometimes, plans need a helping paw."));
			SetSelectionCritieriaRegistryValues(3, Tuple.Create("CCC", "What are the kitties to do, but buckle together and work as a team."));
			SetSelectionCritieriaRegistryValues(4, Tuple.Create("DDD", "This fall sparks will fly between one guy who can't get a break..."));
			SetSelectionCritieriaRegistryValues(5, Tuple.Create("EEE", "And nine cats who break all the rules."));
		}

		public static void SetSelectionCritieriaRegistryValues(int criteriaNumber, params Tuple<string, string>[] codeDescriptions)
		{
			var list = new CodeDescriptionPairList();

			foreach (var tuple in codeDescriptions)
			{
				list.AddPair(tuple.Item1, tuple.Item2);
			}

			switch (criteriaNumber)
			{
				case 1:
					ProcessManagementRegistry.Instance.SelectionCriterion1Values.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
					break;
				case 2:
					ProcessManagementRegistry.Instance.SelectionCriterion2Values.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
					break;
				case 3:
					ProcessManagementRegistry.Instance.SelectionCriterion3Values.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
					break;
				case 4:
					ProcessManagementRegistry.Instance.SelectionCriterion4Values.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
					break;
				case 5:
					ProcessManagementRegistry.Instance.SelectionCriterion5Values.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
					break;

				default:
					throw new ArgumentException("Invalid selection critierion: " + criteriaNumber, nameof(criteriaNumber));
			}
		}

		public static Project CreateProject(BusinessObjectFactory factory, string summary = "Old Women are Coming", string projectType = "AAA", OrgContact client = null)
		{
			var project = factory.NewWithValidTestData<Project>();
			project.WKP_Summary = summary;
			project.WKP_Type = projectType;

			if (client != null)
			{
				project.WKP_OC_Contact = client.PK;
				project.WKP_OA_ClientAddress = project.Contact.Header.MainAddress.PK;
			}

			return project;
		}

		public static WorkItem CreateWorkItem(BusinessObjectFactory factory, string summary = "I hope our prices aren't too low!", string workItemType = null)
		{
			var workItem = factory.NewWithValidTestData<WorkItem>();
			workItem.WKI_Summary = summary;

			if (workItemType != null)
			{
				workItem.WKI_WorkItemType = workItemType;
			}

			return workItem;
		}

		public static WorkRequest CreateWorkRequest(BusinessObjectFactory factory, string summary = "Pliz halp",
			string selectionCriterion1 = null, string selectionCriterion2 = null, string selectionCriterion3 = null, string selectionCriterion4 = null, string selectionCriterion5 = null,
			GlbBranch branch = null, GlbDepartment department = null, string country = null, string clientEmail = "JosefJanssen@hunrath.com")
		{
			var request = factory.NewWithValidTestData<WorkRequest>();

			request.WKR_Summary = summary;
			request.WKR_RequestNumber = ZString.Empty; // Make it empty so it's populated on saving, like in production.

			request.WKR_SelectionCriteria1 = selectionCriterion1;
			request.WKR_SelectionCriteria2 = selectionCriterion2;
			request.WKR_SelectionCriteria3 = selectionCriterion3;
			request.WKR_SelectionCriteria4 = selectionCriterion4;
			request.WKR_SelectionCriteria5 = selectionCriterion5;

			if (branch != null)
			{
				request.WKR_GB_Branch = branch.PK;
			}

			if (department != null)
			{
				request.WKR_GE_Department = department.PK;
			}

			request.WKR_RN_NKCountry = country;
			request.Client.OC_Email = clientEmail;

			return request;
		}

		public static WorkItemRequestLink CreateWorkItemRequestLink(BusinessObjectFactory factory)
		{
			var workItem = CreateWorkItem(factory);
			var request = CreateWorkRequest(factory);

			return CreateWorkItemRequestLink(request, workItem);
		}

		public static WorkItemRequestLink CreateWorkItemRequestLink(WorkRequest workRequest, WorkItem workItem)
		{
			workRequest.RelatedItems.Add(workItem);

			var linkQuery = new ZQuery(WorkItemRequestLinkSchema.WKL_WKR_Request, workRequest.PK)
				.AddToFilter(WorkItemRequestLinkSchema.WKL_WKI_WorkItem, workItem.PK);

			return workRequest.Factory.Load<WorkItemRequestLink>(linkQuery).First();
		}

		public static ProcessTaskTemplate CreateWorkRequestWorkflowTemplate(BusinessObjectFactory factory, string name, string taskFallbackMethod = FallbackTypeList.Codes.EmptyFallback,
			string selectionCriterion1 = null, string selectionCriterion2 = null, string selectionCriterion3 = null, string selectionCriterion4 = null, string selectionCriterion5 = null,
			GlbBranch branch = null, GlbDepartment department = null, string country = null)
		{
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(factory, WorkflowDescriptors.CustomerServiceTicketWorkflowDescriptorCode, selectionCriterion1, selectionCriterion2, selectionCriterion3, selectionCriterion4, selectionCriterion5, country, null, department?.PK);
			template.P0_Name = name;
			template.P0_TaskFallbackMethod = taskFallbackMethod;

			if (branch != null)
			{
				template.P0_GB = branch.PK;
			}

			return template;
		}

		public static ProcessTask CreateTemplateTask(ProcessTaskTemplate template, string description)
		{
			var task = template.WorkflowItems.Tasks.AddNew();

			task.P9_Description = description;

			return task;
		}

		public static OrgContact CreateOrganizationAndContact(BusinessObjectFactory factory, string organisationName = "The Organisation", string contactName = "Jan Michael Vincent", string contactEmail = "jan@michael.vincent")
		{
			var org = factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = organisationName;

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = contactName;
			contact.OC_Email = contactEmail;
			contact.OC_OA_OrgAddress = org.MainAddress.PK;

			return contact;
		}

		public static GlbStaff CreateStaff(BusinessObjectFactory factory, string emailAddress = null, GlbGroup[] groups = null)
		{
			var staff = factory.NewWithValidTestData<GlbStaff>();

			if (emailAddress != null)
			{
				staff.GS_EmailAddress = emailAddress;
			}

			if (groups != null)
			{
				staff.Groups.AddRange(groups);
			}

			return staff;
		}

		public static void CreateConversationAndAddRelatedPartyThatWeWouldLikeToEmail(BusinessObject conversationParent, string emailAddress)
		{
			var conversation = JobConversation.CreateWithoutCheckingForExistingConversation(conversationParent, conversationParent.Factory);
			var participant = conversation.RelatedParties.AddNew();
			participant.RelatedPartyTypeName = JobConversationParticipantLookups.EmailConstant;
			participant.EmailAddress = emailAddress;
		}

		public static ZGuid AddExternalEConversationMessageFromClient(WorkRequest ticket, string message)
		{
			var startingNumberOfEmailsSent = Env.AllEmailsCreated.Count();

			if (ticket.Client == null)
			{
				throw new InvalidOperationException("There needs to be a valid client in order for them to send an eConversation message.");
			}

			var newFactory = ticket.Factory.CreateNewFactory();
			var conversation = newFactory.Load<JobConversation>(ticket.Conversation.PK);
			var client = newFactory.Load<OrgContact>(ticket.WKR_OC_Client);

			var conversationMessage = conversation.Messages.AddNew();
			conversationMessage.JCM_Body = message;
			conversationMessage.JCM_IsInternal = false;
			conversationMessage.JCM_IsLocal = false;

			var participant = conversation.Participants.GetOrAdd(client); // TODO: use GetWithoutAdd once client participant is maintained automatically.
			conversationMessage.JCM_JCP_Participant = participant.PK;

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			conversationMessage.GetLogs().AddNew(AutoEvents.AddedARecordToTheSystem); // Simulate how GLOW creates an audit log on creation of a JobConversationMessage.
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			newFactory.Save();

			return conversationMessage.PK;
		}

		public static void SetFallbackSequenceForNonSubscribedCSTicketNotifications(params string[] notificationTypes)
		{
			var newValue = new RecipientSourceFallbackHeader();

			foreach (var type in notificationTypes)
			{
				newValue.SourceCollection.AddNew().SourceType = type;
			}

			ProcessManagementRegistry.Instance.RecipientDeterminationFallbackForNonSubscribedCSTickets.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
		}

		public static void AssertCodeDescriptionPairListContents(ICodeDescriptionPairList list, params Tuple<string, string>[] expectedCodeDescriptionPairs)
		{
			AssertCodeDescriptionPairListContents(string.Empty, list, expectedCodeDescriptionPairs);
		}

		public static void AssertCodeDescriptionPairListContents(string message, ICodeDescriptionPairList list, params Tuple<string, string>[] expectedCodeDescriptionPairs)
		{
			var items = list.Cast<ICodeDescription>().Select(cdp => Tuple.Create(cdp.Code, cdp.Description));

			Assertion.AssertContainsExactElementsInAnyOrder(message, expectedCodeDescriptionPairs, items);
		}

		public static void CreateAndSetRegistryNotificationGroup(BusinessObjectFactory factory)
		{
			var notificationGroup = factory.NewWithValidTestData<GlbGroup>();
			notificationGroup.GG_Code = "~NT";
			var staff = notificationGroup.Staff.AddNew();

			staff.FillWithValidTestData();
			staff.GS_EmailAddress = "dummy@test.com";

			factory.Save();

			ProcessManagementRegistry.Instance.CustomerServiceTicketNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, notificationGroup.PK.ToGuid());
		}

		#region Jira Integration

		public static ProjectsToImportViewModel GetProjectsToImportViewModel(BusinessObjectFactory factory) => new ProjectsToImportViewModel(factory);

		public static IssueTypeMap GetDummyIssueTypeMap()
		{
			var map = new IssueTypeMap();

			map.JiraClassificationMap.Add(new IssueTypeMapItem
			{
				JiraEntityName = "Light Horse Man",
				SelectionCriterionFieldName = WorkItemSchema.Constants.WKI_WorkItemArea,
				SelectionCriterionFieldValue = "HOR",
			});

			map.JiraClassificationMap.Add(new IssueTypeMapItem
			{
				JiraEntityName = "TOM'S DAMN FAN IS DRIVING ME CRAZY",
				SelectionCriterionFieldName = WorkItemSchema.Constants.WKI_WorkItemArea,
				SelectionCriterionFieldValue = "TOM",
			});

			return map;
		}

		public static JiraCustomFieldMap GetDummyJiraCustomFieldMap()
		{
			var map = new JiraCustomFieldMap();

			map.JiraClassificationMap.Add(new JiraCustomFieldMapItem
			{
				JiraEntityName = "Finance",
				SelectionCriterionFieldName = WorkItemSchema.Constants.WKI_WorkItemArea,
				SelectionCriterionFieldValue = "FIN",
				CustomFieldId = "10907",
			});

			map.JiraClassificationMap.Add(new JiraCustomFieldMapItem
			{
				JiraEntityName = "Booking & Operations",
				SelectionCriterionFieldName = WorkItemSchema.Constants.WKI_WorkItemArea,
				SelectionCriterionFieldValue = "BOP",
				CustomFieldId = "10907",
			});

			return map;
		}

		public static ProjectCategoryMap GetDummyProjectCategoryMap()
		{
			var map = new ProjectCategoryMap();

			map.JiraClassificationMap.Add(new ProjectCategoryMapItem
			{
				JiraEntityName = "Jira Integration",
				SelectionCriterionFieldName = WorkProjectSchema.Constants.WKP_Module,
				SelectionCriterionFieldValue = "PAV",
			});

			map.JiraClassificationMap.Add(new ProjectCategoryMapItem
			{
				JiraEntityName = "Crazy deadlines",
				SelectionCriterionFieldName = WorkProjectSchema.Constants.WKP_Priority,
				SelectionCriterionFieldValue = "2WK",
			});

			return map;
		}

		#endregion
	}
}
