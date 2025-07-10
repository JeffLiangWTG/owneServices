using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	class TaskTest : TestCaseWithFactory
	{
		public void TestCloneTask()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task = dummy.WorkflowItems.Tasks.AddNew();
			task.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			task.P9_SE_NKTaskCompletionEvent = Events.CustomisableEvent01Code;
			var clone = (ProcessTask)task.Clone();

			AssertEquals(Events.CustomisableEvent00Code, clone.P9_SE_NKMilestoneEvent);
			AssertEquals(Events.CustomisableEvent01Code, clone.P9_SE_NKTaskCompletionEvent);
		}

		public void TestProcessTaskSavePerformance()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			Factory.Save();

			var expectedHits = new Dictionary<string, int>
			{
				{ ProcessTasksSchema.Constants.TableName, 0 },
			};
			AssertDbHits(expectedHits, Factory);

			var newFactory = Factory.CreateNewFactory();
			var loadedTask = newFactory.Load<ProcessTask>(task.PK);

			loadedTask.P9_Description = "Bob";
			newFactory.Save();

			expectedHits = new Dictionary<string, int>
			{
				{ ProcessTasksSchema.Constants.TableName, 1 },
			};
			AssertDbHits(expectedHits, newFactory);
		}
	}
}
