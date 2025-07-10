using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business.Testing
{
	class IPackableItemParentExtensionsTest : PackingTestCaseWithFactory
	{
		#region TestGetPackedQty

		public void TestGetPackedQty()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew();
			AssertExceptionThrown<ArgumentNullException>(() => ((IPackableItemParent)null).GetPackedQty(package));
			AssertExceptionThrown<ArgumentNullException>(() => Data.DummyLine1.GetPackedQty((PkgPackage)null));
			AssertExceptionThrown<ArgumentNullException>(() => Data.DummyLine1.GetPackedQty((PkgPackage[])null));
			AssertExceptionThrown<ArgumentNullException>(() => Data.DummyLine1.GetPackedQty((BusinessObjectFactory)null));

			var otherPackage = Data.PackageJob.Packages.AddNew("BOX");
			AssertEquals("Nothing should be packed.", 0m, Data.DummyLine1.GetPackedQty(package));
			AssertEquals("Nothing should be packed.", 0m, Data.DummyLine1.GetPackedQty(otherPackage));
			AssertEquals("Nothing should be packed.", 0m, Data.DummyLine1.GetPackedQty(Data.PackageJob.GetAllPackagesOnJob()));
			AssertEquals("Nothing should be packed.", 0m, Data.DummyLine1.GetPackedQty(Factory));

			package.Pack(Data.DummyLine1, 50m);
			AssertEquals("50 Units should be packed.", 50m, Data.DummyLine1.GetPackedQty(package));
			AssertEquals("Nothing should be packed.", 0m, Data.DummyLine1.GetPackedQty(otherPackage));
			AssertEquals("50 Units should be packed.", 50m, Data.DummyLine1.GetPackedQty(Data.PackageJob.GetAllPackagesOnJob()));
			AssertEquals("50 Units should be packed.", 50m, Data.DummyLine1.GetPackedQty(Factory));
			AssertEquals("Should find no Packed Qty when no Packages are passed in.", 0m, Data.DummyLine1.GetPackedQty(Array.Empty<PkgPackage>()));

			otherPackage.Pack(Data.DummyLine1, 30m);
			AssertEquals("50 Units should be packed in first package.", 50m, Data.DummyLine1.GetPackedQty(package));
			AssertEquals("30 Units should be packed in second package.", 30m, Data.DummyLine1.GetPackedQty(otherPackage));
			AssertEquals("80 Units in total should be packed.", 80m, Data.DummyLine1.GetPackedQty(Data.PackageJob.GetAllPackagesOnJob()));
			AssertEquals("80 Units in total should be packed.", 80m, Data.DummyLine1.GetPackedQty(Factory));
		}

		public void TestGetPackedQty_NoDBHitsForPackableItemsNotInDB()
		{
			Data.CreatePackingData();

			AssertEquals("0 Units should be packed.", 0m, Data.DummyLine1.GetPackedQty(Factory));
			AssertEquals("Should have no DB Hits for Divots.", 0, Factory.TableSelects.Count(t => t.TableName == PkgPackageItemDivotSchema.Constants.TableName));

			var package = Data.PackageJob.Packages.AddNew();
			package.Pack(Data.DummyLine1, 50m);
			AssertEquals("50 Units should be packed.", 50m, Data.DummyLine1.GetPackedQty(Factory));
			AssertEquals("Should have no DB Hits for Divots.", 0, Factory.TableSelects.Count(t => t.TableName == PkgPackageItemDivotSchema.Constants.TableName));
		}

		#endregion

		#region TestGetPackedDivots

		public void TestGetPackedDivots()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew();
			AssertExceptionThrown<ArgumentNullException>(() => ((IPackableItemParent)null).GetPackedDivots(package));
			AssertExceptionThrown<ArgumentNullException>(() => Data.DummyLine1.GetPackedDivots((PkgPackage)null));
			AssertExceptionThrown<ArgumentNullException>(() => Data.DummyLine1.GetPackedDivots((PkgPackage[])null));

			var otherPackage = Data.PackageJob.Packages.AddNew("BOX");
			var divots1 = Data.DummyLine1.GetPackedDivots(package);
			AssertEquals("No Packed Divots.", 0, divots1.Length);

			var divots2 = Data.DummyLine1.GetPackedDivots(Data.PackageJob.GetAllPackagesOnJob());
			AssertEquals("No Packed Divots.", 0, divots2.Length);

			package.Pack(Data.DummyLine1, 50m);
			var divots3 = Data.DummyLine1.GetPackedDivots(package);
			AssertEquals("Should have 1 Item Divot.", 1, divots3.Length);

			var itemDivot1 = divots3.Single();
			AssertEquals("Correct Item Divot is returned.", true, Data.DummyLine1.PackableItems.Any(i => itemDivot1.PackedItem == i));

			var divots4 = Data.DummyLine1.GetPackedDivots(Data.PackageJob.GetAllPackagesOnJob());
			AssertEquals("Should have 1 Item Divot.", 1, divots4.Length);
			AssertEquals("Should have Correct Item Divot.", itemDivot1, divots4.Single());

			var divots5 = Data.DummyLine1.GetPackedDivots(Array.Empty<PkgPackage>());
			AssertEquals("Should find no Divots as no Packages were passed in.", 0, divots5.Length);

			otherPackage.Pack(Data.DummyLine1, 30m);
			var divots6 = Data.DummyLine1.GetPackedDivots(package);
			AssertEquals("Only 1 Item Divot for Package 1.", 1, divots6.Length);
			AssertEquals("Should have Correct Item Divot.", itemDivot1, divots6.Single());

			var divots7 = Data.DummyLine1.GetPackedDivots(otherPackage);
			AssertEquals("Only 1 Item Divot for Package 1.", 1, divots7.Length);
			AssertNotEquals("Should have Correct Item Divot.", itemDivot1, divots7.Single());

			var itemDivot2 = divots7.Single();
			var divots8 = Data.DummyLine1.GetPackedDivots(Data.PackageJob.GetAllPackagesOnJob());
			AssertEquals("Should return both Item Divot for Package 1 & 2.", 2, divots8.Length);
			AssertContainsExactElementsInAnyOrder(new[] { itemDivot1, itemDivot2 }, divots8);
		}

		#endregion
	}
}
