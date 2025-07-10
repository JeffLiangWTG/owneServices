using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PackableItemParentWrapper))]
	public class PackableItemParentWrapperTest : PackingNonPersistentBusinessObjectTestCase<PackableItemParentWrapper>
	{
		#region TestConstructor

		public void TestConstructor()
		{
			Data.CreatePackingData();
			AssertExceptionThrown<ArgumentNullException>(() => new PackableItemParentWrapper(Factory, (IGrouping<object, PkgPackageItemDivotsWrapper>)null, null));
			AssertExceptionThrown<ArgumentNullException>(() => new PackableItemParentWrapper(Factory, (IPackableItemParent)null));
			AssertExceptionThrown<ArgumentNullException>(() => new PackableItemParentWrapper(Factory, null));

			AssertExceptionThrown<ArgumentOutOfRangeException>(() => new PackableItemParentWrapper(Factory, new DummyGrouping(), null));
		}

		class DummyGrouping : List<PkgPackageItemDivotsWrapper>, IGrouping<object, PkgPackageItemDivotsWrapper>
		{
			public object Key => this;
		}

		#endregion

		#region Related Entities

		#region TestKey

		public void TestKey()
		{
			Data.CreatePackingData();

			var wrapper = new PackableItemParentWrapper(Factory, Data.DummyLine1);
			AssertEquals(Data.DummyPackableItemOnLine1.Key, wrapper.Key);
			AssertEquals(Data.DummyPackableItemOnLine1.Key, new PackableItemParentWrapper(Factory, wrapper).Key);

			var package = Data.PackageJob.Packages.AddNew();
			var packedItem = package.Pack(Data.DummyLine1, 10m).Single();
			AssertEquals(packedItem.Key, new PackableItemParentWrapper(Factory, packedItem.AsGroup(), Data.Dummy).Key);
		}

		#endregion

		#region TestPackableItemParent

		public void TestPackableItemParent()
		{
			Data.CreatePackingData();
			var wrapper = new PackableItemParentWrapper(Factory, Data.DummyLine1);
			AssertEquals(Data.DummyLine1, wrapper.PackableItemParent);

			Data.DummyLine1.Delete();
			AssertNull(wrapper.PackableItemParent);

			var packedItem = Data.PackageJob.Packages.AddNew().Pack(Data.DummyLine2, 100m).Single();
			AssertEquals(Data.DummyLine2, new PackableItemParentWrapper(Factory, packedItem.AsGroup(), Data.Dummy).PackableItemParent);
		}

		#endregion

		#region TestGroupedPackedItems

		public void TestGroupedPackedItems()
		{
			Data.CreatePackingData();
			var wrapper1 = new PackableItemParentWrapper(Factory, Data.DummyLine1);
			AssertNull(wrapper1.GroupedPackedItems);
			AssertNull(new PackableItemParentWrapper(Factory, wrapper1).GroupedPackedItems);

			var package = Data.PackageJob.Packages.AddNew();
			var packedItem = package.Pack(Data.DummyLine1, 10m).Single();
			var wrapper2 = new PackableItemParentWrapper(Factory, packedItem.AsGroup(), Data.Dummy);
			AssertContainsExactElementsInAnyOrder(new[] { packedItem }, wrapper2.GroupedPackedItems);
			AssertNull(new PackableItemParentWrapper(Factory, wrapper2).GroupedPackedItems);
		}

		#endregion

		#region TestParentJob

		public void TestParentJob()
		{
			Data.CreatePackingData();

			AssertNull(new PackableItemParentWrapper(Factory, Data.DummyLine1).ParentJob);
			AssertEquals(Data.Dummy, new PackableItemParentWrapper(Factory, Data.DummyLine1, Data.Dummy).ParentJob);
		}

		#endregion

		#endregion

		#region Properties

		#region TestIsFullyPacked

		public void TestIsFullyPacked()
		{
			Data.CreatePackingData();

			var wrapper = new PackableItemParentWrapper(Factory, Data.DummyLine1);
			AssertEquals(false, wrapper.IsFullyPacked);

			Data.PackageJob.Packages.AddNew().Pack(Data.DummyLine1, 99.5m);
			AssertEquals(false, wrapper.IsFullyPacked);

			Data.PackageJob.Packages.AddNew().Pack(Data.DummyLine1, 0.5m);
			AssertEquals(true, wrapper.IsFullyPacked);
		}

		#endregion

		#region TestProposedPackQty

		public void TestProposedPackQty()
		{
			Data.CreatePackingData();

			var wrapper = new PackableItemParentWrapper(Factory, Data.DummyLine1);
			AssertEquals("Default Pack Qty should be 0.", 0m, wrapper.ProposedPackQty);

			wrapper.ProposedPackQty = 10;
			AssertEquals(10m, wrapper.ProposedPackQty);

			// validation is tested in PackableItemWrapperValidationTest
		}

		#endregion

		#region TestProposedRemoveQty

		public void TestProposedRemoveQty()
		{
			Data.CreatePackingData();
			var package = Data.PackageJob.Packages.AddNew();
			var packedItem = package.Pack_ForTesting(Data.DummyLine1, 10m);

			var wrapper = new PackableItemParentWrapper(Factory, packedItem.AsGroup(), Data.Dummy);
			AssertEquals("Default Remove Qty should be 0.", 0m, wrapper.ProposedRemoveQty);

			wrapper.ProposedRemoveQty = 10;
			AssertEquals(10m, wrapper.ProposedRemoveQty);

			// validation is tested in PackableItemWrapperValidationTest
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestProposedRemoveQty_NotSupportedWithoutDivot()
		{
			Data.CreatePackingData();
			var wrapper = new PackableItemParentWrapper(Factory, Data.DummyLine1);
			wrapper.ProposedRemoveQty = 5;
		}

		#endregion

		#region TestPackedQty

		public void TestPackedQty()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew();
			var pallet1 = package.Packages.AddNew();
			var pallet2 = package.Packages.AddNew();
			var pallet1_box = pallet1.Packages.AddNew();
			var pallet2_box = pallet2.Packages.AddNew();

			// pack 17 of dummy line 1 into various boxes

			pallet1.Pack(Data.DummyLine1, 10m);
			pallet1_box.Pack(Data.DummyLine1, 5m);
			pallet2_box.Pack(Data.DummyLine1, 2m);

			// pack 14 of dummy line 2 into various boxes

			pallet1.Pack(Data.DummyLine2, 10m);
			pallet2.Pack(Data.DummyLine2, 3m);
			pallet2_box.Pack(Data.DummyLine2, 1m);

			var collection = new PackableItemParentWrapperCollection(Factory, new PackableItemParentWrapper[] { Data.DummyLine1Wrapper, Data.DummyLine2Wrapper });
			var wrapper1 = collection.FindByPackableItemParent(Data.DummyLine1);
			var wrapper2 = collection.FindByPackableItemParent(Data.DummyLine2);

			AssertEquals(17m, wrapper1.PackedQty);
			AssertEquals(14m, wrapper2.PackedQty);
		}

		#endregion

		#region TestPackedQtyFromWrapper

		public void TestPackedQtyFromWrapper()
		{
			Data.CreatePackingData();

			var package1 = Data.PackageJob.Packages.AddNew();
			var package2 = Data.PackageJob.Packages.AddNew();
			var packedItem1 = package1.Pack(Data.DummyLine1, 1m).Single();
			var packedItem2 = package1.Pack(Data.DummyLine1, 2m).Single();
			var packedItem3 = package1.Pack(Data.DummyLine2, 4m).Single();
			var packedItem4 = package2.Pack(Data.DummyLine1, 6m).Single();
			AssertEquals("Should be same as wrappers are grouped together now.", packedItem1, packedItem2);

			var groupedItems = new[] { packedItem1, packedItem3, packedItem4 }.GroupedPackedItems();
			var wrappers = groupedItems.Select(group => new PackableItemParentWrapper(Factory, group, Data.Dummy)).ToArray();
			var wrapper1 = wrappers.Single(w => w.PackableItemParent == Data.DummyLine1 && w.GroupedPackedItems.First().PackagePK == package1.PK);
			var wrapper2 = wrappers.Single(w => w.PackableItemParent == Data.DummyLine1 && w.GroupedPackedItems.First().PackagePK == package2.PK);
			var wrapper3 = wrappers.Single(w => w.PackableItemParent == Data.DummyLine2);
			AssertContainsExactElementsInAnyOrder(new[] { wrapper1, wrapper2, wrapper3 }, wrappers);
			AssertEquals("3 DummyLine1 should be packed in Package 1.", 3m, wrapper1.PackedQtyFromWrapper);
			AssertEquals("6 DummyLine1 should be packed in Package 2.", 6m, wrapper2.PackedQtyFromWrapper);
			AssertEquals("4 DummyLine2 should be packed in Package 1.", 4m, wrapper3.PackedQtyFromWrapper);
		}

		#endregion

		#region TestUnpackedQty

		public void TestUnpackedQty()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew();
			var pallet1 = package.Packages.AddNew();
			var pallet2 = package.Packages.AddNew();
			var pallet1_box = pallet1.Packages.AddNew();
			var pallet2_box = pallet1.Packages.AddNew();

			// pack 17 of dummy line 1 into various boxes

			pallet1.Pack(Data.DummyLine1, 10m);
			pallet1_box.Pack(Data.DummyLine1, 5m);
			pallet2_box.Pack(Data.DummyLine1, 2m);

			// pack 14 of dummy line 2 into various boxes

			pallet1.Pack(Data.DummyLine2, 10m);
			pallet2.Pack(Data.DummyLine2, 3m);
			pallet2_box.Pack(Data.DummyLine2, 1m);

			var collection = new PackableItemParentWrapperCollection(Factory, new PackableItemParentWrapper[] { Data.DummyLine1Wrapper, Data.DummyLine2Wrapper });
			var wrapper1 = collection.FindByPackableItemParent(Data.DummyLine1);
			var wrapper2 = collection.FindByPackableItemParent(Data.DummyLine2);

			AssertEquals(100m - 17m, wrapper1.UnpackedQty);
			AssertEquals(100m - 14m, wrapper2.UnpackedQty);
		}

		#endregion

		#region TestUnpackedWeight

		public void TestUnpackedWeight()
		{
			Data.CreatePackingData();

			var package = Data.PackageJob.Packages.AddNew();
			var pallet1 = package.Packages.AddNew();
			var pallet2 = package.Packages.AddNew();
			var pallet1_box = pallet1.Packages.AddNew();
			var pallet2_box = pallet1.Packages.AddNew();

			// pack 17 of dummy line 1 into various boxes

			pallet1.Pack(Data.DummyLine1, 10m);
			pallet1_box.Pack(Data.DummyLine1, 5m);
			pallet2_box.Pack(Data.DummyLine1, 2m);

			// pack 14 of dummy line 2 into various boxes

			pallet1.Pack(Data.DummyLine2, 10m);
			pallet2.Pack(Data.DummyLine2, 3m);
			pallet2_box.Pack(Data.DummyLine2, 1m);

			var collection = new PackableItemParentWrapperCollection(Factory, new PackableItemParentWrapper[] { Data.DummyLine1Wrapper, Data.DummyLine2Wrapper });
			var wrapper1 = collection.FindByPackableItemParent(Data.DummyLine1);
			var wrapper2 = collection.FindByPackableItemParent(Data.DummyLine2);

			AssertEquals(83 * 2m, wrapper1.UnpackedWeight); //
			AssertEquals(86 * 2m, wrapper2.UnpackedWeight); // each unit weighs 2KG

			// validation is tested in PackableItemWrapperValidationTest
		}

		public void TestUnpackedWeight_IsValidatedOnFirstAccess()
		{
			Data.CreatePackingData();
			Data.DummyLine1.WeightPerUnit = 0m;
			Data.DummyLine1.WeightUQ = "";

			var package = Data.PackageJob.Packages.AddNew();
			package.Pack(Data.DummyLine1, 10m);

			var collection = new PackableItemParentWrapperCollection(Factory, new PackableItemParentWrapper[] { Data.DummyLine1Wrapper, Data.DummyLine2Wrapper, Data.DummyLine3Wrapper });
			var wrapper = collection.FindByPackableItemParent(Data.DummyLine1);
			AssertNoWarnings("Precondition", wrapper.UnpackedWeightInfo);

			// accessing the property should run the validation
			var tmp = wrapper.UnpackedWeight;
			AssertHasWarning(wrapper.UnpackedWeightInfo, "No Weight defined. When packing this item the Package Weight should be manually entered.");

			// validation should not be re-run
			Data.DummyLine3.WeightPerUnit = 0m;
			Data.DummyLine3.WeightUQ = "";
			AssertHasWarning(wrapper.UnpackedWeightInfo, "No Weight defined. When packing this item the Package Weight should be manually entered.");
		}

		#endregion

		#endregion

		#region TestCurrentBarcodeAndMatch

		public void TestCurrentBarcodeAndMatch()
		{
			Data.CreatePackingData();
			var wrapper = Data.DummyLine1Wrapper;

			AssertEquals(true, wrapper.CurrentBarcode.IsEmpty);
			AssertEquals(BarcodeMatch.No, wrapper.CurrentBarcodeMatch);
			AssertEquals(false, wrapper.CurrentBarcodeMatch.IsMatch);

			wrapper.CurrentBarcode = "1+2";
			var match = wrapper.CurrentBarcodeMatch; // store the match for cache testing below
			AssertEquals("1+2", wrapper.CurrentBarcode);
			AssertEquals(true, match.IsMatch);

			wrapper.CurrentBarcode = "1+2";
			AssertEquals("Barcode did not change, BarcodeMatch should have been cached.", match, wrapper.CurrentBarcodeMatch);

			wrapper.CurrentBarcode = "2";
			AssertEquals("2", wrapper.CurrentBarcode);
			AssertEquals(false, wrapper.CurrentBarcodeMatch.IsMatch);

			wrapper.CurrentBarcode = "";
			AssertEquals(true, wrapper.CurrentBarcode.IsEmpty);
			AssertEquals(BarcodeMatch.No, wrapper.CurrentBarcodeMatch);
			AssertEquals(false, wrapper.CurrentBarcodeMatch.IsMatch);
		}

		#endregion

		#region Validation

		public void TestValidation()
		{
			Data.CreatePackingData();

			var wrapper = new PackableItemParentWrapper(Factory, Data.DummyLine1);
			AssertEquals(typeof(PackableItemParentWrapperValidation), wrapper.Validation.GetType());

			wrapper.SetIsValidationDisabled(true);
			AssertEquals(typeof(PackableItemParentWrapperEmptyValidation), wrapper.Validation.GetType());
		}

		#endregion

		#region TestICustomPropertyContainer

		public void TestICustomPropertyContainer()
		{
			Data.CreatePackingData();
			Data.DummyLine1.ZD1_Code = "abc";

			var wrapperAsICustomPropertyContainer = (ICustomPropertyContainer)Data.DummyLine1Wrapper;
			var textCustomProperty = wrapperAsICustomPropertyContainer.CustomProperties.First(property => property.Identifier == "ZD1 Code");
			AssertEquals("Should have proxied GetValue delegate from Wrapper to PackableItem.", "abc", textCustomProperty.GetValue(Data.DummyLine1Wrapper));

			var badDivot = Factory.New<PkgPackageItemDivot>();
			var badWrapper = new PackableItemParentWrapper(Factory, PkgPackageItemDivotsWrapper.New(badDivot, Data.DummyLine1).AsGroup(), Data.Dummy);
			AssertNoExceptionThrown(() => { var poke = ((ICustomPropertyContainer)badWrapper).CustomProperties; });
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			Data.CreatePackingData();
			var packedItem = Data.PackageJob.Packages.AddNew().Pack_ForTesting(Data.DummyLine1, 10m);
			return new PackableItemParentWrapper(Factory, packedItem.AsGroup(), Data.Dummy);
		}

		#endregion
	}
}
