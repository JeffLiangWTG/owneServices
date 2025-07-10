using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class BOMComponentQuantityHelperTest : WhsTestCaseWithFactory
	{
		public void TestGetComponentsQuantityToBuildKits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bomProduct = CreateProduct(data, "B1", Constants.PkgUnit.Pallet);
			var componentProduct1 = CreateProduct(data, "C1", Constants.PkgUnit.Unit);
			Helper.CreateProductUnit(componentProduct1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 2m);
			var bomPart1 = Helper.CreateProductBOM(bomProduct, componentProduct1, 6m, Constants.PkgUnit.Unit);

			bomPart1.OE_F3_NKPackType = Constants.PkgUnit.Pallet;
			AssertEquals("2 units build 1 Pallet, 6 components build 1 product. So, 12 need to build each pallet of bomProduct.", 36m, BOMComponentQuantityHelper.GetComponentsQuantityToBuildKits(bomPart1, 3));

			bomPart1.OE_F3_NKPackType = Constants.PkgUnit.Unit;
			AssertEquals("As component's packType is Unit, 6 components build 1 product. So, 6 need to build each pallet of bomProduct.", 18m, BOMComponentQuantityHelper.GetComponentsQuantityToBuildKits(bomPart1, 3));
		}

		public void TestGetNumberOfPossibleKitsFromChildLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bomProduct = CreateProduct(data, "B1", Constants.PkgUnit.Pallet);
			var componentProduct1 = CreateProduct(data, "C1", Constants.PkgUnit.Unit);
			Helper.CreateProductUnit(componentProduct1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 2m);
			var bomPart1 = Helper.CreateProductBOM(bomProduct, componentProduct1, 6m, Constants.PkgUnit.Unit);

			bomPart1.OE_F3_NKPackType = Constants.PkgUnit.Pallet;
			AssertEquals("2 units build 1 Pallet, 6 components build 1 product. So, 12 need to build each pallet of bomProduct.", 3, BOMComponentQuantityHelper.GetNumberOfPossibleKitsFromComponent(bomPart1, 36));
			AssertEquals("Should not consider fraction value.", 3, BOMComponentQuantityHelper.GetNumberOfPossibleKitsFromComponent(bomPart1, 46));

			bomPart1.OE_F3_NKPackType = Constants.PkgUnit.Unit;
			AssertEquals("As component's packType is Unit, 6 components build 1 product. So, 6 need to build each pallet of bomProduct.", 3, BOMComponentQuantityHelper.GetNumberOfPossibleKitsFromComponent(bomPart1, 18));
			AssertEquals("Should not consider fraction value.", 3, BOMComponentQuantityHelper.GetNumberOfPossibleKitsFromComponent(bomPart1, 20));
		}

		OrgSupplierPart CreateProduct(TestDataSimpleEnvironment data, string partCode, string stockKeepingUnit)
		{
			var part = Helper.CreateProduct(data.Org1, partCode);
			part.OP_StockKeepingUnit = stockKeepingUnit;
			return part;
		}
	}
}
