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
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.DataTransfer.Test
{
	abstract class ActivityDataObjectReaderTestCase<TReader, TBusinessObject> : UniversalActivityImplementationTestCase
		where TReader : ActivityDataObjectReader<TBusinessObject>
		where TBusinessObject : BusinessObject, IWorkTaskRelatedItem, IWorkTaskRelatedItemSource, IWorkflowProvider
	{
		public void TestPopulateBusinessObject()
		{
			var contact1 = CreateContact();
			var otherAddress = contact1.Header.Addresses.AddNew();
			otherAddress.Address1 = "15 Yemen Road";
			otherAddress.OA_RN_NKCountryCode = "YE";

			var contact2 = ProcessMgmtTestHelper.CreateOrganizationAndContact(BizoFactory, "Simple Rick's", "Mrs Sullivan", "mrs@simplericks.com");
			var address2 = contact2.Header.MainAddress;
			address2.Address1 = "3a/72 O'Riordan Street";
			address2.City = "Alexandria";
			address2.State = "NSW";
			address2.Postcode = "2015";
			address2.OA_RN_NKCountryCode = "AU";
			address2.OA_RL_NKRelatedPortCode = "AUSYD";
			BizoFactory.Save();

			var activity = CreateActivity(contact1);
			activity.Branch = Branch.New(CreateBranch());
			activity.Department = Department.New(CreateDepartment());
			activity.Company = Company.New(CreateCompany());
			activity.Location = new CodeDescriptionPair5Char { Code = "LB", Description = "Lebanon" };
			activity.OrganizationAddressCollection.Add(GetOrganizationAddress(contact2, address2, ActivityOrganizationAddressType.TechnicalClient));

			var manager = GlbStaff.New(BizoFactory);
			manager.GS_Code = "JAN";
			manager.GS_FullName = "Janet";
			activity.ProjectManager = Staff.New(manager);

			BizoFactory.Save();

			var existingObjects = BizoFactory.Load<TBusinessObject>(new ZQuery());
			AssertContainsExactElementsInAnyOrder("There should not be any objects in the system already, so that it is proven that the object generated is new. SAD!", Array.Empty<WorkRequest>(), existingObjects);

			var businessObject = ReadBusinessObject(activity);

			AssertEquals("Celebrity Baby Plastic Surgery Disasters", businessObject.ItemDescription); // Summary
			AssertEquals("AAA", businessObject.SelectionCriterion1);
			AssertEquals("BBB", businessObject.SelectionCriterion2);
			AssertEquals("CCC", businessObject.SelectionCriterion3);
			AssertSelectionCriterion4(businessObject);
			AssertSelectionCriterion5(businessObject);

			AssertDescription(businessObject);
			AssertBranch(businessObject);
			AssertDepartment(businessObject);
			AssertCompany(businessObject);
			AssertCountry(businessObject);
			AssertClient1(businessObject);
			AssertClient2(businessObject);
			AssertProjectManager(businessObject);
		}

		protected virtual void AssertSelectionCriterion4(TBusinessObject businessObject)
		{
			AssertEquals("DDD", businessObject.SelectionCriterion4);
		}

		protected virtual void AssertSelectionCriterion5(TBusinessObject businessObject)
		{
			AssertEquals("EEE", businessObject.SelectionCriterion5);
		}

		protected abstract void AssertDescription(TBusinessObject businessObject);
		protected abstract void AssertBranch(TBusinessObject businessObject);
		protected abstract void AssertDepartment(TBusinessObject businessObject);
		protected abstract void AssertCompany(TBusinessObject businessObject);
		protected abstract void AssertClient1(TBusinessObject businessObject);
		protected abstract void AssertClient2(TBusinessObject businessObject);
		protected abstract void AssertCountry(TBusinessObject businessObject);
		protected abstract void AssertProjectManager(TBusinessObject businessObject);

		public void TestRelatedItems()
		{
			var relatedItems = GetRelatedItemsForTests();
			BizoFactory.Save();

			var activity = CreateActivityWithContact();
			activity.SetRelatedActivityCollection(() => new List<Activity>());

			foreach (var item in relatedItems)
			{
				var relatedActivity = ActivityTestHelper.WriteActivity(item);
				ActivityTestHelper.SwapDataSourcesForDataTargets(relatedActivity);
				activity.RelatedActivityCollection.Add(relatedActivity);
			}

			var businessObject = ReadBusinessObject(activity);
			var actualRelatedItems = businessObject.RelatedItems.Cast<IWorkTaskRelatedItem>().Select(x => x.ItemDescription);
			AssertContainsExactElementsInAnyOrder(ExpectedRelatedItemDescriptions, actualRelatedItems);
		}

		BusinessObject[] GetRelatedItemsForTests(BusinessObjectFactory factory = null, OrgContact contact = null)
		{
			var relatedItems = GetRelatedItemsForTestsCore(factory ?? BizoFactory, contact ?? CreateContact()).ToArray();
			AssertGreaterThan("Please create at least two related items :)", relatedItems.Length, 1);

			return relatedItems;
		}

		protected abstract IEnumerable<BusinessObject> GetRelatedItemsForTestsCore(BusinessObjectFactory factory, OrgContact contact);

		protected abstract IEnumerable<string> ExpectedRelatedItemDescriptions { get; }

		public void TestRelatedItems_WithInvalidItemType_ShouldLogWarnings()
		{
			var relatedItems = GetRelatedItemsForTests().ToArray();
			BizoFactory.Save();

			var activity = CreateActivityWithContact();
			activity.SetRelatedActivityCollection(() => new List<Activity>());
			Activity relatedItemWithBadItemType = null;

			foreach (var item in relatedItems)
			{
				var relatedItem = ActivityTestHelper.WriteActivity(item);
				ActivityTestHelper.SwapDataSourcesForDataTargets(relatedItem);

				if (relatedItemWithBadItemType == null)
				{
					relatedItem.DataContext.DataTargetCollection.Single().Type = "FAKE NEWS";
					relatedItemWithBadItemType = relatedItem;
				}

				activity.RelatedActivityCollection.Add(relatedItem);
			}

			var logger = new TestErrorLogger();
			var reader = GetReader(activity, logger);
			var businessObject = reader.ReadIntoBusinessObject();
			var expectedRelatedItemDescriptions = relatedItems.Cast<IWorkTaskRelatedItem>().Where(x => x.ItemDescription != relatedItemWithBadItemType.Summary.Value).Select(x => x.ItemDescription);

			AssertContainsExactElementsInAnyOrder("Only the related items with a valid and supported item types should have been attached. SAD!",
				expectedRelatedItemDescriptions, businessObject.RelatedItems.Cast<IWorkTaskRelatedItem>().Select(x => x.ItemDescription));

			AssertContains("There was a related item with an invalid item type included in the related items xml, so a warning should have been logged. SAD!",
				$"Warning - Could not attach related item [FAKE NEWS: {relatedItemWithBadItemType.DataContext.DataTargetCollection.Single().Key.Value} - {relatedItemWithBadItemType.Summary}]. Only item types {ExpectedValidRelatedItemTypes} can be attached to a {businessObject.Type}.", logger.Logs);
		}

		protected abstract string ExpectedValidRelatedItemTypes { get; }

		public void TestRelatedItems_WithBadJobNumbers_ShouldLogWarnings()
		{
			var relatedItems = GetRelatedItemsForTests().ToArray();
			BizoFactory.Save();

			var activity = CreateActivityWithContact();
			activity.SetRelatedActivityCollection(() => new List<Activity>());
			Activity relatedItemWithBadJobNumber = null;

			foreach (var item in relatedItems)
			{
				var relatedItem = ActivityTestHelper.WriteActivity(item);
				ActivityTestHelper.SwapDataSourcesForDataTargets(relatedItem);

				if (relatedItemWithBadJobNumber == null)
				{
					relatedItem.DataContext.DataTargetCollection.Single().Key = "FAKENEWS";
					relatedItemWithBadJobNumber = relatedItem;
				}

				activity.RelatedActivityCollection.Add(relatedItem);
			}

			var logger = new TestErrorLogger();
			var reader = GetReader(activity, logger);
			var businessObject = reader.ReadIntoBusinessObject();
			var expectedRelatedItemDescriptions = relatedItems.Cast<IWorkTaskRelatedItem>().Where(x => x.ItemDescription != relatedItemWithBadJobNumber.Summary.Value).Select(x => x.ItemDescription);

			AssertContainsExactElementsInAnyOrder("Only the related items with a valid job number should have been attached. SAD!",
				expectedRelatedItemDescriptions, businessObject.RelatedItems.Cast<IWorkTaskRelatedItem>().Select(x => x.ItemDescription));

			AssertContains("There was a related item with an invalid job number included in the related items xml, so a warning should have been logged. SAD!",
				$"Warning - Could not attach related item [{relatedItemWithBadJobNumber.DataContext.DataTargetCollection.Single().Type.Value}: FAKENEWS - {relatedItemWithBadJobNumber.Summary}]. A {relatedItemWithBadJobNumber.DataContext.DataTargetCollection.Single().Type.Value} with the specified job number was not found.", logger.Logs);
		}

		public void TestRelatedItems_WithNullDataTargetCollection_ShouldLogWarnings()
		{
			AssertRelatedItems_WithBadDataTargets_ShouldLogWarnings(relatedItem => ((DataContext)relatedItem.DataContext).DataTargetCollection = null);
		}

		public void TestRelatedItems_WithEmptyDataTargetCollection_ShouldLogWarnings()
		{
			AssertRelatedItems_WithBadDataTargets_ShouldLogWarnings(relatedItem => ((DataContext)relatedItem.DataContext).DataTargetCollection = new List<DataTarget>());
		}

		void AssertRelatedItems_WithBadDataTargets_ShouldLogWarnings(Action<Activity> makeDataTargetBadAgainAction)
		{
			var relatedItems = GetRelatedItemsForTests().ToArray();
			BizoFactory.Save();

			var activity = CreateActivityWithContact();
			activity.SetRelatedActivityCollection(() => new List<Activity>());
			Activity relatedItemWithNoDataTarget = null;

			foreach (var item in relatedItems)
			{
				var relatedItem = ActivityTestHelper.WriteActivity(item);
				ActivityTestHelper.SwapDataSourcesForDataTargets(relatedItem);

				if (relatedItemWithNoDataTarget == null)
				{
					makeDataTargetBadAgainAction.Invoke(relatedItem);
					relatedItemWithNoDataTarget = relatedItem;
				}

				activity.RelatedActivityCollection.Add(relatedItem);
			}

			var logger = new TestErrorLogger();
			var reader = GetReader(activity, logger);
			var businessObject = reader.ReadIntoBusinessObject();
			var expectedRelatedItemDescriptions = relatedItems.Cast<IWorkTaskRelatedItem>().Where(x => x.ItemDescription != relatedItemWithNoDataTarget.Summary.Value).Select(x => x.ItemDescription);

			AssertContainsExactElementsInAnyOrder("Only the related items with a data target should have been attached. SAD!",
				expectedRelatedItemDescriptions, businessObject.RelatedItems.Cast<IWorkTaskRelatedItem>().Select(x => x.ItemDescription));

			AssertContains("There was a related item with a bad DataTarget included in the related items xml, so a warning should have been logged. SAD!",
				$"Warning - Could not attach related item [{relatedItemWithNoDataTarget.Summary}]. Each related Activity must include a valid DataTarget.", logger.Logs);
		}

		public void TestRelatedItems_WhenRelatedItemAlreadyExists_ShouldNotAddItemTwice()
		{
			var businessObject = BizoFactory.NewWithValidTestData<TBusinessObject>();
			var relatedItems = GetRelatedItemsForTests();
			businessObject.RelatedItems.Add(relatedItems.First());

			BizoFactory.Save();

			var activity = CreateActivityWithContact(businessObject.Number);
			activity.SetRelatedActivityCollection(() => new List<Activity>());

			foreach (var item in relatedItems)
			{
				var relatedActivity = ActivityTestHelper.WriteActivity(item);
				ActivityTestHelper.SwapDataSourcesForDataTargets(relatedActivity);
				activity.RelatedActivityCollection.Add(relatedActivity);
			}

			var readBusinessObject = ReadBusinessObject(activity);
			var actualRelatedItems = readBusinessObject.RelatedItems.Cast<IWorkTaskRelatedItem>().Select(x => x.ItemDescription);
			AssertContainsExactElementsInAnyOrder(ExpectedRelatedItemDescriptions, actualRelatedItems);
			AssertEquals("The business object should have been updated.", businessObject.PK, readBusinessObject.PK);
		}

		public void TestRelatedItems_WithNoContextKey_ShouldCreateNewBusinessObjectAndAttach()
		{
			var contact = CreateContact();
			var factoryThatShantBeSaved = new ReadOnlyBusinessObjectFactory { RefreshEnabled = false };
			var relatedItems = GetRelatedItemsForTests(factoryThatShantBeSaved, contact);

			var activity = CreateActivity(contact);
			activity.SetRelatedActivityCollection(() => new List<Activity>());

			foreach (var item in relatedItems)
			{
				var relatedActivity = ActivityTestHelper.WriteActivity(item);
				ActivityTestHelper.SwapDataSourcesForDataTargets(relatedActivity);
				relatedActivity.DataContext.DataTargetCollection.Single().Key = null;
				activity.RelatedActivityCollection.Add(relatedActivity);
			}

			var existingItemsOfMainType = BizoFactory.Load<TBusinessObject>(new ZQuery());
			AssertContainsExactElementsInAnyOrder("The main item should not already exist in the database. SAD!", Array.Empty<TBusinessObject>(), existingItemsOfMainType);

			foreach (var type in relatedItems.Select(x => x.GetType()).Distinct())
			{
				var existingRelatedItems = BizoFactory.Load(type, new ZQuery());
				AssertContainsExactElementsInAnyOrder("The related items should not already exist in the database. SAD!", Array.Empty<BusinessObject>(), existingRelatedItems);
			}

			var businessObject = ReadBusinessObject(activity);
			var expectedDescriptions = relatedItems.Cast<IWorkTaskRelatedItem>().Select(x => x.ItemDescription);
			var actualDescriptions = businessObject.RelatedItems.Cast<IWorkTaskRelatedItem>().Select(x => x.ItemDescription);
			AssertContainsExactElementsInAnyOrder(expectedDescriptions, actualDescriptions);
		}

		public void TestSupportedRelatedModules_HaveDataContextType()
		{
			var businessObject = BizoFactory.New<TBusinessObject>();

			foreach (var source in businessObject.SupportedRelatedItemModules)
			{
				AssertNotEquals("DataContextTypes must be supplied for all related item types that you wish to export with universal xml.", default(DataContextType), source.DataContextType);
			}
		}

		public void TestCustomFields()
		{
			var activity = CreateActivityWithContact();

			activity.SetCustomizedFieldCollection(() => new List<CustomizedField>
			{
				{ "M'string", new ZString("Mrs Refrigerator") },
				{ "M'Int", new ZInt(420) },
			});

			var businessObject = ReadBusinessObject(activity);

			AssertEquals("Mrs Refrigerator", businessObject.GetUserDefinedValue<ZString>("M'string"));
			AssertEquals(420, businessObject.GetUserDefinedValue<ZInt>("M'Int"));
		}

		public void TestStatus_ShouldNotUpdate()
		{
			var businessObject = BizoFactory.NewWithValidTestData<TBusinessObject>();
			BizoFactory.Save();

			var activity = CreateActivityWithContact(businessObject.Number);
			activity.Status = new CodeDescriptionPair { Code = "CAN", Description = "Canceled" };

			var readBusinessObject = ReadBusinessObject(activity);
			var originalStatus = GetStatusPropertyInfo(businessObject).Value;
			var newStatus = GetStatusPropertyInfo(readBusinessObject).Value;

			AssertEquals("The activity should not have created a new job, but performed updates on the existing one. SAD!", businessObject.PK, readBusinessObject.PK);
			AssertEquals("The activity should not have updated the status of the job. SAD!", originalStatus, newStatus);
			AssertNotEquals("The activity should not have updated the status of the job. SAD!", "CAN", newStatus);
		}

		protected abstract ZPropertyInfo GetStatusPropertyInfo(TBusinessObject businessObject);

		public void TestClient_ShouldIgnoreCaseForMatchingContactName()
		{
			var contact = CreateContact();

			BizoFactory.Save();

			var activity = CreateActivity(contact);
			var addressElement = activity.OrganizationAddressCollection.Single();
			addressElement.Contact = "jan MICHAEL vinCENT";
			AssertNotEquals(contact.OC_ContactName, addressElement.Contact.Value);
			AssertEquals(contact.OC_ContactName.ToLower(), addressElement.Contact.Value.ToLower());

			var logger = new TestErrorLogger();
			var reader = GetReader(activity, logger);
			TBusinessObject businessObject = null;

			AssertNoExceptionThrown("Attempting to import the client name, even with different casing, should succeed and not throw an exception. SAD!", () => businessObject = reader.ReadIntoBusinessObject());

			AssertNotContains("Could not find a contact with the name 'jan MICHAEL vinCENT' in the matched organization 'Vandelay Industries'", logger.Logs);
			AssertClient1(businessObject);
		}

		public void TestNotes()
		{
			var activity = CreateActivityWithContact();
			activity.SetNoteCollection(() => new DataObjectList<Note>
			{
				new Note { Description = "First note", NoteText = "You don't know me." },
				new Note { Description = "Second note", NoteText = "You DO know me." }
			});

			var businessObject = ReadBusinessObject(activity);
			Factory.SaveForTesting();
			var notes = businessObject.GetNotes().GetAllNotes().Cast<StmNote>();
			var filteredNotesText = FilterOutNotesWeDontCareAboutForThisTest(notes.Select(x => x.ST_NoteDataAsText));

			AssertContainsExactElementsInAnyOrder("The notes should have been created. SAD!", new[] { "You don't know me.", "You DO know me." }, filteredNotesText);

			activity.DataContext.DataTargetCollection.Single().Key = businessObject.Number;
			activity.NoteCollection.RemoveAt(0);
			activity.NoteCollection.Single().NoteText = "I'm walkin' here!";

			var updatedBusinessObject = ReadBusinessObject(activity);
			notes = updatedBusinessObject.GetNotes().GetAllNotes().Cast<StmNote>();
			filteredNotesText = FilterOutNotesWeDontCareAboutForThisTest(notes.Select(x => x.ST_NoteDataAsText));

			AssertContainsExactElementsInAnyOrder("The note should have been updated. SAD!", new[] { "You don't know me.", "I'm walkin' here!" }, filteredNotesText);
			AssertEquals("The business object should have been updated rather than creating a new one. SAD!", businessObject.PK, updatedBusinessObject.PK);
		}

		protected virtual IEnumerable<ZString> FilterOutNotesWeDontCareAboutForThisTest(IEnumerable<ZString> allNotes)
		{
			return allNotes;
		}

		public void TestCreatedBy_ShouldDoNothing()
		{
			var staff = GlbStaff.New(BizoFactory);
			staff.GS_Code = "PPP";
			AssertNotEquals("The random code I've chosen needs to be different than the current user code, otherwise this test is MEANINGLESS!", staff.GS_Code, GlbStaff.CurrentUser.GS_Code);

			var activity = CreateActivityWithContact();
			var businessObject = ReadBusinessObject(activity);
			Factory.SaveForTesting();

			AssertEquals("The object should be created by the logged in user by default. SAD!", GlbStaff.CurrentUser.GS_Code, GetCreateUserCode(businessObject));

			activity.DataContext.DataTargetCollection.Single().Key = businessObject.Number;
			activity.Summary = "This object has been updated.";
			activity.CreatedBy = Staff.New(staff);

			var updatedBusinessObject = ReadBusinessObject(activity);
			AssertEquals("The business object should have been updated instead of creating a new one, otherwise this test is MEANINGLESS!", businessObject.PK, updatedBusinessObject.PK);
			AssertEquals("This object has been updated.", businessObject.ItemDescription);
			AssertEquals("The Created By field should not have been updated by Universal XML. SAD!", GlbStaff.CurrentUser.GS_Code, GetCreateUserCode(businessObject));
		}

		protected abstract ZString GetCreateUserCode(TBusinessObject businessObject);

		public void TestTaskSetCollection_ShouldDoNothing()
		{
			var businessObject = BizoFactory.NewWithValidTestData<TBusinessObject>();

			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(BizoFactory, businessObject.WorkflowType);

			var jobHeader = helper.GetJobHeaderForParent(businessObject, BizoFactory, addDefaultProcessHeaderIfNone: false);
			var deliverableUnit = helper.CreateWorkflow(jobHeader, "Actual Deliverable Unit");
			var task = helper.CreateTask(deliverableUnit, description: "There's a meeting you don't know about.");

			BizoFactory.Save();

			var taskSet = new TaskSet(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Description = "That meeting never happened.",
				Type = new CodeDescriptionPair { Code = "TKS", Description = "Job Workflow" },
			};

			taskSet.SetTaskCollection(() => new List<Task>(1)
				{
					new Task
					{
						Sequence = 2,
						TaskID = "T12345",
						Description = "Believe me.",
						Type = new CodeDescriptionPair { Code = "UDF", Description = "Undefined" },
					},
				});

			var activity = ActivityTestHelper.WriteActivity(businessObject);
			ActivityTestHelper.SwapDataSourcesForDataTargets(activity);
			activity.Summary = "This has been updated.";
			activity.TaskSetCollection.Add(taskSet);

			AssertEquals(2, activity.TaskSetCollection.Count);

			var updatedBusinessObject = ReadBusinessObject(activity);
			Factory.SaveForTesting();

			AssertEquals("The business object should have been updated rather than creating a new one. SAD!", businessObject.PK, updatedBusinessObject.PK);
			AssertEquals("The business object should have been updated. SAD!", "This has been updated.", updatedBusinessObject.ItemDescription);

			var deliverableUnits = new BusinessObjectFactory().Load<IProcessHeader>(new ZQuery(ProcessHeaderSchema.FH_ParentId, updatedBusinessObject.PK).AddToFilter(ProcessHeaderSchema.FH_FH_ParentHeader, SQLComparisonOperator.NotEqual, null));
			AssertContainsExactElementsInAnyOrder("No new deliverable units should have been added, because deliverable units are read-only in Universal XML. SAD!", new[] { "Actual Deliverable Unit" }, deliverableUnits.Select(x => x.FH_CompletionStatement));

			AssertContainsExactElementsInAnyOrder("No new tasks should have been added, because the tasks are read-only in Universal XML. SAD!",
				new[] { "There's a meeting you don't know about." }, updatedBusinessObject.WorkflowItems.Tasks.Cast<ProcessTask>().Select(x => x.P9_Description));
		}

		public void TestMilestoneCollection_ShouldDoNothing()
		{
			var businessObject = BizoFactory.NewWithValidTestData<TBusinessObject>();
			var task = businessObject.WorkflowItems.Milestones.AddNew();
			task.FillWithValidTestData();
			task.P9_Description = "There's a meeting you don't know about.";

			BizoFactory.Save();

			var activity = CreateActivityWithContact(businessObject.Number);
			activity.Summary = "This has been updated.";
			activity.SetMilestoneCollection(() => new List<Milestone>(1)
			{
				new Milestone
				{
					Sequence = 2,
					Description = "That meeting never happened.",
					EventCode = "Z69",
				}
			});

			var updatedBusinessObject = ReadBusinessObject(activity);

			AssertEquals("The business object should have been updated rather than creating a new one. SAD!", businessObject.PK, updatedBusinessObject.PK);
			AssertEquals("The business object should have been updated. SAD!", "This has been updated.", updatedBusinessObject.ItemDescription);

			AssertContainsExactElementsInAnyOrder("No new tasks should have been added, because the task collection is read only. SAD!",
				new[] { "There's a meeting you don't know about." }, updatedBusinessObject.WorkflowItems.Milestones.Cast<ProcessTask>().Select(x => x.P9_Description));
		}

		public void TestJobCosting_ShouldDoNothing()
		{
			var businessObject = BizoFactory.NewWithValidTestData<TBusinessObject>();

			if (businessObject is IJobHeaderParentCore)
			{
				var contact = CreateContact();
				var creator = ObjectFactory.New<IAccountingTestDataCreator>();
				creator.CreateJobHeader(businessObject, contact.Header.PK);
				creator.AddChargeLineToCreatedJobHeader("BAF", "Strawberry Smiggles", 123.45m, "AUD");

				BizoFactory.Save();

				var recipients = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ORP } };
				var writer = ActivityTestHelper.GetWriter(businessObject, recipientRoles: recipients);
				var activity = ActivityTestHelper.WriteActivity(businessObject, writer);

				ActivityTestHelper.SwapDataSourcesForDataTargets(activity);
				activity.Summary = "This object has been updated.";
				var line = (ChargeLine)activity.JobCosting.ChargeLineCollection.Single().Clone();
				line.Description = "Eye Holes";
				activity.JobCosting.ChargeLineCollection.Add(line);

				var updatedBusinessObject = ReadBusinessObject(activity);
				AssertEquals("The business object should have been updated instead of creating a new one, otherwise this test is MEANINGLESS!", businessObject.PK, updatedBusinessObject.PK);
				AssertEquals("This object has been updated.", updatedBusinessObject.ItemDescription);

				Factory.SaveForTesting();

				var factory = new BusinessObjectFactory();
				var query = new ZDBOnlyQuery(typeof(JobCharge));
				var subQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobChargeSchema.JR_JH);
				subQuery.AddToFilter(JobHeaderSchema.JH_ParentID, businessObject.PK);
				query.AddSubQuery(subQuery, JoinCondition.And);

				var lines = factory.Load<JobCharge>(query);
				AssertContainsExactElementsInAnyOrder(new[] { "Strawberry Smiggles" }, lines.Select(x => x.JR_Desc));
			}
			else
			{
				Assert("The job type doesn't support job costing so there's nothing to test.", true);
			}
		}

		public void TestConversation_ShouldDoNothing()
		{
			EnvProxy.Instance.Registry.MailboxEmailAddress = "DoNotReply@wisetechglobal.com.au";
			var businessObject = BizoFactory.NewWithValidTestData<TBusinessObject>();
			BizoFactory.Save();

			if (businessObject is IConversationProvider provider)
			{
				provider.eConversation.AddMessageFromCurrentUser("Hello", false, false);

				BizoFactory.Save();

				var activity = ActivityTestHelper.WriteActivity(businessObject);
				ActivityTestHelper.SwapDataSourcesForDataTargets(activity);

				activity.Summary = "This has been updated.";
				activity.Conversation.ConversationMessageCollection.Add(new ConversationMessage { ParticipantName = GlbStaff.CurrentUser.GS_FullName, Text = "This shant be added." });
				var updatedBusinessObject = ReadBusinessObject(activity);

				AssertEquals("The business object should have been updated rather than creating a new one. SAD!", businessObject.PK, updatedBusinessObject.PK);
				AssertEquals("The business object should have been updated rather than creating a new one. SAD!", "This has been updated.", updatedBusinessObject.ItemDescription);

				var messages = ((IConversationProvider)updatedBusinessObject).eConversation.Messages.Where(x => !x.JCM_Body.EndsWith("has been added to the conversation.", StringComparison.Ordinal)).Select(x => x.JCM_Body);
				AssertContainsExactElementsInAnyOrder("The new message should not have been added because econversation is meant to be read-only for universal xml. SAD!", new[] { "Hello" }, messages);
			}
			else
			{
				Assert("The job type doesn't support eConversation so there's nothing to test.", true);
			}
		}

		public void TestParticipantCollections_ShouldDoNothing()
		{
			EnvProxy.Instance.Registry.MailboxEmailAddress = "DoNotReply@wisetechglobal.com.au";
			var otherStaff = BizoFactory.NewWithValidTestData<GlbStaff>();
			var businessObject = BizoFactory.NewWithValidTestData<TBusinessObject>();
			BizoFactory.Save();

			if (businessObject is IConversationProvider provider)
			{
				provider.eConversation.AddMessageFromCurrentUser("Hello", false, false);

				BizoFactory.Save();

				var activity = ActivityTestHelper.WriteActivity(businessObject);
				ActivityTestHelper.SwapDataSourcesForDataTargets(activity);

				activity.Summary = "This has been updated.";

				var newParticipant = (ParticipantStaff)activity.Conversation.ParticipantStaffCollection.Single().Clone();
				newParticipant.Staff = Staff.New(otherStaff);
				activity.Conversation.ParticipantStaffCollection.Add(newParticipant);

				var updatedBusinessObject = ReadBusinessObject(activity);

				AssertEquals("The business object should have been updated rather than creating a new one. SAD!", businessObject.PK, updatedBusinessObject.PK);
				AssertEquals("The business object should have been updated rather than creating a new one. SAD!", "This has been updated.", updatedBusinessObject.ItemDescription);

				var participants = ((IConversationProvider)updatedBusinessObject).eConversation.Staff.Select(x => x.Parent.Name);
				AssertContainsExactElementsInAnyOrder("The new participant should not have been added because econversation is meant to be read-only for universal xml. SAD!", new[] { "CargoWise Support" }, participants);
			}
			else
			{
				Assert("The job type doesn't support eConversation so there's nothing to test.", true);
			}
		}

		public void TestTopLevelDataContextType()
		{
			var activity = new Activity(DefaultDataObjectWriterStrategy.TestInstance);
			var reader = GetReader(activity);

			AssertEquals(ExpectedDataContextType, reader.DataContextType);
		}

		protected abstract DataContextType ExpectedDataContextType { get; }

		#region Implementation

		protected Activity CreateActivity(OrgContact contact, ZString? dataTargetJobNumber = null)
		{
			var activity = new Activity(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Summary = "Celebrity Baby Plastic Surgery Disasters",
				Description = "Lonely Gal Margarita Mix for One",
				SelectionCriterion1 = new CodeDescriptionPair { Code = "AAA", Description = "Clam Chowder" },
				SelectionCriterion2 = new CodeDescriptionPair { Code = "BBB", Description = "A Little Bit Chowder Now" },
				SelectionCriterion3 = new CodeDescriptionPair { Code = "CCC", Description = "Pump Up the Clam" },
				SelectionCriterion4 = new CodeDescriptionPair { Code = "DDD", Description = "It's basically a savoury latte with bugs in it" },
				SelectionCriterion5 = new CodeDescriptionPair { Code = "EEE", Description = "It's hot ocean milk with dead animal croutons" },
				Status = new CodeDescriptionPair { Code = "CAN", Description = "Canceled" },
				DataContext = new DataContext(),
			};

			activity.DataContext.AddDataTarget(ExpectedDataContextType, dataTargetJobNumber);
			activity.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { GetOrganizationAddress(contact, contact.Header.MainAddress, ActivityOrganizationAddressType.Client) });

			return activity;
		}

		static OrganizationAddress GetOrganizationAddress(OrgContact contact, OrgAddress address, ActivityOrganizationAddressType type)
		{
			return new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Contact = contact.OC_ContactName,
				Address1 = address.Address1,
				Address2 = address.Address2,
				City = address.City,
				State = address.State,
				Postcode = address.Postcode,
				Country = UniversalDataBuss.DataObjects.Universal.Country.New(address.RelatedCountry),
				OrganizationCode = contact.OrganisationCode,
				AddressType = type.ToString(),
			};
		}

		protected Activity CreateActivityWithContact(ZString? dataTargetJobNumber = null)
		{
			var contact = CreateContact();
			return CreateActivity(contact, dataTargetJobNumber);
		}

		protected TReader GetReader(Activity activity)
		{
			return GetReader(activity, new DummyLogger());
		}

		protected TReader GetReader(Activity activity, IXmlImportLogger logger)
		{
			var businessObject = new BusinessObjectFactory().New<TBusinessObject>();
			var contextManager = (IActivityDataContextManager)businessObject.GetUniversalDataContextManager();
			return (TReader)contextManager.GetActivityDataObjectReader(activity, logger, Factory);
		}

		protected TBusinessObject ReadBusinessObject(Activity activity)
		{
			var reader = GetReader(activity);
			return reader.ReadIntoBusinessObject();
		}

		#endregion
	}
}
