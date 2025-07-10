using System;
using System.Linq;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(UnpackItemsViaScanBusinessObject))]
	public class UnpackItemsViaScanBusinessObjectTest : ItemsBusinessObjectTestCase<UnpackItemsViaScanBusinessObject>
	{
		#region Related Entities

		#region PackableItemParentsForBinding

		#region TestPackableItemParentsForBinding

		/// <summary>
		/// This also tests IScanningItemsBusinessObject.GetAllWrappers().
		/// </summary>
		public void TestPackableItemParentsForBinding()
		{
			Data.CreatePackingData();

			var box = Data.PackageJob.Packages.AddNew("BOX");

			var packedItems = new[]
			{
				box.Pack_ForTesting(Data.DummyLine1, 26m),
				box.Pack_ForTesting(Data.DummyLine2, 6m),
			};
			var packedItemsAndBarcodes = WrapPackedItemsWithBarcodeYes(packedItems);

			var bizO = new UnpackItemsViaScanBusinessObject(Data.PackageJob, packedItemsAndBarcodes, isUnpackingQty: false);
			var wrappers = bizO.PackableItemParentsForBinding.Cast<PackableItemParentWrapper>();
			AssertEquals("PackableItemParents should contain all packed items (as they are always unique when not unpacking TUNs).", 2, wrappers.Count());
			AssertEquals(true, bizO.PackableItemParentsForBinding.All<PackableItemParentWrapper>(w => w.ProposedRemoveQty == 1m));
		}

		#endregion

		#region TestPackableItemParentsForBinding_TUN

		public void TestPackableItemParentsForBinding_TUN()
		{
			AssertPackableItemParentsForBinding_TUN(isUnpackingQty: false);
			AssertPackableItemParentsForBinding_TUN(isUnpackingQty: true);
		}

		void AssertPackableItemParentsForBinding_TUN(bool isUnpackingQty) // no attribs
		{
			Data.CreatePackingData();

			var packedItems = new[]
			{
				Data.PackageJob.Packages.AddNew("KEG").Pack_ForTesting(Data.DummyLine3, Data.Barcodes.Dummy3TUNPackQty),
				Data.PackageJob.Packages.AddNew("KEG").Pack_ForTesting(Data.DummyLine3, Data.Barcodes.Dummy3TUNPackQty),
				Data.PackageJob.Packages.AddNew("KEG").Pack_ForTesting(Data.DummyLine3, Data.Barcodes.Dummy3TUNPackQty)
			};
			var barcodeMatch = new BarcodeMatch(true, Data.Barcodes.Dummy3TUNPackType, Data.Barcodes.Dummy3TUNPackQty);
			var packedItemsWithBarcodes = packedItems.Select(d => new PkgPackageItemDivotsWrapperAndBarcode(d, barcodeMatch));

			var bizO = new UnpackItemsViaScanBusinessObject(Data.PackageJob, packedItemsWithBarcodes, isUnpackingQty);
			AssertEquals("PackableItemParents should be updated on load to match Pack to Create Qty.", 1, bizO.PackableItemParentsForBinding.Count);
			AssertEquals(30m, bizO.PackableItemParentsForBinding[0].ProposedRemoveQty); // Data.Barcodes.Dummy3TUNPackQty = 30

			// visible packed items should not change, even when unpacking qty and changing the qty value
			bizO.PackageQtyToCreate = 3;
			var expectedCount = 1;
			AssertEquals(expectedCount, bizO.PackableItemParentsForBinding.Count);
			AssertEquals(true, bizO.PackableItemParentsForBinding.All<PackableItemParentWrapper>(w => w.ProposedRemoveQty == 30m));

			// clean up
			Array.ForEach(packedItems, d => d.Delete());
		}

		#endregion

		#region TestPackableItemParentsForBinding_TUN_Attributes + ScanQty

		public void TestPackableItemParentsForBinding_TUN_Attributes()
		{
			AssertPackableItemParentsForBinding_TUN_Attributes_ScanQty(isUnpackingQty: false);
		}

		public void TestPackableItemParentsForBinding_TUN_Attributes_ScanQty()
		{
			AssertPackableItemParentsForBinding_TUN_Attributes_ScanQty(isUnpackingQty: true);
		}

		void AssertPackableItemParentsForBinding_TUN_Attributes_ScanQty(bool isUnpackingQty)
		{
			var tunQty = isUnpackingQty ? 5 : 1; // when entering qty, ensure the TUN qty is > 1 to adequately test that remove Qty is set to the TUN qty.

			Data.CreatePackingData();
			Data.DummyLine1.BarcodeTUNPackQty = tunQty;
			Data.DummyLine2.BarcodeTUNPackQty = tunQty;

			var packedItems = new[]
			{
				Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine1, tunQty),
				Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine1, tunQty),
				Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine2, tunQty) // line 2
			};
			var barcodeMatch = new BarcodeMatch(true, Data.Barcodes.Dummy1And2TUNPackType, tunQty);
			var packedItemsWithBarcodes = packedItems.Select(d => new PkgPackageItemDivotsWrapperAndBarcode(d, barcodeMatch));

			var bizO = new UnpackItemsViaScanBusinessObject(Data.PackageJob, packedItemsWithBarcodes, isUnpackingQty);
			var wrappers = bizO.PackableItemParentsForBinding.Cast<PackableItemParentWrapper>();
			wrappers.Single(w => w.PackableItemParent == Data.DummyLine1); //
			wrappers.Single(w => w.PackableItemParent == Data.DummyLine2); // PackableItemParents should be reduced to the distinct item by attribute.
			AssertEquals(true, wrappers.All(w => w.ProposedRemoveQty == tunQty)); // Data.Barcodes.Dummy1And2TUNPackQty = 1
		}

		#endregion

		#endregion

		#endregion

		#region Properties

		#region TestDefaultPackageTypeToCreate

		public void TestDefaultPackageTypeToCreate()
		{
			AssertPackageType(b => b.PackageTypeToCreate);
		}

		#endregion

		#region TestTUNCode

		public void TestTUNCode()
		{
			AssertPackageType(b => ((IScanningItemsBusinessObject)b).TUNCode);
		}

		#endregion

		#region TestTUNCodeAvailableCount

		public void TestTUNCodeAvailableCount()
		{
			Data.CreatePackingData();
			var barcodes = Data.Barcodes;

			var packedItem1 = Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine1, barcodes.Dummy1And2TUNPackQty);
			var dAb1 = new PkgPackageItemDivotsWrapperAndBarcode(packedItem1, new BarcodeMatch(true, barcodes.Dummy1And2TUNPackType, barcodes.Dummy1And2TUNPackQty));
			var bizO1 = new UnpackItemsViaScanBusinessObject(Data.PackageJob, new[] { dAb1 });
			AssertEquals(1, bizO1.TUNCodeAvailableCount);

			var packedItem2 = Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine1, barcodes.Dummy1And2TUNPackQty);
			var dAb2 = new PkgPackageItemDivotsWrapperAndBarcode(packedItem2, new BarcodeMatch(true, barcodes.Dummy1And2TUNPackType, barcodes.Dummy1And2TUNPackQty));
			var bizO2 = new UnpackItemsViaScanBusinessObject(Data.PackageJob, new[] { dAb1, dAb2 });
			AssertEquals(2, bizO2.TUNCodeAvailableCount);
		}

		public void TestTUNCodeCount_Attribs()
		{
			Data.CreatePackingData();

			var packedItems = new[]
			{
				Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine1, Data.Barcodes.Dummy1And2TUNPackQty),
				Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine1, Data.Barcodes.Dummy1And2TUNPackQty),
				Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine2, Data.Barcodes.Dummy1And2TUNPackQty) // line 2
			};
			var barcodeMatch = new BarcodeMatch(true, Data.Barcodes.Dummy1And2TUNPackType, Data.Barcodes.Dummy1And2TUNPackQty);
			var packedItemsWithBarcodes = packedItems.Select(d => new PkgPackageItemDivotsWrapperAndBarcode(d, barcodeMatch));

			var bizO = new UnpackItemsViaScanBusinessObject(Data.PackageJob, packedItemsWithBarcodes, isUnpackingQty: true);
			var iScanBizO = (IScanningItemsBusinessObject)bizO;
			var wrappers = bizO.PackableItemParentsForBinding.Cast<PackableItemParentWrapper>();

			iScanBizO.SelectPackableItemParent(wrappers.First(w => w.PackableItemParent == Data.DummyLine1));
			AssertEquals(2, bizO.TUNCodeAvailableCount);

			iScanBizO.SelectPackableItemParent(wrappers.First(w => w.PackableItemParent == Data.DummyLine2));
			AssertEquals(1, bizO.TUNCodeAvailableCount);
		}

		#endregion

		#region AssertPackageType

		void AssertPackageType(Func<UnpackItemsViaScanBusinessObject, ZString> property)
		{
			Data.CreatePackingData();
			var barcodes = Data.Barcodes;

			var packedItem1 = Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine1, barcodes.Dummy1And2TUNPackQty);
			var dAb1 = new PkgPackageItemDivotsWrapperAndBarcode(packedItem1, new BarcodeMatch(true, barcodes.Dummy1And2TUNPackType, barcodes.Dummy1And2TUNPackQty));
			var bizO1 = new UnpackItemsViaScanBusinessObject(Data.PackageJob, new[] { dAb1 });
			AssertEquals("BOX", property(bizO1));

			var packedItem2 = Data.PackageJob.Packages.AddNew("KEG").Pack_ForTesting(Data.DummyLine1, barcodes.Dummy3TUNPackQty);
			var dAb2 = new PkgPackageItemDivotsWrapperAndBarcode(packedItem2, new BarcodeMatch(true, barcodes.Dummy3TUNPackType, barcodes.Dummy3TUNPackQty));
			var bizO2 = new UnpackItemsViaScanBusinessObject(Data.PackageJob, new[] { dAb2 });
			AssertEquals("KEG", property(bizO2));
		}

		#endregion

		#endregion

		#region Flags

		#region TestCanAutoApplyChanges

		public void TestCanAutoApplyChanges()
		{
			Data.CreatePackingData();

			var box = Data.PackageJob.Packages.AddNew();
			var dAb1 = new PkgPackageItemDivotsWrapperAndBarcode(box.Pack_ForTesting(Data.DummyLine1, 5m), BarcodeMatch.Yes);
			var dAb2 = new PkgPackageItemDivotsWrapperAndBarcode(box.Pack_ForTesting(Data.DummyLine2, 5m), BarcodeMatch.Yes);

			AssertEquals(false, new UnpackItemsViaScanBusinessObject(Data.PackageJob, Array.Empty<PkgPackageItemDivotsWrapperAndBarcode>()).CanAutoApplyChanges);
			AssertEquals(false, new UnpackItemsViaScanBusinessObject(Data.PackageJob, Array.Empty<PkgPackageItemDivotsWrapperAndBarcode>(), isUnpackingQty: true).CanAutoApplyChanges);
			AssertEquals(true, new UnpackItemsViaScanBusinessObject(Data.PackageJob, new[] { dAb1 }).CanAutoApplyChanges);
			AssertEquals(false, new UnpackItemsViaScanBusinessObject(Data.PackageJob, new[] { dAb1 }, isUnpackingQty: true).CanAutoApplyChanges);
			AssertEquals(false, new UnpackItemsViaScanBusinessObject(Data.PackageJob, new[] { dAb1, dAb2 }).CanAutoApplyChanges);
			AssertEquals(false, new UnpackItemsViaScanBusinessObject(Data.PackageJob, new[] { dAb1, dAb2 }, isUnpackingQty: true).CanAutoApplyChanges);
		}

		public void TestCanAutoApplyChanges_MultipleDivots()
		{
			Data.CreatePackingData();

			var box = Data.PackageJob.Packages.AddNew();
			var dAb1a = new PkgPackageItemDivotsWrapperAndBarcode(box.Pack_ForTesting(Data.DummyLine1, 3m), BarcodeMatch.Yes);
			var dAb1b = new PkgPackageItemDivotsWrapperAndBarcode(box.Pack_ForTesting(Data.DummyLine1, 2m), BarcodeMatch.Yes);
			var dAb2 = new PkgPackageItemDivotsWrapperAndBarcode(box.Pack_ForTesting(Data.DummyLine2, 5m), BarcodeMatch.Yes);
			AssertEquals(true, new UnpackItemsViaScanBusinessObject(Data.PackageJob, new[] { dAb1a, dAb1b }).CanAutoApplyChanges);
		}

		#endregion

		#region TestCanScanPackQuanity

		public void TestCanScanPackQuanity()
		{
			Data.CreatePackingData();
			var box = Data.PackageJob.Packages.AddNew();
			var packedItem1 = new PkgPackageItemDivotsWrapperAndBarcode(box.Pack_ForTesting(Data.DummyLine1, 5m), BarcodeMatch.Yes);
			var packedItem2 = new PkgPackageItemDivotsWrapperAndBarcode(box.Pack_ForTesting(Data.DummyLine2, 5m), BarcodeMatch.Yes);

			AssertEquals(false, new UnpackItemsViaScanBusinessObject(Data.PackageJob, Array.Empty<PkgPackageItemDivotsWrapperAndBarcode>()).CanScanPackQuanity);
			AssertEquals(false, new UnpackItemsViaScanBusinessObject(Data.PackageJob, Array.Empty<PkgPackageItemDivotsWrapperAndBarcode>(), isUnpackingQty: true).CanScanPackQuanity);
			AssertEquals(false, new UnpackItemsViaScanBusinessObject(Data.PackageJob, new PkgPackageItemDivotsWrapperAndBarcode[] { packedItem1 }).CanScanPackQuanity);
			AssertEquals(true, new UnpackItemsViaScanBusinessObject(Data.PackageJob, new PkgPackageItemDivotsWrapperAndBarcode[] { packedItem1 }, isUnpackingQty: true).CanScanPackQuanity);
			AssertEquals(false, new UnpackItemsViaScanBusinessObject(Data.PackageJob, new PkgPackageItemDivotsWrapperAndBarcode[] { packedItem1, packedItem2 }).CanScanPackQuanity);
			AssertEquals(false, new UnpackItemsViaScanBusinessObject(Data.PackageJob, new PkgPackageItemDivotsWrapperAndBarcode[] { packedItem1, packedItem2 }, isUnpackingQty: true).CanScanPackQuanity);
		}

		public void TestCanScanPackQuanity_MultipleDivots()
		{
			Data.CreatePackingData();
			var box = Data.PackageJob.Packages.AddNew();
			var packedItem1 = new PkgPackageItemDivotsWrapperAndBarcode(box.Pack_ForTesting(Data.DummyLine1, 3m), BarcodeMatch.Yes);
			var packedItem2 = new PkgPackageItemDivotsWrapperAndBarcode(box.Pack_ForTesting(Data.DummyLine1, 2m), BarcodeMatch.Yes);
			var packedItem3 = new PkgPackageItemDivotsWrapperAndBarcode(box.Pack_ForTesting(Data.DummyLine2, 5m), BarcodeMatch.Yes);
			AssertEquals(true, new UnpackItemsViaScanBusinessObject(Data.PackageJob, new PkgPackageItemDivotsWrapperAndBarcode[] { packedItem1, packedItem2 }, isUnpackingQty: true).CanScanPackQuanity);
			AssertEquals(false, new UnpackItemsViaScanBusinessObject(Data.PackageJob, new PkgPackageItemDivotsWrapperAndBarcode[] { packedItem1, packedItem2, packedItem3 }, isUnpackingQty: true).CanScanPackQuanity);
		}

		#endregion

		#region TestIsAnythingSelectedToPackOrUnpack

		public void TestIsAnythingSelectedToPackOrUnpack()
		{
			Data.CreatePackingData();

			var box = Data.PackageJob.Packages.AddNew();
			var dAb = new PkgPackageItemDivotsWrapperAndBarcode(box.Pack_ForTesting(Data.DummyLine1, 5m), BarcodeMatch.Yes);
			var bizO = new UnpackItemsViaScanBusinessObject(Data.PackageJob, new[] { dAb });

			bizO.PackableItemParentsForBinding[0].ProposedRemoveQty = 0;
			AssertEquals("Proposed Unpack Qty is 0, should not be unpackable.", false, bizO.IsAnythingSelectedToPackOrUnpack);

			bizO.PackableItemParentsForBinding[0].ProposedRemoveQty = 1;
			AssertEquals(true, bizO.IsAnythingSelectedToPackOrUnpack);
		}

		public void TestIsAnythingSelectedToPackOrUnpack_Attribs()
		{
			Data.CreatePackingData();

			var box = Data.PackageJob.Packages.AddNew();
			var dAb1 = new PkgPackageItemDivotsWrapperAndBarcode(box.Pack_ForTesting(Data.DummyLine1, 5m), BarcodeMatch.Yes);
			var dAb2 = new PkgPackageItemDivotsWrapperAndBarcode(box.Pack_ForTesting(Data.DummyLine2, 5m), BarcodeMatch.Yes);

			var bizO = new UnpackItemsViaScanBusinessObject(Data.PackageJob, new[] { dAb1, dAb2 });
			AssertEquals("No distinct selection, should not be unpackable.", false, bizO.IsAnythingSelectedToPackOrUnpack);

			((IScanningItemsBusinessObject)bizO).SelectPackableItemParent(bizO.PackableItemParentsForBinding[0]);
			AssertEquals("Selection is distinct, should be unpackable.", true, bizO.IsAnythingSelectedToPackOrUnpack);
		}

		#endregion

		#endregion

		#region Mode

		public void TestMode()
		{
			Data.CreatePackingData();
			var barcodes = Data.Barcodes;

			var packedItem = Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine1, 5m);
			var dAb = new PkgPackageItemDivotsWrapperAndBarcode(packedItem, new BarcodeMatch(true));

			// cursory check to ensure we call through to the scan helper
			AssertEquals(PackOrUnpackMode.SingleItemNoQty, new UnpackItemsViaScanBusinessObject(Data.PackageJob, new[] { dAb }).Mode);
			AssertEquals(PackOrUnpackMode.Qty, new UnpackItemsViaScanBusinessObject(Data.PackageJob, new[] { dAb }, isUnpackingQty: true).Mode);
		}

		#endregion

		#region RunValidationAndApplyChanges

		#region TestRunValidationAndApplyChanges

		public void TestRunValidationAndApplyChanges()
		{
			Data.CreatePackingData();

			var box = Data.PackageJob.Packages.AddNew("BOX", "abc");
			var packedItem = box.Pack(Data.DummyLine1, 1m).Single();
			var divot = box.PackedItemDivots.Single();
			var bizO = new UnpackItemsViaScanBusinessObject(Data.PackageJob, WrapPackedItemsWithBarcodeYes(packedItem));
			bizO.RunValidationAndApplyChanges();
			AssertEquals(true, divot.IsDeleted);
			AssertEquals(true, packedItem.IsDeleted);
			AssertEquals("Non-TUN Package had an ID and should *not* have been deleted.", false, box.IsDeleted);

			// ensure factory.save doesn't die in the arse
			Factory.Save();
		}

		#endregion

		#region TestRunValidationAndApplyChanges_Attribs

		public void TestRunValidationAndApplyChanges_Attribs()
		{
			Data.CreatePackingData();

			var box = Data.PackageJob.Packages.AddNew("BOX");
			var packedItem1 = box.Pack_ForTesting(Data.DummyLine1, 10m);
			var packedItem2 = box.Pack_ForTesting(Data.DummyLine2, 10m);
			var packedItem3 = box.Pack_ForTesting(Data.DummyLine3, 10m);
			AssertEquals("Precondition - Item Weights should be added to all parent Packages.", 60m, box.KP_Weight);

			// When using attribs the selection / filter must result in a distinct item. it is important to use SelectPackableItem instead
			// of AddFilter -- this ensures the PackableItemParentsForBinding still has 2 items, but that we only unpack the selection.
			var bizO = new UnpackItemsViaScanBusinessObject(Data.PackageJob, WrapPackedItemsWithBarcodeYes(new[] { packedItem1, packedItem2 }));
			((IScanningItemsBusinessObject)bizO).SelectPackableItemParent(bizO.PackableItemParentsForBinding.FindByPackedItem(packedItem2));

			bizO.RunValidationAndApplyChanges();
			AssertEquals(10m, packedItem1.PackedQty);
			AssertEquals(9m, packedItem2.PackedQty);
			AssertEquals(10m, packedItem3.PackedQty);
			AssertEquals("Remove should reduce parent package weight.", 58m, box.KP_Weight);

			// ensure factory.save doesn't die in the arse
			Factory.Save();
		}

		#endregion

		#region TestRunValidationAndApplyChanges_Attribs_Qty

		public void TestRunValidationAndApplyChanges_Attribs_Qty()
		{
			Data.CreatePackingData();

			var box = Data.PackageJob.Packages.AddNew("BOX");
			var packedItem1 = box.Pack_ForTesting(Data.DummyLine1, 10m);
			var packedItem2 = box.Pack_ForTesting(Data.DummyLine2, 10m);
			var packedItem3 = box.Pack_ForTesting(Data.DummyLine3, 10m);
			AssertEquals("Precondition - Item Weights should be added to all parent Packages.", 60m, box.KP_Weight);

			// when using attribs the selection / filter must result in a distinct item
			var bizO = new UnpackItemsViaScanBusinessObject(Data.PackageJob, WrapPackedItemsWithBarcodeYes(new[] { packedItem1, packedItem2 }), isUnpackingQty: true);
			var wrappers = bizO.PackableItemParentsForBinding.Cast<PackableItemParentWrapper>();
			AssertEquals("Precondition - Should default unpack to total Qty.", true, wrappers.All(w => w.ProposedRemoveQty == 10m));

			var wrapper1 = wrappers.Single(w => w.PackableItemParent == Data.DummyLine1);
			wrapper1.ProposedRemoveQty = 7;

			bizO.RunValidationAndApplyChanges();
			AssertEquals("Unpack Qty was 7, 3 items should remain.", 3m, packedItem1.PackedQty);
			AssertEquals("Unpack Qty was 10, all items should have been unpacked.", true, packedItem2.IsDeleted);
			AssertEquals("Item3 was not selected for unpack, all items should remain.", 10m, packedItem3.PackedQty);
			AssertEquals("Remove should reduce parent package weight.", 26m, box.KP_Weight);

			// ensure factory.save doesn't die in the arse
			Factory.Save();
		}

		#endregion

		#region TestRunValidationAndApplyChanges_Qty

		public void TestRunValidationAndApplyChanges_Qty()
		{
			Data.CreatePackingData();

			var box = Data.PackageJob.Packages.AddNew("BOX");
			var packedItem = box.Pack(Data.DummyLine1, 10m).Single();
			var divot = box.PackedItemDivots.Single();

			var bizO = new UnpackItemsViaScanBusinessObject(Data.PackageJob, WrapPackedItemsWithBarcodeYes(packedItem), isUnpackingQty: true);
			AssertEquals("Precondition - Should default unpack to total Qty.", 10m, bizO.PackableItemParentsForBinding[0].ProposedRemoveQty);

			bizO.RunValidationAndApplyChanges();
			AssertEquals("All 10 items should have been deleted.", true, divot.IsDeleted);

			// ensure factory.save doesn't die in the arse
			Factory.Save();
		}

		#endregion

		#region TestRunValidationAndApplyChanges_TUN

		public void TestRunValidationAndApplyChanges_TUN()
		{
			Data.CreatePackingData();
			var barcodes = Data.Barcodes;

			var packedItems = new[]
			{
				Data.PackageJob.Packages.AddNew("BOX", "abc").Pack_ForTesting(Data.DummyLine1, 1m),
				Data.PackageJob.Packages.AddNew("BOX", "xyz").Pack_ForTesting(Data.DummyLine1, 1m),
				Data.PackageJob.Packages.AddNew("BOX", "gfe").Pack_ForTesting(Data.DummyLine1, 1m)
			};
			var packedItemsAndBarcodes = packedItems.Select(d => new PkgPackageItemDivotsWrapperAndBarcode(d, new BarcodeMatch(true, barcodes.Dummy1And2TUNPackType, barcodes.Dummy1And2TUNPackQty)));
			var bizO = new UnpackItemsViaScanBusinessObject(Data.PackageJob, packedItemsAndBarcodes);

			AssertEquals("Precondition", 3, Data.PackageJob.Packages.Count);
			AssertEquals(true, bizO.RunValidationAndApplyChanges());
			AssertEquals("Package Qty when not entering qty is always 1 therefore 1 package should have been unpacked.", 2, Data.PackageJob.Packages.Count);
		}

		#endregion

		#region TestRunValidationAndApplyChanges_TUN_Qty

		public void TestRunValidationAndApplyChanges_TUN_Qty()
		{
			Data.CreatePackingData();
			var barcodes = Data.Barcodes;

			var packedItems = new[]
			{
				Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine1, 1m),
				Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine1, 1m),
				Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine1, 1m)
			};
			var packedItemsAndBarcodes = packedItems.Select(d => new PkgPackageItemDivotsWrapperAndBarcode(d, new BarcodeMatch(true, barcodes.Dummy1And2TUNPackType, barcodes.Dummy1And2TUNPackQty)));
			var bizO = new UnpackItemsViaScanBusinessObject(Data.PackageJob, packedItemsAndBarcodes, isUnpackingQty: true);

			bizO.PackageQtyToCreate = 2; // simulate user typing 2 TUNs to unpack
			AssertEquals("Precondition", 3, Data.PackageJob.Packages.Count);
			AssertEquals(true, bizO.RunValidationAndApplyChanges());
			AssertEquals("Package Qty was 2 therefore 2 packages should have been unpacked.", 1, Data.PackageJob.Packages.Count);
		}

		#endregion

		#region TestRunValidationAndApplyChanges_TUN_Attribs

		public void TestRunValidationAndApplyChanges_TUN_Attribs()
		{
			Data.CreatePackingData();
			var barcodes = Data.Barcodes;

			var packedItems = new[]
			{
				Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine1, 1m), //
				Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine2, 1m)  // diff attrib
			};
			var packedItemsAndBarcodes = packedItems.Select(d => new PkgPackageItemDivotsWrapperAndBarcode(d, new BarcodeMatch(true, barcodes.Dummy1And2TUNPackType, barcodes.Dummy1And2TUNPackQty)));
			var bizO = new UnpackItemsViaScanBusinessObject(Data.PackageJob, packedItemsAndBarcodes);

			// select item 1 for unpack
			((IScanningItemsBusinessObject)bizO).SelectPackableItemParent(bizO.PackableItemParentsForBinding.FindByPackedItem(packedItems[0]));
			AssertEquals("Precondition", 2, Data.PackageJob.Packages.Count);
			AssertEquals(true, bizO.RunValidationAndApplyChanges());
			AssertEquals("Should have unpacked the selected attrib item (1).", true, Data.PackageJob.Packages.Single().PackedItemDivots.Single().GetPackableItemParent() == Data.DummyLine2);
		}

		#endregion

		#region TestRunValidationAndApplyChanges_TUN_Attribs_Qty

		public void TestRunValidationAndApplyChanges_TUN_Attribs_Qty()
		{
			Data.CreatePackingData();
			var barcodes = Data.Barcodes;

			var box1 = Data.PackageJob.Packages.AddNew("BOX");
			var box2 = Data.PackageJob.Packages.AddNew("BOX");
			var box3 = Data.PackageJob.Packages.AddNew("BOX");
			var box4 = Data.PackageJob.Packages.AddNew("BOX");
			var box5 = Data.PackageJob.Packages.AddNew("BOX");

			var packedItems = new[]
			{
				box1.Pack_ForTesting(Data.DummyLine1, 1m), //
				box2.Pack_ForTesting(Data.DummyLine1, 1m), //
				box3.Pack_ForTesting(Data.DummyLine1, 1m), //
				box4.Pack_ForTesting(Data.DummyLine2, 1m), // diff attrib
				box5.Pack_ForTesting(Data.DummyLine2, 1m)  // diff attrib
			};
			var packedItemsAndBarcodes = packedItems.Select(d => new PkgPackageItemDivotsWrapperAndBarcode(d, new BarcodeMatch(true, barcodes.Dummy1And2TUNPackType, barcodes.Dummy1And2TUNPackQty)));
			var bizO = new UnpackItemsViaScanBusinessObject(Data.PackageJob, packedItemsAndBarcodes, isUnpackingQty: true);

			// select item 1 and enter TUN qty of 2 packs
			((IScanningItemsBusinessObject)bizO).SelectPackableItemParent(bizO.PackableItemParentsForBinding.FindByPackedItem(packedItems[0]));
			bizO.PackageQtyToCreate = 2;

			AssertEquals("Precondition", 5, Data.PackageJob.Packages.Count);
			AssertEquals(true, bizO.RunValidationAndApplyChanges());

			AssertEquals("Should have unpacked 2x Box with Item1 (TUN definition).", 1,
				(from p in Data.PackageJob.Packages where p.PackedItemDivots[0].GetPackableItemParent() == Data.DummyLine1 select p).Count());

			AssertEquals("Should not have unpacked any Box with Item2 (was not selected).", 2,
				(from p in Data.PackageJob.Packages where p.PackedItemDivots[0].GetPackableItemParent() == Data.DummyLine2 select p).Count());
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

			var packedItem = Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine1, 5m);
			var dAb = new PkgPackageItemDivotsWrapperAndBarcode(packedItem, new BarcodeMatch(true));

			AssertEquals(false, new UnpackItemsViaScanBusinessObject(Data.PackageJob, new[] { dAb }).IsUserEnteringQty);
			AssertEquals(true, new UnpackItemsViaScanBusinessObject(Data.PackageJob, new[] { dAb }, isUnpackingQty: true).IsUserEnteringQty);
		}

		#endregion

		#region TestIScanningItemsBusinessObject_IsTUN

		public void TestIScanningItemsBusinessObject_IsTUN()
		{
			Data.CreatePackingData();

			var packedItem1 = Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine1, 5m);
			var dAb1 = new PkgPackageItemDivotsWrapperAndBarcode(packedItem1, new BarcodeMatch(true));

			AssertEquals(false, new UnpackItemsViaScanBusinessObject(Data.PackageJob, new[] { dAb1 }).IsTUN);

			var packedItem2 = Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine1, 5m);
			var dAb2 = new PkgPackageItemDivotsWrapperAndBarcode(packedItem2, new BarcodeMatch(true, Data.DummyLine1.BarcodeTUNPackType, Data.DummyLine1.BarcodeTUNPackQty));

			AssertEquals(true, new UnpackItemsViaScanBusinessObject(Data.PackageJob, new[] { dAb2 }).IsTUN);
		}

		#endregion

		#region TestIScanningItemsBusinessObject_TUNCode

		public void TestIScanningItemsBusinessObject_TUNCode()
		{
			Data.CreatePackingData();

			// no packed items to unpack
			IScanningItemsBusinessObject bizO1 = new UnpackItemsViaScanBusinessObject(Data.PackageJob, Enumerable.Empty<PkgPackageItemDivotsWrapperAndBarcode>());
			AssertEquals(true, bizO1.TUNCode.IsEmpty);

			var packedItem1 = Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine1, 5m);
			var dAb1 = new PkgPackageItemDivotsWrapperAndBarcode(packedItem1, new BarcodeMatch(true));

			// unpacking normal packed items
			IScanningItemsBusinessObject bizO2 = new UnpackItemsViaScanBusinessObject(Data.PackageJob, new[] { dAb1 });
			AssertEquals(true, bizO2.TUNCode.IsEmpty);

			var packedItem2 = Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine1, 5m);
			var dAb2 = new PkgPackageItemDivotsWrapperAndBarcode(packedItem2, new BarcodeMatch(true, Data.DummyLine1.BarcodeTUNPackType, Data.DummyLine1.BarcodeTUNPackQty));

			// unpacking TUN packed items
			IScanningItemsBusinessObject bizO3 = new UnpackItemsViaScanBusinessObject(Data.PackageJob, new[] { dAb2 });
			AssertEquals("BOX", bizO3.TUNCode);
		}

		#endregion

		#region TestIScanningItemsBusinessObject_AddFilterIfValidAttribute

		public void TestIScanningItemsBusinessObject_AddFilterIfValidAttribute()
		{
			Data.CreatePackingData();

			var packedItem1 = Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine1, 26m);
			var packedItem2 = Data.PackageJob.Packages.AddNew("BOX").Pack_ForTesting(Data.DummyLine2, 6m);

			var dAb1 = new PkgPackageItemDivotsWrapperAndBarcode(packedItem1, new BarcodeMatch(true));
			var dAb2 = new PkgPackageItemDivotsWrapperAndBarcode(packedItem2, new BarcodeMatch(true));
			var bizO = new UnpackItemsViaScanBusinessObject(Data.PackageJob, new[] { dAb1, dAb2 });
			var wrappers = bizO.PackableItemParentsForBinding.Cast<PackableItemParentWrapper>();
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { Data.DummyLine1, Data.DummyLine2 }, wrappers.Select(w => w.PackableItemParent));

			AssertEquals(true, ((IScanningItemsBusinessObject)bizO).AddFilterIfValidAttribute("attr1"));
			AssertContainsExactElementsInAnyOrder(new[] { Data.DummyLine1 }, wrappers.Select(w => w.PackableItemParent));
		}

		#endregion

		#region TestIScanningItemsBusinessObject_GetAllWrappers

		// this is tested in TestPackableItemParentsForBinding() because PackableItemParentsForBinding uses the scanHelper which calls GetAllWrappers().

		#endregion

		#region TestIScanningItemsBusinessObject_GetAllWrappers

		public void TestIScanningItemsBusinessObject_GetAllWrappers_OneAtATime()
		{
			Data.CreatePackingData();

			var box1 = Data.PackageJob.Packages.AddNew("BOX");

			var packedItems = new[]
			{
				box1.Pack_ForTesting(Data.DummyLine1, 26m),
				box1.Pack_ForTesting(Data.DummyLine2, 6m),
			};

			IScanningItemsBusinessObject_GetAllWrappers_Core(packedItems, 2);

			Data.DummyLine2.Barcode = Data.DummyLine1.Barcode; // (just to show are the same)
			Data.DummyLine2.ZD1_Code = Data.DummyLine1.ZD1_Code;
			Data.DummyLine2.ZD1_Number = Data.DummyLine1.ZD1_Number;

			var box2 = Data.PackageJob.Packages.AddNew("BOX"); // Need another box, because if same box used, packedItem adds divot to existing and key will remain same.
			packedItems = new[]
			{
				box2.Pack_ForTesting(Data.DummyLine1, 26m),
				box2.Pack_ForTesting(Data.DummyLine2, 6m),
			};

			IScanningItemsBusinessObject_GetAllWrappers_Core(packedItems, 1);
		}

		void IScanningItemsBusinessObject_GetAllWrappers_Core(PkgPackageItemDivotsWrapper[] packedItems, int expectedWrapperCount)
		{
			var packedItemsAndBarcodes = WrapPackedItemsWithBarcodeYes(packedItems);

			var bizO = new UnpackItemsViaScanBusinessObject(Data.PackageJob, packedItemsAndBarcodes, isUnpackingQty: false);
			var wrappers = ((IScanningItemsBusinessObject)bizO).GetAllWrappers();
			AssertEquals(2, wrappers.Count());
			AssertEquals(expectedWrapperCount, bizO.PackableItemParentsForBinding.Count<PackableItemParentWrapper>(w => w.ProposedRemoveQty == 1m));
		}

		#endregion

		#endregion

		#region TestSetPackOrRemoveQuantity

		public override void TestSetPackOrRemoveQuantity()
		{
			var bizO = GetNewItemsBusinessObject();
			var wrapper = bizO.PackableItemParentsForBinding.Single<PackableItemParentWrapper>();
			AssertEquals("Precondition", 1m, wrapper.ProposedRemoveQty);

			bizO.SetPackOrRemoveQuantity(wrapper, 10m);
			AssertEquals(10m, wrapper.ProposedRemoveQty);
		}

		#endregion

		#region Implementation

		protected override UnpackItemsViaScanBusinessObject GetNewItemsBusinessObject()
		{
			Data.CreatePackingData();
			var packedItem = Data.PackageJob.Packages.AddNew().Pack_ForTesting(Data.DummyLine1, 50m);
			return new UnpackItemsViaScanBusinessObject(Data.PackageJob, WrapPackedItemsWithBarcodeYes(packedItem));
		}

		protected override ZString ProposedColumnName
		{
			get { return PackableItemParentWrapper.Schema.ProposedRemoveQty; }
		}

		protected override ZString ProposedDescription
		{
			get { return "Unpack"; }
		}

		protected override Type ValidationType
		{
			get { return typeof(UnpackItemsViaScanBusinessObjectValidation); }
		}

		#endregion
	}
}
