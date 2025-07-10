using System;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class CanTransferProductToUnexpectedScannedLocationTest : WhsSecureServiceTestCase
	{
		#region TestCanTransferProductToUnexpectedScannedLocation

		public void TestCanTransferProductToUnexpectedScannedLocation()
		{
			var factory = Helper.Factory;
			var data = new TestDataSimpleEnvironment(factory, 3, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var normalLocation = data.Whs1.FindLocation("A-1");
			var dynamicLocation = data.Whs1.FindLocation("A-2");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			var dynamicLocation2 = data.Whs1.FindLocation("A-3");
			dynamicLocation2.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation2.WLV_WA_PickingArea = dynamicArea.PK;
			factory.Save();

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, normalLocation, "");
			factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, normalLocation, dynamicLocation);
			transfer.RunPreSaveValidation();
			factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff);
			var response = webService.CanTransferProductToUnexpectedScannedLocation(transfer.PK.ToGuid(), data.Org1.OH_Code, data.Part1.PK.ToGuid(), dynamicLocation.ToLocationString(), dynamicLocation2.ToLocationString());
			AssertEquals("Response error type is correct.", response.Error, ErrorTypes.YesNoEnquiry);
			AssertEquals("Response error is correct.", $"Location '{dynamicLocation2.ToLocationString()}' is not the expected location. Would you like to continue the transfer to this location?", response.ErrorMessage);
		}

		public void TestCanTransferProductToUnexpectedScannedLocation_NonDynamicLocation()
		{
			var factory = Helper.Factory;
			var data = new TestDataSimpleEnvironment(factory, 3, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var normalLocation = data.Whs1.FindLocation("A-1");
			var dynamicLocation = data.Whs1.FindLocation("A-2");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;

			var normalLocation2 = data.Whs1.FindLocation("A-3");
			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, normalLocation, "");
			factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, normalLocation, dynamicLocation);
			transfer.RunPreSaveValidation();
			factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff);
			var response = webService.CanTransferProductToUnexpectedScannedLocation(transfer.PK.ToGuid(), data.Org1.OH_Code, data.Part1.PK.ToGuid(), dynamicLocation.ToLocationString(), normalLocation2.ToLocationString());
			AssertEquals("Response error type is correct.", response.Error, ErrorTypes.BusinessValidationError);
			AssertEquals("Response error is correct.", response.ErrorMessage, $"Not a dynamic location. Scan a dynamic location.");
		}

		public void TestCanTransferProductToUnexpectedScannedLocation_UnassignedDynamicLocation()
		{
			var factory = Helper.Factory;
			var data = new TestDataSimpleEnvironment(factory, 3, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");

			var dynamicArea1 = Helper.CreateArea(data.Whs1, "DYNAMIC1", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicArea2 = Helper.CreateArea(data.Whs1, "DYNAMIC2", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var normalLocation = data.Whs1.FindLocation("A-1");
			var dynamicLocation = data.Whs1.FindLocation("A-2");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea1.PK;

			var dynamicLocation2 = data.Whs1.FindLocation("A-3");
			dynamicLocation2.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation2.WLV_WA_PickingArea = dynamicArea2.PK;
			factory.Save();

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea1.PK;
			factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, normalLocation, "");
			factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, normalLocation, dynamicLocation);
			transfer.RunPreSaveValidation();
			factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff);
			var response = webService.CanTransferProductToUnexpectedScannedLocation(transfer.PK.ToGuid(), data.Org1.OH_Code, data.Part1.PK.ToGuid(), dynamicLocation.ToLocationString(), dynamicLocation2.ToLocationString());
			AssertEquals("Response error type is correct.", response.Error, ErrorTypes.BusinessValidationError);
			AssertEquals("Response error is correct.", response.ErrorMessage, $"Scanned location '{dynamicLocation2.ToLocationString()}' is not assigned to dynamic area '{dynamicArea1.WA_Name}'");
		}

		public void TestCanTransferProductToUnexpectedScannedLocation_UnknownLocation()
		{
			var factory = Helper.Factory;
			var data = new TestDataSimpleEnvironment(factory, 3, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var normalLocation = data.Whs1.FindLocation("A-1");
			var dynamicLocation = data.Whs1.FindLocation("A-2");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			factory.Save();

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, normalLocation, "");
			factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, normalLocation, dynamicLocation);
			transfer.RunPreSaveValidation();
			factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff);
			var response = webService.CanTransferProductToUnexpectedScannedLocation(transfer.PK.ToGuid(), data.Org1.OH_Code, data.Part1.PK.ToGuid(), dynamicLocation.ToLocationString(), "Unknown-Loc");
			AssertEquals("Response error type is correct.", response.Error, ErrorTypes.BusinessValidationError);
			AssertEquals("Response error is correct.", response.ErrorMessage, "Invalid Location. Scan a valid dynamic location.");
		}

		public void TestCanTransferProductToUnexpectedScannedLocation_LocationWithSOH_SOHWarningOn()
		{
			WarehouseDataRegistry.Instance.SOHLocationWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var factory = Helper.Factory;
			var data = new TestDataSimpleEnvironment(factory, 3, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var normalLocation = data.Whs1.FindLocation("A-1");
			var dynamicLocation = data.Whs1.FindLocation("A-2");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			factory.Save();

			var sohLocation = data.Whs1.FindLocation("A-3");
			sohLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			sohLocation.WLV_WA_PickingArea = dynamicArea.PK;
			factory.Save();

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			factory.Save();

			var receiveFillLocation = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, sohLocation, "");
			factory.Save();
			AssertIsFinalisedPrecondition(receiveFillLocation);
			AssertEquals("Precondition: Location should have stock on hand.", 5m, WebServiceHelper.GetConsumedCapacityForLocation(factory, sohLocation.PK));

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, normalLocation, "");
			factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, normalLocation, dynamicLocation);
			transfer.RunPreSaveValidation();
			factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff);
			var response = webService.CanTransferProductToUnexpectedScannedLocation(transfer.PK.ToGuid(), data.Org1.OH_Code, data.Part1.PK.ToGuid(), dynamicLocation.ToLocationString(), sohLocation.ToLocationString());
			AssertEquals("Response error type is correct.", response.Error, ErrorTypes.YesNoEnquiry);
			AssertEquals("Response error is correct.", response.ErrorMessage, $"'{sohLocation.ToLocationString()}' has stock on hand and is not the expected location. Would you like to continue the transfer to this location?");
		}

		public void TestCanTransferProductToUnexpectedScannedLocation_LocationWithSOH_SOHWarningOff()
		{
			WarehouseDataRegistry.Instance.SOHLocationWarning.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var factory = Helper.Factory;
			var data = new TestDataSimpleEnvironment(factory, 3, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var normalLocation = data.Whs1.FindLocation("A-1");
			var dynamicLocation = data.Whs1.FindLocation("A-2");
			dynamicLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation.WLV_WA_PickingArea = dynamicArea.PK;
			factory.Save();

			var sohLocation = data.Whs1.FindLocation("A-3");
			sohLocation.WLV_WLT_LocationType = dynamicLocationType.PK;
			sohLocation.WLV_WA_PickingArea = dynamicArea.PK;
			factory.Save();

			var productParams = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
			productParams.W3_WA_DynamicPickFaceArea = dynamicArea.PK;
			factory.Save();

			var receiveFillLocation = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, sohLocation, "");
			factory.Save();
			AssertIsFinalisedPrecondition(receiveFillLocation);
			AssertEquals("Precondition: Location should have stock on hand.", 5m, WebServiceHelper.GetConsumedCapacityForLocation(factory, sohLocation.PK));

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, normalLocation, "");
			factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, normalLocation, dynamicLocation);
			transfer.RunPreSaveValidation();
			factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff);
			var response = webService.CanTransferProductToUnexpectedScannedLocation(transfer.PK.ToGuid(), data.Org1.OH_Code, data.Part1.PK.ToGuid(), dynamicLocation.ToLocationString(), sohLocation.ToLocationString());
			AssertEquals("Response error type is correct.", response.Error, ErrorTypes.YesNoEnquiry);
			AssertEquals("Response error is correct.", $"Location '{sohLocation.ToLocationString()}' is not the expected location. Would you like to continue the transfer to this location?", response.ErrorMessage);
		}

		public void TestCanTransferProductToUnexpectedScannedLocation_SourceLocationNotDynamic_ScannedLocationNotCorrect()
		{
			var factory = Helper.Factory;
			var data = new TestDataSimpleEnvironment(factory, 3, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");

			var normalLocation1 = data.Whs1.FindLocation("A-1");
			var normalLocation2 = data.Whs1.FindLocation("A-2");
			var normalLocation3 = data.Whs1.FindLocation("A-3");
			factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, normalLocation1, "");
			factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 2m, normalLocation1, normalLocation2);
			transfer.RunPreSaveValidation();
			factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff);
			var response = webService.CanTransferProductToUnexpectedScannedLocation(transfer.PK.ToGuid(), data.Org1.OH_Code, data.Part1.PK.ToGuid(), normalLocation2.ToLocationString(), normalLocation3.ToLocationString());
			AssertEquals("Response error type is correct.", response.Error, ErrorTypes.BusinessValidationError);
			AssertEquals("Response error is correct.", $"Scanned value of '{normalLocation3.ToLocationString()}' does not match expected Location. Please scan again.", response.ErrorMessage);
		}

		public void TestCanTransferProductToUnexpectedScannedLocation_NonDynamicLocation_ReplenishingPick()
		{
			var factory = Helper.Factory;
			var data = new TestDataSimpleEnvironment(factory, 3, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");

			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var normalLocation = data.Whs1.FindLocation("A-1");
			var dynamicLocation1 = data.Whs1.FindLocation("A-2");
			dynamicLocation1.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation1.WLV_WA_PickingArea = dynamicArea.PK;

			var dynamicLocation2 = data.Whs1.FindLocation("A-3");
			dynamicLocation2.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation2.WLV_WA_PickingArea = dynamicArea.PK;
			factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, normalLocation, "");
			factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var pick = factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WA_DynamicPickAreaOverride = dynamicArea.PK;
			Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_WP_PickBeingReplenished = pick.PK;
			factory.Save();

			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, normalLocation, dynamicLocation1);
			transfer.RunPreSaveValidation();
			factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff);
			var response = webService.CanTransferProductToUnexpectedScannedLocation(transfer.PK.ToGuid(), data.Org1.OH_Code, data.Part1.PK.ToGuid(), dynamicLocation1.ToLocationString(), dynamicLocation2.ToLocationString());
			CombineAssertions(() =>
			{
				AssertEquals("Response error type is correct.", response.Error, ErrorTypes.YesNoEnquiry);
				AssertEquals("Response error is correct.", "Location 'A-3' is not the expected location, but is in the required dynamic area. Would you like to continue the transfer to this location?", response.ErrorMessage);
			});
		}

		public void TestCanTransferProductToUnexpectedScannedLocation_NonDynamicLocation_ReplenishingPick_DifferentArea()
		{
			var factory = Helper.Factory;
			var data = new TestDataSimpleEnvironment(factory, 3, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");

			var dynamicArea1 = Helper.CreateArea(data.Whs1, "DYNAMIC1", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicArea2 = Helper.CreateArea(data.Whs1, "DYNAMIC2", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var normalLocation = data.Whs1.FindLocation("A-1");
			var dynamicLocation1 = data.Whs1.FindLocation("A-2");
			dynamicLocation1.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation1.WLV_WA_PickingArea = dynamicArea1.PK;

			var dynamicLocation2 = data.Whs1.FindLocation("A-3");
			dynamicLocation2.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation2.WLV_WA_PickingArea = dynamicArea2.PK;
			factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, normalLocation, "");
			factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var pick = factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WA_DynamicPickAreaOverride = dynamicArea1.PK;
			Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_WP_PickBeingReplenished = pick.PK;
			factory.Save();
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, normalLocation, dynamicLocation1);
			transfer.RunPreSaveValidation();
			factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff);
			var response = webService.CanTransferProductToUnexpectedScannedLocation(transfer.PK.ToGuid(), data.Org1.OH_Code, data.Part1.PK.ToGuid(), dynamicLocation1.ToLocationString(), dynamicLocation2.ToLocationString());
			CombineAssertions(() =>
			{
				AssertEquals("Response error type is correct.", response.Error, ErrorTypes.BusinessValidationError);
				AssertEquals("Response error is correct.", response.ErrorMessage, "Scanned location 'A-3' is not assigned to required dynamic area 'DYNAMIC1'");
			});
		}

		public void TestCanTransferProductToUnexpectedScannedLocation_NonDynamicLocation_ReplenishingPick_MissingPick()
		{
			var factory = Helper.Factory;
			var data = new TestDataSimpleEnvironment(factory, 3, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");

			var dynamicArea1 = Helper.CreateArea(data.Whs1, "DYNAMIC1", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicArea2 = Helper.CreateArea(data.Whs1, "DYNAMIC2", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);

			var normalLocation = data.Whs1.FindLocation("A-1");
			var dynamicLocation1 = data.Whs1.FindLocation("A-2");
			dynamicLocation1.WLV_WLT_LocationType = dynamicLocationType.PK;
			dynamicLocation1.WLV_WA_PickingArea = dynamicArea1.PK;
			factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, normalLocation, "");
			factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var pick = factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WA_DynamicPickAreaOverride = dynamicArea1.PK;
			Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);

			var randomPick = factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_WP_PickBeingReplenished = randomPick.PK; // unknown WhsPick PK.
			factory.Save();
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, normalLocation, dynamicLocation1);
			transfer.RunPreSaveValidation();
			factory.Save();

			var webService = GetNewWebService();
			SetupSecurityHeader(webService, data.Whs1, staff);
			var response = webService.CanTransferProductToUnexpectedScannedLocation(transfer.PK.ToGuid(), data.Org1.OH_Code, data.Part1.PK.ToGuid(), dynamicLocation1.ToLocationString(), dynamicLocation1.ToLocationString());
			CombineAssertions(() =>
			{
				AssertEquals("Response error type is correct.", response.Error, ErrorTypes.BusinessValidationError);
				AssertEquals("Response error is correct.", response.ErrorMessage, "The pick to be replenished attached to this transfer does not have a dynamic pick area override.");
			});
		}

		#endregion
	}
}
