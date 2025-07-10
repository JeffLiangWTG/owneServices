using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Rating.Business.Testing
{
	public abstract class BizObjectCollectionAddDeleteTestCase : BusinessObjectCollectionTestCase
	{
		protected abstract BusinessObjectCollection GetCollection();

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return GetCollection();
		}

		public override void TestAdd()
		{
			var testCollection = GetCollectionToTest();

			int initialCount = testCollection.Count;

			var item1 = testCollection.AddNew();
			var item2 = testCollection.AddNew();

			AssertEquals("Precondition : Collection count", initialCount + 2, testCollection.Count);

			testCollection.RemoveAndDelete(item1);

			AssertEquals("Collection count", initialCount + 1, testCollection.Count);
			Assert("Contains element 2", testCollection.Contains(item2));
			Assert("Doesn't contain element 1", !testCollection.Contains(item1));
			Assert("Element 1 was removed, and deleted", item1.IsDeleted);
		}

		public override void TestDelete()
		{
			var testCollection = GetCollectionToTest();

			var item1NotToBeDeleted = testCollection.AddNew();
			var item2ToBeDeleted = testCollection.AddNew();

			AssertEquals("Precondition : Collection count", 2, testCollection.Count);

			testCollection.RemoveAndDelete(item2ToBeDeleted);

			AssertEquals("Collection count", 1, testCollection.Count);

			Assert("Doesn't contain element 1", !testCollection.Contains(item2ToBeDeleted));
			Assert("Element 1 was removed, and deleted", item2ToBeDeleted.IsDeleted);
		}

		public override void TestSuspendCountChanged()
		{
			var testCollection = GetCollectionToTest();

			var item1 = testCollection.AddNew();
			var item2 = testCollection.AddNew();

			Action<IEnumerable<CollectionCountChangedEventArgs>> suspendedEventsProcessor = (suspendedEventArgs) =>
			{
				AssertEquals("All event args have been passed to processor", 2, suspendedEventArgs.Count());
				AssertEquals(true, suspendedEventArgs.First().ItemRemoved);
				AssertEquals(true, suspendedEventArgs.Skip(1).First().ItemAdded);
			};

			int countChangedEventHandlerCalled = 0;
			testCollection.CountChanged += (s, e) => { countChangedEventHandlerCalled++; };

			using (testCollection.SuspendCountChanged(suspendedEventsProcessor))
			{
				testCollection.Remove(item2);
				testCollection.AddNew();
			}

			AssertEquals("CountChanged event was suspended", 0, countChangedEventHandlerCalled);

			AssertNoExceptionThrown("Null argument is allowed", () =>
			{
				using (testCollection.SuspendCountChanged(null))
				{
					testCollection.AddNew();
					testCollection.AddNew();
				}
			});

			AssertEquals("CountChanged event was suspended", 0, countChangedEventHandlerCalled);

			testCollection.Remove(item1);
			AssertEquals("CountChanged event was not suspended", 1, countChangedEventHandlerCalled);
		}
	}
}
