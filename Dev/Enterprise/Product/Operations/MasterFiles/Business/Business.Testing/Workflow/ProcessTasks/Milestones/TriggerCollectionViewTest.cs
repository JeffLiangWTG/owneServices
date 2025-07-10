using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TriggerCollectionView))]
	sealed class TriggerCollectionViewTest : ProcessTaskBaseCollectionViewTest<TriggerCollectionView>
	{
		public override void TestIsThisPartOfTheCollection()
		{
			var task = Collection.AddNew();
			task.IsMilestone = false;
			task.IsWorkflowTrigger = false;

			var milestone = Collection.AddNew();
			milestone.IsMilestone = true;
			milestone.IsWorkflowTrigger = false;

			var trigger = Collection.AddNew();
			trigger.IsMilestone = false;
			trigger.IsWorkflowTrigger = true;

			AssertEquals("The item that returns true on IsTypeMatch should be included", 1, Collection.Count);
			AssertEquals("The item that returns true on IsTypeMatch should be included", trigger, Collection[0]);
		}

		[TestDate(2005, 1, 1)]
		public void TestSetCollectionRelationships()
		{
			var trigger = Dummy.WorkflowItems.Triggers.AddNew();
			AssertEquals("IsWorkflowTrigger", true, trigger.IsWorkflowTrigger);
			AssertEquals("P9_ParentID NOT attached to job", Dummy.PK, trigger.P9_ParentID);
		}

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<ProcessTask>();
			result.IsWorkflowTrigger = true;
			return result;
		}

		protected override WorkflowItemCollectionView GetNewCollectionView(ProcessTaskCollection collection)
		{
			return new TriggerCollectionView(collection);
		}

		#endregion
	}
}
