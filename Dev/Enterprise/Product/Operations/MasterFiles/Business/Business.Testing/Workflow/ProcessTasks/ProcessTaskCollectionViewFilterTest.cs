using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business
{
	[TestedType(typeof(ProcessTaskCollectionViewFilter))]
	sealed class ProcessTaskCollectionViewFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTasksViewDoesntSetHasChanges()
		{
			using (ProcessTaskCollection.CanCreateTaskCollection())
			{
				var parent = Factory.New<OrgHeader>();
				parent.OH_Code = "PONYORG";
				var tasks = new ProcessTaskCollection(parent);
				var filter = new ProcessTaskCollectionViewFilter(tasks);

				Factory.Save();

				AssertEquals(false, parent.HasChanges);
				AssertEquals(false, tasks.HasChanges);
				AssertEquals(false, filter.HasChanges);

				filter.P9_Status = "SUS";

				AssertEquals(false, parent.HasChanges);
				AssertEquals(false, tasks.HasChanges);
				AssertEquals(false, filter.HasChanges);
			}
		}

		public void TestTasksViewFilterForTasksViewIsSet()
		{
			using (ProcessTaskCollection.CanCreateTaskCollection())
			{
				OrgOpportunity parent = Factory.New<OrgOpportunity>();
				ProcessTaskCollection tasks = new ProcessTaskCollection(parent);
				ProcessTaskCollectionViewFilter filter = new ProcessTaskCollectionViewFilter(tasks);
				AssertEquals("TaskViewFilter on Tasks collection should be the same as the new filter.", filter, tasks.Tasks.TasksViewFilter);
			}
		}

		public void TestTasksViewTakenFromTasks()
		{
			using (ProcessTaskCollection.CanCreateTaskCollection())
			{
				OrgOpportunity parent = Factory.New<OrgOpportunity>();
				ProcessTaskCollection tasks = new ProcessTaskCollection(parent);
				ProcessTaskCollectionViewFilter filter = new ProcessTaskCollectionViewFilter(tasks);
				AssertEquals("TaskView on filter should be the same view as that on the Tasks collection", tasks.Tasks, filter.TasksView);
			}
		}

		public void TestFilterTasks()
		{
			ZGuid guid1 = ZGuid.NewZGuid();
			ZGuid guid2 = ZGuid.NewZGuid();

			ProcessTask task1 = Filter.TasksView.AddNew();
			ProcessTask task2 = Filter.TasksView.AddNew();

			task1.P9_Status = "A";
			task1.P9_GS_NKAssignedStaffMember = "ZA";
			task2.P9_Type = "B";
			task2.P9_GG_AssignedGroup = guid1;

			Filter.P9_Status = "A";
			Filter.P9_Type = "";

			Filter.FilterTasks();
			AssertEquals("TasksView.Contains(Task1)", true, Filter.TasksView.Contains(task1));
			AssertEquals("TasksView.Contains(Task2)", false, Filter.TasksView.Contains(task2));

			Filter.P9_Status = "";
			Filter.P9_Type = "B";

			Filter.FilterTasks();
			AssertEquals("TasksView.Contains(Task1)", false, Filter.TasksView.Contains(task1));
			AssertEquals("TasksView.Contains(Task2)", true, Filter.TasksView.Contains(task2));

			Filter.P9_GG_AssignedGroup = guid2;

			Filter.FilterTasks();
			AssertEquals("TasksView.Contains(Task1)", false, Filter.TasksView.Contains(task1));
			AssertEquals("TasksView.Contains(Task2)", false, Filter.TasksView.Contains(task2));

			Filter.P9_Status = "A";
			Filter.P9_Type = "";
			Filter.P9_GG_AssignedGroup = Guid.Empty;
			Filter.P9_GS_NKAssignedStaffMember = "ZA";

			Filter.FilterTasks();
			AssertEquals("TasksView.Contains(Task1)", true, Filter.TasksView.Contains(task1));
			AssertEquals("TasksView.Contains(Task2)", false, Filter.TasksView.Contains(task2));

			Filter.P9_GS_NKAssignedStaffMember = "RA";

			Filter.FilterTasks();
			AssertEquals("TasksView.Contains(Task1)", false, Filter.TasksView.Contains(task1));
			AssertEquals("TasksView.Contains(Task2)", false, Filter.TasksView.Contains(task2));
		}

		public void TestSortInformationIsRetainedOnFilter()
		{
			ProcessTask task1 = Filter.TasksView.AddNew();
			ProcessTask task2 = Filter.TasksView.AddNew();

			task1.P9_Sequence = 2;
			task2.P9_Sequence = 1;

			Filter.TasksView.Sort(ProcessTasksSchema.Constants.P9_Sequence, ListSortDirection.Ascending);
			Filter.FilterTasks();

			AssertEquals("TasksView[0]", task2, Filter.TasksView[0]);
			AssertEquals("TasksView[1]", task1, Filter.TasksView[1]);
		}

		public void TestClearTasksFilter()
		{
			Filter.P9_Status = "A";
			Filter.P9_Type = "B";
			Filter.P9_GS_NKAssignedStaffMember = "ZA";
			Filter.P9_GG_AssignedGroup = ZGuid.NewZGuid();

			Filter.ClearTasksFilter();
			AssertEquals("P9_Status", "", Filter.P9_Status);
			AssertEquals("P9_Type", "", Filter.P9_Type);
			AssertEquals("P9_GS_NKAssignedStaffMember", "", Filter.P9_GS_NKAssignedStaffMember);
			AssertEquals("P9_GG_AssignedGroup", ZGuid.Empty, Filter.P9_GG_AssignedGroup);
		}

		public void TestRunPreSaveValidation()
		{
			ProcessTask task = Filter.TasksView.AddNew();
			task.P9_Status = "A";

			Filter.P9_Status = "A";
			Filter.P9_Type = "B";
			Filter.FilterTasks();

			Filter.RunPreSaveValidation();
			AssertEquals("TasksView.Contains(Task)", true, Filter.TasksView.Contains(task));
			AssertEquals("P9_Status", "", Filter.P9_Status);
			AssertEquals("P9_Type", "", Filter.P9_Type);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			DummyWithWorkflow parent = Factory.New<DummyWithWorkflow>();
			return new ProcessTaskCollectionViewFilter(parent.WorkflowItems);
		}

		ProcessTaskCollectionViewFilter Filter
		{
			get
			{
				if (fFilter == null)
				{
					fFilter = (ProcessTaskCollectionViewFilter)GetNewBusinessObject();
				}
				return fFilter;
			}
		}

		ProcessTaskCollectionViewFilter fFilter;

		#endregion
	}
}
