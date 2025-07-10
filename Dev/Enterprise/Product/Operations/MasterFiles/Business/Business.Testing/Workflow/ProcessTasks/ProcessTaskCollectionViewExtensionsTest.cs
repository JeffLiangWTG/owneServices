using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	class ProcessTaskCollectionViewExtensionsTest : TestCaseWithFactory
	{
		public void TestNextTaskReturnsNullWhenNoTaskExists()
		{
			var collection = GetProcessTaskCollectionView();

			AssertNull(collection.GetNextTask());
		}

		public void TestNextTaskReturnsNullWhenAllTasksClosedOrCancelled()
		{
			var collection = GetProcessTaskCollectionView();

			var task1 = collection.AddNew();
			task1.P9_Type = "INV";
			task1.P9_Description = "Description1";

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertNull(collection.GetNextTask());

			var task2 = collection.AddNew();
			task2.P9_Type = "HLP";
			task2.P9_Description = "Description2";

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertNull(collection.GetNextTask());
		}
		public void TestNextTaskShowsFirstNonClosedOrCancelledTasks()
		{
			var collection = GetProcessTaskCollectionView();

			var task1 = collection.AddNew();
			task1.P9_Type = "INV";
			task1.P9_Description = "Description1";

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertNull(collection.GetNextTask());

			var task2 = collection.AddNew();
			task2.P9_Type = "CDU";
			task2.P9_Description = "Description2";

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals("Description2", collection.GetNextTask().P9_Description);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			AssertEquals("Description2", collection.GetNextTask().P9_Description);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			AssertEquals("Description2", collection.GetNextTask().P9_Description);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals("Description2", collection.GetNextTask().P9_Description);
		}

		public void TestNextTaskShowsTasksOrderedBySequenceAndThenTaskID()
		{
			var collection = GetProcessTaskCollectionView();

			var task1 = collection.AddNew();
			task1.P9_Type = "INV";
			task1.P9_Description = "Description1";
			task1.P9_Sequence = 1;

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals("Description1", collection.GetNextTask().P9_Description);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			AssertEquals("Description1", collection.GetNextTask().P9_Description);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			AssertEquals("Description1", collection.GetNextTask().P9_Description);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals("Description1", collection.GetNextTask().P9_Description);

			var task2 = collection.AddNew();
			task2.P9_Type = "CDU";
			task2.P9_Description = "Description2";
			task2.P9_Sequence = 2;

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals("Description1", collection.GetNextTask().P9_Description);

			task1.P9_Sequence = 2;
			task2.P9_Sequence = 1;
			AssertEquals("Description2", collection.GetNextTask().P9_Description);

			task1.P9_Sequence = 1;
			task2.P9_Sequence = 1;
			task1.P9_TaskID = "2";
			task2.P9_TaskID = "1";
			AssertEquals("Description2", collection.GetNextTask().P9_Description);
		}

		public ProcessTaskCollectionView GetProcessTaskCollectionView()
		{
			return new ProcessTaskCollectionView(Factory.New<DummyWithWorkflow>().WorkflowItems);
		}
	}
}
