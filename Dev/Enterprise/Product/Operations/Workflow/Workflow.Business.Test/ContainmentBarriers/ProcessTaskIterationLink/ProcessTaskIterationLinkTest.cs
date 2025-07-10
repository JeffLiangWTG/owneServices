using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(ProcessTaskIterationLink))]
	class ProcessTaskIterationLinkTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var link = Factory.New<ProcessTaskIterationLink>();

			AssertEquals(IterationLinkTypeList.Codes.QualityIterationTask, link.P9I_LinkType);
			AssertEquals(new ZByte(1), link.P9I_Sequence);
		}

		public void TestDelete_ShouldDeleteTaskPivots()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			var jobHeader = helper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobHeader, "Workflow");

			var task1 = helper.CreateTask(workflow);
			var task2 = helper.CreateTask(workflow);
			var task3 = helper.CreateTask(workflow);

			var iteration = Factory.New<IProcessTaskIterationLink>();
			iteration.P9I_FH_IterationWorkflow = workflow.PK;
			iteration.P9I_P9_ContainmentBarrierTask = task1.PK;
			iteration.P9I_P9_IterationTask = task2.PK;
			iteration.P9I_LinkType = IterationLinkTypeList.Codes.QualityIterationTask;
			iteration.P9I_Outcome = IterationLinkOutcomeList.Codes.IterationRequired;
			iteration.P9I_GS_NKResourceUnderReview = GlbStaff.GetCurrentUser(Factory).GS_Code;

			var pivot1 = iteration.TaskPivots.AddNewForTask(task1);
			var pivot2 = iteration.TaskPivots.AddNewForTask(task2);
			var pivot3 = iteration.TaskPivots.AddNewForTask(task3);

			Factory.Save();

			iteration.Delete();

			AssertEquals("Deleting an iteration should delete any associated pivots. SAD!", true, ((BusinessObject)pivot1).IsDeleted);
			AssertEquals("Deleting an iteration should delete any associated pivots. SAD!", true, ((BusinessObject)pivot2).IsDeleted);
			AssertEquals("Deleting an iteration should delete any associated pivots. SAD!", true, ((BusinessObject)pivot3).IsDeleted);
		}

		public void TestDelete_ShouldOnlyHitPivotTableOnce()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			var jobHeader = helper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobHeader, "Workflow");

			var task1 = helper.CreateTask(workflow);
			var task2 = helper.CreateTask(workflow);
			var task3 = helper.CreateTask(workflow);

			var iteration = Factory.New<ProcessTaskIterationLink>();
			iteration.P9I_FH_IterationWorkflow = workflow.PK;
			iteration.P9I_P9_ContainmentBarrierTask = task1.PK;
			iteration.P9I_P9_IterationTask = task2.PK;
			iteration.P9I_LinkType = IterationLinkTypeList.Codes.QualityIterationTask;
			iteration.P9I_Outcome = IterationLinkOutcomeList.Codes.IterationRequired;
			iteration.P9I_GS_NKResourceUnderReview = GlbStaff.GetCurrentUser(Factory).GS_Code;

			var pivot1 = iteration.TaskPivots.AddNewForTask(task1);
			var pivot2 = iteration.TaskPivots.AddNewForTask(task2);
			var pivot3 = iteration.TaskPivots.AddNewForTask(task3);

			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loadedIteration = newFactory.Load<ProcessTaskIterationLink>(iteration.PK);

			loadedIteration.Delete();
			newFactory.Save();

			AssertEquals("We need to delete any corresponding pivots when deleting iterations, but we should load those pivots in one hit when deleting them all at once. SAD!",
				1, newFactory.GetTableHitCount(ProcessTaskIterationLinkPivotSchema.Constants.TableName));
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetLinkForStandardTests(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetLinkForStandardTests(factory);
		}

		static ProcessTaskIterationLink GetLinkForStandardTests(BusinessObjectFactory factory)
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			var jobHeader = helper.CreateJobHeader<DummyWithWorkflow>(factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobHeader, "Workflow");

			var task1 = helper.CreateTask(workflow);
			var task2 = helper.CreateTask(workflow);

			var iteration = factory.New<ProcessTaskIterationLink>();
			iteration.P9I_FH_IterationWorkflow = workflow.PK;
			iteration.P9I_P9_ContainmentBarrierTask = task1.PK;
			iteration.P9I_P9_IterationTask = task2.PK;
			iteration.P9I_LinkType = IterationLinkTypeList.Codes.QualityIterationTask;
			iteration.P9I_Outcome = IterationLinkOutcomeList.Codes.IterationRequired;
			iteration.P9I_GS_NKResourceUnderReview = GlbStaff.GetCurrentUser(factory).GS_Code;

			return iteration;
		}

		#endregion
	}
}
