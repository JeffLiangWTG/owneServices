using System;
using System.Collections.Generic;

namespace Enterprise.Packing.Business.Testing
{
	public class ScanningItemsBusinessObjectHelperTestCase : PackingTestCaseWithFactory
	{
		#region TestCanAutoApplyChangesAndCanScanPackOrRemoveQuanity

		public void TestCanAutoApplyChangesAndCanScanPackOrRemoveQuanity()
		{
			Data.CreatePackingData();

			// single item
			var bizOWithSingleItem = new ItemsBusinessObjectTest.ItemsBusinessObjectForTest(Data);
			var helper = new ScanningItemsBusinessObjectHelperForTest(bizOWithSingleItem);
			bizOWithSingleItem.NewPackableItemParentsForBinding = new[] { Data.DummyLine1Wrapper };

			bizOWithSingleItem.IsUserEnteringQty = true;
			AssertEquals(false, helper.CanAutoApplyChanges);
			AssertEquals(true, helper.CanScanPackOrRemoveQuanity);

			bizOWithSingleItem.IsUserEnteringQty = false;
			AssertEquals(true, helper.CanAutoApplyChanges);
			AssertEquals(false, helper.CanScanPackOrRemoveQuanity);

			// multi item
			var bizOWithMultiItems = new ItemsBusinessObjectTest.ItemsBusinessObjectForTest(Data);
			var helperMulti = new ScanningItemsBusinessObjectHelperForTest(bizOWithMultiItems);
			bizOWithMultiItems.NewPackableItemParentsForBinding = new[] { Data.DummyLine1Wrapper, Data.DummyLine2Wrapper };

			bizOWithMultiItems.IsUserEnteringQty = true;
			AssertEquals(false, helperMulti.CanAutoApplyChanges);
			AssertEquals(false, helper.CanScanPackOrRemoveQuanity);

			bizOWithMultiItems.IsUserEnteringQty = false;
			AssertEquals(false, helperMulti.CanAutoApplyChanges);
			AssertEquals(false, helper.CanScanPackOrRemoveQuanity);
		}

		#endregion

		#region TestCanAutoApplyChangesAndCanScanPackOrRemoveQuanity_MultiItems

		public void TestCanAutoApplyChangesAndCanScanPackOrRemoveQuanity_MultiItems()
		{
			Data.CreatePackingData();
			Data.DummyLine2.Barcode = Data.DummyLine1.Barcode; // (just to show are the same)
			Data.DummyLine2.ZD1_Code = Data.DummyLine1.ZD1_Code;
			Data.DummyLine2.ZD1_Number = Data.DummyLine1.ZD1_Number;
			CanAutoApplyChangesAndCanScanPackOrRemoveQuanity_MultiItems_Core((data) => new[] { data.DummyLine1Wrapper, data.DummyLine2Wrapper }, true);
			CanAutoApplyChangesAndCanScanPackOrRemoveQuanity_MultiItems_Core((data) => new[] { data.DummyLine1Wrapper, data.DummyLine1Wrapper }, true);
			CanAutoApplyChangesAndCanScanPackOrRemoveQuanity_MultiItems_Core((data) => new[] { data.DummyLine1Wrapper, data.DummyLine3Wrapper }, false);
		}

		void CanAutoApplyChangesAndCanScanPackOrRemoveQuanity_MultiItems_Core(Func<TestDataForPacking, IEnumerable<PackableItemParentWrapper>> items, bool expected)
		{
			var bizOWithSamesItems = new ItemsBusinessObjectTest.ItemsBusinessObjectForTest(Data);
			var helper = new ScanningItemsBusinessObjectHelperForTest(bizOWithSamesItems);
			bizOWithSamesItems.NewPackableItemParentsForBinding = items(Data);
			bizOWithSamesItems.IsUserEnteringQty = false;
			AssertEquals(expected, helper.CanAutoApplyChanges);
		}

		#endregion

		#region TestSelectPackableItemParent

		public void TestSelectPackableItemParent()
		{
			Data.CreatePackingData();

			var bizO = new ItemsBusinessObjectTest.ItemsBusinessObjectForTest(Data);
			var helper = new ScanningItemsBusinessObjectHelperForTest(bizO);
			bizO.NewPackableItemParentsForBinding = new[] { Data.DummyLine1Wrapper, Data.DummyLine2Wrapper };
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { Data.DummyLine1Wrapper, Data.DummyLine2Wrapper }, helper.WrappersCacheCurrent);
			AssertEquals("Precondition", false, helper.OnWrapperSelectedHit);

			// set some bad data that would incur validation errors
			Data.DummyLine1Wrapper.ProposedPackQty = -1;
			Data.DummyLine2Wrapper.ProposedPackQty = -1;

			// ensure all items have errors
			AssertEquals("No selection, all items should be validated.", true, Data.DummyLine1Wrapper.HasErrors);
			AssertEquals("No selection, all items should be validated.", true, Data.DummyLine2Wrapper.HasErrors);

			// select dummy 1
			helper.SelectPackableItemParent(Data.DummyLine1Wrapper);
			AssertContainsExactElementsInAnyOrder(Data.DummyLine1Wrapper, helper.WrappersCacheCurrent);
			AssertEquals(true, helper.OnWrapperSelectedHit);
			AssertEquals("Item1 is selected and should be validated.", true, Data.DummyLine1Wrapper.HasErrors);
			AssertEquals("Item2 is not selected and should not be validated.", false, Data.DummyLine2Wrapper.HasErrors);

			// select dummy 2
			helper.SelectPackableItemParent(Data.DummyLine2Wrapper);
			AssertContainsExactElementsInAnyOrder(Data.DummyLine2Wrapper, helper.WrappersCacheCurrent);
			AssertEquals("Item1 is not selected and should not be validated.", false, Data.DummyLine1Wrapper.HasErrors);
			AssertEquals("Item2 is selected and should be validated.", true, Data.DummyLine2Wrapper.HasErrors);

			// various unsupported values
			AssertNoExceptionThrown(() => helper.SelectPackableItemParent(null));
			AssertExceptionThrown(typeof(ArgumentException), "Wrapper cannot be selected because it does not exist in the PackableItemParents collection.",
				() => helper.SelectPackableItemParent(Data.DummyLine3Wrapper));
		}

		#endregion

		#region TestIsAnythingSelectedToPackOrUnpack

		public void TestIsAnythingSelectedToPackOrUnpack()
		{
			Data.CreatePackingData();

			var bizO = new ItemsBusinessObjectTest.ItemsBusinessObjectForTest(Data);
			var helper = new ScanningItemsBusinessObjectHelperForTest(bizO);
			bizO.NewPackableItemParentsForBinding = new[] { Data.DummyLine1Wrapper, Data.DummyLine2Wrapper }; // attribs

			bizO.SetModeForTest(PackOrUnpackMode.Attribs);
			AssertEquals("Items not distinct, should *not* be packable.", false, helper.IsAnythingSelectedToPackOrUnpack);

			bizO.SetModeForTest(PackOrUnpackMode.TUN_Qty_Attribs);
			AssertEquals("Items not distinct, should *not* be packable.", false, helper.IsAnythingSelectedToPackOrUnpack);

			bizO.SetModeForTest(PackOrUnpackMode.Attribs);
			helper.SelectPackableItemParent(bizO.PackableItemParentsForBinding[0]);
			AssertEquals("Items are distinct, should be packable.", true, helper.IsAnythingSelectedToPackOrUnpack);

			bizO.SetModeForTest(PackOrUnpackMode.TUN_Qty_Attribs);
			AssertEquals("Items are distinct, should be packable.", true, helper.IsAnythingSelectedToPackOrUnpack);

			bizO.SetModeForTest(PackOrUnpackMode.SingleItemNoQty);
			AssertEquals("No item has Qty > 0, should *not* be packable.", false, helper.IsAnythingSelectedToPackOrUnpack);

			bizO.PackableItemParentsForBinding[1].ProposedPackQty = 5m;
			AssertEquals("At least one item has Qty > 0, should be packable.", true, helper.IsAnythingSelectedToPackOrUnpack);
		}

		#endregion

		#region TestAddFilterIfValidAttribute

		public void TestAddFilterIfValidAttribute()
		{
			Data.CreatePackingData();

			// fudge item3 to partially matches item1, in order to test the whittling down of the items collection with each successive attrib filter/scan
			Data.DummyLine3.ZD1_Code = "attr1";
			Data.DummyLine3.ZD1_Number = 3;
			Data.DummyLine3.Barcode = Data.Barcodes.Dummy1And2Barcode;
			Data.DummyLine3.BarcodeTUN = Data.Barcodes.Dummy1And2TUNBarcode;
			Data.DummyLine3.BarcodeTUNPackType = Data.Barcodes.Dummy1And2TUNPackType;
			Data.DummyLine3.BarcodeTUNPackQty = Data.Barcodes.Dummy1And2TUNPackQty;

			var bizO = new ItemsBusinessObjectTest.ItemsBusinessObjectForTest(Data);
			var helper = new ScanningItemsBusinessObjectHelperForTest(bizO);
			bizO.NewPackableItemParentsForBinding = new[] { Data.DummyLine1Wrapper, Data.DummyLine2Wrapper, Data.DummyLine3Wrapper };
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { Data.DummyLine1Wrapper, Data.DummyLine2Wrapper, Data.DummyLine3Wrapper }, helper.WrappersCacheCurrent);

			// set some bad data that would incur validation errors
			Data.DummyLine1Wrapper.ProposedPackQty = -1;
			Data.DummyLine2Wrapper.ProposedPackQty = -1;
			Data.DummyLine3Wrapper.ProposedPackQty = -1;

			// ensure all items have errors
			AssertEquals("No selection, all items should be validated.", true, Data.DummyLine1Wrapper.HasErrors);
			AssertEquals("No selection, all items should be validated.", true, Data.DummyLine2Wrapper.HasErrors);
			AssertEquals("No selection, all items should be validated.", true, Data.DummyLine3Wrapper.HasErrors);

			// add a dodgy attribute filter
			AssertEquals(false, helper.AddFilterIfValidAttribute("moo"));
			AssertContainsExactElementsInAnyOrder(new[] { Data.DummyLine1Wrapper, Data.DummyLine2Wrapper, Data.DummyLine3Wrapper }, helper.WrappersCacheCurrent);

			// add filter "attr1" (should match dummy1 + dummy3)
			AssertEquals(true, helper.AddFilterIfValidAttribute("attr1"));
			AssertContainsExactElementsInAnyOrder(new[] { Data.DummyLine1Wrapper, Data.DummyLine3Wrapper }, helper.WrappersCacheCurrent);
			AssertEquals("Item1 is still in the list and should be validated.", true, Data.DummyLine1Wrapper.HasErrors);
			AssertEquals("Item2 was filtered out and should not be validated.", false, Data.DummyLine2Wrapper.HasErrors);
			AssertEquals("Item3 is still in the list and should be validated.", true, Data.DummyLine3Wrapper.HasErrors);

			// add filter "1" (should match dummy1 only)
			AssertEquals(true, helper.AddFilterIfValidAttribute("1"));
			AssertContainsExactElementsInAnyOrder(new[] { Data.DummyLine1Wrapper }, helper.WrappersCacheCurrent);
			AssertEquals("Item1 is still in the list and should be validated.", true, Data.DummyLine1Wrapper.HasErrors);
			AssertEquals("Item2 was filtered out and should not be validated.", false, Data.DummyLine2Wrapper.HasErrors);
			AssertEquals("Item3 was filtered out and should not be validated.", false, Data.DummyLine3Wrapper.HasErrors);

			// unsupported empty value
			AssertExceptionThrown(typeof(ArgumentException), () => helper.AddFilterIfValidAttribute(""));
		}

		#endregion

		#region TestWrappersCacheCurrent

		public void TestWrappersCacheCurrent()
		{
			Data.CreatePackingData();

			var bizO = new ItemsBusinessObjectTest.ItemsBusinessObjectForTest(Data);
			var helper = new ScanningItemsBusinessObjectHelperForTest(bizO);
			bizO.NewPackableItemParentsForBinding = new[] { Data.DummyLine1Wrapper, Data.DummyLine2Wrapper };

			// all
			AssertContainsExactElementsInAnyOrder(new[] { Data.DummyLine1Wrapper, Data.DummyLine2Wrapper }, helper.WrappersCacheCurrent);

			// for selected item
			helper.SelectPackableItemParent(Data.DummyLine1Wrapper);
			AssertContainsExactElementsInAnyOrder(new[] { Data.DummyLine1Wrapper }, helper.WrappersCacheCurrent);

			// for scanned attribute
			helper.AddFilterIfValidAttribute("attr2");
			AssertContainsExactElementsInAnyOrder(new[] { Data.DummyLine2Wrapper }, helper.WrappersCacheCurrent);
		}

		#endregion

		#region TestGetMode

		public void TestGetMode()
		{
			Data.CreatePackingData();

			// no attribs
			var bizO = new ItemsBusinessObjectTest.ItemsBusinessObjectForTest(Data);
			var helper = new ScanningItemsBusinessObjectHelperForTest(bizO);
			bizO.NewPackableItemParentsForBinding = new[] { Data.DummyLine1Wrapper };

			AssertGetMode(bizO, helper, isQty: false, isTUN: false, expectedMode: PackOrUnpackMode.SingleItemNoQty);
			AssertGetMode(bizO, helper, isQty: true, isTUN: false, expectedMode: PackOrUnpackMode.Qty);
			AssertGetMode(bizO, helper, isQty: true, isTUN: true, expectedMode: PackOrUnpackMode.TUN_Qty);

			// attribs
			var bizOWithAttribs = new ItemsBusinessObjectTest.ItemsBusinessObjectForTest(Data);
			var helperWithAttribs = new ScanningItemsBusinessObjectHelperForTest(bizOWithAttribs);
			bizOWithAttribs.NewPackableItemParentsForBinding = new[] { Data.DummyLine1Wrapper, Data.DummyLine2Wrapper }; // attribs

			AssertGetMode(bizOWithAttribs, helperWithAttribs, isQty: false, isTUN: false, expectedMode: PackOrUnpackMode.Attribs);
			AssertGetMode(bizOWithAttribs, helperWithAttribs, isQty: true, isTUN: false, expectedMode: PackOrUnpackMode.Qty);
			AssertGetMode(bizOWithAttribs, helperWithAttribs, isQty: true, isTUN: true, expectedMode: PackOrUnpackMode.TUN_Qty_Attribs);
		}

		void AssertGetMode(ItemsBusinessObjectTest.ItemsBusinessObjectForTest bizO, ScanningItemsBusinessObjectHelperForTest helper, bool isQty, bool isTUN, PackOrUnpackMode expectedMode)
		{
			bizO.IsUserEnteringQty = isQty;
			bizO.IsTUN = isTUN;
			AssertEquals(expectedMode, helper.GetMode());
		}

		#endregion

		#region ScanningItemsBusinessObjectHelperForTest

		class ScanningItemsBusinessObjectHelperForTest : ScanningItemsBusinessObjectHelper<ItemsBusinessObjectTest.ItemsBusinessObjectForTest>
		{
			public ScanningItemsBusinessObjectHelperForTest(ItemsBusinessObjectTest.ItemsBusinessObjectForTest bizO)
				: base(bizO)
			{
			}

			protected override void OnWrapperSelected()
			{
				base.OnWrapperSelected();
				OnWrapperSelectedHit = true;
			}

			public bool OnWrapperSelectedHit
			{
				get;
				private set;
			}
		}

		#endregion
	}
}
