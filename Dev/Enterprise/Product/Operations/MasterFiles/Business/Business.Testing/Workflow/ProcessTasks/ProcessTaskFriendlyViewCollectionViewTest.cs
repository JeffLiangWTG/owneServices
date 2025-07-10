using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessTaskFriendlyViewCollectionView))]
	sealed class ProcessTaskFriendlyViewCollectionViewTest : NonPersistentBusinessObjectCollectionTestCase<ProcessTaskFriendlyViewCollectionView>
	{
		protected override ProcessTaskFriendlyViewCollectionView GetCollectionToTest()
		{
			return new ProcessTaskFriendlyViewCollectionView(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			return new ProcessTaskFriendlyView(task);
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(ProcessTaskFriendlyViewCollectionView);
		}

		public void TestElementAddedAsProcessTaskFriendlyView()
		{
			var processTaskWrapper = processTaskWrapperCollection.AddNew();
			AssertEquals(1, processTaskWrapperCollection.Count);
			AssertEquals(processTaskWrapper.GetType(), typeof(ProcessTaskFriendlyView));
		}

		public void TestProcessTaskCollectionIsWrapped()
		{
			var processTaskCollection = new ProcessTaskCollection(Factory);
			var task1 = processTaskCollection.AddNew();
			var task2 = processTaskCollection.AddNew();
			var collection = new ProcessTaskFriendlyViewCollectionView(new ProcessTaskCollectionView(processTaskCollection));

			AssertEquals(2, collection.Count);
			AssertEquals(collection[0].GetType(), typeof(ProcessTaskFriendlyView));
			AssertEquals(collection[0].TaskID, task1.P9_TaskID);
		}

		ProcessTaskFriendlyViewCollectionView processTaskWrapperCollection;

		protected override void SetUp()
		{
			base.SetUp();
			processTaskWrapperCollection = new ProcessTaskFriendlyViewCollectionView(Factory);
		}
	}
}
