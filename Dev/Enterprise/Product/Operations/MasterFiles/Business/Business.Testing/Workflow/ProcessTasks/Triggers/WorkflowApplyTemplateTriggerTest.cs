using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class WorkflowApplyTemplateTriggerTest : TestCaseWithFactory
	{
		public void TestApplyTemplateTrigger_WithMatchingWorkflow_ShouldAddTasksToWorkflow()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, "DUM");

			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			template.P0_Name = "Added by Trigger";
			var templateWorkflow = template.ProcessHeaders.AddNew();
			templateWorkflow.FH_CompletionStatement = "Workflow 1";
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_FH_ProcessHeader = templateWorkflow.PK;
			templateTask.P9_Description = "This is a new task!";

			var jobHeader = helper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var dummy = (DummyWithWorkflow)jobHeader.Parent;
			var dummyWorkflow = jobHeader.ProcessHeaders.AddNew();
			dummyWorkflow.FH_CompletionStatement = "Workflow 1";
			helper.CreateTask(dummyWorkflow, taskType: "ABC", description: "This task was already here.");

			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "trigger 1";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomsEntryStatus.Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			action.PQ_P0_WorkflowTemplate = template.PK;

			dummy.Logs.AddNew(Events.CustomsEntryStatus);

			Factory.Save();

			AssertEquals(1, dummy.WorkflowItems.Tasks.Count);

			var notifications = new NotificationsForTest();
			new WorkflowApplyTemplateProcessor(action, dummy).Process(notifications);
			var expectedLog = ApplyTemplateProcessorLogger.GetTemplateApplicationResultLog(dummy.JobNumber, template.P0_Name, TemplateApplicationResult.Success).Log;
			AssertContains(expectedLog, notifications.ToString());

			AssertEquals(2, jobHeader.ProcessHeaders.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "This task was already here.", "This is a new task!" }, dummyWorkflow.Tasks.Select(x => x.P9_Description.ToString()));
			var newTask = (ProcessTask)dummyWorkflow.Tasks.Single(x => x.P9_Description == "This is a new task!");
			AssertEquals(template.PK, newTask.SourceTemplatePK);

			notifications = new NotificationsForTest();
			new WorkflowApplyTemplateProcessor(action, dummy).Process(notifications);
			expectedLog = ApplyTemplateProcessorLogger.GetTemplateApplicationResultLog(dummy.JobNumber, template.P0_Name, TemplateApplicationResult.NoNewMatchingTemplateItems).Log;
			AssertContains("Template has already been applied", expectedLog, notifications.ToString());
		}

		public void TestTwoPartialTemplateTriggerActionsOnTheSameTrigger()
		{
			var factory = new BusinessObjectFactory();

			var partialTemplate1 = factory.NewWithValidTestData<ProcessTaskTemplate>();
			partialTemplate1.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			partialTemplate1.P0_IsPartialTemplate = true;
			var trigger1 = partialTemplate1.WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "TEST 1";
			trigger1.TriggerConditions.TriggerEventCode = "Z00";

			var partialTemplate2 = factory.NewWithValidTestData<ProcessTaskTemplate>();
			partialTemplate2.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			partialTemplate2.P0_IsPartialTemplate = true;
			var trigger2 = partialTemplate2.WorkflowItems.Triggers.AddNew();
			trigger2.P9_Description = "TEST 2";
			trigger2.TriggerConditions.TriggerEventCode = "Z00";

			var template = factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;

			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "TEST 3";
			trigger.TriggerConditions.TriggerEventCode = "Z00";
			var action2 = trigger.ProcessTaskNotifications.AddNew();
			action2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			action2.PQ_P0_WorkflowTemplate = partialTemplate2.PK;

			var action1 = trigger.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			action1.PQ_P0_WorkflowTemplate = partialTemplate1.PK;

			factory.Save();

			var shipment = factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "ABC";
			factory.Save();

			var workflowProvider = shipment as IWorkflowProvider;
			AssertEquals("Template should create trigger", 1, workflowProvider.WorkflowItems.Triggers.Count);

			workflowProvider.Logs.AddNew(Events.CustomisableEvent00);
			factory.Save();

			var res = MasterFilesTestHelper.RunLogWalker();

			shipment = new BusinessObjectFactory().Load<IForwardingShipment>(shipment.PK);
			workflowProvider = shipment as IWorkflowProvider;
			AssertEquals("Expecting 3 triggers", 3, workflowProvider.WorkflowItems.Triggers.Count);
		}

		public void TestTwoPartialTemplateTriggerActionsOnDifferentTriggers()
		{
			var factory = new BusinessObjectFactory();

			var partialTemplate1 = factory.NewWithValidTestData<ProcessTaskTemplate>();
			partialTemplate1.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			partialTemplate1.P0_IsPartialTemplate = true;
			var trigger1 = partialTemplate1.WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "TEST 1";
			trigger1.TriggerConditions.TriggerEventCode = "Z00";
			var action11 = trigger1.ProcessTaskNotifications.AddNew();
			action11.PQ_TriggerType = "XUT";

			var partialTemplate2 = factory.NewWithValidTestData<ProcessTaskTemplate>();
			partialTemplate2.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			partialTemplate2.P0_IsPartialTemplate = true;
			var trigger2 = partialTemplate2.WorkflowItems.Triggers.AddNew();
			trigger2.P9_Description = "TEST 2";
			trigger2.TriggerConditions.TriggerEventCode = "Z00";
			var action12 = trigger2.ProcessTaskNotifications.AddNew();
			action12.PQ_TriggerType = "XUT";

			var template = factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;

			var trigger3 = template.WorkflowItems.Triggers.AddNew();
			trigger3.P9_Description = "TEST 3";
			trigger3.TriggerConditions.TriggerEventCode = "Z00";
			var action1 = trigger3.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			action1.PQ_P0_WorkflowTemplate = partialTemplate2.PK;

			var trigger4 = template.WorkflowItems.Triggers.AddNew();
			trigger4.P9_Description = "TEST 4";
			trigger4.TriggerConditions.TriggerEventCode = "Z00";
			var action2 = trigger4.ProcessTaskNotifications.AddNew();
			action2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			action2.PQ_P0_WorkflowTemplate = partialTemplate1.PK;

			factory.Save();

			var shipment = factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "ABC";
			factory.Save();

			var workflowProvider = shipment as IWorkflowProvider;
			AssertEquals("Template should create trigger", 2, workflowProvider.WorkflowItems.Triggers.Count);

			workflowProvider.Logs.AddNew(Events.CustomisableEvent00);
			factory.Save();

			new WorkflowApplyTemplateProcessor(action1, workflowProvider).Process(new NotificationsForTest());
			new WorkflowApplyTemplateProcessor(action2, workflowProvider).Process(new NotificationsForTest());
			factory.Save();

			shipment = new BusinessObjectFactory().Load<IForwardingShipment>(shipment.PK);
			workflowProvider = shipment as IWorkflowProvider;
			AssertEquals("Expecting 4 triggers", 4, workflowProvider.WorkflowItems.Triggers.Count);
		}

		public void TestGetTemplateApplicationUnsupportedISometimesWorkflowProviderResult()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			var jobHeader = helper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var dummy = (DummyWithWorkflow)jobHeader.Parent;
			var template = Factory.New<ProcessTaskTemplate>();
			ApplyTemplateProcessorLogger.GetTemplateApplicationResultLog(dummy.JobNumber, template.P0_Name, TemplateApplicationResult.UnsupportedISometimesWorkflowProvider);

			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestReportIfTemplateApplicationIsSuspended()
		{
			var factory = new BusinessObjectFactory();

			var partialTemplate1 = factory.NewWithValidTestData<ProcessTaskTemplate>();
			partialTemplate1.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			partialTemplate1.P0_IsPartialTemplate = true;
			var trigger = partialTemplate1.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "TEST 1";
			trigger.TriggerConditions.TriggerEventCode = "Z00";
			var action11 = trigger.ProcessTaskNotifications.AddNew();
			action11.PQ_TriggerType = "XUT";

			var template = factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			var trigger2 = template.WorkflowItems.Triggers.AddNew();
			trigger2.P9_Description = "TEST 4";
			trigger2.TriggerConditions.TriggerEventCode = "Z00";
			var action = trigger2.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			action.PQ_P0_WorkflowTemplate = partialTemplate1.PK;

			factory.Save();

			var shipment = factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "ABC";
			factory.Save();

			var workflowProvider = shipment as IWorkflowProvider;
			AssertEquals("Template should create trigger", 1, workflowProvider.WorkflowItems.Triggers.Count);

			workflowProvider.Logs.AddNew(Events.CustomisableEvent00);
			factory.Save();

			var notifications = new NotificationsForTest();
			using (ProcessTask.Loader.SuppressTemplateApplication())
			{
				new WorkflowApplyTemplateProcessor(action, workflowProvider).Process(notifications);
			}
			var expectedLog = ApplyTemplateProcessorLogger.GetTemplateApplicationResultLog(shipment.JS_UniqueConsignRef, partialTemplate1.P0_Name, TemplateApplicationResult.TemplateApplicationSuspended).Log;
			AssertContains(expectedLog, notifications.ToString());

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestLoggingIfJobCancelled()
		{
			var factory = new BusinessObjectFactory();

			var partialTemplate1 = factory.NewWithValidTestData<ProcessTaskTemplate>();
			partialTemplate1.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			partialTemplate1.P0_IsPartialTemplate = true;
			var trigger = partialTemplate1.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "TEST 1";
			trigger.TriggerConditions.TriggerEventCode = "Z00";

			var template = factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			var trigger2 = template.WorkflowItems.Triggers.AddNew();
			trigger2.P9_Description = "TEST 4";
			trigger2.TriggerConditions.TriggerEventCode = "Z00";
			var action = trigger2.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			action.PQ_P0_WorkflowTemplate = partialTemplate1.PK;

			factory.Save();

			var shipment = factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "ABC";
			factory.Save();

			var workflowProvider = shipment as IWorkflowProvider;
			AssertEquals("Template should create trigger", 1, workflowProvider.WorkflowItems.Triggers.Count);

			shipment.JS_IsCancelled = true;
			var notifications = new NotificationsForTest();

			new WorkflowApplyTemplateProcessor(action, workflowProvider).Process(notifications);
			var expectedLog = ApplyTemplateProcessorLogger.GetTemplateApplicationResultLog(shipment.JS_UniqueConsignRef, partialTemplate1.P0_Name, TemplateApplicationResult.JobCancelled).Log;
			AssertContains(expectedLog, notifications.ToString());
		}

		public void TestLoggingIfJobDeleted()
		{
			var factory = new BusinessObjectFactory();

			var partialTemplate1 = factory.NewWithValidTestData<ProcessTaskTemplate>();
			partialTemplate1.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			partialTemplate1.P0_IsPartialTemplate = true;
			var trigger = partialTemplate1.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "TEST 1";
			trigger.TriggerConditions.TriggerEventCode = "Z00";

			var template = factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.DummyWorkflowDescriptorCode;
			var trigger2 = template.WorkflowItems.Triggers.AddNew();
			trigger2.P9_Description = "TEST 4";
			trigger2.TriggerConditions.TriggerEventCode = "Z00";
			var action = trigger2.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			action.PQ_P0_WorkflowTemplate = partialTemplate1.PK;

			factory.Save();

			var dummy = factory.New<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			factory.Save();

			var workflowProvider = dummy as IWorkflowProvider;
			AssertEquals("Template should create trigger", 1, workflowProvider.WorkflowItems.Triggers.Count);

			dummy.Delete();
			var notifications = new NotificationsForTest();

			new WorkflowApplyTemplateProcessor(action, workflowProvider).Process(notifications);
			var expectedLog = ApplyTemplateProcessorLogger.GetTemplateApplicationResultLog(dummy.JobNumber, partialTemplate1.P0_Name, TemplateApplicationResult.JobDeleted).Log;
			AssertContains(expectedLog, notifications.ToString());
			ErrorReporter.Clear();
		}

		public void TestPartialTemplateAppliedAnotherPartialTemplate()
		{
			var factory = new BusinessObjectFactory();

			var partialTemplate1 = factory.NewWithValidTestData<ProcessTaskTemplate>();
			partialTemplate1.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			partialTemplate1.P0_IsPartialTemplate = true;
			var trigger1 = partialTemplate1.WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "TEST 1";
			trigger1.TriggerConditions.TriggerEventCode = "Z00";

			var partialTemplate2 = factory.NewWithValidTestData<ProcessTaskTemplate>();
			partialTemplate2.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			partialTemplate2.P0_IsPartialTemplate = true;
			var tmpTrigger = partialTemplate2.WorkflowItems.Triggers.AddNew();
			tmpTrigger.P9_Description = "TEST 2";
			tmpTrigger.TriggerConditions.TriggerEventCode = "Z00";
			var action1 = tmpTrigger.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			action1.PQ_P0_WorkflowTemplate = partialTemplate1.PK;

			var template = factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;

			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "TEST 3";
			trigger.TriggerConditions.TriggerEventCode = "Z00";
			var action2 = trigger.ProcessTaskNotifications.AddNew();
			action2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			action2.PQ_P0_WorkflowTemplate = partialTemplate2.PK;

			factory.Save();

			var shipment = factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "ABC";
			factory.Save();

			var workflowProvider = shipment as IWorkflowProvider;
			AssertEquals("Template should create trigger", 1, workflowProvider.WorkflowItems.Triggers.Count);

			workflowProvider.Logs.AddNew(Events.CustomisableEvent00);
			factory.Save();

			var res = MasterFilesTestHelper.RunLogWalker();

			shipment = new BusinessObjectFactory().Load<IForwardingShipment>(shipment.PK);
			workflowProvider = shipment as IWorkflowProvider;
			AssertEquals("Expecting 3 triggers", 3, workflowProvider.WorkflowItems.Triggers.Count);
		}

		public void TestApplyTemplateTrigger_WithoutMatchingWorkflow_ShouldCreateNewWorkflow()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, "DUM");

			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_Name = "Added by Trigger";
			template.P0_ProcessType = "DUM";
			var templateWorkflow = template.ProcessHeaders.AddNew();
			templateWorkflow.FH_CompletionStatement = "Workflow From Template";
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_FH_ProcessHeader = templateWorkflow.PK;
			templateTask.P9_Description = "This is a new task!";

			var jobHeader = helper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var dummy = (DummyWithWorkflow)jobHeader.Parent;
			var dummyWorkflow = jobHeader.ProcessHeaders.AddNew();
			dummyWorkflow.FH_CompletionStatement = "Workflow 1";
			helper.CreateTask(dummyWorkflow, taskType: "ABC", description: "This task was already here.");

			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "trigger 1";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomsEntryStatus.Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			action.PQ_P0_WorkflowTemplate = template.PK;

			dummy.Logs.AddNew(Events.CustomsEntryStatus);

			Factory.Save();

			AssertEquals(1, dummy.WorkflowItems.Tasks.Count);

			new WorkflowApplyTemplateProcessor(action, dummy).Process(new NotificationsForTest());

			var workflows = jobHeader.ProcessHeaders.Cast<ProcessHeader>();
			AssertContainsExactElementsInAnyOrder(new[] { "Job Workflow", "Workflow 1", "Workflow From Template" }, workflows.Select(x => x.FH_CompletionStatement.ToString()));
			AssertContainsExactElementsInAnyOrder(new[] { "This task was already here." }, dummyWorkflow.Tasks.Select(x => x.P9_Description.ToString()));
			var newWorkflow = workflows.Single(x => x.FH_CompletionStatement == "Workflow From Template");
			AssertContainsExactElementsInAnyOrder(new[] { "This is a new task!" }, newWorkflow.Tasks.Cast<ProcessTask>().Select(x => x.P9_Description.ToString()));

			var newTask = newWorkflow.Tasks.Single();
			AssertEquals(template.PK, newTask.SourceTemplatePK);
			AssertEquals(template.P0_Name, newTask.SourceTemplateName);
		}

		public void TestApplyWorkflowTemplateTrigger_AddOnce()
		{
			var factory = new BusinessObjectFactory();

			var template = factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			var baseTrigger = template.WorkflowItems.Triggers.AddNew();
			baseTrigger.P9_Description = "Base Trigger";
			baseTrigger.TriggerConditions.TriggerEventCode = "Z00";

			var partialTemplate1 = factory.NewWithValidTestData<ProcessTaskTemplate>();
			partialTemplate1.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			partialTemplate1.P0_IsPartialTemplate = true;
			var task1 = partialTemplate1.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "TEST 1";

			var baseAction = baseTrigger.ProcessTaskNotifications.AddNew();
			baseAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			baseAction.PQ_P0_WorkflowTemplate = partialTemplate1.PK;

			factory.Save();

			var shipment = factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "ABC";
			factory.Save();

			var workflowProvider = shipment as IWorkflowProvider;
			AssertEquals("Template should create base trigger", 1, workflowProvider.WorkflowItems.Triggers.Count);
			AssertEquals("Precondition: Task count", 0, workflowProvider.WorkflowItems.Tasks.Count);

			workflowProvider.Logs.AddNew(AutoEvents.CustomisableEvent00);
			factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			var newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<IForwardingShipment>(shipment.PK);
			workflowProvider = shipment as IWorkflowProvider;
			AssertEquals("Trigger should have applied partial template to add new task", 1, workflowProvider.WorkflowItems.Tasks.Count);

			workflowProvider.Logs.AddNew(AutoEvents.CustomisableEvent00);
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();

			shipment = new BusinessObjectFactory().Load<IForwardingShipment>(shipment.PK);
			workflowProvider = shipment as IWorkflowProvider;
			AssertEquals("Trigger should not have reapply partial template to add new task again", 1, workflowProvider.WorkflowItems.Tasks.Count);
		}

		public void TestApplyWorkflowTemplateTrigger_AlwaysApply()
		{
			var factory = new BusinessObjectFactory();

			var template = factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			var baseTrigger = template.WorkflowItems.Triggers.AddNew();
			baseTrigger.P9_Description = "Base Trigger";
			baseTrigger.TriggerConditions.TriggerEventCode = "Z00";

			var partialTemplate1 = factory.NewWithValidTestData<ProcessTaskTemplate>();
			partialTemplate1.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			partialTemplate1.P0_IsPartialTemplate = true;
			var task1 = partialTemplate1.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "TEST 1";

			var baseAction = baseTrigger.ProcessTaskNotifications.AddNew();
			baseAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateAlways;
			baseAction.PQ_P0_WorkflowTemplate = partialTemplate1.PK;

			factory.Save();

			var shipment = factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "ABC";
			factory.Save();

			var workflowProvider = shipment as IWorkflowProvider;
			AssertEquals("Template should create base trigger", 1, workflowProvider.WorkflowItems.Triggers.Count);
			AssertEquals("Precondition: Task count", 0, workflowProvider.WorkflowItems.Tasks.Count);

			workflowProvider.Logs.AddNew(AutoEvents.CustomisableEvent00);
			factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			var newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<IForwardingShipment>(shipment.PK);
			workflowProvider = shipment as IWorkflowProvider;
			AssertEquals("Trigger should have applied partial template to add new task", 1, workflowProvider.WorkflowItems.Tasks.Count);

			workflowProvider.Logs.AddNew(AutoEvents.CustomisableEvent00);
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();

			shipment = new BusinessObjectFactory().Load<IForwardingShipment>(shipment.PK);
			workflowProvider = shipment as IWorkflowProvider;
			AssertEquals("Trigger should have applied partial template to add new task again", 2, workflowProvider.WorkflowItems.Tasks.Count);
		}

		public void TestApplyWorkflowTemplateAlwaysShouldNotDuplicatePartialTemplateMilestones()
		{
			var factory = new BusinessObjectFactory();

			var template = factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			var baseTrigger = template.WorkflowItems.Triggers.AddNew();
			baseTrigger.P9_Description = "Base Trigger";
			baseTrigger.TriggerConditions.TriggerEventCode = "Z00";

			var partialTemplate1 = factory.NewWithValidTestData<ProcessTaskTemplate>();
			partialTemplate1.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			partialTemplate1.P0_IsPartialTemplate = true;
			var milestone1 = partialTemplate1.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "TEST 1";
			milestone1.TriggerConditions.TriggerEventCode = "Z00";
			var task1 = partialTemplate1.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "task 1";

			var partialTemplate2 = factory.NewWithValidTestData<ProcessTaskTemplate>();
			partialTemplate2.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			partialTemplate2.P0_IsPartialTemplate = true;
			var milestone2 = partialTemplate2.WorkflowItems.Milestones.AddNew();
			milestone2.P9_Description = "TEST 2";
			milestone2.TriggerConditions.TriggerEventCode = "Z00";
			var task2 = partialTemplate2.WorkflowItems.Tasks.AddNew();
			task2.P9_Description = "task 2";

			//A one time action to add trigger1
			var baseAction = baseTrigger.ProcessTaskNotifications.AddNew();
			baseAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			baseAction.PQ_P0_WorkflowTemplate = partialTemplate1.PK;

			//Always action which will try add trigger2 when the 1st is triggered
			var milestone1Action = milestone1.ProcessTaskNotifications.AddNew();
			milestone1Action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateAlways;
			milestone1Action.PQ_P0_WorkflowTemplate = partialTemplate2.PK;

			//Always action which will try add trigger1 when the 2nd is triggered
			var milestone2Action = milestone2.ProcessTaskNotifications.AddNew();
			milestone2Action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateAlways;
			milestone2Action.PQ_P0_WorkflowTemplate = partialTemplate1.PK;

			factory.Save();

			var shipment = factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "ABC";
			factory.Save();

			var workflowProvider = shipment as IWorkflowProvider;
			AssertEquals("Template should create base trigger", 1, workflowProvider.WorkflowItems.Triggers.Count);
			AssertEquals("Precondition: Template should not have any milestones yet", 0, workflowProvider.WorkflowItems.Milestones.Count);
			AssertEquals("Precondition: Template should not have any tasks yet", 0, workflowProvider.WorkflowItems.Tasks.Count);

			workflowProvider.Logs.AddNew(AutoEvents.CustomisableEvent00);
			factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			var newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<IForwardingShipment>(shipment.PK);
			workflowProvider = shipment as IWorkflowProvider;
			AssertEquals("Trigger should create milestones", 2, workflowProvider.WorkflowItems.Milestones.Count);
			AssertEquals("Base trigger + the 2 milestones should create tasks", 3, workflowProvider.WorkflowItems.Tasks.Count);

			workflowProvider.Logs.AddNew(AutoEvents.CustomisableEvent00);
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();

			shipment = new BusinessObjectFactory().Load<IForwardingShipment>(shipment.PK);
			workflowProvider = shipment as IWorkflowProvider;
			AssertEquals("Milestones should not create other milestones", 2, workflowProvider.WorkflowItems.Milestones.Count);
			AssertEquals("Milestones should only apply once", 3, workflowProvider.WorkflowItems.Tasks.Count);
		}

		public void TestApplyWorkflowTemplateAlwaysShouldNotDuplicatePartialTemplateTriggers()
		{
			var factory = new BusinessObjectFactory();

			var template = factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			template.P0_Name = "BaseTemplate";
			var baseTrigger = template.WorkflowItems.Triggers.AddNew();
			baseTrigger.P9_Description = "Base Trigger";
			baseTrigger.TriggerConditions.TriggerEventCode = "Z00";

			var partialTemplate1 = factory.NewWithValidTestData<ProcessTaskTemplate>();
			partialTemplate1.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			partialTemplate1.P0_IsPartialTemplate = true;
			partialTemplate1.P0_Name = "PartialTemplate1";
			var trigger1 = partialTemplate1.WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "TEST 1";
			trigger1.TriggerConditions.TriggerEventCode = "Z00";
			var task1 = partialTemplate1.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "task 1";

			var partialTemplate2 = factory.NewWithValidTestData<ProcessTaskTemplate>();
			partialTemplate2.P0_Name = "PartialTemplate2";
			partialTemplate2.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			partialTemplate2.P0_IsPartialTemplate = true;
			var trigger2 = partialTemplate2.WorkflowItems.Triggers.AddNew();
			trigger2.P9_Description = "TEST 2";
			trigger2.TriggerConditions.TriggerEventCode = "Z00";
			var task2 = partialTemplate2.WorkflowItems.Tasks.AddNew();
			task2.P9_Description = "task 2";

			//A one time action to add trigger1
			var baseAction = baseTrigger.ProcessTaskNotifications.AddNew();
			baseAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			baseAction.PQ_P0_WorkflowTemplate = partialTemplate1.PK;

			//Always action which will try add trigger2 when the 1st is triggered
			var trigger1Action = trigger1.ProcessTaskNotifications.AddNew();
			trigger1Action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateAlways;
			trigger1Action.PQ_P0_WorkflowTemplate = partialTemplate2.PK;

			var trigger2Action = trigger2.ProcessTaskNotifications.AddNew();
			trigger2Action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			trigger2Action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			trigger2Action.PQ_EmailText = "hello there";
			trigger2Action.PQ_EmailAddr = "obi@wan.com";

			factory.Save();

			AssertEquals("Precondition: Template should have 1 task", 1, partialTemplate1.WorkflowItems.Tasks.Count);
			AssertEquals("Precondition: Template should have 1 task", 1, partialTemplate2.WorkflowItems.Tasks.Count);
			AssertEquals("Precondition: Template should have 1 trigger", 1, partialTemplate1.WorkflowItems.Triggers.Count);
			AssertEquals("Precondition: Template should have 1 trigger", 1, partialTemplate2.WorkflowItems.Triggers.Count);

			var shipment = factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "ABC";
			factory.Save();

			var workflowProvider = shipment as IWorkflowProvider;
			AssertEquals("Template should create base trigger", 1, workflowProvider.WorkflowItems.Triggers.Count);

			workflowProvider.Logs.AddNew(AutoEvents.CustomisableEvent00);
			factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			var newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<IForwardingShipment>(shipment.PK);
			workflowProvider = shipment as IWorkflowProvider;
			AssertEquals("Triggers should apply partial templates to create the 2 extra triggers", 3, workflowProvider.WorkflowItems.Triggers.Count);
			AssertEquals("TMP baseAction should create task1, TMA trigger1Action should create task2", 2, workflowProvider.WorkflowItems.Tasks.Count);

			workflowProvider.Logs.AddNew(AutoEvents.CustomisableEvent00);
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();

			shipment = new BusinessObjectFactory().Load<IForwardingShipment>(shipment.PK);
			workflowProvider = shipment as IWorkflowProvider;
			AssertEquals("Triggers should not reapply partial template triggers even if trigger type is ApplyWorkflowTemplateAlways", 3, workflowProvider.WorkflowItems.Triggers.Count);
			AssertEquals("TMA trigger1Action should create task2 again", 3, workflowProvider.WorkflowItems.Tasks.Count);
		}

		public void TestApplyWorkflowTemplateAlways_IfReapplyingTasksShouldGenerateHigherSequenceNumber()
		{
			var factory = new BusinessObjectFactory();

			var template = factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			var baseTrigger = template.WorkflowItems.Triggers.AddNew();
			baseTrigger.P9_Description = "Base Trigger";
			baseTrigger.TriggerConditions.TriggerEventCode = "Z00";

			var partialTemplate1 = factory.NewWithValidTestData<ProcessTaskTemplate>();
			partialTemplate1.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			partialTemplate1.P0_IsPartialTemplate = true;
			var task1 = partialTemplate1.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "TEST 1";
			task1.P9_Sequence = 5;
			var task2 = partialTemplate1.WorkflowItems.Tasks.AddNew();
			task2.P9_Description = "TEST 2";
			task2.P9_Sequence = 10;

			var baseAction = baseTrigger.ProcessTaskNotifications.AddNew();
			baseAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateAlways;
			baseAction.PQ_P0_WorkflowTemplate = partialTemplate1.PK;

			factory.Save();

			var shipment = factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "ABC";
			var workflowProvider = shipment as IWorkflowProvider;
			var existingTask = workflowProvider.WorkflowItems.Tasks.AddNew();
			existingTask.P9_Description = "Existing";
			existingTask.P9_Sequence = 100;

			factory.Save();

			AssertEquals("Template should create base trigger", 1, workflowProvider.WorkflowItems.Triggers.Count);
			AssertEquals("Precondition: Task count", 1, workflowProvider.WorkflowItems.Tasks.Count);

			workflowProvider.Logs.AddNew(AutoEvents.CustomisableEvent00);
			factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			var newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<IForwardingShipment>(shipment.PK);
			workflowProvider = shipment as IWorkflowProvider;
			AssertEquals("Trigger should have applied partial template to add new tasks", 3, workflowProvider.WorkflowItems.Tasks.Count);
			var processTasks = workflowProvider.WorkflowItems.Tasks.Cast<ProcessTask>().ToArray();
			AssertEquals("Sequence should apply as template dictates", 5, processTasks.FirstOrDefault(x => x.P9_Description == task1.P9_Description).P9_Sequence);
			AssertEquals("Sequence should apply as template dictates", 10, processTasks.FirstOrDefault(x => x.P9_Description == task2.P9_Description).P9_Sequence);

			workflowProvider.Logs.AddNew(AutoEvents.CustomisableEvent00);
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();

			shipment = new BusinessObjectFactory().Load<IForwardingShipment>(shipment.PK);
			workflowProvider = shipment as IWorkflowProvider;
			AssertEquals("Trigger should have applied partial template to add new tasks again", 5, workflowProvider.WorkflowItems.Tasks.Count);
			processTasks = workflowProvider.WorkflowItems.Tasks.Cast<ProcessTask>().ToArray();
			AssertEquals("Sequence should be added on top of highest existing task sequence number", task1.P9_Description, processTasks.FirstOrDefault(x => x.P9_Sequence == 205).P9_Description);
			AssertEquals("Sequence should be added on top of highest existing task sequence number", task2.P9_Description, processTasks.FirstOrDefault(x => x.P9_Sequence == 210).P9_Description);
		}

		public void TestApplyWorkflowTemplateAlways_ShouldNotUseCompletionStatementsForSequenceNumberGeneration()
		{
			var factory = new BusinessObjectFactory();

			var template = factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			var baseTrigger = template.WorkflowItems.Triggers.AddNew();
			baseTrigger.P9_Description = "Base Trigger";
			baseTrigger.TriggerConditions.TriggerEventCode = "Z00";

			var partialTemplate1 = factory.NewWithValidTestData<ProcessTaskTemplate>();
			partialTemplate1.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			partialTemplate1.P0_IsPartialTemplate = true;
			var task1 = partialTemplate1.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "TEST 1";
			task1.P9_Sequence = 5;

			var baseAction = baseTrigger.ProcessTaskNotifications.AddNew();
			baseAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateAlways;
			baseAction.PQ_P0_WorkflowTemplate = partialTemplate1.PK;

			factory.Save();

			var shipment = factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "ABC";
			var workflowProvider = shipment as IWorkflowProvider;
			var jobHeader = ProcessJobHeader.GetForParent(workflowProvider, factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			var existingTask = workflowProvider.WorkflowItems.Tasks.AddNew();
			existingTask.P9_Description = "Existing";
			existingTask.P9_Sequence = 100;
			MasterFilesTestHelper.MakeCompletionStatementTaskType(workflowProvider.WorkflowType, "COM");
			var completionStatement1 = workflow.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();
			completionStatement1.P9_NotesAsString = "Do thing 1";
			completionStatement1.P9_Sequence = 1000;

			factory.Save();

			AssertEquals("Template should create base trigger", 1, workflowProvider.WorkflowItems.Triggers.Count);
			AssertEquals("Precondition: Task count", 2, workflowProvider.WorkflowItems.Tasks.Count);

			workflowProvider.Logs.AddNew(AutoEvents.CustomisableEvent00);
			factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			var newFactory = new BusinessObjectFactory();
			shipment = newFactory.Load<IForwardingShipment>(shipment.PK);
			workflowProvider = shipment as IWorkflowProvider;
			AssertEquals("Trigger should have applied partial template to add new task", 3, workflowProvider.WorkflowItems.Tasks.Count);
			var processTasks = workflowProvider.WorkflowItems.Tasks.Cast<ProcessTask>().ToArray();
			AssertEquals("Sequence should apply as template dictates", 5, processTasks.FirstOrDefault(x => x.P9_Description == task1.P9_Description).P9_Sequence);

			workflowProvider.Logs.AddNew(AutoEvents.CustomisableEvent00);
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();

			shipment = new BusinessObjectFactory().Load<IForwardingShipment>(shipment.PK);
			workflowProvider = shipment as IWorkflowProvider;
			AssertEquals("Trigger should have applied partial template to add new task again", 4, workflowProvider.WorkflowItems.Tasks.Count);
			processTasks = workflowProvider.WorkflowItems.Tasks.Cast<ProcessTask>().ToArray();
			AssertEquals("Sequence should be added on top of highest existing task sequence number excluding completion statements", task1.P9_Description, processTasks.FirstOrDefault(x => x.P9_Sequence == 205).P9_Description);
		}
	}
}
