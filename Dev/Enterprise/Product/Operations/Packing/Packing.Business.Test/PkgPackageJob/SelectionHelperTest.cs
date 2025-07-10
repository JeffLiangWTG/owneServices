using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Packing.Business.Testing
{
	public class SelectionHelperTest : PackingTestCaseWithFactory
	{
		#region Flags

		#region TestIsNothingSelected

		public void TestIsNothingSelected()
		{
			Data.CreatePackingData();
			var selected = Data.PackageJob.Selected;
			AssertEquals(true, selected.IsNothingSelected);

			var box = Data.PackageJob.Packages.AddNew("BOX");
			var packedItems = box.Pack(Data.DummyLine1, 10m);
			AssertEquals(true, selected.IsNothingSelected);

			selected.UpdateSelectedPackages(new[] { box });
			AssertEquals(false, selected.IsNothingSelected);

			selected.UpdateSelectedPackedItems(packedItems);
			AssertEquals(false, selected.IsNothingSelected);

			selected.UpdateSelectedPackages(Array.Empty<PkgPackage>());
			AssertEquals(false, selected.IsNothingSelected);

			selected.UpdateSelectedPackedItems(Array.Empty<PkgPackageItemDivotsWrapper>());
			AssertEquals(true, selected.IsNothingSelected);
		}

		#endregion

		#region TestIsPackageSelected

		public void TestIsPackageSelected()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			AssertEquals(false, packageJob.Selected.IsPackageSelected);

			packageJob.Selected.UpdateSelectedPackages(new[] { packageJob.Packages.AddNew() });
			AssertEquals(true, packageJob.Selected.IsPackageSelected);
		}

		#endregion

		#region TestIsPackedItemSelected

		public void TestIsPackedItemSelected()
		{
			Data.CreatePackingData();
			var box = Data.PackageJob.Packages.AddNew("BOX");
			var packedItem1 = box.Pack_ForTesting(Data.DummyLine1, 7m);
			var packedItem2 = box.Pack_ForTesting(Data.DummyLine2, 11m);

			var selected = Data.PackageJob.Selected;
			AssertEquals(false, selected.IsPackedItemSelected);

			Data.PackageJob.Selected.UpdateSelectedPackedItems(new[] { packedItem1 });
			AssertEquals(true, selected.IsPackedItemSelected);

			Data.PackageJob.Selected.UpdateSelectedPackedItems(Enumerable.Empty<PkgPackageItemDivotsWrapper>());
			AssertEquals(false, selected.IsPackedItemSelected);

			Data.PackageJob.Selected.UpdateSelectedPackedItems(new[] { packedItem1, packedItem2 });
			AssertEquals(true, selected.IsPackedItemSelected);
		}

		#endregion

		#region TestIsSinglePackageSelected

		public void TestIsSinglePackageSelected()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			AssertEquals(false, packageJob.Selected.IsSinglePackageSelected);

			packageJob.Selected.UpdateSelectedPackages(new[] { packageJob.Packages.AddNew() });
			AssertEquals(true, packageJob.Selected.IsSinglePackageSelected);

			packageJob.Selected.UpdateSelectedPackages(new[] { packageJob.Packages[0], packageJob.Packages.AddNew() });
			AssertEquals(false, packageJob.Selected.IsSinglePackageSelected);
		}

		#endregion

		#region TestIsSinglePackedItemSelected

		public void TestIsSinglePackedItemSelected()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var box = Data.PackageJob.Packages.AddNew("BOX");
			var packedItem1 = box.Pack_ForTesting(Data.DummyLine1, 10m);
			var packedItem2 = box.Pack_ForTesting(Data.DummyLine2, 10m);
			AssertEquals(false, packageJob.Selected.IsSinglePackedItemSelected);

			packageJob.Selected.UpdateSelectedPackedItems(new[] { packedItem1 });
			AssertEquals(true, packageJob.Selected.IsSinglePackedItemSelected);

			packageJob.Selected.UpdateSelectedPackedItems(new[] { packedItem1, packedItem2 });
			AssertEquals(false, packageJob.Selected.IsSinglePackedItemSelected);
		}

		#endregion

		#endregion

		#region Selections

		#region TestSelectedPackage

		public void TestSelectedPackage()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var keg1 = packageJob.Packages.AddNew("KEG");
			var keg2 = packageJob.Packages.AddNew("KEG");

			// nothing selected
			AssertNull(packageJob.Selected.SelectedPackage);

			packageJob.Selected.UpdateSelectedPackages(new PkgPackage[] { keg1 });
			AssertEquals(keg1, packageJob.Selected.SelectedPackage);

			packageJob.Selected.UpdateSelectedPackages(new PkgPackage[] { keg2 });
			AssertEquals(keg2, packageJob.Selected.SelectedPackage);

			// mutli-packages selected
			packageJob.Selected.UpdateSelectedPackages(new PkgPackage[] { keg1, keg2 });
			AssertExceptionThrown(typeof(InvalidOperationException), () => { var poke = packageJob.Selected.SelectedPackage; });
		}

		#endregion

		#region TestSelectedPackages

		public void TestSelectedPackages()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var selected = packageJob.Selected;
			var keg1 = packageJob.Packages.AddNew("KEG");
			var keg2 = packageJob.Packages.AddNew("KEG");
			var keg3 = packageJob.Packages.AddNew("KEG");

			// nothing selected
			AssertEquals(false, selected.SelectedPackages.Any());

			selected.UpdateSelectedPackages(new[] { keg1, keg2 });
			AssertContainsExactElementsInAnyOrder(new[] { keg1, keg2 }, selected.SelectedPackages);

			selected.UpdateSelectedPackages(new[] { keg3 });
			AssertContainsExactElementsInAnyOrder(new[] { keg3 }, selected.SelectedPackages);

			// attempt to select a package that is not part of the PackageJob
			AssertExceptionThrown(typeof(InvalidOperationException), () => selected.UpdateSelectedPackages(new[] { Factory.New<PkgPackage>() }));
		}

		#endregion

		#region TestSelectionParentPackage

		public void TestSelectionParentPackage()
		{
			Data.CreatePackingData();
			var selected = Data.PackageJob.Selected;
			var pallet = Data.PackageJob.Packages.AddNew("PLT");
			var box1 = pallet.Packages.AddNew("BOX");
			var box2 = pallet.Packages.AddNew("BOX");
			var box2PackedItems = box2.Pack(Data.DummyLine1, 26m);

			// nothing selected
			AssertNull(selected.SelectionParentPackage);

			// outer (pallet) selected
			selected.UpdateSelectedPackages(new[] { pallet });
			AssertNull(selected.SelectionParentPackage);

			// box selected
			selected.UpdateSelectedPackages(new[] { box1 });
			AssertEquals(pallet, selected.SelectionParentPackage);

			// both boxes selected
			selected.UpdateSelectedPackages(new[] { box1, box2 });
			AssertEquals(pallet, selected.SelectionParentPackage);

			// packed item selected
			selected.UpdateSelectedPackages(Array.Empty<PkgPackage>());
			selected.UpdateSelectedPackedItems(box2PackedItems);
			AssertEquals(box2, selected.SelectionParentPackage);

			// packed items and packages from different levels selected (cannot actually happen in the Packing Tree, just here to confirm it doesn't die if invoked this way)
			selected.UpdateSelectedPackages(new PkgPackage[] { box1 });
			selected.UpdateSelectedPackedItems(box2PackedItems);
			AssertEquals(pallet, selected.SelectionParentPackage);
		}

		#endregion

		#region TestSelectedPackedItem

		public void TestSelectedPackedItem()
		{
			Data.CreatePackingData();
			var selected = Data.PackageJob.Selected;
			var pallet = Data.PackageJob.Packages.AddNew("PLT");
			var packedItem1 = pallet.Pack_ForTesting(Data.DummyLine1, 7m);
			var packedItem2 = pallet.Pack_ForTesting(Data.DummyLine2, 10m);

			// nothing selected
			AssertNull(selected.SelectedPackedItem);

			selected.UpdateSelectedPackedItems(new[] { packedItem1 });
			AssertEquals(packedItem1, selected.SelectedPackedItem);

			selected.UpdateSelectedPackedItems(new[] { packedItem2 });
			AssertEquals(packedItem2, selected.SelectedPackedItem);

			// mutli-PackedItems selected
			selected.UpdateSelectedPackedItems(new[] { packedItem1, packedItem2 });
			AssertExceptionThrown(typeof(InvalidOperationException), () => { var poke = selected.SelectedPackedItem; });
		}

		#endregion

		#region TestSelectedPackedItems

		public void TestSelectedPackedItems()
		{
			Data.CreatePackingData();
			var selected = Data.PackageJob.Selected;
			var pallet = Data.PackageJob.Packages.AddNew("PLT");
			var packedItem1 = pallet.Pack_ForTesting(Data.DummyLine1, 7m);
			var packedItem2 = pallet.Pack_ForTesting(Data.DummyLine2, 10m);
			var packedItem3 = pallet.Pack_ForTesting(Data.DummyLine3, 6m);

			// nothing selected
			AssertEquals(false, selected.SelectedPackedItems.Any());

			selected.UpdateSelectedPackedItems(new[] { packedItem1, packedItem2 });
			AssertContainsExactElementsInAnyOrder(new[] { packedItem1, packedItem2 }, selected.SelectedPackedItems);

			selected.UpdateSelectedPackedItems(new[] { packedItem3 });
			AssertContainsExactElementsInAnyOrder(new[] { packedItem3 }, selected.SelectedPackedItems);

			// attempt to select a PackedItem that is not part of the PackedItemJob
			var packageJob2 = Factory.New<PkgPackageJob>();
			var dodgyBox = packageJob2.Packages.AddNew("BOX");
			var dodgyPackedItems = dodgyBox.Pack(Data.DummyLine1, 5m);
			AssertExceptionThrown(typeof(InvalidOperationException), () => selected.UpdateSelectedPackedItems(dodgyPackedItems));
		}

		public void TestSelectedPackedItems_ParentPackageIsRemoved()
		{
			Data.CreatePackingData();
			var selected = Data.PackageJob.Selected;
			var parentPakckage1 = Data.PackageJob.Packages.AddNew("PLT");
			var packedItem1 = parentPakckage1.Pack_ForTesting(Data.DummyLine1, 7m);
			var packedItem2 = parentPakckage1.Pack_ForTesting(Data.DummyLine2, 4m);

			var parentPakckage2 = Data.PackageJob.Packages.AddNew("BOX");
			var packedItem3 = parentPakckage2.Pack_ForTesting(Data.DummyLine3, 5m);

			selected.UpdateSelectedPackedItems(new[] { packedItem1, packedItem2, packedItem3 });
			AssertContainsExactElementsInAnyOrder(new[] { packedItem1, packedItem2, packedItem3 }, selected.SelectedPackedItems);

			// attempt to select a PackedItem whose Parent Package is removed
			Data.PackageJob.Packages.Delete(parentPakckage1);
			selected.UpdateSelectedPackedItems(new[] { packedItem1, packedItem2, packedItem3 });
			AssertContainsExactElementsInAnyOrder(new[] { packedItem3 }, selected.SelectedPackedItems);
		}

		#endregion

		#endregion

		#region Items to Unpack

		#region TestItemsToUnpackBasedOnSelected

		public void TestItemsToUnpackBasedOnSelected()
		{
			Data.CreatePackingData();
			var selected = Data.PackageJob.Selected;

			// setup some packages and packed items
			var pallet1 = Data.PackageJob.Packages.AddNew("PLT");
			var box1 = pallet1.Packages.AddNew("BOX");
			var box2 = pallet1.Packages.AddNew("BOX");
			var box1PackedItem1 = box1.Pack_ForTesting(Data.DummyLine1, 10m);
			var box1PackedItem2 = box1.Pack_ForTesting(Data.DummyLine2, 7m);

			var pallet2 = Data.PackageJob.Packages.AddNew("PLT");
			var pallet2PackedItem = pallet2.Pack_ForTesting(Data.DummyLine3, 26);

			// valid selections

			// 2 pallets selected
			selected.UpdateSelectedPackages(new[] { pallet1, pallet2 });
			AssertPackagesAndPackedItems(selected, new[] { pallet1, pallet2 }, Array.Empty<PkgPackageItemDivotsWrapper>());

			// box1 selected
			selected.UpdateSelectedPackages(new[] { box1 });
			AssertPackagesAndPackedItems(selected, new[] { box1 }, Array.Empty<PkgPackageItemDivotsWrapper>());

			// items packed into box1 selected
			selected.UpdateSelectedPackages(Array.Empty<PkgPackage>());
			selected.UpdateSelectedPackedItems(new[] { box1PackedItem1, box1PackedItem2 });
			AssertPackagesAndPackedItems(selected, Array.Empty<PkgPackage>(), new[] { box1PackedItem1, box1PackedItem2 });

			// combination of packages and packed items selected (cannot actually happen based on current tree selection rules, but in case this changes..)
			selected.UpdateSelectedPackages(new[] { pallet1, pallet2, box2 });
			selected.UpdateSelectedPackedItems(new[] { box1PackedItem1, box1PackedItem2, pallet2PackedItem });
			AssertPackagesAndPackedItems(selected, new[] { pallet1, pallet2, box2 }, new[] { box1PackedItem1, box1PackedItem2, pallet2PackedItem });

			// invalid selections

			// nothing selected
			selected.UpdateSelectedPackages(Array.Empty<PkgPackage>());
			selected.UpdateSelectedPackedItems(Array.Empty<PkgPackageItemDivotsWrapper>());
			AssertPackagesAndPackedItems(selected, Array.Empty<PkgPackage>(), Array.Empty<PkgPackageItemDivotsWrapper>(), "Select one or more Items or Packages to Unpack.");

			// 1 open and 1 closed pallet selected
			selected.UpdateSelectedPackages(new[] { pallet1, pallet2 });
			pallet1.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			AssertPackagesAndPackedItems(selected, Array.Empty<PkgPackage>(), Array.Empty<PkgPackageItemDivotsWrapper>(), "Cannot Unpack because one or more Packages are Closed or Released.");
			pallet1.KP_ClosedTimeUtc = ZDateTime.Empty; // cleanup

			// 1 closed child box
			selected.UpdateSelectedPackages(new[] { pallet1, pallet2 });
			var childBox = pallet1.Packages.AddNew();
			childBox.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			AssertPackagesAndPackedItems(selected, Array.Empty<PkgPackage>(), Array.Empty<PkgPackageItemDivotsWrapper>(), "Cannot Unpack because one or more Child Packages are Closed.");
			childBox.Delete(); // cleanup

			// box1 selected -- parent pallet is released
			selected.UpdateSelectedPackages(new[] { box1 });
			pallet1.KP_ReleasedTimeUtc = ZDateTime.UtcNow;
			AssertPackagesAndPackedItems(selected, Array.Empty<PkgPackage>(), Array.Empty<PkgPackageItemDivotsWrapper>(), "Cannot Unpack the selected BOX because it is packed onto a PLT that is already Released.");
			pallet1.KP_ReleasedTimeUtc = ZDateTime.Empty; // cleanup

			// items packed into box1 selected -- box is closed
			selected.UpdateSelectedPackages(Array.Empty<PkgPackage>());
			selected.UpdateSelectedPackedItems(new[] { box1PackedItem1, box1PackedItem2 });
			box1.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			AssertPackagesAndPackedItems(selected, Array.Empty<PkgPackage>(), Array.Empty<PkgPackageItemDivotsWrapper>(), "Cannot Unpack the selected BOX because it is already Closed.");
			box1.KP_ClosedTimeUtc = ZDateTime.Empty; // cleanup
		}

		void AssertPackagesAndPackedItems(SelectionHelper selected, IEnumerable<PkgPackage> expectedPackages, IEnumerable<PkgPackageItemDivotsWrapper> expectedPackedItems, string expectedNotification = "")
		{
			var items = selected.ItemsToUnpackBasedOnSelected();

			AssertContainsExactElementsInAnyOrder(expectedPackages, items.Packages);
			AssertContainsExactElementsInAnyOrder(expectedPackedItems, items.PackedItemsAndBarcodes.Select(dAb => dAb.PackedItem));
			AssertEquals(expectedNotification, items.ErrorMessage);

			foreach (var barcodeMatch in items.PackedItemsAndBarcodes.Select(dAb => dAb.Barcode))
			{
				AssertEquals(BarcodeMatch.Yes, barcodeMatch);
			}
		}

		#endregion

		#region TestItemsToUnpackBasedOnSelected_Barcode

		#region TestItemsToUnpackBasedOnSelected_Barcode_WhenPackedItemIsSelected

		public void TestItemsToUnpackBasedOnSelected_Barcode_WhenPackedItemIsSelected()
		{
			var itemNotInAvailableForUnpackMsg = "There is no single Package Selected from which to unpack any Speakers.";

			Data.CreatePackingData();

			var box = Data.PackageJob.Packages.AddNew("BOX");
			var packedItem = box.Pack_ForTesting(Data.DummyLine1, 1m);

			Data.PackageJob.Selected.UpdateSelectedPackedItems(new[] { packedItem });
			AssertItemsToUnpackBasedOnSelected_SinglePackageOrPackedItem(box, packedItem, itemNotInAvailableForUnpackMsg);
		}

		#endregion

		#region TestItemsToUnpackBasedOnSelected_Barcode_WhenPackageIsSelected

		public void TestItemsToUnpackBasedOnSelected_Barcode_WhenPackageIsSelected()
		{
			var itemNotInAvailableForUnpackMsg = "There is no Speakers packed into the Selected BOX.";

			Data.CreatePackingData();

			var box = Data.PackageJob.Packages.AddNew("BOX");
			var packedItem = box.Pack_ForTesting(Data.DummyLine1, 1m);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { box });
			AssertItemsToUnpackBasedOnSelected_SinglePackageOrPackedItem(box, packedItem, itemNotInAvailableForUnpackMsg);
		}

		#endregion

		#region TestItemsToUnPackBasedOnSelected_Barcode_WhenSelectionDoesNotMatchButParentPackageContainsChildMatch

		public void TestItemsToUnPackBasedOnSelected_Barcode_WhenSelectionDoesNotMatchButParentPackageContainsChildMatch()
		{
			Data.CreatePackingData();

			var dummyLine4 = Data.Dummy.Lines.AddNew();
			dummyLine4.Description = "Mouse";
			dummyLine4.DescriptionSupplement = "Model: M950";
			dummyLine4.TotalQty = 100;
			dummyLine4.TotalQtyUQ = "UNT";
			dummyLine4.AutoPackQtyPerPackage = 10;
			dummyLine4.AutoPackPackageType = "CTN";

			var box = Data.PackageJob.Packages.AddNew("BOX");
			var innerBox = box.Packages.AddNew("BOX");
			var packedItem = box.Pack_ForTesting(Data.DummyLine1, 1m);
			var nonMatchingPackedItem = box.Pack_ForTesting(dummyLine4, 1m);

			// non-matching packed item (alongside matching packed item) is selected
			Data.PackageJob.Selected.UpdateSelectedPackedItems(new[] { nonMatchingPackedItem });
			AssertItemsToUnpackBasedOnSelected_SinglePackageOrPackedItem(box, packedItem, "There is no single Package Selected from which to unpack any Speakers.");

			// non-matching package (alongside matching packed item) is selected
			Data.PackageJob.Selected.UpdateSelectedPackedItems(Enumerable.Empty<PkgPackageItemDivotsWrapper>());
			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { innerBox });
			AssertItemsToUnpackBasedOnSelected_SinglePackageOrPackedItem(box, packedItem, "There is no Speakers packed into the Selected BOX.");
		}

		#endregion

		#region TestItemsToUnpackBasedOnSelected_Barcode_WhenPackedItemIsSelected_MultipleDivots

		public void TestItemsToUnpackBasedOnSelected_Barcode_WhenPackedItemIsSelected_MultipleDivots()
		{
			var itemNotInAvailableForUnpackMsg = "There is no single Package Selected from which to unpack any Speakers.";

			Data.CreatePackingData();

			var box = Data.PackageJob.Packages.AddNew("BOX");
			var packedItem1 = box.Pack_ForTesting(Data.DummyLine1, 0.5m);
			var packedItem2 = box.Pack_ForTesting(Data.DummyLine1, 0.5m);
			AssertEquals("These wrappers should be same.", packedItem1, packedItem2);

			var dummyLine = Data.Dummy.Lines.AddNew();
			dummyLine.Barcode = "4";
			var packedItem3 = box.Pack_ForTesting(dummyLine, 1m);

			Data.PackageJob.Selected.UpdateSelectedPackedItems(new[] { packedItem1, packedItem3 });
			AssertItemsToUnpackBasedOnSelected_SinglePackageOrPackedItem(box, new[] { packedItem1 }, itemNotInAvailableForUnpackMsg);

			var packedItem4 = box.Pack_ForTesting(Data.DummyLine2, 1m);
			Data.PackageJob.Selected.UpdateSelectedPackedItems(new[] { packedItem1, packedItem4 });
			AssertItemsToUnpackBasedOnSelected_SinglePackageOrPackedItem(box, new[] { packedItem1, packedItem4 }, itemNotInAvailableForUnpackMsg);
		}

		#endregion

		#region AssertItemsToUnpack

		void AssertItemsToUnpackBasedOnSelected_SinglePackageOrPackedItem(PkgPackage box, PkgPackageItemDivotsWrapper packedItem, string itemNotInAvailableForUnpackMsg = "")
		{
			AssertItemsToUnpackBasedOnSelected_SinglePackageOrPackedItem(box, new[] { packedItem }, itemNotInAvailableForUnpackMsg);
		}

		void AssertItemsToUnpackBasedOnSelected_SinglePackageOrPackedItem(PkgPackage box, PkgPackageItemDivotsWrapper[] packedItems, string itemNotInAvailableForUnpackMsg = "")
		{
			var selected = Data.PackageJob.Selected;
			var barcodes = Data.Barcodes;
			ZString errorMessage;

			// assume selection of dummy line 1, scan dummy line 1
			var items = selected.ItemsToUnpackBasedOnSelected(barcodes.Dummy1And2Barcode, out errorMessage);
			AssertContainsExactElementsInAnyOrder("Selected Item + ParentPackage matches barcode, should be returned.", packedItems, items.Select(dAb => dAb.PackedItem));
			AssertEquals("", errorMessage);

			// scan dummy line 1 -- parent box is closed
			box.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			items = selected.ItemsToUnpackBasedOnSelected(barcodes.Dummy1And2Barcode, out errorMessage);
			AssertEquals("Selected Item's ParentPackage is Closed, should not be returned.", false, items.Any());
			AssertEquals("Cannot Unpack the selected BOX because it is already Closed.", errorMessage);
			box.KP_ClosedTimeUtc = ZDateTime.Empty; // cleanup

			// scan dummy line 1 -- parent box is released
			box.KP_ReleasedTimeUtc = ZDateTime.UtcNow;
			items = selected.ItemsToUnpackBasedOnSelected(barcodes.Dummy1And2Barcode, out errorMessage);
			AssertEquals("Selected Item's ParentPackage is Released, should not be returned.", false, items.Any());
			AssertEquals("Cannot Unpack the selected BOX because it is already Released.", errorMessage);
			box.KP_ReleasedTimeUtc = ZDateTime.Empty; // cleanup

			// scan dummy line 3
			items = selected.ItemsToUnpackBasedOnSelected(barcodes.Dummy3Barcode, out errorMessage);
			AssertEquals("Selected Item (DummyLine1) does not match barcode (DummyLine3), should not be returned.", false, items.Any());
			AssertEquals(itemNotInAvailableForUnpackMsg, errorMessage);

			// no selection
			selected.UpdateSelectedPackages(Array.Empty<PkgPackage>());
			selected.UpdateSelectedPackedItems(Array.Empty<PkgPackageItemDivotsWrapper>());
			items = selected.ItemsToUnpackBasedOnSelected(barcodes.Dummy1And2Barcode, out errorMessage);
			AssertEquals("Nothing is selected for unpack, should not return anything.", false, items.Any());
			AssertEquals("Select one or more Items or Packages to Unpack.", errorMessage);
		}

		#endregion

		#endregion

		#region TestItemsToUnpackBasedOnSelected_Barcode_TUN

		#region TestItemsToUnpackBasedOnSelected_Barcode_TUN_WhenPackedItemIsSelected

		public void TestItemsToUnpackBasedOnSelected_Barcode_TUN_WhenPackedItemIsSelected()
		{
			var errorMsg = "The scanned TUN Code '1+2-TUN' defines a BOX with 1x TV. This does not match the current selection.";

			Data.CreatePackingData();

			// setup valid tun for dummyline 1
			var box = Data.PackageJob.Packages.AddNew("BOX");
			var packedItem = box.Pack_ForTesting(Data.DummyLine1, 1m);

			Data.PackageJob.Selected.UpdateSelectedPackedItems(new[] { packedItem });
			AssertItemsToUnpackBasedOnSelected_TUN_SinglePackageOrPackedItem(box, packedItem, errorMsg);
		}

		#endregion

		#region TestItemsToUnpackBasedOnSelected_Barcode_TUN_WhenPackageIsSelected

		public void TestItemsToUnpackBasedOnSelected_Barcode_TUN_WhenPackageIsSelected()
		{
			var errorMsg = "The scanned TUN Code '1+2-TUN' defines a BOX with 1x TV. This does not match the current selection.";
			var errorMsgForDiffPackType = "The scanned TUN Code '1+2-TUN' defines a BOX with 1x TV. The Selected PLT does not contain a BOX that matches this definition.";

			Data.CreatePackingData();

			// setup valid tun for dummyline 1
			var box = Data.PackageJob.Packages.AddNew("BOX");
			var packedItem = box.Pack_ForTesting(Data.DummyLine1, 1m);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { box });
			AssertItemsToUnpackBasedOnSelected_TUN_SinglePackageOrPackedItem(box, packedItem, errorMsg, errorMsgForDiffPackType);
		}

		#endregion

		#region TestItemsToUnpackBasedOnSelected_Barcode_TUN_WhenPackageParentIsSelected

		public void TestItemsToUnpackBasedOnSelected_Barcode_TUN_WhenPackageParentIsSelected()
		{
			var errorMsg = "The scanned TUN Code '1+2-TUN' defines a BOX with 1x TV. The Selected CNT does not contain a BOX that matches this definition.";

			Data.CreatePackingData();

			// add a container that *contains* a valid tun for dummyline 1
			var cnt = Data.PackageJob.Packages.AddNew("CNT");
			var box = cnt.Packages.AddNew("BOX");
			var packedItem = box.Pack_ForTesting(Data.DummyLine1, 1m);

			// select the parent CNT
			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { cnt });
			AssertItemsToUnpackBasedOnSelected_TUN_SinglePackageOrPackedItem(box, packedItem, errorMsg);

			// add another valid tun that has an ID, thus:
			//	[CNT]
			//	  BOX1
			//	    1x DummyLine1
			//	  BOX2 (ID: abc)
			//	    1x DummyLine1
			var box2 = cnt.Packages.AddNew("BOX");
			box2.Pack(Data.DummyLine1, 1m);
			box2.KP_PackageID = "abc";

			ZString errorMessage;
			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { cnt });
			var items = Data.PackageJob.Selected.ItemsToUnpackBasedOnSelected(Data.Barcodes.Dummy1And2TUNBarcode, out errorMessage);
			AssertEquals("Selected Package has at least one child TUN with an ID, should not be returned.", false, items.Any());
			AssertEquals("One or more TUNs have a Package ID. Scan the ID or select the TUN to unpack, then re-scan the TUN barcode.", errorMessage);
		}

		#endregion

		#region TestItemsToUnpackBasedOnSelected_Barcode_TUN_WhenPackageParentIsSelected_MultipleResults

		public void TestItemsToUnpackBasedOnSelected_Barcode_TUN_WhenPackageParentIsSelected_MultipleResults()
		{
			Data.CreatePackingData();

			var selected = Data.PackageJob.Selected;
			var barcodes = Data.Barcodes;
			ZString errorMessage;

			// add a top-level pallet
			var plt = Data.PackageJob.Packages.AddNew("PLT");

			// add 3 valid tuns for dummyline 1 & 2
			var box1a = plt.Packages.AddNew("BOX");
			var packedItem1a = box1a.Pack_ForTesting(Data.DummyLine1, 1m);

			var box1b = plt.Packages.AddNew("BOX");
			var packedItem1b = box1b.Pack_ForTesting(Data.DummyLine1, 1m);

			var box2a = plt.Packages.AddNew("BOX");
			var packedItem2a = box2a.Pack_ForTesting(Data.DummyLine2, 1m);

			// add an empty box
			var boxEmpty = plt.Packages.AddNew("BOX");

			// add a random box
			var boxRnd = plt.Packages.AddNew("BOX");
			boxRnd.Pack(Data.DummyLine1, 1m);
			boxRnd.Pack(Data.DummyLine2, 1m);

			// select the parent PLT
			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { plt });

			// Scan All -- should locate either 1 of the 2 valid dummy1 boxes, as well as the dummy2 box (because it is differentiated by attribute)
			var items = selected.ItemsToUnpackBasedOnSelected(barcodes.Dummy1And2TUNBarcode, out errorMessage);
			var packedItems = items.Select(dAb => dAb.PackedItem);
			AssertEquals(2, packedItems.Count());
			AssertCollectionContains(packedItem2a, packedItems);
			AssertEquals("2 possible matches (packed items on Box1a, Box1b), should return either.", true, packedItems.Contains(packedItem1a) ^ packedItems.Contains(packedItem1b));

			// Scan Qty -- should locate all valid boxes
			items = selected.ItemsToUnpackBasedOnSelected(barcodes.Dummy1And2TUNBarcode, out errorMessage, isUserScanningQty: true);
			AssertContainsExactElementsInAnyOrder(new[] { packedItem1a, packedItem1b, packedItem2a }, items.Select(dAb => dAb.PackedItem));
		}

		#endregion

		#region TestItemsToUnpackBasedOnSelected_Barcode_TUN_WhenMultipleTUNsAreSelected

		public void TestItemsToUnpackBasedOnSelected_Barcode_TUN_WhenMultipleTUNsAreSelected()
		{
			Data.CreatePackingData();

			ZString errorMessage;

			var box1 = Data.PackageJob.Packages.AddNew("BOX");
			var box2 = Data.PackageJob.Packages.AddNew("BOX");
			box1.Pack(Data.DummyLine1, 1m);
			box2.Pack(Data.DummyLine1, 1m);

			// select the parent PLT
			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { box1, box2 });

			// Scan All -- should locate either 1 of the 2 valid dummy1 boxes, as well as the dummy2 box (because it is differentiated by attribute)
			var items = Data.PackageJob.Selected.ItemsToUnpackBasedOnSelected(Data.Barcodes.Dummy1And2TUNBarcode, out errorMessage);
			var packedItems = items.Select(dAb => dAb.PackedItem);
			AssertEquals(0, packedItems.Count());
			AssertEquals("Select a Single TUN or its Parent Package to Unpack the TUN.", errorMessage);
		}

		#endregion

		#region TestItemsToUnpackBasedOnSelected_Barcode_TUN_WhenNothingIsSelected

		public void TestItemsToUnpackBasedOnSelected_Barcode_TUN_WhenNothingIsSelected()
		{
			Data.CreatePackingData();
			var selected = Data.PackageJob.Selected;
			ZString errorMessage;

			// add a pallet that *contains* a valid tun for dummyline 1
			var pallet = Data.PackageJob.Packages.AddNew("PLT");
			var innerBox = pallet.Packages.AddNew("BOX");
			var innerPackedItem = innerBox.Pack(Data.DummyLine1, 1m);

			// nothing selected and no valid outers to unpack
			var items = selected.ItemsToUnpackBasedOnSelected(Data.Barcodes.Dummy1And2TUNBarcode, out errorMessage);
			AssertEquals(false, items.Any());
			AssertEquals("The scanned TUN Code '1+2-TUN' defines a BOX with 1x TV. No Outer matches this definition.", errorMessage);

			// add 2 valid tun outers
			var box1 = Data.PackageJob.Packages.AddNew("BOX");
			var box2 = Data.PackageJob.Packages.AddNew("BOX");
			var packedItem1 = box1.Pack_ForTesting(Data.DummyLine1, 1m);
			var packedItem2 = box2.Pack_ForTesting(Data.DummyLine1, 1m);

			// nothing selected - should unpack any single outer tun
			items = selected.ItemsToUnpackBasedOnSelected(Data.Barcodes.Dummy1And2TUNBarcode, out errorMessage);
			var packedItem = items.Single().PackedItem;
			AssertEquals("2 possible matches (packed items on Box1a, Box1b), should return either.", true, packedItem == packedItem1 || packedItem == packedItem2);
			AssertEquals("", errorMessage);

			// nothing selected - should unpack the outer tun
			items = selected.ItemsToUnpackBasedOnSelected(Data.Barcodes.Dummy1And2TUNBarcode, out errorMessage, isUserScanningQty: true);
			AssertContainsExactElementsInAnyOrder("Nothing or PackageJob Selected, should return outers matching barcode TUN.",
				new[] { packedItem1, packedItem2 }, items.Select(dAb => dAb.PackedItem));
			AssertEquals("", errorMessage);
		}

		#endregion

		#region Multiple Grouped Divots

		#region TestItemsToUnpackBasedOnSelected_Barcode_TUN_WhenPackedItemIsSelected_MultipleDivots

		public void TestItemsToUnpackBasedOnSelected_Barcode_TUN_WhenPackedItemIsSelected_MultipleDivots()
		{
			var errorMsg = "The scanned TUN Code '1+2-TUN' defines a BOX with 1x TV. This does not match the current selection.";

			Data.CreatePackingData();

			// setup valid tun for dummyline 1
			var box = Data.PackageJob.Packages.AddNew("BOX");
			var packedItem1 = box.Pack_ForTesting(Data.DummyLine1, 0.5m);
			var packedItem2 = box.Pack_ForTesting(Data.DummyLine1, 0.5m);
			AssertEquals("These wrappers should be same.", packedItem1, packedItem2);

			Data.PackageJob.Selected.UpdateSelectedPackedItems(new[] { packedItem1 });
			AssertItemsToUnpackBasedOnSelected_TUN_SinglePackageOrPackedItem(box, new[] { packedItem1 }, errorMsg);

			Data.PackageJob.Selected.UpdateSelectedPackedItems(new[] { packedItem1, packedItem2 }); // selecting multiple item to make "multiple packages or items selected" true.

			ZString errorMessage;
			var items = Data.PackageJob.Selected.ItemsToUnpackBasedOnSelected(Data.Barcodes.Dummy1And2TUNBarcode, out errorMessage);
			AssertEquals(0, items.Count());
			AssertEquals("Select a Single TUN or its Parent Package to Unpack the TUN.", errorMessage);
		}

		#endregion

		#region TestItemsToUnpackBasedOnSelected_Barcode_TUN_WhenPackageIsSelected_MultipleDivots

		public void TestItemsToUnpackBasedOnSelected_Barcode_TUN_WhenPackageIsSelected_MultipleDivots()
		{
			var errorMsg = "The scanned TUN Code '1+2-TUN' defines a BOX with 1x TV. This does not match the current selection.";
			var errorMsgForDiffPackType = "The scanned TUN Code '1+2-TUN' defines a BOX with 1x TV. The Selected PLT does not contain a BOX that matches this definition.";

			Data.CreatePackingData();

			// setup valid tun for dummyline 1
			var box = Data.PackageJob.Packages.AddNew("BOX");
			var packedItem1 = box.Pack_ForTesting(Data.DummyLine1, 0.5m);
			var packedItem2 = box.Pack_ForTesting(Data.DummyLine1, 0.5m);
			AssertEquals("These wrappers should be same.", packedItem1, packedItem2);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { box });
			AssertItemsToUnpackBasedOnSelected_TUN_SinglePackageOrPackedItem(box, new[] { packedItem1 }, errorMsg, errorMsgForDiffPackType);
		}

		#endregion

		#region TestItemsToUnpackBasedOnSelected_Barcode_TUN_WhenPackageParentIsSelected_MultipleDivots

		public void TestItemsToUnpackBasedOnSelected_Barcode_TUN_WhenPackageParentIsSelected_MultipleDivots()
		{
			var errorMsg = "The scanned TUN Code '1+2-TUN' defines a BOX with 1x TV. The Selected CNT does not contain a BOX that matches this definition.";

			Data.CreatePackingData();

			// add a container that *contains* a valid tun for dummyline 1
			var cnt = Data.PackageJob.Packages.AddNew("CNT");
			var box1 = cnt.Packages.AddNew("BOX");
			var packedItem1 = box1.Pack_ForTesting(Data.DummyLine1, 0.5m);
			var packedItem2 = box1.Pack_ForTesting(Data.DummyLine1, 0.5m);
			AssertEquals("These wrappers should be same.", packedItem1, packedItem2);

			// select the parent CNT
			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { cnt });
			AssertItemsToUnpackBasedOnSelected_TUN_SinglePackageOrPackedItem(box1, new[] { packedItem1 }, errorMsg);

			// add another valid tun that has an ID, thus:
			//	[CNT]
			//	  BOX1
			//	    1x DummyLine1
			//	  BOX2 (ID: abc)
			//	    1x DummyLine1
			var box2 = cnt.Packages.AddNew("BOX");
			box2.Pack(Data.DummyLine1, 1m);
			box2.KP_PackageID = "abc";

			ZString errorMessage;
			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { cnt });
			var items = Data.PackageJob.Selected.ItemsToUnpackBasedOnSelected(Data.Barcodes.Dummy1And2TUNBarcode, out errorMessage);
			AssertEquals("Selected Package has at least one child TUN with an ID, should not be returned.", false, items.Any());
			AssertEquals("One or more TUNs have a Package ID. Scan the ID or select the TUN to unpack, then re-scan the TUN barcode.", errorMessage);
		}

		#endregion

		#region TestItemsToUnpackBasedOnSelected_Barcode_TUN_WhenPackageParentIsSelected_MultipleDivotsAndResults

		public void TestItemsToUnpackBasedOnSelected_Barcode_TUN_WhenPackageParentIsSelected_MultipleDivotsAndResults()
		{
			Data.CreatePackingData();

			var selected = Data.PackageJob.Selected;
			var barcodes = Data.Barcodes;
			ZString errorMessage;

			// add a top-level pallet
			var plt = Data.PackageJob.Packages.AddNew("PLT");

			// add 3 valid tuns for dummyline 1 & 2
			var box1a = plt.Packages.AddNew("BOX");
			var packedItem1aa = box1a.Pack_ForTesting(Data.DummyLine1, 0.5m);
			var packedItem1ab = box1a.Pack_ForTesting(Data.DummyLine1, 0.5m);
			AssertEquals("These wrappers should be same.", packedItem1aa, packedItem1ab);

			var box1b = plt.Packages.AddNew("BOX");
			var packedItem1ba = box1b.Pack_ForTesting(Data.DummyLine1, 0.5m);
			var packedItem1bb = box1b.Pack_ForTesting(Data.DummyLine1, 0.5m);
			AssertEquals("These wrappers should be same.", packedItem1ba, packedItem1bb);

			var box2a = plt.Packages.AddNew("BOX");
			var packedItem2aa = box2a.Pack_ForTesting(Data.DummyLine2, 0.5m);
			var packedItem2ab = box2a.Pack_ForTesting(Data.DummyLine2, 0.5m);
			AssertEquals("These wrappers should be same.", packedItem2aa, packedItem2ab);

			// add an empty box
			var boxEmpty = plt.Packages.AddNew("BOX");

			// add a random box
			var boxRnd = plt.Packages.AddNew("BOX");
			boxRnd.Pack(Data.DummyLine1, 1m);
			boxRnd.Pack(Data.DummyLine2, 1m);

			// select the parent PLT
			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { plt });

			// Scan All -- should locate either 1 of the 2 valid dummy1 boxes, as well as the dummy2 box (because it is differentiated by attribute)
			var items = selected.ItemsToUnpackBasedOnSelected(barcodes.Dummy1And2TUNBarcode, out errorMessage);
			var packedItems = items.Select(dAb => dAb.PackedItem);
			AssertEquals(2, packedItems.Count());
			AssertCollectionContains(packedItem2aa, packedItems);
			AssertEquals("2 possible matches (packed items on Box1a, Box1b), should return either.", true,
				(packedItems.Contains(packedItem1aa) && packedItems.Contains(packedItem1ab)) ^ (packedItems.Contains(packedItem1ba) && packedItems.Contains(packedItem1bb)));

			// Scan Qty -- should locate all valid boxes
			items = selected.ItemsToUnpackBasedOnSelected(barcodes.Dummy1And2TUNBarcode, out errorMessage, isUserScanningQty: true);
			AssertContainsExactElementsInAnyOrder(new[] { packedItem1aa, packedItem1ba, packedItem2aa }, items.Select(dAb => dAb.PackedItem));
		}

		#endregion

		#region TestItemsToUnpackBasedOnSelected_Barcode_TUN_WhenNothingIsSelected_MultipleDivots

		public void TestItemsToUnpackBasedOnSelected_Barcode_TUN_WhenNothingIsSelected_MultipleDivots()
		{
			Data.CreatePackingData();
			var selected = Data.PackageJob.Selected;
			ZString errorMessage;

			// add a pallet that *contains* a valid tun for dummyline 1
			var pallet = Data.PackageJob.Packages.AddNew("PLT");
			var innerBox = pallet.Packages.AddNew("BOX");
			var innerPackedItem1 = innerBox.Pack(Data.DummyLine1, 0.5m);
			var innerPackedItem2 = innerBox.Pack(Data.DummyLine1, 0.5m);

			// nothing selected and no valid outers to unpack
			var items = selected.ItemsToUnpackBasedOnSelected(Data.Barcodes.Dummy1And2TUNBarcode, out errorMessage);
			AssertEquals(false, items.Any());
			AssertEquals("The scanned TUN Code '1+2-TUN' defines a BOX with 1x TV. No Outer matches this definition.", errorMessage);

			// add 2 valid tun outers
			var box1 = Data.PackageJob.Packages.AddNew("BOX");
			var box2 = Data.PackageJob.Packages.AddNew("BOX");
			var packedItem1a = box1.Pack_ForTesting(Data.DummyLine1, 0.5m);
			var packedItem1b = box1.Pack_ForTesting(Data.DummyLine1, 0.5m);
			AssertEquals("These wrappers should be same.", packedItem1a, packedItem1b);

			var packedItem2a = box2.Pack_ForTesting(Data.DummyLine1, 0.5m);
			var packedItem2b = box2.Pack_ForTesting(Data.DummyLine1, 0.5m);
			AssertEquals("These wrappers should be same.", packedItem2a, packedItem2b);

			// nothing selected - should unpack any single outer tun
			items = selected.ItemsToUnpackBasedOnSelected(Data.Barcodes.Dummy1And2TUNBarcode, out errorMessage);

			var packedItems = items.Select(i => i.PackedItem).ToArray();
			AssertEquals(1, items.Count());
			AssertEquals("1 possible matches (packed items on Box1a, Box1b), should return either.", true,
				(packedItems.Contains(packedItem1a) && packedItems.Contains(packedItem1b)) ^ (packedItems.Contains(packedItem2a) && packedItems.Contains(packedItem2b)));
			AssertEquals("", errorMessage);

			// nothing selected - should unpack the outer tun
			items = selected.ItemsToUnpackBasedOnSelected(Data.Barcodes.Dummy1And2TUNBarcode, out errorMessage, isUserScanningQty: true);
			AssertContainsExactElementsInAnyOrder("Nothing or PackageJob Selected, should return outers matching barcode TUN.",
				new[] { packedItem1a, packedItem2a }, items.Select(dAb => dAb.PackedItem));
			AssertEquals("", errorMessage);
		}

		#endregion

		#endregion

		#region AssertItemsToUnpack

		void AssertItemsToUnpackBasedOnSelected_TUN_SinglePackageOrPackedItem(PkgPackage box, PkgPackageItemDivotsWrapper packedItem, string expectedErrorMessage, string expectedErrorMessageForDiffPackType = "")
		{
			AssertItemsToUnpackBasedOnSelected_TUN_SinglePackageOrPackedItem(box, new[] { packedItem }, expectedErrorMessage, expectedErrorMessageForDiffPackType);
		}

		void AssertItemsToUnpackBasedOnSelected_TUN_SinglePackageOrPackedItem(PkgPackage box, PkgPackageItemDivotsWrapper[] packedItems, string expectedErrorMessage, string expectedErrorMessageForDiffPackType = "")
		{
			if (String.IsNullOrEmpty(expectedErrorMessageForDiffPackType))
			{
				expectedErrorMessageForDiffPackType = expectedErrorMessage;
			}

			var selected = Data.PackageJob.Selected;
			var barcodes = Data.Barcodes;
			ZString errorMessage;

			// assume selection of dummy line 1, scan dummy line 1 TUN
			var items = selected.ItemsToUnpackBasedOnSelected(barcodes.Dummy1And2TUNBarcode, out errorMessage);
			AssertEquals(packedItems.Length, items.Count());
			AssertContainsExactElementsInAnyOrder("Selected Item + ParentPackage matches barcode TUN, should be returned.", packedItems, items.Select(dAb => dAb.PackedItem));
			AssertEquals("", errorMessage);

			// scan dummy line 1 TUN -- packed item qty mismatch
			var packedItem = packedItems.First();
			var divot = packedItem.ParentPackage.PackedItemDivots.FirstOrDefault(d => d.PackedItem.Key == packedItem.Key);
			packedItem.SetDivotPackedQty(divot, divot.KI_PackedQty + 1);
			items = selected.ItemsToUnpackBasedOnSelected(barcodes.Dummy1And2TUNBarcode, out errorMessage);
			AssertEquals("Selected Item's Qty is != TUN qty, should not be returned.", false, items.Any());
			AssertEquals(expectedErrorMessage, errorMessage);
			packedItem.SetDivotPackedQty(divot, divot.KI_PackedQty - 1); // cleanup

			// scan dummy line 1 TUN -- parent package type mismatch
			box.KP_F3_NKPackType = "PLT";
			items = selected.ItemsToUnpackBasedOnSelected(barcodes.Dummy1And2TUNBarcode, out errorMessage);
			AssertEquals("Selected Item's ParentPackage type is != TUN type, should not be returned.", false, items.Any());
			AssertEquals(expectedErrorMessageForDiffPackType, errorMessage);
			box.KP_F3_NKPackType = "BOX"; // cleanup

			// scan dummy line 1 TUN -- parent package qty mismatch
			box.KP_PackageQty = 2;
			items = selected.ItemsToUnpackBasedOnSelected(barcodes.Dummy1And2TUNBarcode, out errorMessage);
			AssertEquals("Selected Item's ParentPackage Qty is > 1, should not be returned.", false, items.Any());
			AssertEquals(expectedErrorMessage, errorMessage);
			box.KP_PackageQty = 1; // cleanup

			// scan dummy line 1 TUN -- parent package contains other packages
			box.Packages.AddNew();
			items = selected.ItemsToUnpackBasedOnSelected(barcodes.Dummy1And2TUNBarcode, out errorMessage);
			AssertEquals("Selected Item's ParentPackage contains Packages, should not be returned.", false, items.Any());
			AssertEquals(expectedErrorMessage, errorMessage);
			box.Packages[0].Delete(); // cleanup

			// scan dummy line 1 TUN -- parent package contains other packed items
			var extraPackedItem = box.Pack_ForTesting(Data.DummyLine3, 1m);
			items = selected.ItemsToUnpackBasedOnSelected(barcodes.Dummy1And2TUNBarcode, out errorMessage);
			AssertEquals("Selected Item's ParentPackage contains other Items, should not be returned.", false, items.Any());
			AssertEquals(expectedErrorMessage, errorMessage);
			box.Unpack(extraPackedItem, 1); // cleanup

			// scan dummy line 1 TUN -- parent box is closed
			box.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			items = selected.ItemsToUnpackBasedOnSelected(barcodes.Dummy1And2TUNBarcode, out errorMessage);
			AssertEquals("Selected Item's ParentPackage is Closed, should not be returned.", false, items.Any());
			AssertEquals("Cannot Unpack the selected BOX because it is already Closed.", errorMessage);
			box.KP_ClosedTimeUtc = ZDateTime.Empty; // cleanup

			// scan dummy line 1 TUN -- parent box is released
			box.KP_ReleasedTimeUtc = ZDateTime.UtcNow;
			items = selected.ItemsToUnpackBasedOnSelected(barcodes.Dummy1And2TUNBarcode, out errorMessage);
			AssertEquals("Selected Item's ParentPackage is Released, should not be returned.", false, items.Any());
			AssertEquals("Cannot Unpack the selected BOX because it is already Released.", errorMessage);
			box.KP_ReleasedTimeUtc = ZDateTime.Empty; // cleanup

			// select dummy line 1 and scan dummy line 3 TUN
			items = selected.ItemsToUnpackBasedOnSelected(barcodes.Dummy3TUNBarcode, out errorMessage);
			AssertEquals("Selected Item (DummyLine1) does not match TUN barcode (DummyLine3), should not be returned.", false, items.Any());
			AssertStartsWith("", "The scanned TUN Code '3-TUN' defines a KEG with 30x Speakers.", errorMessage);

			// no selection
			selected.UpdateSelectedPackages(Array.Empty<PkgPackage>());
			selected.UpdateSelectedPackedItems(Array.Empty<PkgPackageItemDivotsWrapper>());
			items = selected.ItemsToUnpackBasedOnSelected(barcodes.Dummy1And2Barcode, out errorMessage);
			AssertEquals("Nothing is selected for unpack, should not return anything.", false, items.Any());
			AssertEquals("Select one or more Items or Packages to Unpack.", errorMessage);
		}

		#endregion

		#endregion

		#endregion
	}
}
