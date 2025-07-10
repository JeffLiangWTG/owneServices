using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class ValidateLocationOrPalletIDOnUnloadTest : WhsSecureServiceTestCase
	{
		#region ValidateLocationOrPalletIDOnUnload

		#region TestValidateLocationOrPalletID_Location

		public void TestValidateLocationOrPalletID_Location_OldBarcodes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 1, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[0].WLV_LocationStatus = LocationStatus.Codes.Normal;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ValidateLocationOrPalletID(receive.PK.ToGuid(), locations[0].OldBarcode, Guid.Empty, true, false);
			AssertValidLocation(response, locations[0], false, false);
			AssertEquals(false, response.WarnUserStockOnHandInTheLocationExist);
			AssertNull("No errors should be encountered during validation of the location", response.ErrorMessage);
		}

		public void TestValidateLocationOrPalletID_Location_IsVoid()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 1, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[0].WLV_LocationStatus = LocationStatus.Codes.Void;     // void location

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ValidateLocationOrPalletID(receive.PK.ToGuid(), locations[0].ToLocationString(), Guid.Empty, true, false);
			AssertValidLocation(response, locations[0], true, false);
			AssertEquals(false, response.WarnUserStockOnHandInTheLocationExist);
			AssertNull("No errors should be encountered during validation of the location", response.ErrorMessage);
		}

		public void TestValidateLocationOrPalletID_Location_WithStockOnHand_WithWarningForSOH()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[0].WLV_LocationStatus = LocationStatus.Codes.Normal;
			locations[1].WLV_LocationStatus = LocationStatus.Codes.Normal;   // SOH location

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, locations[1], "");
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ValidateLocationOrPalletID(receive.PK.ToGuid(), locations[1].ToLocationString(), Guid.Empty, true, false);
			AssertValidLocation(response, locations[1], false, false);
			AssertEquals(true, response.WarnUserStockOnHandInTheLocationExist);
			AssertNull("No errors should be encountered during validation of the location", response.ErrorMessage);
		}

		public void TestValidateLocationOrPalletID_Location_WithStockOnHand_NoWarningForSOH()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[0].WLV_LocationStatus = LocationStatus.Codes.Normal;
			locations[1].WLV_LocationStatus = LocationStatus.Codes.Normal;   // SOH location

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, locations[1], "");
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ValidateLocationOrPalletID(receive.PK.ToGuid(), locations[1].ToLocationString(), Guid.Empty, false, false);
			AssertEquals(false, response.WarnUserStockOnHandInTheLocationExist);
			AssertNull("No errors should be encountered during validation of the location", response.ErrorMessage);
		}

		public void TestValidateLocationOrPalletID_Location_HasPalletSpaces()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[0].WLV_PalletFloorSpaces = 5;
			locations[0].WLV_PalletStackHeight = 6;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, locations[0], "PLT0001");
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ValidateLocationOrPalletID(receive.PK.ToGuid(), locations[0].ToLocationString(), Guid.Empty, false, false);
			AssertEquals(true, response.HasPalletSpaces);
			AssertNull("No errors should be encountered during validation of the location", response.ErrorMessage);
		}

		public void TestValidateLocationOrPalletID_Location_NoPalletSpaces()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, locations[0], "PLT0001");
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ValidateLocationOrPalletID(receive.PK.ToGuid(), locations[0].ToLocationString(), Guid.Empty, false, false);
			AssertEquals(false, response.HasPalletSpaces);
			AssertNull("No errors should be encountered during validation of the location", response.ErrorMessage);
		}

		#endregion

		#region TestValidateLocationOrPalletID_Location

		public void TestValidateLocationOrPalletID_PreventDockDoorLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-1");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response1 = webService1.ValidateLocationOrPalletID(receive.PK.ToGuid(), nonDockDoorLocation.WLV_LocationString, Guid.Empty, true, false);
			AssertEquals(nonDockDoorLocation.ToLocationString(), response1.Location);
			AssertEquals(nonDockDoorLocation.PK, response1.LocationPK);
			AssertNull("No errors should be encountered during validation of the non-dock-door location", response1.ErrorMessage);

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response2 = webService2.ValidateLocationOrPalletID(receive.PK.ToGuid(), dockDoorLocation.WLV_LocationString, Guid.Empty, true, false);
			AssertEquals(dockDoorLocation.ToLocationString(), response2.Location);
			AssertEquals(dockDoorLocation.PK, response2.LocationPK);
			AssertEquals("You cannot use a Dock door location.", response2.ErrorMessage);
		}

		#endregion

		#region TestValidateLocationOrPalletID_Location_Fixed

		public void TestValidateLocationOrPalletID_Location_Fixed()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FFF", "Test1", false, 1, LocationClasses.Codes.FIX);
			var normalLocationType = Helper.CreateLocationType("NNN", "Test2", false, 0, LocationClasses.Codes.NOR);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[0].WLV_WLT_LocationType = normalLocationType.PK;
			locations[1].WLV_WLT_LocationType = fixLocationType.PK; // fixed location

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.Factory.Save();

			// fixed location with no Product specified - no error
			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ValidateLocationOrPalletID(receive.PK.ToGuid(), locations[1].ToLocationString(), Guid.Empty, true, false);
			AssertValidLocation(response, locations[1], false, true);
			AssertNull("Since we didn't pass any product there should be no errors.", response.ErrorMessage);
		}

		public void TestValidateLocationOrPalletID_Location_Fixed_WithProduct()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FFF", "Test1", false, 1, LocationClasses.Codes.FIX);
			var normalLocationType = Helper.CreateLocationType("NNN", "Test2", false, 0, LocationClasses.Codes.NOR);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[0].WLV_WLT_LocationType = normalLocationType.PK;
			locations[1].WLV_WLT_LocationType = fixLocationType.PK; // fixed location

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.Factory.Save();

			// fixed location with Product specified but no pick face - error
			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ValidateLocationOrPalletID(receive.PK.ToGuid(), locations[1].ToLocationString(), data.Part1.PK.ToGuid(), true, false);
			AssertValidLocation(response, locations[1], false, true);
			AssertEquals("This location is a fixed pick face location and 'P1' is not assigned to this location.", response.ErrorMessage);
		}

		public void TestValidateLocationOrPalletID_Location_Fixed_WithPickFace()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FFF", "Test1", false, 1, LocationClasses.Codes.FIX);
			var normalLocationType = Helper.CreateLocationType("NNN", "Test2", false, 0, LocationClasses.Codes.NOR);

			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[0].WLV_WLT_LocationType = normalLocationType.PK;
			locations[1].WLV_WLT_LocationType = fixLocationType.PK; // fixed location

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.Factory.Save();

			// fixed location with product and pick face - no error.
			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, locations[1]);
			pickFace.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ValidateLocationOrPalletID(receive.PK.ToGuid(), locations[1].ToLocationString(), data.Part1.PK.ToGuid(), true, false);
			AssertValidLocation(response, locations[1], false, true);
			AssertEquals("Fixed location with product and pick face should have no errors.", "", response.ErrorMessage);
		}

		#endregion

		#region TestValidateLocationOrPalletID_InvalidParameters

		public void TestValidateLocationOrPalletID_InvalidParameters_NoReceipt()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			var response = webService.ValidateLocationOrPalletID(Guid.Empty, "", Guid.Empty, false, false);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Please provide a Receipt PK.", response.ErrorMessage);
		}

		public void TestValidateLocationOrPalletID_InvalidParameters_UnknownReceive()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			AssertBusinessValidationError(webService, "Receive record could not be found.", "warehouse cannot be null.", webService.ValidateLocationOrPalletID(Guid.NewGuid(), "AAA", Guid.Empty, false, false));
		}

		public void TestValidateLocationOrPalletID_InvalidParameters_NoLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			var response = webService.ValidateLocationOrPalletID(receive.PK.ToGuid(), "", Guid.Empty, false, false);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Please provide a Location.", response.ErrorMessage);
		}

		public void TestValidateLocationOrPalletID_InvalidParameters_NoPalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			var response = webService.ValidateLocationOrPalletID(receive.PK.ToGuid(), "", Guid.Empty, false, true);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Please provide a Pallet ID.", response.ErrorMessage);
		}

		public void TestValidateLocationOrPalletID_InvalidParameters_PalletIDNotLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ValidateLocationOrPalletID(receive.PK.ToGuid(), "A-1", Guid.Empty, false, true); // enter a location as pallet id
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Must enter a valid Pallet ID not a Location.", response.ErrorMessage);
		}

		public void TestValidateLocationOrPalletID_InvalidParameters_InvalidLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ValidateLocationOrPalletID(receive.PK.ToGuid(), "D-1", Guid.Empty, false, false);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Must enter a valid Location.", response.ErrorMessage);
		}

		public void TestValidateLocationOrPalletID_InvalidParameters_NoProduct()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ValidateLocationOrPalletID(receive.PK.ToGuid(), "A-1", Guid.Empty, false, false);
			AssertEquals(ErrorTypes.None, response.Error);
			AssertNull("Product is not mandatory therefore should have no errors.", response.ErrorMessage); //no error
		}

		#endregion

		#region TestValidateLocationOrPalledID_SamePalletIDCannotBeEnteredAfterCreatingPutawayTransfers

		public void TestValidateLocationOrPalledID_SamePalletIDCannotBeEnteredAfterCreatingPutawayTransfers()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var lineInPendingStatus = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			var lineInReceivedStatus = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, dockDoorLocation, "B");
			lineInReceivedStatus.WE_AdjustmentArrivalDate = ZDateTimeOffset.Today;

			var lineInPutawayStatusWithDockDoorLocation = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, dockDoorLocation, "D");
			lineInPutawayStatusWithDockDoorLocation.WE_AdjustmentArrivalDate = ZDateTimeOffset.Today;

			var lineInArrivedStatus = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			lineInArrivedStatus.WE_OriginalInventoryStatus = InventoryStatus.Codes.Arrived;
			lineInArrivedStatus.WE_AdjustmentArrivalDate = ZDateTimeOffset.Today;

			var lineInPutawayStatusWithoutDockDoorLocation = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, nonDockDoorLocation, "E");
			lineInPutawayStatusWithoutDockDoorLocation.WE_AdjustmentArrivalDate = ZDateTimeOffset.Today;
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLineForPalletB = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "B", 1m);
			var transferLineForPalletD = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "D", 1m);
			transfer.RunPreSaveValidation();
			transferLineForPalletD.PickedTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();

			transferLineForPalletD.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLineForPalletD);
			AssertEquals("Precondition", InventoryStatus.Codes.Pending, lineInPendingStatus.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, lineInReceivedStatus.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, lineInPutawayStatusWithDockDoorLocation.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Putaway, transferLineForPalletD.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Arrived, lineInArrivedStatus.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Putaway, lineInPutawayStatusWithoutDockDoorLocation.WE_OriginalInventoryStatus);

			using (WarehouseDataRegistry.Instance.WarnWhenDuplicatePalletIdScanned.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var webService = GetNewWebService();
				webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
				AssertPalletIDErrors(webService, receive.PK, "D", "Pallet ID D exists on another Job.");
				AssertPalletIDErrors(webService, receive.PK, "B", "Pallet ID is assigned to a putaway transfer. Use a different Pallet ID.");
				AssertPalletIDErrors(webService, receive.PK, "E");
			}
		}

		static void AssertPalletIDErrors(WhsSecureService service, ZGuid receivePK, string palletID, string exceptionMessage = "")
		{
			var response = service.ValidateLocationOrPalletID(receivePK.ToGuid(), palletID, Guid.Empty, false, true);
			if (string.IsNullOrEmpty(exceptionMessage))
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertNullOrEmpty(response.ErrorMessage);
			}
			else
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals(exceptionMessage, response.ErrorMessage);
			}
		}

		#endregion

		#region TestValidateLocationOrPalletID_OnlyForPalletID

		#region TestValidateLocationOrPalletID_OnlyForPalletID_EnteringLocation

		public void TestValidateLocationOrPalletID_OnlyForPalletID_EnteringLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "12345");
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response1 = webService1.ValidateLocationOrPalletID(receive.PK.ToGuid(), "A-1", Guid.Empty, false, true);
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);
			AssertEquals("Must enter a valid Pallet ID not a Location.", response1.ErrorMessage);
			AssertNullOrEmpty(response1.PalletID);
			AssertNullOrEmpty(response1.Location);

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response2 = webService2.ValidateLocationOrPalletID(receive.PK.ToGuid(), "A-1NotALocation", Guid.Empty, false, true);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals(ErrorTypes.None, response2.Error);
			AssertNullOrEmpty(response2.ErrorMessage);
			AssertEquals("A-1NotALocation", response2.PalletID);
			AssertNullOrEmpty(response2.Location);
		}

		#endregion

		#region TestValidateLocationOrPalletID_PalletID

		public void TestValidateLocationOrPalletID_PalletID()
		{
			var year = ZDateTime.Today.Year - 2;

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true, "Attr2");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true, "Attr3");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "12345");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PalletID1", new ZDate(year, 04, 02), new ZDate(year, 04, 01), "P1A1", "P1A2", "P1A3", "");
			inventory1.WI_F3_NKPackType = "M3";
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response1 = webService1.ValidateLocationOrPalletID(receive1.PK.ToGuid(), "PalletID0", Guid.Empty, true, true);
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals(response1.PalletID, "PalletID0");
			AssertEquals(response1.InventoriesOnThePallet.InventoryLineInfos.Count, 0);

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response2 = webService2.ValidateLocationOrPalletID(receive1.PK.ToGuid(), "PalletID1", Guid.Empty, true, true);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals(response2.PalletID, "PalletID1");
			AssertNotNull(response2.InventoriesOnThePallet);
			AssertEquals(response2.InventoriesOnThePallet.InventoryLineInfos.Count, 1);
			AssertValidPallet(response2, 0, "P1", new DateTime(year, 4, 2), new DateTime(year, 4, 1), "P1A1", "P1A2", "P1A3", "Attr1", "Attr2", "Attr3", 10m);
		}

		public void TestValidateLocationOrPalletID_PalletID_PalletIDIsOnAnotherJob()
		{
			var year = ZDateTime.Today.Year - 2;

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true, "Attr2");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true, "Attr3");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "12345");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PalletID1", new ZDate(year, 04, 02), new ZDate(year, 04, 01), "P1A1", "P1A2", "P1A3", "");
			inventory1.WI_F3_NKPackType = "M3";
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1234567");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 10m, null, "PalletID2", new ZDate(year, 04, 05), new ZDate(year, 04, 04), "P2A1", "P2A2", "P2A3", "");
			inventory2.WI_F3_NKPackType = "CTN";
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var responseErr = webService1.ValidateLocationOrPalletID(receive2.PK.ToGuid(), "PalletID1", Guid.Empty, true, true);
			AssertEquals("Pallet ID PalletID1 exists on another Job.", responseErr.ErrorMessage);
		}

		public void TestValidateLocationOrPalletID_PalletID_MultipleReceievesAndPallets()
		{
			var year = ZDateTime.Today.Year - 2;

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true, "Attr2");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true, "Attr3");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			var part3 = Helper.CreateProduct(data.Org1, "P3");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "12345");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PalletID1", new ZDate(year, 04, 02), new ZDate(year, 04, 01), "P1A1", "P1A2", "P1A3", "");
			inventory1.WI_F3_NKPackType = "M3";
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1234567");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 10m, null, "PalletID2", new ZDate(year, 04, 05), new ZDate(year, 04, 04), "P2A1", "P2A2", "P2A3", "");
			inventory2.WI_F3_NKPackType = "CTN";
			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response3 = webService1.ValidateLocationOrPalletID(receive2.PK.ToGuid(), "PalletID2", Guid.Empty, true, true);
			AssertSuccessfulResponse(response3, webService1);
			AssertEquals("PalletID2", response3.PalletID);
			AssertNotNull(response3.InventoriesOnThePallet);
			AssertEquals(1, response3.InventoriesOnThePallet.InventoryLineInfos.Count);
			AssertValidPallet(response3, 0, "P2", new DateTime(year, 4, 5), new DateTime(year, 4, 4), "P2A1", "P2A2", "P2A3", "Attr1", "Attr2", "Attr3", 10m);

			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive2, part3, 15m, null, "PalletID2", new ZDate(year + 3, 08, 07), new ZDate(year + 2, 10, 11), "P3A1", "P3A2", "P3A3", "");
			Helper.Factory.Save();

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response4 = webService2.ValidateLocationOrPalletID(receive2.PK.ToGuid(), "PalletID2", Guid.Empty, true, true);
			AssertSuccessfulResponse(response4, webService2);
			AssertEquals("PalletID2", response4.PalletID);
			AssertNotNull(response4.InventoriesOnThePallet);
			AssertEquals(2, response4.InventoriesOnThePallet.InventoryLineInfos.Count);
			AssertValidPallet(response4, 0, "P2", new DateTime(year, 4, 5), new DateTime(year, 4, 4), "P2A1", "P2A2", "P2A3", "Attr1", "Attr2", "Attr3", 10m);
			AssertValidPallet(response4, 1, "P3", new DateTime(year + 3, 08, 07), new DateTime(year + 2, 10, 11), "P3A1", "P3A2", "P3A3", "Attr1", "Attr2", "Attr3", 15m);
		}

		#endregion

		#region TestValidateLocationOrPalletID_PalletID_InTransit

		public void TestValidateLocationOrPalletID_PalletID_InTransit()
		{
			var year = ZDateTime.Today.Year - 2;
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "12345");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1234567");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PalletID1");
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			AssertEquals("Precondition: Stock On Hand.", 10m, inventory1.WI_TotalUnits);

			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Today);
			transferLine.WE_PalletID = "PalletID1";
			AssertEquals("Precondition: No Stock On Hand.", 0m, inventory1.WI_TotalUnits);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var responseErr = webService.ValidateLocationOrPalletID(receive2.PK.ToGuid(), "PalletID1", Guid.Empty, true, true);
			AssertEquals("Pallet ID PalletID1 exists on another Job.", responseErr.ErrorMessage);

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Helper.Factory.Save();

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertNoExceptionThrown(() => webService2.ValidateLocationOrPalletID(receive2.PK.ToGuid(), "PalletID1", Guid.Empty, true, true));
		}

		#endregion

		#region TestValidateLocationOrPalletID_PalletID_IsAvaialbleAfterStockIsReleased

		public void TestValidateLocationOrPalletID_PalletID_IsAvaialbleAfterStockIsReleased()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 1, 2);
			var locationWithFinalisedInventoryForPLT1 = data.Whs1.FindLocation("A-1-1");
			var anotherLocation = data.Whs1.FindLocation("A-1-2");

			// create inventory for PLT1 and release it
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m,
				locationWithFinalisedInventoryForPLT1, "PLT1", false, true);
			Helper.Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Helper.Factory.Save();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			// create a receive for a different location 
			var receiveForAnotherLocation = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ValidateLocationOrPalletID(receiveForAnotherLocation.PK.ToGuid(), "PLT1", Guid.Empty, false, false);
			AssertSuccessfulResponse(response, webService);
		}

		#endregion

		#region TestValidateLocationOrPalletID_PalletID_IsAvaialbleForDifferentWarehouse

		public void TestValidateLocationOrPalletID_PalletID_IsAvaialbleForDifferentWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var warehouse = Helper.CreateWarehouse("Warehouse");

			// create receive for PLT1 in one warehouse
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m, null, "PLT1", false, false);

			// create receive for PLT1 in a different warehouse
			var receiveInDifferentWarehouse = Helper.CreateWhsReceive(data.Org1, warehouse, "R2", Notify);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = warehouse.WW_WarehouseCode;
			var response = webService.ValidateLocationOrPalletID(receiveInDifferentWarehouse.PK.ToGuid(), "PLT1", Guid.Empty, false, true);
			AssertSuccessfulResponse(response, webService);
			AssertEquals(0, response.InventoriesOnThePallet.InventoryLineInfos.Count);
		}

		#endregion

		#region TestValidateLocationOrPalletID_PalletID_AlreadyExists

		public void TestValidateLocationOrPalletID_PalletID_AlreadyExists()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "RCV001");
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response1 = webService.ValidateLocationOrPalletID(receive.PK.ToGuid(), "PalletID1", Guid.Empty, true, true);
			AssertSuccessfulResponse(response1, webService);
			AssertNullOrEmpty(response1.ErrorMessage);

			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PalletID1");
			Helper.Factory.Save();

			using (WarehouseDataRegistry.Instance.WarnWhenDuplicatePalletIdScanned.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var webService2 = GetNewWebService();
				webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
				var response2 = webService2.ValidateLocationOrPalletID(receive.PK.ToGuid(), "PalletID1", Guid.Empty, true, true);
				AssertSuccessfulResponse(response2, webService2);
				AssertEquals(ErrorTypes.PalletAlreadyUnloaded, response2.Error);
				AssertEquals("Pallet ID PalletID1 is already unloaded.", response2.ErrorMessage);
			}

			using (WarehouseDataRegistry.Instance.WarnWhenDuplicatePalletIdScanned.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var webService3 = GetNewWebService();
				webService3.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
				var response3 = webService3.ValidateLocationOrPalletID(receive.PK.ToGuid(), "PalletID1", Guid.Empty, true, true);
				AssertSuccessfulResponse(response3, webService3);
				AssertNullOrEmpty("This should have no error message.", response3.ErrorMessage);
			}
		}

		#endregion

		#region TestValidateLocationOrPalletID_PalletID_AlreadyExistsOnAnotherJob

		public void TestValidateLocationOrPalletID_PalletID_AlreadyExistsOnAnotherJob()
		{
			var year = ZDateTime.Today.Year;
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "RCV001");
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ001");
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, 10m, data.Whs1.DefaultLocation.RowName, "PalletID1", ZDateTimeOffset.Today);
			adjustment.FinaliseDocket();

			AssertIsFinalisedPrecondition(adjustment);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PalletID1");
			Helper.Factory.Save();

			using (WarehouseDataRegistry.Instance.WarnWhenDuplicatePalletIdScanned.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var webService = GetNewWebService();
				webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
				var response1 = webService.ValidateLocationOrPalletID(receive.PK.ToGuid(), "PalletID1", Guid.Empty, true, true);
				AssertSuccessfulResponse(response1, webService);
				AssertEquals(ErrorTypes.PalletAlreadyUnloaded, response1.Error);
				AssertEquals("Pallet ID PalletID1 is already unloaded.", response1.ErrorMessage);
			}

			using (WarehouseDataRegistry.Instance.WarnWhenDuplicatePalletIdScanned.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var webService2 = GetNewWebService();
				webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
				var response2 = webService2.ValidateLocationOrPalletID(receive.PK.ToGuid(), "PalletID1", Guid.Empty, true, true);
				AssertSuccessfulResponse(response2, webService2);
				AssertEquals("Pallet ID PalletID1 exists on another Job.", response2.ErrorMessage);
			}
		}

		#endregion

		#endregion

		#region TestValidateLocationOrPalletID_PalletIDTotalValidation

		public void TestValidateLocationOrPalletID_PalletIDTotalValidation_RegistryEnabled()
		{
			TestValidateLocationOrPalletID_PalletIDTotalValidationCore(true);
		}

		public void TestValidateLocationOrPalletID_PalletIDTotalValidation_RegistryDisabled()
		{
			TestValidateLocationOrPalletID_PalletIDTotalValidationCore(false);
		}

		void TestValidateLocationOrPalletID_PalletIDTotalValidationCore(bool isRegistryControlEnabled)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, data.Whs1.DefaultLocation, "PLT2");
			receive.WD_TotalPallets = 2;
			Helper.Factory.Save();

			receive.PopulateASNLines();
			Helper.Factory.Save();

			AssertEquals("Precondition: ASN lines is not empty.", true, receive.AsnLines.Any());

			using (WarehouseDataRegistry.Instance.TotalPalletsValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isRegistryControlEnabled))
			{
				var webService = GetNewWebService(data.Whs1);
				webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
				var response = webService.ValidateLocationOrPalletID(receive.PK.ToGuid(), "PLT3", data.Part1.PK.ToGuid(), true, true);

				if (isRegistryControlEnabled)
				{
					AssertEquals("Error is returned in the response.", "Total number of pallets unloaded is already equal to or more than the expected number of pallets. You cannot unload more pallets.", response.ErrorMessage);
					AssertEquals("Error type is BusinessValidationError.", ErrorTypes.BusinessValidationError, response.Error);
				}
				else
				{
					AssertNullOrEmpty("No error in the response.", response.ErrorMessage);
				}
			}
		}

		public void TestValidateLocationOrPalletID_PalletIDTotalValidation_NoAsnLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, data.Whs1.DefaultLocation, "PLT2");
			receive.WD_TotalPallets = 2;
			Helper.Factory.Save();

			AssertEquals("Precondition: ASN lines is empty.", false, receive.AsnLines.Any());

			using (WarehouseDataRegistry.Instance.TotalPalletsValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var webService = GetNewWebService(data.Whs1);
				var response = webService.ValidateLocationOrPalletID(receive.PK.ToGuid(), "PLT3", data.Part1.PK.ToGuid(), true, true);

				AssertNullOrEmpty("No error in the response.", response.ErrorMessage);
			}
		}

		public void TestValidateLocationOrPalletID_PalletIDTotalValidation_DuplicatePalletId()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, data.Whs1.DefaultLocation, "PLT2");
			receive.WD_TotalPallets = 2;
			Helper.Factory.Save();

			receive.PopulateASNLines();
			Helper.Factory.Save();

			AssertEquals("Precondition: ASN lines is not empty.", true, receive.AsnLines.Any());

			using (WarehouseDataRegistry.Instance.TotalPalletsValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (WarehouseDataRegistry.Instance.WarnWhenDuplicatePalletIdScanned.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var webService = GetNewWebService(data.Whs1);
				var response = webService.ValidateLocationOrPalletID(receive.PK.ToGuid(), "PLT2", data.Part1.PK.ToGuid(), true, true);

				AssertNullOrEmpty("No total number of pallets error in the response.", response.ErrorMessage);
			}
		}

		#endregion

		#region TestValidateLocationOrPalletID_PalletIDTotalValidation_MaxPalletIdLengthExceeded

		public void TestValidateLocationOrPalletID_PalletIDTotalValidation_MaxPalletIdLengthExceeded()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, data.Whs1.DefaultLocation, "PLT2");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var palletId = "1234567890123456789012345678901";

			Assert("Precondition: Pallet id exceeds maximum length for pallet ids.", palletId.Length > WhsDocketLineSchema.WE_PalletID.MaxLength);
			LocationOrPalletIDWebServiceResponse response = null;
			AssertNoExceptionThrown("No exception is thrown.", () => response = webService.ValidateLocationOrPalletID(receive.PK.ToGuid(), palletId, data.Part1.PK.ToGuid(), true, true));

			AssertEquals("No error reported.", string.Empty, ErrorReporter.LastMessageReported);
			AssertEquals("Error is returned in the response.", "WE_PalletID exceeds maximum length allowed. The maximum length of this property is 30 characters, but 31 were entered.", response.ErrorMessage);
			AssertEquals("Error type is BusinessValidationError.", ErrorTypes.BusinessValidationError, response.Error);
		}

		#endregion

		#region Implementation

		void AssertValidLocation(LocationOrPalletIDWebServiceResponse response, WhsLocation location, bool isVoid, bool isFixed)
		{
			AssertEquals(location.ToLocationString(), response.Location);
			AssertEquals(location.PK, response.LocationPK);
			AssertEquals(isVoid, response.IsVoidLocation);
			AssertEquals(isFixed, response.IsFixed);
			AssertNull("The data we processed is location, therefore Pallet ID should be empty.", response.PalletID);
			AssertNull("Inventories for location should not be uploaded.", response.InventoriesOnThePallet);
		}

		void AssertValidPallet(LocationOrPalletIDWebServiceResponse response, int inventoryIndex, String code, DateTime expiry, DateTime packing, String attr1,
			String attr2, String attr3, String attr1Caption, String attr2Caption, String attr3Caption, decimal packs)
		{
			AssertNotNull(response.InventoriesOnThePallet.ProductInfos[inventoryIndex]);
			AssertEquals(code, response.InventoriesOnThePallet.ProductInfos[inventoryIndex].Code);
			AssertEquals(expiry, response.InventoriesOnThePallet.InventoryLineInfos[inventoryIndex].ExpiryDate);
			AssertEquals(packing, response.InventoriesOnThePallet.InventoryLineInfos[inventoryIndex].PackingDate);
			AssertEquals(attr1, response.InventoriesOnThePallet.InventoryLineInfos[inventoryIndex].Attribute1);
			AssertEquals(attr2, response.InventoriesOnThePallet.InventoryLineInfos[inventoryIndex].Attribute2);
			AssertEquals(attr3, response.InventoriesOnThePallet.InventoryLineInfos[inventoryIndex].Attribute3);
			AssertEquals(attr1Caption, response.InventoriesOnThePallet.ProductPartAttributesInfos[inventoryIndex].Attribute1Caption);
			AssertEquals(attr2Caption, response.InventoriesOnThePallet.ProductPartAttributesInfos[inventoryIndex].Attribute2Caption);
			AssertEquals(attr3Caption, response.InventoriesOnThePallet.ProductPartAttributesInfos[inventoryIndex].Attribute3Caption);
			AssertEquals(packs, response.InventoriesOnThePallet.InventoryLineInfos[inventoryIndex].Packs);
		}

		#endregion

		#endregion
	}
}
