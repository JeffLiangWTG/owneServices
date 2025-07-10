using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(WorkflowTriggerCollectionIncludingRelatedView))]
	sealed class WorkflowTriggerCollectionIncludingRelatedViewTest : ProcessTaskBaseCollectionIncludingRelatedViewTest<WorkflowTriggerCollectionIncludingRelatedView>
	{
		public override void TestIsThisPartOfTheCollection()
		{
			var task = Collection.AddNew();
			task.IsWorkflowTrigger = false;
			AssertEquals(true, task.IsTask);

			var milestone = Collection.AddNew();
			milestone.IsMilestone = true;

			var exception = Collection.AddNew();
			exception.IsException = true;

			var trigger = Collection.AddNew();
			trigger.IsWorkflowTrigger = true;

			AssertEquals("The item that returns true on IsTypeMatch should be included", 1, Collection.Count);
			AssertEquals("The item that returns true on IsTypeMatch should be included", trigger, Collection[0]);
		}

		public new void TestFetchHintsOnRebuild()
		{
			Dummy.InitRelatedDummyWithTasks();
			Dummy.WorkflowItems.Tasks.AddNew();
			Dummy.RelatedDummyWithTasks.WorkflowItems.Tasks.AddNew();
			Dummy.RelatedDummyWithTasks2.WorkflowItems.Tasks.AddNew();
			Factory.Save();

			CollectionIncludingRelated.Load();

			AssertEquals("1 hit for main collection, and 2 for related, and 1 each for looking for line triggers", 5, Factory.GetTableHitCount(ProcessTasksSchema.Constants.TableName));
		}

		public new void TestAllowSort()
		{
			Assert(((System.ComponentModel.IBindingList)Collection).SupportsSorting);
		}

		#region ProcessTaskBaseCollectionIncludingRelatedViewTest

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<ProcessTask>();
			result.IsWorkflowTrigger = true;

			return result;
		}

		protected override WorkflowItemCollectionView GetNewCollectionViewNotIncludingRelated(ProcessTaskCollection collection)
		{
			return new WorkflowTriggerCollectionView(collection);
		}

		protected override WorkflowItemCollectionView GetNewCollectionViewIncludingRelated(ProcessTaskCollection collection)
		{
			return new WorkflowTriggerCollectionIncludingRelatedView(collection);
		}

		protected override void SetUp()
		{
			AssertNotNull(DummyWorkflowDescriptor.Instance);
			base.SetUp();
		}

		#endregion
	}
}
