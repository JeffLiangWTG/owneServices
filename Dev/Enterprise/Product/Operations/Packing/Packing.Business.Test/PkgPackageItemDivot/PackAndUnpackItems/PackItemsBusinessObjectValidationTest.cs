using System;

namespace Enterprise.Packing.Business.Testing
{
	public class PackItemsBusinessObjectValidationTest : PackingBusinessObjectValidationTestCase
	{
		#region TestValidatePackageQtyToCreate

		public void TestValidatePackageQtyToCreate()
		{
			Data.CreatePackingData();
			var bizO = new PackItemsBusinessObject(Data.PackageJob, new[] { Data.DummyLine1 }, Array.Empty<PkgPackage>());

			bizO.PackageQtyToCreate = PackItemsBusinessObject.MaxPackagesToCreate;
			AssertNoErrors(bizO.PackageQtyToCreateInfo);

			bizO.PackageQtyToCreate = PackItemsBusinessObject.MaxPackagesToCreate + 1;
			AssertHasError(bizO.PackageQtyToCreateInfo, string.Format("Cannot create more than {0} Packages at a time.", PackItemsBusinessObject.MaxPackagesToCreate));

			AssertValidatesProposedPackQtyOnAllItems((b) => b.Validation.ValidatePackageQtyToCreate());
		}

		#endregion

		#region TestValidatePackageTypeToCreate

		public void TestValidatePackageTypeToCreate()
		{
			AssertValidatesProposedPackQtyOnAllItems((b) => b.Validation.ValidatePackageTypeToCreate());
		}

		#endregion

		#region AssertValidatesProposedPackQtyOnAllItems

		void AssertValidatesProposedPackQtyOnAllItems(Action<PackItemsBusinessObject> validation)
		{
			Data.CreatePackingData();
			var bizO = new PackItemsBusinessObject(Data.PackageJob, new[] { Data.DummyLine1, Data.DummyLine2, Data.DummyLine3 }, Array.Empty<PkgPackage>());
			bizO.PackageQtyToCreate = 5;

			foreach (var item in bizO.PackableItemParentsForBinding)
			{
				using (item.GetValidationSuspender())
				{ item.ProposedPackQty = 4; }
				AssertNoWarnings("Precondition", item.ProposedPackQtyInfo);
			}

			validation(bizO);
			foreach (var item in bizO.PackableItemParentsForBinding) // eg. "There are 5 Boxes but only 4 TV(s). 1 Box will not contain a TV."
			{
				AssertHasWarnings("Validating PackageQtyToCreate should run validation on ProposedPackQty for each item.", item.ProposedPackQtyInfo);
			}
		}

		#endregion
	}
}
