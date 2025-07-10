using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(WorkflowTriggerCollectionView))]
	sealed class WorkflowTriggerCollectionViewTest : MilestoneOrTriggerCollectionViewTest<WorkflowTriggerCollectionView>
	{
		public override void TestIsThisPartOfTheCollection()
		{
			ProcessTask task = Collection.AddNew();
			task.IsWorkflowTrigger = false;
			AssertEquals(true, task.IsTask);

			ProcessTask milestone = Collection.AddNew();
			milestone.IsMilestone = true;

			ProcessTask exception = Collection.AddNew();
			exception.IsException = true;

			ProcessTask trigger = Collection.AddNew();
			trigger.IsWorkflowTrigger = true;

			AssertEquals("The item that returns true on IsTypeMatch should be included", 1, Collection.Count);
			AssertEquals("The item that returns true on IsTypeMatch should be included", trigger, Collection[0]);
		}

		public void TestSetCollectionRelationships()
		{
			ProcessTask trigger = Dummy.WorkflowItems.Triggers.AddNew();
			AssertEquals("IsWorkflowTrigger", true, trigger.IsWorkflowTrigger);
			AssertEquals("P9_ParentID NOT attached to job", Dummy.PK, trigger.P9_ParentID);
		}

		public new void TestAllowSort()
		{
			AssertEquals("Workflow Triggers may be sorted", true, Collection.AllowSort);
		}

		#region Test Classes

		class TestWorkflowTriggerCollectionView : WorkflowTriggerCollectionView
		{
			public TestWorkflowTriggerCollectionView(ProcessTaskCollection collection)
				: base(collection)
			{
			}

			public new bool AllowSort
			{
				get { return base.AllowSort; }
			}
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			ProcessTask result = Factory.New<ProcessTask>();
			result.IsWorkflowTrigger = true;
			return result;
		}

		new TestWorkflowTriggerCollectionView Collection
		{
			get { return (TestWorkflowTriggerCollectionView)base.Collection; }
		}

		protected override WorkflowItemCollectionView GetNewCollectionView(ProcessTaskCollection collection)
		{
			return new TestWorkflowTriggerCollectionView(collection);
		}

		#endregion
	}
}
