using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Workflow.Business.Test
{
	class WorkflowTemplateAppliedEventTest : WorkflowTestCase
	{
		#region Setup

		WorkflowTemplateAppliedEventTestSetup GetSetup() => new WorkflowTemplateAppliedEventTestSetup(Factory);

		(WorkflowTemplateAppliedEventTestSetup, WorkflowTemplateAppliedEventTestSetup) GetDoubleSetup()
		{
			var setup1 = GetSetup();
			setup1.Template.P0_SubType1 = "BOKKO";
			setup1.AlwaysFallback();
			setup1.AddOneOfEach();

			var setup2 = GetSetup();
			setup2.AlwaysFallback();
			setup2.AddOneOfEach();

			return (setup1, setup2);
		}

		class WorkflowTemplateAppliedEventTestSetup
		{
			public WorkflowTemplateAppliedEventTestSetup(BusinessObjectFactory factory)
			{
				Template = factory.NewWithValidTestData<ProcessTaskTemplate>();
				Template.P0_ProcessType = "DUM";
			}
			int notificationCount;

			public ProcessTaskTemplate Template { get; }

			public ProcessTask AddTemplateTask()
			{
				var task = Template.WorkflowItems.Tasks.AddNew();
				task.P9_Description = "task" + Template.P0_Name + task.P9_Sequence;
				return task;
			}

			public ProcessTask AddTemplateTrigger()
			{
				var trigger = Template.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "trigga" + Template.P0_Name + trigger.P9_Sequence;
				trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
				return trigger;
			}

			public ProcessTask AddTemplateMilestone()
			{
				var milestone = Template.WorkflowItems.Milestones.AddNew();
				milestone.P9_Description = "mil" + Template.P0_Name + milestone.P9_Sequence;
				milestone.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
				return milestone;
			}

			public ProcessTask[] AddOneOfEach()
			{
				return new[]
				{
					AddTemplateTask(),
					AddTemplateTrigger(),
					AddTemplateMilestone(),
				};
			}

			public ProcessTaskNotification AddNotification(ProcessTask triggerable)
			{
				var notification = triggerable.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
				notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
				notification.PQ_EmailAddr = $"earsare{notificationCount++}{Template.PK}@burning.com"; // Notifications have to be distinct to prevent merging.
				return notification;
			}

			public void AlwaysFallback()
			{
				Template.P0_TaskFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
				Template.P0_MilestoneFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
				Template.P0_TriggerFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			}
		}

		static StmALog[] GetWTALogs<T>(T provider)
			where T : BusinessObject, IWorkflowProvider
		{
			return provider.GetLogs().GetAllLogs().Cast<StmALog>().Where(l => l.SL_SE_NKEvent == AutoEvents.WorkflowTemplateAppliedCode).ToArray();
		}

		void AssertApplyingTemplateCreatesWTALogs(string message, int expectedLogs)
		{
			var setup = GetSetup();
			setup.AddTemplateMilestone();
			Factory.Save();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			AssertEquals(message, expectedLogs, GetWTALogs(dummy).Length);
		}

		#endregion

		#region SL_Reference

		public void TestReference_HasTaskFlag()
		{
			var setup = GetSetup();
			setup.AddTemplateTask();
			setup.AddTemplateTask();
			Factory.Save();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();

			AssertContains("SL_Reference shows how many tasks were added", "TSK=2", GetWTALogs(dummy).Single().SL_Reference);
		}

		public void TestReference_HasTriggerFlag()
		{
			var setup = GetSetup();
			setup.AddTemplateTrigger();
			setup.AddTemplateTrigger();
			Factory.Save();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();

			AssertContains("SL_Reference shows how many triggers were added", "TRG=2", GetWTALogs(dummy).Single().SL_Reference);
		}

		public void TestReference_HasMilestoneFlag()
		{
			var setup = GetSetup();
			setup.AddTemplateMilestone();
			setup.AddTemplateMilestone();
			Factory.Save();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();

			AssertContains("SL_Reference shows how many milestones were added", "MIL=2", GetWTALogs(dummy).Single().SL_Reference);
		}

		public void TestReference_HasTemplateName()
		{
			var setup = GetSetup();
			setup.AddTemplateMilestone();
			setup.Template.P0_Name = "Nognog Template";
			Factory.Save();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();

			var log = GetWTALogs(dummy).Single();
			AssertContains(setup.Template.PK.ToString(), log.SL_Reference);
			AssertContains(setup.Template.P0_Name, log.SL_Reference);
		}

		public void TestReference_OnlyNotificationsChange()
		{
			var setup = GetSetup();
			var milestone = setup.AddTemplateMilestone();
			Factory.Save();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();

			var log = GetWTALogs(dummy).Single();
			setup.AddNotification(milestone);
			dummy.ApplyWorkflowTemplates();

			AssertEquals("No new log added yet...", false, log.IsDeleted);
			AssertContains("there is still a log", setup.Template.PK.ToString(), GetWTALogs(dummy).Single().SL_Reference);
		}

		public void TestReference_WhenRegistyDisabled_DoesNotHaveMachineDetails()
		{
			CreateLogAndAssertEnvironmentDetails(false);
		}

		public void TestReference_WhenRegistyEnabled_HasMachineDetails()
		{
			CreateLogAndAssertEnvironmentDetails(true);
		}

		void CreateLogAndAssertEnvironmentDetails(bool enableEnvironmentDetailsRegistryItem)
		{
			var setup = GetSetup();
			setup.AddTemplateMilestone();
			setup.AddTemplateMilestone();

			Factory.Save();

			if (enableEnvironmentDetailsRegistryItem)
			{
				WorkflowDataRegistry.Instance.EnableEnvironmentLoggingOnWorkflowTemplateAppliedEvent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			}

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();

			var log = GetWTALogs(dummy).Single();
			var regex = new Regex(".*\\|MIL=2\\|NAM=.+\\|PID=\\d+\\|TID=\\d+");

			if (enableEnvironmentDetailsRegistryItem)
			{
				AssertMatch(regex, log.SL_Reference);
			}
			else
			{
				AssertNoMatch(regex, log.SL_Reference);
			}
		}

		#endregion

		#region Single Template tests

		public void TestApplyTemplate_FirstTime()
		{
			AssertApplyingTemplateCreatesWTALogs(message: "In the most trivial case there is just one WTA log", expectedLogs: 1);
		}

		public void TestApplyTemplate_ServiceTaskCode()
		{
			var someServiceTaskCode = "IT IS A GOOD TIME TO BE ALIIIIVE";
			using (Env.Instance.TemporaryServiceTaskContext(someServiceTaskCode, false))
			{
				var setup = GetSetup();
				setup.AddTemplateMilestone();
				Factory.Save();
				var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
				dummy.ApplyWorkflowTemplates();
				AssertEquals(someServiceTaskCode, GetWTALogs(dummy).Single().Parameters["SRV"]);
			}
		}

		public void TestApplyTwice()
		{
			var setup = GetSetup();
			setup.AddTemplateMilestone();
			Factory.Save();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			var log = GetWTALogs(dummy).Single();
			dummy.WorkflowItems.DeleteAll();
			dummy.ApplyWorkflowTemplates();

			AssertEquals("The log gets deleted by the new apply.", true, log.IsDeleted);
			AssertEquals("there is still a log", 1, GetWTALogs(dummy).Length);
		}

		public void TestApplySeparateApplicationsForTaskTypes()
		{
			var setup = GetSetup();
			setup.AddTemplateMilestone();
			setup.AddTemplateTrigger();
			setup.AddTemplateTask();
			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals(1, dummy.WorkflowItems.Count);
			AssertEquals("No duplicate logs.", 1, GetWTALogs(dummy).Length);

			dummy.WorkflowItems.Milestones.CreateItemsFromTemplate();
			AssertEquals(2, dummy.WorkflowItems.Count);
			AssertEquals("No duplicate logs.", 1, GetWTALogs(dummy).Length);

			dummy.WorkflowItems.Triggers.CreateItemsFromTemplate();
			AssertEquals(3, dummy.WorkflowItems.Count);
			AssertEquals("No duplicate logs.", 1, GetWTALogs(dummy).Length);
		}

		#endregion

		#region Multi-company Tests

		public void TestMultiCompanySystemWithCurrentCompanyTask()
		{
			var companies = new CompanyTestProvider(Factory, 4);
			WorkflowDataRegistry.Instance.CalculateTemplateUsingCurrentCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var taskReferences = SetupMultiCompanyTasks(companies, true, false);
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.WorkflowInformationProviderOverride = companies.InformationProvider;

			dummy.ApplyWorkflowTemplates();
			var logs = GetWTALogs(dummy).ToArray();
			AssertTasksApplied("Each company gets to have its own task and is logged. Without the current company", 4, logs, taskReferences);
		}

		public void TestMultiCompanySystemWithoutCurrentCompanyTask()
		{
			var companies = new CompanyTestProvider(Factory, 4);
			WorkflowDataRegistry.Instance.CalculateTemplateUsingCurrentCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var taskReferences = SetupMultiCompanyTasks(companies, false, false);

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.WorkflowInformationProviderOverride = companies.InformationProvider;

			dummy.ApplyWorkflowTemplates();
			var logs = GetWTALogs(dummy).ToArray();
			//Test new behaviour which includes other company tasks irrelevant of local one.
			AssertTasksApplied("No local current company task still logs each companies task.", 4, logs, taskReferences);
		}

		List<string> SetupMultiCompanyTasks(CompanyTestProvider companies, bool currentCompanyTask, bool sharedTasks)
		{
			DummyWorkflowDescriptor.Instance.SetAreTasksCompanySpecific(true);
			Factory.Save();

			if (currentCompanyTask)
			{
				var s = GetSetup(); //Prior functionality required a local company task before other company tasks were applied.
				s.Template.GlobalTemplate = false;
				var sTask = s.AddTemplateTask();
				sTask.P9_ShareTasksForAllCompanies = sharedTasks;
			}

			var taskReferences = new List<string>();
			companies.ForAllCompanies(a =>
			{
				var setup = GetSetup();
				setup.Template.GlobalTemplate = false;
				var task = setup.AddTemplateTask();
				taskReferences.Add(task.DescriptionWithReference);
				task.P9_ShareTasksForAllCompanies = sharedTasks;
			});
			Factory.Save();
			return taskReferences;
		}

		public void TestMultiCompanySystemWithSharedTaskAndUseCurrentCompanySet()
		{
			var companies = new CompanyTestProvider(Factory, 4);
			WorkflowDataRegistry.Instance.CalculateTemplateUsingCurrentCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var taskReferences = SetupMultiCompanyTasks(companies, true, true);
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.WorkflowInformationProviderOverride = companies.InformationProvider;

			dummy.ApplyWorkflowTemplates();
			var logs = GetWTALogs(dummy).ToArray();

			AssertTasksApplied("Local Company template applied with shared task.", 1, logs, taskReferences);
			AssertEquals(1, dummy.WorkflowItems.Tasks.Count);
			Assert(dummy.WorkflowItems.Tasks[0].P9_ShareTasksForAllCompanies);
		}

		public void TestMultiCompanySystemWithSharedTasksAcrossCompanies()
		{
			var companies = new CompanyTestProvider(Factory, 4);
			WorkflowDataRegistry.Instance.CalculateTemplateUsingCurrentCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var taskReferences = SetupMultiCompanyTasks(companies, false, true);
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.WorkflowInformationProviderOverride = companies.InformationProvider;

			dummy.ApplyWorkflowTemplates();
			var logs = GetWTALogs(dummy).ToArray();

			AssertTasksApplied("Tasks applied from all companies and all set to shared.", 4, logs, taskReferences);
			AssertEquals(4, dummy.WorkflowItems.Tasks.Count);
			foreach (ProcessTask task in dummy.WorkflowItems.Tasks)
			{
				Assert(task.P9_ShareTasksForAllCompanies);
			}
		}

		void AssertTasksApplied(string message, int taskCount, StmALog[] logs, List<string> taskReferences)
		{
			AssertEquals(message, taskCount, logs.Length);
			foreach (var reference in taskReferences)
			{
				Assert(logs.Select(x => x.SL_Reference.Contains(reference)).Any());
			}
		}

		#endregion

		#region Multi-template Tests

		public void TestApplyTemplate_MultipleTemplates()
		{
			var (setup1, setup2) = GetDoubleSetup();
			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.SubType1 = setup1.Template.P0_SubType1;

			dummy.ApplyWorkflowTemplates();

			AssertEquals("There ought to be separate logs for each template", 2, GetWTALogs(dummy).Length);
			AssertContains("One task", "TSK=1", GetWTALogs(dummy).Single(s => s.SL_Reference.StartsWith(setup1.Template.PK.ToString())).SL_Reference);
			AssertContains("One task", "TSK=1", GetWTALogs(dummy).Single(s => s.SL_Reference.StartsWith(setup2.Template.PK.ToString())).SL_Reference);
		}

		public void TestApplyTwice_MultipleTemplates()
		{
			var (setup1, setup2) = GetDoubleSetup();
			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.SubType1 = setup1.Template.P0_SubType1;

			dummy.ApplyWorkflowTemplates();
			dummy.ApplyWorkflowTemplates();
			dummy.ApplyWorkflowTemplates();

			AssertEquals("There ought to be separate logs for each template", 2, GetWTALogs(dummy).Length);

			Factory.Save();

			dummy.ApplyWorkflowTemplates();
			dummy.ApplyWorkflowTemplates();
			dummy.ApplyWorkflowTemplates();

			AssertEquals("There ought to be separate logs for each template", 2, GetWTALogs(dummy).Length);
		}

		public void TestApplySeparateApplicationsForTaskTypes_MultipleTemplates()
		{
			var (setup1, setup2) = GetDoubleSetup();
			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.SubType1 = setup1.Template.P0_SubType1;
			dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("There ought to be separate logs for each template", 2, GetWTALogs(dummy).Length);
			dummy.WorkflowItems.Triggers.CreateItemsFromTemplate();
			AssertEquals("There ought to be separate logs for each template", 2, GetWTALogs(dummy).Length);
			dummy.WorkflowItems.Milestones.CreateItemsFromTemplate();
			AssertEquals("There ought to be separate logs for each template", 2, GetWTALogs(dummy).Length);

			var log = GetWTALogs(dummy).First();
			AssertContains("TRG=1", log.SL_Reference);
			AssertContains("MIL=1", log.SL_Reference);
			AssertContains("TSK=1", log.SL_Reference);
		}

		#endregion

		#region Template Condition Updates

		public void TestUdfUpdated()
		{
			var setup = GetSetup();
			setup.AddTemplateTask();
			var udfTask = setup.AddTemplateTask();
			udfTask.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			udfTask.TemplateConditions.TemplateCondition2Value = "\"<Z0_Code>\"==\"bob\"";
			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			AssertEquals("We got the log", 1, GetWTALogs(dummy).Length);
			AssertContains("TSK=1", GetWTALogs(dummy).Single().SL_Reference);

			Factory.Save();

			dummy.Z0_Code = "bob";
			dummy.ApplyWorkflowTemplates();
			AssertEquals("We got the log", 2, GetWTALogs(dummy).Length);
			AssertContains("The udf task is considered", "TSK=1", GetWTALogs(dummy).MaxBy(l => l.SL_PostedTimeUtc).SL_Reference);
		}

		#endregion

		#region Edge Cases

		public void TestDeletingEverythingAndStartingAgain_BeforeSave()
		{
			var setup = GetSetup();
			setup.AddTemplateTask();
			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			dummy.WorkflowItems.RemoveAndDeleteAll();
			dummy.ApplyWorkflowTemplates();

			AssertContains(@"
We double-count the template applications.
The alternative to doing this would be keeping track of all of the deletions as well as the adds, but I don't want to do that.",
"TSK=2", GetWTALogs(dummy).Single().SL_Reference);
		}

		public void TestDeletingEverythingAndStartingAgain_AfterSave()
		{
			var setup = GetSetup();
			setup.AddTemplateTask();
			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			Factory.Save();
			dummy.WorkflowItems.RemoveAndDeleteAll();
			dummy.ApplyWorkflowTemplates();

			AssertContains("TSK=1", GetWTALogs(dummy).First().SL_Reference);
			AssertContains("TSK=1", GetWTALogs(dummy).Skip(1).Single().SL_Reference);
		}

		public void TestPartialTemplateApplication()
		{
			var setup = GetSetup();
			setup.AddTemplateTask();
			setup.Template.P0_IsPartialTemplate = true;
			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates(TemplateApplicationParameters.ApplySpecificTemplates(new[] { setup.Template }));
			AssertEquals(1, GetWTALogs(dummy).Length);
		}

		public void TestMultiFactory()
		{
			var setup = GetSetup();
			setup.AddTemplateTask();
			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDummy = newFactory.Load<DummyWithWorkflow>(dummy.PK);
			loadedDummy.ApplyWorkflowTemplates();
			AssertEquals(1, GetWTALogs(loadedDummy).Length);
		}

		public void TestManySaves_DontMakeMillionsOfLogs()
		{
			var setup = GetSetup();
			setup.AddTemplateTask();
			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			Factory.Save();
			dummy.ApplyWorkflowTemplates();
			Factory.Save();
			dummy.ApplyWorkflowTemplates();
			Factory.Save();
			AssertContains("TSK=1", GetWTALogs(dummy).Single().SL_Reference);
		}

		#endregion

		#region Toggle-able

		public void TestRegistryItemToggle()
		{
			// The registry item is on by default because this is a useful feature, however, just in case template application goes *insane*
			// And also because I want to patch this feature back to GP1, this is being added.

			WorkflowDataRegistry.Instance.EnableWorkflowTemplateAppliedEvent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertApplyingTemplateCreatesWTALogs(message: "No logs, since the registry item is disabled", expectedLogs: 0);
		}

		#endregion

		#region MTR Tests

		public void TestMTR_AdditionalActionAdded()
		{
			var setup = GetSetup();
			var trigger = setup.AddTemplateTrigger();
			trigger.TemplateConditions.TemplateCondition2 = "UDF";
			trigger.TemplateConditions.TemplateCondition2Value = "\"1\"==\"1\"";
			setup.AddNotification(trigger);

			Factory.Save();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();

			setup.AddNotification(trigger);

			Factory.Save();

			dummy.ApplyWorkflowTemplates();

			var logs = GetWTALogs(dummy);
			AssertEquals(2, logs.Length);
			AssertNotContains("MTR=1", logs[0].SL_Reference);
			AssertContains("MTR=1", logs[1].SL_Reference);
		}

		public void TestMTR_MultipleTemplatesApplySimultaneously()
		{
			var (setup1, setup2) = GetDoubleSetup();
			var t1 = setup1.AddTemplateTrigger();
			setup1.AddNotification(t1);
			setup1.Template.P0_TriggerFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;

			var t2 = setup2.AddTemplateTrigger();
			t2.P9_Description = t1.P9_Description;
			setup2.AddNotification(t2);
			setup2.Template.P0_TriggerFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;

			Factory.Save();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.SubType1 = "BOKKO"; // The secret subtype.

			dummy.ApplyWorkflowTemplates();

			var logs = GetWTALogs(dummy);
			AssertEquals(2, logs.Length);
			AssertNotContains("MTR=1", logs[0].SL_Reference);
			AssertContains("MTR=1", logs[1].SL_Reference);
		}

		#endregion

		#region MMI Tests

		public void TestMMI_AdditionalActionAdded()
		{
			var setup = GetSetup();
			var milestone = setup.AddTemplateMilestone();
			milestone.TemplateConditions.TemplateCondition2 = "UDF";
			milestone.TemplateConditions.TemplateCondition2Value = "\"1\"==\"1\"";
			setup.AddNotification(milestone);

			Factory.Save();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();

			setup.AddNotification(milestone);

			Factory.Save();

			dummy.ApplyWorkflowTemplates();

			var logs = GetWTALogs(dummy);
			AssertEquals(2, logs.Length);
			AssertNotContains("MMI=1", logs[0].SL_Reference);
			AssertContains("MMI=1", logs[1].SL_Reference);
		}

		public void TestMMI_MultipleTemplatesApplySimultaneously()
		{
			var (setup1, setup2) = GetDoubleSetup();
			var t1 = setup1.AddTemplateMilestone();
			setup1.AddNotification(t1);
			setup1.Template.P0_TriggerFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;

			var t2 = setup2.AddTemplateMilestone();
			t2.P9_Description = t1.P9_Description;
			setup2.AddNotification(t2);
			setup2.Template.P0_TriggerFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;

			Factory.Save();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.SubType1 = "BOKKO"; // The secret subtype.

			dummy.ApplyWorkflowTemplates();

			var logs = GetWTALogs(dummy);
			AssertEquals(2, logs.Length);
			AssertNotContains("MMI=1", logs[0].SL_Reference);
			AssertContains("MMI=1", logs[1].SL_Reference);
		}

		#endregion

		#region MMI/MTR Regresion testing

		public void TestNotificationLog_MultipleApplications()
		{
			var setup = GetSetup();
			var trigger = setup.AddTemplateTrigger();
			trigger.TemplateConditions.TemplateCondition2 = "UDF";
			trigger.TemplateConditions.TemplateCondition2Value = "\"1\"==\"1\"";
			setup.AddNotification(trigger);

			var milestone = setup.AddTemplateMilestone();
			milestone.TriggerConditions.TriggerCondition = "UDF";
			milestone.TriggerConditions.TriggerConditionValue = "\"1\"==\"1\"";
			setup.AddNotification(milestone);

			Factory.Save();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();

			setup.AddNotification(trigger);
			setup.AddNotification(milestone);

			Factory.Save();

			dummy.ApplyWorkflowTemplates();
			dummy.ApplyWorkflowTemplates();
			dummy.ApplyWorkflowTemplates();
			Factory.Save();
			dummy.ApplyWorkflowTemplates();
			dummy.ApplyWorkflowTemplates();
			dummy.ApplyWorkflowTemplates();

			var logs = GetWTALogs(dummy);
			AssertEquals(2, logs.Length);
		}

		public void TestNotificationLog_DifferentFactories()
		{
			var setup = GetSetup();
			var trigger = setup.AddTemplateTrigger();
			trigger.TemplateConditions.TemplateCondition2 = "UDF";
			trigger.TemplateConditions.TemplateCondition2Value = "\"1\"==\"1\"";
			setup.AddNotification(trigger);

			var milestone = setup.AddTemplateMilestone();
			milestone.TemplateConditions.TemplateCondition2 = "UDF";
			milestone.TemplateConditions.TemplateCondition2Value = "\"1\"==\"1\"";
			setup.AddNotification(milestone);

			Factory.Save();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();

			setup.AddNotification(trigger);
			setup.AddNotification(milestone);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			dummy = newFactory.Load<DummyWithWorkflow>(dummy.PK);
			dummy.ApplyWorkflowTemplates();

			var logs = GetWTALogs(dummy);
			AssertEquals(2, logs.Length);
		}
		#endregion
	}
}
