using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ExceptionCollectionIncludingRelatedView))]
	sealed class ExceptionCollectionIncludingRelatedViewTest : ProcessTaskBaseCollectionIncludingRelatedViewTest<ExceptionCollectionIncludingRelatedView>
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
			AssertEquals("The item that returns true on IsTypeMatch should be included", exception, Collection[0]);
		}

		public void TestFewerValueChangedEvents()
		{
			var exception = Collection.AddNew();
			exception.IsException = true;
			var valueChangedCount = 0;
			exception.IsExceptionInfo.ValueChanged += (s, e) => valueChangedCount++;
			Collection.Rebuild();
			AssertEquals(0, valueChangedCount);
		}

		[TestDate(2005, 1, 1)]
		public void TestSetCollectionRelationships()
		{
			ProcessTask exception = Dummy.WorkflowItems.Exceptions.AddNew();
			AssertEquals("IsException", true, exception.IsException);
			AssertEquals("P9_ParentID NOT attached to job", Dummy.PK, exception.P9_ParentID);
		}

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			ProcessTask result = Factory.New<ProcessTask>();
			result.IsException = true;
			return result;
		}

		protected override WorkflowItemCollectionView GetNewCollectionViewNotIncludingRelated(ProcessTaskCollection collection)
		{
			return new ExceptionCollectionView(collection);
		}

		protected override WorkflowItemCollectionView GetNewCollectionViewIncludingRelated(ProcessTaskCollection collection)
		{
			return new ExceptionCollectionIncludingRelatedView(collection);
		}

		#endregion
	}
}
