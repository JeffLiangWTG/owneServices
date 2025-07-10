using System.Linq;

namespace Enterprise.Packing.Business.Testing
{
	public class UnpackItemsViaScanBusinessObjectValidationTest : PackingBusinessObjectValidationTestCase
	{
		#region TestValidatePackageQtyToCreate

		public void TestValidatePackageQtyToCreate()
		{
			Data.CreatePackingData();

			var packedItems = new[]
			{
				Data.PackageJob.Packages.AddNew("KEG").Pack_ForTesting(Data.DummyLine3, Data.Barcodes.Dummy3TUNPackQty),
				Data.PackageJob.Packages.AddNew("KEG").Pack_ForTesting(Data.DummyLine3, Data.Barcodes.Dummy3TUNPackQty),
			};
			var barcodeMatch = new BarcodeMatch(true, Data.Barcodes.Dummy3TUNPackType, Data.Barcodes.Dummy3TUNPackQty);
			var packedItemsWithBarcodes = packedItems.Select(d => new PkgPackageItemDivotsWrapperAndBarcode(d, barcodeMatch));
			var bizO = new UnpackItemsViaScanBusinessObject(Data.PackageJob, packedItemsWithBarcodes)
			{
				PackageQtyToCreate = 1
			};
			AssertNoErrors(bizO.PackageQtyToCreateInfo);

			bizO.PackageQtyToCreate = 3;
			AssertHasError(bizO.PackageQtyToCreateInfo, "Only 2 KEGS are available for Unpack.");

			bizO.PackageQtyToCreate = 2;
			AssertNoErrors(bizO.PackageQtyToCreateInfo);

			// test with a single package for grammar check

			var itemDivot = Data.PackageJob.Packages[0].PackedItemDivots[0];
			var packedItemsWithBarcodes2 = new[] { PkgPackageItemDivotsWrapper.New(itemDivot, Data.DummyLine3) }.Select(d => new PkgPackageItemDivotsWrapperAndBarcode(d, barcodeMatch));
			var bizO2 = new UnpackItemsViaScanBusinessObject(Data.PackageJob, packedItemsWithBarcodes2)
			{
				PackageQtyToCreate = 2
			};
			AssertHasError(bizO2.PackageQtyToCreateInfo, "Only 1 KEG is available for Unpack.");
		}

		#endregion

		#region TestValidatePackageTypeToCreate_IsOnlyRunIfUnpackingTUN

		public void TestValidatePackageTypeToCreate_IsOnlyRunIfUnpackingTUN()
		{
			Data.CreatePackingData();
			var barcodes = Data.Barcodes;
			var packedItem = Data.PackageJob.Packages.AddNew("KEG").Pack_ForTesting(Data.DummyLine1, barcodes.Dummy1And2TUNPackQty);

			// non-TUN unpack
			var dAB = new PkgPackageItemDivotsWrapperAndBarcode(packedItem, BarcodeMatch.Yes);
			var bizO = new UnpackItemsViaScanBusinessObject(Data.PackageJob, new[] { dAB });
			AssertNoErrors("Precondition", bizO.PackageTypeToCreateInfo);

			bizO.PackageTypeToCreate = "xXx";
			bizO.Validation.ValidatePackageTypeToCreate();
			AssertNoErrors(bizO.PackageTypeToCreateInfo);

			// TUN unpack
			var dAB_TUN = new PkgPackageItemDivotsWrapperAndBarcode(packedItem, new BarcodeMatch(true, barcodes.Dummy1And2TUNPackType, barcodes.Dummy1And2TUNPackQty));
			var bizO_TUN = new UnpackItemsViaScanBusinessObject(Data.PackageJob, new[] { dAB_TUN });

			AssertNoErrors("Precondition", bizO_TUN.PackageTypeToCreateInfo);
			bizO_TUN.PackageTypeToCreate = "xXx";
			bizO_TUN.Validation.ValidatePackageTypeToCreate();
			AssertHasErrors(bizO_TUN.PackageTypeToCreateInfo);
		}

		#endregion
	}
}
