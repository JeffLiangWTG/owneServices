
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using ZSaveException = CargoWise.EntityFramework.ZSaveException;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(ProcessTaskIterationLinkPivot))]
	class ProcessTaskIterationLinkPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestClone_ShouldExcludeTaskPk()
		{
			var pivot = Factory.New<ProcessTaskIterationLinkPivot>();
			pivot.P9P_P9I_Iteration = ZGuid.NewZGuid();
			pivot.P9P_ParentId = ZGuid.NewZGuid();
			pivot.P9P_ParentTableCode = "ZZZ";
			pivot.P9P_P9_Task = ZGuid.NewZGuid();

			var clone = (ProcessTaskIterationLinkPivot)pivot.Clone();

			AssertEquals(pivot.P9P_P9I_Iteration, clone.P9P_P9I_Iteration);
			AssertEquals(pivot.P9P_ParentId, clone.P9P_ParentId);
			AssertEquals(pivot.P9P_ParentTableCode, clone.P9P_ParentTableCode);
			AssertEquals("Task PK should be excluded from the cloning, because each task can only have one iteration pivot. SAD!", ZGuid.Empty, clone.P9P_P9_Task);
		}

		public void TestOnSaving_WhenParentFieldsAreNotEmpty_ShouldNotReportError()
		{
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("CB", "DUM");
			var helper = ObjectFactory.Get<IBMTestHelper>();

			var jobHeader = helper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)helper.CreateTask(workflow);
			var task2 = (ProcessTask)helper.CreateTask(workflow, taskType: "CB");

			var iterationWorkflow = helper.CreateQualityIteration(task1, task2, "Quality Iteration");
			var iterationTask = iterationWorkflow.Tasks.Cast<ProcessTask>().First();
			var pivot = (ProcessTaskIterationLinkPivot)iterationTask.IterationPivot;

			AssertEquals(iterationTask.P9_ParentID, pivot.P9P_ParentId);
			AssertEquals(iterationTask.P9_ParentTableCode, pivot.P9P_ParentTableCode);

			Factory.Save();
			AssertEquals("If the parent fields on the pivot are filled in we shouldn't report any error.", string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestOnSaving_WhenParentIdFieldIsEmpty_ShouldAttemptToFillFromTask_AndReportError()
		{
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("CB", "DUM");
			var helper = ObjectFactory.Get<IBMTestHelper>();

			var jobHeader = helper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)helper.CreateTask(workflow);
			var task2 = (ProcessTask)helper.CreateTask(workflow, taskType: "CB");

			var iterationWorkflow = helper.CreateQualityIteration(task1, task2, "Quality Iteration");
			var iterationTask = iterationWorkflow.Tasks.Cast<ProcessTask>().First();
			var pivot = (ProcessTaskIterationLinkPivot)iterationTask.IterationPivot;

			pivot.P9P_ParentId = ZGuid.Empty;
			Factory.Save();

			AssertEquals("The missing fields should be filled in from the task on saving.", iterationTask.P9_ParentID, pivot.P9P_ParentId);
			AssertEquals(@"Tried to save ProcessTaskIterationLinkPivot with missing ParentId and/or ParentTableCode.
Details have been filled in from the pivot's task but this shouldn't be necessary.
Please investigate the stack trace and ensure that these fields are set before saving.
Task is not null.", ErrorReporter.LastMessageReported);

			AssertEquals("Just making sure we're not ignoring any other errors.", 1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestOnSaving_WhenParentTableCodeFieldIsEmpty_ShouldAttemptToFillFromTask_AndReportError()
		{
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("CB", "DUM");
			var helper = ObjectFactory.Get<IBMTestHelper>();

			var jobHeader = helper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)helper.CreateTask(workflow);
			var task2 = (ProcessTask)helper.CreateTask(workflow, taskType: "CB");

			var iterationWorkflow = helper.CreateQualityIteration(task1, task2, "Quality Iteration");
			var iterationTask = iterationWorkflow.Tasks.Cast<ProcessTask>().First();
			var pivot = (ProcessTaskIterationLinkPivot)iterationTask.IterationPivot;

			pivot.P9P_ParentTableCode = ZString.Empty;
			Factory.Save();

			AssertEquals("The missing fields should be filled in from the task on saving.", iterationTask.P9_ParentTableCode, pivot.P9P_ParentTableCode);
			AssertEquals(@"Tried to save ProcessTaskIterationLinkPivot with missing ParentId and/or ParentTableCode.
Details have been filled in from the pivot's task but this shouldn't be necessary.
Please investigate the stack trace and ensure that these fields are set before saving.
Task is not null.", ErrorReporter.LastMessageReported);

			AssertEquals("Just making sure we're not ignoring any other errors.", 1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestOnSaving_WhenParentFieldsAreEmpty_AndTaskIsNull_ShouldAttemptToFillFromTask_AndReportErrorWithNoteAboutNullTask()
		{
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("CB", "DUM");
			var helper = ObjectFactory.Get<IBMTestHelper>();

			var jobHeader = helper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)helper.CreateTask(workflow);
			var task2 = (ProcessTask)helper.CreateTask(workflow, taskType: "CB");

			var iterationWorkflow = helper.CreateQualityIteration(task1, task2, "Quality Iteration");
			var iterationTask = iterationWorkflow.Tasks.Cast<ProcessTask>().First();
			var pivot = (ProcessTaskIterationLinkPivot)iterationTask.IterationPivot;

			pivot.P9P_ParentTableCode = ZString.Empty;
			pivot.P9P_P9_Task = ZGuid.Empty;

			AssertExceptionThrown<ZSaveException>("The missing information will cause the pivot to not be able to be saved.", Factory.Save);

			AssertEquals("The missing fields would not have been able to be filled in when saving.", ZString.Empty, pivot.P9P_ParentTableCode);
			AssertEquals(@"Tried to save ProcessTaskIterationLinkPivot with missing ParentId and/or ParentTableCode.
Details have been filled in from the pivot's task but this shouldn't be necessary.
Please investigate the stack trace and ensure that these fields are set before saving.
Task is null.", ErrorReporter.LastMessageReported);

			AssertEquals("Just making sure we're not ignoring any other errors.", 1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewPivotForStandardTests(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewPivotForStandardTests(factory);
		}

		static ProcessTaskIterationLinkPivot GetNewPivotForStandardTests(BusinessObjectFactory factory)
		{
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("CB", "DUM");
			var helper = ObjectFactory.Get<IBMTestHelper>();

			var jobHeader = helper.CreateJobHeader<DummyWithWorkflow>(factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = (ProcessTask)helper.CreateTask(workflow);
			var task2 = (ProcessTask)helper.CreateTask(workflow, taskType: "CB");

			var iterationWorkflow = helper.CreateQualityIteration(task1, task2, "Quality Iteration");
			var iterationTask = iterationWorkflow.Tasks.Cast<ProcessTask>().First();

			return (ProcessTaskIterationLinkPivot)iterationTask.IterationPivot;
		}

		protected override void SetUp()
		{
			base.SetUp();

			WorkflowTestCase.EnableBufferManagement();
		}

		#endregion
	}
}
