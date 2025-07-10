using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(typeof(IWorkflowProvider), typeof(TestExcludeWorkflowProviderHasTestCaseAttribute), RequireTestOnlyInFirstSubLevel = true)]
	public abstract class WorkflowProviderTest<BusinessObjectT, ExpectedProcessTaskCollectionT> : TestCaseWithFactory
			where BusinessObjectT : BusinessObject, IWorkflowProvider
			where ExpectedProcessTaskCollectionT : ProcessTaskCollection
	{
		public void TestTaskCanLoadParent()
		{
			var parent = BusinessObject;
			var task = parent.WorkflowItems.Tasks.AddNew();
			var descriptor = WorkflowDescriptors.Instance.TryGetValueSafe(parent.WorkflowType);

			CombineAssertions(() =>
			{
				AssertNotNull("There should be a workflow descriptor with code " + parent.WorkflowType + " present in WorkflowDescriptors.Instance. This is done in the WorkflowDescriptors element of WorkflowDescriptorsConfiguration.xml.", descriptor);
				var parentType = ParentProxyType ?? task.Parent.GetType();
				AssertNotNull($"Parent was null", parentType);
				Assert($"Provider type ({descriptor.WorkflowProviderType}) should be assignable from the parent type ({parentType})", descriptor.WorkflowProviderType.IsAssignableFrom(parentType));
				AssertNotEquals($"Parent type never ought to be this ({task.ParentType})", typeof(BusinessObject), task.ParentType);
				AssertEquals("But maybe we can load the task anyway", GetParent(parent), task.Parent);
			});

			OnFinishedRunningTestThatLoadsWorkflowDescriptor();
		}

		protected virtual BusinessObject GetParent(BusinessObjectT bizo) => bizo;
		protected virtual Type ParentProxyType => null;

		#region Workflows

		public void TestWorkflowsCollection()
		{
			BMTestHelper.EnableBMSInRegistry();
			BMTestHelper.CreateSystem(Factory, WorkflowType);

			var parent = (IWorkflowProvider)GetParent(BusinessObject);
			var jobHeader = ProcessJobHeaderProvider.GetForParent(parent, Factory);

			var firstWorkflow = jobHeader.ProcessHeaders[0];
			var secondWorkflow = jobHeader.ProcessHeaders.AddNew();

			Factory.Save();

			AssertNotNull(parent.Workflows);
			AssertEquals(2, parent.Workflows.Count);

			AssertContainsExactElementsInAnyOrder(new List<IProcessHeader> { firstWorkflow, secondWorkflow }, parent.Workflows);
		}

		public void TestWorkflowsEmptyCollection_BMSDisabled()
		{
			BMTestHelper.DisableBMSInRegistry();

			var parent = (IWorkflowProvider)GetParent(BusinessObject);
			var jobHeader = ProcessJobHeaderProvider.GetForParent(parent, Factory);

			AssertNull(jobHeader);

			Factory.Save();

			AssertNotNull(parent.Workflows);
			AssertEquals(0, parent.Workflows.Count);
		}

		#endregion

		#region Universal Triggers

		public void TestUniversalTemplateExists_ShouldAppearOnJob()
		{
			var universalTemplate = MasterFilesTestHelper.CreateUniversalTemplate(Factory, WorkflowType);
			var universalTrigger = (ITemplateTrigger)universalTemplate.TemplateTriggers.AddNew();
			universalTrigger.Description = "Maximum Hodgson";
			universalTrigger.TriggerEventCode = Events.TagWasAddedOrRemovedCode;

			Factory.Save();

			var job = GetNewBusinessObject(Factory);
			job.ApplyWorkflowTemplates();

			Factory.Save();

			AssertEquals(1, job.WorkflowItems.TriggersIncludingRelated.Count);
			AssertEquals("Maximum Hodgson", job.WorkflowItems.TriggersIncludingRelated[0].P9_Description);
		}

		public void TestLoadParentJobFromUniversalTrigger()
		{
			var universalTemplate = MasterFilesTestHelper.CreateUniversalTemplate(Factory, WorkflowType);
			var universalTrigger = (ITemplateTrigger)universalTemplate.TemplateTriggers.AddNew();
			universalTrigger.Description = "Sax and Mam";
			universalTrigger.TriggerEventCode = Events.TagWasAddedOrRemovedCode;

			var job = GetNewBusinessObject(Factory);
			var jobTriggerLink = Factory.New<IProcessJobTriggerLink>();
			jobTriggerLink.P9L_P9T_TemplateTrigger = universalTrigger.Identifier;
			jobTriggerLink.P9L_ParentId = job.PK;
			jobTriggerLink.P9L_ParentTableCode = string.IsNullOrEmpty(RealTableNameForNonPersistentIWorkflowProvider) ? job.TablePrefix : ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(RealTableNameForNonPersistentIWorkflowProvider);

			var normalTrigger = job.WorkflowItems.Triggers.AddNew();
			normalTrigger.FillWithValidTestData();
			normalTrigger.P9_Description = "STAHP";

			var descriptor = WorkflowDescriptors.Instance.TryGetValueSafe(job.WorkflowType);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();

			ProcessTask loadedNormalTrigger;
			IWorkflowProvider loadedJobThroughNormalTrigger;

			try
			{
				loadedNormalTrigger = newFactory.Load<ProcessTask>(normalTrigger.PK);
				loadedJobThroughNormalTrigger = loadedNormalTrigger.Parent;
			}
			catch (ApplicationException)
			{
				if (IsExcludedFromTestLoadParentJobFromUniversalTrigger(descriptor))
				{
					Assert(true);
				}
				else
				{
					FailedToLoadParentJobFromUniversalTrigger(job, descriptor, null, null);
				}

				return;
			}

			var loadedTriggerLink = newFactory.Load<IProcessJobTriggerLink>(jobTriggerLink.Identifier);
			var loadedJobThroughUniversalTrigger = loadedTriggerLink.GetJob() as IWorkflowProvider;

			var loadedJobSuccessfully = loadedJobThroughNormalTrigger != null;
			var isCorrectTypeWhenLoadedThroughUniversalTrigger = loadedJobThroughNormalTrigger?.GetType() == loadedJobThroughUniversalTrigger?.GetType();

			if ((loadedJobSuccessfully && isCorrectTypeWhenLoadedThroughUniversalTrigger) || IsExcludedFromTestLoadParentJobFromUniversalTrigger(descriptor))
			{
				Assert(true);
			}
			else
			{
				FailedToLoadParentJobFromUniversalTrigger(job, descriptor, loadedJobThroughNormalTrigger, loadedJobThroughUniversalTrigger);
			}

			OnFinishedRunningTestThatLoadsWorkflowDescriptor();
			OnFinishedRunningTestThatLoadsParentJob();
		}

		static bool IsExcludedFromTestLoadParentJobFromUniversalTrigger(WorkflowDescriptor descriptor)
		{
			return descriptor == null || !descriptor.SupportsUniversalTemplates;
		}

		static void FailedToLoadParentJobFromUniversalTrigger(BusinessObjectT originalJob, WorkflowDescriptor descriptor, IWorkflowProvider loadedJobThroughNormalTrigger, IWorkflowProvider loadedJobThroughUniversalTrigger)
		{
			var message = string.Format(@"It should be possible to load the parent business object from a ProcessJobTriggerLink record and it be the same type as constructed by this test.
This test is using IBaseTrigger.GetParent extension method, which uses WorkflowDescriptor.WorkflowProviderType. For some reason this is different to the type of bizo constructed by the test case.
It may also be different from the type constructed by ProcessTask.Parent which uses ProcessTask.ParentType. These really should all be the same.

Job type: [{0}]
Workflow type: [{1}]
Workflow Descriptor: [{2}].", originalJob.GetType(), originalJob.WorkflowType, descriptor.GetType().FullName);

			AssertNotNull(message, loadedJobThroughNormalTrigger);
			AssertType(message, loadedJobThroughNormalTrigger.GetType(), loadedJobThroughUniversalTrigger);
		}

		#endregion

		#region Task Line Triggers

		[TestDate(2015, 7, 14)]
		public void TestEventRaisedOnTask_WhenLineTriggerDefinedOnParent_ShouldFireFromTaskEventOnly()
		{
			var job = GetNewBusinessObject(Factory);
			var worflowDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe(job.WorkflowType);

			if (worflowDescriptor.SupportsTaskLineTriggers)
			{
				var jobTrigger = MasterFilesTestHelper.CreateTrigger(job, Events.TagWasAddedOrRemovedCode);
				var lineTrigger = MasterFilesTestHelper.CreateTrigger(job, Events.TagWasAddedOrRemovedCode);
				lineTrigger.P9_LineTriggerType = ProcessTasksLookups.TaskLineTriggerCode;

				var task = MasterFilesTestHelper.CreateTask(job, assignedStaff: GlbStaff.CurrentUser.GS_Code, status: ProcessTaskStatusCodeList.Codes.Assigned);

				Factory.Save();

				AssertEquals(ZDateTime.Empty, jobTrigger.P9_ActualDate);
				AssertEquals(ZDateTime.Empty, lineTrigger.P9_ActualDate);

				task.GetLogs().AddNew(Events.TagWasAddedOrRemoved);
				Factory.Save();

				AssertEquals("Raising a TAG event on the task should NOT fire the standard trigger, since it only responds to events raised on the job directly.", ZDateTime.Empty, jobTrigger.P9_ActualDate);
				AssertEquals("Raising a TAG event on the task SHOULD fire the line trigger, since it responds to events raised on any task within the job.", new ZDateTime(2015, 7, 14), lineTrigger.P9_ActualDate);

				TestDateAttribute.AddDays(1);

				job.GetLogs().AddNew(Events.TagWasAddedOrRemoved);
				Factory.Save();

				AssertEquals("Raising a TAG event on the job directly SHOULD fire the standard trigger, since it responds to events raised on the job directly.", new ZDateTime(2015, 7, 15), jobTrigger.P9_ActualDate);
				AssertEquals("Raising a TAG event on the job directly should NOT fire the line trigger, since it only responds to task events.", new ZDateTime(2015, 7, 14), lineTrigger.P9_ActualDate);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestLineTriggerTypesLookup_WhenParentSupportsTasksAndTriggers_ShouldIncludeTaskLineTriggers()
		{
			var job = GetNewBusinessObject(Factory);
			var worflowDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe(job.WorkflowType);
			var trigger = MasterFilesTestHelper.CreateTrigger(job, Events.TagWasAddedOrRemovedCode);

			AssertEquals(worflowDescriptor.SupportsTaskLineTriggers, trigger.Lookups.LineTriggerTypes.ContainsCode(ProcessTasksLookups.TaskLineTriggerCode));
		}

		public void TestMessagingTriggerPartiesList_ForTaskLineTrigger_ShouldContainAdditionalRecipients()
		{
			var worflowDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe(WorkflowType);

			if (worflowDescriptor.SupportsTaskLineTriggers)
			{
				var job = GetNewBusinessObject(Factory);
				var trigger = MasterFilesTestHelper.CreateTrigger(job, Events.TagWasAddedOrRemovedCode, lineTriggerType: ProcessTasksLookups.TaskLineTriggerCode);
				var triggerAction = MasterFilesTestHelper.CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.NotificationEmail);

				AssertSequencesEqual(new[]
				{
					Tuple.Create("EML", "Email"),
					Tuple.Create("STF", "Assigned Staff"),
					Tuple.Create("CAP", "Required Capability Members"),
					Tuple.Create("GRP", "Assigned Group Members")
				}, triggerAction.Lookups.MessagingTriggerPartiesList.Cast<ICodeDescription>().Select(cdp => Tuple.Create(cdp.Code, cdp.Description)));

				triggerAction.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.AssignedStaff;
				triggerAction.RunPreSaveValidation();

				AssertNoErrors(triggerAction);
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestFireTaskLineTrigger_ShouldSendToAppropriateRecipients()
		{
			var worflowDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe(WorkflowType);

			if (worflowDescriptor.SupportsTaskLineTriggers)
			{
				using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
				{
					var staffDirectlyAssigned = MasterFilesTestHelper.CreateStaff(Factory, "staffDirectlyAssigned@wtg.100years.com");
					var staffWithCapability_1 = MasterFilesTestHelper.CreateStaff(Factory, "staffWithCapability_1@wtg.100years.com");
					var staffWithCapability_2 = MasterFilesTestHelper.CreateStaff(Factory, "staffWithCapability_2@wtg.100years.com");
					var staffInGroup_1 = MasterFilesTestHelper.CreateStaff(Factory, "staffInGroup_1@wtg.100years.com");
					var staffInGroup_2 = MasterFilesTestHelper.CreateStaff(Factory, "staffInGroup_2@wtg.100years.com");
					var staffWithCapabilityAndInGroup = MasterFilesTestHelper.CreateStaff(Factory, "staffWithCapabilityAndInGroup@wtg.100years.com");

					var capability = MasterFilesTestHelper.CreateCapability(Factory, staffWithCapability_1, staffWithCapability_2, staffWithCapabilityAndInGroup);
					var group = MasterFilesTestHelper.CreateGroup(Factory, staffInGroup_1, staffInGroup_2, staffWithCapabilityAndInGroup);

					var job = GetNewBusinessObject(Factory);

					var task_assignedToStaff = MasterFilesTestHelper.CreateTask(job, staffDirectlyAssigned.GS_Code);
					var task_requiresCapability = MasterFilesTestHelper.CreateTask(job, assignedStaff: "", requiredCapability: capability);
					var task_assignedToGroup = MasterFilesTestHelper.CreateTask(job, assignedGroup: group);
					var task_withAllThree = MasterFilesTestHelper.CreateTask(job, staffDirectlyAssigned.GS_Code, requiredCapability: capability, assignedGroup: group);

					var trigger = MasterFilesTestHelper.CreateTrigger(job, Events.TagWasAddedOrRemovedCode, lineTriggerType: ProcessTasksLookups.TaskLineTriggerCode, description: "Frangelico Frapulence");
					var triggerAction1 = MasterFilesTestHelper.CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, MessageRecipientPartyTypeList.Codes.AssignedStaff, "Email to assigned staff");
					var triggerAction2 = MasterFilesTestHelper.CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, MessageRecipientPartyTypeList.Codes.RequiredCapabilityMembers, "Email to staff in capability");
					var triggerAction3 = MasterFilesTestHelper.CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, MessageRecipientPartyTypeList.Codes.AssignedGroupMembers, "Email to staff in group");

					Factory.Save();

					RaiseEventOnTaskAndAssertEmailSent(trigger, task_assignedToStaff, Tuple.Create("Email to assigned staff", "staffDirectlyAssigned@wtg.100years.com"));

					RaiseEventOnTaskAndAssertEmailSent(trigger, task_requiresCapability,
						Tuple.Create("Email to staff in capability", "staffWithCapability_1@wtg.100years.com"),
						Tuple.Create("Email to staff in capability", "staffWithCapability_2@wtg.100years.com"),
						Tuple.Create("Email to staff in capability", "staffWithCapabilityAndInGroup@wtg.100years.com")
						);

					RaiseEventOnTaskAndAssertEmailSent(trigger, task_assignedToGroup,
						Tuple.Create("Email to staff in group", "staffInGroup_1@wtg.100years.com"),
						Tuple.Create("Email to staff in group", "staffInGroup_2@wtg.100years.com"),
						Tuple.Create("Email to staff in group", "staffWithCapabilityAndInGroup@wtg.100years.com")
						);

					RaiseEventOnTaskAndAssertEmailSent(trigger, task_withAllThree,
						Tuple.Create("Email to assigned staff", "staffDirectlyAssigned@wtg.100years.com"),
						Tuple.Create("Email to staff in group", "staffInGroup_1@wtg.100years.com"),
						Tuple.Create("Email to staff in group", "staffInGroup_2@wtg.100years.com"),
						Tuple.Create("Email to staff in capability", "staffWithCapability_1@wtg.100years.com"),
						Tuple.Create("Email to staff in capability", "staffWithCapability_2@wtg.100years.com"),
						Tuple.Create("Email to staff in capability", "staffWithCapabilityAndInGroup@wtg.100years.com"),
						Tuple.Create("Email to staff in group", "staffWithCapabilityAndInGroup@wtg.100years.com")
						);
				}
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestFireTaskLineTrigger_WithCapabilityRecipient_ForTaskWithGroupSpecifiedOnTask_ForGroupScopedCapability_ShouldSendToCapabilityAndGroupIntersection()
		{
			var worflowDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe(WorkflowType);

			if (worflowDescriptor.SupportsTaskLineTriggers)
			{
				using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
				{
					var staffInGroupAndCapability = MasterFilesTestHelper.CreateStaff(Factory, "staffInGroupAndCapability@wtg.100years.com");
					var staffInGroupOnly = MasterFilesTestHelper.CreateStaff(Factory, "staffInGroupOnly@wtg.100years.com");
					var staffInCapabilityOnly = MasterFilesTestHelper.CreateStaff(Factory, "staffInCapabilityOnly@wtg.100years.com");

					var group = MasterFilesTestHelper.CreateGroup(Factory, staffInGroupAndCapability, staffInGroupOnly);
					var capability = MasterFilesTestHelper.CreateCapability(Factory, staffInGroupAndCapability, staffInCapabilityOnly);

					var job = GetNewBusinessObject(Factory);
					var taskWithCapabilityOnly = MasterFilesTestHelper.CreateTask(job, requiredCapability: capability);
					var taskWithGroupAndCapability = MasterFilesTestHelper.CreateTask(job, requiredCapability: capability, assignedGroup: group);

					const string emailSubject = "Email staff with capability, and also member of group when scope set thusly";

					var trigger = MasterFilesTestHelper.CreateTrigger(job, Events.TagWasAddedOrRemovedCode, lineTriggerType: ProcessTasksLookups.TaskLineTriggerCode, description: "Bipsido Tables");
					var triggerAction = MasterFilesTestHelper.CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, MessageRecipientPartyTypeList.Codes.RequiredCapabilityMembers, emailSubject);

					Factory.Save();

					RaiseEventOnTaskAndAssertEmailSent(trigger, taskWithCapabilityOnly, Tuple.Create(emailSubject, "staffInGroupAndCapability@wtg.100years.com"), Tuple.Create(emailSubject, "staffInCapabilityOnly@wtg.100years.com"));
					RaiseEventOnTaskAndAssertEmailSent(trigger, taskWithGroupAndCapability, Tuple.Create(emailSubject, "staffInGroupAndCapability@wtg.100years.com"), Tuple.Create(emailSubject, "staffInCapabilityOnly@wtg.100years.com"));

					capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
					Factory.Save();

					RaiseEventOnTaskAndAssertEmailSent(trigger, taskWithCapabilityOnly, Tuple.Create(emailSubject, "staffInGroupAndCapability@wtg.100years.com"), Tuple.Create(emailSubject, "staffInCapabilityOnly@wtg.100years.com"));
					RaiseEventOnTaskAndAssertEmailSent(trigger, taskWithGroupAndCapability, Tuple.Create(emailSubject, "staffInGroupAndCapability@wtg.100years.com"));
				}
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestFireTaskLineTrigger_WithCapabilityRecipient_ForTaskWithGroupSpecifiedOnWorkflow_ForGroupScopedCapability_ShouldSendToCapabilityAndGroupIntersection()
		{
			var worflowDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe(WorkflowType);

			if (worflowDescriptor.SupportsTaskLineTriggers && worflowDescriptor.SupportsBufferManagement)
			{
				using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
				{
					var testHelper = ObjectFactory.Get<IBMTestHelper>();
					testHelper.EnableBMSInRegistry();
					testHelper.CreateSystem(Factory, WorkflowType);

					var staffInGroupAndCapability = MasterFilesTestHelper.CreateStaff(Factory, "staffInGroupAndCapability@wtg.100years.com");
					var staffInGroupOnly = MasterFilesTestHelper.CreateStaff(Factory, "staffInGroupOnly@wtg.100years.com");
					var staffInCapabilityOnly = MasterFilesTestHelper.CreateStaff(Factory, "staffInCapabilityOnly@wtg.100years.com");

					var group = MasterFilesTestHelper.CreateGroup(Factory, staffInGroupAndCapability, staffInGroupOnly);
					var capability = MasterFilesTestHelper.CreateCapability(Factory, staffInGroupAndCapability, staffInCapabilityOnly);

					var jobHeader = testHelper.CreateJobHeader<BusinessObjectT>(Factory, addDefaultProcessHeaderIfNone: false);
					var workflowNotInReleaseGroup = testHelper.CreateWorkflow(jobHeader, "Workflow 1");
					var workflowInReleaseGroup = testHelper.CreateWorkflow(jobHeader, "Workflow 2", releaseGroupPK: group.PK.ToGuid());
					var task1 = (ProcessTask)testHelper.CreateTask(workflowNotInReleaseGroup, capability: capability);
					var task2 = (ProcessTask)testHelper.CreateTask(workflowInReleaseGroup, capability: capability);

					const string emailSubject = "Email staff with capability, and also member of group when scope set thusly";

					var trigger = MasterFilesTestHelper.CreateTrigger(task1.Parent, Events.TagWasAddedOrRemovedCode, lineTriggerType: ProcessTasksLookups.TaskLineTriggerCode, description: "Bipsido Tables");
					var triggerAction = MasterFilesTestHelper.CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, MessageRecipientPartyTypeList.Codes.RequiredCapabilityMembers, emailSubject);

					Factory.Save();

					RaiseEventOnTaskAndAssertEmailSent(trigger, task1, Tuple.Create(emailSubject, "staffInGroupAndCapability@wtg.100years.com"), Tuple.Create(emailSubject, "staffInCapabilityOnly@wtg.100years.com"));
					RaiseEventOnTaskAndAssertEmailSent(trigger, task2, Tuple.Create(emailSubject, "staffInGroupAndCapability@wtg.100years.com"), Tuple.Create(emailSubject, "staffInCapabilityOnly@wtg.100years.com"));

					capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
					Factory.Save();

					RaiseEventOnTaskAndAssertEmailSent(trigger, task1, Tuple.Create(emailSubject, "staffInGroupAndCapability@wtg.100years.com"), Tuple.Create(emailSubject, "staffInCapabilityOnly@wtg.100years.com"));
					RaiseEventOnTaskAndAssertEmailSent(trigger, task2, Tuple.Create(emailSubject, "staffInGroupAndCapability@wtg.100years.com"));
				}
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestFireTaskLineTrigger_WithCapabilityRecipient_ForTaskWithGroupSpecifiedOnTaskAndWorkflow_ShouldUseTaskGroupAndNotWorkflowGroup()
		{
			var worflowDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe(WorkflowType);

			if (worflowDescriptor.SupportsTaskLineTriggers && worflowDescriptor.SupportsBufferManagement)
			{
				using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
				{
					var testHelper = ObjectFactory.Get<IBMTestHelper>();
					testHelper.EnableBMSInRegistry();
					testHelper.CreateSystem(Factory, WorkflowType);

					var staffInTaskGroup = MasterFilesTestHelper.CreateStaff(Factory, "staffInTaskGroup@wtg.100years.com");
					var staffInWorkflowGroup = MasterFilesTestHelper.CreateStaff(Factory, "staffInWorkflowGroup@wtg.100years.com");
					var staffInNeitherGroupButInCapability = MasterFilesTestHelper.CreateStaff(Factory, "staffInNeitherGroupButInCapability@wtg.100years.com");
					var staffInBothGroupsButNotCapability = MasterFilesTestHelper.CreateStaff(Factory, "staffInBothGroupsButNotCapability@wtg.100years.com");

					var groupOnTask = MasterFilesTestHelper.CreateGroup(Factory, staffInTaskGroup, staffInBothGroupsButNotCapability);
					var groupOnWorkflow = MasterFilesTestHelper.CreateGroup(Factory, staffInWorkflowGroup, staffInBothGroupsButNotCapability);
					var capability = MasterFilesTestHelper.CreateCapability(Factory, staffInTaskGroup, staffInWorkflowGroup, staffInNeitherGroupButInCapability);

					var jobHeader = testHelper.CreateJobHeader<BusinessObjectT>(Factory, addDefaultProcessHeaderIfNone: false);
					var workflow = testHelper.CreateWorkflow(jobHeader, "This had better work", releaseGroupPK: groupOnWorkflow.PK.ToGuid());
					var task = (ProcessTask)testHelper.CreateTask(workflow, capability: capability, group: groupOnTask);

					const string emailSubject = "Email staff with in the group assigned to the task when scope set thusly";

					var trigger = MasterFilesTestHelper.CreateTrigger(task.Parent, Events.TagWasAddedOrRemovedCode, lineTriggerType: ProcessTasksLookups.TaskLineTriggerCode, description: "Bipsido Tables");
					var triggerAction = MasterFilesTestHelper.CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, MessageRecipientPartyTypeList.Codes.RequiredCapabilityMembers, emailSubject);

					Factory.Save();

					RaiseEventOnTaskAndAssertEmailSent(trigger, task, Tuple.Create(emailSubject, "staffInTaskGroup@wtg.100years.com"), Tuple.Create(emailSubject, "staffInWorkflowGroup@wtg.100years.com"), Tuple.Create(emailSubject, "staffInNeitherGroupButInCapability@wtg.100years.com"));

					capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
					Factory.Save();

					RaiseEventOnTaskAndAssertEmailSent(trigger, task, Tuple.Create(emailSubject, "staffInTaskGroup@wtg.100years.com"));
				}
			}
			else
			{
				Assert(true);
			}
		}

		static void RaiseEventOnTaskAndAssertEmailSent(ProcessTask trigger, ProcessTask taskOnWhichToRaiseEvent, params Tuple<string, string>[] expectedEmailBodyAndRecipients)
		{
			taskOnWhichToRaiseEvent.GetLogs().AddNew(Events.TagWasAddedOrRemoved);
			taskOnWhichToRaiseEvent.Factory.Save();

			Env.ClearAllEmailsCreated(); // Inidents like to send emails when they're edited.

			AssertEquals(ZDateTime.Now, trigger.P9_ActualDate);

			MasterFilesTestHelper.RunLogWalker();

			var emailsSent = Env.AllEmailsCreated.Select(email => Tuple.Create(email.Body, email.Recipients.Cast<RecipientDef>().Select(r => r.Email).FirstOrDefault())).OrderBy(x => x.Item2).ToArray();

			CombineAssertions("Email body and recipient should be correct", () =>
			{
				foreach (var expectedEmailDetails in expectedEmailBodyAndRecipients)
				{
					var actualEmail = emailsSent.SingleOrDefault(x => x.Item1.Contains(expectedEmailDetails.Item1) && x.Item2 == expectedEmailDetails.Item2);
					var message = $"An email should have been sent containing body [{expectedEmailDetails.Item1}] to recipient {expectedEmailDetails.Item2}.";

					AssertNotNull(message, actualEmail);
				}

				AssertEquals("The correct number of emails should have been sent", expectedEmailBodyAndRecipients.Length, emailsSent.Length);
			});

			TestDateAttribute.AddMinutes(1);
			Env.ClearAllEmailsCreated();
		}

		#endregion

		#region Release Group Rules

		public void TestReleaseGroupRules_ShouldWorkOnEveryWorkflowType()
		{
			var descriptor = WorkflowDescriptors.Instance.TryGetValueSafe(WorkflowType);

			if (descriptor.SupportsReleaseGroupRules)
			{
				try
				{
					var templateFactory = Factory.CreateNewFactory();
					var job = GetNewBusinessObject(Factory);
					var realJob = GetParent(job); // Customs like to do strange things.

					var testHelper = ObjectFactory.Get<IBMTestHelper>();
					var system = testHelper.CreateSystem(templateFactory, WorkflowType);
					testHelper.EnableBMSInRegistry();

					var group1 = templateFactory.NewWithValidTestData<GlbGroup>();
					var group2 = templateFactory.NewWithValidTestData<GlbGroup>();

					group1.GG_Code = "FAKE";
					group2.GG_Code = "NEWS";

					var template = MasterFilesTestHelper.CreateWorkflowTemplate(templateFactory, WorkflowType);
					var templateWorkflow = testHelper.CreateWorkflow(template, "Didgeri oh no she better don't");
					var templateTask = testHelper.CreateTask(template, templateWorkflow);

					var propertyName = GetPropertyNameForReleaseGroupRulesTest(realJob);
					var rule = (IProcessTemplateReleaseGroupRule)template.ReleaseGroupRules.AddNew();
					rule.PTR_ValueSelectionMacro = $"<{propertyName}>";

					var mapping1 = (IProcessTemplateReleaseGroupRuleMapping)rule.GroupMappings.AddNew();
					mapping1.PTM_GG_Group = group1.PK;
					mapping1.PTM_Value = "FAK";

					var mapping2 = (IProcessTemplateReleaseGroupRuleMapping)rule.GroupMappings.AddNew();
					mapping2.PTM_GG_Group = group2.PK;
					mapping2.PTM_Value = "NEW";

					templateFactory.Save();

					SetPropertyValueForReleaseGroupRulesTest(realJob, propertyName, "FAK");

					Factory.Save();
					ManuallyApplyTemplatesIfRequired();

					var jobLevelWorkflow = ProcessJobHeaderProvider.GetForParent((IWorkflowProvider)realJob, Factory, addDefaultProcessHeaderIfNone: false);

					AssertNotNull(jobLevelWorkflow);
					AssertEquals(1, jobLevelWorkflow.ProcessHeaders.Count);

					var workflow = jobLevelWorkflow.ProcessHeaders[0];
					AssertEquals("Didgeri oh no she better don't", workflow.FH_CompletionStatement);

					AssertReleaseGroup("Saving the job initially should apply workflow template and determine the release group based on the defined rule.",
						"FAKE");

					SetPropertyValueForReleaseGroupRulesTest(realJob, propertyName, "NEW");
					Factory.Save();
					ManuallyApplyTemplatesIfRequired();

					AssertReleaseGroup($"Updating the job and saving again should re-determine the release group based on the defined rule. If this fails, the business object probably isn't calling {nameof(WorkflowExtensionMethods.ApplyWorkflowTemplates)} in its OnFactorySavingBeforeTransactionCore method.",
						"NEWS");

					void AssertReleaseGroup(string assertionMessage, string expectedReleaseGroupCode)
					{
						CombineAssertions(assertionMessage, () =>
						{
							AssertEquals(expectedReleaseGroupCode, jobLevelWorkflow.ReleaseGroup?.GG_Code);
							AssertEquals(expectedReleaseGroupCode, workflow.ReleaseGroup?.GG_Code);
						});
					}

					void ManuallyApplyTemplatesIfRequired()
					{
						if (WorkflowProviderDoesNotApplyTemplatesWhenSavingFactory) // This business object probably controls when it fires templates, such as in subclasses. All workflow providers being run in this test support templates somehow.
						{
							((IWorkflowProvider)realJob).ApplyWorkflowTemplates();
						}
					}
				}
				finally
				{
					OnFinishedRunningTestThatLoadsParentJob(); // Because Customs raise errors and then suppress them.
				}
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual string GetPropertyNameForReleaseGroupRulesTest(BusinessObject job)
		{
			var descriptor = WorkflowDescriptors.Instance.TryGetValueSafe(WorkflowType);
			var properties = descriptor.WorkflowProviderType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			var property = properties.FirstOrDefault(p => p.Name.Contains("_" + AuditDetailsColumns.SystemCreateUser) || p.Name.Contains("_" + AuditDetailsColumns.SystemLastEditUser));

			if (property == null)
			{
				var schema = EnterpriseSchema.GetTableSchemaFromColumnNamePrefix(job.TablePrefix);
				var schemaColumn = schema?.All.OfType<SchemaStringColumn>().FirstOrDefault();

				if (schemaColumn != null)
				{
					property = properties.FirstOrDefault(p => p.Name == schemaColumn.Name);
				}

				if (property == null)
				{
					property = properties.FirstOrDefault(p => p.PropertyType == typeof(ZString) && p.CanWrite);

					if (property == null)
					{
						Fail($"Please override {nameof(GetPropertyNameForReleaseGroupRulesTest)} and supply a valid property on {descriptor.WorkflowProviderType.FullName} that can accept a 3 character string. Or extend this test so it can handle your case.");
					}
				}
			}

			return property.Name;
		}

		protected virtual void SetPropertyValueForReleaseGroupRulesTest(BusinessObject job, string propertyName, string value)
		{
			job[propertyName] = value;
		}

		#endregion

		public void TestWorkflowSupportableTableNames()
		{
			// NOTE: this is also tested in EnterpriseBusinessObjectTestCase.TestWorkflowSupportableBusinessObject.
			// It seems some implementations of IWorkflowProvider are suppressing that entire test case, hence the duplicated test here.

			var tableName = BusinessObject.TableName;

			if (string.IsNullOrEmpty(tableName))
			{
				if (BusinessObject is NonPersistentBusinessObject)
				{
					tableName = RealTableNameForNonPersistentIWorkflowProvider;

					if (string.IsNullOrEmpty(tableName))
					{
						Fail("For non-persistent IWorkflowProviders, you must override RealTableNameForNonPersistentIWorkflowProvider to specify where the real workflow records will be saved against. This is the table name to which StmALog.SL_Table will be referring.");
					}
				}
			}

			var supportedTableNames = WorkflowSupportableTableNames.Instance.GetTableNames();
			var message = $"WorkflowSupportableTableNames.Instance.GetTableNames should contain {tableName}. This makes it possible to process StmALogQueue records where SL_FireWorkflow is true in TasksAndMilestonesLoader. Without this, it is not possible to apply workflow templates or fire milestones/triggers outside the main CW1 process.";

			AssertCollectionContains(message, tableName, supportedTableNames);
		}

		protected virtual string RealTableNameForNonPersistentIWorkflowProvider => string.Empty;

		public virtual void TestProcessTasksAreSavedThenReloaded()
		{
			var trigger = WorkflowProvider.WorkflowItems.AddNew();
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging;

			Factory.Save();

			var workflowProviderReloaded = ReloadWorkflowProvider(new BusinessObjectFactory(), BusinessObject);
			AssertNotNull("Should be able to reload the WorkflowProvider", workflowProviderReloaded);

			var triggersReloaded = workflowProviderReloaded.WorkflowItems;
			AssertEquals("triggersReloaded.Count", 1, triggersReloaded.Count);

			var triggerReloaded = triggersReloaded[0];
			AssertEquals("triggerReloaded.P9_Type", Constants.Workflow.WorkflowTriggerType, triggerReloaded.P9_Type);
			AssertEquals("triggerReloaded.P9_SE_NKMilestoneEvent", Events.AuthorisedCode, triggerReloaded.P9_SE_NKMilestoneEvent);

			var triggerActionsReloaded = triggerReloaded.ProcessTaskNotifications;
			AssertEquals("triggerActionsReloaded.Count", 1, triggerActionsReloaded.Count);
			AssertEquals("triggerActionsReloaded[0].PQ_TriggerType", WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging, triggerActionsReloaded[0].PQ_TriggerType);

			OnFinishedRunningTestThatLoadsParentJob();
		}

		public virtual void TestProcessTasksCreatedOnSave()
		{
			WorkflowTemplate.Factory.Save();
			AssertEquals("No tasks initially", 0, WorkflowProvider.WorkflowItems.Count);

			((BusinessObject)WorkflowProvider).HasChanges = true;
			Factory.Save();
			AssertEquals("Tasks should be created from the template on save. Override OnFactorySavingBeforeTransactionCore and add 'if (HasChanges) new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);'", true, WorkflowProvider.WorkflowItems.Count > 0);
			WorkflowProvider.WorkflowItems.RemoveAndDeleteAll();
		}

		public virtual void TestProcessTasksCascadeDeleted()
		{
			ProcessTaskCollection workflowItems = WorkflowProvider.WorkflowItems;
			workflowItems.AddNew();
			SetupBusinessObjectForDeletion();
			BusinessObject.Delete();
			AssertEquals("Tasks should be cascade deleted by overriding Delete() on the parent business object", 0, workflowItems.Count);
		}

		public virtual void TestProcessTaskLoadsCorrectType()
		{
			ProcessTask createdTask = WorkflowProvider.WorkflowItems.AddNew();
			ProcessTask loadedTask = Factory.Load<ProcessTask>(createdTask.PK);
			AssertEquals("Loading a ProcessTask should return the correct object type", createdTask.GetType(), loadedTask.GetType());

			OnFinishedRunningTestThatLoadsParentJob();
		}

		public void TestProcessTaskCollectionOfCorrectType()
		{
			AssertEquals("Expected IWorkflowProvider.WorkflowItems collection to be of type " + typeof(ExpectedProcessTaskCollectionT).Name, true, WorkflowProvider.WorkflowItems is ExpectedProcessTaskCollectionT);
		}

		[ExpectNoExceptions]
		public void TestCreateTaskAndSave()
		{
			ProcessTask createdTask = WorkflowProvider.WorkflowItems.Tasks.AddNew();
			createdTask.P9_Type = "UDF";
			createdTask.P9_Description = "Description";
			Factory.Save();
		}

		[ExpectNoExceptions]
		public void TestWorkflowProviderDoesntInteractWithWorkflowOnConstruction()
		{
			Factory.RefreshEnabled = false;
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = false;
			var job = (IWorkflowProvider)GetNewBusinessObject(Factory);
			var task = job.WorkflowItems.AddNew();
			task.P9_Type = "UDF";
			task.P9_Description = "Description";

			Factory.Save();

			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			var bMTestHelper = ObjectFactory.Get<IBMTestHelper>();
			var system = bMTestHelper.CreateSystem(Factory, job.WorkflowType);
			bMTestHelper.CreateBucket(system);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			if (!typeof(BusinessObjectT).IsSubclassOf(typeof(NonPersistentBusinessObject)))
			{
				newFactory.Load<BusinessObjectT>(job.PK);
			}
			else
			{
				newFactory.Load<BusinessObjectT>(task.P9_ParentID);
			}
		}

		public void TestCancelNonStartedTasksAndClosePartiallyCompletedTasks()
		{
			var job = GetNewBusinessObject(Factory);

			var workingTask = MasterFilesTestHelper.CreateTask(job, GlbStaff.CurrentUser.GS_Code, status: ProcessTaskStatusCodeList.Codes.Working);
			var suspendedTask = MasterFilesTestHelper.CreateTask(job, status: ProcessTaskStatusCodeList.Codes.Suspended);
			var closedTask = MasterFilesTestHelper.CreateTask(job, status: ProcessTaskStatusCodeList.Codes.Closed);
			var assignedTask = MasterFilesTestHelper.CreateTask(job, status: ProcessTaskStatusCodeList.Codes.Assigned);
			var openTask = MasterFilesTestHelper.CreateTask(job, status: ProcessTaskStatusCodeList.Codes.Open);
			var cancelledTask = MasterFilesTestHelper.CreateTask(job, status: ProcessTaskStatusCodeList.Codes.Cancelled);

			job.CancelNonStartedTasksAndClosePartiallyCompletedTasks();

			AssertEquals("Task that started out with WRK status should be closed instead of cancelled since there was some time recording to track.", ProcessTaskStatusCodeList.Codes.Closed, workingTask.P9_Status);
			AssertEquals("Task that started out with SUS status should be closed instead of cancelled since there was some time recording to track.", ProcessTaskStatusCodeList.Codes.Closed, suspendedTask.P9_Status);
			AssertEquals("Already closed task shouldn't be affected", ProcessTaskStatusCodeList.Codes.Closed, closedTask.P9_Status);
			AssertEquals("Un-started task should be cancelled", ProcessTaskStatusCodeList.Codes.Cancelled, assignedTask.P9_Status);
			AssertEquals("Un-started task should be cancelled", ProcessTaskStatusCodeList.Codes.Cancelled, openTask.P9_Status);
			AssertEquals("Already cancelled task should still be cancelled", ProcessTaskStatusCodeList.Codes.Cancelled, cancelledTask.P9_Status);

			OnFinishedRunningTestThatLoadsParentJob();
		}

		protected virtual void OnFinishedRunningTestThatLoadsParentJob()
		{
		}

		protected virtual void OnFinishedRunningTestThatLoadsWorkflowDescriptor()
		{
		}

		#region EventDatePropertyAttribute

		public virtual void TestEventDatePropertyAttribute_AppliedCorrectlyToProperties()
		{
			foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(BusinessObject))
			{
				EventDatePropertyAttribute attr = (EventDatePropertyAttribute)property.Attributes[typeof(EventDatePropertyAttribute)];
				if (attr != null && attr.EstimateActual == EstimateActual.MilestoneEstimateOnly)
				{
					ZPropertyInfo propertyInfo = BusinessObject.ZPropertyInfoHash[property.Name];
					Event eventType = Events.All[attr.EventType];

					AssertNotNull("An invalid event type was specified ('" + attr.EventType + "')", eventType);
					AssertEventDatePropertyAttributeAppliedCorrectly(propertyInfo, eventType);
				}
			}
			Assert(true);
		}

		void AssertEventDatePropertyAttributeAppliedCorrectly(ZPropertyInfo property, Event eventType)
		{
			AssertMilestoneEstimateUpdatedFromDateProperty(property, eventType);
			AssertMilestoneEstimateDateDefaultedFromDateProperty(property, eventType);
			AssertDatePropertyUpdatedFromMilestoneChange(property, eventType);
		}

		void AssertMilestoneEstimateUpdatedFromDateProperty(ZPropertyInfo property, Event eventType)
		{
			ProcessTask milestone = WorkflowProvider.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = eventType.Code;

			var dateTime = new ZDateTime(2005, 1, 2);
			if (property.Value is ZDateTimeOffset)
			{
				property.Value = new ZDateTimeOffset(dateTime);
			}
			else
			{
				property.Value = dateTime;
			}

			AssertEquals("P9_ScheduledDate updated from property " + property.Name, new ZDateTimeOffset(dateTime), milestone.P9_ScheduledDateForBinding);
		}

		void AssertMilestoneEstimateDateDefaultedFromDateProperty(ZPropertyInfo property, Event eventType)
		{
			var dateTime = new ZDateTime(2005, 1, 2);
			if (property.Value is ZDateTimeOffset)
			{
				property.Value = new ZDateTimeOffset(dateTime);
			}
			else
			{
				property.Value = dateTime;
			}
			ProcessTask milestone = WorkflowProvider.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = eventType.Code;
			AssertEquals("P9_ScheduledDate defaulted from property " + property.Name, dateTime, milestone.P9_ScheduledDate.ToZDateTime());
		}

		void AssertDatePropertyUpdatedFromMilestoneChange(ZPropertyInfo property, Event eventType)
		{
			ProcessTask milestone = WorkflowProvider.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = eventType.Code;

			milestone.P9_ScheduledDateForBinding = new ZDateTimeOffset(new ZDateTime(2005, 1, 2));
			AssertEquals("Property " + property.Name + " updated from P9_ScheduledDate", new ZDateTime(2005, 1, 2), property.Value is ZDateTimeOffset offset ? offset.ToZDateTime() : (ZDateTime)property.Value);
		}

		#endregion

		public void TestWorkflowType()
		{
			AssertEquals("WorkflowType", ExpectedWorkflowType, WorkflowProvider.WorkflowType);
		}

		protected abstract ZString ExpectedWorkflowType { get; }

		#region AssertGetTemplateFilterCriteria

		protected delegate void SetBusinessObjectPropertyValueDelegate<TValue>(TValue value) where TValue : IZType;

		protected void AssertGetTemplateFilterCriteria<TValue>(ZPropertyInfo businessObjectProperty, ZPropertyInfo templateProperty, TValue sampleValue1, TValue sampleValue2, TValue emptyValue) where TValue : IZType
		{
			AssertGetTemplateFilterCriteria(v => businessObjectProperty.Value = v, templateProperty, sampleValue1, sampleValue2, emptyValue);
		}

		protected void AssertGetTemplateFilterCriteria<TValue>(ZPropertyInfo businessObjectProperty, ZPropertyInfo templateProperty, TValue templateValue, TValue sampleValue1, TValue sampleValue2, TValue emptyValue) where TValue : IZType
		{
			AssertGetTemplateFilterCriteria(v => businessObjectProperty.Value = v, templateProperty, templateValue, sampleValue1, sampleValue2, emptyValue);
		}

		protected void AssertGetTemplateFilterCriteria<TValue>(SetBusinessObjectPropertyValueDelegate<TValue> businessObjectPropertySetter, ZPropertyInfo templateProperty, TValue sampleValue1, TValue sampleValue2, TValue emptyValue) where TValue : IZType
		{
			AssertGetTemplateFilterCriteria(businessObjectPropertySetter, templateProperty, sampleValue1, sampleValue1, sampleValue2, emptyValue);
		}

		protected void AssertGetTemplateFilterCriteria<TValue>(SetBusinessObjectPropertyValueDelegate<TValue> businessObjectPropertySetter, ZPropertyInfo templateProperty, TValue templateValue, TValue sampleValue1, TValue sampleValue2, TValue emptyValue) where TValue : IZType
		{
			ProcessTaskTemplate.P0_ProcessType = WorkflowProvider.WorkflowType;
			templateProperty.Value = templateValue;
			Factory.Save();

			businessObjectPropertySetter(sampleValue1);
			AssertItemCreatedFromTemplate(true);

			businessObjectPropertySetter(sampleValue2);
			AssertItemCreatedFromTemplate(false);

			Factory.Save();
			templateProperty.Value = emptyValue;
			Factory.Save();
			AssertItemCreatedFromTemplate(true);
		}

		void AssertItemCreatedFromTemplate(bool expectItemToBeCreated)
		{
			if (ProcessTaskTemplate.WorkflowDescriptor.SupportsTasks)
			{
				WorkflowProvider.WorkflowItems.Tasks.RemoveAndDeleteAll();
				WorkflowProvider.WorkflowItems.Tasks.CreateItemsFromTemplate();
				AssertEquals(expectItemToBeCreated ? 1 : 0, WorkflowProvider.WorkflowItems.Tasks.Count);
			}
			else
			{
				var matches = new ProcessTaskTemplate.Loader(Factory).FindMatches(WorkflowProvider);
				AssertEquals((expectItemToBeCreated ? 1 : 0), matches.Length);
			}
		}

		protected void AssertHasJobRelatedTemplateFilterCriteria(PopulateIJobInvoicingPlugInForTest populateJobParent)
		{
			AssertHasJobRelatedTemplateFilterCriteria(populateJobParent, true);
		}

		protected void AssertHasJobRelatedTemplateFilterCriteria(PopulateIJobInvoicingPlugInForTest populateJobParent, bool hasConsigneeConsignor)
		{
			IJobInvoicingPlugIn jobParent = BusinessObject as IJobInvoicingPlugIn;
			AssertNotNull("BusinessObject is not an IJobInvoicingPlugIn so should not have Job Related Template Filter Criteria", jobParent);

			JobHeader jobHeader = new JobHeader.Loader(jobParent).TryLoadOrCreate();
			jobHeader.JH_GB = ZGuid.NewZGuid();
			jobHeader.JH_GE = ZGuid.NewZGuid();
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			jobHeader.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			populateJobParent(BusinessObject, consignee, consignor, "AUSYD", "IDJKT");

			ColumnValueRanker ranker = (ColumnValueRanker)BusinessObject.GetTemplateSelectionCriteria();
			AssertArrayEqualsByElements(new object[] { jobHeader.JH_GB, null }, ranker.GetValues(ProcessTaskTemplateSchema.P0_GB));
			AssertArrayEqualsByElements(new object[] { jobHeader.JH_GE, null }, ranker.GetValues(ProcessTaskTemplateSchema.P0_GE));
			object[] clientCriteria = (hasConsigneeConsignor)
				? new object[] { consignor.PK, consignee.PK, jobHeader.LocalChargesPK, ZGuid.Empty }
				: new object[] { jobHeader.LocalChargesPK, ZGuid.Empty };
			AssertArrayEqualsByElements(clientCriteria, ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
			object[] originCriteria = { "AUSYD", "AU", "" };
			AssertArrayEqualsByElements(originCriteria, ranker.GetValues(ProcessTaskTemplateSchema.P0_LoadPortCountry));
			object[] destinationCriteria = { "IDJKT", "ID", "" };
			AssertArrayEqualsByElements(destinationCriteria, ranker.GetValues(ProcessTaskTemplateSchema.P0_DischargePortCountry));

			populateJobParent(BusinessObject, consignee, consignor, "IDJKT", "AUSYD");
			ranker = (ColumnValueRanker)BusinessObject.GetTemplateSelectionCriteria();
			AssertArrayEqualsByElements(new object[] { jobHeader.JH_GB, null }, ranker.GetValues(ProcessTaskTemplateSchema.P0_GB));
			AssertArrayEqualsByElements(new object[] { jobHeader.JH_GE, null }, ranker.GetValues(ProcessTaskTemplateSchema.P0_GE));
			clientCriteria = (hasConsigneeConsignor)
				? new object[] { consignee.PK, consignor.PK, jobHeader.LocalChargesPK, ZGuid.Empty }
				: new object[] { jobHeader.LocalChargesPK, ZGuid.Empty };
			AssertArrayEqualsByElements(clientCriteria, ranker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
			originCriteria = new object[] { "IDJKT", "ID", "" };
			AssertArrayEqualsByElements(originCriteria, ranker.GetValues(ProcessTaskTemplateSchema.P0_LoadPortCountry));
			destinationCriteria = new object[] { "AUSYD", "AU", "" };
			AssertArrayEqualsByElements(destinationCriteria, ranker.GetValues(ProcessTaskTemplateSchema.P0_DischargePortCountry));
		}

		public delegate void PopulateIJobInvoicingPlugInForTest(BusinessObjectT workflowProvider, OrgHeader consignee, OrgHeader consignor, string originCode, string destinationCode);

		#endregion

		#region Test Objects

		protected ProcessTaskTemplate ProcessTaskTemplate
		{
			get
			{
				if (processTaskTemplate == null)
				{
					processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
					processTaskTemplate.WorkflowItems.AddNew();
				}
				return processTaskTemplate;
			}
		}
		ProcessTaskTemplate processTaskTemplate;

		protected OrgHeader Client
		{
			get
			{
				if (client == null)
				{
					client = Factory.NewWithValidTestData<OrgHeader>();
				}
				return client;
			}
		}
		OrgHeader client;

		protected OrgHeader Client2
		{
			get
			{
				if (client2 == null)
				{
					client2 = Factory.NewWithValidTestData<OrgHeader>();
				}
				return client2;
			}
		}
		OrgHeader client2;

		protected GlbBranch Branch
		{
			get
			{
				if (branch == null)
				{
					branch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SYD");
				}
				return branch;
			}
		}
		GlbBranch branch;

		protected GlbBranch Branch2
		{
			get
			{
				if (branch2 == null)
				{
					branch2 = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "BNE");
				}
				return branch2;
			}
		}
		GlbBranch branch2;

		#endregion

		#region Implementation

		protected virtual IWorkflowProvider ReloadWorkflowProvider(BusinessObjectFactory factory, BusinessObjectT workFlowProvider)
		{
			return factory.Load(workFlowProvider.GetType(), workFlowProvider.PK) as IWorkflowProvider;
		}

		protected virtual bool WorkflowProviderDoesNotApplyTemplatesWhenSavingFactory => false;

		ProcessTaskTemplate WorkflowTemplate
		{
			get
			{
				if (workflowTemplate == null)
				{
					workflowTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
					workflowTemplate.P0_ProcessType = WorkflowType;

					ProcessTask task = workflowTemplate.WorkflowItems.AddNew();
					AssertEquals("Task template created", true, task.IsTask);

					ProcessTask milestone = workflowTemplate.WorkflowItems.AddNew();
					milestone.IsMilestone = true;
					AssertEquals("Milestone template created", true, milestone.IsMilestone);
				}
				return workflowTemplate;
			}
		}
		ProcessTaskTemplate workflowTemplate;

		IWorkflowProvider WorkflowProvider
		{
			get { return BusinessObject; }
		}

		protected BusinessObjectT BusinessObject
		{
			get
			{
				if (businessObject == null)
				{
					businessObject = GetNewBusinessObject(Factory);
				}
				return businessObject;
			}
		}
		BusinessObjectT businessObject;

		protected virtual BusinessObjectT GetNewBusinessObject(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<BusinessObjectT>();
		}

		protected virtual void SetupBusinessObjectForDeletion()
		{
		}

		protected virtual ZString WorkflowType
		{
			get
			{
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				IWorkflowProvider provider = GetNewBusinessObject(newFactory);
				return provider.WorkflowItems.WorkflowType;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			MasterFilesTestHelper.ClearWorkflowTables();
		}

		IBMTestHelper BMTestHelper
		{
			get { return bmTestHelper ?? (bmTestHelper = ObjectFactory.Get<IBMTestHelper>()); }
		}

		IBMTestHelper bmTestHelper;

		#endregion
	}
}
