using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PackItemsBusinessObject))]
	public class PackItemsBusinessObjectTest : ItemsBusinessObjectTestCase<PackItemsBusinessObject>
	{
		#region Related Entities

		#region TestPackableItemParentsForBinding

		public void TestPackableItemParentsForBinding()
		{
			Data.CreatePackingData();
			Data.DummyLine1.TotalQty = 26;
			Data.DummyLine2.TotalQty = 6;
			Data.DummyLine3.TotalQty = 2012;

			// fully pack one item
			Data.PackageJob.Packages.AddNew().Pack(Data.DummyLine1, Data.DummyLine1.Base.TotalQty);

			var bizO = new PackItemsBusinessObject(Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2, Data.DummyLine3 }, Array.Empty<PkgPackage>());
			var wrappers = bizO.PackableItemParentsForBinding.Cast<PackableItemParentWrapper>();

			AssertEquals(2, wrappers.Count());
			wrappers.Single(w => w.PackableItemParent == Data.DummyLine2 && w.ProposedPackQty == 6m);
			wrappers.Single(w => w.PackableItemParent == Data.DummyLine3 && w.ProposedPackQty == 2012m);

			bizO.RefreshPackableItemParentsForBinding();
			AssertContainsExactElementsInAnyOrder("Wrappers should be cached (there is no need to recreate them).", wrappers, bizO.PackableItemParentsForBinding);
		}

		#endregion

		#region NewPackages

		public void TestNewPackages()
		{
			Data.CreatePackingData();
			var bizO = new PackItemsBusinessObject(Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2, Data.DummyLine3 }, Array.Empty<PkgPackage>());

			AssertEquals(false, bizO.NewPackages.Any());
		}

		#endregion

		#endregion

		#region Properties

		#region TestDefaultPackageTypeToCreate

		public void TestDefaultPackageTypeToCreate()
		{
			PackingRegistry.Instance.SetOuterPackageUnitForTest("PLT");
			AssertEquals("PLT", GetNewItemsBusinessObject().PackageTypeToCreate);

			PackingRegistry.Instance.SetOuterPackageUnitForTest("BOX");
			AssertEquals("BOX", GetNewItemsBusinessObject().PackageTypeToCreate);
		}

		#endregion

		#endregion

		#region Flags

		#region TestIsPackableAndErrorAndIntoExistingPackages

		public void TestIsPackableAndErrorAndIntoExistingPackages()
		{
			Data.CreatePackingData();
			var box1 = Data.PackageJob.Packages.AddNew("BOX");
			var box2 = Data.PackageJob.Packages.AddNew("BOX");

			const bool IS_SCAN_PACK = true;
			const bool IS_TUN = true;
			const bool IS_PACKABLE_INTO_EXISTING = true;

			// nothing to pack
			AssertItemPackability(Array.Empty<IPackableItemParent>(), Array.Empty<PkgPackage>(), !IS_SCAN_PACK, "Select one or more items to Pack.");

			// nothing to pack -- when Scan Packing
			AssertItemPackability(Array.Empty<IPackableItemParent>(), Array.Empty<PkgPackage>(), IS_SCAN_PACK, "Nothing to Pack.");

			// item is fully packed
			var packedItem1 = box1.Pack_ForTesting(Data.DummyLine1, Data.DummyLine1.Base.TotalQty);
			AssertItemPackability(new[] { Data.DummyLine1 }, Array.Empty<PkgPackage>(), !IS_SCAN_PACK, "All TV items are fully packed.");

			// multiple items are fully packed
			var packedItem2 = box2.Pack_ForTesting(Data.DummyLine2, Data.DummyLine2.Base.TotalQty);
			AssertItemPackability(new[] { Data.DummyLine1, Data.DummyLine2 }, Array.Empty<PkgPackage>(), !IS_SCAN_PACK, "The selected items are fully packed.");
			packedItem2.Delete(); // cleanup

			// item is fully packed -- when Scan Packing -- ensures we use the matched item description dummy1 instead of dummy3
			AssertItemPackability(new[] { Data.DummyLine3, Data.DummyLine1 }, Array.Empty<PkgPackage>(), IS_SCAN_PACK, "All TV items are fully packed.");

			AssertItemPackability(new[] { Data.DummyLine1 }, Array.Empty<PkgPackage>(), IS_SCAN_PACK, "All TV items are fully packed.");
			AssertItemPackability(new[] { Data.DummyLine1 }, Array.Empty<PkgPackage>(), IS_SCAN_PACK, "The scanned TUN Code '1+2-TUN' defines a BOX with 1x TV but none are available.", !IS_PACKABLE_INTO_EXISTING, IS_TUN);
			packedItem1.Delete(); // cleanup

			// multiple packages selected and one or more is CLOSED / RELEASED / RELEASED VIA JOB
			box2.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			AssertItemPackability(new[] { Data.DummyLine1 }, new[] { box1, box2 }, !IS_SCAN_PACK, "Cannot Pack into the selected Packages because one or more Packages are Closed or Released.");
			box2.KP_ClosedTimeUtc = ZDateTime.Empty; // cleanup

			// multiple packages selected -- Scan Packing
			AssertItemPackability(new[] { Data.DummyLine1 }, new[] { box1, box2 }, IS_SCAN_PACK, "Select a Single Package to pack into and then try again.");

			// the selected package is closed, released etc.
			box1.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			AssertItemPackability(new[] { Data.DummyLine1 }, new[] { box1 }, !IS_SCAN_PACK, "Cannot Pack into the selected BOX because it is already Closed.");

			// the selected package is closed, released etc. -- Scan Packing (can pack into new)
			AssertItemPackability(new[] { Data.DummyLine1 }, new[] { box1 }, IS_SCAN_PACK, "");
			box1.KP_ClosedTimeUtc = ZDateTime.Empty; // cleanup

			// can pack ok

			// 0 packages selected
			AssertItemPackability(new[] { Data.DummyLine1 }, Array.Empty<PkgPackage>(), !IS_SCAN_PACK, ""); // can pack into new via menu

			// 0 packages selected -- Scan Packing
			AssertItemPackability(new[] { Data.DummyLine1 }, Array.Empty<PkgPackage>(), IS_SCAN_PACK, ""); // scan pack will auto-pack into new

			// can pack into existing
			AssertItemPackability(new[] { Data.DummyLine1 }, new[] { box1 }, IS_SCAN_PACK, "", IS_PACKABLE_INTO_EXISTING);
			AssertItemPackability(new[] { Data.DummyLine1 }, new[] { box1 }, !IS_SCAN_PACK, "", IS_PACKABLE_INTO_EXISTING);
			AssertItemPackability(new[] { Data.DummyLine1 }, new[] { box1, box2 }, !IS_SCAN_PACK, "", IS_PACKABLE_INTO_EXISTING);

			// 1 item is fully packed but another is not
			packedItem1 = box1.Pack_ForTesting(Data.DummyLine1, Data.DummyLine1.Base.TotalQty);
			AssertItemPackability(new[] { Data.DummyLine1, Data.DummyLine2 }, Array.Empty<PkgPackage>(), !IS_SCAN_PACK, "");
			packedItem1.Delete(); // cleanup
		}

		public void TestIsPackableAndErrorAndIntoExistingPackages_PackableItemParentWithNoPackableItems()
		{
			Data.CreatePackingData();
			const bool IS_SCAN_PACK = true;

			// no packable items in  packable item parent
			var lineWithEmptyPackableItem = Data.Dummy.Lines.AddNew();
			((DummyPackableItemParent)lineWithEmptyPackableItem).RemoveItem((DummyPackableItem)lineWithEmptyPackableItem.PackableItems.First());
			AssertEquals("Precondition: Packable item parent has no packable items.", 0, lineWithEmptyPackableItem.PackableItems.Count());

			AssertItemPackability(new[] { lineWithEmptyPackableItem }, Array.Empty<PkgPackage>(), !IS_SCAN_PACK, "There are no items to pack. Someone has made changes to the items to pack. Please close and re-open the form.");
			AssertItemPackability(new[] { lineWithEmptyPackableItem }, Array.Empty<PkgPackage>(), IS_SCAN_PACK, "There are no items to pack. Someone has made changes to the items to pack. Please close and re-open the form.");
		}

		public void TestIsPackableAndErrorAndIntoExistingPackages_WhenPackedItemSelected()
		{
			Data.CreatePackingData();
			var box = Data.PackageJob.Packages.AddNew("BOX");
			var innerCtn = box.Packages.AddNew("CTN");
			const bool IS_SCAN_PACK = true;

			// select a packed item -- should make everything non-packable
			var packedItem = box.Pack(Data.DummyLine1, 7m);
			Data.PackageJob.Selected.UpdateSelectedPackedItems(packedItem);

			// can pack into new via menu
			AssertItemPackability(new[] { Data.DummyLine1 }, Array.Empty<PkgPackage>(), !IS_SCAN_PACK, "");
			AssertItemPackability(new[] { Data.DummyLine1 }, new[] { innerCtn }, !IS_SCAN_PACK, "", expectIsPackableIntoExisting: true);

			// scan pack should *not* pack into new or selected package when a packed item is also selected
			AssertItemPackability(new[] { Data.DummyLine1 }, Array.Empty<PkgPackage>(), IS_SCAN_PACK, "Select a Single Package to pack into and then try again.");
			AssertItemPackability(new[] { Data.DummyLine1 }, new[] { innerCtn }, IS_SCAN_PACK, "Select a Single Package to pack into and then try again.");
		}

		void AssertItemPackability(IEnumerable<IPackableItemParent> itemsToPack, IReadOnlyList<PkgPackage> packagesToPackInto, bool isScanPacking, ZString expectedErrorMessage, bool expectIsPackableIntoExisting = false, bool isTUN = false)
		{
			var bizO = isScanPacking
				? new PackItemsViaScanBusinessObject(isTUN ? Data.Barcodes.Dummy1And2TUNBarcode : Data.Barcodes.Dummy1And2Barcode, Data.PackageJob, itemsToPack, packagesToPackInto)
				: new PackItemsBusinessObject(Data.PackageJob, itemsToPack, packagesToPackInto);

			// test IsPackable
			AssertEquals("IsPackable should be true if no error otherwise false.", bizO.IsPackable, bizO.IsPackableErrorMessage.IsEmpty);

			// test IsPackableErrorMessage
			AssertEquals(expectedErrorMessage, bizO.IsPackableErrorMessage);

			// test IsPackableIntoExistingPackages
			AssertEquals(bizO.IsPackableIntoExistingPackages, expectIsPackableIntoExisting);
			if (expectIsPackableIntoExisting)
			{
				AssertEquals("If can Pack into Existing Packages, IsPackable should always be true.", true, bizO.IsPackable);
			}
		}

		#endregion

		#region TestIsScanPacking

		public void TestIsScanPacking()
		{
			AssertEquals(false, GetNewItemsBusinessObject().IsScanPacking);
		}

		#endregion

		#region TestIsPackingAlongsideExistingPackage

		public void TestIsPackingAlongsideExistingPackage()
		{
			AssertEquals(false, GetNewItemsBusinessObject().IsPackingAlongsideExistingPackage);
		}

		#endregion

		#endregion

		#region RunValidationAndApplyChanges

		#region TestRunValidationAndApplyChanges_WithNewPackages

		public void TestRunValidationAndApplyChanges_WithNewPackages()
		{
			Data.CreatePackingData();
			Data.PackageJob.LastUsedOuterPackType = "PLT";

			var packageCountChangedInvoked = false;
			var divotCountChangedInvoked = false;
			Data.PackageJob.Packages.CollectionCountChange += (sender, e) =>
			{
				packageCountChangedInvoked = true;
				AssertEquals("Mass Package Process should be running when Packing.", true, Data.PackageJob.IsMassPackageProcessRunning);
				((PkgPackage)e.BizObject).PackedItemDivots.CountChanged += (x, y) =>
				{
					divotCountChangedInvoked = true;
					AssertEquals("Mass Package Process should be running when Packing.", true, Data.PackageJob.IsMassPackageProcessRunning);
				};
			};

			// pack 30 of Line1 and 100 of Line2 into 3 new pallets
			var bizO = new PackItemsBusinessObject(Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2 }, Array.Empty<PkgPackage>());
			bizO.PackableItemParentsForBinding.FindByPackableItemParent(Data.DummyLine1).ProposedPackQty = 30m;
			bizO.PackageQtyToCreate = 3;
			AssertEquals("Mass Package Process should *not* be running before Packing.", false, Data.PackageJob.IsMassPackageProcessRunning);

			bizO.RunValidationAndApplyChanges();
			AssertRunValidationAndApplyChanges(Data.PackageJob);
			AssertEquals("Mass Package Process should *not* be running after Packing.", false, Data.PackageJob.IsMassPackageProcessRunning);
			AssertEquals("Mass Package Process assertions should have run.", true, packageCountChangedInvoked);
			AssertEquals("Mass Package Process assertions should have run.", true, divotCountChangedInvoked);
		}

		#endregion

		#region TestRunValidationAndApplyChanges_WithNewPackages_SetsNewPackagesProperty

		public void TestRunValidationAndApplyChanges_WithNewPackages_SetsNewPackagesProperty()
		{
			Data.CreatePackingData();
			Data.PackageJob.LastUsedOuterPackType = "PLT";

			// pack 100 of Line1 and 100 of Line2 into 2 new pallets
			var bizO = new PackItemsBusinessObject(Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2 }, Array.Empty<PkgPackage>());
			bizO.PackageQtyToCreate = 2;

			bizO.RunValidationAndApplyChanges();

			var packageCount =
			(
				from p in bizO.NewPackages
				where p.PackedItemDivots.Count == 2
				where p.PackedItemDivots.FindByPackableItemParent_ForTesting(Data.DummyLine1).KI_PackedQty == 50
				where p.PackedItemDivots.FindByPackableItemParent_ForTesting(Data.DummyLine2).KI_PackedQty == 50
				select p
			).Count();
			AssertEquals(2, packageCount);
		}

		#endregion

		#region TestApplyChanges_WithExistingPackages

		public void TestApplyChanges_WithExistingPackages()
		{
			Data.CreatePackingData();
			Data.PackageJob.LastUsedOuterPackType = "PLT";

			// create 3 empty pallets
			var pallet1 = Data.PackageJob.Packages.AddNew();
			var pallet2 = Data.PackageJob.Packages.AddNew();
			var pallet3 = Data.PackageJob.Packages.AddNew();

			// pack 30 of Line1 and 100 of Line2 into the 3 pallets
			var items = new[] { Data.DummyLine1, Data.DummyLine2 };
			var packages = new[] { pallet1, pallet2, pallet3 };
			var bizO = new PackItemsBusinessObject(Data.PackageJob, items, packages);
			bizO.PackableItemParentsForBinding.FindByPackableItemParent(Data.DummyLine1).ProposedPackQty = 30m;

			bizO.RunValidationAndApplyChanges();
			AssertEquals(false, bizO.NewPackages.Any());
			AssertRunValidationAndApplyChanges(Data.PackageJob);
		}

		#endregion

		#region TestApplyChanges_WithExistingPackages_Container

		public void TestApplyChanges_WithExistingPackages_Container()
		{
			Data.CreatePackingData();
			Data.PackageJob.LastUsedOuterPackType = "CNT";

			// create empty container
			var container = Data.PackageJob.Packages.AddNew();
			AssertEquals("Precondition", true, container.IsContainer);

			// pack container
			var items = new[] { Data.DummyLine1, Data.DummyLine2 };
			var packages = new[] { container };
			var bizO = new PackItemsBusinessObject(Data.PackageJob, items, packages);
			AssertEquals("Should pass validation and apply changes.", true, bizO.RunValidationAndApplyChanges());
			AssertEquals(false, bizO.NewPackages.Any());
			AssertEquals("Should have packed the items.", 2, container.PackedItemDivots.Count);
		}

		#endregion

		void AssertRunValidationAndApplyChanges(PkgPackageJob packageJob)
		{
			// ensure we have:
			//
			// 2x Pallet
			//    10x Item1
			//    33x Item2 
			//
			// 1x Pallet
			//    10x Item1
			//    34x Item2 (33 + remainder of 1)
			//
			AssertEquals(3, packageJob.Packages.Count);

			int palletsWith10OfItem1 = 0;
			int palletsWith33OfItem2 = 0;
			int palletsWith34OfItem2 = 0;

			foreach (var pallet in packageJob.Packages)
			{
				AssertEquals("PLT", pallet.KP_F3_NKPackType);

				var packedItem1 = pallet.PackedItemDivots.Single(d => d.GetPackableItemParent() == Data.DummyLine1);
				if (packedItem1.KI_PackedQty == 10)
				{
					palletsWith10OfItem1++;
				}

				var packedItem2 = pallet.PackedItemDivots.Single(d => d.GetPackableItemParent() == Data.DummyLine2);
				if (packedItem2.KI_PackedQty == 33)
				{
					palletsWith33OfItem2++;
				}

				if (packedItem2.KI_PackedQty == 34)
				{
					palletsWith34OfItem2++;
				}
			}

			AssertEquals("There should be 3 Pallets with 10 of Item 1 in each.", 3, palletsWith10OfItem1);
			AssertEquals("There should be 2 Pallets with 33 of Item 2 in each.", 2, palletsWith33OfItem2);
			AssertEquals("There should be 1 Pallet with 33 + 1 (remainder) of Item 2 in each.", 1, palletsWith34OfItem2);
		}

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

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewItemsBusinessObject();
		}

		protected override PackItemsBusinessObject GetNewItemsBusinessObject()
		{
			Data.CreatePackingData();
			return new PackItemsBusinessObject(Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2, Data.DummyLine3 }, Array.Empty<PkgPackage>());
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
