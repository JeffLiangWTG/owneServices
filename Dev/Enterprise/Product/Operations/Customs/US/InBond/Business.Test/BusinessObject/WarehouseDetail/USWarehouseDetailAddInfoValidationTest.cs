using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class USWarehouseDetailAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_WarehouseBondedQuantity()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var warehouseDetail = moveDetail.WarehouseDetails.AddNew();
			warehouseDetail.US_WarehouseBondedQuantity = -10m;
			AssertHasWarningContaining(warehouseDetail.US_WarehouseBondedQuantityInfo, MandatoryValidation.ValueCannotBeNegative);
			warehouseDetail.US_WarehouseBondedQuantity = 10m;
			AssertNoWarningContaining(warehouseDetail.US_WarehouseBondedQuantityInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckUS_WarehouseWithdrawQuantity()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var warehouseDetail = moveDetail.WarehouseDetails.AddNew();
			warehouseDetail.US_WarehouseBondedQuantity = 10m;
			warehouseDetail.US_WarehouseWithdrawQuantity = -11m;
			warehouseDetail.AddInfoValidation.ValidateUS_WarehouseWithdrawQuantity();
			AssertHasWarningContaining(warehouseDetail.US_WarehouseWithdrawQuantityInfo, MandatoryValidation.ValueCannotBeNegative);
			warehouseDetail.US_WarehouseWithdrawQuantity = 11m;
			AssertNoWarningContaining(warehouseDetail.US_WarehouseWithdrawQuantityInfo, MandatoryValidation.ValueCannotBeNegative);
			AssertHasWarning(warehouseDetail.US_WarehouseWithdrawQuantityInfo, USWarehouseDetailAddInfoValidation.WithdrawQuantityShouldNotBeGreaterThanBondedQuantity);
			warehouseDetail.US_WarehouseBondedQuantity = 15m;
			warehouseDetail.AddInfoValidation.ValidateUS_WarehouseWithdrawQuantity();
			AssertNoWarning(warehouseDetail.US_WarehouseWithdrawQuantityInfo, USWarehouseDetailAddInfoValidation.WithdrawQuantityShouldNotBeGreaterThanBondedQuantity);
		}
	}
}
