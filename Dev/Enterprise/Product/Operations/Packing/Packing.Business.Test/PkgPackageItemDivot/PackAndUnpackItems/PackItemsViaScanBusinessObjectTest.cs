using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PackItemsViaScanBusinessObject))]
	public class PackItemsViaScanBusinessObjectTest : ItemsBusinessObjectTestCase<PackItemsViaScanBusinessObject>
	{
		#region Related Entities

		#region PackableItemParentsForBinding

		#region TestPackableItemParentsForBinding

		public void TestPackableItemParentsForBinding()
		{
			Data.CreatePackingData();

			// create with all packable items. this tests that we match barcodes when building the PackableItemParentsForBinding collection.
			var bizO = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy3Barcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2, Data.DummyLine3 }, null);
			var wrappers = bizO.PackableItemParentsForBinding.Cast<PackableItemParentWrapper>();

			AssertEquals("Of the 3 items only 1 should match the barcode.", 1, wrappers.Count());
			AssertNotNull(wrappers.First(w => w.PackableItemParent == Data.DummyLine3 && w.ProposedPackQty == 1m));
		}

		#endregion

		#region TestPackableItemParentsForBinding_Attribs

		public void TestPackableItemParentsForBinding_Attribs()
		{
			Data.CreatePackingData();

			var bizO = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2Barcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2 }, null);
			var wrappers = bizO.PackableItemParentsForBinding.Cast<PackableItemParentWrapper>();
			AssertContainsExactElementsInAnyOrder(new[] { Data.DummyLine1, Data.DummyLine2 }, wrappers.Select(w => w.PackableItemParent));

			((IScanningItemsBusinessObject)bizO).AddFilterIfValidAttribute("attr1");
			AssertContainsExactElementsInAnyOrder(new[] { Data.DummyLine1 }, wrappers.Select(w => w.PackableItemParent));
		}

		#endregion

		#region TestPackableItemParentsForBinding_Attribs_Qty

		public void TestPackableItemParentsForBinding_Attribs_Qty()
		{
			Data.CreatePackingData();

			var bizO = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2Barcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2 }, null, isPackingQty: true);
			var wrappers = bizO.PackableItemParentsForBinding.Cast<PackableItemParentWrapper>();

			AssertEquals(2, wrappers.Count());
			wrappers.Single(w => w.PackableItemParent == Data.DummyLine1 && w.ProposedPackQty == 100m);
			wrappers.Single(w => w.PackableItemParent == Data.DummyLine2 && w.ProposedPackQty == 100m);

			((IScanningItemsBusinessObject)bizO).AddFilterIfValidAttribute("attr1");
			AssertEquals(1, wrappers.Count());
			wrappers.Single(w => w.PackableItemParent == Data.DummyLine1 && w.ProposedPackQty == 100m);
		}

		#endregion

		#region TestPackableItemParentsForBinding_Qty

		public void TestPackableItemParentsForBinding_Qty()
		{
			Data.CreatePackingData();

			var bizO = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy3Barcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2, Data.DummyLine3 }, null, isPackingQty: true);
			var wrappers = bizO.PackableItemParentsForBinding.Cast<PackableItemParentWrapper>();

			AssertEquals("Of the 3 items only 1 should match the barcode.", 1, wrappers.Count());
			AssertNotNull(wrappers.First(w => w.PackableItemParent == Data.DummyLine3 && w.ProposedPackQty == 100m));
		}

		#endregion

		#region TestPackableItemParentsForBinding_TUN

		public void TestPackableItemParentsForBinding_TUN()
		{
			Data.CreatePackingData();

			// create with all packable items. this tests that we match barcodes when building the PackableItemParentsForBinding collection.
			var bizO = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy3TUNBarcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2, Data.DummyLine3 }, null);
			var wrappers = bizO.PackableItemParentsForBinding.Cast<PackableItemParentWrapper>();

			AssertEquals("Of the 3 items only 1 should match the barcode.", 1, wrappers.Count());
			AssertNotNull(wrappers.First(w => w.PackableItemParent == Data.DummyLine3 && w.ProposedPackQty == 30));
		}

		#endregion

		#region TestPackableItemParentsForBinding_TUN_Qty

		public void TestPackableItemParentsForBinding_TUN_Qty()
		{
			Data.CreatePackingData();

			// create with all packable items. this tests that we match barcodes when building the PackableItemParentsForBinding collection.
			var bizO = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy3TUNBarcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2, Data.DummyLine3 }, null, isPackingQty: true);
			bizO.PackageQtyToCreate = 3;
			var wrappers = bizO.PackableItemParentsForBinding.Cast<PackableItemParentWrapper>();

			AssertEquals("Of the 3 items only 1 should match the barcode.", 1, wrappers.Count());
			AssertNotNull(wrappers.First(w => w.PackableItemParent == Data.DummyLine3 && w.ProposedPackQty == 3 * 30m));
		}

		#endregion

		#region TestPackableItemParentsForBinding_TUN_Attribs_Qty

		public void TestPackableItemParentsForBinding_TUN_Attribs_Qty()
		{
			Data.CreatePackingData();

			// create with all packable items. this tests that we match barcodes when building the PackableItemParentsForBinding collection.
			var bizO = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2TUNBarcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2 }, null, isPackingQty: true);
			bizO.PackageQtyToCreate = 3;
			var wrappers = bizO.PackableItemParentsForBinding.Cast<PackableItemParentWrapper>();

			AssertEquals(2, wrappers.Count());
			wrappers.Single(w => w.PackableItemParent == Data.DummyLine1 && w.ProposedPackQty == 3 * 1m);
			wrappers.Single(w => w.PackableItemParent == Data.DummyLine2 && w.ProposedPackQty == 0m);

			((IScanningItemsBusinessObject)bizO).AddFilterIfValidAttribute("attr2");
			AssertEquals(1, wrappers.Count());
			wrappers.Single(w => w.PackableItemParent == Data.DummyLine2 && w.ProposedPackQty == 3 * 1m);
		}

		#endregion

		#region TestPackableItemParentsForBinding_ItemsFullyPackedOrNotEnoughForTUN

		public void TestPackableItemParentsForBinding_ItemsFullyPackedOrNotEnoughForTUN()
		{
			Data.CreatePackingData();

			// create with all packable items. this tests that we match barcodes when building the PackableItemParentsForBinding collection.
			var box = Data.PackageJob.Packages.AddNew("BOX");
			box.Pack(Data.DummyLine2, 99.5m);

			var bizO = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2TUNBarcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2, Data.DummyLine3 }, null);
			var wrappers = bizO.PackableItemParentsForBinding.Cast<PackableItemParentWrapper>();

			AssertContainsExactElementsInAnyOrder("Should only contain item1 because there are only 0.5x item2 (the TUN is 1), and item3 does not match the barcode.",
				new[] { Data.DummyLine1 }, wrappers.Select(w => w.PackableItemParent));
		}

		#endregion

		#endregion

		#endregion

		#region Properties

		#region TestDefaultPackageTypeToCreate

		public void TestDefaultPackageTypeToCreate()
		{
			PackingRegistry.Instance.SetOuterPackageUnitForTest("PLT");
			Data.CreatePackingData();

			// Box TUN code
			var bizO1 = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2TUNBarcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2 }, null);
			AssertEquals("Precondition", "BOX", ((IScanningItemsBusinessObject)bizO1).TUNCode);
			AssertEquals("Default Package Type to Create should come from the TUN Code.", "BOX", bizO1.PackageTypeToCreate);

			// Keg TUN code
			var bizO2 = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy3TUNBarcode, Data.PackageJob, new[] { Data.DummyLine3 }, null);
			AssertEquals("Precondition", "KEG", ((IScanningItemsBusinessObject)bizO2).TUNCode);
			AssertEquals("Default Package Type to Create should come from the TUN Code.", "KEG", bizO2.PackageTypeToCreate);

			// not a TUN scan, should fall back to the LastOuterPack
			var bizO3 = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy3Barcode, Data.PackageJob, new[] { Data.DummyLine3 }, null);
			AssertEquals("Precondition", "", ((IScanningItemsBusinessObject)bizO3).TUNCode);
			AssertEquals("Default Package Type to Create should come from the registry.", "PLT", bizO3.PackageTypeToCreate);
		}

		#endregion

		#region TestPackageQtyToCreate_RecalcsProposedPackQtys

		public void TestPackageQtyToCreate_RecalcsProposedPackQtys()
		{
			Data.CreatePackingData();

			var bizO = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2TUNBarcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2 }, null);
			var wrappers = bizO.PackableItemParentsForBinding.Cast<PackableItemParentWrapper>();
			var wrapper1 = wrappers.Single(w => w.PackableItemParent == Data.DummyLine1);
			var wrapper2 = wrappers.Single(w => w.PackableItemParent == Data.DummyLine2);

			AssertEquals("Precondition", 1, bizO.PackageQtyToCreate);
			AssertEquals("Precondition", 1m, wrapper1.ProposedPackQty);
			AssertEquals("Precondition", 0m, wrapper2.ProposedPackQty);

			bizO.PackageQtyToCreate = 3;
			AssertEquals(3m, wrapper1.ProposedPackQty);
			AssertEquals(0m, wrapper2.ProposedPackQty);

			bizO.PackageQtyToCreate = -5;
			AssertEquals(0m, wrapper1.ProposedPackQty);
			AssertEquals(0m, wrapper2.ProposedPackQty);
		}

		public void TestPackageQtyToCreate_RecalcsProposedPackQtys_Attribs()
		{
			Data.CreatePackingData();
			Data.DummyLine1.BarcodeTUNPackQty = 5;
			Data.DummyLine2.BarcodeTUNPackQty = 5;

			var bizO = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2TUNBarcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2 }, null, isPackingQty: true);
			var wrappers = bizO.PackableItemParentsForBinding.Cast<PackableItemParentWrapper>();
			var wrapper1 = wrappers.Single(w => w.PackableItemParent == Data.DummyLine1);
			var wrapper2 = wrappers.Single(w => w.PackableItemParent == Data.DummyLine2);

			bizO.PackageQtyToCreate = 3;
			AssertEquals(3 * 5m, wrapper1.ProposedPackQty);
			AssertEquals(0m, wrapper2.ProposedPackQty);

			((IScanningItemsBusinessObject)bizO).SelectPackableItemParent(wrapper2);
			AssertEquals("Selecting one item should not result in clearing of others, because the Pack will pack the selection only anyway.", 0m, wrapper1.ProposedPackQty);
			AssertEquals("Selecting one item should not result in clearing of others, because the Pack will pack the selection only anyway.", 3 * 5m, wrapper2.ProposedPackQty);
		}

		#endregion

		#endregion

		#region Flags

		#region TestCanAutoApplyChanges

		public void TestCanAutoApplyChanges()
		{
			Data.CreatePackingData();

			var barcode = Data.Barcodes.Dummy1And2Barcode;
			var packageJob = Data.PackageJob;

			var item1 = Data.DummyLine1;
			var item2 = Data.DummyLine2;

			AssertEquals(false, new PackItemsViaScanBusinessObject(barcode, packageJob, Array.Empty<IPackableItemParent>(), Array.Empty<PkgPackage>()).CanAutoApplyChanges);
			AssertEquals(false, new PackItemsViaScanBusinessObject(barcode, packageJob, Array.Empty<IPackableItemParent>(), Array.Empty<PkgPackage>(), isPackingQty: true).CanAutoApplyChanges);
			AssertEquals(true, new PackItemsViaScanBusinessObject(barcode, packageJob, new[] { item1 }, Array.Empty<PkgPackage>()).CanAutoApplyChanges);
			AssertEquals(false, new PackItemsViaScanBusinessObject(barcode, packageJob, new[] { item1 }, Array.Empty<PkgPackage>(), isPackingQty: true).CanAutoApplyChanges);
			AssertEquals(false, new PackItemsViaScanBusinessObject(barcode, packageJob, new[] { item1, item2 }, Array.Empty<PkgPackage>()).CanAutoApplyChanges);
			AssertEquals(false, new PackItemsViaScanBusinessObject(barcode, packageJob, new[] { item1, item2 }, Array.Empty<PkgPackage>(), isPackingQty: true).CanAutoApplyChanges);
		}

		#endregion

		#region TestCanScanPackQuanity

		public void TestCanScanPackQuanity()
		{
			Data.CreatePackingData();

			var barcode = Data.Barcodes.Dummy1And2Barcode;
			var packageJob = Data.PackageJob;
			var item1 = Data.DummyLine1;
			var item2 = Data.DummyLine2;

			AssertEquals(false, new PackItemsViaScanBusinessObject(barcode, packageJob, Array.Empty<IPackableItemParent>(), Array.Empty<PkgPackage>()).CanScanPackQuanity);
			AssertEquals("Has no items, so can't scan QTY.", false, new PackItemsViaScanBusinessObject(barcode, packageJob, Array.Empty<IPackableItemParent>(), Array.Empty<PkgPackage>(), isPackingQty: true).CanScanPackQuanity);
			AssertEquals(false, new PackItemsViaScanBusinessObject(barcode, packageJob, new[] { item1 }, Array.Empty<PkgPackage>()).CanScanPackQuanity);
			AssertEquals(true, new PackItemsViaScanBusinessObject(barcode, packageJob, new[] { item1 }, Array.Empty<PkgPackage>(), isPackingQty: true).CanScanPackQuanity);
			AssertEquals(false, new PackItemsViaScanBusinessObject(barcode, packageJob, new[] { item1, item2 }, Array.Empty<PkgPackage>()).CanScanPackQuanity);
			AssertEquals("Has more than One item, so can't scan QTY.", false, new PackItemsViaScanBusinessObject(barcode, packageJob, new[] { item1, item2 }, Array.Empty<PkgPackage>(), isPackingQty: true).CanScanPackQuanity);
		}

		#endregion

		#region TestHasItemMatchingBarcode

		public void TestHasItemMatchingBarcode()
		{
			Data.CreatePackingData();
			AssertEquals(true, new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2Barcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine3 }, null).HasItemMatchingBarcode);
			AssertEquals(false, new PackItemsViaScanBusinessObject("dodgy", Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2, Data.DummyLine3 }, null).HasItemMatchingBarcode);
			AssertEquals(false, new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2Barcode, Data.PackageJob, new[] { Data.DummyLine3 }, null).HasItemMatchingBarcode);

			Data.PackageJob.Packages.AddNew().Pack(Data.DummyLine1, Data.DummyLine1.Base.TotalQty); // fully-packed
			AssertEquals("Even though there is nothing left to pack, the barcode should still be valid.", true,
				new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2Barcode, Data.PackageJob, new[] { Data.DummyLine1 }, null).HasItemMatchingBarcode);
		}

		#endregion

		#region TestIsAnythingSelectedToPackOrUnpack

		public void TestIsAnythingSelectedToPackOrUnpack()
		{
			Data.CreatePackingData();
			var bizO = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2Barcode, Data.PackageJob, new[] { Data.DummyLine1 }, null);

			bizO.PackableItemParentsForBinding[0].ProposedPackQty = 0;
			AssertEquals("Proposed Pack Qty is 0, should not be packable.", false, bizO.IsAnythingSelectedToPackOrUnpack);

			bizO.PackableItemParentsForBinding[0].ProposedPackQty = 1;
			AssertEquals(true, bizO.IsAnythingSelectedToPackOrUnpack);
		}

		public void TestIsAnythingSelectedToPackOrUnpack_Attribs()
		{
			Data.CreatePackingData();
			var bizO = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2Barcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2 }, null);
			AssertEquals("No distinct selection, should not be packable.", false, bizO.IsAnythingSelectedToPackOrUnpack);

			((IScanningItemsBusinessObject)bizO).SelectPackableItemParent(bizO.PackableItemParentsForBinding[0]);
			AssertEquals("Selection is distinct, should be packable.", true, bizO.IsAnythingSelectedToPackOrUnpack);
		}

		#endregion

		#region TestIsPackable

		public void TestIsPackable()
		{
			Data.CreatePackingData();
			var box1 = Data.PackageJob.Packages.AddNew("BOX");
			var box2 = Data.PackageJob.Packages.AddNew("BOX");

			Data.DummyLine1.BarcodeTUNPackQty = 5;
			Data.DummyLine2.BarcodeTUNPackQty = 5;

			// 4 item 1 available, 3 item 2 available, cannot know which qty to state, should say "not enough" (tests rare case of tun + attribs)
			box1.Pack(Data.DummyLine1, Data.DummyLine1.Base.TotalQty - 4m);
			box2.Pack(Data.DummyLine2, Data.DummyLine2.Base.TotalQty - 2m);
			AssertItemPackability(new[] { Data.DummyLine1, Data.DummyLine2 }, Array.Empty<PkgPackage>(), "The scanned TUN Code '1+2-TUN' defines a BOX with 5x TV but not enough are available.");

			// 4 available
			AssertItemPackability(new[] { Data.DummyLine1 }, Array.Empty<PkgPackage>(), "The scanned TUN Code '1+2-TUN' defines a BOX with 5x TV but only 4 are available.");

			// 1 available
			box1.Pack(Data.DummyLine1, 3m);
			AssertItemPackability(new[] { Data.DummyLine1 }, Array.Empty<PkgPackage>(), "The scanned TUN Code '1+2-TUN' defines a BOX with 5x TV but only 1 is available.");

			// 0.5 available
			box1.Pack(Data.DummyLine1, 0.5m);
			AssertItemPackability(new[] { Data.DummyLine1 }, Array.Empty<PkgPackage>(), "The scanned TUN Code '1+2-TUN' defines a BOX with 5x TV but only 0.5 are available.");

			// 0 available
			box1.Pack(Data.DummyLine1, 0.5m);
			AssertItemPackability(new[] { Data.DummyLine1 }, Array.Empty<PkgPackage>(), "The scanned TUN Code '1+2-TUN' defines a BOX with 5x TV but none are available.");

			// not TUN, should use base messages instead
			AssertItemPackability(new[] { Data.DummyLine1 }, Array.Empty<PkgPackage>(), "All TV items are fully packed.", isTUN: false);
		}

		void AssertItemPackability(IEnumerable<IPackableItemParent> itemsToPack, IReadOnlyList<PkgPackage> packagesToPackInto, ZString expectedErrorMessage, bool isTUN = true)
		{
			var bizO = new PackItemsViaScanBusinessObject(isTUN ? Data.Barcodes.Dummy1And2TUNBarcode : Data.Barcodes.Dummy1And2Barcode, Data.PackageJob, itemsToPack, packagesToPackInto);

			AssertEquals("IsPackable should be true if no error otherwise false.", bizO.IsPackable, bizO.IsPackableErrorMessage.IsEmpty);
			AssertEquals(expectedErrorMessage, bizO.IsPackableErrorMessage);
		}

		#endregion

		#region TestIsScanPacking

		public void TestIsScanPacking()
		{
			AssertEquals(true, new PackItemsViaScanBusinessObject("123", Factory.New<PkgPackageJob>(), Array.Empty<IPackableItemParent>(), Array.Empty<PkgPackage>()).IsScanPacking);
		}

		#endregion

		#endregion

		#region Mode

		public void TestMode()
		{
			Data.CreatePackingData();

			var bizO = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2Barcode, Data.PackageJob, new[] { Data.DummyLine1 }, null);
			AssertEquals(PackOrUnpackMode.SingleItemNoQty, bizO.Mode);

			var bizOWithAttribs = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2Barcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2 }, null);
			AssertEquals(PackOrUnpackMode.Attribs, bizOWithAttribs.Mode);

			var bizOWithAttribsTUN = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2TUNBarcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2 }, null, isPackingQty: true);
			AssertEquals(PackOrUnpackMode.TUN_Qty_Attribs, bizOWithAttribsTUN.Mode);
		}

		#endregion

		#region RunValidationAndApplyChanges

		#region TestRunValidationAndApplyChanges_WithNewPackages_UsingBarcodes

		public void TestRunValidationAndApplyChanges_WithNewPackages_UsingBarcodes()
		{
			Data.CreatePackingData();
			Data.PackageJob.LastUsedOuterPackType = "PLT";

			// pack 1x item2 into a new PLT
			var bizO = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2Barcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2 }, null);
			var wrappers = bizO.PackableItemParentsForBinding.Cast<PackableItemParentWrapper>();

			((IScanningItemsBusinessObject)bizO).SelectPackableItemParent(wrappers.Single(w => w.PackableItemParent == Data.DummyLine2)); // simulate user choosing to pack item 2
			bizO.RunValidationAndApplyChanges();

			// ensure we have:
			//
			// 1x PLT
			//    1x Item2
			//
			var pallet = Data.PackageJob.Packages.Single(p => p.KP_F3_NKPackType == "PLT" && p.KP_PackageQty == 1);
			var divot = pallet.PackedItemDivots.Single(i => i.GetPackableItemParent() == Data.DummyLine2 && i.KI_PackedQty == 1m);

			// ensure nothing else was packed
			AssertContainsExactElementsInAnyOrder("PackageJob should have only 1 Pallet.", new[] { pallet }, Data.PackageJob.Packages);
			AssertEquals("Pallet should not have any Packages.", false, pallet.Packages.Any());
			AssertContainsExactElementsInAnyOrder("Pallet should have only 1 Packed Item.", new[] { divot }, pallet.PackedItemDivots);

			// ensure the NewPackages property is updated with the newly created pallet
			AssertContainsExactElementsInAnyOrder(new[] { pallet }, bizO.NewPackages);
		}

		#endregion

		#region TestRunValidationAndApplyChanges_WithNewPackages_UsingBarcodesWithPackTypeAndQty

		public void TestRunValidationAndApplyChanges_WithNewPackages_UsingBarcodesWithPackTypeAndQty()
		{
			Data.CreatePackingData();
			Data.PackageJob.LastUsedOuterPackType = "PLT";

			// pack 1x item2 into a BOX
			var bizO = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2TUNBarcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2 }, null);
			var wrappers = bizO.PackableItemParentsForBinding.Cast<PackableItemParentWrapper>();

			((IScanningItemsBusinessObject)bizO).SelectPackableItemParent(wrappers.Single(w => w.PackableItemParent == Data.DummyLine2)); // simulate user choosing to pack item 2
			bizO.RunValidationAndApplyChanges();

			// ensure we have:
			//
			//  1x BOX
			//    1x Item2
			//
			var box = Data.PackageJob.Packages.Single(p => p.KP_F3_NKPackType == "BOX" && p.KP_PackageQty == 1);
			var divot = box.PackedItemDivots.Single(i => i.GetPackableItemParent() == Data.DummyLine2 && i.KI_PackedQty == 1m);

			// ensure nothing else was packed
			AssertContainsExactElementsInAnyOrder("PackageJob should have only 1 BOX.", new[] { box }, Data.PackageJob.Packages);
			AssertContainsExactElementsInAnyOrder("BOX should have only 1 Packed Item.", new[] { divot }, box.PackedItemDivots);
			AssertEquals("BOX should not have any Packages.", false, box.Packages.Any());

			// ensure the NewPackages property is updated with the newly created box
			AssertContainsExactElementsInAnyOrder(new[] { box }, bizO.NewPackages);
		}

		#endregion

		#region TestRunValidationAndApplyChanges_WithNewPackage_UsingBarcodesWithPackTypeAndQty_ScanQtyMode

		public void TestRunValidationAndApplyChanges_WithNewPackage_UsingBarcodesWithPackTypeAndQty_ScanQtyMode()
		{
			Data.CreatePackingData();

			var bizO = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy3TUNBarcode, Data.PackageJob, new[] { Data.DummyLine3 }, null);

			// attempt to pack 4x KEG each with 30 of item3 (only 100 available)
			bizO.PackageQtyToCreate = 4;
			bizO.RunValidationAndApplyChanges();
			AssertEquals("Attempt to pack 4 KEGs of 30 units but only 100 units were available, should not have packed anything.", false, bizO.NewPackages.Any());
			AssertEquals(false, Data.PackageJob.Packages.Any());

			// pack 3x KEG each with 30 of item3
			bizO.PackageQtyToCreate = 3;
			bizO.RunValidationAndApplyChanges();

			// ensure we have:
			//
			// 1x KEG
			//   30x Item3
			// 1x KEG
			//	 30x Item3
			// 1x KEG
			//	 30x Item3
			//
			var expectedKegs =
			(
				from p in Data.PackageJob.Packages
				where p.KP_F3_NKPackType == "KEG"
					 && p.PackedItemDivots.Count == 1
					 && p.PackedItemDivots[0].GetPackableItemParent() == Data.DummyLine3
					 && p.PackedItemDivots[0].KI_PackedQty == 30m
				select p
			);
			AssertContainsExactElementsInAnyOrder(expectedKegs, Data.PackageJob.Packages);
			AssertContainsExactElementsInAnyOrder("New Packages should contain all created Packages after MultiPack.", expectedKegs, bizO.NewPackages);
		}

		#endregion

		#region TestRunValidationAndApplyChanges_WithExistingPackage_UsingBarcodes

		public void TestRunValidationAndApplyChanges_WithExistingPackage_UsingBarcodes()
		{
			Data.CreatePackingData();
			var pallet = Data.PackageJob.Packages.AddNew("PLT");

			// pack 1x item2 into the existing PLT
			var bizO = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2Barcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2 }, new[] { pallet });
			var wrappers = bizO.PackableItemParentsForBinding.Cast<PackableItemParentWrapper>();

			((IScanningItemsBusinessObject)bizO).SelectPackableItemParent(wrappers.Single(w => w.PackableItemParent == Data.DummyLine2)); // simulate user choosing to pack item 2
			bizO.RunValidationAndApplyChanges();

			// ensure we have:
			//
			// 1x PLT
			//    1x Item2
			//
			var divot = pallet.PackedItemDivots.Single(i => i.GetPackableItemParent() == Data.DummyLine2 && i.KI_PackedQty == 1m);

			// ensure nothing else was packed
			AssertContainsExactElementsInAnyOrder("PackageJob should have only 1 PLT.", new[] { pallet }, Data.PackageJob.Packages);
			AssertEquals("PLT should not have any Packages.", false, pallet.Packages.Any());
			AssertContainsExactElementsInAnyOrder("PLT should have only 1 Packed Item.", new[] { divot }, pallet.PackedItemDivots);
		}

		#endregion

		#region TestRunValidationAndApplyChanges_WithExistingPackage_UsingBarcodesWithPackTypeAndQty

		public void TestRunValidationAndApplyChanges_WithExistingPackage_UsingBarcodesWithPackTypeAndQty()
		{
			Data.CreatePackingData();
			var pallet = Data.PackageJob.Packages.AddNew("PLT");

			// pack 30x item3 into a KEG (on the PLT)
			var bizO = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy3TUNBarcode, Data.PackageJob, new[] { Data.DummyLine3 }, new[] { pallet });
			bizO.RunValidationAndApplyChanges();

			// ensure we have:
			//
			// 1x PLT
			//    1x KEG
			//		30x Item3
			//
			var keg = pallet.Packages.Single(p => p.KP_F3_NKPackType == "KEG" && p.KP_PackageQty == 1);
			var divot = keg.PackedItemDivots.Single(i => i.GetPackableItemParent() == Data.DummyLine3 && i.KI_PackedQty == 30m);

			// ensure nothing else was packed
			AssertContainsExactElementsInAnyOrder("PackageJob should have only 1 PLT.", new[] { pallet }, Data.PackageJob.Packages);

			AssertEquals("PLT should not have any Packed Items.", false, pallet.PackedItemDivots.Any());
			AssertContainsExactElementsInAnyOrder("PLT should have only 1 KEG.", new[] { keg }, pallet.Packages);

			AssertEquals("KEG should not have any Packages.", false, keg.Packages.Any());
			AssertContainsExactElementsInAnyOrder("KEG should have only 1 Packed Item.", new[] { divot }, keg.PackedItemDivots);

			// ensure correct state for subcribers
			AssertEquals(false, bizO.IsPackingAlongsideExistingPackage);
		}

		#endregion

		#region TestRunValidationAndApplyChanges_WithExistingPackage_UsingBarcodesWithPackTypeAndQty_ScanQtyMode

		public void TestRunValidationAndApplyChanges_WithExistingPackage_UsingBarcodesWithPackTypeAndQty_ScanQtyMode()
		{
			Data.CreatePackingData();
			var pallet = Data.PackageJob.Packages.AddNew("PLT");

			var bizO = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy3TUNBarcode, Data.PackageJob, new[] { Data.DummyLine3 }, new[] { pallet });

			// attempt to pack 4x KEG each with 30 of item3 (only 100 available)
			bizO.PackageQtyToCreate = 4;
			bizO.RunValidationAndApplyChanges();
			AssertEquals("Attempt to pack 4 KEGs of 30 units but only 100 units were available, should not have packed anything.", false, bizO.NewPackages.Any());
			AssertContainsExactElementsInAnyOrder(new[] { pallet }, Data.PackageJob.Packages);
			AssertEquals(false, pallet.Packages.Any());
			AssertEquals(false, pallet.PackedItemDivots.Any());

			// pack 3x KEG each with 30 of item3
			bizO.PackageQtyToCreate = 3;
			bizO.RunValidationAndApplyChanges();

			// ensure we have:
			//
			// 1x PLT
			//    1x KEG
			//		30x Item3
			//    1x KEG
			//		30x Item3
			//    1x KEG
			//		30x Item3
			//
			var expectedKegs =
			(
				from p in pallet.Packages
				where p.KP_F3_NKPackType == "KEG"
					 && p.PackedItemDivots.Count == 1
					 && p.PackedItemDivots[0].GetPackableItemParent() == Data.DummyLine3
					 && p.PackedItemDivots[0].KI_PackedQty == 30m
				select p
			);
			AssertContainsExactElementsInAnyOrder(expectedKegs, pallet.Packages);
			AssertContainsExactElementsInAnyOrder("New Packages should contain all created Packages after MultiPack.", expectedKegs, bizO.NewPackages);

			// ensure nothing else was packed
			AssertEquals(1, Data.PackageJob.Packages.Count);
			AssertEquals(false, pallet.PackedItemDivots.Any());

			// ensure correct state for subcribers
			AssertEquals(false, bizO.IsPackingAlongsideExistingPackage);
		}

		#endregion

		#region TestRunValidationAndApplyChanges_WithExistingPackage_UsingBarcodesWithPackTypeAndQty_PackBoxIntoBox

		public void TestRunValidationAndApplyChanges_WithExistingPackage_UsingBarcodesWithPackTypeAndQty_PackBoxIntoBox()
		{
			Data.CreatePackingData();
			var box1 = Data.PackageJob.Packages.AddNew("BOX");

			// pack 1x item2 into a BOX
			var bizO = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2TUNBarcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2 }, new[] { box1 });
			var wrappers = bizO.PackableItemParentsForBinding.Cast<PackableItemParentWrapper>();
			((IScanningItemsBusinessObject)bizO).SelectPackableItemParent(wrappers.Single(w => w.PackableItemParent == Data.DummyLine2)); // simulate user choosing to pack item 2
			bizO.RunValidationAndApplyChanges();

			// ensure we have:
			//
			// 1x BOX (original)
			// 1x BOX
			//   1x Item2
			//
			var box2 = Data.PackageJob.Packages.Single(p => p.KP_F3_NKPackType == "BOX" && p.KP_PackageQty == 1 && p.PackedItemDivots.Any());
			var divot = box2.PackedItemDivots.Single(i => i.GetPackableItemParent() == Data.DummyLine2 && i.KI_PackedQty == 1m);

			// ensure nothing else was packed
			AssertContainsExactElementsInAnyOrder("PackageJob should have 2x BOX.", new[] { box1, box2 }, Data.PackageJob.Packages);
			AssertEquals("Existing BOX should not have any Packed Items.", false, box1.PackedItemDivots.Any());
			AssertEquals("Existing BOX should not have any Packages.", false, box1.Packages.Any());
			AssertEquals("New BOX should not have any Packages.", false, box2.Packages.Any());
			AssertContainsExactElementsInAnyOrder("New BOX should have only 1 Packed Item.", new[] { divot }, box2.PackedItemDivots);

			// ensure correct state for subcribers
			AssertEquals(true, bizO.IsPackingAlongsideExistingPackage);
		}

		#endregion

		#endregion

		#region IScanningItemsBusinessObject

		#region TestIScanningItemsBusinessObject_SelectPackableItemParent

		public void TestIScanningItemsBusinessObject_SelectPackableItemParent()
		{
			IScanningItemsBusinessObject bizO = GetNewItemsBusinessObject();
			AssertExceptionThrown("Test the call-through to the scan helper.", typeof(ArgumentException), () => bizO.SelectPackableItemParent(Data.DummyLine3Wrapper));
		}

		#endregion

		#region TestIScanningItemsBusinessObject_IsUserEnteringQty

		public void TestIScanningItemsBusinessObject_IsUserEnteringQty()
		{
			Data.CreatePackingData();

			IScanningItemsBusinessObject bizO1 = new PackItemsViaScanBusinessObject("123", Data.PackageJob, new[] { Data.DummyLine1 }, null);
			AssertEquals(false, bizO1.IsUserEnteringQty);

			IScanningItemsBusinessObject bizO2 = new PackItemsViaScanBusinessObject("123", Data.PackageJob, new[] { Data.DummyLine1 }, null, isPackingQty: true);
			AssertEquals(true, bizO2.IsUserEnteringQty);
		}

		#endregion

		#region TestIScanningItemsBusinessObject_IsTUN

		public void TestIScanningItemsBusinessObject_IsTUN()
		{
			Data.CreatePackingData();

			IScanningItemsBusinessObject bizO1 = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2Barcode, Data.PackageJob, new[] { Data.DummyLine1 }, null);
			AssertEquals(false, bizO1.IsTUN);

			IScanningItemsBusinessObject bizO2 = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2TUNBarcode, Data.PackageJob, new[] { Data.DummyLine1 }, null);
			AssertEquals(true, bizO2.IsTUN);
		}

		#endregion

		#region TestIScanningItemsBusinessObject_TUNCode

		public void TestIScanningItemsBusinessObject_TUNCode()
		{
			Data.CreatePackingData();
			Data.DummyLine1.BarcodeTUNPackQty = 5;

			// no current barcode
			IScanningItemsBusinessObject bizO1 = new PackItemsViaScanBusinessObject("NOTATUN", Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2 }, null);
			AssertEquals(true, bizO1.TUNCode.IsEmpty);

			// BOX TUN code
			IScanningItemsBusinessObject bizO2 = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2TUNBarcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2 }, null);
			AssertEquals("BOX", bizO2.TUNCode);

			// BOX TUN Code -- not enough to pack the TUN, but TUNCode should be retained
			Data.PackageJob.Packages.AddNew("BOX").Pack(Data.DummyLine1, Data.DummyLine1.Base.TotalQty - 4m);
			IScanningItemsBusinessObject bizO3 = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2TUNBarcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2 }, null);
			AssertEquals("BOX", bizO3.TUNCode);

			// valid barcode but not a TUN
			IScanningItemsBusinessObject bizO4 = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2Barcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2 }, null);
			AssertEquals(true, bizO4.TUNCode.IsEmpty);

			// TUN code that does not match any of the packable items
			IScanningItemsBusinessObject bizO5 = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy3TUNBarcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2 }, null);
			AssertEquals(true, bizO5.TUNCode.IsEmpty);
		}

		#endregion

		#region TestIScanningItemsBusinessObject_AddFilterIfValidAttribute

		public void TestIScanningItemsBusinessObject_AddFilterIfValidAttribute()
		{
			Data.CreatePackingData();

			var bizO = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2Barcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2 }, null);
			var wrappers = bizO.PackableItemParentsForBinding.Cast<PackableItemParentWrapper>();
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { Data.DummyLine1, Data.DummyLine2 }, wrappers.Select(w => w.PackableItemParent));

			AssertEquals(true, ((IScanningItemsBusinessObject)bizO).AddFilterIfValidAttribute("attr1"));
			AssertContainsExactElementsInAnyOrder(new[] { Data.DummyLine1 }, wrappers.Select(w => w.PackableItemParent));
		}

		#endregion

		#region TestIScanningItemsBusinessObject_GetAllWrappers

		public void TestIScanningItemsBusinessObject_GetAllWrappers()
		{
			PackingRegistry.Instance.SetOuterPackageUnitForTest("PLT");
			Data.CreatePackingData();
			Data.DummyLine1.BarcodeTUNPackQty = 5;

			var bizO1 = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2TUNBarcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2 }, null);
			var wrappers = ((IScanningItemsBusinessObject)bizO1).GetAllWrappers();
			wrappers.Single(w => w.PackableItemParent == Data.DummyLine1 && w.ProposedPackQty == 5m);
			wrappers.Single(w => w.PackableItemParent == Data.DummyLine2 && w.ProposedPackQty == 0m);
			AssertEquals("BOX", bizO1.PackageTypeToCreate);

			var bizO2 = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2Barcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2 }, null);
			((IScanningItemsBusinessObject)bizO2).GetAllWrappers();
			AssertEquals(PackingRegistry.Instance.OuterPackageUnit.Value, bizO2.PackageTypeToCreate);
		}

		#endregion

		#region TestIScanningItemsBusinessObject_GetAllWrappers_OneAtATime

		public void TestIScanningItemsBusinessObject_GetAllWrappers_OneAtATime()
		{
			Data.CreatePackingData();

			var bizO1 = new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2TUNBarcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2 }, null);
			var wrappers = ((IScanningItemsBusinessObject)bizO1).GetAllWrappers();
			AssertEquals(2, wrappers.Count());
			AssertEquals(1, wrappers.Count(w => w.ProposedPackQty == 1m));
		}

		#endregion

		#endregion

		#region TestSetPackOrRemoveQuantity

		public override void TestSetPackOrRemoveQuantity()
		{
			var bizO = GetNewItemsBusinessObject();
			var wrapper = bizO.PackableItemParentsForBinding[0];
			bizO.SetPackOrRemoveQuantity(wrapper, 10m);
			AssertEquals(10m, wrapper.ProposedPackQty);
		}

		#endregion

		#region Implementation

		protected override PackItemsViaScanBusinessObject GetNewItemsBusinessObject()
		{
			Data.CreatePackingData();
			return new PackItemsViaScanBusinessObject(Data.Barcodes.Dummy1And2Barcode, Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2 }, null);
		}

		protected override ZString ProposedColumnName
		{
			get { return PackableItemParentWrapper.Schema.ProposedPackQty; }
		}

		protected override ZString ProposedDescription
		{
			get { return "Pack"; }
		}

		protected override Type ValidationType
		{
			get { return typeof(PackItemsBusinessObjectValidation); }
		}

		#endregion
	}
}
