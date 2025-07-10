using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class ValidateBarcodeIsValidEntityTest : WhsSecureServiceTestCase
	{
		#region ValidateBarcodeIsValidEntity

		public void TestValidateBarcodeIsValidEntity_FromProductCode()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 10, 5);
			var location = data.Whs1.DefaultLocation;
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, location, "PLT-1");
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ValidateBarcodeIsValidEntity("P1", data.Org1.OH_Code);
			AssertEquals(response.Error, ErrorTypes.None);
			AssertNull(response.ErrorMessage);
		}

		public void TestValidateBarcodeIsValidEntity_FromLocationString()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 10, 5);
			var location = data.Whs1.DefaultLocation;
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, location, "PLT-1");
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ValidateBarcodeIsValidEntity(location.ToLocationString(), data.Org1.OH_Code);
			AssertEquals(response.Error, ErrorTypes.None);
			AssertNull(response.ErrorMessage);
		}

		public void TestValidateBarcodeIsValidEntity_FromPalletId()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 10, 5);
			var location = data.Whs1.DefaultLocation;
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, location, "PLT-1");
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ValidateBarcodeIsValidEntity("PLT-1", data.Org1.OH_Code);
			AssertEquals(response.Error, ErrorTypes.None);
			AssertNull(response.ErrorMessage);
		}

		public void TestValidateBarcodeIsValidEntity_InvalidBarcode()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 10, 5);
			var location = data.Whs1.DefaultLocation;
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, location, "PLT-1");
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ValidateBarcodeIsValidEntity("RANDOMBARCODE", data.Org1.OH_Code);
			AssertEquals(response.Error, ErrorTypes.BusinessValidationError);
			AssertEquals(response.ErrorMessage, "Barcode is not valid entity.");
		}

		#endregion

		#region TestLoadWhsInventoryValidatingBarcode_Pallets

		public void TestLoadWhsInventoryValidatingBarcode_Pallets_Finalised()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT-1");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ValidateBarcodeIsValidEntity("PLT-1", data.Org1.OH_Code);
			AssertEquals(response.Error, ErrorTypes.None);
			AssertNull(response.ErrorMessage);
		}

		public void TestLoadWhsInventoryValidatingBarcode_Pallets_PuttingAway()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			var otherTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			otherTransfer.WD_IsPutawayTransfer = true;
			var otherTransferLine = Helper.SetupTransferLineForDockDoorLocation(otherTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			otherTransfer.RunPreSaveValidation();
			otherTransferLine.PickedTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();
			AssertEquals("Precondition - ensure inventory is Putting Away.", InventoryStatus.Codes.PuttingAway, otherTransferLine.Inventory[0].WI_InventoryStatus);
			AssertEquals("Precondition - ensure inventory is on transfer.", 50m, otherTransferLine.Inventory[0].WI_TotalUnits);

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ValidateBarcodeIsValidEntity("PLT-1", data.Org1.OH_Code);
			AssertEquals(response.Error, ErrorTypes.None);
			AssertNull(response.ErrorMessage);
		}

		public void TestLoadWhsInventoryValidatingBarcode_Pallets_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);

			var location = data.Whs1.FindLocation("A-1");
			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location, "Pallet-Here", today, today, "A1", "A2", "A3", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 10m, "A-1", "Pallet-Here", ZGuid.Empty, "", "Pallet-Here", new ZDateTimeOffset(today), today, today, "A1", "A2", "A3");
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();
			AssertEquals("Precondition - ensure inventory is Intransit.", InventoryStatus.Codes.InTransit, transferLine.Inventory[0].WI_InventoryStatus);
			AssertEquals("Precondition - ensure inventory is on transfer.", 10m, transferLine.Inventory[0].WI_TotalUnits);
			AssertEquals("Precondition - ensure transfer Pallet is correct.", "Pallet-Here", transferLine.Inventory[0].WI_PalletID);

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ValidateBarcodeIsValidEntity("Pallet-Here", data.Org1.OH_Code);
			AssertEquals(response.Error, ErrorTypes.None);
			AssertNull(response.ErrorMessage);
		}

		public void TestLoadWhsInventoryValidatingBarcode_Pallets_InTransit_TransferFromPalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);

			var location = data.Whs1.FindLocation("A-1");
			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location, "Pallet-Here", today, today, "A1", "A2", "A3", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 10m, "A-1", "Pallet-Here", ZGuid.Empty, "", "", today.ToZDateTime().ToOffset(), today, today, "A1", "A2", "A3");
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();
			AssertEquals("Precondition - ensure transferLine is Intransit.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition - ensure inventory is on transfer.", 10m, transferLine.Inventory[0].WI_TotalUnits);
			AssertEquals("Precondition - ensure transfer Pallet is correct.", "", transferLine.Inventory[0].WI_PalletID);

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ValidateBarcodeIsValidEntity("Pallet-Here", data.Org1.OH_Code);
			AssertEquals(response.Error, ErrorTypes.None);
			AssertNull(response.ErrorMessage);
		}

		public void TestLoadWhsInventoryValidatingBarcode_Pallets_NoLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PLT-1");
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ValidateBarcodeIsValidEntity("PLT-1", data.Org1.OH_Code);
			AssertEquals(response.Error, ErrorTypes.None);
			AssertNull(response.ErrorMessage);
		}

		public void TestLoadWhsInventoryValidatingBarcode_Pallets_EmptyString()
		{
			TestLoadWhsInventoryValidatingBarcode_Pallets_Core(string.Empty);
		}

		public void TestLoadWhsInventoryValidatingBarcode_Pallets_WhitespaceString()
		{
			TestLoadWhsInventoryValidatingBarcode_Pallets_Core("      ");
		}

		void TestLoadWhsInventoryValidatingBarcode_Pallets_Core(string testValue)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.ValidateBarcodeIsValidEntity(testValue, data.Org1.OH_Code);
			AssertEquals(response.Error, ErrorTypes.BusinessValidationError);
			AssertEquals(response.ErrorMessage, "Barcode is not valid entity.");
		}

		#endregion
	}
}
