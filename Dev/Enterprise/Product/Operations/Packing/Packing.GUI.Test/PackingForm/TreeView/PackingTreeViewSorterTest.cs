using System.Collections;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;

namespace Enterprise.Packing.GUI.Testing
{
	class PackingTreeViewSorterTest : PackingTestCaseWithFactory
	{
		public void TestCompare()
		{
			var data = new TestDataForPacking(Factory);
			data.CreatePackingData();

			var dummyLine4 = data.Dummy.Lines.AddNew();
			dummyLine4.Code = "P1";
			dummyLine4.Description = "TV";
			dummyLine4.DescriptionSupplement = "Size: 10in";

			// create packages / packed items
			var package_CNT1 = data.PackageJob.Packages.AddNew("CNT");

			var package_CNT2 = data.PackageJob.Packages.AddNew("CNT");
			package_CNT2.KP_PackageQty = 2;
			package_CNT2.KP_PackageID = "C1";

			var package_CNT3 = data.PackageJob.Packages.AddNew("CNT");
			package_CNT3.KP_PackageQty = 2;
			package_CNT3.KP_PackageID = "C2";

			var package_PLT = data.PackageJob.Packages.AddNew("PLT");
			var packedItem1 = package_CNT1.Pack_ForTesting(data.DummyLine1, 5m);
			var packedItem2 = package_CNT1.Pack_ForTesting(data.DummyLine2, 5m);
			var packedItem3 = package_CNT1.Pack_ForTesting(dummyLine4, 5m);

			var packageJob1 = Factory.NewWithValidTestData<PkgPackageJob>();
			var packageJob2 = Factory.NewWithValidTestData<PkgPackageJob>();

			// create nodes
			using (var packageNode_CNT1 = new PackingTreeNode(package_CNT1))
			using (var packageNode_CNT2 = new PackingTreeNode(package_CNT2))
			using (var packageNode_CNT3 = new PackingTreeNode(package_CNT3))
			using (var packageNode_PLT = new PackingTreeNode(package_PLT))
			using (var itemNode1 = new PackingTreeNode(packedItem1))
			using (var itemNode2 = new PackingTreeNode(packedItem2))
			using (var itemNode2_Dupe = new PackingTreeNode(packedItem2))
			using (var itemNode3 = new PackingTreeNode(packedItem3))
			using (var packageJobNode1 = new PackingTreeNode(packageJob1))
			using (var packageJobNode2 = new PackingTreeNode(packageJob2))
			{
				// compare
				var comparer = new PackingTreeNodeSorter();
				AssertPersistentPropertiesHitCount("Properties get hit multiple times with no caching.", 46, () =>
				{
					AssertEquals("1x Container should come before 1x Pallet.", -1, comparer.Compare(packageNode_CNT1, packageNode_PLT));
					AssertEquals("2x Containers should come before 1x Pallet.", -1, comparer.Compare(packageNode_CNT2, packageNode_PLT));
					AssertEquals("Packages are identical, should Secondary sort by PackageID.", -1, comparer.Compare(packageNode_CNT2, packageNode_CNT3));
					AssertEquals("Packages are identical, should Secondary sort by PackageID, empty package last.", -1, comparer.Compare(packageNode_CNT1, packageNode_CNT2));
					AssertEquals("Packages should come before packed items.", -1, comparer.Compare(packageNode_PLT, itemNode1));
					AssertEquals("'P1 - TV - Size: 63in' should come before 'P2 - Amp'", -1, comparer.Compare(itemNode1, itemNode2));
					AssertEquals("'P1 - TV - Size: 10in' should come before 'P1 - TV - Size: 63in'", 1, comparer.Compare(itemNode1, itemNode3));
					AssertEquals(0, comparer.Compare(itemNode2, itemNode2_Dupe));
					AssertNoExceptionThrown("Package Job sort order doesn't matter, as long as it doesn't throw an exception.", () => { comparer.Compare(packageJobNode1, packageJobNode2); });

					AssertEquals("IComparer.Compare() should proxy through to IComparer<PackingNode>.Compare().", -1,
						((IComparer)comparer).Compare(packageNode_PLT, itemNode1));
				});

				using (comparer.CacheSortingProperties())
				{
					AssertPersistentPropertiesHitCount("Each property per package/packed item only gets hit once with caching.", 16, () =>
					{
						AssertEquals("1x Container should come before 1x Pallet.", -1, comparer.Compare(packageNode_CNT1, packageNode_PLT));
						AssertEquals("2x Containers should come before 1x Pallet.", -1, comparer.Compare(packageNode_CNT2, packageNode_PLT));
						AssertEquals("Packages are identical, should Secondary sort by PackageID.", -1, comparer.Compare(packageNode_CNT2, packageNode_CNT3));
						AssertEquals("Packages are identical, should Secondary sort by PackageID, empty package last.", -1, comparer.Compare(packageNode_CNT1, packageNode_CNT2));
						AssertEquals("Packages should come before packed items.", -1, comparer.Compare(packageNode_PLT, itemNode1));
						AssertEquals("'P1 - TV - Size: 63in' should come before 'P2 - Amp'", -1, comparer.Compare(itemNode1, itemNode2));
						AssertEquals("'P1 - TV - Size: 10in' should come before 'P1 - TV - Size: 63in'", 1, comparer.Compare(itemNode1, itemNode3));
						AssertEquals(0, comparer.Compare(itemNode2, itemNode2_Dupe));
						AssertNoExceptionThrown("Package Job sort order doesn't matter, as long as it doesn't throw an exception.", () => { comparer.Compare(packageJobNode1, packageJobNode2); });

						AssertEquals("IComparer.Compare() should proxy through to IComparer<PackingNode>.Compare().", -1,
							((IComparer)comparer).Compare(packageNode_PLT, itemNode1));
					});
				}
			}
		}
	}
}
