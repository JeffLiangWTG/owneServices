using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TaskIterationPropertyTest : TestCaseWithFactory
	{
		#region Getter

		public void TestIteration_WhenTaskHasPivotRow()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var task1 = BMSTestHelper.CreateTask(workflow1);
			var task2 = BMSTestHelper.CreateTask(workflow1, taskType: "CB");

			var iterationWorkflow = BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration");

			foreach (var task in iterationWorkflow.Tasks.Cast<ProcessTask>())
			{
				AssertEquals("1", task.Iteration);
			}
		}

		public void TestIteration_WhenTaskHasNoPivotRow()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var task = (ProcessTask)BMSTestHelper.CreateTask(workflow);

			AssertEquals(ZString.Empty, task.Iteration);
		}

		public void TestIteration_MultipleIterationsInWorkflow()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			AssertEquals(string.Empty, task1.Iteration);
			AssertEquals(string.Empty, task2.Iteration);

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			var iterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 4);

			AssertEquals(string.Empty, task1.Iteration);
			AssertEquals(string.Empty, task2.Iteration);
			AssertEquals("1", iterationTask1.Iteration);
			AssertEquals("1", containmentBarrierIterationTask1.Iteration);

			BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "QI", shouldCreateWorkflowForIteration: false);
			var iterationTask2 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 5);
			var containmentBarrierIterationTask2 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 6);

			AssertEquals(string.Empty, task1.Iteration);
			AssertEquals(string.Empty, task2.Iteration);
			AssertEquals("1", iterationTask1.Iteration);
			AssertEquals("1", containmentBarrierIterationTask1.Iteration);
			AssertEquals("2", iterationTask2.Iteration);
			AssertEquals("2", containmentBarrierIterationTask2.Iteration);
		}

		public void TestIteration_NotThrowException()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			AssertEquals(string.Empty, task1.Iteration);
			AssertEquals(string.Empty, task2.Iteration);

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);

			var iterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 4);

			AssertEquals(string.Empty, task1.Iteration);
			AssertEquals(string.Empty, task2.Iteration);
			AssertEquals("1", iterationTask1.Iteration);
			AssertEquals("1", containmentBarrierIterationTask1.Iteration);

			var somePivot = Factory.New<IProcessTaskIterationLinkPivot>();
			somePivot.P9P_P9_Task = iterationTask1.PK;
			somePivot.P9P_ParentId = iterationTask1.P9_ParentID;
			somePivot.P9P_ParentTableCode = iterationTask1.P9_ParentTableCode;

			AssertEquals("1", iterationTask1.Iteration);
		}

		#endregion

		#region Setter

		public void TestSetIteration_FromBlankToExistingIteration_ShouldCreatePivotRow_FirstIteration()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			var iterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 4);

			BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "QI", shouldCreateWorkflowForIteration: false);

			var task = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			AssertEquals(ZString.Empty, task.Iteration);
			AssertNull(GetPivotForTask(task));

			task.Iteration = "1";
			var pivot = GetPivotForTask(task);
			AssertNotNull("A pivot should be created when setting the Iteration field to a valid value. SAD!", pivot);

			var iteration = GetIterationCreatedFromContainmentBarrierTask(task2);
			AssertEquals("The associated pivot should point to the correct iteration. SAD!", iteration.PK, pivot.P9P_P9I_Iteration);
			AssertEquals("The associated pivot should point to the correct iteration. SAD!", (ZByte)1, iteration.P9I_Sequence);
		}

		public void TestSetIteration_FromBlankToExistingIteration_ShouldCreatePivotRow_SecondIteration()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			var iterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 4);

			BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "QI", shouldCreateWorkflowForIteration: false);

			var task = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			AssertEquals(ZString.Empty, task.Iteration);
			AssertNull(GetPivotForTask(task));

			task.Iteration = "2";
			var pivot = GetPivotForTask(task);
			AssertNotNull("A pivot should be created when setting the Iteration field to a valid value. SAD!", pivot);

			var iteration = GetIterationCreatedFromContainmentBarrierTask(containmentBarrierIterationTask1);
			AssertEquals("The associated pivot should point to the correct iteration. SAD!", iteration.PK, pivot.P9P_P9I_Iteration);
			AssertEquals("The associated pivot should point to the correct iteration. SAD!", (ZByte)2, iteration.P9I_Sequence);
		}

		public void TestSetIteration_FromBlankToInvalidValue_ShouldNotCreatePivot_ButGetterShouldReturnInvalidValue()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);

			var task = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			AssertEquals(ZString.Empty, task.Iteration);
			AssertNull(GetPivotForTask(task));

			task.Iteration = "2";
			AssertNull("No pivot should be created since the value entered doesn't represent any iteration for this workflow. SAD!", GetPivotForTask(task));
			AssertEquals("The value entered should still be shown on the task, even though it's invalid (users don't like it when you delete the text they just entered). Don't worry, there will be a validation error.", "2", task.Iteration);
		}

		public void TestSetIteration_FromOneIterationToAnother_ShouldCreateNewPivotAndDeleteTheOldOne()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			var iterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 4);

			BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "QI", shouldCreateWorkflowForIteration: false);

			var task = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			AssertEquals(ZString.Empty, task.Iteration);
			AssertNull(GetPivotForTask(task));

			task.Iteration = "1";
			var pivot1 = GetPivotForTask(task);
			AssertNotNull("A pivot should be created when setting the Iteration field to a valid value. SAD!", pivot1);

			var iteration1 = GetIterationCreatedFromContainmentBarrierTask(task2);
			AssertEquals("The associated pivot should point to the correct iteration. SAD!", iteration1.PK, pivot1.P9P_P9I_Iteration);
			AssertEquals("The associated pivot should point to the correct iteration. SAD!", (ZByte)1, iteration1.P9I_Sequence);

			task.Iteration = "2";
			var pivot2 = GetPivotForTask(task);
			AssertNotNull("A new pivot should be created when setting the Iteration field to a valid value. SAD!", pivot2);

			var iteration2 = GetIterationCreatedFromContainmentBarrierTask(containmentBarrierIterationTask1);
			AssertEquals("The new pivot should point to the correct iteration. SAD!", iteration2.PK, pivot2.P9P_P9I_Iteration);
			AssertEquals("The new pivot should point to the correct iteration. SAD!", (ZByte)2, iteration2.P9I_Sequence);

			AssertEquals("The original pivot should be deleted because the selected iteration has changed. SAD!", true, ((BusinessObject)pivot1).IsDeleted);
		}

		public void TestSetIteration_FromExistingIterationToBlank_ShouldDeletePivot_AndGetterShouldHaveBlankValue()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);

			var task = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			AssertEquals(ZString.Empty, task.Iteration);
			AssertNull(GetPivotForTask(task));

			task.Iteration = "1";
			var pivot = GetPivotForTask(task);
			AssertNotNull("A pivot should be created when setting the Iteration field to a valid value. SAD!", pivot);

			var iteration = GetIterationCreatedFromContainmentBarrierTask(task2);
			AssertEquals("The associated pivot should point to the correct iteration. SAD!", iteration.PK, pivot.P9P_P9I_Iteration);
			AssertEquals("The associated pivot should point to the correct iteration. SAD!", (ZByte)1, iteration.P9I_Sequence);

			task.Iteration = ZString.Empty;

			AssertEquals("The original pivot should be deleted because the selected iteration has changed. SAD!", true, ((BusinessObject)pivot).IsDeleted);
			AssertNull("No new pivot should have been created. SAD!", GetPivotForTask(task));
			AssertEquals(ZString.Empty, task.Iteration);
		}

		public void TestSetIteration_FromExistingIterationToInvalidValue_ShouldDeletePivot_ButGetterShouldReturnInvalidValue()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);

			var task = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			AssertEquals(ZString.Empty, task.Iteration);
			AssertNull(GetPivotForTask(task));

			task.Iteration = "1";
			var pivot = GetPivotForTask(task);
			AssertNotNull("A pivot should be created when setting the Iteration field to a valid value. SAD!", pivot);

			var iteration = GetIterationCreatedFromContainmentBarrierTask(task2);
			AssertEquals("The associated pivot should point to the correct iteration. SAD!", iteration.PK, pivot.P9P_P9I_Iteration);
			AssertEquals("The associated pivot should point to the correct iteration. SAD!", (ZByte)1, iteration.P9I_Sequence);

			task.Iteration = "2";

			AssertEquals("The original pivot should be deleted because the selected iteration has changed. SAD!", true, ((BusinessObject)pivot).IsDeleted);
			AssertNull("No new pivot should have been created. SAD!", GetPivotForTask(task));
			AssertEquals("The value entered should still be shown on the task, even though it's invalid (users don't like it when you delete the text they just entered). Don't worry, there will be a validation error.", "2", task.Iteration);
		}

		public void TestSetIteration_MultipleWorkflowsWithOverlappingIterationSequenceNumbers()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var task1 = BMSTestHelper.CreateTask(workflow1);
			var task2 = BMSTestHelper.CreateTask(workflow1, taskType: "CB");

			var iterationWorkflow1 = BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration");
			var iterationTask1 = (ProcessTask)iterationWorkflow1.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)iterationWorkflow1.Tasks.Single(x => x.P9_Sequence == 4);

			var iterationWorkflow2 = BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "Quality Iteration");
			var iterationTask2 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 5);

			var iteration1 = GetIterationCreatedFromContainmentBarrierTask(task2);
			var iteration2 = GetIterationCreatedFromContainmentBarrierTask(containmentBarrierIterationTask1);
			AssertEquals(iteration1.P9I_Sequence, iteration2.P9I_Sequence);

			iterationTask1.Iteration = ZString.Empty; // Needed in order to avoid the Iteration being automatically set on the tasks we're about to add, once that feature is added.
			iterationTask2.Iteration = ZString.Empty; // Needed in order to avoid the Iteration being automatically set on the tasks we're about to add, once that feature is added.

			var newTaskInIteration1 = (ProcessTask)BMSTestHelper.CreateTask(iterationWorkflow1);
			AssertEquals(ZString.Empty, newTaskInIteration1.Iteration);

			newTaskInIteration1.Iteration = "1";
			var pivot1 = GetPivotForTask(newTaskInIteration1);
			AssertEquals("The pivot should point to the iteration associated with the task's workflow, which could be confusing since the iteration for the other workflow also has the specified sequence number.",
				newTaskInIteration1.P9_FH_ProcessHeader, pivot1.Iteration.P9I_FH_IterationWorkflow);

			var newTaskInIteration2 = (ProcessTask)BMSTestHelper.CreateTask(iterationWorkflow2);
			newTaskInIteration2.Iteration = ZString.Empty;
			AssertNull(GetPivotForTask(newTaskInIteration2));

			newTaskInIteration2.Iteration = "1";
			var pivot2 = GetPivotForTask(newTaskInIteration2);
			AssertEquals("The pivot should point to the iteration associated with the task's workflow, which could be confusing since the iteration for the other workflow also has the specified sequence number.",
				newTaskInIteration2.P9_FH_ProcessHeader, pivot2.Iteration.P9I_FH_IterationWorkflow);
		}

		public void TestIterationMaxLength()
		{
			var task = Factory.New<ProcessTask>();
			AssertEquals("MaxLength for Iteration should be 3, since the backing value is a ZByte which can't exceed 255. SAD!", 3, task.IterationInfo.MaxLength);
			AssertEquals("Just checking my logic here.", 3, byte.MaxValue.ToString(CultureInfo.InvariantCulture).Length);
		}

		public void TestSetIteration_WhenTaskHasNoWorkflow_ShouldDoNothing()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);

			var task = ((IWorkflowProvider)jobHeader.Parent).WorkflowItems.Tasks.AddNew();
			task.Iteration = "1";
			AssertEquals("1", task.Iteration);
			AssertNull("We can't assign an iteration to a task without a workflow. Don't worry, there will be a validation error, tested elsewhere. SAD!", GetPivotForTask(task));
		}

		[TestDate(2021, 1, 21)]
		public void TestSetIteration_WhenMultipleIterationsWithTheSameSequenceExist_ShouldSelectMostRecentIterationByCreateTime()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			var iterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 4);
			Factory.Save(); // SystemCreateTime is set on factory save.

			TestDateAttribute.AddMinutes(10);
			BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "QI", shouldCreateWorkflowForIteration: false);
			var iterationTask2 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 5);
			var containmentBarrierIterationTask2 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 6);
			Factory.Save(); // SystemCreateTime is set on factory save.

			TestDateAttribute.AddMinutes(-5);
			BMSTestHelper.CreateQualityIteration(iterationTask2, containmentBarrierIterationTask2, "QI", shouldCreateWorkflowForIteration: false);
			Factory.Save(); // SystemCreateTime is set on factory save.

			TestDateAttribute.AddMinutes(15);
			var iterations = Factory.Load<IProcessTaskIterationLink>(new ZQuery());

			AssertEquals(3, iterations.Length);
			iterations[0].P9I_Sequence = 1;
			iterations[1].P9I_Sequence = 1;
			iterations[2].P9I_Sequence = 1;

			var task = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			AssertEquals(ZString.Empty, task.Iteration);
			AssertNull(GetPivotForTask(task));

			task.Iteration = "1";
			var pivot = GetPivotForTask(task);
			AssertNotNull(pivot);

			AssertEquals("The iteration that was created latest by SystemCreateTime should be the one selected when there's more than one of the same sequence number.", containmentBarrierIterationTask1.PK, pivot.Iteration.ContainmentBarrierTask.PK);
			// Known limitation: if multiple iterations are created in the same minute, the behaviour will be determined by database row order among those (because the field is smalldatetime). This whole thing is an unlikely edge case anyway so we don't really care.
		}

		public void TestSetIteration_WhenOtherTypesOfIterationLinksWithSameSequenceExist_ShouldSelectCorrectIterationLinkForPivot()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow, description: "Task 1");
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, description: "Task 2");
			var task3 = (ProcessTask)BMSTestHelper.CreateTask(workflow, description: "Task 3");
			var task4 = (ProcessTask)BMSTestHelper.CreateTask(workflow, description: "Task 4");
			var task5 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var cbTask = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			var createTime = ZDateTime.UtcNow;

			IProcessTaskIterationLink CreateIterationLink(ProcessTask iterationTask, string linkType, string outcome, byte sequence)
			{
				var iterationLink = (IProcessTaskIterationLink)cbTask.IterationLinks.AddNew();
				iterationLink.P9I_FH_IterationWorkflow = workflow.PK;
				iterationLink.P9I_P9_IterationTask = iterationTask.PK;
				iterationLink.P9I_LinkType = linkType;
				iterationLink.P9I_Outcome = outcome;
				iterationLink.P9I_Sequence = sequence;
				iterationLink.P9I_SystemCreateTimeUtc = createTime;

				createTime = createTime.AddMinutes(1);

				return iterationLink;
			}

			var link1 = CreateIterationLink(task1, IterationLinkTypeList.Codes.PassedContainmentBarrier, IterationLinkOutcomeList.Codes.Passed, 1);
			var link2 = CreateIterationLink(task2, IterationLinkTypeList.Codes.QualityIterationTask, IterationLinkOutcomeList.Codes.IterationRequired, 1);
			var link3 = CreateIterationLink(task3, IterationLinkTypeList.Codes.QualityIterationTask, IterationLinkOutcomeList.Codes.Deferred, 1);
			var link4 = CreateIterationLink(task4, IterationLinkTypeList.Codes.PassedContainmentBarrier, IterationLinkOutcomeList.Codes.IterationRequired, 1);

			task5.Iteration = "1";
			var pivot = GetPivotForTask(task5);
			AssertNotNull(pivot);

			var iterationTaskOnPivotIteration = Factory.Load<ProcessTask>(pivot.Iteration.P9I_P9_IterationTask);

			AssertEquals("Only links with Type = 'QIT' and Outcome = 'ITR' should be considered when setting the selected iteration.", "Task 2", iterationTaskOnPivotIteration.P9_Description);
		}

		#endregion

		#region Changing Workflows

		public void TestChangeWorkflows_WhenTaskHasIteration_AndTaskWouldNotBeAutoAssignedToIteration_ShouldClearIterationAndDeletePivot()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 2");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow1);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow1, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);

			var task = (ProcessTask)BMSTestHelper.CreateTask(workflow1);
			task.Iteration = "1";
			var pivot = GetPivotForTask(task);

			task.P9_FH_ProcessHeader = workflow2.PK;

			AssertEquals("Moving the task to a different workflow should delete the original pivot. A task can only be associated with an iteration in the workflow that it's in.", true, ((BusinessObject)pivot).IsDeleted);
			AssertEquals("The task should no longer be associated with a quality iteration because it was moved to a different workflow.", ZString.Empty, task.Iteration);
		}

		#endregion

		#region Auto Association

		#region Add Task

		public void TestAddTask_WhenWorkflowHasNoIterations_ShouldNotAssignIteration()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow);

			AssertEquals(ZString.Empty, task1.Iteration);
			AssertEquals(ZString.Empty, task2.Iteration);
		}

		public void TestAddTask_WhenAllTasksInWorkflowHaveSameIteration_ShouldBeAssignedToThatIteration()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			var iterationWorkflow = BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration");
			var iterationTasks = iterationWorkflow.Tasks.Cast<ProcessTask>().ToArray();

			AssertContainsExactElementsInAnyOrder(new[] { "1", "1" }, iterationTasks.Select(x => x.Iteration));

			var newTask = (ProcessTask)BMSTestHelper.CreateTask(iterationWorkflow);
			AssertEquals("All existing tasks in the workflow are in an iteration, so we should assume the new tasks should be in that iteration as well.", "1", newTask.Iteration);
		}

		public void TestAddTask_WhenAnyTaskInWorkflowIsNotInAnyIteration_AndSequenceIsHighestInWorkflow_ShouldNotAssignIteration()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);

			var allTasks = workflow.Tasks.Cast<ProcessTask>().ToArray();
			AssertContainsExactElementsInAnyOrder(new[] { string.Empty, string.Empty, "1", "1" }, allTasks.Select(x => x.Iteration));

			var newTask = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			AssertEquals("Not all existing tasks in the workflow are in an iteration, so we should not assume the new tasks should be in that iteration as well.", ZString.Empty, newTask.Iteration);
		}

		public void TestAddTask_WhenWorkflowHasMultipleIterations_AndSequenceIsHighestInWorkflow_ShouldNotAssignIteration()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			var iterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 4);

			BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "QI", shouldCreateWorkflowForIteration: false);
			var iterationTask2 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 5);
			var containmentBarrierIterationTask2 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 6);

			task1.Iteration = "1";
			task2.Iteration = "2";

			AssertEquals("1", task1.Iteration);
			AssertEquals("2", task2.Iteration);
			AssertEquals("1", iterationTask1.Iteration);
			AssertEquals("1", containmentBarrierIterationTask1.Iteration);
			AssertEquals("2", iterationTask2.Iteration);
			AssertEquals("2", containmentBarrierIterationTask2.Iteration);

			var newTask = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			AssertEquals("All tasks in the workflow have an iteration, but there's more than one iteration to choose from, so new tasks should not have an iteration automatically assigned.", ZString.Empty, newTask.Iteration);
		}

		#endregion

		#region Move to different workflow

		public void TestMoveTask_WhenWorkflowHasNoIterations_ShouldNotAssignIteration()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow1);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow1, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			var iterationTask1 = (ProcessTask)workflow1.Tasks.Single(x => x.P9_Sequence == 3);

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 2");
			var task3 = (ProcessTask)BMSTestHelper.CreateTask(workflow2);

			AssertEquals("1", iterationTask1.Iteration);
			AssertEquals(ZString.Empty, task3.Iteration);

			iterationTask1.P9_FH_ProcessHeader = workflow2.PK;
			AssertEquals("The destination workflow doesn't have any iterations, so the task should also not have one.", ZString.Empty, iterationTask1.Iteration);
		}

		public void TestMoveTask_WhenAllTasksInWorkflowHaveSameIteration_ShouldBeAssignedToThatIteration()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow1);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow1, taskType: "CB");

			var iterationWorkflow1 = BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration");
			var iterationTask1 = (ProcessTask)iterationWorkflow1.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)iterationWorkflow1.Tasks.Single(x => x.P9_Sequence == 4);

			var iterationWorkflow2 = BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "QI");
			var iterationTask2 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 5);
			var containmentBarrierIterationTask2 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 6);

			AssertEquals("1", iterationTask1.Iteration);
			AssertEquals("1", iterationTask2.Iteration);
			AssertEquals("1", containmentBarrierIterationTask2.Iteration);

			iterationTask1.P9_FH_ProcessHeader = iterationWorkflow2.PK;
			AssertEquals("All of the tasks in the destination workflow have the same iteration, so the moved task should get that iteration as well.", "1", iterationTask1.Iteration);
		}

		public void TestMoveTask_WhenAnyTaskInWorkflowIsNotInAnyIteration_AndSequenceIsHighestInWorkflow_ShouldNotAssignIteration()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow1);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow1, taskType: "CB");

			var iterationWorkflow1 = BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration");
			var iterationTask1 = (ProcessTask)iterationWorkflow1.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)iterationWorkflow1.Tasks.Single(x => x.P9_Sequence == 4);

			var iterationWorkflow2 = BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "QI");
			var iterationTask2 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 5);
			var containmentBarrierIterationTask2 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 6);

			iterationTask2.Iteration = ZString.Empty;
			AssertEquals(ZString.Empty, iterationTask2.Iteration);
			AssertEquals("1", containmentBarrierIterationTask2.Iteration);

			iterationTask1.P9_Sequence = 7;
			iterationTask1.P9_FH_ProcessHeader = iterationWorkflow2.PK;
			AssertEquals("Not all tasks in the destination workflow are associated with the same iteration, and since this task's sequence is the highest in that workflow, we can't assume its iteration.", ZString.Empty, iterationTask1.Iteration);
		}

		public void TestMoveTask_WhenWorkflowHasMultipleIterations_AndSequenceIsHighestInWorkflow_ShouldNotAssignIteration()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow1);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow1, taskType: "CB");

			var iterationWorkflow1 = BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration");
			var iterationTask1 = (ProcessTask)iterationWorkflow1.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)iterationWorkflow1.Tasks.Single(x => x.P9_Sequence == 4);

			var iterationWorkflow2 = BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "QI");
			var iterationTask2 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 5);
			var containmentBarrierIterationTask2 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 6);

			BMSTestHelper.CreateQualityIteration(iterationTask2, containmentBarrierIterationTask2, "QI", shouldCreateWorkflowForIteration: false);
			AssertContainsExactElementsInAnyOrder(new[] { "1", "1", "2", "2" }, iterationWorkflow2.Tasks.Cast<ProcessTask>().Select(x => x.Iteration));

			iterationTask1.P9_Sequence = 9;
			iterationTask1.P9_FH_ProcessHeader = iterationWorkflow2.PK;

			AssertEquals("There are multiple iterations in the destination workflow, and since this task's sequence is the highest in that workflow, we can't assume its iteration.", ZString.Empty, iterationTask1.Iteration);
		}

		public void TestMoveTask_WhenSequenceEqualsTaskWithIteration_AndWorkflowHasOneIterationButNotAllTasksInIt_ShouldAssignIteration()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow1);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow1, taskType: "CB");

			var iterationWorkflow1 = BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration");
			var iterationTask1 = (ProcessTask)iterationWorkflow1.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)iterationWorkflow1.Tasks.Single(x => x.P9_Sequence == 4);

			var iterationWorkflow2 = BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "QI");
			var iterationTask2 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 5);
			var containmentBarrierIterationTask2 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 6);

			var startingPivotIteration = iterationTask1.IterationPivot.P9P_P9I_Iteration;

			iterationTask2.Iteration = ZString.Empty;
			iterationTask1.P9_Sequence = containmentBarrierIterationTask2.P9_Sequence;
			iterationTask1.P9_FH_ProcessHeader = iterationWorkflow2.PK;

			AssertEquals("1", containmentBarrierIterationTask2.Iteration);
			AssertEquals("The sequence matches a task in the destination workflow with an iteration, so the same iteration should be assumed.", "1", iterationTask1.Iteration);
			AssertNotEquals("Should be pointing to a new iteration, even though the sequence number is the same.", startingPivotIteration, iterationTask1.IterationPivot.P9P_P9I_Iteration);
		}

		public void TestMoveTask_WhenSequenceEqualsTaskWithIteration_AndWorkflowHasMultipleIterations_ShouldAssignIteration()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow1);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow1, taskType: "CB");

			var iterationWorkflow1 = BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration");
			var iterationTask1 = (ProcessTask)iterationWorkflow1.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)iterationWorkflow1.Tasks.Single(x => x.P9_Sequence == 4);

			var iterationWorkflow2 = BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "QI");
			var iterationTask2 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 5);
			var containmentBarrierIterationTask2 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 6);

			BMSTestHelper.CreateQualityIteration(iterationTask2, containmentBarrierIterationTask2, "QI", shouldCreateWorkflowForIteration: false);
			AssertContainsExactElementsInAnyOrder(new[] { "1", "1", "2", "2" }, iterationWorkflow2.Tasks.Cast<ProcessTask>().Select(x => x.Iteration));

			iterationTask1.P9_Sequence = containmentBarrierIterationTask2.P9_Sequence;
			iterationTask1.P9_FH_ProcessHeader = iterationWorkflow2.PK;

			AssertEquals("1", containmentBarrierIterationTask2.Iteration);
			AssertEquals("The sequence matches a task in the destination workflow with an iteration, so the same iteration should be assumed.", "1", iterationTask1.Iteration);

			iterationTask1.P9_FH_ProcessHeader = workflow1.PK;
			AssertEquals(ZString.Empty, iterationTask1.Iteration);

			iterationTask1.P9_Sequence = 7;
			iterationTask1.P9_FH_ProcessHeader = iterationWorkflow2.PK;

			AssertEquals("The sequence matches a task in the destination workflow with an iteration, so the same iteration should be assumed.", "2", iterationTask1.Iteration);
		}

		public void TestMoveTask_WhenSequenceBetweenTasksWithIteration_AndWorkflowHasOneIterationButNotAllTasksInIt_ShouldAssignIteration()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow1);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow1);
			var task3 = (ProcessTask)BMSTestHelper.CreateTask(workflow1, taskType: "CB");

			var iterationWorkflow1 = BMSTestHelper.CreateQualityIteration(task1, task3, "Quality Iteration");
			var iterationTask1 = (ProcessTask)iterationWorkflow1.Tasks.Single(x => x.P9_Sequence == 4);
			var containmentBarrierIterationTask1 = (ProcessTask)iterationWorkflow1.Tasks.Single(x => x.P9_Sequence == 6);

			var iterationWorkflow2 = BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "QI");
			var iterationTask2 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 7);
			var otherClonedTask = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 8);
			var containmentBarrierIterationTask2 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 9);

			otherClonedTask.Iteration = ZString.Empty;
			AssertEquals("The workflow should have at least one task that doesn't have an iteration. Otherwise the 'all tasks in the workflow have the same iteration' rule will cause the assignment, which is not what we want to test here.",
				2, iterationWorkflow2.Tasks.Cast<ProcessTask>().Select(x => x.Iteration).Distinct().Count());

			iterationTask2.P9_Sequence = 10;
			containmentBarrierIterationTask2.P9_Sequence = 20;
			iterationTask1.P9_Sequence = 14;
			iterationTask1.P9_FH_ProcessHeader = iterationWorkflow2.PK;

			AssertEquals("After moving the task, when sorting the destination workflow's tasks by sequence, the moved task falls between two tasks that belong to the same iteration. Therefore we can assume that the moved task should be in that iteration too.",
				"1", iterationTask1.Iteration);
		}

		public void TestMoveTask_WhenSequenceBetweenTasksWithIteration_AndWorkflowHasMultipleIterations_ShouldAssignIteration()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow1);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow1, taskType: "CB");

			var iterationWorkflow1 = BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration");
			var iterationTask1 = (ProcessTask)iterationWorkflow1.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)iterationWorkflow1.Tasks.Single(x => x.P9_Sequence == 4);

			var iterationWorkflow2 = BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "QI");
			var iterationTask2 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 5);
			var containmentBarrierIterationTask2 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 6);

			BMSTestHelper.CreateQualityIteration(iterationTask2, containmentBarrierIterationTask2, "QI", shouldCreateWorkflowForIteration: false);
			AssertContainsExactElementsInAnyOrder(new[] { "1", "1", "2", "2" }, iterationWorkflow2.Tasks.Cast<ProcessTask>().Select(x => x.Iteration));

			AssertEquals("The workflow should have at least one task that doesn't have an iteration. Otherwise the 'all tasks in the workflow have the same iteration' rule will cause the assignment, which is not what we want to test here.",
				2, iterationWorkflow2.Tasks.Cast<ProcessTask>().Select(x => x.Iteration).Distinct().Count());

			iterationTask2.P9_Sequence = 10;
			containmentBarrierIterationTask2.P9_Sequence = 20;
			iterationTask1.P9_Sequence = 14;
			iterationTask1.P9_FH_ProcessHeader = iterationWorkflow2.PK;

			AssertEquals("After moving the task, when sorting the destination workflow's tasks by sequence, the moved task falls between two tasks that belong to the same iteration. Therefore we can assume that the moved task should be in that iteration too.",
				"1", iterationTask1.Iteration);
		}

		public void TestMoveTask_WhenSequenceBetweenTasksWithIteration_ButOtherTasksAlsoAppearBetweenIterationTasks_ShouldNotAssignIteration()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow1);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow1, taskType: "CB");

			var iterationWorkflow1 = BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration");
			var iterationTask1 = (ProcessTask)iterationWorkflow1.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)iterationWorkflow1.Tasks.Single(x => x.P9_Sequence == 4);

			var iterationWorkflow2 = BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "QI");
			var iterationTask2 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 5);
			var containmentBarrierIterationTask2 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 6);

			BMSTestHelper.CreateQualityIteration(iterationTask2, containmentBarrierIterationTask2, "QI", shouldCreateWorkflowForIteration: false);
			var iterationTask3 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 7);
			AssertContainsExactElementsInAnyOrder(new[] { "1", "1", "2", "2" }, iterationWorkflow2.Tasks.Cast<ProcessTask>().Select(x => x.Iteration));

			AssertEquals("The workflow should have at least one task that doesn't have an iteration. Otherwise the 'all tasks in the workflow have the same iteration' rule will cause the assignment, which is not what we want to test here.",
				2, iterationWorkflow2.Tasks.Cast<ProcessTask>().Select(x => x.Iteration).Distinct().Count());

			iterationTask2.P9_Sequence = 10;
			containmentBarrierIterationTask2.P9_Sequence = 20;

			iterationTask3.P9_Sequence = 11; // this is the difference between this test and the test above.
			iterationTask3.Iteration = ZString.Empty;

			iterationTask1.P9_Sequence = 14;
			iterationTask1.P9_FH_ProcessHeader = iterationWorkflow2.PK;

			AssertEquals("After moving the task, when sorting the destination workflow's tasks by sequence, the moved task falls between a task with no iteration and a task with one. Therefore we can't assume its iteration.",
				ZString.Empty, iterationTask1.Iteration);
		}

		public void TestMoveTask_WhenWorkflowSequenceBetweenTwoDifferentIterations_HighestSequencedTasksIterationShouldBeAssigned()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow1);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow1, taskType: "CB");

			var iterationWorkflow1 = BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration");
			var iterationTask1 = (ProcessTask)iterationWorkflow1.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)iterationWorkflow1.Tasks.Single(x => x.P9_Sequence == 4);

			var iterationWorkflow2 = BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "QI");
			var iterationTask2 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 5);
			var containmentBarrierIterationTask2 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 6);

			BMSTestHelper.CreateQualityIteration(iterationTask2, containmentBarrierIterationTask2, "QI", shouldCreateWorkflowForIteration: false);
			var iterationTask3 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 7);
			AssertContainsExactElementsInAnyOrder(new[] { "1", "1", "2", "2" }, iterationWorkflow2.Tasks.Cast<ProcessTask>().Select(x => x.Iteration));

			AssertEquals("The workflow should have at least one task that doesn't have an iteration. Otherwise the 'all tasks in the workflow have the same iteration' rule will cause the assignment, which is not what we want to test here.",
				2, iterationWorkflow2.Tasks.Cast<ProcessTask>().Select(x => x.Iteration).Distinct().Count());

			iterationTask2.P9_Sequence = 10;
			iterationTask3.P9_Sequence = 20;
			AssertEquals("1", iterationTask2.Iteration);
			AssertEquals("2", iterationTask3.Iteration);

			iterationTask1.P9_Sequence = 14;
			iterationTask1.P9_FH_ProcessHeader = iterationWorkflow2.PK;

			AssertEquals(@"After moving the task, when sorting the destination workflow's tasks by sequence, the moved task falls between a task with one iteration and another task with a different iteration.
The iteration for the task with the higher sequence number should be chosen.", "2", iterationTask1.Iteration);
		}

		public void TestMoveTask_WhenWorkflowSequenceBetweenTwoDifferentIterations_HighestSequencedTasksIterationShouldBeAssigned_EvenWhenThatIterationHasLowerSequence()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow1);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow1, taskType: "CB");

			var iterationWorkflow1 = BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration");
			var iterationTask1 = (ProcessTask)iterationWorkflow1.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)iterationWorkflow1.Tasks.Single(x => x.P9_Sequence == 4);

			var iterationWorkflow2 = BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "QI");
			var iterationTask2 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 5);
			var containmentBarrierIterationTask2 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 6);

			BMSTestHelper.CreateQualityIteration(iterationTask2, containmentBarrierIterationTask2, "QI", shouldCreateWorkflowForIteration: false);
			var iterationTask3 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 7);
			AssertContainsExactElementsInAnyOrder(new[] { "1", "1", "2", "2" }, iterationWorkflow2.Tasks.Cast<ProcessTask>().Select(x => x.Iteration));

			AssertEquals("The workflow should have at least one task that doesn't have an iteration. Otherwise the 'all tasks in the workflow have the same iteration' rule will cause the assignment, which is not what we want to test here.",
				2, iterationWorkflow2.Tasks.Cast<ProcessTask>().Select(x => x.Iteration).Distinct().Count());

			iterationTask2.P9_Sequence = 20; // reversed these, unlike the previous test
			iterationTask3.P9_Sequence = 10;
			AssertEquals("1", iterationTask2.Iteration);
			AssertEquals("2", iterationTask3.Iteration);

			iterationTask1.P9_Sequence = 14;
			iterationTask1.P9_FH_ProcessHeader = iterationWorkflow2.PK;

			AssertEquals(@"After moving the task, when sorting the destination workflow's tasks by sequence, the moved task falls between a task with one iteration and another task with a different iteration.
The iteration for the task with the higher sequence number should be chosen, even if that iteration's sequence is lower.", "1", iterationTask1.Iteration);
		}

		public void TestMoveTask_WhenEligibleForAssignmentToMoreThanOneIteration_HighestSequencedIterationShouldBeAssigned()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow1);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow1, taskType: "CB");

			var iterationWorkflow1 = BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration");
			var iterationTask1 = (ProcessTask)iterationWorkflow1.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)iterationWorkflow1.Tasks.Single(x => x.P9_Sequence == 4);

			var iterationWorkflow2 = BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "QI");
			var iterationTask2 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 5);
			var containmentBarrierIterationTask2 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 6);

			BMSTestHelper.CreateQualityIteration(iterationTask2, containmentBarrierIterationTask2, "QI", shouldCreateWorkflowForIteration: false);
			var iterationTask3 = (ProcessTask)iterationWorkflow2.Tasks.Single(x => x.P9_Sequence == 7);
			AssertContainsExactElementsInAnyOrder(new[] { "1", "1", "2", "2" }, iterationWorkflow2.Tasks.Cast<ProcessTask>().Select(x => x.Iteration));

			iterationTask2.P9_Sequence = 7;
			AssertEquals("1", iterationTask2.Iteration);
			AssertEquals("2", iterationTask3.Iteration);
			AssertEquals(7, iterationTask2.P9_Sequence);
			AssertEquals(7, iterationTask3.P9_Sequence);

			iterationTask1.P9_Sequence = 7;
			iterationTask1.P9_FH_ProcessHeader = iterationWorkflow2.PK;

			AssertEquals("There is more than one iteration that matches the task's sequence, so the iteration with the highest sequence number should be chosen.", "2", iterationTask1.Iteration);
		}

		#endregion

		#region Change sequence number

		public void TestChangeSequence_WhenWorkflowHasNoIterations_ShouldNotAssignIteration()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow1);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow1, taskType: "CB");

			task1.P9_Sequence = 2;
			AssertEquals("The workflow doesn't have any iterations, so the task should also not have one.", ZString.Empty, task1.Iteration);
		}

		public void TestChangeSequence_WhenAllTasksInWorkflowHaveSameIteration_ShouldLeaveExistingIterationAssignment()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow1);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow1, taskType: "CB");

			var iterationWorkflow1 = BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration");
			var iterationTask1 = (ProcessTask)iterationWorkflow1.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)iterationWorkflow1.Tasks.Single(x => x.P9_Sequence == 4);

			AssertEquals("1", iterationTask1.Iteration);

			iterationTask1.P9_Sequence = 6;
			AssertEquals("All the tasks in the workflow have the same iteration, so moving one should not change that iteration assignment.", "1", iterationTask1.Iteration);
		}

		public void TestChangeSequence_WhenAnyTaskInWorkflowIsNotInAnyIteration_AndSequenceIsHighestInWorkflow_ShouldNotAssignIteration()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);

			var allTasks = workflow.Tasks.Cast<ProcessTask>().ToArray();
			AssertContainsExactElementsInAnyOrder(new[] { string.Empty, string.Empty, "1", "1" }, allTasks.Select(x => x.Iteration));
			AssertEquals(4, allTasks.Max(x => x.P9_Sequence));

			task1.P9_Sequence = 5;
			AssertEquals("Not all existing tasks in the workflow are in an iteration, so we should not assume the re-sequenced task should be in that iteration as well when it has been moved to the highest sequence.", ZString.Empty, task1.Iteration);
		}

		public void TestChangeSequence_WhenWorkflowHasMultipleIterations_AndSequenceIsHighestInWorkflow_ShouldNotAssignIteration()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			var iterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 4);

			BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "QI", shouldCreateWorkflowForIteration: false);

			var allTasks = workflow.Tasks.Cast<ProcessTask>().ToArray();
			AssertContainsExactElementsInAnyOrder(new[] { string.Empty, string.Empty, "1", "1", "2", "2" }, allTasks.Select(x => x.Iteration));
			AssertEquals(6, allTasks.Max(x => x.P9_Sequence));

			task1.P9_Sequence = 7;

			AssertEquals("There are multiple iterations in the workflow, so we should not assume the re-sequenced task should be in that iteration as well when it has been moved to the highest sequence.", ZString.Empty, task1.Iteration);
		}

		public void TestChangeSequence_WhenSequenceEqualsTaskWithIteration_AndChangingTaskHasNoIteration_ShouldAssignIteration()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);

			AssertEquals(ZString.Empty, task1.Iteration);

			task1.P9_Sequence = 4;
			AssertEquals("The new sequence matches one task with an iteration, and the moving task doesn't have an iteration yet, so the iteration from the same-sequenced task should be selected.", "1", task1.Iteration);
		}

		public void TestChangeSequence_WhenSequenceEqualsTaskWithIteration_AndChangingTaskHasIteration_ShouldLeaveExistingIterationAssignment()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			var iterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 4);

			BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "QI", shouldCreateWorkflowForIteration: false);
			var iterationTask2 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 5);

			var allTasks = workflow.Tasks.Cast<ProcessTask>().ToArray();
			AssertContainsExactElementsInAnyOrder(new[] { string.Empty, string.Empty, "1", "1", "2", "2" }, allTasks.Select(x => x.Iteration));

			AssertEquals("1", iterationTask1.Iteration);
			AssertEquals("2", iterationTask2.Iteration);

			iterationTask1.P9_Sequence = iterationTask2.P9_Sequence; // 5
			AssertEquals("The new sequence matches one task with an iteration, but the moving task already has an iteration, so that shouldn't be changed.", "1", iterationTask1.Iteration);
		}

		public void TestChangeSequence_WhenSequenceBetweenTasksWithIteration_AndChangingTaskHasNoIteration_ShouldAssignIteration()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			var iterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 4);

			BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "QI", shouldCreateWorkflowForIteration: false);
			var iterationTask2 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 5);

			var allTasks = workflow.Tasks.Cast<ProcessTask>().ToArray();
			AssertContainsExactElementsInAnyOrder(new[] { string.Empty, string.Empty, "1", "1", "2", "2" }, allTasks.Select(x => x.Iteration));

			iterationTask1.P9_Sequence = 10;
			iterationTask2.P9_Sequence = 20;

			AssertEquals("1", iterationTask1.Iteration);
			AssertEquals("2", iterationTask2.Iteration);

			task1.P9_Sequence = 16;
			AssertEquals(@"When sorting the workflow's tasks by sequence, the re-sequenced task falls between a task with one iteration and another task with a different iteration.
The iteration for the task with the higher sequence number should be chosen.", "2", task1.Iteration);
		}

		public void TestChangeSequence_WhenSequenceBetweenTasksWithIteration_AndChangingTaskHasNoIteration_ShouldAssignIteration_EvenWhenThatIterationHasLowerSequence()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			var iterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 4);

			BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "QI", shouldCreateWorkflowForIteration: false);
			var iterationTask2 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 5);

			var allTasks = workflow.Tasks.Cast<ProcessTask>().ToArray();
			AssertContainsExactElementsInAnyOrder(new[] { string.Empty, string.Empty, "1", "1", "2", "2" }, allTasks.Select(x => x.Iteration));

			iterationTask1.P9_Sequence = 20; // reversed from above test.
			iterationTask2.P9_Sequence = 10;

			AssertEquals("1", iterationTask1.Iteration);
			AssertEquals("2", iterationTask2.Iteration);

			task1.P9_Sequence = 16;
			AssertEquals(@"When sorting the workflow's tasks by sequence, the re-sequenced task falls between a task with one iteration and another task with a different iteration.
The iteration for the task with the higher sequence number should be chosen, even if that iteration's sequence is lower.", "1", task1.Iteration);
		}

		public void TestChangeSequence_WhenSequenceBetweenTasksWithIteration_AndChangingTaskHasIteration_ShouldLeaveExistingIterationAssignment()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			var iterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 4);

			BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "QI", shouldCreateWorkflowForIteration: false);
			var iterationTask2 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 5);
			var containmentBarrierIterationTask2 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 6);

			BMSTestHelper.CreateQualityIteration(iterationTask2, containmentBarrierIterationTask2, "QI", shouldCreateWorkflowForIteration: false);
			var iterationTask3 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 7);
			AssertContainsExactElementsInAnyOrder(new[] { string.Empty, string.Empty, "1", "1", "2", "2", "3", "3" }, workflow.Tasks.Cast<ProcessTask>().Select(x => x.Iteration));

			iterationTask2.P9_Sequence = 10;
			iterationTask3.P9_Sequence = 20;

			AssertEquals("1", iterationTask1.Iteration);
			AssertEquals("2", iterationTask2.Iteration);
			AssertEquals("3", iterationTask3.Iteration);

			iterationTask1.P9_Sequence = 14;
			AssertEquals("The re-sequenced task already has an iteration, so this should not be changed.", "1", iterationTask1.Iteration);
		}

		public void TestChangeSequence_WhenOtherTasksAlsoAppearBetweenIterationTasks_AndChangingTaskHasNoIteration_ShouldNotAssignIteration()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			var iterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 4);

			BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "QI", shouldCreateWorkflowForIteration: false);
			var iterationTask2 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 5);

			var allTasks = workflow.Tasks.Cast<ProcessTask>().ToArray();
			AssertContainsExactElementsInAnyOrder(new[] { string.Empty, string.Empty, "1", "1", "2", "2" }, allTasks.Select(x => x.Iteration));

			iterationTask1.P9_Sequence = 10;
			iterationTask2.P9_Sequence = 20;
			task1.P9_Sequence = 13;
			task1.Iteration = ZString.Empty;

			AssertEquals("1", iterationTask1.Iteration);
			AssertEquals("2", iterationTask2.Iteration);

			AssertEquals(ZString.Empty, task1.Iteration);
			AssertEquals(ZString.Empty, task2.Iteration);

			task2.P9_Sequence = 16;
			AssertEquals(@"When sorting the workflow's tasks by sequence, the re-sequenced task falls between a task with not iteration and another task with an iteration.
Therefore we shouldn't assume its iteration.", ZString.Empty, task2.Iteration);
		}

		public void TestChangeSequence_WhenOtherTasksAlsoAppearBetweenIterationTasks_AndChangingTaskHasIteration_ShouldLeaveExistingIterationAssignment()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			var iterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 4);

			BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "QI", shouldCreateWorkflowForIteration: false);
			var iterationTask2 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 5);
			var containmentBarrierIterationTask2 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 6);

			BMSTestHelper.CreateQualityIteration(iterationTask2, containmentBarrierIterationTask2, "QI", shouldCreateWorkflowForIteration: false);
			var iterationTask3 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 7);
			AssertContainsExactElementsInAnyOrder(new[] { string.Empty, string.Empty, "1", "1", "2", "2", "3", "3" }, workflow.Tasks.Cast<ProcessTask>().Select(x => x.Iteration));

			iterationTask2.P9_Sequence = 10;
			iterationTask3.P9_Sequence = 20;
			task1.P9_Sequence = 13;
			task1.Iteration = ZString.Empty;

			AssertEquals("1", iterationTask1.Iteration);
			AssertEquals("2", iterationTask2.Iteration);
			AssertEquals("3", iterationTask3.Iteration);
			AssertEquals(ZString.Empty, task1.Iteration);

			iterationTask1.P9_Sequence = 14;
			AssertEquals("The re-sequenced task already has an iteration, so this should not be changed.", "1", iterationTask1.Iteration);
		}

		public void TestChangeSequence_WhenSequenceBetweenTwoDifferentIterations_AndChangingTaskHasNoIteration_HighestSequencedTasksIterationShouldBeAssigned()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			var iterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 4);

			BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "QI", shouldCreateWorkflowForIteration: false);
			var iterationTask2 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 5);
			var containmentBarrierIterationTask2 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 6);

			BMSTestHelper.CreateQualityIteration(iterationTask2, containmentBarrierIterationTask2, "QI", shouldCreateWorkflowForIteration: false);
			var iterationTask3 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 7);
			var containmentBarrierIterationTask3 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 8);
			AssertContainsExactElementsInAnyOrder(new[] { string.Empty, string.Empty, "1", "1", "2", "2", "3", "3" }, workflow.Tasks.Cast<ProcessTask>().Select(x => x.Iteration));

			iterationTask2.P9_Sequence = 10;
			iterationTask3.P9_Sequence = 20;
			containmentBarrierIterationTask2.P9_Sequence = 20;
			containmentBarrierIterationTask3.P9_Sequence = 10;

			AssertEquals("1", iterationTask1.Iteration);
			AssertEquals("2", iterationTask2.Iteration);
			AssertEquals("2", containmentBarrierIterationTask2.Iteration);
			AssertEquals("3", iterationTask3.Iteration);
			AssertEquals("3", containmentBarrierIterationTask3.Iteration);
			AssertEquals(ZString.Empty, task1.Iteration);

			task1.P9_Sequence = 14;
			AssertEquals("When multiple iterations are available, the correct iteration should be selected first by the highest sequence number of the next highest task(s), then by the highest iteration number.", "3", task1.Iteration);
		}

		public void TestChangeSequence_WhenEligibleForAssignmentToMoreThanOneIteration_HighestSequencedIterationShouldBeAssigned()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)BMSTestHelper.CreateTask(workflow);
			var task2 = (ProcessTask)BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			var iterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask1 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 4);

			BMSTestHelper.CreateQualityIteration(iterationTask1, containmentBarrierIterationTask1, "QI", shouldCreateWorkflowForIteration: false);
			var iterationTask2 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 5);
			var containmentBarrierIterationTask2 = (ProcessTask)workflow.Tasks.Single(x => x.P9_Sequence == 6);

			iterationTask1.P9_Sequence = 5;

			AssertEquals("1", iterationTask1.Iteration);
			AssertEquals("2", iterationTask2.Iteration);
			AssertEquals(5, iterationTask1.P9_Sequence);
			AssertEquals(5, iterationTask2.P9_Sequence);
			AssertEquals(ZString.Empty, task1.Iteration);

			task1.P9_Sequence = 5;

			AssertEquals("The new sequence matches multiple tasks with multiple iterations, so the highest iteration sequence should be selected.", "2", task1.Iteration);
		}

		#endregion

		#endregion

		#region Implementation

		IProcessTaskIterationLinkPivot GetPivotForTask(IProcessTask task)
		{
			return Factory.LoadTop1<IProcessTaskIterationLinkPivot>(new ZQuery(ProcessTaskIterationLinkPivotSchema.P9P_P9_Task, task.PK));
		}

		IProcessTaskIterationLink GetIterationCreatedFromContainmentBarrierTask(IProcessTask task)
		{
			return Factory.LoadTop1<IProcessTaskIterationLink>(new ZQuery(ProcessTaskIterationLinkSchema.P9I_P9_ContainmentBarrierTask, task.PK));
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("CB", "DUM");
		}

		IBMTestHelper BMSTestHelper => bmsTestHelper ?? (bmsTestHelper = ObjectFactory.Get<IBMTestHelper>());
		IBMTestHelper bmsTestHelper;

		#endregion
	}
}
