using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	abstract class ProcessTaskBaseCollectionIncludingRelatedViewTest<T> : ProcessTaskBaseCollectionViewTest<T> where T : WorkflowItemCollectionView
	{
		[SnailTest]
		public void TestManyRelatedItems()
		{
			var dummiesToCreate = 1000;
			var dummies = new List<DummyWithWorkflow>(dummiesToCreate);
			for (int i = 0; i < dummiesToCreate; i++)
			{
				var dummy = Factory.New<DummyWithWorkflow>();
				dummy.GetRelatedWorkflowProviders_ForTest = () => dummies.Except(new[] { dummy });
				dummies.Add(dummy);
				GetNewCollectionView(dummy.WorkflowItems).AddNew();
			}
			Factory.Save();

			var view = (WorkflowItemCollectionIncludingRelatedView)GetNewCollectionView(dummies[0].WorkflowItems);
			AssertEquals("When there are so many related items, don't get the related items", 1, view.Count);
			AssertEquals("When there are so many related items, don't get the related items", false, view.CanShowRelatedItems());
		}

		public void TestLoad()
		{
			using (ProcessTaskCollection.CanCreateTaskCollection())
			{
				var collection = GetNewCollectionViewIncludingRelated(new DummyProcessTaskCollection(Dummy));
				AssertEquals("No items in collection initially", 0, collection.Count);
				ProcessTask item = GetNewCollectionViewIncludingRelated(new DummyProcessTaskCollection(Dummy)).AddNew();
				AssertEquals("No items after adding to another collection for the test", 0, collection.Count);

				collection.Load();
				AssertEquals("Loaded collection includes added item", 1, collection.Count);
			}
		}

		public virtual void TestRelatedItemsIncludedInCollection()
		{
			ProcessTask item = CollectionNotIncludingRelated.AddNew();
			Dummy.InitRelatedDummyWithTasks();
			ProcessTask relatedItem = GetNewCollectionViewIncludingRelated(Dummy.RelatedDummyWithTasks.WorkflowItems).AddNew();
			relatedItem.P9_LineTriggerType = DummyWorkflowDescriptor.Instance.Code;
			AssertEquals("1 ProcessTask in the wrapped ProcessTaskCollection initially", 1, Dummy.WorkflowItems.Count);

			AssertEquals("1 item and 1 related item", 2, CollectionIncludingRelated.Count);
			AssertCollectionContains("1 item and 1 related item", relatedItem, CollectionIncludingRelated);
			AssertCollectionContains("1 item and 1 related item", item, CollectionIncludingRelated);

			AssertEquals("Related ProcessTasks not added to the wrapped ProcessTaskCollection", 1, Dummy.WorkflowItems.Count);
		}

		public void TestIsRelatedItem()
		{
			Dummy.InitRelatedDummyWithTasks();
			ProcessTask relatedItem = GetNewCollectionViewNotIncludingRelated(Dummy.RelatedDummyWithTasks.WorkflowItems).AddNew();
			ProcessTask item = CollectionIncludingRelated.AddNew();
			AssertEquals("Related item", true, CollectionIncludingRelated.IsRelatedItem(relatedItem));
			AssertEquals("Non-related item", false, CollectionIncludingRelated.IsRelatedItem(item));
		}

		public void TestReloadedOnSaving()
		{
			GetNewCollectionViewIncludingRelated(Dummy.WorkflowItems).AddNew();

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			newFactory.RefreshEnabled = false;
			var loadedDummy = newFactory.Load<DummyWithWorkflow>(Dummy.PK);
			var dummyCollection = GetNewCollectionViewIncludingRelated(loadedDummy.WorkflowItems);

			GetNewCollectionViewIncludingRelated(Dummy.WorkflowItems).AddNew();
			Factory.Save();

			AssertEquals(1, dummyCollection.Count);
			loadedDummy.Factory.Save();
			AssertEquals("Saving factory must reload collection with related items so gui refreshes when milestones are automatically added on save", 2, dummyCollection.Count);
		}

		public void TestNotReloadedOnSavingIfParentIsDeleted()
		{
			AssertEquals("No items in collection initially", 0, CollectionIncludingRelated.Count);
			var item = GetNewCollectionViewIncludingRelated(Dummy.WorkflowItems).AddNew();
			AssertEquals("Creating extra ProcessTaskCollections is pretty wasteful", 1, CollectionIncludingRelated.Count);

			Dummy.Delete();
			Factory.Save();

			AssertEquals("Saving factory do not reload collection if Parent is deleted.", 1, CollectionIncludingRelated.Count);
		}

		public void TestFetchHintsOnRebuild()
		{
			Dummy.InitRelatedDummyWithTasks();
			Dummy.WorkflowItems.Tasks.AddNew();
			Dummy.RelatedDummyWithTasks.WorkflowItems.Tasks.AddNew();
			Dummy.RelatedDummyWithTasks2.WorkflowItems.Tasks.AddNew();
			Factory.Save();

			CollectionIncludingRelated.Load();

			AssertEquals("1 hit for main collection, and 1 for all related, 1 each for looking for line triggers on the parent job", 4, Factory.GetTableHitCount(((ITableSchema)ProcessTasksSchema.Instance).TableName));
		}

		public void TestLoadAndRebuildRaiseOnlyOneEvent()
		{
			var newFactory = Factory.CreateNewFactory();
			var newDummy = newFactory.NewWithValidTestData<DummyWithWorkflow>();
			GetNewCollectionViewIncludingRelated(newDummy.WorkflowItems).AddNew();
			newFactory.Save();

			var dummy = Factory.Load<DummyWithWorkflow>(newDummy.PK);
			var collectionIncludingRelated = GetNewCollectionViewIncludingRelated(dummy.WorkflowItems);

			int eventsCounter = 0;
			System.ComponentModel.ListChangedEventHandler collectionChangedHandler = (o, e) => { eventsCounter++; };
			((System.ComponentModel.IBindingList)CollectionIncludingRelated).ListChanged += collectionChangedHandler;
			try
			{
				AssertEquals("It's immediately loaded since the base collection was loaded.", 1, collectionIncludingRelated.Count);
				eventsCounter = 0;
				collectionIncludingRelated.Rebuild();
				AssertEquals("After rebuilding", 1, collectionIncludingRelated.Count);
				AssertEquals("This list hasn't changed... Why would you expect more events?", 0, eventsCounter);
			}
			finally
			{
				((System.ComponentModel.IBindingList)collectionIncludingRelated).ListChanged -= collectionChangedHandler;
			}
		}

		#region Remove / Delete

		public void TestCannotDeleteRelatedItemFromCollection()
		{
			Dummy.InitRelatedDummyWithTasks();
			ProcessTask item = CollectionNotIncludingRelated.AddNew();
			ProcessTask relatedItem = GetNewCollectionViewNotIncludingRelated(Dummy.RelatedDummyWithTasks.WorkflowItems).AddNew();

			try
			{
				CollectionIncludingRelated.RemoveAndDelete(relatedItem);
				Fail("Expected a " + nameof(CannotDeleteException) + " so that related items can't be deleted from the grid");
			}
			catch (CannotDeleteException ex)
			{
				AssertEquals("You cannot delete a related item. Open 'Dummy Business Object Default' and navigate to the 'Tracking' tab to delete this item.", ex.Message);
			}
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestRemoveAndDeleteAll_ThrowsNotSupportedException()
		{
			CollectionIncludingRelated.RemoveAndDeleteAll();
		}

		#endregion

		#region Synchronizing with underlying collection

		public void TestAddNewAndRemove_FromViewUpdatesUnderlyingCollection()
		{
			ProcessTask item = CollectionIncludingRelated.AddNew();
			Dummy.InitRelatedDummyWithTasks();
			ProcessTask relatedItem = GetNewCollectionViewNotIncludingRelated(Dummy.RelatedDummyWithTasks.WorkflowItems).AddNew();
			AssertEquals("1 ProcessTask in the wrapped ProcessTaskCollection initially", 1, Dummy.WorkflowItems.Count);

			ProcessTask addedItem = CollectionIncludingRelated.AddNew();
			AssertEquals("Item also added to underlying collection without a Load() required", 2, Dummy.WorkflowItems.Count);
			CollectionIncludingRelated.Remove(item);
			AssertEquals("Item removed to underlying collection without a Load() required", 1, Dummy.WorkflowItems.Count);
		}

		#endregion

		#region CollectionIncludingRelated / CollectionNotIncludingRelated

		protected WorkflowItemCollectionIncludingRelatedView CollectionIncludingRelated
		{
			get { return (WorkflowItemCollectionIncludingRelatedView)Collection; }
		}

		protected WorkflowItemCollectionView CollectionNotIncludingRelated
		{
			get
			{
				if (collectionNotIncludingRelated == null)
				{
					collectionNotIncludingRelated = GetNewCollectionViewNotIncludingRelated(Dummy.WorkflowItems);
				}
				return collectionNotIncludingRelated;
			}
		}
		WorkflowItemCollectionView collectionNotIncludingRelated;

		protected sealed override WorkflowItemCollectionView GetNewCollectionView(ProcessTaskCollection collection)
		{
			return GetNewCollectionViewIncludingRelated(collection);
		}

		protected abstract WorkflowItemCollectionView GetNewCollectionViewNotIncludingRelated(ProcessTaskCollection collection);
		protected abstract WorkflowItemCollectionView GetNewCollectionViewIncludingRelated(ProcessTaskCollection collection);

		#endregion
	}
}
