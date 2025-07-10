using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	public class CurrentTaskFinderTest : TestCaseWithFactory
	{
		public void TestFindCurrentStartableTask()
		{
			var workitem = Factory.NewWithValidTestData<WorkItem>();
			CurrentTaskTestHelper.AssertCurrentStartableTask(workitem, () => new CurrentTaskFinder(workitem).FindCurrentStartableTask());
		}

		public void TestFindCurrentStartableTask_InactiveWorkflowTypeForBufferManagement()
		{
			var workitem = Factory.NewWithValidTestData<WorkItem>();
			CurrentTaskTestHelper.AssertCurrentStartableTask_InactiveWorkflowTypeForBufferManagement(workitem, () => new CurrentTaskFinder(workitem).FindCurrentStartableTask());
		}

		public void TestFindCurrentOrNextStartableTask()
		{
			var workitem = Factory.NewWithValidTestData<WorkItem>();
			CurrentTaskTestHelper.AssertCurrentOrNextStartableTask(workitem, () => new CurrentTaskFinder(workitem).FindCurrentOrNextStartableTask());
		}

		public void TestFindCurrentOrNextStartableTask_InactiveWorkflowTypeForBufferManagement()
		{
			var workitem = Factory.NewWithValidTestData<WorkItem>();
			CurrentTaskTestHelper.AssertCurrentOrNextStartableTask_InactiveWorkflowTypeForBufferManagement(workitem, () => new CurrentTaskFinder(workitem).FindCurrentOrNextStartableTask());
		}

		[TestDate(2022, 2, 18)]
		public void TestFindCurrentStartableTask_DbHits()
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, workItem.WorkflowType);
			var jobHeader = helper.GetJobHeaderForParent(workItem, Factory, addDefaultProcessHeaderIfNone: false);

			var maxWorkflowCount = 20;
			var maxTaskCount = 10;

			for (int i = 0; i < maxWorkflowCount; i++)
			{
				var workflow = helper.CreateWorkflow(jobHeader, $"Workflow {i}");
				for (int j = 0; j < maxTaskCount; j++)
				{
					var task = workItem.WorkflowItems.Tasks.AddNew();
					task.P9_Sequence = i * 100 + j;
					task.P9_Description = $"Task {task.P9_Sequence}";
					task.P9_FH_ProcessHeader = workflow.PK;

					if (i == maxWorkflowCount - 1 && j == maxTaskCount - 1)
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
						task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
					}
					else
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					}
				}
			}

			Factory.Save();

			var loadedWorkItem = new BusinessObjectFactory().Load<WorkItem>(workItem.PK);
			var currentTaks = loadedWorkItem.CurrentOrNextTask;

			AssertEquals("The current task is the task with highest sequence", (maxWorkflowCount - 1) * 100 + maxTaskCount - 1, currentTaks.P9_Sequence);

			var expectedHitCounts = new Dictionary<string, int>
			{
				{ BMSystemSchema.Constants.TableName, 1 },
				{ BMSystemWorkflowDeterminerSchema.Constants.TableName, 1 },
				{ ProcessHeaderSchema.Constants.TableName, 1 },
				{ ProcessTaskIterationLinkSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 2 },
				{ WorkItemSchema.Constants.TableName, 1 }
			};
			AssertDbHits(expectedHitCounts, loadedWorkItem.Factory);
		}
	}

	public static class CurrentTaskTestHelper
	{
		public static void AssertCurrentStartableTask(IWorkflowProvider provider, Func<ProcessTask> getCurrentStartableTask, bool createBMSystem = true)
		{
			var factory = ((BusinessObject)provider).Factory;

			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			if (createBMSystem)
			{
				helper.CreateSystem(factory, provider.WorkflowType);
			}

			AssertSingleWorkflowWithSingleStartableTask(provider, getCurrentStartableTask, helper, factory);
			AssertSingleWorkflowWithMultipleStartableTasks(provider, getCurrentStartableTask, helper, factory);
			AssertMultipleWorkflowsWithSingleStartableTask(provider, getCurrentStartableTask, helper, factory);
			AssertMultipleWorkflowsWithMultipleStartableTasks(provider, getCurrentStartableTask, helper, factory);
			AssertMultipleWorkflowsWithPrerequisite(provider, getCurrentStartableTask, helper, factory);
			AssertMultipleWorkflowsWithBMSystemDisabled(provider, getCurrentStartableTask, helper, factory);
		}

		static void AssertSingleWorkflowWithSingleStartableTask(IWorkflowProvider provider, Func<ProcessTask> getCurrentStartableTask, IBMTestHelper helper, BusinessObjectFactory factory)
		{
			provider.WorkflowItems.RemoveAndDeleteAll();
			var jobHeader = helper.GetJobHeaderForParent(provider, factory, addDefaultProcessHeaderIfNone: false);
			jobHeader.ProcessHeaders.DeleteAll();

			//WorkflowA 1 TaskA.1 50
			//WorkflowA 2 TaskA.2 60
			//WorkflowA 3 TaskA.3 70

			var workflow1 = helper.CreateWorkflow(jobHeader, "Workflow A");

			var task1 = provider.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow1.PK;
			task1.P9_Description = "Task A.1";
			task1.P9_Sequence = 1;
			task1.P9_TaskID = "50";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var task2 = provider.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = workflow1.PK;
			task2.P9_Description = "Task A.2";
			task2.P9_Sequence = 2;
			task2.P9_TaskID = "60";
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var task3 = provider.WorkflowItems.AddNew();
			task3.P9_FH_ProcessHeader = workflow1.PK;
			task3.P9_Description = "Task A.3";
			task3.P9_Sequence = 3;
			task3.P9_TaskID = "70";
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var currentStartableTask = getCurrentStartableTask();
			Assertion.AssertEquals("Single workflow with single startable task", "Task A.1", currentStartableTask.P9_Description);
		}

		static void AssertSingleWorkflowWithMultipleStartableTasks(IWorkflowProvider provider, Func<ProcessTask> getCurrentStartableTask, IBMTestHelper helper, BusinessObjectFactory factory)
		{
			provider.WorkflowItems.RemoveAndDeleteAll();
			var jobHeader = helper.GetJobHeaderForParent(provider, factory, addDefaultProcessHeaderIfNone: false);
			jobHeader.ProcessHeaders.DeleteAll();

			//WorkflowA 1 TaskA.1 50
			//WorkflowA 1 TaskA.2 60
			//WorkflowA 3 TaskA.3 70

			var workflow1 = helper.CreateWorkflow(jobHeader, "Workflow A");

			CreateTask(provider, workflow1, "Task A.1", 1, "50");
			CreateTask(provider, workflow1, "Task A.2", 2, "60");
			CreateTask(provider, workflow1, "Task A.3", 3, "70");

			var currentStartableTask = getCurrentStartableTask();
			Assertion.AssertEquals("Single workflow with multiple startable tasks", "Task A.1", currentStartableTask.P9_Description);
		}

		static void AssertMultipleWorkflowsWithSingleStartableTask(IWorkflowProvider provider, Func<ProcessTask> getCurrentStartableTask, IBMTestHelper helper, BusinessObjectFactory factory)
		{
			provider.WorkflowItems.RemoveAndDeleteAll();
			var jobHeader = helper.GetJobHeaderForParent(provider, factory, addDefaultProcessHeaderIfNone: false);
			jobHeader.ProcessHeaders.DeleteAll();

			//WorkflowA 11 TaskA.1 050
			//WorkflowA 12 TaskA.2 060
			//WorkflowA 13 TaskA.3 070
			//WorkflowB 1 TaskB.1 080
			//WorkflowB 2 TaskB.2 090
			//WorkflowB 3 TaskB.3 100

			var workflow1 = helper.CreateWorkflow(jobHeader, "Workflow A");
			var workflow2 = helper.CreateWorkflow(jobHeader, "Workflow B");
			helper.CreateLink(workflow1, workflow2);

			CreateTask(provider, workflow1, "Task A.1", 11, "050");
			CreateTask(provider, workflow1, "Task A.2", 12, "060");
			CreateTask(provider, workflow1, "Task A.3", 13, "070");

			CreateTask(provider, workflow2, "Task B.1", 1, "080");
			CreateTask(provider, workflow2, "Task B.2", 2, "090");
			CreateTask(provider, workflow2, "Task B.3", 3, "100");

			var currentStartableTask = getCurrentStartableTask();
			Assertion.AssertEquals("Multiple workflows with single startable task", "Task A.1", currentStartableTask.P9_Description);
		}

		static void AssertMultipleWorkflowsWithMultipleStartableTasks(IWorkflowProvider provider, Func<ProcessTask> getCurrentStartableTask, IBMTestHelper helper, BusinessObjectFactory factory)
		{
			provider.WorkflowItems.RemoveAndDeleteAll();
			var jobHeader = helper.GetJobHeaderForParent(provider, factory, addDefaultProcessHeaderIfNone: false);
			jobHeader.ProcessHeaders.DeleteAll();

			//WorkflowA 1 TaskA.1 050
			//WorkflowA 2 TaskA.2 060
			//WorkflowA 3 TaskA.3 070
			//WorkflowB 1 TaskB.1 080
			//WorkflowB 2 TaskB.2 090
			//WorkflowB 3 TaskB.3 100
			//WorkflowC 1 TaskC.1 110
			//WorkflowC 2 TaskC.2 120
			//WorkflowC 3 TaskC.3 130

			var workflow1 = helper.CreateWorkflow(jobHeader, "Workflow A");
			var workflow2 = helper.CreateWorkflow(jobHeader, "Workflow B");
			var workflow3 = helper.CreateWorkflow(jobHeader, "Workflow C");
			helper.CreateLink(workflow1, workflow2);

			var taskA1 = CreateTask(provider, workflow1, "Task A.1", 1, "050");
			var taskA2 = CreateTask(provider, workflow1, "Task A.2", 2, "060");
			var taskA3 = CreateTask(provider, workflow1, "Task A.3", 3, "070");

			CreateTask(provider, workflow2, "Task B.1", 1, "080");
			CreateTask(provider, workflow2, "Task B.2", 2, "090");
			CreateTask(provider, workflow2, "Task B.3", 3, "100");

			var taskC1 = CreateTask(provider, workflow3, "Task C.1", 1, "110");
			var taskC2 = CreateTask(provider, workflow3, "Task C.2", 2, "120");
			var taskC3 = CreateTask(provider, workflow3, "Task C.3", 3, "130");

			var currentStartableTask = getCurrentStartableTask();
			Assertion.AssertEquals("Multiple workflows with multiple startable tasks", "Task A.1", currentStartableTask.P9_Description);

			taskA1.P9_Sequence = 4;
			taskA2.P9_Sequence = 2;
			taskA3.P9_Sequence = 3;
			taskC1.P9_Sequence = 2;
			currentStartableTask = getCurrentStartableTask();
			Assertion.AssertEquals("Multiple workflows with multiple startable tasks after seq update", "Task A.2", currentStartableTask.P9_Description);

			CreateTask(provider, workflow1, "Task A.4", 2, "140");
			currentStartableTask = getCurrentStartableTask();
			Assertion.AssertEquals("Multiple workflows with multiple startable tasks after new task added", "Task A.2", currentStartableTask.P9_Description);

			taskC1.P9_Sequence = 1000;
			taskC2.P9_Sequence = 1002;
			taskC3.P9_Sequence = 1003;
			currentStartableTask = getCurrentStartableTask();
			Assertion.AssertEquals("Multiple workflows with multiple startable tasks after seq update", "Task A.2", currentStartableTask.P9_Description);

			taskA2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			taskC1.P9_Sequence = 1;
			taskC2.P9_Sequence = 2;
			taskC3.P9_Sequence = 3;
			currentStartableTask = getCurrentStartableTask();
			Assertion.AssertEquals("Multiple workflows with multiple startable tasks after closing current task", "Task C.1", currentStartableTask.P9_Description);
		}

		static void AssertMultipleWorkflowsWithPrerequisite(IWorkflowProvider provider, Func<ProcessTask> getCurrentStartableTask, IBMTestHelper helper, BusinessObjectFactory factory)
		{
			provider.WorkflowItems.RemoveAndDeleteAll();
			var jobHeader = helper.GetJobHeaderForParent(provider, factory, addDefaultProcessHeaderIfNone: false);
			jobHeader.ProcessHeaders.DeleteAll();

			var workItem = factory.NewWithValidTestData<WorkItem>();
			var prereqJobHeader = helper.GetJobHeaderForParent(workItem, factory, addDefaultProcessHeaderIfNone: false);
			var prereqWorkflow = helper.CreateWorkflow(prereqJobHeader, "Workflow Prereq");
			CreateTask(workItem, prereqWorkflow, "Task 0", 1, "040");

			//Prereq Job
			//Workflow1 1 Task0 040

			//Job
			//WorkflowA 4 TaskA.1 050
			//WorkflowB 1 TaskB.1 080
			//WorkflowB 2 TaskB.2 090
			//WorkflowB 3 TaskB.3 100

			var workflow1 = helper.CreateWorkflow(jobHeader, "Workflow A");
			var workflow2 = helper.CreateWorkflow(jobHeader, "Workflow B");
			helper.CreateLink(prereqWorkflow, workflow1);
			helper.CreateLink(workflow1, workflow2);

			CreateTask(provider, workflow1, "Task A.1", 4, "050");

			CreateTask(provider, workflow2, "Task B.1", 1, "080");
			CreateTask(provider, workflow2, "Task B.2", 2, "090");
			CreateTask(provider, workflow2, "Task B.3", 3, "100");

			var currentStartableTask = getCurrentStartableTask();
			Assertion.AssertEquals("Multiple workflows with pre-requisite workflow", "Task A.1", currentStartableTask.P9_Description);
		}

		static void AssertMultipleWorkflowsWithBMSystemDisabled(IWorkflowProvider provider, Func<ProcessTask> getCurrentStartableTask, IBMTestHelper helper, BusinessObjectFactory factory)
		{
			provider.WorkflowItems.RemoveAndDeleteAll();
			var jobHeader = helper.GetJobHeaderForParent(provider, factory, addDefaultProcessHeaderIfNone: false);
			jobHeader.ProcessHeaders.DeleteAll();

			//WorkflowA 1 TaskA.1 050
			//WorkflowA 2 TaskA.2 060
			//WorkflowB 1 TaskB.1 010
			//WorkflowB 2 TaskB.2 020

			var workflow1 = helper.CreateWorkflow(jobHeader, "Workflow A");
			var workflow2 = helper.CreateWorkflow(jobHeader, "Workflow B");
			helper.CreateLink(workflow1, workflow2);

			CreateTask(provider, workflow1, "Task A.1", 1, "050");
			CreateTask(provider, workflow1, "Task A.2", 2, "060");

			CreateTask(provider, workflow2, "Task B.1", 1, "010");
			CreateTask(provider, workflow2, "Task B.3", 2, "020");

			helper.DisableBMSInRegistry();
			var currentStartableTask = getCurrentStartableTask();
			Assertion.AssertEquals("Multiple workflows with BM system disabled", "Task B.1", currentStartableTask.P9_Description);
		}

		public static void AssertCurrentStartableTask_InactiveWorkflowTypeForBufferManagement(IWorkflowProvider provider, Func<ProcessTask> getCurrentStartableTask)
		{
			var factory = ((BusinessObject)provider).Factory;

			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystemAndRelatedWorkflowType(factory, provider.WorkflowType, isActive: false);

			var jobHeader = helper.GetJobHeaderForParent(provider, factory, addDefaultProcessHeaderIfNone: false);
			jobHeader.ProcessHeaders.DeleteAll();

			//WorkflowA 1 TaskA.1 050
			//WorkflowA 2 TaskA.2 060
			//WorkflowB 1 TaskB.1 010
			//WorkflowB 2 TaskB.2 020

			var workflow1 = helper.CreateWorkflow(jobHeader, "Workflow A");
			var workflow2 = helper.CreateWorkflow(jobHeader, "Workflow B");
			helper.CreateLink(workflow1, workflow2);

			CreateTask(provider, workflow1, "Task A.1", 1, "050");
			CreateTask(provider, workflow1, "Task A.2", 2, "060");

			CreateTask(provider, workflow2, "Task B.1", 1, "010");
			CreateTask(provider, workflow2, "Task B.3", 2, "020");

			var currentStartableTask = getCurrentStartableTask();
			Assertion.AssertEquals("Multiple workflows with BM system enabled but inactive workflow type", "Task B.1", currentStartableTask.P9_Description);
		}

		public static void AssertCurrentOrNextStartableTask(IWorkflowProvider provider, Func<ProcessTask> getCurrentOrNextStartableTask, bool createBMSystem = true)
		{
			var factory = ((BusinessObject)provider).Factory;

			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			if (createBMSystem)
			{
				helper.CreateSystem(factory, provider.WorkflowType);
			}

			var jobHeader = helper.GetJobHeaderForParent(provider, factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = helper.CreateWorkflow(jobHeader, "Workflow 1");
			var workflow2 = helper.CreateWorkflow(jobHeader, "Workflow 2");
			helper.CreateLink(workflow1, workflow2);

			//workflow2 10 task2
			//workflow1 20 task1
			//workflow2 30 task4
			//workflow1 40 task3

			var task1 = CreateTask(provider, workflow1, "Task 1", 20, "10");
			var task2 = CreateTask(provider, workflow2, "Task 2", 10, "20");
			var task3 = CreateTask(provider, workflow1, "Task 3", 40, "30", ProcessTaskStatusCodeList.Codes.Open);
			var task4 = CreateTask(provider, workflow2, "Task 4", 30, "40", ProcessTaskStatusCodeList.Codes.Open);

			factory.Save();

			//Scenario 1
			var currentOrNextStartableTask = getCurrentOrNextStartableTask();
			Assertion.AssertEquals("Scenario 1: task1 has no open prereq, task2 has open prereq - task1 is current startable task", "Task 1", currentOrNextStartableTask.P9_Description);

			//Scenario 2
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			factory.Save();
			currentOrNextStartableTask = getCurrentOrNextStartableTask();
			Assertion.AssertEquals("Scenario 2: task3 has no open prereq, task2 has open prereq - task3 is next startable task", "Task 3", currentOrNextStartableTask.P9_Description);

			//Scenario 3
			var anotherJob = factory.New<WorkItem>();
			var jobHeader2 = helper.GetJobHeaderForParent(anotherJob, factory, addDefaultProcessHeaderIfNone: false);

			var workflow5 = helper.CreateWorkflow(jobHeader2, "Workflow 5");
			CreateTask(anotherJob, workflow5, "Task 5", 10, "50");

			helper.CreateLink(workflow5, workflow1);
			factory.Save();

			currentOrNextStartableTask = getCurrentOrNextStartableTask();
			Assertion.AssertEquals("Scenario 3: task3 only has open prereq from another job - task3 is next open task", "Task 3", currentOrNextStartableTask.P9_Description);

			//Scenario 4
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			factory.Save();

			currentOrNextStartableTask = getCurrentOrNextStartableTask();
			Assertion.AssertEquals("Scenario 4: workflow1 is closed - task2 is next task", "Task 2", currentOrNextStartableTask.P9_Description);

			//Scenario 5
			helper.DisableBMSInRegistry();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			factory.Save();

			currentOrNextStartableTask = getCurrentOrNextStartableTask();
			Assertion.AssertEquals("Scenario 5: BMS is disabled - task2 is current startable task", "Task 2", currentOrNextStartableTask.P9_Description);

			//Scenario 6
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			factory.Save();
			currentOrNextStartableTask = getCurrentOrNextStartableTask();
			Assertion.AssertEquals("Scenario 6: BMS is disabled - task1 is current startable task", "Task 1", currentOrNextStartableTask.P9_Description);

			//Scenario 7
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			factory.Save();
			currentOrNextStartableTask = getCurrentOrNextStartableTask();
			Assertion.AssertEquals("Scenario 7: BMS is disabled - task4 is next startable task", "Task 4", currentOrNextStartableTask.P9_Description);

			//Scenario 8
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			factory.Save();
			currentOrNextStartableTask = getCurrentOrNextStartableTask();
			Assertion.AssertEquals("Scenario 8: BMS is disabled - task3 is next startable task", "Task 3", currentOrNextStartableTask.P9_Description);
		}

		public static void AssertCurrentOrNextStartableTask_InactiveWorkflowTypeForBufferManagement(IWorkflowProvider provider, Func<ProcessTask> getCurrentOrNextStartableTask, bool createBMSystem = true)
		{
			var factory = ((BusinessObject)provider).Factory;

			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystemAndRelatedWorkflowType(factory, provider.WorkflowType, isActive: false);

			var jobHeader = helper.GetJobHeaderForParent(provider, factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = helper.CreateWorkflow(jobHeader, "Workflow 1");
			var workflow2 = helper.CreateWorkflow(jobHeader, "Workflow 2");
			helper.CreateLink(workflow1, workflow2);

			//workflow2 10 task2 10
			//workflow1 10 task1 11

			CreateTask(provider, workflow1, "Task 1", 10, "11", ProcessTaskStatusCodeList.Codes.Open);
			CreateTask(provider, workflow2, "Task 2", 10, "10", ProcessTaskStatusCodeList.Codes.Open);

			factory.Save();

			var currentOrNextStartableTask = getCurrentOrNextStartableTask();
			Assertion.AssertEquals("Multiple workflows with BM system enabled but inactive workflow type", "Task 2", currentOrNextStartableTask.P9_Description);
		}

		static ProcessTask CreateTask(IWorkflowProvider provider, IProcessHeader processHeader, string description, int sequence, string taskID, string status = null)
		{
			var task = provider.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = processHeader.PK;
			task.P9_Description = description;
			task.P9_Sequence = sequence;
			task.P9_TaskID = taskID;
			task.P9_Status = status ?? ProcessTaskStatusCodeList.Codes.Assigned;
			if (task.P9_Status == ProcessTaskStatusCodeList.Codes.Assigned)
			{
				task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			}

			return task;
		}
	}
}
