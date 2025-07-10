using System.Linq;

namespace Enterprise.Packing.Business.Testing
{
	class PkgPackageItemDivotsWrapperExtensionsTest : PackingTestCaseWithFactory
	{
		#region TestIsOnePackedItem

		public void TestIsOnePackedItem()
		{
			Data.CreatePackingData();

			var package1 = Data.PackageJob.Packages.AddNew();
			var package2 = Data.PackageJob.Packages.AddNew();
			var packedItem1 = package1.Pack(Data.DummyLine1, 1m).Single();
			var packedItem2 = package1.Pack(Data.DummyLine1, 2m).Single();

			AssertEquals("Only 1 Group, IsOnePackedItem() should be true.", true, new[] { packedItem1, packedItem2 }.IsOnePackedItem());

			var packedItem3 = package2.Pack(Data.DummyLine1, 1m).Single();
			AssertEquals("More than 1 Group, IsOnePackedItem() should be false.", false, new[] { packedItem1, packedItem2, packedItem3 }.IsOnePackedItem());
		}

		#endregion

		#region TestGroupedPackedItems

		public void TestGroupedPackedItems()
		{
			Data.CreatePackingData();

			var package1 = Data.PackageJob.Packages.AddNew();
			var package2 = Data.PackageJob.Packages.AddNew();
			var packedItem1 = package1.Pack(Data.DummyLine1, 1m).Single();
			var packedItem2 = package1.Pack(Data.DummyLine1, 2m).Single();
			var packedItem3 = package1.Pack(Data.DummyLine2, 2m).Single();
			var packedItem4 = package2.Pack(Data.DummyLine1, 1m).Single();
			AssertEquals("Should be same as wrappers are grouped together now.", packedItem1, packedItem2);

			var groupedItems = new[] { packedItem1, packedItem3, packedItem4 }.GroupedPackedItems();
			AssertEquals("Should be three Groups.", 3, groupedItems.Count);
			AssertContainsExactElementsInAnyOrder(new[] { packedItem1 }, groupedItems[new GroupedPackedItemsKey(packedItem1.Key, package1.PK)]);
			AssertContainsExactElementsInAnyOrder(new[] { packedItem3 }, groupedItems[new GroupedPackedItemsKey(packedItem3.Key, package1.PK)]);
			AssertContainsExactElementsInAnyOrder(new[] { packedItem4 }, groupedItems[new GroupedPackedItemsKey(packedItem4.Key, package2.PK)]);
		}

		#endregion
	}
}
