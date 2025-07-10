using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ExceptionCollectionView))]
	sealed class ExceptionCollectionViewTest : ProcessTaskBaseCollectionViewTest<ExceptionCollectionView>
	{
		public void HasOpenExceptionsWithEventType()
		{
			ProcessTask exception = Dummy.WorkflowItems.Exceptions.AddNew();
			AssertEquals("Exception without an event code", false, Dummy.WorkflowItems.Exceptions.HasOpenExceptionsWithEventType(Events.Arrival, null));

			exception.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			AssertEquals("Exception open with matching event code", true, Dummy.WorkflowItems.Exceptions.HasOpenExceptionsWithEventType(Events.Arrival, null));

			exception.IsExceptionActioned = true;
			AssertEquals("Exception resolved", false, Dummy.WorkflowItems.Exceptions.HasOpenExceptionsWithEventType(Events.Arrival, null));
		}

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

		protected override WorkflowItemCollectionView GetNewCollectionView(ProcessTaskCollection collection)
		{
			return new ExceptionCollectionView(collection);
		}

		#endregion
	}
}
