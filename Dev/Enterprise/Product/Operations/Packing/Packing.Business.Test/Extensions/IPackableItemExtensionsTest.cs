using System;
using System.Linq;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business.Testing
{
	class IPackableItemExtensionsTest : PackingTestCaseWithFactory
	{
		#region TestIsUnpacked

		public void TestIsUnpacked()
		{
			Data.CreatePackingData();

			AssertExceptionThrown<ArgumentNullException>(() => ((IPackableItem)null).IsUnpacked(Factory));
			AssertExceptionThrown<ArgumentNullException>(() => Data.DummyPackableItemOnLine1.IsUnpacked(null));
			AssertEquals(true, Data.DummyPackableItemOnLine1.IsUnpacked(Factory));

			var divot = Factory.New<PkgPackageItemDivot>();
			divot.KI_ParentID = Data.DummyPackableItemOnLine1.PK;
			divot.KI_ParentTableCode = Data.DummyPackableItemOnLine1.TablePrefix;
			AssertEquals(false, Data.DummyPackableItemOnLine1.IsUnpacked(Factory));
		}

		public void TestIsUnpacked_NoDBHitsForItemsNotInDB()
		{
			Data.CreatePackingData();
			AssertEquals(true, Data.DummyPackableItemOnLine1.IsUnpacked(Factory));
			AssertEquals("Should have no DB Hits for Divots.", 0, Factory.TableSelects.Count(t => t.TableName == PkgPackageItemDivotSchema.Constants.TableName));

			var divot = Factory.New<PkgPackageItemDivot>();
			divot.KI_ParentID = Data.DummyPackableItemOnLine1.PK;
			divot.KI_ParentTableCode = Data.DummyPackableItemOnLine1.TablePrefix;
			AssertEquals(false, Data.DummyPackableItemOnLine1.IsUnpacked(Factory));
			AssertEquals("Should have no DB Hits for Divots.", 0, Factory.TableSelects.Count(t => t.TableName == PkgPackageItemDivotSchema.Constants.TableName));
		}

		public void TestIsUnpacked_MultipleDivots()
		{
			var package = Factory.New<PkgPackage>();
			var packableItem = Helper.CreatePackableItem(10m);
			AssertEquals("Precondition", true, packableItem.IsUnpacked(Factory));

			var divot1 = Helper.CreatePackageDivot(package, packableItem);
			AssertEquals("When a divot exist for a packable item, then it is already packed.", false, packableItem.IsUnpacked(Factory));

			// should not happen, but can happen in some rare cases like due to concurency
			var divot2 = Helper.CreatePackageDivot(package, packableItem);
			AssertEquals("When a least 1 divot exist for a packable item, then it is already packed.", false, packableItem.IsUnpacked(Factory));
		}

		#endregion

		#region TestGetPackedQty

		public void TestGetPackedQty()
		{
			Data.CreatePackingData();
			var divot = Factory.New<PkgPackageItemDivot>();
			divot.KI_ParentID = Data.DummyPackableItemOnLine1.PK;
			divot.KI_ParentTableCode = Data.DummyPackableItemOnLine1.TablePrefix;
			divot.KI_PackedQty = 3m;
			AssertEquals(3m, Data.DummyPackableItemOnLine1.GetPackedQty(Factory));
		}

		public void TestGetPackedQty_NoDBHitsForItemsNotInDB()
		{
			Data.CreatePackingData();

			AssertEquals(0m, Data.DummyPackableItemOnLine1.GetPackedQty(Factory));
			AssertEquals("Should have no DB Hits for Divots.", 0, Factory.TableSelects.Count(t => t.TableName == PkgPackageItemDivotSchema.Constants.TableName));

			var divot = Factory.New<PkgPackageItemDivot>();
			divot.KI_ParentID = Data.DummyPackableItemOnLine1.PK;
			divot.KI_ParentTableCode = Data.DummyPackableItemOnLine1.TablePrefix;
			divot.KI_PackedQty = 3m;
			AssertEquals(3m, Data.DummyPackableItemOnLine1.GetPackedQty(Factory));
			AssertEquals("Should have no DB Hits for Divots.", 0, Factory.TableSelects.Count(t => t.TableName == PkgPackageItemDivotSchema.Constants.TableName));
		}

		public void TestGetPackageQty_MultipleDivots()
		{
			var package = Factory.New<PkgPackage>();
			var packableItem = Helper.CreatePackableItem(10m);
			AssertEquals("Precondition", 0m, packableItem.GetPackedQty(Factory));

			var divot1 = Helper.CreatePackageDivot(package, packableItem);
			AssertEquals("When a divot exist for a packable item, then it is already packed.", 10m, packableItem.GetPackedQty(Factory));

			// should not happen, but can happen in some rare cases like due to concurency
			var divot2 = Helper.CreatePackageDivot(package, packableItem);
			AssertEquals("When a least multiple divots exist for a packable item, then it is over packed.", 20m, packableItem.GetPackedQty(Factory));
		}

		#endregion
	}
}
