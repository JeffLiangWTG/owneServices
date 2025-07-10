using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.TR.Business.Testing
{
	class ManifestToOpenPackValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTPI_LineNumber_MandatoryWhenIncludeAllItems()
		{
			var bill = Factory.New<ManifestToOpenBill>();
			var pack = bill.Packs.AddNew();
			bill.TPD_IncludeAllItems = false;

			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(pack.TPI_LineNumberInfo);

				bill.TPD_IncludeAllItems = true;
				AssertNoError(pack.TPI_LineNumberInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckTPI_Quantity_MandatoryWhenIncludeAllItems()
		{
			var bill = Factory.New<ManifestToOpenBill>();
			var pack = bill.Packs.AddNew();
			bill.TPD_IncludeAllItems = false;
			bill.TPD_IsInWarehouse = true;
			pack.TPI_LineNumber = 1;

			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(pack.TPI_QuantityInfo);

				pack.TPI_LineNumber = 0;
				AssertNoError(pack.TPI_QuantityInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckTPI_WarehouseCode_MandatoryWhenIsInWarehouse()
		{
			var bill = Factory.New<ManifestToOpenBill>();
			var pack = bill.Packs.AddNew();
			bill.TPD_IncludeAllItems = false;
			bill.TPD_IsInWarehouse = true;
			pack.TPI_LineNumber = 1;

			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(pack.TPI_WarehouseCodeInfo);

				pack.TPI_LineNumber = 0;
				AssertNoError(pack.TPI_WarehouseCodeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckTPI_WarehouseCode_ListValidation()
		{
			var yesterday = ZDateTime.Now.AddDays(-1);
			var tomorrow = ZDateTime.Now.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TRCWH", "Turkey Warehouse Codes");
			helper.CreateCusCodeList("TR", "TRCWH", "K61000008", yesterday, tomorrow);
			Factory.Save();

			var pack = Factory.New<ManifestToOpenPack>();

			ValidationTestHelper.AssertInvalidCodeMessageError(pack.TPI_WarehouseCodeInfo, "K65000006", "K61000008");
		}
	}
}
