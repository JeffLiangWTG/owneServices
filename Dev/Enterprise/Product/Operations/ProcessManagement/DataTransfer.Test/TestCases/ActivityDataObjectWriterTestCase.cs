using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.ProcessManagement.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.DataTransfer.Test
{
	abstract class ActivityDataObjectWriterTestCase<T> : UniversalActivityImplementationTestCase
		where T : BusinessObject, IJobNumber, IWorkTaskRelatedItemSource, IWorkflowProvider
	{
		public void TestPopulateDataObject()
		{
			var businessObject = GetBusinessObject();
			var relatedItems = GetRelatedItems();
			businessObject.RelatedItems.AddRange(relatedItems);
			BizoFactory.Save();

			var activity = ActivityTestHelper.WriteActivity(businessObject);

			AssertMainDetails(activity, businessObject);
			AssertRelatedActivityDescriptions(activity);
		}

		void AssertMainDetails(Activity activity, T businessObject)
		{
			var dataSource = activity.DataContext.DataSourceCollection.SingleOrDefault();
			AssertNotNull("Every UniversalActivity should have a DataSource included, since this is currently the only supported matching technique. SAD!", dataSource);
			AssertEquals(ExpectedDataContextType.ToString(), dataSource.Type);
			AssertEquals(businessObject.JobNumber, dataSource.Key);

			AssertEquals(ExpectedSummary, activity.Summary);
			AssertEquals(ExpectedDescription, activity.Description);
			AssertEquals(ExpectedStatus, activity.Status?.Code);
			AssertEquals(ExpectedSelectionCriterion1, activity.SelectionCriterion1?.Code);
			AssertEquals(ExpectedSelectionCriterion2, activity.SelectionCriterion2?.Code);
			AssertEquals(ExpectedSelectionCriterion3, activity.SelectionCriterion3?.Code);
			AssertEquals(ExpectedSelectionCriterion4, activity.SelectionCriterion4?.Code);
			AssertEquals(ExpectedSelectionCriterion5, activity.SelectionCriterion5?.Code);
			AssertEquals(ExpectedLocation, activity.Location?.Code);

			AssertEquals(ExpectedBranchCode, activity.Branch?.Code);
			AssertEquals(ExpectedDepartmentCode, activity.Department?.Code);
			AssertEquals(ExpectedCompanyCode, activity.Company?.Code);
			AssertEquals(ExpectedProjectManagerCode, activity.ProjectManager?.Code);

			var client1 = activity.OrganizationAddressCollection?.FindByType_ForTest(ActivityOrganizationAddressType.Client);
			var client2 = activity.OrganizationAddressCollection?.FindByType_ForTest(ActivityOrganizationAddressType.TechnicalClient);

			AssertEquals(ExpectedClient1Name, client1?.Contact);
			AssertEquals(ExpectedClient1Address1, client1?.Address1);
			AssertEquals(ExpectedClient2Name, client2?.Contact);
			AssertEquals(ExpectedClient2Address1, client2?.Address1);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, activity.CreatedBy?.Code);
		}

		void AssertRelatedActivityDescriptions(Activity activity)
		{
			var relatedItemSummary = activity.RelatedActivityCollection.Select(x => $"{x.DataContext.DataSourceCollection.Single().Type}: {x.DataContext.DataSourceCollection.Single().Key} - {x.Summary}");
			AssertContainsExactElementsInAnyOrder(ExpectedRelatedItems, relatedItemSummary);
		}

		protected abstract string ExpectedSummary { get; }
		protected abstract string ExpectedDescription { get; }
		protected abstract string ExpectedStatus { get; }
		protected abstract string ExpectedSelectionCriterion1 { get; }
		protected abstract string ExpectedSelectionCriterion2 { get; }
		protected abstract string ExpectedSelectionCriterion3 { get; }
		protected abstract string ExpectedSelectionCriterion4 { get; }
		protected abstract string ExpectedSelectionCriterion5 { get; }
		protected abstract string ExpectedLocation { get; }
		protected abstract string ExpectedBranchCode { get; }
		protected abstract string ExpectedDepartmentCode { get; }
		protected abstract string ExpectedCompanyCode { get; }
		protected abstract string ExpectedProjectManagerCode { get; }
		protected abstract string ExpectedClient1Name { get; }
		protected abstract string ExpectedClient1Address1 { get; }
		protected abstract string ExpectedClient2Name { get; }
		protected abstract string ExpectedClient2Address1 { get; }
		protected abstract IEnumerable<string> ExpectedRelatedItems { get; }

		public void TestTopLevelDataContextType()
		{
			var businessObject = BizoFactory.NewWithValidTestData<T>();
			var writer = ActivityTestHelper.GetWriter(businessObject);

			AssertEquals(ExpectedDataContextType, writer.TopLevelDataContextType);
		}

		public void TestCustomFields()
		{
			var businessObjectWithoutCustomFields = Factory.NewWithValidTestData<T>();
			var businessObjectWithCustomFields = Factory.NewWithValidTestData<T>();

			businessObjectWithCustomFields.SetUserDefinedValue("M'string", new ZString("Squanch"));
			businessObjectWithCustomFields.SetUserDefinedValue("M'int", new ZInt(69));

			AssertEquals("Squanch", businessObjectWithCustomFields.GetUserDefinedValue<ZString>("M'string"));

			var activityWithoutCustomFields = ActivityTestHelper.WriteActivity(businessObjectWithoutCustomFields);
			AssertNull("If there are no custom fields, the collection shouldn't even be included in the activity. SAD!", activityWithoutCustomFields.CustomizedFieldCollection);

			var activityWithCustomFields = ActivityTestHelper.WriteActivity(businessObjectWithCustomFields);
			var customFields = activityWithCustomFields.CustomizedFieldCollection.Select(x => $"{x.DataType}: {x.Key} - {x.Value}");
			AssertContainsExactElementsInAnyOrder(new[] { "String: M'string - Squanch", "Integer: M'int - 69" }, customFields);
		}

		public void TestRelatedItems_ShouldOnlyWriteOneLevelDeep()
		{
			var businessObject = GetBusinessObject();
			var relatedItems = GetRelatedItems().ToArray();
			businessObject.RelatedItems.AddRange(relatedItems);

			var secondLevelRelatedItem = GetSecondLevelRelatedItem();
			((IWorkTaskRelatedItemSource)relatedItems.First()).RelatedItems.Add((BusinessObject)secondLevelRelatedItem);

			BizoFactory.Save();

			var activity = ActivityTestHelper.WriteActivity(businessObject);
			AssertRelatedActivityDescriptions(activity);

			var relatedActivities = activity.RelatedActivityCollection.ToArray();
			AssertGreaterThanOrEqualTo("At least two activities should be related. SAD!", 2, relatedActivities.Length);
			AssertNotContains("The second level item should not be included in the related items collection because it isn't directly related to the source business object. SAD!", secondLevelRelatedItem.ItemDescription, activity.Description);

			foreach (var relatedActivity in relatedActivities)
			{
				AssertNull("The related activities should not have related activity collections because we're only recording related items one level deep. SAD!", relatedActivity.RelatedActivityCollection);
			}
		}

		[TestDate(2018, 7, 18)]
		public void TestNotes()
		{
			var businessObject = GetBusinessObject();
			businessObject.GetNotes().AddNew(true, "First note", "Remember that one time?");
			businessObject.GetNotes().AddNew(true, "Second note", "We couldn't stop screamin'.");

			var activity = ActivityTestHelper.WriteActivity(businessObject);

			AssertNotNull("The note collection should have been initialised. SAD!", activity.NoteCollection);

			var notesText = activity.NoteCollection.Select(x => x.NoteText.ToString());
			var expectedNotesText = new[] { "Remember that one time?", "We couldn't stop screamin'." }.Concat(GetAdditionalExpectedNoteText());
			AssertContainsExactElementsInAnyOrder("The notes should have been written into the Activity. SAD!", expectedNotesText, notesText);
		}

		protected virtual IEnumerable<string> GetAdditionalExpectedNoteText()
		{
			return Enumerable.Empty<string>();
		}

		[TestDate(2018, 2, 8)]
		public void TestTaskSetCollection()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			var capability = BizoFactory.NewWithValidTestData<GlbCapability>();
			var group = BizoFactory.NewWithValidTestData<GlbGroup>();

			var job = BizoFactory.New<T>();
			job.FillWithValidTestData();

			helper.CreateSystem(BizoFactory, job.WorkflowType);

			var jobHeader = helper.GetJobHeaderForParent(job, BizoFactory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobHeader, "Exported Workflow");
			var task = (ProcessTask)helper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 1, taskStatus: ProcessTaskStatusCodeList.Codes.Working, sequence: 2, description: "Be exported");
			task.P9_G4_RequiredCapability = capability.PK;
			task.P9_GG_AssignedGroup = group.PK;
			task.P9_EstimateVariationFactor = 2;
			task.P9_NotesAsString = "That's my note.";
			task.P9_CardNote = "READY TO START";
			task.P9_ScheduledDateUtc = ZDateTime.UtcNow.AddDays(2);
			task.P9_ActualDateUtc = ZDateTime.UtcNow.AddDays(3);

			BizoFactory.Save();

			TestDateAttribute.AddMinutes(10);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			BizoFactory.Save();

			var activity = ActivityTestHelper.WriteActivity(job);

			CombineAssertions(() =>
			{
				AssertNotNull(activity.TaskSetCollection);
				var activityJobData = activity.TaskSetCollection.Single();
				AssertEquals("JOB", activityJobData.Type?.Code);
				AssertNull(activityJobData.TaskCollection);
				AssertNotNull(activityJobData.TaskSetCollection);
				AssertContainsExactElementsInAnyOrder(new[] { "Exported Workflow" }, activityJobData.TaskSetCollection.Where(x => x.Description.HasValue).Select(x => x.Description.Value));

				var activityTaskSet = activityJobData.TaskSetCollection.Single();
				AssertEquals("TKS", activityTaskSet.Type?.Code);
				AssertNull(activityTaskSet.TaskSetCollection);
				AssertNotNull(activityTaskSet.TaskCollection);
				AssertContainsExactElementsInAnyOrder(new[] { "Be exported" }, activityTaskSet.TaskCollection.Where(x => x.Description.HasValue).Select(x => x.Description.Value));

				var activityTask = activityTaskSet.TaskCollection.Single();
				AssertEquals(task.P9_TaskID, activityTask.TaskID);
				AssertEquals(2, activityTask.Sequence);
				AssertEquals("That's my note.", activityTask.TaskNotes);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, activityTask.Status?.Code);
				AssertEquals("UDF", activityTask.Type?.Code);
				AssertEquals(GlbStaff.CurrentUser.GS_Code, activityTask.AssignedStaff?.Code);
				AssertEquals(capability.G4_Code, activityTask.AssignedCapability?.Code);
				AssertEquals(group.GG_Code, activityTask.AssignedGroup?.Code);
				AssertEquals(task.P9_EstDuration.ToTimeSpan(), activityTask.EstimatedDuration);
				AssertEquals(2m, activityTask.EstimateVariationFactor);
				AssertEquals(task.P9_ActualDuration.ToTimeSpan(), activityTask.ActualDuration);
				AssertEquals(ZDateTime.UtcNow, activityTask.CompletedTimeUTC.Value.ToZDateTime());
				AssertEquals(task.P9_ScheduledDateUtc, activityTask.EstimatedStartTimeUTC.Value.ToZDateTime());
				AssertEquals(task.P9_ActualDateUtc, activityTask.ActualStartTimeUTC.Value.ToZDateTime());
			});
		}

		public void TestMilestoneCollection()
		{
			var businessObject = GetBusinessObject();
			var milestone = businessObject.WorkflowItems.Milestones.AddNew();
			milestone.P9_Sequence = 2;
			milestone.P9_Description = "It's done!";
			milestone.TriggerConditions.TriggerEventCode = "Z69";

			BizoFactory.Save();

			var activity = ActivityTestHelper.WriteActivity(businessObject);
			AssertNotNull(activity.MilestoneCollection);

			var activityMilestone = activity.MilestoneCollection.Single();
			AssertEquals(2, activityMilestone.Sequence);
			AssertEquals("It's done!", activityMilestone.Description);
			AssertEquals("Z69", activityMilestone.EventCode);
		}

		public void TestJobCosting()
		{
			var businessObject = GetBusinessObject();

			if (businessObject is IJobHeaderParentCore)
			{
				var creator = ObjectFactory.New<IAccountingTestDataCreator>();
				creator.CreateJobHeader(businessObject, ZGuid.Empty);
				var chargeCode = BizoFactory.NewWithValidTestData<AccChargeCode>();
				creator.AddChargeLineToCreatedJobHeader(chargeCode.AC_Code, "Strawberry Smiggles", 123.45m, "AUD");
				creator.AddChargeLineToCreatedJobHeader(chargeCode.AC_Code, "Eye Holes", 1000000.00m, "CAD");

				BizoFactory.Save();

				var recipients = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ORP } };
				var writer = ActivityTestHelper.GetWriter(businessObject, recipientRoles: recipients);
				var activity = ActivityTestHelper.WriteActivity(businessObject, writer);

				AssertNotNull(activity.JobCosting);
				AssertContainsExactElementsInAnyOrder(new[] { "Strawberry Smiggles", "Eye Holes" }, activity.JobCosting.ChargeLineCollection.Select(x => x.Description.Value));
			}
			else
			{
				Assert("The job type doesn't support job costing so there's nothing to test.", true);
			}
		}

		[TestDate(2018, 8, 6)]
		public void TestConversation()
		{
			var businessObject = GetBusinessObject();

			if (businessObject is IConversationProvider provider)
			{
				BizoFactory.Save();

				var staff = BizoFactory.NewWithValidTestData<GlbStaff>();
				staff.GS_FullName = "Jan Michael Vincent";
				var participant = provider.eConversation.Participants.GetOrAdd(staff);
				var messageBizo = provider.eConversation.Messages.AddNew(participant, "Hello", false, false);
				provider.eConversation.AddMessageFromCurrentUser("System message", false, true);

				BizoFactory.Save();

				var activity = ActivityTestHelper.WriteActivity(businessObject);
				AssertNotNull(activity.Conversation);

				AssertContainsExactElementsInAnyOrder(new[]
				{
					"Hello, Jan Michael Vincent",
					"System message, CargoWise Support, system",
				}, activity.Conversation.ConversationMessageCollection.Select(GetMessageSummary));

				string GetMessageSummary(ConversationMessage message)
				{
					var internalString = message.IsInternal.HasValue && message.IsInternal.Value
						? ", internal"
						: string.Empty;
					var systemString = message.IsSystem.HasValue && message.IsSystem.Value ? ", system" : string.Empty;
					return FormattableString.Invariant(
						$"{message.Text}, {message.ParticipantName}{internalString}{systemString}");
				}

				var messageData = activity.Conversation.ConversationMessageCollection.Single(x => x.Text.HasValue && x.Text.Value == "Hello");
				AssertEquals(messageBizo.JCM_PostedTimeUtc, messageData.CreatedTime);
			}
			else
			{
				var activity = ActivityTestHelper.WriteActivity(businessObject);
				AssertNull("Conversation should be null when the business object doesn't support eConversation. SAD!", activity.Conversation);
			}
		}

		public void TestConversation_ShouldOnlyIncludeInternalMessages_WhenOrgProxyRecipientRoleUsed()
		{
			var businessObject = GetBusinessObject();

			if (businessObject is IConversationProvider provider)
			{
				BizoFactory.Save();

				provider.eConversation.AddMessageFromCurrentUser("External message", isInternal: false);
				provider.eConversation.AddMessageFromCurrentUser("Internal message", isInternal: true);

				BizoFactory.Save();

				AssertMessages(null, "External message, CargoWise Support");
				AssertMessages(new[] { new RecipientRoleDetail { Type = RecipientRoleType.AAD } }, "External message, CargoWise Support");

				var expectedMessagesForORP = new[] { "External message, CargoWise Support", "Internal message, CargoWise Support, internal" }.Concat(GetExpectedExtraMessagesForORP()).ToArray();
				AssertMessages(new[] { new RecipientRoleDetail { Type = RecipientRoleType.ORP } }, expectedMessagesForORP);

				void AssertMessages(RecipientRoleDetail[] recipients, params string[] expectedMessages)
				{
					var writer = ActivityTestHelper.GetWriter(businessObject, recipientRoles: recipients);
					var activity = ActivityTestHelper.WriteActivity(businessObject, writer);

					AssertContainsExactElementsInAnyOrder(expectedMessages, activity.Conversation.ConversationMessageCollection.Select(GetMessageSummary));
				}
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual IEnumerable<string> GetExpectedExtraMessagesForORP()
		{
			return Enumerable.Empty<string>();
		}

		static string GetMessageSummary(ConversationMessage message)
		{
			var internalString = message.IsInternal.HasValue && message.IsInternal.Value ? ", internal" : string.Empty;
			var systemString = message.IsSystem.HasValue && message.IsSystem.Value ? ", system" : string.Empty;
			return FormattableString.Invariant($"{message.Text}, {message.ParticipantName}{internalString}{systemString}");
		}

		public void TestParticipantCollections()
		{
			var branch = BizoFactory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUSYD";

			var group = BizoFactory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "GEORGECOSTANZA";
			group.GG_Desc = "The Human Fund";
			group.GG_IsActive = false;
			group.GG_IsSales = true; // there's a db constraint that requires this if you're setting a company, which we need to test the Location value.
			group.GG_GC = GlbCompany.CurrentCompany.PK;
			group.Company.Branches.Add(branch);

			var staff = BizoFactory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "STA";
			staff.GS_FullName = "Krombopulos Michael";
			staff.GS_Title = "Architect";
			staff.GS_GB_HomeBranch = branch.PK;

			BizoFactory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(staff.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var businessObject = GetBusinessObject();
				BizoFactory.Save();

				if (businessObject is IConversationProvider provider)
				{
					provider.eConversation.AddMessageFromCurrentUser("Hello", isInternal: false);
					provider.eConversation.Participants.AddNewParticipant(group);

					BizoFactory.Save();

					var orgParticipant = provider.eConversation.RelatedParties.SingleOrDefault();

					if (orgParticipant == null)
					{
						var orgContact = CreateContact();
						orgParticipant = provider.eConversation.RelatedParties.AddNewParticipant(orgContact);
						orgParticipant.RelatedPartyTypeName = "Contact";
					}

					orgParticipant.JCP_Relation = "Squanch";
					orgParticipant.JCP_IsSubscribed = false;

					var otherContact = ProcessMgmtTestHelper.CreateOrganizationAndContact(BizoFactory, "Other Org", "Bird Person");
					var otherOrg = otherContact.Header;
					otherOrg.MainAddress.Address1 = "321 Yemen Road";

					provider.eConversation.RelatedParties.AddNewParticipant(otherOrg);

					BizoFactory.Save();

					var activity = ActivityTestHelper.WriteActivity(businessObject);
					var staffParticipant = activity.Conversation.ParticipantStaffCollection.Single();

					CombineAssertions("Staff participant:", () =>
					{
						AssertEquals("Code", "STA", staffParticipant.Staff.Code);
						AssertEquals("Name", "Krombopulos Michael", staffParticipant.Staff.Name);
						AssertEquals("Location", "AUSYD", staffParticipant.Location.Code);
						AssertEquals("Job Title", "Architect", staffParticipant.JobTitle);
						AssertEquals("Is Active", true, staffParticipant.IsActive);
						AssertEquals("Is Subscribed", true, staffParticipant.IsSubscribed);
					});

					var groupParticipant = activity.Conversation.ParticipantGroupCollection.Single();

					CombineAssertions("Group participant:", () =>
					{
						AssertEquals("Code", "GEORGECOSTANZA", groupParticipant.Group.Code);
						AssertEquals("Name", "The Human Fund", groupParticipant.Group.Name);
						AssertEquals("Location", "AUSYD", groupParticipant.Location.Code);
						AssertEquals("Is Active", false, groupParticipant.IsActive);
						AssertEquals("Is Subscribed", true, groupParticipant.IsSubscribed);
					});

					var relatedContactParticipant = activity.Conversation.ParticipantRelatedPartyCollection.Single(x => x.RelatedPartyType == RelatedPartyType.Contact);

					CombineAssertions("Contact participant:", () =>
					{
						AssertEquals("Name", "Jan Michael Vincent", relatedContactParticipant.Party.Contact);
						AssertEquals("Location", "AUSYD", relatedContactParticipant.Location?.Code);
						AssertEquals("Is Active", true, relatedContactParticipant.IsActive);
						AssertEquals("Is Subscribed", false, relatedContactParticipant.IsSubscribed);
						AssertEquals("Relation", "Squanch", relatedContactParticipant.Relation);
						AssertEquals("Org Address", "123 Canada Street", relatedContactParticipant.Party.Address1);
						AssertEquals("Org Address Type", ZString.Empty, relatedContactParticipant.Party.AddressType);
					});

					var relatedOrgParticipant = activity.Conversation.ParticipantRelatedPartyCollection.Single(x => x.RelatedPartyType == RelatedPartyType.Organization);

					CombineAssertions("Organization participant:", () =>
					{
						AssertEquals("Location", null, relatedOrgParticipant.Location?.Code);
						AssertEquals("Full Name", "Other Org", relatedOrgParticipant.Party.CompanyName);
						AssertEquals("Is Active", true, relatedOrgParticipant.IsActive);
						AssertEquals("Is Subscribed", true, relatedOrgParticipant.IsSubscribed);
						AssertEquals("Relation", ZString.Empty, relatedOrgParticipant.Relation);
						AssertEquals("Org Address", "321 Yemen Road", relatedOrgParticipant.Party.Address1);
						AssertEquals("Org Address Type", ZString.Empty, relatedOrgParticipant.Party.AddressType);
					});
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestWriteActivity_WhenNoEConversationExists_ShouldNotPopulateEConversationOnBusinessObject()
		{
			var businessObject = GetBusinessObject();
			BizoFactory.Save();

			if (businessObject is IConversationProvider)
			{
				var conversation = JobConversation.GetConversation(businessObject);
				AssertNull(conversation);

				var activity = ActivityTestHelper.WriteActivity(businessObject);
				AssertNull(activity.Conversation);

				BizoFactory.Save();

				conversation = JobConversation.GetConversation(businessObject);
				AssertNull("Writing a business object into an Activity should not populate the business object's EConversation if one didn't already exist, even if the business object's factory is saved. SAD!", conversation);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestPopulateDataObjectAndGenerateXMLWithEmailBasedEConversationParticipantAdded()
		{
			var businessObject = GetBusinessObject();

			BizoFactory.Save();

			if (businessObject is IConversationProvider provider)
			{
				ProcessMgmtTestHelper.CreateConversationAndAddRelatedPartyThatWeWouldLikeToEmail(businessObject, "ben.govett@nospam.org");

				BizoFactory.Save();

				ParticipantRelatedParty relatedEmailParticipant = null;

				AssertNoExceptionThrown(() =>
				{
					var activity = ActivityTestHelper.WriteActivity(businessObject);
					AssertNotNull(activity.Conversation);

					relatedEmailParticipant = activity.Conversation.ParticipantRelatedPartyCollection.Single(x => x.RelatedPartyType == RelatedPartyType.Email);
				});

				CombineAssertions("Email participant:", () =>
				{
					AssertEquals("Email", "ben.govett@nospam.org", provider.eConversation.RelatedParties.Single().Parent.Email);
					AssertNotNull(relatedEmailParticipant);
					AssertNull("Party", relatedEmailParticipant.Party);
					AssertNull("Location", relatedEmailParticipant.Location);
					AssertEquals("Relation", ZString.Empty, relatedEmailParticipant.Relation);
					AssertEquals("Is Active", true, relatedEmailParticipant.IsActive);
					AssertEquals("Is Subscribed", true, relatedEmailParticipant.IsSubscribed);
					AssertEquals("Related Party Type", RelatedPartyType.Email, relatedEmailParticipant.RelatedPartyType);
				});
			}
			else
			{
				AssertNoExceptionThrown(() =>
				{
					var activity = ActivityTestHelper.WriteActivity(businessObject);
					AssertNull("Conversation should be null when the business object doesn't support eConversation.", activity.Conversation);
				});
			}
		}

		#region Implementation

		protected abstract T GetBusinessObject();

		protected abstract IEnumerable<IWorkTaskRelatedItem> GetRelatedItems();

		protected abstract IWorkTaskRelatedItem GetSecondLevelRelatedItem();

		protected abstract DataContextType ExpectedDataContextType { get; }

		#endregion
	}
}
