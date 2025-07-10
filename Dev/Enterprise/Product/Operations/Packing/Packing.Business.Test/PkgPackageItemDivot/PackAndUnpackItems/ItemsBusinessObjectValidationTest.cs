namespace Enterprise.Packing.Business.Testing
{
	public class ItemsBusinessObjectValidationTest : PackingBusinessObjectValidationTestCase
	{
		#region TestValidatePackageQtyToCreate

		public void TestValidatePackageQtyToCreate()
		{
			Data.CreatePackingData();
			var bizO = new ItemsBusinessObjectTest.ItemsBusinessObjectForTest(Data);

			bizO.PackageQtyToCreate = 0;
			AssertHasError(bizO.PackageQtyToCreateInfo, "Package Qty cannot be less than 1.");

			bizO.PackageQtyToCreate = 1;
			AssertNoErrors(bizO.PackageQtyToCreateInfo);
		}

		#endregion

		#region TestValidatePackageTypeToCreate

		public void TestValidatePackageTypeToCreate()
		{
			Data.CreatePackingData();
			var bizO = new ItemsBusinessObjectTest.ItemsBusinessObjectForTest(Data);

			bizO.PackageTypeToCreate = "";
			AssertHasErrors(bizO.PackageTypeToCreateInfo);

			bizO.PackageTypeToCreate = "PLT";
			AssertNoErrors(bizO.PackageTypeToCreateInfo);

			bizO.PackageTypeToCreate = "xXx";
			AssertHasErrors(bizO.PackageTypeToCreateInfo);
		}

		#endregion

		#region TestValidateAll

		#region TestValidateAll

		public void TestValidateAll()
		{
			Data.CreatePackingData();
			var bizO = new ItemsBusinessObjectTest.ItemsBusinessObjectForTest(Data);

			using (bizO.GetValidationSuspender())
			{
				bizO.PackageTypeToCreate = "";
				bizO.PackageQtyToCreate = 0;
			}

			AssertNoErrors("Precondition", bizO.PackageTypeToCreateInfo);
			AssertNoErrors("Precondition", bizO.PackageQtyToCreateInfo);

			bizO.Validation.ValidateAll();
			AssertHasErrors(bizO.PackageTypeToCreateInfo);
			AssertHasErrors(bizO.PackageQtyToCreateInfo);
		}

		#endregion

		#region TestValidateAll_AddsErrorIfNothingToPackOrUnpack

		public void TestValidateAll_AddsErrorIfNothingToPackOrUnpack()
		{
			Data.CreatePackingData();

			var bizO = new ItemsBusinessObjectTest.ItemsBusinessObjectForTest(Data);

			// select an item to pack
			bizO.PackableItemParentsForBinding[0].ProposedPackQty = 1;
			bizO.RunPreSaveValidation();
			AssertNoRowErrors(bizO);

			// ensure error if nothing selected to pack
			bizO.PackableItemParentsForBinding[0].ProposedPackQty = 0;
			bizO.RunPreSaveValidation();
			AssertHasRowError(bizO, "Nothing is selected to Test Desc.");

			// ensure we clear the notifications
			bizO.PackableItemParentsForBinding[0].ProposedPackQty = 1;
			bizO.RunPreSaveValidation();
			AssertNoRowErrors(bizO);
		}

		#endregion

		#endregion
	}
}
