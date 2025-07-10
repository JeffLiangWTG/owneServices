using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(ViewProcessTask))]
	class ViewProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSaveNewTask_ShouldThrowException()
		{
			var taskView = Factory.New<ViewProcessTask>();

			AssertExceptionThrown<NotSupportedException>(Factory.Save);
		}

		public void TestUpdateExistingTask_ShouldThrowException()
		{
			var task = Factory.New<ProcessTask>();

			Factory.Save();

			var taskView = Factory.Load<ViewProcessTask>(task.PK);

			taskView.P9_Sequence = 2;
			AssertExceptionThrown<NotSupportedException>(Factory.Save);
		}

		public void TestDeleteExistingTask_ShouldThrowException()
		{
			var task = Factory.New<ProcessTask>();

			Factory.Save();

			var taskView = Factory.Load<ViewProcessTask>(task.PK);

			AssertExceptionThrown<NotSupportedException>(taskView.Delete);
		}

		public void TestIsTask_IsMilestone_IsTrigger()
		{
			var task = Factory.New<ViewProcessTask>();

			AssertEquals(true, task.IsTask());
			AssertEquals(false, task.IsTrigger());
			AssertEquals(false, task.IsMilestone());

			task.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			AssertEquals(false, task.IsTask());
			AssertEquals(true, task.IsTrigger());
			AssertEquals(false, task.IsMilestone());

			task.P9_Type = Core.Constants.Workflow.MilestoneType;
			AssertEquals(false, task.IsTask());
			AssertEquals(false, task.IsTrigger());
			AssertEquals(true, task.IsMilestone());
		}

		protected override bool IsDeleteSupported()
		{
			return false;
		}
	}
}
