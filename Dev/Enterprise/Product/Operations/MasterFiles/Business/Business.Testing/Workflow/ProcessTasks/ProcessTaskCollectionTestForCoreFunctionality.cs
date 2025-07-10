using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessTaskCollection))]
	class ProcessTaskCollectionTestForCoreFunctionality : BusinessObjectCollectionTestCase
	{
		#region Delete

		public void TestDeleteTaskThatCannotBeDeleted()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			ProcessTask task1 = template.WorkflowItems.AddNew();
			task1.P9_TaskCannotBeDeleted = true;

			ProcessTask task2 = template.WorkflowItems.AddNew();
			AssertEquals(2, template.WorkflowItems.Count);

			AssertEquals(false, task1.CanDelete);
			AssertEquals(true, task2.CanDelete);
		}

		#endregion

		#region CreateNewCollection

		public void TestCreateNewCollection()
		{
			OrgOpportunity parent = Factory.New<OrgOpportunity>();
			DummyProcessTaskCollection collection = new DummyProcessTaskCollection(parent);
			DummyProcessTaskCollection newCollection = (DummyProcessTaskCollection)collection.CreateNewCollection();
			Assert("A new collection should be created", collection != newCollection);
			AssertEquals("New collection type same as the existing collection", typeof(DummyProcessTaskCollection), newCollection.GetType());
		}

		#endregion

		public void TestSetDefaultsForNewTask()
		{
			var parent = Factory.New<OrgOpportunity>();
			var collection = new DummyProcessTaskCollection(parent);

			var task1 = Factory.New<ProcessTask>();
			task1.P9_ParentID = parent.PK;

			var task2 = Factory.New<ProcessTask>();
			task2.P9_ParentTableCode = OrgOpportunitySchema.Constants.Prefix;

			var task3 = Factory.New<ProcessTask>();

			collection.SetDefaultsForNewTask(task1, false);
			collection.SetDefaultsForNewTask(task2, false);
			collection.SetDefaultsForNewTask(task3, false);

			AssertEquals(parent.PK, task1.P9_ParentID);
			AssertEquals(parent.PK, task2.P9_ParentID);
			AssertEquals(parent.PK, task3.P9_ParentID);

			AssertEquals(OrgOpportunitySchema.Constants.Prefix, task1.P9_ParentTableCode);
			AssertEquals(OrgOpportunitySchema.Constants.Prefix, task2.P9_ParentTableCode);
			AssertEquals(OrgOpportunitySchema.Constants.Prefix, task3.P9_ParentTableCode);
		}

		public void TestWorkflowTypeRefreshed()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "CAM";
			template.WorkflowItems.AddNew();
			Factory.Save();

			IWorkflowProvider campaign = (IWorkflowProvider)Factory.New<IGlbCompanyCampaign>();
			campaign.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("CAM", campaign.WorkflowItems[0].WorkflowType);
			AssertEquals("Campaign list found", WorkflowDataRegistry.Instance.TaskTypes.Value.GetTaskTypesFromWorkflowCode("CAM").Count, campaign.WorkflowItems[0].Lookups.Types.Count);
		}

		public void TestP9_Description_WorkflowProcessTypePOD()
		{
			var query = new ZQuery(ProcessTaskTemplateSchema.P0_ProcessType, "POD");
			query.AddToFilter(ProcessTaskTemplateSchema.P0_IsSystem, 1);
			var template = Factory.Load<ProcessTaskTemplate>(query).First();
			AssertEquals(8, template.WorkflowItems.Count);

			var actualDescriptionList = template.WorkflowItems.Cast<ProcessTask>().OrderBy(x => x.P9_Description).Select(x => x.P9_Description.ToString()).ToArray();
			AssertArrayEqualsByElements(podDescriptionList, actualDescriptionList);
		}

		readonly string[] podDescriptionList = { "AP Invoice Posted",
"Approved",
"Delivered",
"Expected Delivery Date Estimated",
"Order Placed",
"Order/Invoice/GRN Audit Complete",
"Ready for Pickup/Delivery Date Estimated",
"Supplier Reference Number assigned to Order" };

		public void TestWorkflowTypeWhenNoParentAvailable()
		{
			ProcessTaskCollection collection = new ProcessTaskCollection(Factory);
			AssertEquals(ZString.Empty, collection.WorkflowType);
		}

		public void TestDefaultsOnChildSetWhenTasksCreatedFromTemplate()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "OPP";
			template.WorkflowItems.AddNew();

			Factory.Save();

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.Contacts.AddNew();
			OrgOpportunity opp = org.SalesOpportunities.AddNew();
			opp.P8_OC = org.Contacts[0].PK;
			opp.P8_OA = org.MainAddress.PK;

			opp.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals(1, opp.WorkflowItems.Count);
			AssertEquals(org.Contacts[0].PK, opp.WorkflowItems[0].P9_OC);
			AssertEquals(org.MainAddress.PK, opp.WorkflowItems[0].P9_OA);
		}

		public virtual void TestSupportsContactAndAddress()
		{
			OrgOpportunity opp = Factory.New<OrgOpportunity>();
			ProcessTaskCollection tasks = new ProcessTaskCollection(opp);
			AssertEquals("Stand-Alone Process Task does NOT support contact or addresses", false, tasks.SupportsContactAndAddress);

			DummyProcessTaskCollection tasks2 = new DummyProcessTaskCollection(opp);
			tasks2.SupportsContactAndAddress_Override = true;
			AssertEquals("Overidden Task DOES support contacts and addresses", true, tasks2.SupportsContactAndAddress);
		}

		public virtual void TestOriginCountry()
		{
			AssertEquals("Empty by default", ZString.Empty, Collection.OriginCountry);
		}

		public virtual void TestDestinationCountry()
		{
			AssertEquals("Empty by default", ZString.Empty, Collection.DestinationCountry);
		}

		public void TestCreateTemplateTasks()
		{
			OrgOpportunity opp = Factory.New<OrgOpportunity>();
			ProcessTaskCollection collection = new ProcessTaskCollection(opp);
			bool result = collection.Tasks.CreateItemsFromTemplate().HasTemplateApplied;
			AssertEquals("By default, no template tasks ability exists", false, result);
		}

		[ExpectException(typeof(NullReferenceException))]
		public void TestParentNullException()
		{
			OrgOpportunity opp = Factory.New<OrgOpportunity>();
			ProcessTaskCollection collection = new ProcessTaskCollection(opp);

			ProcessTaskCollection collectionNoParent = new ProcessTaskCollection((BusinessObject)null);
		}

		public void TestSetDefaultsOnChild()
		{
			GlbGroup testGroup = Factory.New<GlbGroup>();

			OrgOpportunity opp = Factory.New<OrgOpportunity>();
			ProcessTask task = opp.WorkflowItems.AddNew();
			AssertEquals("Task Parent Table is Org Opportunity", OrgOpportunitySchema.Constants.Prefix, task.P9_ParentTableCode);
			AssertEquals("Task Parent ID is Org Opportunity PK", opp.PK, task.P9_ParentID);
			AssertEquals("Sequence", 1, task.P9_Sequence);

			ProcessTask task2 = opp.WorkflowItems.AddNew();
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_GG_AssignedGroup = testGroup.PK;
			AssertEquals("Sequence", 2, task2.P9_Sequence);

			ProcessTask task3 = opp.WorkflowItems.AddNew();
			AssertEquals("Sequence", 3, task3.P9_Sequence);
			AssertEquals("Staff", GlbStaff.CurrentUser.GS_Code, task2.P9_GS_NKAssignedStaffMember);
			AssertEquals("Group", testGroup.PK, task2.P9_GG_AssignedGroup);
		}

		public void TestAreTasksCompanySpecific()
		{
			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			DummyWorkflowDescriptor.Instance.SetAreTasksCompanySpecific(true);
			Assert(dummy.WorkflowItems.AreTasksCompanySpecific);

			DummyWorkflowDescriptor.Instance.SetAreTasksCompanySpecific(false);
			Assert(!dummy.WorkflowItems.AreTasksCompanySpecific);
		}

		public void TestLoadWithoutParent()
		{
			List<DummyProcessTask> developersTasks = new List<DummyProcessTask>();
			foreach (var activeCompany in GlbCompany.GetActiveCompanies())
			{
				using (DisposableEnvironment.ForBranch(activeCompany.FirstActiveBranch.PK.ToGuid()))
				{
					DummyProcessTask dummyTask = Factory.New<DummyProcessTask>();
					dummyTask.P9_GS_NKAssignedStaffMember = "C";
					developersTasks.Add(dummyTask);
				}
			}

			DummyProcessTask anotherTask = Factory.New<DummyProcessTask>();
			anotherTask.P9_GS_NKAssignedStaffMember = "ZZ";

			ProcessTaskCollection collection = new ProcessTaskCollection(Factory);
			collection.Load(new ZQuery(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, "C"));
			CombineAssertions(delegate
			{
				Assert("Sanity check, ensure that there is more than one active company", developersTasks.Count > 1);
				AssertEquals(developersTasks.Count, collection.Count);
				AssertContainsExactElementsInAnyOrder(developersTasks, collection);
			});
		}

		#region HasTasks / HasMilestones / HasWorkflowTriggers

		public void TestHasTasks()
		{
			AssertEquals(false, Collection.HasTasks());
			AssertEquals(true, Collection.Contains(Task));
			AssertEquals(true, Collection.HasTasks());
		}

		public void TestHasMilestones()
		{
			AssertEquals(false, Collection.HasMilestones());
			AssertEquals(true, Collection.Contains(Milestone));
			AssertEquals(true, Collection.HasMilestones());
		}

		public void TestHasWorkflowTriggers()
		{
			AssertEquals(false, Collection.HasWorkflowTriggers());
			AssertEquals(true, Collection.Contains(WorkflowTrigger));
			AssertEquals(true, Collection.HasWorkflowTriggers());
		}

		#endregion

		#region Relationship

		public void TestRelationshipFilter()
		{
			DummyWorkflowDescriptor.Instance.SetAreTasksCompanySpecific(true);
			ProcessTask task = Dummy.WorkflowItems.Tasks.AddNew();
			ProcessTask milestone = Dummy.WorkflowItems.Milestones.AddNew();
			AssertEquals(Dummy.PK, task.P9_ParentID);
			AssertEquals(Dummy.PK, milestone.P9_ParentID);

			Factory.Save();
			ProcessTaskCollection newCollection = new ProcessTaskCollection(Dummy);
			newCollection.Load();

			AssertEquals(2, newCollection.Count);
			AssertCollectionContains(task, newCollection);
			AssertCollectionContains(milestone, newCollection);
		}

		public void TestSetCollectionRelationship_ForMilestones()
		{
			ProcessTask milestone = Dummy.WorkflowItems.Milestones.AddNew();
			AssertEquals("Relationship is set", true, milestone.IsMilestone);
			AssertEquals("Milestones are attached to the parent", Dummy.PK, milestone.P9_ParentID);
			AssertEquals("Milestones are attached to the parent", DummyBizoSchema.Constants.Prefix, milestone.P9_ParentTableCode);
		}

		public void TestCompletionMilestoneCodeDescriptionPairList()
		{
			var completionMilestoneCodeDescriptionPairList = Dummy.WorkflowItems.CompletionMilestoneCodeDescriptionPairList;

			AssertEquals(0, completionMilestoneCodeDescriptionPairList.Count);

			var milestone1 = Dummy.WorkflowItems.Milestones.AddNew();
			var milestone2 = Dummy.WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = Events.EstimatedDateChangedCode;
			var milestone3 = Dummy.WorkflowItems.Milestones.AddNew();
			milestone3.TriggerConditions.TriggerEventCode = Events.ArrivalDocumentationReceivedCode;
			milestone3.P9_Description = "Mile";

			completionMilestoneCodeDescriptionPairList = Dummy.WorkflowItems.CompletionMilestoneCodeDescriptionPairList;

			var actual = completionMilestoneCodeDescriptionPairList
				.ToArray()
				.Select(cd => (new Guid(cd.PK.ToString()), cd.Code, cd.Description))
				.ToArray();

			var expected = new (Guid, string, string)[]
			{
				(milestone1.PK.ToGuid(), " -", ZString.Empty),
				(milestone2.PK.ToGuid(), "EST -", milestone2.P9_Description),
				(milestone3.PK.ToGuid(), "ADR - Mile", milestone3.P9_Description),
				(Guid.Empty, string.Empty, string.Empty)
			}.ToArray();

			AssertContainsExactElementsInExactOrder(expected, actual);
		}

		public void TestSetCollectionRelationship_ForTriggers()
		{
			ProcessTask trigger = Dummy.WorkflowItems.Triggers.AddNew();
			AssertEquals("Relationship is set", true, trigger.IsWorkflowTrigger);
			AssertEquals("Triggers are attached to the parent", Dummy.PK, trigger.P9_ParentID);
			AssertEquals("Triggers are attached to the parent", DummyBizoSchema.Constants.Prefix, trigger.P9_ParentTableCode);
		}

		public void TestSetCollectionRelationship_ForTasks()
		{
			DummyWorkflowDescriptor.Instance.SetAreTasksCompanySpecific(true);
			ProcessTask task = Dummy.WorkflowItems.Tasks.AddNew();
			AssertEquals("Relationship is set", true, task.IsTask);
			AssertEquals(Dummy.PK, task.P9_ParentID);
			AssertEquals(Dummy.TablePrefix, task.P9_ParentTableCode);
		}

		public void TestSetCollectionRelationship_NonCompanySpecificTasks()
		{
			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			DummyWorkflowDescriptor.Instance.SetAreTasksCompanySpecific(false);

			ProcessTask task = dummy.WorkflowItems.AddNew();
			AssertEquals("Task not attached to JobHeader", DummyBizoSchema.Constants.Prefix, task.P9_ParentTableCode);
		}

		public void TestLoad_ShouldNotIncludeTasksFromOtherCompanies()
		{
			DummyWorkflowDescriptor.Instance.SetAreTasksCompanySpecific(true);
			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			Factory.Save();

			var activeCompanies = GlbCompany.GetActiveCompanies();
			foreach (var company in activeCompanies)
			{
				using (DisposableEnvironment.ForBranch(company.FirstActiveBranch.PK.ToGuid()))
				{
					BusinessObjectFactory newFactory = new BusinessObjectFactory();
					var dummyInNewFactory = newFactory.Load<DummyWithWorkflow>(dummy.PK);
					var task = dummyInNewFactory.WorkflowItems.Tasks.AddNew();
					task.P9_Description = company.GC_Code;
					var milestone = dummyInNewFactory.WorkflowItems.Milestones.AddNew();
					var trigger = dummyInNewFactory.WorkflowItems.Triggers.AddNew();
					var exception = dummyInNewFactory.WorkflowItems.Exceptions.AddNew();
					newFactory.Save();
				}
			}

			var sharedTask = new BusinessObjectFactory().New<DummyProcessTask>();
			sharedTask.P9_ParentID = dummy.PK;
			sharedTask.P9_ParentTableCode = "Z0";
			sharedTask.P9_ShareTasksForAllCompanies = true;
			sharedTask.P9_Description = "Shared";
			sharedTask.Factory.Save();

			foreach (var company in activeCompanies)
			{
				using (DisposableEnvironment.ForBranch(company.FirstActiveBranch.PK.ToGuid()))
				{
					BusinessObjectFactory newFactory = new BusinessObjectFactory();
					DummyWithWorkflow dummyInNewFactory = newFactory.Load<DummyWithWorkflow>(dummy.PK);
					AssertEquals("include all milestones / triggers / exceptions but only include tasks from the current company (1) and shared (1)", activeCompanies.Length * 3 + 2, dummyInNewFactory.WorkflowItems.Count);
					AssertEquals(2, dummyInNewFactory.WorkflowItems.Tasks.Count);
					Assert("Includes company task", dummyInNewFactory.WorkflowItems.Tasks.Cast<ProcessTask>().Any(t => t.P9_Description == company.GC_Code));
					Assert("Includes shared task", dummyInNewFactory.WorkflowItems.Tasks.Cast<ProcessTask>().Any(t => t.PK == sharedTask.PK));
					AssertEquals(activeCompanies.Length, dummyInNewFactory.WorkflowItems.Milestones.Count);
					AssertEquals(activeCompanies.Length, dummyInNewFactory.WorkflowItems.Triggers.Count);
					AssertEquals(activeCompanies.Length, dummyInNewFactory.WorkflowItems.Exceptions.Count);
				}
			}
		}

		#endregion

		#region IWorkflowProvider

		public void TestTasks()
		{
			AssertEquals("TaskCollection", Collection, ((IWorkflowProvider)Collection).WorkflowItems);
		}

		#endregion

		public void TestAllTasksClosedOrCancelled()
		{
			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			ProcessTaskCollection collection = dummy.WorkflowItems;
			Assert(!collection.AllTasksCancelled);
			Assert(!collection.AllTasksClosedOrCancelled);

			ProcessTask task1 = collection.Tasks.AddNew();
			ProcessTask task2 = collection.Tasks.AddNew();
			ProcessTask task3 = collection.Tasks.AddNew();
			ProcessTask trigger = collection.Triggers.AddNew();
			Assert(!collection.AllTasksCancelled);
			Assert(!collection.AllTasksClosedOrCancelled);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			trigger.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			Assert(collection.AllTasksCancelled);
			Assert(collection.AllTasksClosedOrCancelled);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Assert(!collection.AllTasksCancelled);
			Assert(collection.AllTasksClosedOrCancelled);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Assert(!collection.AllTasksCancelled);
			Assert(collection.AllTasksClosedOrCancelled);
		}

		public void TestAnyTaskIsAssigned()
		{
			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			ProcessTaskCollection collection = dummy.WorkflowItems;
			Assert(!collection.AnyTaskIsAssigned);

			ProcessTask task = collection.Tasks.AddNew();
			ProcessTask trigger = collection.Triggers.AddNew();
			task.P9_GS_NKAssignedStaffMember = "";
			trigger.P9_GS_NKAssignedStaffMember = "";
			Assert(!collection.AnyTaskIsAssigned);

			trigger.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			Assert(!collection.AnyTaskIsAssigned);

			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			Assert(collection.AnyTaskIsAssigned);
		}

		public void TestGetWorkflows_ShouldNotThrowException()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			var system = helper.CreateSystem(Factory, "DUM");

			var job = (IWorkflowProvider)Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory);
			var defaultWorkflow = job.Workflows[0];
			var workflowA = helper.CreateWorkflow(jobHeader, "WorkFlow A");
			var workflowB = helper.CreateWorkflow(jobHeader, "WorkFlow B");
			var workflowC = helper.CreateWorkflow(jobHeader, "WorkFlow C");

			var taskA = helper.CreateTask(workflowA, "GNA", 60, "UDF", "ASN", 1, "My task A");
			var taskB = helper.CreateTask(workflowA, "GNA", 60, "UDF", "ASN", 2, "My task B");
			var taskC = helper.CreateTask(workflowB, "GNA", 60, "UDF", "ASN", 3, "My task C");

			Factory.Save();

			var workflows = job.WorkflowItems.Workflows;

			AssertContainsExactElementsInAnyOrder(new[] { defaultWorkflow, workflowA, workflowB, workflowC }, workflows);
		}

		#region TestIsUserDefinedConditionMet

		public void TestIsCondition2Met_WithEnvMetMcrCondition_ReturnsTrue()
		{
			var workflowItem = Dummy.WorkflowItems.Milestones.AddNew();
			workflowItem.IsMilestone = true;
			workflowItem.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.MacroCondition;
			workflowItem.TemplateConditions.TemplateCondition2Value = "@env.CurrentUser.Name != \"\"";

			Assert(Dummy.WorkflowItems.IsCondition2Met(workflowItem));
		}

		public void TestIsCondition2Met_WithMetMcrCondition_ReturnsTrue()
		{
			var workflowItem = Dummy.WorkflowItems.Milestones.AddNew();
			workflowItem.IsMilestone = true;
			workflowItem.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.MacroCondition;
			workflowItem.TemplateConditions.TemplateCondition2Value = "Z0_Code == \"XYZ\"";

			Dummy.Z0_Code = "XYZ";
			Assert(Dummy.WorkflowItems.IsCondition2Met(workflowItem));
		}

		public void TestIsCondition2Met_WithUnmetMcrCondition_ReturnsFalse()
		{
			var workflowItem = Dummy.WorkflowItems.Milestones.AddNew();
			workflowItem.IsMilestone = true;
			workflowItem.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.MacroCondition;
			workflowItem.TemplateConditions.TemplateCondition2Value = "Z0_Code == \"XYZ\"";
						
			Assert(!Dummy.WorkflowItems.IsCondition2Met(workflowItem));
		}

		public void TestIsUserDefinedConditionMet()
		{
			ProcessTask milestone = Dummy.WorkflowItems.Milestones.AddNew();
			milestone.IsMilestone = true;
			milestone.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			milestone.TemplateConditions.TemplateCondition2Value = "\"<Z0_Code>\" == \"XYZ\"";

			Dummy.Z0_Code = "ABC";
			Assert(!Dummy.WorkflowItems.IsCondition2Met(milestone));
			Assert(!Dummy.WorkflowItems.IsCondition2Met(milestone));

			Dummy.Z0_Code = "XYZ";
			Assert("Should not calculate UDF without ProcessTask provided", !Dummy.WorkflowItems.IsCondition2Met(milestone));
		}

		public void TestIsUserDefinedConditionMet_OneQuote()
		{
			var milestone = Dummy.WorkflowItems.Milestones.AddNew();
			milestone.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			milestone.TemplateConditions.TemplateCondition2Value = "<If(\" < Left(\"<GetCustomField(Service Code)>\", 1) > \"==\"A\")>\"";

			AssertEquals(false, Dummy.WorkflowItems.IsCondition2Met(milestone));
		}

		public void TestUdfConditionNotMetWhenEmpty()
		{
			var template = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);
			var task = template.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "TASK";
			task.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			task.TemplateConditions.TemplateCondition2Value = "";

			AssertEquals(false, Dummy.WorkflowItems.IsCondition2Met(task));

			task.TemplateConditions.TemplateCondition2Value = "\"1\"==\"1\"";
			AssertEquals(true, Dummy.WorkflowItems.IsCondition2Met(task));
		}

		public void TestIsUserDefinedConditionMet_UtilisingCache_ForMultipleJobs()
		{
			var template = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);

			var trigger1 = (ITemplateTrigger)template.TemplateTriggers.AddNew();
			var trigger2 = (ITemplateTrigger)template.TemplateTriggers.AddNew();

			trigger1.Description = trigger2.Description = "Gush";
			trigger1.TriggerEventCode = trigger2.TriggerEventCode = Events.TagWasAddedOrRemovedCode;

			trigger1.TemplateCondition2 = trigger2.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			trigger1.TemplateCondition2Value = "\"<Z0_Code>\" == \"GUD\"";
			trigger2.TemplateCondition2Value = "\"<Z0_Code>\" == \"GUY\"";

			Factory.Save();

			var job1 = Factory.New<DummyWithWorkflow>();
			var job2 = Factory.New<DummyWithWorkflow>();

			job1.Z0_Code = "GUD";
			job2.Z0_Code = "GUY";

			AssertEquals(1, job1.WorkflowItems.TriggersIncludingRelated.Count);
			AssertEquals(1, job2.WorkflowItems.TriggersIncludingRelated.Count);

			AssertEquals(trigger1.Identifier, job1.WorkflowItems.TriggersIncludingRelated[0].P9_ParentTemplateID);
			AssertEquals(trigger2.Identifier, job2.WorkflowItems.TriggersIncludingRelated[0].P9_ParentTemplateID);

			job1.Z0_Code = "GUY";
			job2.Z0_Code = "GUD";

			job1.WorkflowItems.TriggersIncludingRelated.Rebuild();
			job2.WorkflowItems.TriggersIncludingRelated.Rebuild();

			AssertEquals(1, job1.WorkflowItems.TriggersIncludingRelated.Count);
			AssertEquals(1, job2.WorkflowItems.TriggersIncludingRelated.Count);

			AssertEquals("Cache hasn't been cleared yet, so cached conditions applicability still used", trigger1.Identifier, job1.WorkflowItems.TriggersIncludingRelated[0].P9_ParentTemplateID);
			AssertEquals("Cache hasn't been cleared yet, so cached conditions applicability still used", trigger2.Identifier, job2.WorkflowItems.TriggersIncludingRelated[0].P9_ParentTemplateID);

			Factory.Save();

			job1.WorkflowItems.TriggersIncludingRelated.Rebuild();
			job2.WorkflowItems.TriggersIncludingRelated.Rebuild();

			AssertEquals(1, job1.WorkflowItems.TriggersIncludingRelated.Count);
			AssertEquals(1, job2.WorkflowItems.TriggersIncludingRelated.Count);

			AssertEquals("Cache has been cleared, so cached conditions applicability updated and triggers have now switched", trigger2.Identifier, job1.WorkflowItems.TriggersIncludingRelated[0].P9_ParentTemplateID);
			AssertEquals("Cache has been cleared, so cached conditions applicability updated and triggers have now switched", trigger1.Identifier, job2.WorkflowItems.TriggersIncludingRelated[0].P9_ParentTemplateID);
		}

		#endregion

		public void TestUniversalTriggersDoNotSetHasChanges()
		{
			var template = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);

			var trigger1 = (ITemplateTrigger)template.TemplateTriggers.AddNew();
			trigger1.Description = "Gush";
			trigger1.TriggerEventCode = Events.CustomisableEvent00Code;
			trigger1.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			trigger1.TemplateCondition2Value = "\"<Z0_Code>\" == \"ONE\"";

			var job1 = Factory.New<DummyWithWorkflow>();

			Factory.Save();

			job1.Z0_Code = "ONE";
			Factory.Save();
			AssertEquals("Adding a universal trigger should not set has changes after the job has just been saved", false, job1.HasChanges);
			AssertEquals(1, job1.WorkflowItems.TriggersIncludingRelated.Count);

			job1.Z0_Code = "";
			Factory.Save();
			AssertEquals("Removing a universal trigger should not set has changes after the job has just been saved", false, job1.HasChanges);
			AssertEquals(0, job1.WorkflowItems.TriggersIncludingRelated.Count);
		}

		public void TestSetCollectionDefaultsForNewChild()
		{
			var dummyTask1 = Factory.NewWithValidTestData<ProcessTask>();
			var dummyTask2 = Factory.NewWithValidTestData<ProcessTask>();

			var dummyMilestone1 = Factory.NewWithValidTestData<ProcessTask>();
			var dummyMilestone2 = Factory.NewWithValidTestData<ProcessTask>();
			dummyMilestone1.P9_Type = Core.Constants.Workflow.MilestoneType;
			dummyMilestone1.P9_Description = "ARV";
			dummyMilestone1.TriggerConditions.TriggerEventCode = "ARV";
			dummyMilestone2.P9_Type = Core.Constants.Workflow.MilestoneType;
			dummyMilestone2.P9_Description = "DEP";
			dummyMilestone2.TriggerConditions.TriggerEventCode = "DEP";

			var dummyException1 = Factory.NewWithValidTestData<ProcessTask>();
			var dummyException2 = Factory.NewWithValidTestData<ProcessTask>();
			dummyException1.P9_Type = Core.Constants.Workflow.ExceptionType;
			dummyException1.P9_Description = "ARV";
			dummyException1.TriggerConditions.TriggerEventCode = "ARV";
			dummyException2.P9_Type = Core.Constants.Workflow.ExceptionType;
			dummyException2.P9_Description = "DEP";
			dummyException2.TriggerConditions.TriggerEventCode = "DEP";

			var dummyTrigger1 = Factory.NewWithValidTestData<ProcessTask>();
			var dummyTrigger2 = Factory.NewWithValidTestData<ProcessTask>();
			dummyTrigger1.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			dummyTrigger2.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;

			GlbStaff dummyStaff = Factory.New<GlbStaff>();
			dummyStaff.GS_IsActive = true;
			dummyStaff.GS_Code = "DEA";
			GlbStaff dummyStaff2 = Factory.New<GlbStaff>();
			dummyStaff2.GS_IsActive = true;
			dummyStaff2.GS_Code = "ZEB";
			GlbStaff dummyStaff3 = Factory.New<GlbStaff>();
			dummyStaff3.GS_IsActive = true;
			dummyStaff3.GS_Code = "VXA";

			GlbGroup dummyGroup = Factory.New<GlbGroup>();
			dummyGroup.GG_IsActive = true;
			GlbGroup dummyGroup2 = Factory.New<GlbGroup>();
			dummyGroup2.GG_IsActive = true;
			GlbGroup dummyGroup3 = Factory.New<GlbGroup>();
			dummyGroup3.GG_IsActive = true;

			dummyTask1.P9_GS_NKAssignedStaffMember = dummyStaff.GS_Code;
			dummyTask1.P9_GG_AssignedGroup = dummyGroup.PK;

			//Set defaults with nothing in collection
			AssertEquals("Pre", 0, Collection.Count);
			ProcessTaskCollection.SetCollectionDefaultsForNewChild(Collection, dummyTask2, true);
			AssertNotEquals("Previous task details NOT copied", dummyTask1.P9_GS_NKAssignedStaffMember, dummyTask2.P9_GS_NKAssignedStaffMember);
			AssertNotEquals("Previous task details NOT copied", dummyTask1.P9_GG_AssignedGroup, dummyTask2.P9_GG_AssignedGroup);

			//Add 'previous' task to collection and reset defaults
			Collection.Add(dummyTask1);
			AssertEquals("Pre", 1, Collection.Count);
			ProcessTaskCollection.SetCollectionDefaultsForNewChild(Collection, dummyTask2, true);
			AssertEquals("Previous task details WERE copied", dummyTask1.P9_GS_NKAssignedStaffMember, dummyTask2.P9_GS_NKAssignedStaffMember);
			AssertEquals("Previous task details WERE copied", dummyTask1.P9_GG_AssignedGroup, dummyTask2.P9_GG_AssignedGroup);

			//Details not copied when argument flag is false
			ProcessTaskCollection.SetCollectionDefaultsForNewChild(Collection, dummyTask2, false);
			AssertEquals("Previous task details WERE copied", dummyTask1.P9_GS_NKAssignedStaffMember, dummyTask2.P9_GS_NKAssignedStaffMember);
			AssertEquals("Previous task details WERE copied", dummyTask1.P9_GG_AssignedGroup, dummyTask2.P9_GG_AssignedGroup);

			//Milestones should not be defaulted from tasks
			ProcessTaskCollection.SetCollectionDefaultsForNewChild(Collection, dummyMilestone1, true);
			AssertNotEquals("Previous task details NOT copied", dummyTask1.P9_GS_NKAssignedStaffMember, dummyMilestone1.P9_GS_NKAssignedStaffMember);
			AssertNotEquals("Previous task details NOT copied", dummyTask1.P9_GG_AssignedGroup, dummyMilestone1.P9_GG_AssignedGroup);

			//Milestones should default from milestones
			dummyMilestone1.P9_GS_NKAssignedStaffMember = dummyStaff2.GS_Code;
			dummyMilestone1.P9_GG_AssignedGroup = dummyGroup2.PK;
			Collection.Add(dummyMilestone1);
			ProcessTaskCollection.SetCollectionDefaultsForNewChild(Collection, dummyMilestone2, true);
			AssertEquals("Previous task details WERE copied", dummyMilestone1.P9_GS_NKAssignedStaffMember, dummyMilestone2.P9_GS_NKAssignedStaffMember);
			AssertEquals("Previous task details WERE copied", dummyMilestone1.P9_GG_AssignedGroup, dummyMilestone2.P9_GG_AssignedGroup);

			//Exceptions should not be defaulted from exceptions
			dummyException1.P9_GS_NKAssignedStaffMember = dummyStaff3.GS_Code;
			dummyException1.P9_GG_AssignedGroup = dummyGroup3.PK;
			Collection.Add(dummyException1);
			ProcessTaskCollection.SetCollectionDefaultsForNewChild(Collection, dummyException2, true);
			AssertNotEquals("Previous task details NOT copied", dummyException1.P9_GS_NKAssignedStaffMember, dummyException2.P9_GS_NKAssignedStaffMember);
			AssertNotEquals("Previous task details NOT copied", dummyException1.P9_GG_AssignedGroup, dummyException2.P9_GG_AssignedGroup);

			//Exceptions should be defaulted from their respective milestone
			Collection.Add(dummyMilestone2);
			Collection.Add(dummyException2);
			ProcessTaskCollection.SetCollectionDefaultsForNewChild(Collection, dummyException2, true);
			AssertEquals("Previous task details WERE copied", dummyMilestone2.P9_GS_NKAssignedStaffMember, dummyException2.P9_GS_NKAssignedStaffMember);
			AssertEquals("Previous task details WERE copied", dummyMilestone2.P9_GG_AssignedGroup, dummyException2.P9_GG_AssignedGroup);

			//Triggers should default from triggers
			dummyTrigger1.P9_GS_NKAssignedStaffMember = dummyStaff2.GS_Code;
			dummyTrigger1.P9_GG_AssignedGroup = dummyGroup2.PK;
			Collection.Add(dummyTrigger1);
			ProcessTaskCollection.SetCollectionDefaultsForNewChild(Collection, dummyTrigger2, true);
			AssertEquals("Previous task details WERE copied", dummyTrigger1.P9_GS_NKAssignedStaffMember, dummyTrigger2.P9_GS_NKAssignedStaffMember);
			AssertEquals("Previous task details WERE copied", dummyTrigger1.P9_GG_AssignedGroup, dummyTrigger2.P9_GG_AssignedGroup);

			//Deactivate staff and reset defaults
			dummyStaff.GS_IsActive = false;
			dummyTask2 = Factory.NewWithValidTestData<ProcessTask>();
			ProcessTaskCollection.SetCollectionDefaultsForNewChild(Collection, dummyTask2, true);
			AssertNotEquals("Previous task details NOT copied", dummyTask1.P9_GS_NKAssignedStaffMember, dummyTask2.P9_GS_NKAssignedStaffMember);
			AssertEquals("Previous task details WERE copied", dummyTask1.P9_GG_AssignedGroup, dummyTask2.P9_GG_AssignedGroup);

			//Deactivate group and reset defaults
			dummyGroup.GG_IsActive = false;
			dummyTask2 = Factory.NewWithValidTestData<ProcessTask>();
			ProcessTaskCollection.SetCollectionDefaultsForNewChild(Collection, dummyTask2, true);
			AssertNotEquals("Previous task details NOT copied", dummyTask1.P9_GS_NKAssignedStaffMember, dummyTask2.P9_GS_NKAssignedStaffMember);
			AssertNotEquals("Previous task details NOT copied", dummyTask1.P9_GG_AssignedGroup, dummyTask2.P9_GG_AssignedGroup);
		}

		#region CurrentTask

		public void TestCurrentTask()
		{
			var header1 = Factory.New<IProcessHeader>();
			var header2 = Factory.New<IProcessHeader>();
			var header3 = Factory.New<IProcessHeader>();

			var task1a = Factory.New<ProcessTask>();
			task1a.P9_FH_ProcessHeader = header1.PK;
			var task1b = Factory.New<ProcessTask>();
			task1b.P9_FH_ProcessHeader = header1.PK;
			var task1c = Factory.New<ProcessTask>();
			task1c.P9_FH_ProcessHeader = header1.PK;
			var task3a = Factory.New<ProcessTask>();
			task3a.P9_FH_ProcessHeader = header3.PK;

			Collection.AddRange(new[] { task1a, task1b, task1c, task3a });

			task1a.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1b.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1c.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task3a.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1a.P9_Sequence = 4;
			task1b.P9_Sequence = 3;
			task1c.P9_Sequence = 1;
			task3a.P9_Sequence = 2;

			AssertNull(Collection.GetCurrentTask(header2));
			AssertEquals(task1c, Collection.GetCurrentTask());
			AssertEquals(task1c, Collection.GetCurrentTask(header1));

			task1c.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(task3a, Collection.GetCurrentTask());
			AssertEquals(task1b, Collection.GetCurrentTask(header1));

			task1b.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(task3a, Collection.GetCurrentTask());
			AssertEquals(task1a, Collection.GetCurrentTask(header1));

			task1a.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals(task3a, Collection.GetCurrentTask());
			AssertEquals(task1a, Collection.GetCurrentTask(header1));

			task1a.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			AssertEquals(task3a, Collection.GetCurrentTask());
			AssertEquals(task1a, Collection.GetCurrentTask(header1));

			task1a.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(task3a, Collection.GetCurrentTask());
			AssertNull(Collection.GetCurrentTask(header1));
		}

		#endregion

		#region Data refresh

		public void TestApplyingTemplateTask_OnSameJob_OnDifferentInstances()
		{
			WorkflowDataRegistry.Instance.EnableTemplateApplicationConcurrencyProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TemplateApplicationRaceConditionHandlerOptions.Codes.UserInterfaceAndServiceTasks);

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.CampaignWorkflowDescriptorCode;
			var templateTask = template.WorkflowItems.AddNew();
			templateTask.P9_Description = "task from template";
			templateTask.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTask.TemplateConditions.TemplateCondition2Value = "\"<G0_CampaignName>\" == \"ABC\"";

			Factory.Save();

			var campaign = Factory.New<IGlbCompanyCampaign>();
			campaign.G0_CampaignName = ZGuid.NewZGuid().ToString();
			campaign.G0_CampaignID = "42";
			var campaignAsWorkflowProvider = campaign as IWorkflowProvider;
			var campaignTask = campaignAsWorkflowProvider.WorkflowItems.AddNew();
			campaignTask.P9_Description = "Manually created task";

			Factory.Save();

			AssertEquals("GIVEN template-task condition2 hasn't met, no template-task applied i.e. 1 manually created task.", 1, campaignAsWorkflowProvider.WorkflowItems.Count);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false, NameForDebugging = "factory2" }; // imitate different application instances: no refresh

			var campaignInFactory2 = factory2.Load<IGlbCompanyCampaign>(campaign.PK);
			var campaignInFactory2AsWorkflowProvider = campaignInFactory2 as IWorkflowProvider;
			AssertEquals("GIVEN template-task condition2 hasn't met, no template-task applied i.e. 1 manually created task.", 1, campaignInFactory2AsWorkflowProvider.WorkflowItems.Count);

			campaign.G0_CampaignName = "ABC";

			var internalFactory2 = factory2 as IBusinessObjectFactoryInternals;
			var originalValue = internalFactory2.DisableQueryCacheReset;
			internalFactory2.DisableQueryCacheReset = true; // imitate different application instances: stop Factory.Save() from clearing factory2.QueryCache
			Factory.Save();
			internalFactory2.DisableQueryCacheReset = originalValue;

			campaignInFactory2.G0_CampaignName = "ABC";
			factory2.Save();

			var factory3 = new BusinessObjectFactory { RefreshEnabled = false, NameForDebugging = "factory3" }; // imitating different app domains - needs to be new factory - same factory may have caching
			var campaignInFactory3AsWorkflowProvider = factory3.Load<IGlbCompanyCampaign>(campaign.PK) as IWorkflowProvider;

			AssertEquals("GIVEN same job were opened on 2 factories and processes, WHEN template got applied, it should only applied once", 2, campaignInFactory3AsWorkflowProvider.WorkflowItems.Count);

			AssertContainsExactElementsInAnyOrder(
				"GIVEN same job were opened on 2 factories and processes, WHEN template got applied, it should only applied once",
				new[] { "Manually created task", "task from template" },
				campaignInFactory3AsWorkflowProvider.WorkflowItems.Select(item => item.P9_Description.ToString()));
		}

		public void TestApplyingTemplateTask_OnSameJob_OnDifferentInstances_Twice()
		{
			WorkflowDataRegistry.Instance.EnableTemplateApplicationConcurrencyProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TemplateApplicationRaceConditionHandlerOptions.Codes.UserInterfaceAndServiceTasks);

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.CampaignWorkflowDescriptorCode;
			var templateTask = template.WorkflowItems.AddNew();
			templateTask.P9_Description = "task from template";
			templateTask.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTask.TemplateConditions.TemplateCondition2Value = "\"<G0_CampaignName>\" == \"ABC\"";

			Factory.Save();

			var campaign = Factory.New<IGlbCompanyCampaign>();
			campaign.G0_CampaignName = ZGuid.NewZGuid().ToString();
			campaign.G0_CampaignID = "42";
			var campaignAsWorkflowProvider = campaign as IWorkflowProvider;
			var campaignTask = campaignAsWorkflowProvider.WorkflowItems.AddNew();
			campaignTask.P9_Description = "Manually created task";

			Factory.Save();

			AssertEquals("GIVEN template-task condition2 hasn't met, no template-task applied i.e. 1 manually created task.", 1, campaignAsWorkflowProvider.WorkflowItems.Count);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false, NameForDebugging = "factory2" }; // imitate different application instances: no refresh

			var campaignInFactory2 = factory2.Load<IGlbCompanyCampaign>(campaign.PK);

			var campaignInFactory2AsWorkflowProvider = campaignInFactory2 as IWorkflowProvider;
			AssertEquals("GIVEN template-task condition2 hasn't met, no template-task applied i.e. 1 manually created task.", 1, campaignInFactory2AsWorkflowProvider.WorkflowItems.Count);

			campaign.G0_CampaignName = "ABC";
			var internalFactory2 = factory2 as IBusinessObjectFactoryInternals;
			var originalValue = internalFactory2.DisableQueryCacheReset;
			internalFactory2.DisableQueryCacheReset = true; // imitate different application instances: stop Factory.Save() from clearing factory2.QueryCache
			Factory.Save();
			internalFactory2.DisableQueryCacheReset = originalValue;

			campaignInFactory2.G0_CampaignName = "ABC";
			factory2.Save();

			var factory3 = new BusinessObjectFactory { RefreshEnabled = false, NameForDebugging = "factory3" }; // imitating different app domains - needs to be new factory - same factory may have caching
			var campaignInFactory3AsWorkflowProvider = factory3.Load<IGlbCompanyCampaign>(campaign.PK) as IWorkflowProvider;

			AssertEquals("GIVEN same job were opened on 2 factories and processes, WHEN template got applied, it should only applied once", 2, campaignInFactory3AsWorkflowProvider.WorkflowItems.Count);
			AssertContainsExactElementsInAnyOrder(
				"GIVEN same job were opened on 2 factories and processes, WHEN template got applied, it should only applied once",
				new[] { "Manually created task", "task from template" },
				campaignInFactory3AsWorkflowProvider.WorkflowItems.Select(item => item.P9_Description.ToString()));

			// 2nd sequence
			campaign.G0_CampaignName = "Not Triggering Template";
			campaignAsWorkflowProvider.WorkflowItems.First(x => ((ProcessTask)x).P9_Description == "task from template").Delete();
			Factory.Save();

			var factory4 = new BusinessObjectFactory { RefreshEnabled = false, NameForDebugging = "factory4" }; // imitate different application instances: no refresh
			var campaignInFactory4 = factory4.Load<IGlbCompanyCampaign>(campaign.PK);

			var campaignInFactory4AsWorkflowProvider = campaignInFactory4 as IWorkflowProvider;
			AssertEquals("GIVEN template-task condition2 hasn't met, no template-task applied i.e. 1 manually created task.", 1, campaignInFactory4AsWorkflowProvider.WorkflowItems.Count);

			campaign.G0_CampaignName = "ABC";

			var internalFactory4 = factory4 as IBusinessObjectFactoryInternals;
			originalValue = internalFactory2.DisableQueryCacheReset;
			internalFactory4.DisableQueryCacheReset = true; // imitate different application instances: stop Factory.Save() from clearing factory2.QueryCache
			Factory.Save();
			internalFactory4.DisableQueryCacheReset = originalValue;

			campaignInFactory4.G0_CampaignName = "ABC";
			factory4.Save();

			var factory5 = new BusinessObjectFactory { RefreshEnabled = false, NameForDebugging = "factory5" }; // imitating different app domains - needs to be new factory - same factory may have caching
			var campaignInFactory5AsWorkflowProvider = factory5.Load<IGlbCompanyCampaign>(campaign.PK) as IWorkflowProvider;

			AssertEquals("GIVEN same job were opened on 2 factories and processes, WHEN template got applied, it should only applied once", 2, campaignInFactory5AsWorkflowProvider.WorkflowItems.Count);
			AssertContainsExactElementsInAnyOrder(
				"GIVEN same job were opened on 2 factories and processes, WHEN template got applied, it should only applied once",
				new[] { "Manually created task", "task from template" },
				campaignInFactory5AsWorkflowProvider.WorkflowItems.Select(item => item.P9_Description.ToString()));
		}

		public void TestApplyingTemplateTask_OnSameJob_AfterTheTaskWasDeleted()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.CampaignWorkflowDescriptorCode;

			var templateTask1 = template.WorkflowItems.AddNew();
			templateTask1.P9_Description = "task 1 from template";
			templateTask1.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTask1.TemplateConditions.TemplateCondition2Value = "\"<G0_CampaignName>\" == \"ABC\"";

			var templateTask2 = template.WorkflowItems.AddNew();
			templateTask2.P9_Description = "task 2 from template";
			templateTask2.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTask2.TemplateConditions.TemplateCondition2Value = "\"<G0_CampaignName>\" == \"ABC\"";

			Factory.Save();

			var campaign = Factory.New<IGlbCompanyCampaign>();
			campaign.G0_CampaignName = "ABC";
			Factory.Save();

			var campaignAsWorkflowProvider = campaign as IWorkflowProvider;

			AssertEquals("GIVEN template-task condition2 has met, template-task applied", 2, campaignAsWorkflowProvider.WorkflowItems.Count);

			templateTask1.P9_Description = "task 1 from template - updated";
			templateTask2.Delete();
			Factory.Save();

			campaignAsWorkflowProvider.WorkflowItems.DeleteAll();

			Factory.Save();

			AssertEquals("GIVEN template-task condition2 has met and template-task was deleted, 1 updated-template-task applied", 1, campaignAsWorkflowProvider.WorkflowItems.Count);
			AssertEquals("GIVEN template-task condition2 has met and template-task was deleted, 1 updated-template-task applied", "task 1 from template - updated", campaignAsWorkflowProvider.WorkflowItems[0].P9_Description);
		}

		public void TestApplyingTemplateTask_OnSameJob_AfterSameTaskGotDeleted_ThenResaving()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.CampaignWorkflowDescriptorCode;

			var templateTask = template.WorkflowItems.AddNew();
			templateTask.P9_Description = "task from template";
			templateTask.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTask.TemplateConditions.TemplateCondition2Value = "\"<G0_CampaignName>\" == \"ABC\"";

			Factory.Save();

			var campaign = Factory.New<IGlbCompanyCampaign>();
			campaign.G0_CampaignName = "ABC";
			Factory.Save();

			var campaignAsWorkflowProvider = campaign as IWorkflowProvider;

			AssertEquals("GIVEN template-task condition2 has met, template-task applied", 1, campaignAsWorkflowProvider.WorkflowItems.Count);

			campaignAsWorkflowProvider.WorkflowItems[0].Delete();
			Factory.Save();

			AssertEquals("GIVEN template-task condition2 has met and template-task was deleted, 1 template-task applied", 1, campaignAsWorkflowProvider.WorkflowItems.Count);

			campaign.G0_CampaignName = "Campaign 1";
			Factory.Save();

			AssertEquals("GIVEN template-task condition2 has met and template-task was deleted and saved and re-saving, template-task should not be applied", 1, campaignAsWorkflowProvider.WorkflowItems.Count);
		}

		public void TestReloadAfterTaskIsDeleted()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var dummy = factory.New<DummyWithWorkflow>();
			dummy.WorkflowItems.Tasks.AddNew();

			factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var dummy2 = factory2.Load<DummyWithWorkflow>(dummy.PK);

			dummy2.WorkflowItems.Tasks.AddNew();
			dummy2.WorkflowItems.AddView(dummy2.WorkflowItems.Tasks);
			dummy.WorkflowItems[0].Delete();
			factory.Save();

			AssertEquals("Precondition: Dummy has 2 workflow item", 2, dummy2.WorkflowItems.Count);
			factory2.Save();
			AssertEquals("Dummy has 1 workflow item after reload since 1 was deleted in the DB", 1, dummy2.WorkflowItems.Count);
		}

		public void TestViewsCollectionModificationDuringRebuild()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var dummy = factory.New<DummyWithWorkflow>();
			dummy.WorkflowItems.Tasks.AddNew();

			dummy.WorkflowItems.AddView(dummy.WorkflowItems.Tasks);
			var added = false;
			dummy.WorkflowItems.Tasks.OnRebuild += (sender, args) =>
			{
				if (!added)
				{
					dummy.WorkflowItems.AddView(dummy.WorkflowItems.Tasks);
					added = true;
				}
			};

			AssertNoExceptionThrown(() => factory.Save());
		}

		public void TestReloadAfterTaskIsDeleted_SaveFailed()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var dummy = factory.New<DummyWithWorkflow>();
			dummy.WorkflowItems.Tasks.AddNew();

			factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var dummy2 = factory2.Load<DummyWithWorkflow>(dummy.PK);
			dummy2.WorkflowItems.Tasks.AddNew();
			dummy2.WorkflowItems.AddView(dummy2.WorkflowItems.Tasks);

			dummy.WorkflowItems[0].Delete();
			var deletedTask = dummy2.WorkflowItems[0];
			deletedTask.P9_Description = "Something Different";

			factory.Save();
			AssertEquals("Precondition: Dummy has 2 workflow item", 2, dummy2.WorkflowItems.Count);
			AssertExceptionThrown<ZSaveConcurrencyException>(factory2.Save);
			AssertEquals("Only reload on successful saves to avoid losing uncommitted data, just let normal concurrency handling handle this.", 2, dummy2.WorkflowItems.Count);
		}

		public void TestDBHitsTest_ApplyingTemplateTask_OnCreatedJob()
		{
			var factory = new BusinessObjectFactory();

			var expectedHits = new Dictionary<string, int>
			{
				{ ProcessTasksSchema.Constants.TableName, 0 }, // no db-hit because no load
			};

			using (AssertDbHitsForAllFactories(expectedHits, true, true, 20, f => f != factory))
			{
				var template = factory.NewWithValidTestData<ProcessTaskTemplate>();
				template.P0_ProcessType = WorkflowDescriptors.CampaignWorkflowDescriptorCode;

				for (var i = 0; i < 10; i++)
				{
					var templateTask = template.WorkflowItems.AddNew();
					templateTask.P9_Description = string.Format("task {0} from template", i);
					templateTask.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
					templateTask.TemplateConditions.TemplateCondition2Value = "\"<G0_CampaignName>\" == \"ABC\"";
				}

				factory.Save();

				var campaign = factory.New<IGlbCompanyCampaign>();
				campaign.G0_CampaignName = "ABC - Template Not applied yet";
				factory.Save();

				var campaignAsWorkflowProvider = campaign as IWorkflowProvider;
				AssertEquals("GIVEN template-task condition2 has not met, no template-task applied", 0, campaignAsWorkflowProvider.WorkflowItems.Count);

				campaign.G0_CampaignName = "ABC";
				factory.Save();

				AssertEquals("GIVEN template-task condition2 has met, template-task applied", 10, campaignAsWorkflowProvider.WorkflowItems.Count);
			}
		}

		public void TestDBHitsTest_ApplyingTemplateTask_OnLoadedJob()
		{
			var factory = new BusinessObjectFactory();

			var expectedHits = new Dictionary<string, int>
			{
				{ ProcessTasksSchema.Constants.TableName, 2 }, //Reload WorkflowItems disabled during save when auto merge enable.
			};

			var template = factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.CampaignWorkflowDescriptorCode;

			for (var i = 0; i < 3; i++)
			{
				var templateTask = template.WorkflowItems.Tasks.AddNew();
				templateTask.P9_Description = string.Format("task {0} from template", i);
				templateTask.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
				templateTask.TemplateConditions.TemplateCondition2Value = "\"<G0_CampaignName>\" == \"ABC\"";
			}

			var events = new string[] { Events.AuthorisedCode, Events.ArrivalCode, Events.DepartureCode };

			for (var j = 0; j < 3; j++)
			{
				var templateMilestone = template.WorkflowItems.Milestones.AddNew();
				templateMilestone.P9_Description = string.Format("milestone {0} from template", j);
				templateMilestone.TriggerConditions.TriggerEventCode = events[j];
				templateMilestone.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
				templateMilestone.TemplateConditions.TemplateCondition2Value = "\"<G0_CampaignName>\" == \"ABC\"";
			}

			for (var k = 0; k < 3; k++)
			{
				var templateTrigger = template.WorkflowItems.Triggers.AddNew();
				templateTrigger.P9_Description = string.Format("trigger {0} from template", k);
				templateTrigger.TriggerConditions.TriggerEventCode = events[k];
				templateTrigger.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
				templateTrigger.TemplateConditions.TemplateCondition2Value = "\"<G0_CampaignName>\" == \"ABC\"";
			}

			factory.Save();

			var campaign = factory.New<IGlbCompanyCampaign>();
			campaign.G0_CampaignName = ZGuid.NewZGuid().ToString();
			campaign.G0_CampaignID = "42";
			factory.Save();

			using (AssertDbHitsForAllFactories(expectedHits, true, true, 20, f => f != factory))
			{
				var factory2 = new BusinessObjectFactory { RefreshEnabled = false, NameForDebugging = "factory2" }; // imitate different application instances: no refresh

				var campaignInFactory2 = factory2.Load<IGlbCompanyCampaign>(campaign.PK);
				campaignInFactory2.G0_CampaignName = "ABC - Template Not applied yet";

				factory2.Save();

				var campaignAsWorkflowProvider = campaignInFactory2 as IWorkflowProvider;
				AssertEquals("GIVEN template-task condition2 has not met, no template-task applied", 0, campaignAsWorkflowProvider.WorkflowItems.Count);

				campaignInFactory2.G0_CampaignName = "ABC";

				factory2.Save();

				AssertEquals("GIVEN template-task condition2 has met, template-items applied", 9, campaignAsWorkflowProvider.WorkflowItems.Count);

				AssertEquals("GIVEN template-task condition2 has met, template-tasks applied", 3, campaignAsWorkflowProvider.WorkflowItems.Tasks.Count);
				AssertEquals("GIVEN template-task condition2 has met, template-milestones applied", 3, campaignAsWorkflowProvider.WorkflowItems.Milestones.Count);
				AssertEquals("GIVEN template-task condition2 has met, template-triggers applied", 3, campaignAsWorkflowProvider.WorkflowItems.Triggers.Count);
			}
		}

		public void TestBizOCollection_Reload()
		{
			var workflow = GetWorkflow_ForTestBizOCollection_Reload();
			workflow.WorkflowItems.Reload(reLoadExistingRows: true, assumeRowsMissingFromQueryResultsAreDeleted: true);
			AssertEquals("WHEN Reload with reLoadExistingRows, SHOULD get new bizO from db", 5, workflow.WorkflowItems.Count);
			AssertEquals("WHEN Reload with reLoadExistingRows, SHOULD update current bizO",
				"Task 2 - updated on diff-factory-same-inst",
				workflow.WorkflowItems.Cast<ProcessTask>().First(x => x.P9_Sequence == 2).P9_Description);

			workflow = GetWorkflow_ForTestBizOCollection_Reload();
			workflow.WorkflowItems.Reload(reLoadExistingRows: false, assumeRowsMissingFromQueryResultsAreDeleted: true);
			AssertEquals("WHEN Reload with !reLoadExistingRows, SHOULD get new bizO from db", 5, workflow.WorkflowItems.Count);
			AssertEquals("WHEN Reload with !reLoadExistingRows, SHOULD NOT update current bizO",
				"Task 2 @ Factory",
				workflow.WorkflowItems.Cast<ProcessTask>().First(x => x.P9_Sequence == 2).P9_Description);
		}

		public void TestBizOCollection_Reload_DoNotClobberUnsavedRows()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var t = dummy.WorkflowItems.Tasks.AddNew();
			t.P9_Description = "I like chicken";
			t.P9_ParentID = ZGuid.Empty;
			dummy.WorkflowItems.Reload(reLoadExistingRows: true, assumeRowsMissingFromQueryResultsAreDeleted: true);
			AssertEquals(0, dummy.WorkflowItems.Count);
			AssertEquals("It is possible for an object to be transiently part of the wrong collection when it is being first constructed, so we ignore this case.", false, t.IsDeleted);
		}

		public void TestBizOCollection_Reload_OnDelete()
		{
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.WorkflowItems.Tasks.AddNew();
			dummy.WorkflowItems.Tasks.AddNew();
			dummy.WorkflowItems.Milestones.AddNew();
			dummy.WorkflowItems.Milestones.AddNew();
			dummy.WorkflowItems.Triggers.AddNew();
			dummy.WorkflowItems.Triggers.AddNew();
			dummy.WorkflowItems.Exceptions.AddNew();
			dummy.WorkflowItems.Exceptions.AddNew();

			Factory.Save();

			var loadedDummy = newFactory.Load<DummyWithWorkflow>(dummy.PK);
			AssertEquals(8, loadedDummy.WorkflowItems.Count);

			dummy.WorkflowItems.Tasks.First().Delete();
			dummy.WorkflowItems.Milestones.First().Delete();
			dummy.WorkflowItems.Triggers.First().Delete();
			dummy.WorkflowItems.Exceptions.First().Delete();
			Factory.Save();

			// Checking precondition.
			AssertEquals(2, loadedDummy.WorkflowItems.Tasks.Count);
			AssertEquals(2, loadedDummy.WorkflowItems.Milestones.Count);
			AssertEquals(2, loadedDummy.WorkflowItems.Triggers.Count);
			AssertEquals(2, loadedDummy.WorkflowItems.Exceptions.Count);

			loadedDummy.WorkflowItems.Reload(true, true);

			AssertEquals(1, loadedDummy.WorkflowItems.Tasks.Count);
			AssertEquals(1, loadedDummy.WorkflowItems.Milestones.Count);
			AssertEquals(1, loadedDummy.WorkflowItems.Triggers.Count);
			AssertEquals(1, loadedDummy.WorkflowItems.Exceptions.Count);
		}

		public void TestBizOCollection_Reload_FromDB_OnDelete()
		{
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.WorkflowItems.Tasks.AddNew();
			dummy.WorkflowItems.Tasks.AddNew();
			dummy.WorkflowItems.Milestones.AddNew();
			dummy.WorkflowItems.Milestones.AddNew();
			dummy.WorkflowItems.Triggers.AddNew();
			dummy.WorkflowItems.Triggers.AddNew();
			dummy.WorkflowItems.Exceptions.AddNew();
			dummy.WorkflowItems.Exceptions.AddNew();

			Factory.Save();

			var loadedDummy = newFactory.Load<DummyWithWorkflow>(dummy.PK);
			AssertEquals(8, loadedDummy.WorkflowItems.Count);

			dummy.WorkflowItems.Tasks.First().Delete();
			dummy.WorkflowItems.Milestones.First().Delete();
			dummy.WorkflowItems.Triggers.First().Delete();
			dummy.WorkflowItems.Exceptions.First().Delete();
			Factory.Save();

			// Checking precondition.
			AssertEquals(2, loadedDummy.WorkflowItems.Tasks.Count);
			AssertEquals(2, loadedDummy.WorkflowItems.Milestones.Count);
			AssertEquals(2, loadedDummy.WorkflowItems.Triggers.Count);
			AssertEquals(2, loadedDummy.WorkflowItems.Exceptions.Count);

			loadedDummy.WorkflowItems.Reload(false, true);

			AssertEquals(1, loadedDummy.WorkflowItems.Tasks.Count);
			AssertEquals(1, loadedDummy.WorkflowItems.Milestones.Count);
			AssertEquals(1, loadedDummy.WorkflowItems.Triggers.Count);
			AssertEquals(1, loadedDummy.WorkflowItems.Exceptions.Count);

			var otherCollection = new DummyProcessTaskCollection(loadedDummy);
			otherCollection.Load();
			AssertEquals(1, otherCollection.Tasks.Count);
			AssertEquals(1, otherCollection.Milestones.Count);
			AssertEquals(1, otherCollection.Triggers.Count);
			AssertEquals(1, otherCollection.Exceptions.Count);
		}

		public void TestBizOCollection_Reload_PrefillCache_FromDB_OnDelete()
		{
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.WorkflowItems.Tasks.AddNew();
			dummy.WorkflowItems.Tasks.AddNew();
			dummy.WorkflowItems.Milestones.AddNew();
			dummy.WorkflowItems.Milestones.AddNew();
			dummy.WorkflowItems.Triggers.AddNew();
			dummy.WorkflowItems.Triggers.AddNew();
			dummy.WorkflowItems.Exceptions.AddNew();
			dummy.WorkflowItems.Exceptions.AddNew();

			Factory.Save();

			var loadedDummy = newFactory.Load<DummyWithWorkflow>(dummy.PK);
			AssertEquals(8, loadedDummy.WorkflowItems.Count);
			loadedDummy.WorkflowItems.Reload(false, true);
			loadedDummy.WorkflowItems.Reload(true, true);

			dummy.WorkflowItems.Tasks.First().Delete();
			dummy.WorkflowItems.Milestones.First().Delete();
			dummy.WorkflowItems.Triggers.First().Delete();
			dummy.WorkflowItems.Exceptions.First().Delete();
			Factory.Save();

			// Checking precondition.
			AssertEquals(2, loadedDummy.WorkflowItems.Tasks.Count);
			AssertEquals(2, loadedDummy.WorkflowItems.Milestones.Count);
			AssertEquals(2, loadedDummy.WorkflowItems.Triggers.Count);
			AssertEquals(2, loadedDummy.WorkflowItems.Exceptions.Count);

			loadedDummy.WorkflowItems.Reload(false, true);

			AssertEquals(1, loadedDummy.WorkflowItems.Tasks.Count);
			AssertEquals(1, loadedDummy.WorkflowItems.Milestones.Count);
			AssertEquals(1, loadedDummy.WorkflowItems.Triggers.Count);
			AssertEquals(1, loadedDummy.WorkflowItems.Exceptions.Count);
		}

		IWorkflowProvider GetWorkflow_ForTestBizOCollection_Reload()
		{
			// Setup
			var objOnFactory = Factory.New<DummyWithWorkflow>();
			var workflowOnFactory = objOnFactory as IWorkflowProvider;
			var task1 = workflowOnFactory.WorkflowItems.AddNew();
			task1.P9_Sequence = 1;
			task1.P9_Description = "Task 1 @ Factory";
			var task2 = workflowOnFactory.WorkflowItems.AddNew();
			task2.P9_Sequence = 2;
			task2.P9_Description = "Task 2 @ Factory";
			Factory.Save();

			// Prepare test obj
			var testWorkflowOnFactory = Factory.Load<DummyWithWorkflow>(objOnFactory.PK) as IWorkflowProvider;

			// Same factory
			var workflowOnSameFactory = Factory.Load<DummyWithWorkflow>(objOnFactory.PK) as IWorkflowProvider;
			var task3 = workflowOnSameFactory.WorkflowItems.AddNew();
			task3.P9_Sequence = 3;
			task3.P9_Description = "Task 3 @ Same-Factory";
			Factory.Save();

			// Different factory same instances
			var differentFactory = new BusinessObjectFactory { NameForDebugging = "Different-Factory-Same-Instance" };
			var workflowOnDifferentFactory = differentFactory.Load<DummyWithWorkflow>(objOnFactory.PK) as IWorkflowProvider;
			workflowOnDifferentFactory.WorkflowItems.Cast<ProcessTask>().First(x => x.P9_Sequence == 1).P9_Description = "Task 1 - updated on diff-factory-same-inst";
			var task4 = workflowOnDifferentFactory.WorkflowItems.AddNew();
			task4.P9_Sequence = 4;
			task4.P9_Description = "Task 4 @ different-Factory-same-instance";
			differentFactory.Save();

			// Different factory - different instances
			var differentFactoryDifferentInstance = new BusinessObjectFactory { RefreshEnabled = false, NameForDebugging = "Different-Factory-Different-Instance" };
			var workflowOnDifferentFactoryDifferentInstance = differentFactoryDifferentInstance.Load<DummyWithWorkflow>(objOnFactory.PK) as IWorkflowProvider;
			workflowOnDifferentFactoryDifferentInstance.WorkflowItems.Cast<ProcessTask>().First(x => x.P9_Sequence == 2).P9_Description = "Task 2 - updated on diff-factory-same-inst";
			var task5 = workflowOnDifferentFactoryDifferentInstance.WorkflowItems.AddNew();
			task5.P9_Sequence = 5;
			task5.P9_Description = "Task 5 @ different-Factory-different-instance";

			// Simulating/imitating issue with different application instance by Factory.Load to fill Factory.QueryCache and not clearing it (DisableQueryCacheReset=true) on differentFactoryDifferentInstance.Save
			var tasks = Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, objOnFactory.PK));

			var internalFactory = Factory as IBusinessObjectFactoryInternals;
			var originalValue = internalFactory.DisableQueryCacheReset;
			internalFactory.DisableQueryCacheReset = true;

			differentFactoryDifferentInstance.Save();

			internalFactory.DisableQueryCacheReset = originalValue;

			return testWorkflowOnFactory;
		}

		public void TestDataRefresh()
		{
			var dummy1 = Factory.New<DummyWithWorkflow>();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var dummy1InOtherFactory = factory2.Load<DummyWithWorkflow>(dummy1.PK);
			var dummy2InOtherFactory = factory2.New<DummyWithWorkflow>();

			var task1 = dummy1InOtherFactory.WorkflowItems.AddNew();
			task1.IsWorkflowTrigger = ZBool.True;

			var task2 = dummy2InOtherFactory.WorkflowItems.AddNew();
			task2.IsWorkflowTrigger = ZBool.True;

			AssertEquals("Precondition", 0, dummy1.WorkflowItems.Count);

			factory2.Save();

			AssertEquals(1, dummy1.WorkflowItems.Count);
			AssertEquals(task1.PK, dummy1.WorkflowItems[0].PK);
		}

		#endregion

		[TestDate(2019, 07, 03)]
		public void TestSortByActualDuration()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var p1 = job.WorkflowItems.Tasks.AddNew();
			var p2 = job.WorkflowItems.Tasks.AddNew();
			var p3 = job.WorkflowItems.Tasks.AddNew();

			p3.P9_ActualDuration = new ZDateTime(2018, 1, 1).AddMinutes(30);
			p2.P9_ActualDuration = new ZDateTime(2019, 1, 1).AddMinutes(20);
			p1.P9_ActualDuration = new ZDateTime(2020, 1, 1).AddMinutes(10);

			job.WorkflowItems.AddIsTime(ProcessTasksSchema.P9_ActualDuration.Name); //simulating what will happen from the collection being bound to a grid with time column for this field

			job.WorkflowItems.Tasks.Sort("P9_ActualDuration", ListSortDirection.Ascending);
			AssertArrayEqualsByElements(new[] { p1, p2, p3 }, job.WorkflowItems.Tasks.ToArray());

			job.WorkflowItems.Sort("P9_ActualDuration", ListSortDirection.Ascending);
			AssertArrayEqualsByElements(new[] { p1, p2, p3 }, job.WorkflowItems.ToArray());

			job.WorkflowItems.Tasks.Sort("P9_ActualDuration", ListSortDirection.Descending);
			AssertArrayEqualsByElements(new[] { p3, p2, p1 }, job.WorkflowItems.Tasks.ToArray());

			job.WorkflowItems.Sort("P9_ActualDuration", ListSortDirection.Descending);
			AssertArrayEqualsByElements(new[] { p3, p2, p1 }, job.WorkflowItems.ToArray());

			p3.P9_ActualDuration = ZDateTime.Invalid;
			job.WorkflowItems.Sort("P9_ActualDuration", ListSortDirection.Descending);
			AssertArrayEqualsByElements(new[] { p2, p1, p3 }, job.WorkflowItems.ToArray());

			p3.P9_ActualDuration = ZDateTime.Empty;
			job.WorkflowItems.Sort("P9_ActualDuration", ListSortDirection.Descending);
			AssertArrayEqualsByElements(new[] { p2, p1, p3 }, job.WorkflowItems.ToArray());
		}

		[TestDate(2019, 07, 03)]
		public void TestSortByP9_EstDuration()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var p1 = job.WorkflowItems.Tasks.AddNew();
			var p2 = job.WorkflowItems.Tasks.AddNew();
			var p3 = job.WorkflowItems.Tasks.AddNew();

			p3.P9_EstDuration = new ZDateTime(2018, 1, 1).AddMinutes(30);
			p2.P9_EstDuration = new ZDateTime(2019, 1, 1).AddMinutes(20);
			p1.P9_EstDuration = new ZDateTime(2020, 1, 1).AddMinutes(10);

			job.WorkflowItems.AddIsTime(ProcessTasksSchema.P9_EstDuration.Name); //simulating what will happen from the collection being bound to a grid with time column for this field

			job.WorkflowItems.Tasks.Sort("P9_EstDuration", ListSortDirection.Ascending);
			AssertArrayEqualsByElements(new[] { p1, p2, p3 }, job.WorkflowItems.Tasks.ToArray());

			job.WorkflowItems.Sort("P9_EstDuration", ListSortDirection.Ascending);
			AssertArrayEqualsByElements(new[] { p1, p2, p3 }, job.WorkflowItems.ToArray());

			job.WorkflowItems.Tasks.Sort("P9_EstDuration", ListSortDirection.Descending);
			AssertArrayEqualsByElements(new[] { p3, p2, p1 }, job.WorkflowItems.Tasks.ToArray());

			job.WorkflowItems.Sort("P9_EstDuration", ListSortDirection.Descending);
			AssertArrayEqualsByElements(new[] { p3, p2, p1 }, job.WorkflowItems.ToArray());

			p3.P9_EstDuration = ZDateTime.Invalid;
			job.WorkflowItems.Sort("P9_EstDuration", ListSortDirection.Descending);
			AssertArrayEqualsByElements(new[] { p2, p1, p3 }, job.WorkflowItems.ToArray());

			p3.P9_EstDuration = ZDateTime.Empty;
			job.WorkflowItems.Sort("P9_EstDuration", ListSortDirection.Descending);
			AssertArrayEqualsByElements(new[] { p2, p1, p3 }, job.WorkflowItems.ToArray());
		}

		public void TestSortEffectiveTaskNudgeColumn()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			var helper = ObjectFactory.Get<IBMTestHelper>();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var noNudgeTask = job.WorkflowItems.Tasks.AddNew();
			var positiveNudgeTask = job.WorkflowItems.Tasks.AddNew();
			var negativeNudgeTask = job.WorkflowItems.Tasks.AddNew();
			var zeroNudgeTask = job.WorkflowItems.Tasks.AddNew();

			var tagDef = helper.CreateTagDefinition(Factory, "DEF");

			var positiveTag = helper.CreateTagMagnitude(tagDef, "POT", nudge: 1);
			var negativeTag = helper.CreateTagMagnitude(tagDef, "NET", nudge: -2);
			var zeroTag = helper.CreateTagMagnitude(tagDef, "ZET", nudge: 0);

			var link1 = (ITagLink)((ITagBindable)positiveNudgeTask).TagLinks_ForBinding.AddNew();
			link1.TGL_TGM_Magnitude = positiveTag.PK;

			var link2 = (ITagLink)((ITagBindable)negativeNudgeTask).TagLinks_ForBinding.AddNew();
			link2.TGL_TGM_Magnitude = negativeTag.PK;

			var link3 = (ITagLink)((ITagBindable)zeroNudgeTask).TagLinks_ForBinding.AddNew();
			link3.TGL_TGM_Magnitude = zeroTag.PK;

			job.WorkflowItems.Sort("EffectiveTaskNudge", ListSortDirection.Ascending);
			AssertArrayEqualsByElements(new[] { noNudgeTask, negativeNudgeTask, zeroNudgeTask, positiveNudgeTask }, job.WorkflowItems.ToArray());

			job.WorkflowItems.Sort("EffectiveTaskNudge", ListSortDirection.Descending);
			AssertArrayEqualsByElements(new[] { positiveNudgeTask, zeroNudgeTask, negativeNudgeTask, noNudgeTask }, job.WorkflowItems.ToArray());
		}

		public void TestSortWorkflowSequence()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			var jobHeader = helper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);

			var job = (DummyWithWorkflow)jobHeader.Parent;
			var tasks = Enumerable.Range(0, 23).Select(i =>
				{
					var workflow = helper.CreateWorkflow(jobHeader, $"Workflow {i:00}");
					return (ProcessTask)helper.CreateTask(workflow);
				})
				.ToArray();

			job.WorkflowItems.Sort("WorkflowSequence", ListSortDirection.Ascending);
			AssertArrayEqualsByElements(tasks, job.WorkflowItems.ToArray());

			job.WorkflowItems.Sort("WorkflowSequence", ListSortDirection.Descending);
			AssertArrayEqualsByElements(tasks.Reverse().ToArray(), job.WorkflowItems.ToArray());
		}

		#region Implementation

		DummyWithWorkflow Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyWithWorkflow>()); }
		}
		DummyWithWorkflow dummy;

		ProcessTask Task
		{
			get
			{
				if (task == null)
				{
					task = Collection.AddNew();
					AssertEquals("IsTask true for the test", true, task.IsTask);
				}
				return task;
			}
		}
		ProcessTask task;

		ProcessTask Milestone
		{
			get
			{
				if (milestone == null)
				{
					milestone = Collection.AddNew();
					milestone.IsMilestone = true;
				}
				return milestone;
			}
		}
		ProcessTask milestone;

		ProcessTask WorkflowTrigger
		{
			get
			{
				if (workflowTrigger == null)
				{
					workflowTrigger = Collection.AddNew();
					workflowTrigger.IsWorkflowTrigger = true;
				}
				return workflowTrigger;
			}
		}
		ProcessTask workflowTrigger;

		new ProcessTaskCollection Collection
		{
			get { return (ProcessTaskCollection)base.Collection; }
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgOpportunity opportunity = Factory.New<OrgOpportunity>();
			return new ProcessTaskCollection(opportunity);
		}

		protected override void SetUp()
		{
			base.SetUp();
			disposable = ProcessTaskCollection.CanCreateTaskCollection();
		}

		IDisposable disposable;

		protected override void TearDown()
		{
			disposable.Dispose();
			base.TearDown();
		}

		#endregion
	}
}
