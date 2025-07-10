using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TemplateApplicationRaceConditionHandlerTest : TemplateApplicationTestCase
	{
		public void TestConcurrentEvent_EventSavedBeforeTrigger()
		{
			TestConcurrentEvent(false);
		}

		public void TestConcurrentEvent_TriggerSavedBeforeEvent()
		{
			TestConcurrentEvent(true);
		}

		void TestConcurrentEvent(bool saveTriggerBeforeEvent)
		{
			var template = MakeTemplate(true);
			MakeTrigger(template, Events.CustomisableEvent01Code);
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			Factory.Save();
			var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };

			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(factory1, tryHandleConflictsAutomatically: true));
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(factory2, tryHandleConflictsAutomatically: true));

			var dummy1 = factory1.Load<DummyWithWorkflow>(dummy.PK);
			var dummy2 = factory2.Load<DummyWithWorkflow>(dummy.PK);

			dummy1.ApplyWorkflowTemplates();
			var log = dummy2.Logs.AddNew(Events.CustomisableEvent01);

			if (saveTriggerBeforeEvent)
			{
				factory1.Save();
				factory2.Save();
			}
			else
			{
				factory2.Save();
				factory1.Save();
			}

			AssertEquals(1, dummy.WorkflowItems.Count);
			var trigger = dummy.WorkflowItems.Triggers[0];
			AssertEquals(log.SL_EventTimeOffset, trigger.P9_ActualDateForBinding);
		}

		public void TestConcurrentEvent_InTheFuture_TriggerBeforeEvent()
		{
			TestConcurrentEvent_InTheFuture(true);
		}

		public void TestConcurrentEvent_InTheFuture_EventBeforeTrigger()
		{
			TestConcurrentEvent_InTheFuture(false);
		}

		void TestConcurrentEvent_InTheFuture(bool saveTriggerBeforeEvent)
		{
			WorkflowDataRegistry.Instance.PreventMilestoneFutureActualStart.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var template = MakeTemplate(true);
			MakeMilestone(template, Events.CustomisableEvent01Code);
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			Factory.Save();
			var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };

			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(factory1, tryHandleConflictsAutomatically: true));
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(factory2, tryHandleConflictsAutomatically: true));

			var dummy1 = factory1.Load<DummyWithWorkflow>(dummy.PK);
			var dummy2 = factory2.Load<DummyWithWorkflow>(dummy.PK);

			dummy1.ApplyWorkflowTemplates();
			var log = dummy2.Logs.AddNew(Events.CustomisableEvent01, ZDateTimeOffset.Now.AddDays(2));

			if (saveTriggerBeforeEvent)
			{
				factory1.Save();
				factory2.Save();
			}
			else
			{
				factory2.Save();
				factory1.Save();
			}

			dummy = new BusinessObjectFactory().Load<DummyWithWorkflow>(dummy.PK);
			AssertEquals("Expecting 1 milestone", 1, dummy.WorkflowItems.Milestones.Count);
			AssertEquals("Expecting 1 exception for future event date", 1, dummy.WorkflowItems.Exceptions.Count);
			AssertEquals("Expecting 1 exception for future event date", ProcessWorkflowExceptionType.ExceptionFutureEvent, dummy.WorkflowItems.Exceptions[0].P9_SE_NKExceptionEvent);
		}

		public void TestCheckStmALogDeletedBeforeSaving()
		{
			var template = MakeTemplate(true);
			MakeMilestone(template, Events.CustomisableEvent01Code);
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			Factory.Save();
			var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };

			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(factory1, tryHandleConflictsAutomatically: true));
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(factory2, tryHandleConflictsAutomatically: true));

			var dummy1 = factory1.Load<DummyWithWorkflow>(dummy.PK);
			var dummy2 = factory2.Load<DummyWithWorkflow>(dummy.PK);

			dummy1.ApplyWorkflowTemplates();
			var log1 = dummy2.Logs.AddNew(Events.CustomisableEvent01, ZDateTimeOffset.Now.AddDays(2));
			var log2 = dummy2.Logs.AddNew(Events.CustomisableEvent01, ZDateTimeOffset.Now.AddDays(2));
			factory1.Save();
			var milestone = factory2.Load<ProcessTask>(dummy1.WorkflowItems[0].PK);
			milestone.P9_ActualDateInfo.ValueChanged += (s, e) =>
			{
				log2.Delete();
			};
			AssertNoExceptionThrown(() => factory2.Save());
		}

		public void TestDontConsiderThingsWithTheSameParentIDToBeTheSame()
		{
			// Turn on automatic conflict handling
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(Factory, tryHandleConflictsAutomatically: true));
			// Turn off data refresh bus
			Factory.RefreshEnabled = false;

			var dummy1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var dummy2 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var template = MakeTemplate(true);
			MakeTask(template, false);
			MakeTask(template, false);
			MakeTrigger(template, Events.CustomisableEvent01Code);
			MakeTrigger(template, Events.CustomisableEvent02Code);
			MakeMilestone(template, Events.CustomisableEvent03Code);
			MakeMilestone(template, Events.CustomisableEvent04Code);

			Factory.Save();

			dummy1.ApplyWorkflowTemplates();
			dummy1.Logs.AddNew(Events.CustomisableEvent01); // Add a log so that we get to see this weird conflict manifest

			Factory.Save();
			MakeMilestone(template, Events.CustomisableEvent05Code, udfCondition: "\"1\"==\"1\"");
			Factory.Save();

			dummy1.ApplyWorkflowTemplates();
			dummy2.ApplyWorkflowTemplates();
			AssertNoExceptionThrown(() => Factory.Save());

			AssertEquals(7, dummy1.WorkflowItems.Count);
			AssertEquals(7, dummy2.WorkflowItems.Count);
		}

		public void TestSharedTasksRaceCondition()
		{
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(Factory, tryHandleConflictsAutomatically: true));

				DummyWorkflowDescriptor.Instance.SetAreTasksCompanySpecific(true);
				AssertSharedTasksRaceCondition();

				DummyWorkflowDescriptor.Instance.SetAreTasksCompanySpecific(false);
				AssertSharedTasksRaceCondition();
			}
		}

		void AssertSharedTasksRaceCondition()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			var dummy1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			Factory.Save();

			var template = new BusinessObjectFactory().NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			template.GlobalTemplate = true;

			var task = template.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Shared task";
			task.P9_ShareTasksForAllCompanies = true;

			var task2 = template.WorkflowItems.Tasks.AddNew();
			task2.P9_Description = "Non shared task";
			task2.P9_ShareTasksForAllCompanies = false;

			template.Factory.Save();

			AssertEquals(0, dummy1.WorkflowItems.Count);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var dummy = newFactory.Load<DummyWithWorkflow>(dummy1.PK);
			dummy.ApplyWorkflowTemplates();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var newFactory2 = new BusinessObjectFactory() { RefreshEnabled = false };
				dummy = newFactory2.Load<DummyWithWorkflow>(dummy1.PK);
				dummy.ApplyWorkflowTemplates();
				newFactory2.Save();
			}
			newFactory.Save(); // newFactory2 won the race so this save should not duplicate the task

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				dummy1 = new BusinessObjectFactory().Load<DummyWithWorkflow>(dummy1.PK);

				var query = new ZQuery(ProcessTasksSchema.P9_ParentID, dummy1.PK);
				var tasks = Factory.Load<ProcessTask>(query).Length;

				if (DummyWorkflowDescriptor.Instance.AreTasksCompanySpecific)
				{
					AssertEquals("1 shared task, 1 task specific to this company and 1 task for the other company", 3, tasks);
					AssertEquals(2, dummy1.WorkflowItems.Count);
					AssertEquals("Expecting one shared task and one task for this company", 2, dummy1.WorkflowItems.Where(t => t.P9_ShareTasksForAllCompanies || t.P9_GC == Env.CurrentCompanyPK).Count());
				}
				else
				{
					AssertEquals("Should not create the same task for different companies as task are not company specific", 2, tasks);
					AssertEquals(2, dummy1.WorkflowItems.Count);
				}
			}

			dummy1 = new BusinessObjectFactory().Load<DummyWithWorkflow>(dummy1.PK);
			if (DummyWorkflowDescriptor.Instance.AreTasksCompanySpecific)
			{
				AssertEquals(2, dummy1.WorkflowItems.Count);
				AssertEquals("Expecting one shared task and one task for this company", 2, dummy1.WorkflowItems.Where(t => t.P9_ShareTasksForAllCompanies || t.P9_GC == Env.CurrentCompanyPK).Count());
			}
			else
			{
				AssertEquals(2, dummy1.WorkflowItems.Count);
			}
		}

		[StressTest]
		public void TestHighLoad()
		{
			WorkflowDataRegistry.Instance.MaximumNumberOfWorkflowItemsInTemplateApplication.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3000);
			var dummyFactory1 = Factory.CreateNewFactory();
			var dummyFactory2 = Factory.CreateNewFactory();
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(dummyFactory1, tryHandleConflictsAutomatically: true));
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(dummyFactory2, tryHandleConflictsAutomatically: true));
			dummyFactory1.RefreshEnabled = false;
			dummyFactory2.RefreshEnabled = false;
			Factory.RefreshEnabled = false;

			var template = MakeTemplate(true);
			for (int i = 0; i < 2000; i++)
			{
				MakeTrigger(template);
			}

			var dummy1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			Factory.Save();

			var ld1 = dummyFactory1.Load<DummyWithWorkflow>(dummy1.PK);
			var ld2 = dummyFactory2.Load<DummyWithWorkflow>(dummy1.PK);

			ld1.ApplyWorkflowTemplates();
			ld2.ApplyWorkflowTemplates();
			ld1.Factory.Save();
			ld2.Factory.Save();

			AssertEquals(2000, dummy1.WorkflowItems.Count);
		}

		public void TestSimpleMerge()
		{
			var dummyFactory1 = Factory.CreateNewFactory();
			var dummyFactory2 = Factory.CreateNewFactory();
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(dummyFactory1, tryHandleConflictsAutomatically: true));
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(dummyFactory2, tryHandleConflictsAutomatically: true));
			dummyFactory1.RefreshEnabled = false;
			dummyFactory2.RefreshEnabled = false;
			Factory.RefreshEnabled = false;

			var template = MakeTemplate(true);
			for (int i = 0; i < 100; i++)
			{
				MakeTrigger(template);
			}

			var dummy1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var dummy2 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			Factory.Save();

			var ld1 = dummyFactory1.Load<DummyWithWorkflow>(dummy1.PK);
			var ld2 = dummyFactory2.Load<DummyWithWorkflow>(dummy1.PK);

			var ld3 = dummyFactory1.Load<DummyWithWorkflow>(dummy2.PK);
			var ld4 = dummyFactory2.Load<DummyWithWorkflow>(dummy2.PK);

			ld1.ApplyWorkflowTemplates();
			ld2.ApplyWorkflowTemplates();

			ld3.ApplyWorkflowTemplates();
			ld4.ApplyWorkflowTemplates();

			ld1.Factory.Save();
			ld2.Factory.Save();

			ld3.Factory.Save();
			ld4.Factory.Save();

			AssertEquals(100, dummy1.WorkflowItems.Count);
			AssertEquals(100, dummy2.WorkflowItems.Count);
		}

		public void TestOnDeletedItems()
		{
			var dummyFactory1 = Factory.CreateNewFactory();
			var dummyFactory2 = Factory.CreateNewFactory();
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(dummyFactory1, tryHandleConflictsAutomatically: true));
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(dummyFactory2, tryHandleConflictsAutomatically: true));
			dummyFactory1.RefreshEnabled = false;
			dummyFactory2.RefreshEnabled = false;
			Factory.RefreshEnabled = false;

			var template = MakeTemplate(true);
			MakeTrigger(template);
			MakeTrigger(template);

			var dummy1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			Factory.Save();

			var ld1 = dummyFactory1.Load<DummyWithWorkflow>(dummy1.PK);
			var ld2 = dummyFactory2.Load<DummyWithWorkflow>(dummy1.PK);

			ld1.ApplyWorkflowTemplates();
			ld2.ApplyWorkflowTemplates();
			ld1.Factory.Save();

			ld2.WorkflowItems.Reload(true);
			ld2.WorkflowItems.ToArray().ForEach(f => f.Delete());
			AssertNoExceptionThrown(() => ld2.Factory.Save());
		}

		public void TestTasksLookForSiblingsOnSave_Delete()
		{
			var dummyFactory1 = Factory.CreateNewFactory();
			var dummyFactory2 = Factory.CreateNewFactory();
			// Turn on automatic conflict handling
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(Factory, tryHandleConflictsAutomatically: true));
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(dummyFactory1, tryHandleConflictsAutomatically: true));
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(dummyFactory2, tryHandleConflictsAutomatically: true));
			// Turn off data refresh bus
			dummyFactory1.RefreshEnabled = false;
			dummyFactory2.RefreshEnabled = false;
			Factory.RefreshEnabled = false;

			var dummy1 = dummyFactory1.NewWithValidTestData<DummyWithWorkflow>();
			dummyFactory1.Save();
			var dummy2 = dummyFactory2.Load<DummyWithWorkflow>(dummy1.PK);

			var template = MakeTemplate(true);
			MakeTask(template, false);
			MakeTask(template, false);
			MakeTrigger(template, Events.CustomisableEvent01Code);
			MakeTrigger(template, Events.CustomisableEvent02Code);
			MakeMilestone(template, Events.CustomisableEvent03Code);
			MakeMilestone(template, Events.CustomisableEvent04Code);

			Factory.Save();

			dummy1.ApplyWorkflowTemplates();
			dummy2.ApplyWorkflowTemplates();

			AssertEquals(6, dummy1.WorkflowItems.Count);
			AssertEquals(6, dummy2.WorkflowItems.Count);

			dummyFactory1.Save();
			dummyFactory2.Save();

			var dummy3 = Factory.Load<DummyWithWorkflow>(dummy1.PK);
			AssertEquals(6, dummy3.WorkflowItems.Count);
		}

		public void TestTasksLookForSiblingsOnSave_Validation()
		{
			var dummyFactory1 = Factory.CreateNewFactory();
			var dummyFactory2 = Factory.CreateNewFactory();
			// Turn off automatic conflict handling
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(Factory, tryHandleConflictsAutomatically: false));
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(dummyFactory1, tryHandleConflictsAutomatically: false));
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(dummyFactory2, tryHandleConflictsAutomatically: false));
			// Turn off data refresh bus
			dummyFactory1.RefreshEnabled = false;
			dummyFactory2.RefreshEnabled = false;
			Factory.RefreshEnabled = false;

			var dummy1 = dummyFactory1.NewWithValidTestData<DummyWithWorkflow>();
			dummyFactory1.Save();
			var dummy2 = dummyFactory2.Load<DummyWithWorkflow>(dummy1.PK);

			var template = MakeTemplate(true);
			MakeTask(template, false);
			MakeTask(template, false);
			MakeTrigger(template, Events.CustomisableEvent01Code);
			MakeTrigger(template, Events.CustomisableEvent02Code);
			MakeMilestone(template, Events.CustomisableEvent03Code);
			MakeMilestone(template, Events.CustomisableEvent04Code);

			Factory.Save();

			dummy1.ApplyWorkflowTemplates();
			dummy2.ApplyWorkflowTemplates();

			AssertEquals(6, dummy1.WorkflowItems.Count);
			AssertEquals(6, dummy2.WorkflowItems.Count);

			var itemsExpectingValidationWarnings = dummy2.WorkflowItems.Cast<ProcessTask>().ToArray();

			dummyFactory1.Save();
			dummyFactory2.Save();

			foreach (var item in itemsExpectingValidationWarnings)
			{
				item.Validation.ValidateP9_ParentTemplateID();
				AssertHasWarning(item.P9_ParentTemplateIDInfo, "A very similar row with the same template was detected in the database during save. This may be a duplicate created by another user or service task.");
			}
		}

		public void TestAutomaticallyHandleConflictsInGui()
		{
			// Turn on automatic conflict handling
			WorkflowDataRegistry.Instance.EnableTemplateApplicationConcurrencyProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TemplateApplicationRaceConditionHandlerOptions.Codes.UserInterfaceAndServiceTasks);
			AssertEquals(true, Globals.IsUserInteractive);

			var dummyFactory1 = Factory.CreateNewFactory();
			var dummyFactory2 = Factory.CreateNewFactory();

			// Turn off data refresh bus
			dummyFactory1.RefreshEnabled = false;
			dummyFactory2.RefreshEnabled = false;
			Factory.RefreshEnabled = false;

			var dummy1 = dummyFactory1.NewWithValidTestData<DummyWithWorkflow>();
			dummyFactory1.Save();
			var dummy2 = dummyFactory2.Load<DummyWithWorkflow>(dummy1.PK);

			var template = MakeTemplate(true);
			MakeTask(template, false);
			MakeTask(template, false);
			MakeTrigger(template, Events.CustomisableEvent01Code);
			MakeTrigger(template, Events.CustomisableEvent02Code);
			MakeMilestone(template, Events.CustomisableEvent03Code);
			MakeMilestone(template, Events.CustomisableEvent04Code);

			Factory.Save();

			dummy1.ApplyWorkflowTemplates();
			dummy2.ApplyWorkflowTemplates();

			AssertEquals(6, dummy1.WorkflowItems.Count);
			AssertEquals(6, dummy2.WorkflowItems.Count);

			dummyFactory1.Save();
			dummyFactory2.Save();

			var dummy11 = Factory.Load<DummyWithWorkflow>(dummy1.PK);
			AssertEquals("Tasks should not be duplicated", 6, dummy11.WorkflowItems.Count);
		}

		public void TestAutomaticallyHandleConflictsInGui_WithProcessHeader()
		{
			// Turn on automatic conflict handling
			WorkflowDataRegistry.Instance.EnableTemplateApplicationConcurrencyProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TemplateApplicationRaceConditionHandlerOptions.Codes.UserInterfaceAndServiceTasks);
			AssertEquals(true, Globals.IsUserInteractive);

			var testHelper = ObjectFactory.Get<IBMTestHelper>();
			testHelper.EnableBMSInRegistry();
			testHelper.CreateSystem(Factory, "DUM");

			var dummyFactory1 = Factory.CreateNewFactory();
			var dummyFactory2 = Factory.CreateNewFactory();

			// Turn off data refresh bus
			dummyFactory1.RefreshEnabled = false;
			dummyFactory2.RefreshEnabled = false;
			Factory.RefreshEnabled = false;

			var dummy1 = dummyFactory1.NewWithValidTestData<DummyWithWorkflow>();
			testHelper.GetJobHeaderForParent(dummy1, dummyFactory1, addDefaultProcessHeaderIfNone: false);
			dummyFactory1.Save();
			var dummy2 = dummyFactory2.Load<DummyWithWorkflow>(dummy1.PK);

			var template = MakeTemplate(true);
			var jobHeader = testHelper.GetJobHeaderForParent(template, Factory);
			var workflow1 = testHelper.CreateWorkflow(template, "Workflow 1");
			var workflow2 = testHelper.CreateWorkflow(template, "Workflow 2");
			testHelper.CreateTask(template, workflow1, description: "Task1");
			testHelper.CreateTask(template, workflow2, description: "Task2");

			Factory.Save();

			dummy1.ApplyWorkflowTemplates();
			dummy2.ApplyWorkflowTemplates();

			AssertEquals(2, dummy1.WorkflowItems.Count);
			AssertEquals(2, dummy2.WorkflowItems.Count);
			AssertEquals(2, testHelper.GetJobHeaderForParent(dummy1, dummy1.Factory).ProcessHeaders.Count);
			AssertEquals(2, testHelper.GetJobHeaderForParent(dummy2, dummy2.Factory).ProcessHeaders.Count);

			dummyFactory1.Save();
			dummyFactory2.Save();

			var dummy11 = Factory.Load<DummyWithWorkflow>(dummy1.PK);
			AssertEquals("Tasks should not be duplicated", 2, dummy11.WorkflowItems.Count);
			AssertEquals("Workflows should not be duplicated", 2, testHelper.GetJobHeaderForParent(dummy11, dummy11.Factory).ProcessHeaders.Count);
		}

		public void TestAutomaticallyHandleConflicts_ServiceTasksOnly()
		{
			// Turn on automatic conflict handling
			WorkflowDataRegistry.Instance.EnableTemplateApplicationConcurrencyProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TemplateApplicationRaceConditionHandlerOptions.Codes.ServiceTasksOnly);
			AssertEquals(true, Globals.IsUserInteractive);

			var dummyFactory1 = Factory.CreateNewFactory();
			var dummyFactory2 = Factory.CreateNewFactory();

			// Turn off data refresh bus
			dummyFactory1.RefreshEnabled = false;
			dummyFactory2.RefreshEnabled = false;
			Factory.RefreshEnabled = false;

			var dummy1 = dummyFactory1.NewWithValidTestData<DummyWithWorkflow>();
			dummyFactory1.Save();
			var dummy2 = dummyFactory2.Load<DummyWithWorkflow>(dummy1.PK);

			var template = MakeTemplate(true);
			MakeTask(template, false);
			MakeTask(template, false);
			MakeTrigger(template, Events.CustomisableEvent01Code);
			MakeTrigger(template, Events.CustomisableEvent02Code);
			MakeMilestone(template, Events.CustomisableEvent03Code);
			MakeMilestone(template, Events.CustomisableEvent04Code);

			Factory.Save();

			dummy1.ApplyWorkflowTemplates();
			dummy2.ApplyWorkflowTemplates();

			AssertEquals(6, dummy1.WorkflowItems.Count);
			AssertEquals(6, dummy2.WorkflowItems.Count);

			dummyFactory1.Save();
			dummyFactory2.Save();

			var dummy11 = Factory.Load<DummyWithWorkflow>(dummy1.PK);
			AssertEquals("Tasks will be be duplicated", 12, dummy11.WorkflowItems.Count);
		}

		public void TestAutomaticallyHandleConflicts_TasksCreatedInSaving_ServiceTasksOnly()
		{
			// Turn on automatic conflict handling
			WorkflowDataRegistry.Instance.EnableTemplateApplicationConcurrencyProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TemplateApplicationRaceConditionHandlerOptions.Codes.ServiceTasksOnly);
			AssertEquals(true, Globals.IsUserInteractive);

			var dummyFactory1 = Factory.CreateNewFactory();
			var dummyFactory2 = Factory.CreateNewFactory();

			// Turn off data refresh bus
			dummyFactory1.RefreshEnabled = false;
			dummyFactory2.RefreshEnabled = false;
			Factory.RefreshEnabled = false;

			var dummy1 = dummyFactory1.NewWithValidTestData<DummyWithWorkflow>();
			dummyFactory1.Save();
			var dummy2 = dummyFactory2.Load<DummyWithWorkflow>(dummy1.PK);

			var template = MakeTemplate(true);
			MakeTask(template, false);
			MakeTask(template, false);
			MakeTrigger(template, Events.CustomisableEvent01Code);
			MakeTrigger(template, Events.CustomisableEvent02Code);
			MakeMilestone(template, Events.CustomisableEvent03Code);
			MakeMilestone(template, Events.CustomisableEvent04Code);

			Factory.Save();

			dummy1.ApplyWorkflowTemplates();

			foreach (var workflowItem in dummy1.WorkflowItems)
			{
				AssertEquals("Template is not created during saveing", expected: false, dummy1.WorkflowItems[0].IsCreatedFromTemplateDuringSaving);
			}
			using (BusinessObjectFactory.NotifyIsSavingTogether())
			{
				dummy2.ApplyWorkflowTemplates();
			}
			foreach (var workflowItem in dummy2.WorkflowItems)
			{
				AssertEquals("Template is created during saveing", expected: true, dummy2.WorkflowItems[0].IsCreatedFromTemplateDuringSaving);
			}

			AssertEquals(6, dummy1.WorkflowItems.Count);
			AssertEquals(6, dummy2.WorkflowItems.Count);

			dummyFactory1.Save();
			dummyFactory2.Save();

			var dummy11 = Factory.Load<DummyWithWorkflow>(dummy1.PK);
			AssertEquals("Tasks should not be duplicated", 6, dummy11.WorkflowItems.Count);
		}

		public void TestTasksLookForSiblingsOnSave_ClonedTasksShouldNotComeFromTemplates()
		{
			TemplateApplicationRaceConditionHandler.TryHookupFactory(Factory, tryHandleConflictsAutomatically: false);
			var template = MakeTemplate(true);
			MakeTask(template, false);
			MakeTask(template, false);
			MakeTrigger(template, Events.CustomisableEvent01Code);
			MakeTrigger(template, Events.CustomisableEvent02Code);
			MakeMilestone(template, Events.CustomisableEvent03Code);
			MakeMilestone(template, Events.CustomisableEvent04Code);

			Factory.Save();

			var dummy1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			Factory.Save();
			dummy1.ApplyWorkflowTemplates();

			AssertEquals(6, dummy1.WorkflowItems.Count);
			foreach (var task in dummy1.WorkflowItems.OfType<ProcessTask>().ToArray())
			{
				var clonedTask = TemplateProcessTaskCopier.Clone<DummyProcessTask>(task);
				dummy1.WorkflowItems.SetDefaultsForNewTask(task, false);
				dummy1.WorkflowItems.Add(clonedTask);
			}

			Factory.Save();
			var savedItems = dummy1.WorkflowItems.Cast<ProcessTask>().ToArray();

			AssertEquals("All of the items ought to be in the collection", 12, savedItems.Length);
			foreach (var item in savedItems)
			{
				AssertEquals(false, WorkflowAfterOnSavingBOService.GetTemplateApplicationRaceConditionHandler(Factory).IsSuspectedDuplicate(item));
			}
		}

		public void TestTurnOffRegistryItem_HookupDoesNothing()
		{
			WorkflowDataRegistry.Instance.EnableTemplateApplicationConcurrencyProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TemplateApplicationRaceConditionHandlerOptions.Codes.Off);
			WorkflowDataRegistry.Instance.EnableWorkflowTemplateApplicationConcurrencyProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			TemplateApplicationRaceConditionHandler.TryHookupFactory(Factory, tryHandleConflictsAutomatically: true);
			AssertNull(WorkflowAfterOnSavingBOService.GetTemplateApplicationRaceConditionHandler(Factory));
		}

		public void TestTurnOnRegistryItem_HookupDoesHookup_ServiceTasks()
		{
			WorkflowDataRegistry.Instance.EnableWorkflowTemplateApplicationConcurrencyProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			WorkflowDataRegistry.Instance.EnableTemplateApplicationConcurrencyProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TemplateApplicationRaceConditionHandlerOptions.Codes.ServiceTasksOnly);
			TemplateApplicationRaceConditionHandler.TryHookupFactory(Factory, tryHandleConflictsAutomatically: true);
			AssertNotNull(WorkflowAfterOnSavingBOService.GetTemplateApplicationRaceConditionHandler(Factory));
		}

		public void TestTurnOnRegistryItem_HookupDoesHookup_ServiceTasksAndGui()
		{
			WorkflowDataRegistry.Instance.EnableWorkflowTemplateApplicationConcurrencyProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			WorkflowDataRegistry.Instance.EnableTemplateApplicationConcurrencyProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TemplateApplicationRaceConditionHandlerOptions.Codes.UserInterfaceAndServiceTasks);
			TemplateApplicationRaceConditionHandler.TryHookupFactory(Factory, tryHandleConflictsAutomatically: true);
			AssertNotNull(WorkflowAfterOnSavingBOService.GetTemplateApplicationRaceConditionHandler(Factory));
		}

		public void TestAssumeNoSpooky()
		{
			// Turn on automatic conflict handling
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(Factory, tryHandleConflictsAutomatically: true));

			var template = MakeTemplate(true);
			var templateTask = MakeTask(template, false);

			Factory.Save();

			var otherFactory = Factory.CreateNewFactory();
			Factory.RefreshEnabled = false;
			otherFactory.RefreshEnabled = false;

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();

			AssertEquals("There should be a task", 1, dummy.WorkflowItems.Count);

			var otherTask = otherFactory.New<DummyProcessTask>();
			otherTask.CopyPersistentValuesFrom(dummy.WorkflowItems.Cast<ProcessTask>().First());

			otherTask.Factory.Save(); // Spooky: We save a task on another factory even though the parent isn't saved.
			Factory.Save();

			var reloadedDummy = Factory.CreateNewFactory().Load<DummyWithWorkflow>(dummy.PK);
			AssertEquals("Assume that nobody is creating tasks for parents that are not in the db.", 2, reloadedDummy.WorkflowItems.Count);
		}

		public void TestDoNotModifyListDuringApplication()
		{
			var udfCondition = "\"1\"==\"1\"";
			var template1 = MakeTemplate(true);
			var template2 = MakeTemplate(true);
			SetAllFallback(template1, FallbackTypeList.Codes.AlwaysFallback);
			SetAllFallback(template2, FallbackTypeList.Codes.AlwaysFallback);
			template1.P0_SubType1 = "MIG";
			var m1 = MakeMilestone(template1, Events.CustomisableEvent00Code, udfCondition);
			var m2 = MakeMilestone(template1, Events.CustomisableEvent01Code, udfCondition);
			var m3 = MakeMilestone(template2, Events.CustomisableEvent00Code, udfCondition);
			var m4 = MakeMilestone(template2, Events.CustomisableEvent01Code, udfCondition);
			var m5 = MakeMilestone(template2, Events.CustomisableEvent03Code, udfCondition);

			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.SubType1 = "MIG";
			dummy.ApplyWorkflowTemplates();
			AssertEquals(3, dummy.WorkflowItems.Milestones.Count);

			dummy.WorkflowItems.Milestones.Cast<ProcessTask>().ForEach(m =>
			{
				m.P9_CardNote = "";
				new HiddenUdfConditionNote(m).Text = udfCondition;
			});

			dummy.ApplyWorkflowTemplates();
			AssertEquals(3, dummy.WorkflowItems.Milestones.Count);
		}

		class HiddenUdfConditionNote : HiddenTextNote
		{
			public HiddenUdfConditionNote(IStmNoteParent parent) : base(parent) { }

			protected override ZString Description
			{
				get { return "User Defined Condition"; }
			}
		}

		public void TestNoFutureActualStart_CreateInvalidMilestonesWhereValidationDoesNotOccur()
		{
			WorkflowDataRegistry.Instance.PreventMilestoneFutureActualStart.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var udfCondition = "\"1\"==\"1\"";
			var template1 = MakeTemplate(true);
			var template2 = MakeTemplate(true);
			SetAllFallback(template1, FallbackTypeList.Codes.EmptyFallback);
			SetAllFallback(template2, FallbackTypeList.Codes.AlwaysFallback);

			template1.P0_SubType1 = "MIG";
			var m1 = MakeMilestone(template1, Events.CustomisableEvent00Code, udfCondition);
			var m2 = MakeMilestone(template2, Events.CustomisableEvent03Code, udfCondition);

			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.SubType1 = "MIG";
			dummy.ApplyWorkflowTemplates();
			AssertEquals("Precondition: Template application works", 1, dummy.WorkflowItems.Milestones.Count);
			AssertEquals("Precondition: Got logs", 1, dummy.Logs.Find(l => l.SL_SE_NKEvent == Events.WorkflowTemplateAppliedCode).Count());

			var dummy2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy2.Logs.AddNew(Events.CustomisableEvent00, ZDateTimeOffset.Now.AddDays(6));
			dummy2.SubType1 = "MIG";
			dummy2.ApplyWorkflowTemplates();
			AssertEquals("Create the milestone if it is going to be invalid and raise exception.", 1, dummy2.WorkflowItems.Milestones.Count);
			AssertEquals("WTA logs exist", 1, dummy2.Logs.Find(l => l.SL_SE_NKEvent == Events.WorkflowTemplateAppliedCode).Count());
			AssertEquals("There should be one exception", 1, dummy2.WorkflowItems.Exceptions.Count);
			AssertEquals("There should be a future event exception", ProcessWorkflowExceptionType.ExceptionFutureEvent, dummy2.WorkflowItems.Exceptions[0].P9_SE_NKExceptionEvent);
		}

		public void TestNoFutureActualStart_DoNotCreateInvalidMilestonesWhereValidationDoesNotOccurWithUDF()
		{
			WorkflowDataRegistry.Instance.PreventMilestoneFutureActualStart.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var udfCondition = "\"1\"==\"1\"";
			var template1 = MakeTemplate(true);
			var template2 = MakeTemplate(true);
			SetAllFallback(template1, FallbackTypeList.Codes.EmptyFallback);
			SetAllFallback(template2, FallbackTypeList.Codes.AlwaysFallback);

			template1.P0_SubType1 = "MIG";
			var m1 = MakeMilestone(template1, Events.CustomisableEvent00Code, udfCondition);
			m1.TriggerConditions.TriggerCondition = "UDF";
			m1.TriggerConditions.TriggerConditionValue = "\"1\" == \"1\"";

			var m2 = MakeMilestone(template2, Events.CustomisableEvent03Code, udfCondition);
			m2.TriggerConditions.TriggerCondition = "UDF";
			m2.TriggerConditions.TriggerConditionValue = "\"2\" == \"2\"";

			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.SubType1 = "MIG";
			dummy.ApplyWorkflowTemplates();
			AssertEquals("Precondition: Template application works", 1, dummy.WorkflowItems.Milestones.Count);
			AssertEquals("Precondition: Got logs", 1, dummy.Logs.Find(l => l.SL_SE_NKEvent == Events.WorkflowTemplateAppliedCode).Count());

			var dummy2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy2.Logs.AddNew(Events.CustomisableEvent00, ZDateTimeOffset.Now.AddDays(6));
			dummy2.SubType1 = "MIG";
			dummy2.ApplyWorkflowTemplates();
			AssertEquals("Create the milestone if it is going to be invalid and raise exception.", 1, dummy2.WorkflowItems.Milestones.Count);
			AssertEquals("WTA logs exist", 1, dummy2.Logs.Find(l => l.SL_SE_NKEvent == Events.WorkflowTemplateAppliedCode).Count());
			AssertEquals("There should be one exception", 1, dummy2.WorkflowItems.Exceptions.Count);
			AssertEquals("There should be a future event exception", ProcessWorkflowExceptionType.ExceptionFutureEvent, dummy2.WorkflowItems.Exceptions[0].P9_SE_NKExceptionEvent);
		}

		public void TestNoExceptionIsRaisedForMilestonesWhichAlreadyExist()
		{
			WorkflowDataRegistry.Instance.PreventMilestoneFutureActualStart.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var udfCondition = "\"1\"==\"1\"";
			var template1 = MakeTemplate(true);
			SetAllFallback(template1, FallbackTypeList.Codes.EmptyFallback);

			template1.P0_SubType1 = "MIG";
			var m1 = MakeMilestone(template1, Events.CustomisableEvent00Code, udfCondition);

			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.SubType1 = "MIG";
			dummy.ApplyWorkflowTemplates();
			dummy.Logs.AddNew(Events.CustomisableEvent00, ZDateTimeOffset.Now.AddDays(6));

			Factory.Save();

			var m1_load = (ProcessTask)template1.WorkflowItems.Milestones.First();
			var action = m1_load.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			action.PQ_EmailAddr = "alex@aaa.com";

			Factory.Save();

			WorkflowDataRegistry.Instance.PreventMilestoneFutureActualStart.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			dummy.ApplyWorkflowTemplates();

			AssertEquals("Precondition: Template application works", 1, dummy.WorkflowItems.Milestones.Count);
			AssertEquals("Precondition: Actions Merged", 1, ((ProcessTask)dummy.WorkflowItems.Milestones.First()).ProcessTaskNotifications.Count);
			AssertEquals("There should be no exception", 0, dummy.WorkflowItems.Exceptions.Count);
		}

		public void TestDoNotReloadExistingRows()
		{
			var udfCondition = "\"1\"==\"1\"";
			var template = MakeTemplate(true);
			var trigger = MakeTrigger(template, Events.CustomisableEvent00Code, udfCondition);
			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			Factory.Save();

			AssertEquals(1, dummy.WorkflowItems.Triggers.Count);
			var triggerOnDummy = dummy.WorkflowItems.Triggers[0];
			triggerOnDummy.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;
			triggerOnDummy.P9_ParentTemplateID = ZGuid.Empty;
			dummy.ApplyWorkflowTemplates();
			Factory.Save();

			AssertEquals(2, dummy.WorkflowItems.Triggers.Count);
			AssertEquals(1, dummy.WorkflowItems.Triggers.Where(t => t is ProcessTask pt && pt.TriggerConditions.TriggerEventCode == Events.CustomisableEvent01Code).Count());
			AssertEquals(1, dummy.WorkflowItems.Triggers.Where(t => t is ProcessTask pt && pt.TriggerConditions.TriggerEventCode == Events.CustomisableEvent00Code).Count());
		}

		public void TestProcessBusinesObjects_CallsWorkflowTemplateApplicationRaceConditionHandlerBeforeProcessTasks()
		{
			WorkflowDataRegistry.Instance.EnableTemplateApplicationRaceConditionHandlerProcessTasksLock.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var workflowTemplateApplicationRaceConditionHandlerMock = new Mock<ITemplateApplicationRaceConditionHandler>();
			ObjectFactory.Substitute("WorkflowTemplateApplicationRaceConditionHandler", workflowTemplateApplicationRaceConditionHandlerMock.Object);
			TemplateApplicationRaceConditionHandler.TryHookupFactory(Factory, tryHandleConflictsAutomatically: true);
			var processTasksRaceConditionHandler = WorkflowAfterOnSavingBOService.GetTemplateApplicationRaceConditionHandler(Factory).ProcessTasksRaceConditionHandler;

			var template = MakeTemplate(true);
			MakeTask(template);

			var beforeProcessTasksForTestWasCalled = false;

			processTasksRaceConditionHandler.OnBeforeProcess += (_, __) =>
			{
				workflowTemplateApplicationRaceConditionHandlerMock.Verify(h => h.Process(It.IsAny<IEnumerable<BusinessObject>>()), Times.Once);
				beforeProcessTasksForTestWasCalled = true;
			};

			AssertNoExceptionThrown(() => Factory.Save());
			Assert(beforeProcessTasksForTestWasCalled);
		}

		public void TestShouldDeleteDuplicateIfItDoesNotHaveActualDateButOriginalDoes()
		{
			var dummyFactory1 = Factory.CreateNewFactory();
			var dummyFactory2 = Factory.CreateNewFactory();
			// Turn on automatic conflict handling
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(Factory, tryHandleConflictsAutomatically: true));
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(dummyFactory1, tryHandleConflictsAutomatically: true));
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(dummyFactory2, tryHandleConflictsAutomatically: true));
			// Turn off data refresh bus
			dummyFactory1.RefreshEnabled = false;
			dummyFactory2.RefreshEnabled = false;
			Factory.RefreshEnabled = false;

			var dummy1 = dummyFactory1.NewWithValidTestData<DummyWithWorkflow>();
			dummyFactory1.Save();
			var dummy2 = dummyFactory2.Load<DummyWithWorkflow>(dummy1.PK);

			var template = MakeTemplate(true);
			MakeTrigger(template, AutoEvents.CustomisableEvent01Code);
			Factory.Save();

			dummy1.ApplyWorkflowTemplates();
			dummy1.Logs.AddNew(AutoEvents.CustomisableEvent01); // Add a log so that an unreconcilable concurrency error occurs.
			dummy2.ApplyWorkflowTemplates();

			dummyFactory1.Save();

			AssertNoExceptionThrown(() => dummyFactory2.Save());

			//reload trigger and check p9_actual date is same as StmALog
			var reloadedDummy1 = new BusinessObjectFactory().Load<DummyWithWorkflow>(dummy1.PK);
			var firedTrigger = reloadedDummy1.WorkflowItems.Triggers.First() as ProcessTask;
			var createdLog = reloadedDummy1.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.CustomisableEvent01Code).First();

			AssertEquals(0, createdLog?.SL_EventTime.ToSmallDateTime().CompareTo(firedTrigger?.P9_ActualDate.ToSmallDateTime()));
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestShouldDeleteDuplicateIfItDoesHaveActualDateButOriginalDoesNotAndUpdateOriginal()
		{
			var dummyFactory1 = Factory.CreateNewFactory();
			var dummyFactory2 = Factory.CreateNewFactory();
			// Turn on automatic conflict handling
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(Factory, tryHandleConflictsAutomatically: true));
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(dummyFactory1, tryHandleConflictsAutomatically: true));
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(dummyFactory2, tryHandleConflictsAutomatically: true));
			// Turn off data refresh bus
			dummyFactory1.RefreshEnabled = false;
			dummyFactory2.RefreshEnabled = false;
			Factory.RefreshEnabled = false;

			var dummy1 = dummyFactory1.NewWithValidTestData<DummyWithWorkflow>();
			dummyFactory1.Save();
			var dummy2 = dummyFactory2.Load<DummyWithWorkflow>(dummy1.PK);

			var template = MakeTemplate(true);
			MakeTrigger(template, AutoEvents.CustomisableEvent01Code);
			Factory.Save();

			dummy1.ApplyWorkflowTemplates();
			dummy2.Logs.AddNew(AutoEvents.CustomisableEvent01); // Add a log so that an unreconcilable concurrency error occurs.
			dummy2.ApplyWorkflowTemplates();

			dummyFactory1.Save();

			AssertNoExceptionThrown(() => dummyFactory2.Save());

			//reload trigger and check p9_actual date is same as StmALog
			var reloadedDummy1 = new BusinessObjectFactory().Load<DummyWithWorkflow>(dummy1.PK);
			var firedTrigger = reloadedDummy1.WorkflowItems.Triggers.First() as ProcessTask;
			var createdLog = reloadedDummy1.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.CustomisableEvent01Code).First();

			AssertEquals(0, createdLog?.SL_EventTime.ToSmallDateTime().CompareTo(firedTrigger?.P9_ActualDate.ToSmallDateTime()));
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestShouldDeleteDuplicateIfItDoesHaveActualDateAndOriginalAlsoHasActualDate()
		{
			var dummyFactory1 = Factory.CreateNewFactory();
			var dummyFactory2 = Factory.CreateNewFactory();
			// Turn on automatic conflict handling
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(Factory, tryHandleConflictsAutomatically: true));
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(dummyFactory1, tryHandleConflictsAutomatically: true));
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(dummyFactory2, tryHandleConflictsAutomatically: true));
			// Turn off data refresh bus
			dummyFactory1.RefreshEnabled = false;
			dummyFactory2.RefreshEnabled = false;
			Factory.RefreshEnabled = false;

			var dummy1 = dummyFactory1.NewWithValidTestData<DummyWithWorkflow>();
			dummyFactory1.Save();
			var dummy2 = dummyFactory2.Load<DummyWithWorkflow>(dummy1.PK);

			var template = MakeTemplate(true);
			MakeTrigger(template, AutoEvents.CustomisableEvent01Code);
			Factory.Save();

			dummy1.ApplyWorkflowTemplates();
			dummy2.Logs.AddNew(AutoEvents.CustomisableEvent01); // Add a log so that an unreconcilable concurrency error occurs.
			Thread.Sleep(100); // sleeping to ensure the two dates are always different as we capture to millisecond not nano
			dummy1.Logs.AddNew(AutoEvents.CustomisableEvent01); // Add a log so that an unreconcilable concurrency error occurs.
			dummy2.ApplyWorkflowTemplates();

			dummyFactory1.Save();

			AssertNoExceptionThrown(() => dummyFactory2.Save());

			//reload trigger and check p9_actual date is same as StmALog
			var reloadedDummy1 = new BusinessObjectFactory().Load<DummyWithWorkflow>(dummy1.PK);
			var firedTrigger = reloadedDummy1.WorkflowItems.Triggers.First() as ProcessTask;
			var createdLog = reloadedDummy1.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.CustomisableEvent01Code).First();

			AssertEquals(0, createdLog?.SL_EventTime.ToSmallDateTime().CompareTo(firedTrigger?.P9_ActualDate.ToSmallDateTime()));
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestShouldDeleteDuplicateIfItDoesNotHaveActualDateAndOriginalAlsoDoesNot()
		{
			var dummyFactory1 = Factory.CreateNewFactory();
			var dummyFactory2 = Factory.CreateNewFactory();
			// Turn on automatic conflict handling
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(Factory, tryHandleConflictsAutomatically: true));
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(dummyFactory1, tryHandleConflictsAutomatically: true));
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(dummyFactory2, tryHandleConflictsAutomatically: true));
			// Turn off data refresh bus
			dummyFactory1.RefreshEnabled = false;
			dummyFactory2.RefreshEnabled = false;
			Factory.RefreshEnabled = false;

			var dummy1 = dummyFactory1.NewWithValidTestData<DummyWithWorkflow>();
			dummyFactory1.Save();
			var dummy2 = dummyFactory2.Load<DummyWithWorkflow>(dummy1.PK);

			var template = MakeTemplate(true);
			MakeTrigger(template, AutoEvents.CustomisableEvent01Code);
			Factory.Save();

			dummy1.ApplyWorkflowTemplates();
			dummy2.ApplyWorkflowTemplates();

			dummyFactory1.Save();

			AssertNoExceptionThrown(() => dummyFactory2.Save());

			//reload trigger and check p9_actual date is same as StmALog
			var reloadedDummy1 = new BusinessObjectFactory().Load<DummyWithWorkflow>(dummy1.PK);
			var firedTrigger = reloadedDummy1.WorkflowItems.Triggers.First() as ProcessTask;
			Assert(firedTrigger?.P9_ActualDate.IsEmpty ?? false);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}
	}

	class TemplateApplicationRaceConditionHandlerProcessTasksLockTests : TestCase
	{
		[UseSnapshotProtection]
		public void TestProcessTasksBlocked()
		{
			WorkflowDataRegistry.Instance.EnableTemplateApplicationRaceConditionHandlerProcessTasksLock.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			DbEnv.SetDbEnvironment(new TestEnvironment());

			var factory = new BusinessObjectFactory();

			var mrseOne = new ManualResetEvent(false);
			var mrseTwo = new ManualResetEvent(false);
			var mrseThree = new ManualResetEvent(false);

			var template = factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "SHP";
			template.GlobalTemplate = true;

			for (int i = 0; i < 5; i++)
			{
				var trigger = template.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "R" + trigger.P9_Sequence;
				trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			}

			var shipment = factory.New<IForwardingShipment>();

			factory.RefreshEnabled = false;

			var factory1 = factory.CreateNewFactory();
			factory1.RefreshEnabled = false;
			factory1.RelinquishThreadOwnership();

			var factory2 = factory.CreateNewFactory();
			factory2.RefreshEnabled = false;
			factory2.RelinquishThreadOwnership();

			factory.Save();

			var threadOneHit = false;
			var threadTwoHit = false;

			var threadOne = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					factory1.TakeThreadOwnership();
					TemplateApplicationRaceConditionHandler.TryHookupFactory(factory1, tryHandleConflictsAutomatically: true);
					var processTasksRaceConditionHandlerInFactory1 = WorkflowAfterOnSavingBOService.GetTemplateApplicationRaceConditionHandler(factory1).ProcessTasksRaceConditionHandler;

					var query = new ZQuery();
					query.AddToFilter(JobShipmentSchema.PK, shipment.PK);
					query.IsNoLock = true;

					var ld1 = factory1.Load<IForwardingShipment>(query).First();

					((IWorkflowProvider)ld1).ApplyWorkflowTemplates();

					processTasksRaceConditionHandlerInFactory1.OnAfterLock += (_, __) =>
					{
						mrseTwo.Set();
						threadOneHit = true;
						mrseOne.WaitOne();
					};

					((IBusiness)ld1).Factory.Save();
				}
			});

			var threadTwo = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					factory2.TakeThreadOwnership();
					TemplateApplicationRaceConditionHandler.TryHookupFactory(factory2, tryHandleConflictsAutomatically: true);
					var processTasksRaceConditionHandlerInFactory2 = WorkflowAfterOnSavingBOService.GetTemplateApplicationRaceConditionHandler(factory2).ProcessTasksRaceConditionHandler;

					var query = new ZQuery();
					query.AddToFilter(JobShipmentSchema.PK, shipment.PK);
					query.IsNoLock = true;

					var ld1 = factory2.Load<IForwardingShipment>(query).First();

					((IWorkflowProvider)ld1).ApplyWorkflowTemplates();

					processTasksRaceConditionHandlerInFactory2.OnBeforeLock += (_, __) =>
					{
						mrseThree.Set();
					};

					processTasksRaceConditionHandlerInFactory2.OnAfterLock += (_, __) =>
					{
						threadTwoHit = true;
					};

					((IBusiness)ld1).Factory.Save();
				}
			});

			threadOne.Start();

			mrseTwo.WaitOne();

			threadTwo.Start();

			mrseThree.WaitOne();

			Thread.Sleep(5000);

			AssertEquals(true, threadOneHit);
			AssertEquals(false, threadTwoHit);

			mrseOne.Set();

			mrseTwo.Set();

			threadOne.Join();
			threadTwo.Join();

			AssertEquals(5, ((IWorkflowProvider)shipment).WorkflowItems.Count);

			AssertEquals(true, threadOneHit);
			AssertEquals(true, threadTwoHit);
		}
	}

	class TestEnvironment : BaseDbEnvironment
	{
		public override int ConnectionTimeout => 5;
	}
}
