using System;
using CargoWise.Application;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	abstract class BreakdownStrategyTest<T> : WhsTestCaseWithFactory
		where T : ITaskCreationJobStrategy
	{
		public void TestObjectFactoryConfiguration()
		{
			AssertType<T>(ObjectFactory.Get<ITaskCreationJobStrategyFactory>().GetJobStrategy(JobType));
		}

		public void TestGetWorkflowInfo_NullJob()
		{
			var strategy = GetStrategy();
			AssertExceptionThrown<ArgumentNullException>(() => strategy.GetWorkflowInfo(null));
		}

		public void TestGetWorkflowInfo_JobDoesNotExist()
		{
			var strategy = GetStrategy();
			AssertExceptionThrown<ArgumentNullException>(() => strategy.GetWorkflowInfo(Factory.New<WhsReadyForPlanningJobsView>()));
		}

		public abstract void TestGetWorkflowInfo();

		public void TestSetJobPlanningStatus_NullArguments()
		{
			var readyForPlanningJob = Factory.New<WhsReadyForPlanningJobsView>();
			var strategy = GetStrategy();
			AssertExceptionThrown<ArgumentNullException>(() => strategy.SetJobPlanningStatus(null, readyForPlanningJob, "ERR"));
			AssertExceptionThrown<ArgumentNullException>(() => strategy.SetJobPlanningStatus(Factory, null, "ERR"));
			AssertExceptionThrown<ArgumentNullException>(() => strategy.SetJobPlanningStatus(Factory, readyForPlanningJob, null));
		}

		public void TestSetJobPlanningStatus_JobDoesNotExist()
		{
			TestSetJobPlanningStatus_JobDoesNotExistCore();
		}

		protected virtual void TestSetJobPlanningStatus_JobDoesNotExistCore()
		{
			var strategy = GetStrategy();
			AssertExceptionThrown<ArgumentNullException>(() => strategy.SetJobPlanningStatus(Factory, Factory.New<WhsReadyForPlanningJobsView>(), TaskPlanningStatus.Codes.Error));
		}

		public void TestSetJobPlanningStatus_Error()
			=> TestSetJobPlanningStatus(TaskPlanningStatus.Codes.Error);

		public void TestSetJobPlanningStatus_Empty()
			=> TestSetJobPlanningStatus(string.Empty);

		protected abstract void TestSetJobPlanningStatus(string status);

		public abstract void TestGetTasksToCreate();

		#region Implementation

		protected abstract string JobType { get; }

		protected abstract T GetStrategy();

		#endregion
	}
}
