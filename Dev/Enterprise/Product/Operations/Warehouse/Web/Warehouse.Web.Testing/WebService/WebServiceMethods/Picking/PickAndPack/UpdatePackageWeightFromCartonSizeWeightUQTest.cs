using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class UpdatePackageWeightFromCartonSizeWeightUQTest : WhsSecureServiceTestCase
	{
		public void TestUpdatePackageWeightFromCartonSizeWeightUQ()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var packageInfo = new PackageForPackingInfo
			{
				IsUsingCartonSizes = true,
				Weight = 1.5m,
				WeightUQ = "KG"
			};

			var cartonSizeInfo = new WhsCartonSizeInfo
			{
				WeightUQ = "G"
			};

			var response = webService.UpdatePackageWeightFromCartonSizeWeightUQ(packageInfo, cartonSizeInfo);
			AssertNotNull(response.PackageForPackingInfo);
			AssertNull(response.ErrorMessage);
			AssertEquals(1500m, response.PackageForPackingInfo.Weight);
			AssertEquals("G", response.PackageForPackingInfo.WeightUQ);
		}

		public void TestUpdatePackageWeightFromCartonSizeWeightUQ_WithScannedProducts()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var packageInfo = new PackageForPackingInfo
			{
				IsUsingCartonSizes = true,
				Weight = 1.5m,
				WeightUQ = "KG",
				ScannedProductInfos = new[]
				{
					new WhsPackageProductInfo { ProductWeight = 4m, ProductWeightUQ = "KG", Quantity = 3m },
					new WhsPackageProductInfo { ProductWeight = 20m, ProductWeightUQ = "LB", Quantity = 2m },
				}
			};

			var cartonSizeInfo = new WhsCartonSizeInfo
			{
				WeightUQ = "G"
			};

			var response = webService.UpdatePackageWeightFromCartonSizeWeightUQ(packageInfo, cartonSizeInfo);
			AssertNotNull(response.PackageForPackingInfo);
			AssertNull(response.ErrorMessage);
			AssertEquals(30143.6948m, response.PackageForPackingInfo.Weight);
			AssertEquals("G", response.PackageForPackingInfo.WeightUQ);
		}

		public void TestUpdatePackageWeightFromCartonSizeWeightUQ_InvalidCartonSizeWeightUQ()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var packageInfo = new PackageForPackingInfo
			{
				IsUsingCartonSizes = true,
				WeightUQ = "KG"
			};

			var cartonSizeInfo = new WhsCartonSizeInfo
			{
				WeightUQ = "XX"
			};

			var response = webService.UpdatePackageWeightFromCartonSizeWeightUQ(packageInfo, cartonSizeInfo);
			AssertNull(response.PackageForPackingInfo);
			AssertEquals("Invalid Weight Unit(s) provided. Source 'KG', Target 'XX'.", response.ErrorMessage);
		}

		public void TestUpdatePackageWeightFromCartonSizeWeightUQ_InvalidPackageWeightUQ()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var packageInfo = new PackageForPackingInfo
			{
				IsUsingCartonSizes = true,
				WeightUQ = "XX"
			};

			var cartonSizeInfo = new WhsCartonSizeInfo
			{
				WeightUQ = "KG"
			};

			var response = webService.UpdatePackageWeightFromCartonSizeWeightUQ(packageInfo, cartonSizeInfo);
			AssertNull(response.PackageForPackingInfo);
			AssertEquals("Invalid Weight Unit(s) provided. Source 'XX', Target 'KG'.", response.ErrorMessage);
		}

		public void TestUpdatePackageWeightFromCartonSizeWeightUQ_NotUsingCartonSizes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.UpdatePackageWeightFromCartonSizeWeightUQ(new PackageForPackingInfo { IsUsingCartonSizes = false }, new WhsCartonSizeInfo());
			AssertNull(response.PackageForPackingInfo);
			AssertEquals("Cannot convert Package Weight when not using Carton Sizes.", response.ErrorMessage);
		}
	}
}
