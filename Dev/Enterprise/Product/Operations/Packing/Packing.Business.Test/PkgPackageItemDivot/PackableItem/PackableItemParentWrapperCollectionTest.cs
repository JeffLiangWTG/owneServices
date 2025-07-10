using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PackableItemParentWrapperCollection))]
	public class PackableItemParentWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PackableItemParentWrapperCollection>
	{
		#region TestCollectionIsRefreshedOnPackableItemParentsCountChanged

		public void TestCollectionIsRefreshedOnPackableItemParentsCountChanged()
		{
			Data.CreatePackingData();

			var packableItemParents = new PackableItemParentWrapperCollection(Data.Dummy);
			AssertEquals("Construction should be light-weight -- it should not touch the PackageJob.PackableItemParents collection.", 0, packableItemParents.Count);

			packableItemParents.WrapPackableItemParents();
			AssertEquals(3, packableItemParents.Count);

			var newLine = Data.Dummy.Lines.AddNew();
			AssertEquals("The wrapped collection's CountChanged should have been hooked.", 4, packableItemParents.Count);

			newLine.Delete();
			AssertEquals(3, packableItemParents.Count);
		}

		#endregion

		#region TestCollectionProperlyRemovesDeletedParentsEvenIfTheyHaveTheSameKeyAsCurrentParent

		public void TestCollectionProperlyRemovesDeletedParentsEvenIfTheyHaveTheSameKeyAsCurrentParent()
		{
			Data.CreatePackingData();

			var packableItemParents = new PackableItemParentWrapperCollection(Data.Dummy);
			AssertEquals("Construction should be light-weight -- it should not touch the PackageJob.PackableItemParents collection.", 0, packableItemParents.Count);

			packableItemParents.WrapPackableItemParents();
			AssertEquals(3, packableItemParents.Count);

			var randomPackableItemParent = (DummyPackableItemParent)Data.Dummy.Lines.First();
			var newLine = (DummyPackableItemParent)Data.Dummy.Lines.AddNew();
			((DummyPackableItem)newLine.PackableItems.Single()).Key = randomPackableItemParent.PackableItems.Single().Key;

			randomPackableItemParent.Delete();
			AssertEquals(3, packableItemParents.Count);
			AssertCollectionContains(newLine, packableItemParents.Cast<PackableItemParentWrapper>().Select(w => w.PackableItemParent));
			AssertCollectionNotContains(randomPackableItemParent, packableItemParents.Cast<PackableItemParentWrapper>().Select(w => w.PackableItemParent));

			foreach (var dummyPackableItemParent in Data.Dummy.Lines)
			{
				foreach (var packableItem in dummyPackableItemParent.PackableItems)
				{
					AssertEquals(dummyPackableItemParent, packableItemParents.GetPackableItemParentFromKey(packableItem.Key));
				}
			}
		}

		#endregion

		#region TestAllowNew

		public void TestAllowNew()
		{
			AssertEquals(false, Collection.AllowNew);
		}

		#endregion

		#region TestCannotCreateNewObject

		[ExpectException(typeof(NotSupportedException))]
		public void TestCannotCreateNewObject()
		{
			Collection.AddNew();
		}

		#endregion

		#region TestFindByPackableItemParent

		public void TestFindByPackableItemParent()
		{
			Data.CreatePackingData();

			var collection = new PackableItemParentWrapperCollection(Factory, new PackableItemParentWrapper[] { Data.DummyLine1Wrapper, Data.DummyLine2Wrapper });
			var wrapper1 = collection.FindByPackableItemParent(data.DummyLine1);
			var wrapper2 = collection.FindByPackableItemParent(data.DummyLine2);
			var wrapper3 = collection.FindByPackableItemParent(data.DummyLine3);

			AssertEquals(Data.DummyLine1, wrapper1.PackableItemParent);
			AssertEquals(Data.DummyLine2, wrapper2.PackableItemParent);
			AssertNull(wrapper3);
		}

		#endregion

		#region TestGetPackableItemParentFromKey

		public void TestGetPackableItemParentFromKey()
		{
			Data.CreatePackingData();

			var packableItemParents = Data.PackageJob.PackableItemParents;
			AssertExceptionThrown<ArgumentNullException>(() => packableItemParents.GetPackableItemParentFromKey(null));
			AssertNull(packableItemParents.GetPackableItemParentFromKey(EmptyKey.Instance));
			AssertEquals(Data.DummyLine1, packableItemParents.GetPackableItemParentFromKey(Data.DummyPackableItemOnLine1.Key));
		}

		public void TestGetPackableItemParentFromKey_RebuildsCollectionIfInvalidated()
		{
			Data.CreatePackingData();

			var packableItemParents = new PackableItemParentWrapperCollection(Data.Dummy);
			AssertEquals("Construction should be light-weight -- it should not touch the PackageJob.PackableItemParents collection.", 0, packableItemParents.Count);

			packableItemParents.WrapPackableItemParents();
			AssertEquals(3, packableItemParents.Count);

			var randomPackableItemParent = (DummyPackableItemParent)Data.Dummy.Lines.First();
			var key = randomPackableItemParent.PackableItems.Single().Key;
			randomPackableItemParent.Delete();

			AssertNull(packableItemParents.GetPackableItemParentFromKey(key));
			var anotherPackableItemParent = Data.Dummy.Lines.First();
			var parentFromCollection = packableItemParents.GetPackableItemParentFromKey(anotherPackableItemParent.PackableItems.Single().Key);
			AssertEquals(anotherPackableItemParent, parentFromCollection);
			AssertEquals(2, packableItemParents.Count);
		}

		#endregion

		#region TestWrapPackableItemParents

		public void TestWrapPackableItemParents()
		{
			Data.CreatePackingData();

			var packableItemParents = new PackableItemParentWrapperCollection(Data.Dummy);
			AssertEquals("Collection is empty.", 0, packableItemParents.Count);

			var newDummyLine = Data.Dummy.Lines.AddNew();
			newDummyLine.Description = "Splitter";
			AssertEquals("Collection is empty.", 0, packableItemParents.Count);

			packableItemParents.WrapPackableItemParents();
			AssertContainsExactElementsInAnyOrder(new[] { Data.DummyLine1, Data.DummyLine2, Data.DummyLine3, newDummyLine }, packableItemParents.Cast<PackableItemParentWrapper>().Select(w => w.PackableItemParent));

			var wrapper1 = packableItemParents.Cast<PackableItemParentWrapper>().Single(w => w.PackableItemParent == Data.DummyLine1);
			var wrapper2 = packableItemParents.Cast<PackableItemParentWrapper>().Single(w => w.PackableItemParent == Data.DummyLine2);
			var wrapper3 = packableItemParents.Cast<PackableItemParentWrapper>().Single(w => w.PackableItemParent == Data.DummyLine3);
			var wrapper4 = packableItemParents.Cast<PackableItemParentWrapper>().Single(w => w.PackableItemParent == newDummyLine);

			var key1 = Data.DummyPackableItemOnLine1.Key;
			var key2 = newDummyLine.PackableItems.Single().Key;
			AssertEquals("Should be able to find PackableItem Parent by Key.", Data.DummyLine1, packableItemParents.GetPackableItemParentFromKey(key1));
			AssertEquals("Should be able to find PackableItem Parent by Key.", newDummyLine, packableItemParents.GetPackableItemParentFromKey(key2));

			// should rebuild collection and retain any wrappers that are already wrapping existing PackableItemParents.
			Data.DummyLine1.Delete();
			AssertContainsExactElementsInAnyOrder(new[] { wrapper2, wrapper3, wrapper4 }, packableItemParents);
			AssertNull("Should be not able to find PackableItem Parent.", packableItemParents.GetPackableItemParentFromKey(key1));
			AssertEquals("Should be able to find PackableItem Parent by Key.", newDummyLine, packableItemParents.GetPackableItemParentFromKey(key2));
		}

		public void TestWrapPackableItemParents_PackableItemParentWithQuantityButNoPackableItems()
		{
			var dummy = Factory.New<DummyWithPacking>();

			var dummyLine1 = dummy.Lines.AddNew();
			dummyLine1.Code = "P1";
			dummyLine1.Description = "TV";
			dummyLine1.DescriptionSupplement = "Size: 63in";
			dummyLine1.TotalQty = 100;
			dummyLine1.TotalQtyUQ = "UNT";
			dummyLine1.AutoPackQtyPerPackage = 4;
			dummyLine1.AutoPackPackageType = "PLT";

			var packableItemParent = (IPackableItemParent)dummyLine1;
			var packableItemParents = new PackableItemParentWrapperCollection(dummy);
			Assert("PackableItemParent quantity is > 0.", packableItemParent.TotalQty > 0);
			AssertEquals("Collection is empty.", 0, packableItemParents.Count);

			((DummyPackableItemParent)dummyLine1).RemoveItem((DummyPackableItem)dummyLine1.PackableItems.First());
			AssertNoExceptionThrown(() => packableItemParents.WrapPackableItemParents());
			Assert("PackableItemParent quantity is > 0.", packableItemParent.TotalQty > 0);
			AssertEquals("Collection is empty.", 0, packableItemParents.Count);
		}

		#endregion

		#region TestWrapPackableItemParents_ZeroQuantityWrapper

		public void TestWrapPackableItemParents_ZeroQuantityWrapper()
		{
			Data.CreatePackingData();

			var packableItemParents = new PackableItemParentWrapperCollection(Data.Dummy);
			AssertEquals("Collection is empty.", 0, packableItemParents.Count);

			Data.DummyLine1.TotalQty = 0m;
			Data.DummyLine2.TotalQty = -1m;
			packableItemParents.WrapPackableItemParents();
			AssertContainsExactElementsInAnyOrder(new[] { Data.DummyLine3 }, packableItemParents.Cast<PackableItemParentWrapper>().Select(w => w.PackableItemParent));
		}

		#endregion

		#region TestEnumerator<T>

		public void TestEnumerator()
		{
			Data.CreatePackingData();

			var collection = new PackableItemParentWrapperCollection(Factory, new PackableItemParentWrapper[] { Data.DummyLine1Wrapper, data.DummyLine2Wrapper });
			AssertNotNull(collection.FirstOrDefault<PackableItemParentWrapper>(wrapper => wrapper.PackableItemParent == Data.DummyLine1));
			AssertNotNull(collection.FirstOrDefault<PackableItemParentWrapper>(wrapper => wrapper.PackableItemParent == Data.DummyLine2));
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);
			DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackableItemParent);
		}

		protected override PackableItemParentWrapperCollection GetCollectionToTest()
		{
			return new PackableItemParentWrapperCollection(Factory, Array.Empty<PackableItemParentWrapper>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			Data.CreatePackingData();
			return new PackableItemParentWrapper(Factory, Data.DummyLine1);
		}

		TestDataForPacking Data
		{
			get { return data ?? (data = new TestDataForPacking(Factory)); }
		}

		TestDataForPacking data;

		#endregion
	}
}
