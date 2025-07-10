using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(ProcessTaskIterationLinkPivotCollection))]
	class ProcessTaskIterationLinkPivotCollectionTest : ActiveBusinessObjectCollectionTestCase<ProcessTaskIterationLinkPivotCollection>
	{
		public void TestAddNewForTask()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var task = dummy.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Nasty";

			var iteration = Factory.New<ProcessTaskIterationLink>();
			var collection = new ProcessTaskIterationLinkPivotCollection(iteration);

			AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), collection.Select(x => x.Task.P9_Description));

			var pivot = collection.AddNewForTask(task);

			AssertEquals(iteration.PK, pivot.P9P_P9I_Iteration);
			AssertEquals(task.PK, pivot.P9P_P9_Task);
			AssertEquals(dummy.PK, pivot.P9P_ParentId);
			AssertEquals("Z0", pivot.P9P_ParentTableCode);

			AssertContainsExactElementsInAnyOrder(new[] { "Nasty" }, collection.Select(x => x.Task.P9_Description));
		}

		#region Implementation

		protected override ProcessTaskIterationLinkPivotCollection GetCollectionToTest()
		{
			var iteration = Factory.New<ProcessTaskIterationLink>();
			return new ProcessTaskIterationLinkPivotCollection(iteration);
		}

		#endregion
	}
}
