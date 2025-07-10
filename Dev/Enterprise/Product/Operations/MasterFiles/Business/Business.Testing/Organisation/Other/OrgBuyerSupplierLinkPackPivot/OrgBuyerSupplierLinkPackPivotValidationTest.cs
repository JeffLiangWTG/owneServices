using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgBuyerSupplierLinkPackPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckQ0_F3()
		{
			OrgSupplierBuyerLink link = Factory.New<OrgSupplierBuyerLink>();
			OrgBuyerSupplierLinkPackPivot pivot1 = link.PackPivots.AddNew();
			OrgBuyerSupplierLinkPackPivot pivot2 = link.PackPivots.AddNew();

			pivot1.Q0_F3 = Factory.New<RefPackType>().PK;
			pivot2.Q0_F3 = pivot1.Q0_F3;

			pivot1.Validation.ValidateQ0_F3();
			AssertHasError(pivot1.Q0_F3Info, "Defaults for this Package Type already exist.");

			pivot2.Validation.ValidateQ0_F3();
			AssertHasError(pivot2.Q0_F3Info, "Defaults for this Package Type already exist.");

			pivot2.Q0_F3 = Factory.New<RefPackType>().PK;

			pivot1.Validation.ValidateQ0_F3();
			AssertNoErrors(pivot1.Q0_F3Info);

			pivot2.Validation.ValidateQ0_F3();
			AssertNoErrors(pivot2.Q0_F3Info);
		}

		public void TestCheckQ0_UnitOfDimension()
		{
			var packType = Factory.New<RefPackType>();
			packType.F3_Code = "NEW";
			packType.F3_Description = "Description";

			OrgBuyerSupplierLinkPackPivot pivot = Factory.New<OrgBuyerSupplierLinkPackPivot>();
			pivot.Q0_F3 = packType.PK;
			pivot.Q0_UnitOfDimension = string.Empty;

			pivot.Q0_Height = 0;
			pivot.Q0_Length = 0;
			pivot.Q0_Width = 0;
			pivot.Validation.ValidateQ0_UnitOfDimension();
			AssertNoErrors(pivot.Q0_UnitOfDimensionInfo);

			pivot.Q0_Height = 3;
			pivot.Q0_Length = 0;
			pivot.Q0_Width = 0;
			pivot.Validation.ValidateQ0_UnitOfDimension();
			AssertHasError(pivot.Q0_UnitOfDimensionInfo, "Please enter a Unit of Dimension.");

			pivot.Q0_Height = 0;
			pivot.Q0_Length = 4;
			pivot.Q0_Width = 0;
			pivot.Validation.ValidateQ0_UnitOfDimension();
			AssertHasError(pivot.Q0_UnitOfDimensionInfo, "Please enter a Unit of Dimension.");

			pivot.Q0_Height = 0;
			pivot.Q0_Length = 0;
			pivot.Q0_Width = 5;
			pivot.Validation.ValidateQ0_UnitOfDimension();
			AssertHasError(pivot.Q0_UnitOfDimensionInfo, "Please enter a Unit of Dimension.");

			pivot.Q0_Height = 0;
			pivot.Q0_Length = 4;
			pivot.Q0_Width = 5;
			pivot.Validation.ValidateQ0_UnitOfDimension();
			AssertHasError(pivot.Q0_UnitOfDimensionInfo, "Please enter a Unit of Dimension.");

			pivot.Q0_Height = 3;
			pivot.Q0_Length = 0;
			pivot.Q0_Width = 5;
			pivot.Validation.ValidateQ0_UnitOfDimension();
			AssertHasError(pivot.Q0_UnitOfDimensionInfo, "Please enter a Unit of Dimension.");

			pivot.Q0_Height = 3;
			pivot.Q0_Length = 5;
			pivot.Q0_Width = 0;
			pivot.Validation.ValidateQ0_UnitOfDimension();
			AssertHasError(pivot.Q0_UnitOfDimensionInfo, "Please enter a Unit of Dimension.");

			pivot.Q0_Height = 3;
			pivot.Q0_Length = 4;
			pivot.Q0_Width = 5;
			pivot.Validation.ValidateQ0_UnitOfDimension();
			AssertHasError(pivot.Q0_UnitOfDimensionInfo, "Please enter a Unit of Dimension.");

			pivot.Q0_UnitOfDimension = "NT";
			AssertHasError(pivot.Q0_UnitOfDimensionInfo, "Enter a valid Unit of Dimension.");

			pivot.Q0_UnitOfDimension = "KM";
			AssertNoErrors(pivot.Q0_UnitOfDimensionInfo);
		}

		public void TestCheckQ0_UnitOfWeight()
		{
			var packType = Factory.New<RefPackType>();
			packType.F3_Code = "NEW";
			packType.F3_Description = "Description";

			OrgBuyerSupplierLinkPackPivot pivot = Factory.New<OrgBuyerSupplierLinkPackPivot>();
			pivot.Q0_F3 = packType.PK;
			pivot.Q0_Weight = 0;
			pivot.Q0_UnitOfWeight = string.Empty;
			AssertNoErrors(pivot.Q0_UnitOfWeightInfo);

			pivot.Q0_Weight = 55;
			pivot.Validation.ValidateQ0_UnitOfWeight();
			AssertHasError(pivot.Q0_UnitOfWeightInfo, "Please enter a Unit of Weight.");

			pivot.Q0_UnitOfWeight = "TT";
			AssertHasError(pivot.Q0_UnitOfWeightInfo, "Enter a valid Unit of Weight.");

			pivot.Q0_UnitOfWeight = "KG";
			AssertNoErrors(pivot.Q0_UnitOfWeightInfo);
		}
	}
}
