using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Integration;

namespace Enterprise.Workflow.Business.Test
{
	class TaskPreviewCopyValidationTest : WorkflowTestCase
	{
		public void TestIncludeInIteration()
		{
			var job = (BusinessObject)Factory.New<IWorkItem>();

			var task1 = BMTestHelper.CreateTask(job);
			var task2 = BMTestHelper.CreateTask(job);
			var qcbTask = BMTestHelper.CreateTask(job, taskType: "QCB");

			using (var viewModel = new ContainmentBarrierViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed, deselectCancelledTasksFromIteration: true))
			{
				viewModel.IterateFromTaskPK = task1.PK;
				AssertEquals(3, viewModel.IterationTaskPreviews.Count);

				AssertEquals(task1, viewModel.IterationTaskPreviews[0].Task);
				AssertEquals(task2, viewModel.IterationTaskPreviews[1].Task);
				AssertEquals(qcbTask, viewModel.IterationTaskPreviews[2].Task);

				viewModel.IterationTaskPreviews[0].IncludeInIteration = false;
				AssertHasError(viewModel.IterationTaskPreviews[0].IncludeInIterationInfo, "The Iterate From Task must be included in the Quality Iteration.");

				viewModel.IterationTaskPreviews[0].IncludeInIteration = true;
				viewModel.IterationTaskPreviews[2].IncludeInIteration = false;

				AssertHasError(viewModel.IterationTaskPreviews[2].IncludeInIterationInfo, "The Quality Containment Barrier task must be included in the Quality Iteration.");

				viewModel.IterationTaskPreviews[2].IncludeInIteration = true;
				viewModel.IterationTaskPreviews[1].IncludeInIteration = false;

				AssertNoErrors(viewModel.IterationTaskPreviews[0]);
				AssertNoErrors(viewModel.IterationTaskPreviews[1]);
				AssertNoErrors(viewModel.IterationTaskPreviews[2]);
				AssertNoErrors(viewModel);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			EnableBufferManagement();
			SetAsQCBTaskType("QCB", "WKI");
		}
	}
}
