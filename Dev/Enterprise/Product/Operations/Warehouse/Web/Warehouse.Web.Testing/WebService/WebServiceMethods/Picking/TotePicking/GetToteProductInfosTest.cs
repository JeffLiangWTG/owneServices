using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService;
using Enterprise.Warehouse.Web.WebService.Testing;

namespace Enterprise.Warehouse.Web.Testing
{
	class GetToteProductInfosTest : WhsSecureServiceTestCase
	{
		public void TestGetToteProductInfos_EmptyPackageID()
		{
			var webService = GetNewWebService();
			var response = webService.GetToteProductInfos(Guid.Empty);
			AssertNull("ToteInfo should  be null", response.ProductInfos);
			AssertEquals("Should Error as no packagePK given.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Cannot find Tote with specified Tote ID.", response.ErrorMessage);
		}

		public void TestGetToteProductInfos_InvalidPackagePK()
		{
			var webService = GetNewWebService();
			var response = webService.GetToteProductInfos(Guid.NewGuid());
			AssertNull("ToteInfo should  be null", response.ProductInfos);
			AssertEquals("Should Error as cannot find package with specified PK.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Cannot find Tote with specified Tote ID.", response.ErrorMessage);
		}

		public void TestGetToteProductInfos()
		{
			var factory = Helper.Factory;
			var data = new TestDataSimpleEnvironment(factory);
			var result = new WebServiceResponse();
			var staff = Helper.CreateGlbStaff("JSM", "JohnSmith");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			factory.Save();

			var pick = Helper.CreatePickNew(order);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var package = packingHelper.CreatePackage(packageJob, "P1", 1, "BOX");

			foreach (var orderLine in order.Lines)
			{
				var pickLine = orderLine.PickLines.Single();
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
				packingHelper.CreatePackageDivot(package, pickLine);
			}
			factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.GetToteProductInfos(package.PK.ToGuid());

			AssertEquals(ErrorTypes.None, response.Error);
			AssertNull(response.ErrorMessage);
			AssertEquals("P1", response.PackageID);

			var productInfos = response.ProductInfos;
			AssertEquals("Should have 2 products related to this package.", 2, productInfos.Length);
			AssertNotNull(productInfos.SingleOrDefault(i => i.ProductPK == data.Part1.PK && i.ExpectedQty == 5m));
			AssertNotNull(productInfos.SingleOrDefault(i => i.ProductPK == data.Part2.PK && i.ExpectedQty == 10m));
		}

		public void TestGetToteProductInfos_FindNoProduct()
		{
			var factory = Helper.Factory;
			var data = new TestDataSimpleEnvironment(factory);
			var result = new WebServiceResponse();
			var staff = Helper.CreateGlbStaff("JSM", "JohnSmith");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			factory.Save();

			var pick = Helper.CreatePickNew(order);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var package = packingHelper.CreatePackage(packageJob, "P1", 1, "BOX");

			foreach (var orderLine in order.Lines)
			{
				var pickLine = orderLine.PickLines.Single();
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			}
			factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.GetToteProductInfos(package.PK.ToGuid());
			AssertEquals("Should Error as cannot find no products with specified package PK.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Cannot find any product has been allocated to the Tote.", response.ErrorMessage);
			AssertNull(response.ProductInfos);
		}
	}
}
