using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	abstract class MilestoneCollectionIncludingRelatedViewTest<T> : ProcessTaskBaseCollectionIncludingRelatedViewTest<T> where T : MilestoneCollectionIncludingRelatedView
	{
		public override void TestIsThisPartOfTheCollection()
		{
			ProcessTask task = Collection.AddNew();
			task.IsMilestone = false;
			task.IsException = false;

			ProcessTask milestone = Collection.AddNew();
			milestone.IsMilestone = true;
			milestone.IsException = false;

			ProcessTask exception = Collection.AddNew();
			exception.IsMilestone = false;
			exception.IsException = true;

			AssertEquals("The item that returns true on IsTypeMatch should be included", 1, Collection.Count);
			AssertEquals("The item that returns true on IsTypeMatch should be included", milestone, Collection[0]);
		}

		public void TestAllMilestonesAreIncludedForEnterprise()
		{
			bool originalValueForGlobalsIsWeb = Globals.IsWeb;

			ProcessTask publishedMilestone = Collection.AddNew();
			publishedMilestone.IsMilestone = true;
			publishedMilestone.IsException = false;
			publishedMilestone.P9_IsPublished = true;

			ProcessTask privateMilestone = Collection.AddNew();
			privateMilestone.IsMilestone = true;
			privateMilestone.IsException = false;
			privateMilestone.P9_IsPublished = false;

			try
			{
				Globals.IsWeb = false;
				AssertEquals("Both milestones should be visible from Enterprise", 2, Collection.Count);
				AssertCollectionContains("Published Milestone", publishedMilestone, Collection);
				AssertCollectionContains("Private Milestone", privateMilestone, Collection);
			}
			finally
			{
				Globals.IsWeb = originalValueForGlobalsIsWeb;
			}
		}

		public void TestOnlyPublishedMilestonesAreIncludedForWeb()
		{
			bool originalValueForGlobalsIsWeb = Globals.IsWeb;
			Globals.IsWeb = true;

			ProcessTask publishedMilestone = Collection.AddNew();
			publishedMilestone.IsMilestone = true;
			publishedMilestone.IsException = false;
			publishedMilestone.P9_IsPublished = true;

			ProcessTask privateMilestone = Collection.AddNew();
			privateMilestone.IsMilestone = true;
			privateMilestone.IsException = false;
			privateMilestone.P9_IsPublished = false;

			try
			{
				AssertEquals("Onlye one milestone should be visible from Web", 1, Collection.Count);
				AssertCollectionContains("Published Milestone", publishedMilestone, Collection);
				AssertCollectionNotContains("Private Milestone", privateMilestone, Collection);
			}
			finally
			{
				Globals.IsWeb = originalValueForGlobalsIsWeb;
			}
		}

		public void TestSetCollectionRelationships()
		{
			ProcessTask milestone = Dummy.WorkflowItems.Milestones.AddNew();
			AssertEquals("IsMilestone", true, milestone.IsMilestone);
			AssertEquals("P9_ParentID NOT attached to job", Dummy.PK, milestone.P9_ParentID);
		}

		public void TestSort_ByEstimatedDateWhenAvailable()
		{
			ProcessTask milestone3 = CollectionNotIncludingRelated.AddNew();
			ProcessTask milestone1 = CollectionNotIncludingRelated.AddNew();
			ProcessTask milestone2 = CollectionNotIncludingRelated.AddNew();

			milestone1.P9_ScheduledDateForBinding = new ZDateTimeOffset(new ZDateTime(2000, 1, 1));
			milestone2.P9_ScheduledDateForBinding = new ZDateTimeOffset(new ZDateTime(2000, 2, 2));
			milestone3.P9_ScheduledDateForBinding = new ZDateTimeOffset(new ZDateTime(2000, 3, 3));

			CollectionIncludingRelated.Sort(CollectionIncludingRelated.GetDefaultOrderComparer());
			AssertEquals("Sort by P9_ScheduledDate", milestone1.PK, CollectionIncludingRelated[0].PK);
			AssertEquals("Sort by P9_ScheduledDate", milestone2.PK, CollectionIncludingRelated[1].PK);
			AssertEquals("Sort by P9_ScheduledDate", milestone3.PK, CollectionIncludingRelated[2].PK);
		}

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			ProcessTask result = Factory.New<ProcessTask>();
			result.IsMilestone = true;
			return result;
		}

		protected override WorkflowItemCollectionView GetNewCollectionViewNotIncludingRelated(ProcessTaskCollection collection)
		{
			return new MilestoneCollectionView(collection);
		}

		protected override WorkflowItemCollectionView GetNewCollectionViewIncludingRelated(ProcessTaskCollection collection)
		{
			return new MilestoneCollectionIncludingRelatedView(collection);
		}

		#endregion
	}
}
