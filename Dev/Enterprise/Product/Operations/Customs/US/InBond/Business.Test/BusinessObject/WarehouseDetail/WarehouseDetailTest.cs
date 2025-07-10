using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(WarehouseDetail))]
	sealed class WarehouseDetailTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<WarehouseDetail>
	{
		public void TestDefault()
		{
			WarehouseDetail line = Factory.New<WarehouseDetail>();
			AssertEquals(CusAddInfoSchema.Constants.Prefix, line.B7_ParentTableCode);
			AssertEquals(CusAddInfoTypeAttribute.Codes.USWarehouseDetail, line.B7_Type);
		}

		public void TestDeleteWhenEmpty()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var warehouseDetail = moveDetail.WarehouseDetails.AddNew();
			warehouseDetail.US_WarehouseNumber = "TURE2343251";
			Factory.Save();
			AssertEquals(false, warehouseDetail.IsDeleted);
			warehouseDetail.US_WarehouseNumber = ZString.Empty;
			Factory.Save();
			AssertEquals(true, warehouseDetail.IsDeleted);
		}

		public void TestBondedQuantityBalance()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var warehouseDetail = moveDetail.WarehouseDetails.AddNew();
			AssertEquals(ZDecimal.Zero, warehouseDetail.WarehouseBondedQuantityBalance);
			warehouseDetail.US_WarehouseBondedQuantity = 100m;
			AssertEquals(100m, warehouseDetail.WarehouseBondedQuantityBalance);
			warehouseDetail.US_WarehouseWithdrawQuantity = 40m;
			AssertEquals(60m, warehouseDetail.WarehouseBondedQuantityBalance);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var warehouseDetail = moveDetail.WarehouseDetails.AddNew();
			warehouseDetail.US_WarehouseNumber = "TURE2343251";
			return warehouseDetail;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<WarehouseDetail>();
		}
	}
}
