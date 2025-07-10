namespace Enterprise.Packing.Business.Testing
{
	public class UnpackItemsBusinessObjectValidationTest : PackingBusinessObjectValidationTestCase
	{
		public void TestValidatePackageTypeToCreate_IsNotRun()
		{
			Data.CreatePackingData();

			var packedItem = Data.PackageJob.Packages.AddNew("KEG").Pack_ForTesting(Data.DummyLine1, 10m);
			var dAb = new PkgPackageItemDivotsWrapperAndBarcode(packedItem, BarcodeMatch.Yes);

			var bizO = new UnpackItemsBusinessObject(Data.PackageJob, new[] { dAb }, System.Array.Empty<PkgPackage>());
			AssertNoErrors("Precondition", bizO.PackageTypeToCreateInfo);

			bizO.PackageTypeToCreate = "xXx";
			bizO.Validation.ValidatePackageTypeToCreate();
			AssertNoErrors(bizO.PackageTypeToCreateInfo);
		}
	}
}
