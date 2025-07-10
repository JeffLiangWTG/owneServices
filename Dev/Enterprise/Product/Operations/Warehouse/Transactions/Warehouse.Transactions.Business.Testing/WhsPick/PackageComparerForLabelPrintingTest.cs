using System;
using System.Linq;
using System.Reflection;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class PackageComparerForLabelPrintingTest : WhsTestCaseWithFactory
	{
		#region TestPackageSortingForPrinting

		public void TestCompareAndGetDelimeterTypeBetweenPackages()
		{
			TestCompareAndGetDelimeterTypeBetweenPackagesCore(isPrintingWithoutSeparatorLabels: false);
		}

		public void TestCompareAndGetDelimeterTypeBetweenPackagesWithoutSeparatorlabels()
		{
			TestCompareAndGetDelimeterTypeBetweenPackagesCore(isPrintingWithoutSeparatorLabels: true);
		}

		void TestCompareAndGetDelimeterTypeBetweenPackagesCore(bool isPrintingWithoutSeparatorLabels)
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var rowB = Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 1);
			var rowC = Helper.CreateRowAndGenerateLocations(data.Whs1, "C", 1);
			var rowD = Helper.CreateRowAndGenerateLocations(data.Whs1, "D", 1);
			Factory.Save();

			#region locations / areas setup

			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");
			var loc3 = data.Whs1.FindLocation("A-3");
			var loc4 = data.Whs1.FindLocation("A-4");
			var loc5 = data.Whs1.FindLocation("A-5");
			var loc6 = data.Whs1.FindLocation("B");
			var loc7 = data.Whs1.FindLocation("C");
			var loc8 = data.Whs1.FindLocation("D");

			var areaA = Helper.CreateArea(data.Whs1, "Area - A");
			var areaB = Helper.CreateArea(data.Whs1, "Area - B");
			loc1.WLV_WA_PickingArea = areaA.PK;
			loc2.WLV_WA_PickingArea = areaA.PK;
			loc3.WLV_WA_PickingArea = areaA.PK;
			loc6.WLV_WA_PickingArea = areaA.PK;
			loc7.WLV_WA_PickingArea = areaA.PK;
			loc8.WLV_WA_PickingArea = areaA.PK;
			loc4.WLV_WA_PickingArea = areaB.PK;
			loc5.WLV_WA_PickingArea = areaB.PK;

			Factory.Save();
			loc2.WLV_PickPathSequence = 1;
			loc1.WLV_PickPathSequence = 2;
			loc3.WLV_PickPathSequence = 3;

			loc5.WLV_PickPathSequence = 4;
			loc4.WLV_PickPathSequence = 5;

			loc1.Row.WR_PickPathSequence = 1;
			rowB.WR_PickPathSequence = 2;
			rowC.WR_PickPathSequence = 0;
			rowD.WR_PickPathSequence = 0;

			loc6.WLV_PickPathSequence = 1;
			loc7.WLV_PickPathSequence = 1;
			loc8.WLV_PickPathSequence = 1;

			#endregion

			// so the area and location setup looks like this (increasing sequence from left to right):
			// Area - A: loc2 -> loc1 -> loc3
			// Area - B: loc5 -> loc4
			// looks confusing, but that is the way to ensure that in-memory order of objects has no effect on real sorting

			Helper.CreateProductUnit(data.Part1, "CAS", 5);
			Helper.CreateProductUnit(data.Part1, "PLT", 20);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("UNT")).F3_UOMType = UOMPackTypesList.Codes.SplitCase;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType = UOMPackTypesList.Codes.Case;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType = UOMPackTypesList.Codes.Pallet;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var invAreaALoc1pallet = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, loc1);
			var invAreaALoc2pallet = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, loc2);
			var invAreaALoc3pallet = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, loc3);
			var invAreaBLoc4pallet = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, loc4);
			var invAreaBLoc5pallet = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, loc5);
			var invAreaALoc6pallet = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, loc6);
			var invAreaALoc7pallet = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, loc7);
			var invAreaALoc8pallet = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20, loc8);

			var invAreaALoc1case = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5, loc1);
			var invAreaALoc2case = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5, loc2);
			var invAreaALoc3case = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5, loc3);
			var invAreaBLoc4case = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5, loc4);
			var invAreaBLoc5case = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5, loc5);
			var invAreaALoc6case = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5, loc6);
			var invAreaALoc7case = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5, loc7);
			var invAreaALoc8case = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5, loc8);

			var invAreaALoc1splitCase = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, loc1);
			var invAreaALoc2splitCase = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, loc2);
			var invAreaALoc3splitCase = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, loc3);
			var invAreaBLoc4splitCase = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, loc4);
			var invAreaBLoc5splitCase = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, loc5);
			var invAreaALoc6splitCase = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, loc6);
			var invAreaALoc7splitCase = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, loc7);
			var invAreaALoc8splitCase = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, loc8);

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 208);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			var allPickPackages = pick.OuterPackages;
			AssertEquals("There should be 16 label packages: 8 for pallets and 8 for cases.", 16, allPickPackages.Count);
			var packagePalletALoc1 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaALoc1pallet.CommittedPickLines.Single().PK);
			var packagePalletALoc2 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaALoc2pallet.CommittedPickLines.Single().PK);
			var packagePalletALoc3 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaALoc3pallet.CommittedPickLines.Single().PK);
			var packagePalletBLoc4 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaBLoc4pallet.CommittedPickLines.Single().PK);
			var packagePalletBLoc5 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaBLoc5pallet.CommittedPickLines.Single().PK);
			var packagePalletALoc6 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaALoc6pallet.CommittedPickLines.Single().PK);
			var packagePalletALoc7 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaALoc7pallet.CommittedPickLines.Single().PK);
			var packagePalletALoc8 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaALoc8pallet.CommittedPickLines.Single().PK);
			var packageCaseALoc1 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaALoc1case.CommittedPickLines.Single().PK);
			var packageCaseALoc2 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaALoc2case.CommittedPickLines.Single().PK);
			var packageCaseALoc3 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaALoc3case.CommittedPickLines.Single().PK);
			var packageCaseBLoc4 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaBLoc4case.CommittedPickLines.Single().PK);
			var packageCaseBLoc5 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaBLoc5case.CommittedPickLines.Single().PK);
			var packageCaseALoc6 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaALoc6case.CommittedPickLines.Single().PK);
			var packageCaseALoc7 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaALoc7case.CommittedPickLines.Single().PK);
			var packageCaseALoc8 = allPickPackages.Single(p => p.PackedItemDivots[0].KI_ParentID == invAreaALoc8case.CommittedPickLines.Single().PK);

			var packingHelper = new PackingTestHelper(Factory);
			// As I don't want to rely on cartonisation algorithm to create packages for test, I will create them manually
			var packAreaALoc1 = packingHelper.CreatePackage(order.PackageJob, 1, "UNT");
			packingHelper.CreatePackageDivot(packAreaALoc1, invAreaALoc1splitCase.CommittedPickLines.Single());

			var packAreaALoc2AreaBLoc4 = packingHelper.CreatePackage(order.PackageJob, 2, "UNT"); // this package with 2 picklines
			packingHelper.CreatePackageDivot(packAreaALoc2AreaBLoc4, invAreaBLoc4splitCase.CommittedPickLines.Single());
			packingHelper.CreatePackageDivot(packAreaALoc2AreaBLoc4, invAreaALoc2splitCase.CommittedPickLines.Single()); // this pickline has lower sort order and should be used for package sorting

			var packAreaALoc3 = packingHelper.CreatePackage(order.PackageJob, 1, "UNT");
			packingHelper.CreatePackageDivot(packAreaALoc3, invAreaALoc3splitCase.CommittedPickLines.Single());

			var packAreaBLoc5 = packingHelper.CreatePackage(order.PackageJob, 1, "UNT");
			packingHelper.CreatePackageDivot(packAreaBLoc5, invAreaBLoc5splitCase.CommittedPickLines.Single());

			var packAreaALoc6 = packingHelper.CreatePackage(order.PackageJob, 1, "UNT");
			packingHelper.CreatePackageDivot(packAreaALoc6, invAreaALoc6splitCase.CommittedPickLines.Single());

			var packAreaALoc7 = packingHelper.CreatePackage(order.PackageJob, 1, "UNT");
			packingHelper.CreatePackageDivot(packAreaALoc7, invAreaALoc7splitCase.CommittedPickLines.Single());

			var packAreaALoc8 = packingHelper.CreatePackage(order.PackageJob, 1, "UNT");
			packingHelper.CreatePackageDivot(packAreaALoc8, invAreaALoc8splitCase.CommittedPickLines.Single());

			Factory.Save();

			var comparer = new PackageComparerForLabelPrinting(pick);
			var sortedPackages = pick.OuterPackages.OrderBy(p => p, comparer);
			AssertEquals("Should be 23 packages: 8 for pallets, 8 for cases and 7 for split cases.", 23, sortedPackages.Count());

			using (isPrintingWithoutSeparatorLabels ? pick.EnablePrintingWithoutSeparatorLabels() : null)
			using (var enumerator = sortedPackages.GetEnumerator())
			{
				// first comes pallets for AreaA
				Assert(enumerator.MoveNext());
				var currentPackage = enumerator.Current;
				AssertEquals("Should be Pallet for Loc2.", packagePalletALoc2, currentPackage);
				var prevPackage = currentPackage;

				Assert(enumerator.MoveNext());
				currentPackage = enumerator.Current;
				AssertEquals(PackageComparerForLabelPrinting.DocDelimeterType.None, comparer.GetDelimeterTypeBetweenPackages(prevPackage, currentPackage));
				AssertEquals("Should be Pallet for Loc1.", packagePalletALoc1, currentPackage);
				prevPackage = currentPackage;

				Assert(enumerator.MoveNext());
				currentPackage = enumerator.Current;
				AssertEquals(PackageComparerForLabelPrinting.DocDelimeterType.None, comparer.GetDelimeterTypeBetweenPackages(prevPackage, currentPackage));
				AssertEquals("Should be Pallet for Loc3.", packagePalletALoc3, currentPackage);
				prevPackage = currentPackage;

				Assert(enumerator.MoveNext());
				currentPackage = enumerator.Current;
				AssertEquals(PackageComparerForLabelPrinting.DocDelimeterType.None, comparer.GetDelimeterTypeBetweenPackages(prevPackage, currentPackage));
				AssertEquals("Should be Pallet for Loc6.", packagePalletALoc6, currentPackage);
				prevPackage = currentPackage;

				Assert(enumerator.MoveNext());
				currentPackage = enumerator.Current;
				AssertEquals(PackageComparerForLabelPrinting.DocDelimeterType.None, comparer.GetDelimeterTypeBetweenPackages(prevPackage, currentPackage));
				AssertEquals("Should be Pallet for Loc7.", packagePalletALoc7, currentPackage);
				prevPackage = currentPackage;

				Assert(enumerator.MoveNext());
				currentPackage = enumerator.Current;
				AssertEquals(PackageComparerForLabelPrinting.DocDelimeterType.None, comparer.GetDelimeterTypeBetweenPackages(prevPackage, currentPackage));
				AssertEquals("Should be Pallet for Loc8.", packagePalletALoc8, currentPackage);
				prevPackage = currentPackage;

				// then goes cases for AreaA
				Assert(enumerator.MoveNext());
				currentPackage = enumerator.Current;
				AssertEquals(isPrintingWithoutSeparatorLabels
					? PackageComparerForLabelPrinting.DocDelimeterType.None
					: PackageComparerForLabelPrinting.DocDelimeterType.EndOfPallet, comparer.GetDelimeterTypeBetweenPackages(prevPackage, currentPackage));
				AssertEquals("Should be Case for Loc2.", packageCaseALoc2, currentPackage);
				prevPackage = currentPackage;

				Assert(enumerator.MoveNext());
				currentPackage = enumerator.Current;
				AssertEquals(PackageComparerForLabelPrinting.DocDelimeterType.None, comparer.GetDelimeterTypeBetweenPackages(prevPackage, currentPackage));
				AssertEquals("Should be Case for Loc1.", packageCaseALoc1, currentPackage);
				prevPackage = currentPackage;

				Assert(enumerator.MoveNext());
				currentPackage = enumerator.Current;
				AssertEquals(PackageComparerForLabelPrinting.DocDelimeterType.None, comparer.GetDelimeterTypeBetweenPackages(prevPackage, currentPackage));
				AssertEquals("Should be Case for Loc3.", packageCaseALoc3, currentPackage);
				prevPackage = currentPackage;

				Assert(enumerator.MoveNext());
				currentPackage = enumerator.Current;
				AssertEquals(PackageComparerForLabelPrinting.DocDelimeterType.None, comparer.GetDelimeterTypeBetweenPackages(prevPackage, currentPackage));
				AssertEquals("Should be Case for Loc6.", packageCaseALoc6, currentPackage);
				prevPackage = currentPackage;

				Assert(enumerator.MoveNext());
				currentPackage = enumerator.Current;
				AssertEquals(PackageComparerForLabelPrinting.DocDelimeterType.None, comparer.GetDelimeterTypeBetweenPackages(prevPackage, currentPackage));
				AssertEquals("Should be Case for Loc7.", packageCaseALoc7, currentPackage);
				prevPackage = currentPackage;

				Assert(enumerator.MoveNext());
				currentPackage = enumerator.Current;
				AssertEquals(PackageComparerForLabelPrinting.DocDelimeterType.None, comparer.GetDelimeterTypeBetweenPackages(prevPackage, currentPackage));
				AssertEquals("Should be Case for Loc8.", packageCaseALoc8, currentPackage);
				prevPackage = currentPackage;

				// then goes packages for split cases for areaA
				Assert(enumerator.MoveNext());
				currentPackage = enumerator.Current;
				AssertEquals(isPrintingWithoutSeparatorLabels
					? PackageComparerForLabelPrinting.DocDelimeterType.None
					: PackageComparerForLabelPrinting.DocDelimeterType.EndOfCase, comparer.GetDelimeterTypeBetweenPackages(prevPackage, currentPackage));
				// this package contains picklines from different areas, but the pickline from location with smalles pick sequence should be considered
				AssertEquals("Should be split case package for Loc2.", packAreaALoc2AreaBLoc4, currentPackage);
				prevPackage = currentPackage;

				Assert(enumerator.MoveNext());
				currentPackage = enumerator.Current;
				AssertEquals(PackageComparerForLabelPrinting.DocDelimeterType.None, comparer.GetDelimeterTypeBetweenPackages(prevPackage, currentPackage));
				AssertEquals("Should be split case package for Loc1.", packAreaALoc1, currentPackage);
				prevPackage = currentPackage;

				Assert(enumerator.MoveNext());
				currentPackage = enumerator.Current;
				AssertEquals(PackageComparerForLabelPrinting.DocDelimeterType.None, comparer.GetDelimeterTypeBetweenPackages(prevPackage, currentPackage));
				AssertEquals("Should be split case package for Loc3.", packAreaALoc3, currentPackage);
				prevPackage = currentPackage;

				Assert(enumerator.MoveNext());
				currentPackage = enumerator.Current;
				AssertEquals(PackageComparerForLabelPrinting.DocDelimeterType.None, comparer.GetDelimeterTypeBetweenPackages(prevPackage, currentPackage));
				AssertEquals("Should be split case package for Loc6.", packAreaALoc6, currentPackage);
				prevPackage = currentPackage;

				Assert(enumerator.MoveNext());
				currentPackage = enumerator.Current;
				AssertEquals(PackageComparerForLabelPrinting.DocDelimeterType.None, comparer.GetDelimeterTypeBetweenPackages(prevPackage, currentPackage));
				AssertEquals("Should be split case package for Loc7.", packAreaALoc7, currentPackage);
				prevPackage = currentPackage;

				Assert(enumerator.MoveNext());
				currentPackage = enumerator.Current;
				AssertEquals(PackageComparerForLabelPrinting.DocDelimeterType.None, comparer.GetDelimeterTypeBetweenPackages(prevPackage, currentPackage));
				AssertEquals("Should be split case package for Loc8.", packAreaALoc8, currentPackage);
				prevPackage = currentPackage;

				// then goes next area and we start from pallets again

				Assert(enumerator.MoveNext());
				currentPackage = enumerator.Current;
				AssertEquals(isPrintingWithoutSeparatorLabels
					? PackageComparerForLabelPrinting.DocDelimeterType.None
					: PackageComparerForLabelPrinting.DocDelimeterType.EndOfSplitCase
						| PackageComparerForLabelPrinting.DocDelimeterType.EndOfArea, comparer.GetDelimeterTypeBetweenPackages(prevPackage, currentPackage));
				AssertEquals("Should be Pallet for Loc5.", packagePalletBLoc5, currentPackage);
				prevPackage = currentPackage;

				Assert(enumerator.MoveNext());
				currentPackage = enumerator.Current;
				AssertEquals(PackageComparerForLabelPrinting.DocDelimeterType.None, comparer.GetDelimeterTypeBetweenPackages(prevPackage, currentPackage));
				AssertEquals("Should be Pallet for Loc4.", packagePalletBLoc4, currentPackage);
				prevPackage = currentPackage;

				Assert(enumerator.MoveNext());
				currentPackage = enumerator.Current;
				AssertEquals(isPrintingWithoutSeparatorLabels
					? PackageComparerForLabelPrinting.DocDelimeterType.None
					: PackageComparerForLabelPrinting.DocDelimeterType.EndOfPallet, comparer.GetDelimeterTypeBetweenPackages(prevPackage, currentPackage));
				AssertEquals("Should be Case for Loc5.", packageCaseBLoc5, currentPackage);
				prevPackage = currentPackage;

				Assert(enumerator.MoveNext());
				currentPackage = enumerator.Current;
				AssertEquals(PackageComparerForLabelPrinting.DocDelimeterType.None, comparer.GetDelimeterTypeBetweenPackages(prevPackage, currentPackage));
				AssertEquals("Should be Case for Loc4.", packageCaseBLoc4, currentPackage);
				prevPackage = currentPackage;

				Assert(enumerator.MoveNext());
				currentPackage = enumerator.Current;
				AssertEquals(isPrintingWithoutSeparatorLabels
					? PackageComparerForLabelPrinting.DocDelimeterType.None
					: PackageComparerForLabelPrinting.DocDelimeterType.EndOfCase, comparer.GetDelimeterTypeBetweenPackages(prevPackage, currentPackage));
				AssertEquals("Should be split case package for Loc5.", packAreaBLoc5, currentPackage);

				Assert("Should be no more packages", !enumerator.MoveNext());
				AssertEquals(isPrintingWithoutSeparatorLabels
					? PackageComparerForLabelPrinting.DocDelimeterType.None
					: PackageComparerForLabelPrinting.DocDelimeterType.EndOfSplitCase
							| PackageComparerForLabelPrinting.DocDelimeterType.EndOfArea, comparer.GetDelimeterTypeBetweenPackages(currentPackage, null));
			}
		}

		#endregion

		#region TestComparePackagesWithPackedRCAsAndPickLines

		public void TestComparePackagesWithPackedRCAsAndPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);
			var locA1 = data.Whs1.FindLocation("A-1");
			var locA2 = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1, locA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1, locA2, "");

			Factory.Save();
			locA1.WLV_PickPathSequence = 2;
			locA2.WLV_PickPathSequence = 1;

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 2);
			var pick = Helper.CreatePickNew(order);
			var pickLineA1 = pick.GetAllPickLines().Single(pl => pl.InventoryLine.Location == locA1);
			var pickLineA2 = pick.GetAllPickLines().Single(pl => pl.InventoryLine.Location == locA2);

			pickLineA2.WZ_ReleaseCapturedPartAttrib1 = "ZZZ";

			var packingHelper = new PackingTestHelper(Factory);
			var package1 = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			var package2 = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			packingHelper.CreatePackageDivot(package1, pickLineA1);
			packingHelper.CreatePackageDivot(package2, pickLineA2);
			Factory.Save();

			var sorter = new PackageComparerForLabelPrinting(pick);

			AssertEquals(1, sorter.Compare(package1, package2));
			AssertEquals(-1, sorter.Compare(package2, package1));
		}

		#endregion

		#region TestComparePackages_RowPathSequence

		public void TestComparePackages_RowPathSequence()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var rowB = Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 1);
			Factory.Save();

			var locA = data.Whs1.FindLocation("A");
			var locB = data.Whs1.FindLocation("B");
			locA.Row.WR_PickPathSequence = 1;
			locB.Row.WR_PickPathSequence = 2;
			locA.WLV_PickPathSequence = 2;
			locB.WLV_PickPathSequence = 1;

			var area = Helper.CreateArea(data.Whs1, "AAA");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1, locA, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1, locB, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 2);
			var pick = Helper.CreatePickNew(order);
			var pickLineA = pick.GetAllPickLines().Single(pl => pl.InventoryLine.Location == locA);
			var pickLineB = pick.GetAllPickLines().Single(pl => pl.InventoryLine.Location == locB);

			var packingHelper = new PackingTestHelper(Factory);
			var package1 = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			var package2 = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			packingHelper.CreatePackageDivot(package1, pickLineA);
			packingHelper.CreatePackageDivot(package2, pickLineB);
			Factory.Save();

			var sorter = new PackageComparerForLabelPrinting(pick);
			AssertEquals(-1, sorter.Compare(package1, package2));
			AssertEquals(1, sorter.Compare(package2, package1));
		}

		#endregion

		#region TestComparePackages_RowName

		public void TestComparePackages_RowName()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var rowB = Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 1);
			Factory.Save();

			var locA = data.Whs1.FindLocation("A");
			var locB = data.Whs1.FindLocation("B");
			locA.Row.WR_PickPathSequence = 0;
			locB.Row.WR_PickPathSequence = 0;
			locA.WLV_PickPathSequence = 2;
			locB.WLV_PickPathSequence = 1;

			var area = Helper.CreateArea(data.Whs1, "AAA");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1, locA, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1, locB, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 2);
			var pick = Helper.CreatePickNew(order);
			var pickLineA = pick.GetAllPickLines().Single(pl => pl.InventoryLine.Location == locA);
			var pickLineB = pick.GetAllPickLines().Single(pl => pl.InventoryLine.Location == locB);

			var packingHelper = new PackingTestHelper(Factory);
			var package1 = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			var package2 = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			packingHelper.CreatePackageDivot(package1, pickLineA);
			packingHelper.CreatePackageDivot(package2, pickLineB);
			Factory.Save();

			var sorter = new PackageComparerForLabelPrinting(pick);
			AssertEquals(-1, sorter.Compare(package1, package2));
			AssertEquals(1, sorter.Compare(package2, package1));
		}

		#endregion

		#region TestComparePackages_LocationString

		public void TestComparePackages_LocationString()
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			Factory.Save();

			var locA1 = data.Whs1.FindLocation("A-1");
			var locA2 = data.Whs1.FindLocation("A-2");
			var locA10 = data.Whs1.FindLocation("A-10");

			locA1.WLV_PickPathSequence = 1;
			locA2.WLV_PickPathSequence = 1;
			locA10.WLV_PickPathSequence = 1;

			var area = Helper.CreateArea(data.Whs1, "AAA");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1, locA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1, locA2, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 1, locA10, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 3);
			var pick = Helper.CreatePickNew(order);
			var pickLineA1 = pick.GetAllPickLines().Single(pl => pl.InventoryLine.Location == locA1);
			var pickLineA2 = pick.GetAllPickLines().Single(pl => pl.InventoryLine.Location == locA2);
			var pickLineA10 = pick.GetAllPickLines().Single(pl => pl.InventoryLine.Location == locA10);

			var packingHelper = new PackingTestHelper(Factory);
			var package1 = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			var package2 = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			var package3 = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			packingHelper.CreatePackageDivot(package1, pickLineA1);
			packingHelper.CreatePackageDivot(package2, pickLineA2);
			packingHelper.CreatePackageDivot(package3, pickLineA10);
			Factory.Save();

			var sorter = new PackageComparerForLabelPrinting(pick);

			// A1 before A2
			AssertEquals(-1, sorter.Compare(package1, package2));
			AssertEquals(1, sorter.Compare(package2, package1));

			// A1 before A10
			AssertEquals(true, sorter.Compare(package1, package3) < 0);
			AssertEquals(true, sorter.Compare(package3, package1) > 0);

			// A2 before A10
			AssertEquals(true, sorter.Compare(package2, package3) < 0);
			AssertEquals(true, sorter.Compare(package3, package2) > 0);
		}

		#endregion

		#region TestComparePackages_AreaName_InTransit

		public void TestComparePackages_AreaName_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locA1 = data.Whs1.FindLocation("A-1");
			var locA2 = data.Whs1.FindLocation("A-2");
			var area = Helper.CreateArea(data.Whs1, "AAA");
			locA2.WLV_WA_PickingArea = area.PK;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1, locA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1, locA2, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 2);
			var pick = Helper.CreatePickNew(order);
			var pickLineA1 = pick.GetAllPickLines().Single(pl => pl.InventoryLine.Location == locA1);
			var pickLineA2 = pick.GetAllPickLines().Single(pl => pl.InventoryLine.Location == locA2);

			var packingHelper = new PackingTestHelper(Factory);
			var package1 = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			var package2 = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			packingHelper.CreatePackageDivot(package1, pickLineA1);
			packingHelper.CreatePackageDivot(package2, pickLineA2);

			Helper.PickAndMakeInTransitTransfer(pickLineA1, ZDateTimeOffset.Now);
			Helper.PickAndMakeInTransitTransfer(pickLineA2, ZDateTimeOffset.Now);
			Factory.Save();

			var sorter = new PackageComparerForLabelPrinting(pick);
			AssertEquals(1, sorter.Compare(package1, package2));
			AssertEquals(-1, sorter.Compare(package2, package1));
		}

		#endregion

		#region TestComparePackages_PickPathSequence_InTransit

		public void TestComparePackages_PickPathSequence_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locA1 = data.Whs1.FindLocation("A-1");
			var locA2 = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1, locA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1, locA2, "");

			Factory.Save();
			locA1.WLV_PickPathSequence = 2;
			locA2.WLV_PickPathSequence = 1;

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 2);
			var pick = Helper.CreatePickNew(order);
			var pickLineA1 = pick.GetAllPickLines().Single(pl => pl.InventoryLine.Location == locA1);
			var pickLineA2 = pick.GetAllPickLines().Single(pl => pl.InventoryLine.Location == locA2);

			var packingHelper = new PackingTestHelper(Factory);
			var package1 = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			var package2 = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			packingHelper.CreatePackageDivot(package1, pickLineA1);
			packingHelper.CreatePackageDivot(package2, pickLineA2);

			Helper.PickAndMakeInTransitTransfer(pickLineA1, ZDateTimeOffset.Now);
			Helper.PickAndMakeInTransitTransfer(pickLineA2, ZDateTimeOffset.Now);
			Factory.Save();

			var sorter = new PackageComparerForLabelPrinting(pick);
			AssertEquals(1, sorter.Compare(package1, package2));
			AssertEquals(-1, sorter.Compare(package2, package1));
		}

		#endregion

		#region TestComparePackages_PickGroupPrioritisedOverPickSequence

		public void TestComparePackages_PickGroupPrioritisedOverPickSequence()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locA1 = data.Whs1.FindLocation("A-1");
			var locA2 = data.Whs1.FindLocation("A-2");
			locA2.WLV_PickPathSequence = 2;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1, locA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1, locA2, "");
			Factory.Save();

			var collection = new PickGroupCollection();
			var pickGroup1 = collection.AddNew();
			var pickGroup2 = collection.AddNew();
			pickGroup1.Description = (NoResString)"Group 1";
			pickGroup2.Description = (NoResString)"Group 2";
			WarehouseDataRegistry.Instance.PickGroups.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1);
			var pick = Helper.CreatePickNew(order1, order2);

			var orderLineA1 = order1.Lines[0];
			var orderLineA2 = order2.Lines[0];
			var pickLineA1 = orderLineA1.PickLines[0];
			var pickLineA2 = orderLineA2.PickLines[0];

			AssertEquals("Precondition: Order 1 should have one order line and one pick line, at location A1", locA1, pickLineA1.InventoryLine.Location);
			AssertEquals("Precondition: Order 2 should have one order line and one pick line, at location A2", locA2, pickLineA2.InventoryLine.Location);
			orderLineA1.WE_PickGroup = new ZShort(pickGroup2.Code);
			orderLineA2.WE_PickGroup = new ZShort(pickGroup1.Code);

			var packingHelper = new PackingTestHelper(Factory);
			var package1 = packingHelper.CreatePackage(order1.PackageJob, 1, "CAS");
			var package2 = packingHelper.CreatePackage(order2.PackageJob, 1, "CAS");
			packingHelper.CreatePackageDivot(package1, pickLineA1);
			packingHelper.CreatePackageDivot(package2, pickLineA2);

			Helper.PickAndMakeInTransitTransfer(pickLineA1, ZDateTimeOffset.Now);
			Helper.PickAndMakeInTransitTransfer(pickLineA2, ZDateTimeOffset.Now);

			var sorter = new PackageComparerForLabelPrinting(pick);
			AssertEquals(-1, sorter.Compare(package2, package1));
			AssertEquals(1, sorter.Compare(package1, package2));
		}

		#endregion

		#region TestComparePackages_UOMTypePrioritisedOverPickGroup

		public void TestComparePackages_UOMTypePrioritisedOverPickGroup()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var locA1 = data.Whs1.FindLocation("A-1");
			var locA2 = data.Whs1.FindLocation("A-2");

			Helper.CreateProductUnit(data.Part1, "CAS", 5);
			Helper.CreateProductUnit(data.Part1, "PLT", 20);

			var collection = new PickGroupCollection();
			var pickGroup1 = collection.AddNew();
			var pickGroup2 = collection.AddNew();
			pickGroup1.Description = (NoResString)"Group 1";
			pickGroup2.Description = (NoResString)"Group 2";
			WarehouseDataRegistry.Instance.PickGroups.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType = UOMPackTypesList.Codes.Case;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType = UOMPackTypesList.Codes.Pallet;

			var receivePalletLoc1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20, locA1, "PLT1");
			var receiveCaseLoc2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5, locA2, "CAS1");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 20);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5);
			var pick = Helper.CreatePickNew(order);
			orderLine1.WE_PickGroup = new ZShort(pickGroup2.Code);
			orderLine2.WE_PickGroup = new ZShort(pickGroup1.Code);
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			AssertEquals("There should be 2 packages: 1 for pallet and 1 for case.", 2, pick.OuterPackages.Count);
			var packageCAS = pick.OuterPackages.Single(package => package.PackType.F3_Code.Equals("CAS"));
			var packagePLT = pick.OuterPackages.Single(package => package.PackType.F3_Code.Equals("PLT"));

			var sorter = new PackageComparerForLabelPrinting(pick);
			AssertEquals(-1, sorter.Compare(packagePLT, packageCAS));
			AssertEquals(1, sorter.Compare(packageCAS, packagePLT));
		}

		#endregion

		#region TestGetDelimeterTypeBetweenPackages_InTransit

		public void TestGetDelimeterTypeBetweenPackages_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locA1 = data.Whs1.FindLocation("A-1");
			var locA2 = data.Whs1.FindLocation("A-2");
			var area = Helper.CreateArea(data.Whs1, "AAA");
			locA2.WLV_WA_PickingArea = area.PK;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1, locA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1, locA2, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 2);
			var pick = Helper.CreatePickNew(order);
			var pickLineA1 = pick.GetAllPickLines().Single(pl => pl.InventoryLine.Location == locA1);
			var pickLineA2 = pick.GetAllPickLines().Single(pl => pl.InventoryLine.Location == locA2);

			var packingHelper = new PackingTestHelper(Factory);
			var package1 = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			var package2 = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			packingHelper.CreatePackageDivot(package1, pickLineA1);
			packingHelper.CreatePackageDivot(package2, pickLineA2);

			Helper.PickAndMakeInTransitTransfer(pickLineA1, ZDateTimeOffset.Now);
			Helper.PickAndMakeInTransitTransfer(pickLineA2, ZDateTimeOffset.Now);
			Factory.Save();

			var sorter = new PackageComparerForLabelPrinting(pick);
			AssertEquals(PackageComparerForLabelPrinting.DocDelimeterType.EndOfArea, sorter.GetDelimeterTypeBetweenPackages(package1, package2));
			AssertEquals(PackageComparerForLabelPrinting.DocDelimeterType.EndOfArea, sorter.GetDelimeterTypeBetweenPackages(package2, package1));
		}

		#endregion

		#region TestGetDelimeterTypeBetweenPackages_PickGroup

		public void TestGetDelimeterTypeBetweenPackages_PickGroup()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locA1 = data.Whs1.FindLocation("A-1");
			var locA2 = data.Whs1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1, locA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1, locA2, "");
			Factory.Save();

			var collection = new PickGroupCollection();
			var pickGroup1 = collection.AddNew();
			var pickGroup2 = collection.AddNew();
			pickGroup1.Description = (NoResString)"Group 1";
			pickGroup2.Description = (NoResString)"Group 2";
			WarehouseDataRegistry.Instance.PickGroups.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1);
			var pick = Helper.CreatePickNew(order1, order2);
			var orderLine1 = order1.Lines[0];
			var orderLine2 = order2.Lines[0];
			var pickLine1 = orderLine1.PickLines[0];
			var pickLine2 = orderLine2.PickLines[0];
			orderLine1.WE_PickGroup = new ZShort(pickGroup1.Code);
			orderLine2.WE_PickGroup = new ZShort(pickGroup2.Code);

			var packingHelper = new PackingTestHelper(Factory);
			var package1 = packingHelper.CreatePackage(order1.PackageJob, 1, "CAS");
			var package2 = packingHelper.CreatePackage(order2.PackageJob, 1, "CAS");
			packingHelper.CreatePackageDivot(package1, pickLine1);
			packingHelper.CreatePackageDivot(package2, pickLine2);

			Factory.Save();

			var sorter = new PackageComparerForLabelPrinting(pick);
			AssertEquals(PackageComparerForLabelPrinting.DocDelimeterType.EndOfPickGroup, sorter.GetDelimeterTypeBetweenPackages(package1, package2));
			AssertEquals(PackageComparerForLabelPrinting.DocDelimeterType.EndOfPickGroup, sorter.GetDelimeterTypeBetweenPackages(package2, package1));
		}

		#endregion

		#region TestGetDelimeterTypeBetweenPackages_PickGroup_EndofUOMType

		public void TestGetDelimeterTypeBetweenPackages_PickGroup_EndOfUOMTypeDifferentPickGroups()
		{
			var collection = new PickGroupCollection();
			var pickGroup1 = collection.AddNew();
			var pickGroup2 = collection.AddNew();
			pickGroup1.Description = (NoResString)"Group 1";
			pickGroup2.Description = (NoResString)"Group 2";
			using (WarehouseDataRegistry.Instance.PickGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				TestGetDelimeterTypeBetweenPackages_PickGroup_EndOfUOMTypeCore(pickGroup1, pickGroup2);
			}
		}

		public void TestGetDelimeterTypeBetweenPackages_PickGroup_EndOfUOMTypeSamePickGroup()
		{
			var collection = new PickGroupCollection();
			var pickGroup1 = collection.AddNew();
			pickGroup1.Description = (NoResString)"Group 1";
			using (WarehouseDataRegistry.Instance.PickGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				TestGetDelimeterTypeBetweenPackages_PickGroup_EndOfUOMTypeCore(pickGroup1, pickGroup1);
			}
		}

		void TestGetDelimeterTypeBetweenPackages_PickGroup_EndOfUOMTypeCore(PickGroup firstOrderLinePickGroup, PickGroup secondOrderLinePickGroup)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var locA1 = data.Whs1.FindLocation("A-1");
			var locA2 = data.Whs1.FindLocation("A-2");

			Helper.CreateProductUnit(data.Part1, "CAS", 5);
			Helper.CreateProductUnit(data.Part1, "PLT", 20);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5, locA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 20, locA2, "");

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType = UOMPackTypesList.Codes.Case;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType = UOMPackTypesList.Codes.Pallet;

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 20);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5);
			var pick = Helper.CreatePickNew(order);
			orderLine1.WE_PickGroup = new ZShort(firstOrderLinePickGroup.Code);
			orderLine2.WE_PickGroup = new ZShort(secondOrderLinePickGroup.Code);

			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			var packageCAS = pick.OuterPackages.Single(package => package.PackType.F3_Code.Equals("CAS"));
			var packagePLT = pick.OuterPackages.Single(package => package.PackType.F3_Code.Equals("PLT"));

			var sorter = new PackageComparerForLabelPrinting(pick);
			var delimeterType1 = sorter.GetDelimeterTypeBetweenPackages(packageCAS, packagePLT);
			var delimeterType2 = sorter.GetDelimeterTypeBetweenPackages(packagePLT, packageCAS);
			Assert(delimeterType1.HasFlag(PackageComparerForLabelPrinting.DocDelimeterType.EndOfCase));
			Assert(delimeterType1.HasFlag(PackageComparerForLabelPrinting.DocDelimeterType.EndOfPickGroup));
			Assert(delimeterType2.HasFlag(PackageComparerForLabelPrinting.DocDelimeterType.EndOfPallet));
			Assert(delimeterType2.HasFlag(PackageComparerForLabelPrinting.DocDelimeterType.EndOfPickGroup));
		}

		#endregion

		#region TestGetDelimeterTypeBetweenPackages_PickGroup_LastPackage

		public void TestGetDelimeterTypeBetweenPackages_PickGroup_LastPackageNonEmptyPickGroup()
		{
			var collection = new PickGroupCollection();
			var pickGroup1 = collection.AddNew();
			pickGroup1.Description = (NoResString)"Group 1";
			using (WarehouseDataRegistry.Instance.PickGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				TestGetDelimeterTypeBetweenPackages_PickGroup_LastPackageCore(pickGroup1, true);
			}
		}

		public void TestGetDelimeterTypeBetweenPackages_PickGroup_LastPackageEmptyPickGroup()
		{
			TestGetDelimeterTypeBetweenPackages_PickGroup_LastPackageCore();
		}

		void TestGetDelimeterTypeBetweenPackages_PickGroup_LastPackageCore(PickGroup orderLinePickGroup = null, bool assignPickGroupToOrderLine = false)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var locA1 = data.Whs1.FindLocation("A-1");

			Helper.CreateProductUnit(data.Part1, "CAS", 5);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5, locA1, "");
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType = UOMPackTypesList.Codes.Case;
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5);
			var pick = Helper.CreatePickNew(order);
			if (assignPickGroupToOrderLine)
			{
				order.Lines[0].WE_PickGroup = new ZShort(orderLinePickGroup.Code);
			}

			pick.WP_PickCasesByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();

			var packageCAS = pick.OuterPackages.Single(package => package.PackType.F3_Code.Equals("CAS"));

			var sorter = new PackageComparerForLabelPrinting(pick);
			var delimeterType = sorter.GetDelimeterTypeBetweenPackages(packageCAS, null);
			Assert(delimeterType.HasFlag(PackageComparerForLabelPrinting.DocDelimeterType.EndOfCase));
			AssertEquals("If no line in the case has a pick group, we should not print and end of pick group label",
				assignPickGroupToOrderLine, delimeterType.HasFlag(PackageComparerForLabelPrinting.DocDelimeterType.EndOfPickGroup));
		}

		#endregion

		#region TestAllUOMTypesHandled

		public void TestAllUOMTypesHandled()
		{
			var uomCodes = typeof(UOMPackTypesList.Codes).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();
			CombineAssertions(() =>
			{
				AssertEquals("If this assertion fails - check UOMTypesSortingWeight property.", 3, uomCodes.Count);
				AssertNotNull(uomCodes.SingleOrDefault(code => code.Name == nameof(UOMPackTypesList.Codes.SplitCase)));
				AssertNotNull(uomCodes.SingleOrDefault(code => code.Name == nameof(UOMPackTypesList.Codes.Case)));
				AssertNotNull(uomCodes.SingleOrDefault(code => code.Name == nameof(UOMPackTypesList.Codes.Pallet)));
			});
		}

		#endregion
	}
}
