using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class PickPackInfoTest : WhsSecureServiceTestCase
	{
		#region TestGetPickPackInfo

		public void TestGetPickPackInfo()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickParams = Helper.AddClientPickParamsForWarehouse(WhsClientPickingParams.GetClientPickingParams(data.Org1), data.Whs1, "KEG");
			pickParams.WPP_IsUsingOwnLabel = true;
			var package1 = order.PackageJob.Packages.AddNew("PLT", "ABC");
			var package2 = order.PackageJob.Packages.AddNew("PLT", "DEF");
			package1.KP_ClosedTimeUtc = ZDateTime.UtcNow;

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPickPackInfo(order.WD_DocketID);

			AssertSuccessfulResponse(response, webService);
			CombineAssertions(() =>
			{
				AssertEquals("Should have correct default Pack Type.", "KEG", response.DefaultPackType);
				AssertContainsExactElementsInAnyOrder(new[] { "DEF" }, response.ExistingPackages.Select(p => p.PackageID));
				AssertContainsExactElementsInAnyOrder(new RefPackTypeCollection(Helper.Factory).Select(p => p.F3_Code.ToString()), response.PackTypes.Select(p => p.Code));
				AssertEquals("Should have correct IsUsingOwnLabel flag.", true, response.IsUsingOwnLabel);
			});
		}

		public void TestGetPickPackInfo_FinalisedOrder()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickParams = Helper.AddClientPickParamsForWarehouse(WhsClientPickingParams.GetClientPickingParams(data.Org1), data.Whs1, "KEG");
			pickParams.WPP_IsUsingOwnLabel = true;
			var package1 = order.PackageJob.Packages.AddNew("PLT", "ABC");
			var package2 = order.PackageJob.Packages.AddNew("PLT", "DEF");
			package1.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			Helper.Factory.Save();

			order.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(order);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPickPackInfo(order.WD_DocketID);

			AssertSuccessfulResponse(response, webService);
			AssertEquals("Response should not be in error", ErrorTypes.None, response.Error);
			CombineAssertions(() =>
			{
				AssertEquals("Should have correct default Pack Type.", "KEG", response.DefaultPackType);
				AssertContainsExactElementsInAnyOrder(new[] { "DEF" }, response.ExistingPackages.Select(p => p.PackageID));
				AssertContainsExactElementsInAnyOrder(new RefPackTypeCollection(Helper.Factory).Select(p => p.F3_Code.ToString()), response.PackTypes.Select(p => p.Code));
				AssertEquals("Should have correct IsUsingOwnLabel flag.", true, response.IsUsingOwnLabel);
			});
		}

		public void TestGetPickPackInfo_DocketIDNotDoesExist()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var package1 = order.PackageJob.Packages.AddNew("PLT", "ABC");
			var package2 = order.PackageJob.Packages.AddNew("PLT", "DEF");
			package1.KP_ClosedTimeUtc = ZDateTime.UtcNow;

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPickPackInfo(receive.WD_DocketID);

			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals($"No Warehouse Order found with Docket ID: {receive.WD_DocketID}.", response.ErrorMessage);
		}

		public void TestGetPickPackInfo_PickPackParamsDoesNotExist()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var package1 = order.PackageJob.Packages.AddNew("PLT", "ABC");
			var package2 = order.PackageJob.Packages.AddNew("PLT", "DEF");
			package1.KP_ClosedTimeUtc = ZDateTime.UtcNow;

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPickPackInfo(order.WD_DocketID);

			AssertSuccessfulResponse(response, webService);
			CombineAssertions(() =>
			{
				AssertEquals("Should have no default Pack Type.", "", response.DefaultPackType);
				AssertNull("Should have no Existing Packages.", response.ExistingPackages);
				AssertNull("Should have no Pack Types set.", response.PackTypes);
				AssertEquals("Should have false for IsUsingOwnLabel flag.", false, response.IsUsingOwnLabel);
			});
		}

		public void TestGetPickPackInfo_ClosedPackages_UsingOwnLabelTrue()
		{
			TestGetPickPackInfo_ClosedPackagesCore(true);
		}

		public void TestGetPickPackInfo_ClosedPackages_UsingOwnLabelFalse()
		{
			TestGetPickPackInfo_ClosedPackagesCore(false);
		}

		void TestGetPickPackInfo_ClosedPackagesCore(bool isUsingOwnLabel)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickParams = Helper.AddClientPickParamsForWarehouse(WhsClientPickingParams.GetClientPickingParams(data.Org1), data.Whs1, "KEG");
			pickParams.WPP_IsUsingOwnLabel = isUsingOwnLabel;
			var package1 = order.PackageJob.Packages.AddNew("PLT", "ABC");
			var package2 = order.PackageJob.Packages.AddNew("PLT", "DEF");
			package1.KP_ClosedTimeUtc = ZDateTime.UtcNow;

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPickPackInfo(order.WD_DocketID);

			if (isUsingOwnLabel)
			{
				AssertContainsExactElementsInAnyOrder(new[] { "ABC" }, response.ExistingClosedPackages.Select(p => p.PackageID));
			}
			else
			{
				AssertNull("Should have no Existing Closed Packages if UsingOwnLabel is false as it's not needed.", response.ExistingClosedPackages);
			}
		}

		public void TestGetPickPackInfo_PromptForWeightAndDimsTrue()
		{
			TestGetPickPackInfo_PromptForWeightAndDimsCore(true);
		}

		public void TestGetPickPackInfo_PromptForWeightAndDimsFalse()
		{
			TestGetPickPackInfo_PromptForWeightAndDimsCore(false);
		}

		void TestGetPickPackInfo_PromptForWeightAndDimsCore(bool promptForWeightAndDims)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);
			var pickPackParams = Helper.AddClientPickParamsForWarehouse(WhsClientPickingParams.GetClientPickingParams(data.Org1), data.Whs1, "KEG");
			pickPackParams.WPP_PromptForWeightAndDimensions = promptForWeightAndDims;

			Helper.Factory.Save();
			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPickPackInfo(order.WD_DocketID);
			AssertEquals("PromptForWeightAndDims flag should be based on WPP_PromptForWeightAndDimensions.", promptForWeightAndDims, response.PromptForWeightAndDims);
		}

		public void TestGetPickPackInfo_SupportsCarrierLabelIntegration_WithCarrierBookingAgent()
		{
			TestGetPickPackInfo_SupportsCarrierLabelIntegrationCore(hasCarrierBookingAgent: true);
		}

		public void TestGetPickPackInfo_SupportsCarrierLabelIntegration_NoCarrierBookingAgent()
		{
			TestGetPickPackInfo_SupportsCarrierLabelIntegrationCore(hasCarrierBookingAgent: false);
		}

		void TestGetPickPackInfo_SupportsCarrierLabelIntegrationCore(bool hasCarrierBookingAgent)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);
			Helper.AddClientPickParamsForWarehouse(WhsClientPickingParams.GetClientPickingParams(data.Org1), data.Whs1, "KEG");

			if (hasCarrierBookingAgent)
			{
				var carrierBookingAgent = Helper.CreateClient();
				order.CarrierBookingAgentDocAddress.E2_OA_Address = carrierBookingAgent.MainAddress.PK;
			}

			Helper.Factory.Save();

			if (hasCarrierBookingAgent)
			{
				AssertNotNull("Precondition: order has carrier booking agent.", order.CarrierBookingAgent);
			}
			else
			{
				AssertNull("Precondition: order has no carrier booking agent.", order.CarrierBookingAgent);
			}

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPickPackInfo(order.WD_DocketID);

			AssertEquals("SupportsCarrierLabelIntegration info is true if order has carrier booking agent.", hasCarrierBookingAgent, response.SupportsCarrierLabelIntegration);
		}

		#endregion
	}
}
