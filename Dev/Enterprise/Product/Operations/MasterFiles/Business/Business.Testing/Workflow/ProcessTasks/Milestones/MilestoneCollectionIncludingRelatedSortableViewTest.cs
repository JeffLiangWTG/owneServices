using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(MilestoneCollectionIncludingRelatedSortableView))]
	sealed class MilestoneCollectionIncludingRelatedSortableViewTest : MilestoneCollectionIncludingRelatedViewTest<MilestoneCollectionIncludingRelatedSortableView>
	{
		public new void TestAllowSort()
		{
			Assert(((System.ComponentModel.IBindingList)Collection).SupportsSorting);
		}

		#region Implementation

		protected override WorkflowItemCollectionView GetNewCollectionViewIncludingRelated(ProcessTaskCollection collection)
		{
			return new MilestoneCollectionIncludingRelatedSortableView(collection);
		}

		#endregion
	}
}
