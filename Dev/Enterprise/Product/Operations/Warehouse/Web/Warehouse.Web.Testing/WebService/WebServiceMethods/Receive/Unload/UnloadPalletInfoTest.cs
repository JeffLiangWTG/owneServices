using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class UnloadPalletInfoTest : WhsSecureServiceTestCase
	{
		#region UnloadPalletInfo

		#region TestUnloadPalletInfo

		public void TestUnloadPalletInfo()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Color");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.VIN, "Vehicle");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.Mandatory, "Model");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);

			var location = data.Whs1.DefaultOutboundDockDoorLocation;
			Helper.Factory.Save();

			var year = ZDateTime.Now.Year - 1;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Rev1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, null, "PLT1", new ZDate(year, 10, 12), new ZDate(year, 10, 13), "Red", "SN0", "M1", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, null, "PLT1", new ZDate(year, 10, 12), new ZDate(year, 10, 13), "Red", "SN1", "M1", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, null, "PLT1", new ZDate(year, 10, 12), new ZDate(year, 10, 13), "Red", "SN2", "M1", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, null, "PLT2", new ZDate(year, 10, 12), new ZDate(year, 10, 13), "Red", "SN3", "M1", "");
			Helper.Factory.Save();

			PopluateASNLines(receive);

			// Unload by Pallet
			var webService = GetNewWebService(data.Whs1);
			var inventoryResponse = webService.UnloadPalletInfo(receive.PK.ToGuid(), location.PK.ToGuid(), "Plt1");
			AssertSuccessfulResponse(inventoryResponse, webService);
			AssertEquals(ErrorTypes.None, inventoryResponse.Error);
			AssertEquals(true, string.IsNullOrEmpty(inventoryResponse.ErrorMessage));

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);

			AssertEquals(4, receiveInNewFactory.Inventory.Count);
			AssertInventoryViewValues(receiveInNewFactory.Inventory[0], "P1", 1m, "UNT", 1m, "UNT",
				"Red", "SN0", "M1", "", new DateTime(year, 10, 12), new DateTime(year, 10, 13), "PLT1", location);
			AssertInventoryViewValues(receiveInNewFactory.Inventory[1], "P1", 1m, "UNT", 1m, "UNT",
				"Red", "SN1", "M1", "", new DateTime(year, 10, 12), new DateTime(year, 10, 13), "PLT1", location);
			AssertInventoryViewValues(receiveInNewFactory.Inventory[2], "P1", 1m, "UNT", 1m, "UNT",
				"Red", "SN2", "M1", "", new DateTime(year, 10, 12), new DateTime(year, 10, 13), "PLT1", location);
		}

		public void TestUnloadPalletInfo_WithSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Color");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.Mandatory, "Size");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.Mandatory, "Model");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var location = data.Whs1.DefaultOutboundDockDoorLocation;
			Helper.Factory.Save();

			var year = ZDateTime.Now.Year - 1;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Rev1", Notify);
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, null, "PLT1", new ZDate(year, 10, 12), new ZDate(year, 10, 13), "RED", "BIG", "M1", "");
			line1.WI_SerialNumber = "Sn1";
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, null, "PLT1", new ZDate(year, 10, 12), new ZDate(year, 10, 13), "Red", "Big", "M1", "");
			line2.WI_SerialNumber = "sN2";
			var line3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, null, "PLT1", new ZDate(year, 10, 12), new ZDate(year, 10, 13), "RED", "Big", "M1", "");
			line3.WI_SerialNumber = "sn3";
			var line4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, null, "PLT2", new ZDate(year, 10, 12), new ZDate(year, 10, 13), "Red", "Big", "M1", "");
			line4.WI_SerialNumber = "SN4";
			Helper.Factory.Save();

			PopluateASNLines(receive);

			// Unload by Pallet
			var webService = GetNewWebService(data.Whs1);
			var inventoryResponse = webService.UnloadPalletInfo(receive.PK.ToGuid(), location.PK.ToGuid(), "PLT1");
			AssertSuccessfulResponse(inventoryResponse, webService);
			AssertEquals(ErrorTypes.None, inventoryResponse.Error);
			AssertEquals(true, string.IsNullOrEmpty(inventoryResponse.ErrorMessage));

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);

			AssertEquals(4, receiveInNewFactory.Inventory.Count);
			AssertInventoryViewValues(receiveInNewFactory.Inventory[0], "P1", 1m, "UNT", 1m, "UNT",
				"RED", "BIG", "M1", "Sn1", new DateTime(year, 10, 12), new DateTime(year, 10, 13), "PLT1", location);
			AssertInventoryViewValues(receiveInNewFactory.Inventory[1], "P1", 1m, "UNT", 1m, "UNT",
				"Red", "Big", "M1", "sN2", new DateTime(year, 10, 12), new DateTime(year, 10, 13), "PLT1", location);
			AssertInventoryViewValues(receiveInNewFactory.Inventory[2], "P1", 1m, "UNT", 1m, "UNT",
				"RED", "Big", "M1", "sn3", new DateTime(year, 10, 12), new DateTime(year, 10, 13), "PLT1", location);
		}

		public void TestUnloadPalletInfo_NoAttributes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var location = data.Whs1.DefaultOutboundDockDoorLocation;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, null, "PLT1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, null, "PLT1");
			Helper.Factory.Save();

			PopluateASNLines(receive);

			var webService = GetNewWebService(data.Whs1);
			var inventoryResponse = webService.UnloadPalletInfo(receive.PK.ToGuid(), Guid.Empty, "pLT1");
			AssertSuccessfulResponse(inventoryResponse, webService);
			AssertEquals(ErrorTypes.None, inventoryResponse.Error);
			AssertEquals(null, inventoryResponse.ErrorMessage);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);

			AssertEquals(2, receiveInNewFactory.Inventory.Count);
			AssertEquals(true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
				.Any(inv => inv.WI_ExpectedReceiptQuantity == 10m
					&& inv.WI_InDocketLineUnits == 10m
					&& inv.WI_WL == ZGuid.Empty
					&& inv.WI_PalletID == "PLT1"
					&& inv.WI_OP == data.Part1.PK));

			AssertEquals(true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
				.Any(inv => inv.WI_ExpectedReceiptQuantity == 20m
					&& inv.WI_InDocketLineUnits == 20m
					&& inv.WI_WL == ZGuid.Empty
					&& inv.WI_PalletID == "PLT1"
					&& inv.WI_OP == data.Part1.PK));
		}

		#endregion

		#region TestUnloadPalletInfo_InventoryWithValidationFailed

		public void TestUnloadPalletInfo_InventoryWithValidationFailed()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var webService = GetNewWebService(data.Whs1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Rev1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, null, "PLT1");
			Helper.Factory.Save();

			PopluateASNLines(receive);

			// Unload by Pallet, failed with CreateWhsReceiveLine validation error
			webService.CreateInvalidReceiveLineDataForTest += i =>
			{
				i.AddRowError("Test Error");
			};
			var inventoryResponse = webService.UnloadPalletInfo(receive.PK.ToGuid(), Guid.Empty, "PLT1");
			AssertSuccessfulResponse(inventoryResponse, webService);
			AssertEquals(ErrorTypes.BusinessValidationError, inventoryResponse.Error);

			AssertEquals(@"Errors occurred when trying to unload full pallet, please correct or use Product Unload instead.

Error - Docket Line: Test Error", inventoryResponse.ErrorMessage);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);
			AssertEquals(1, receiveInNewFactory.Inventory.Count);
		}

		#endregion

		#region TestUnloadPalletInfo_ReceiveWithValidationFailed

		public void TestUnloadPalletInfo_ReceiveWithValidationFailed()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var webService = GetNewWebService(data.Whs1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Rev1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, null, "PLT1");
			Helper.Factory.Save();

			PopluateASNLines(receive);

			// Unload by Pallet, failed with receive validation error
			webService.CreateInvalidReceiveDataForTest += r =>
			{
				r.AddRowError("Test Error");
			};
			var inventoryResponse = webService.UnloadPalletInfo(receive.PK.ToGuid(), Guid.Empty, "PLT1");
			AssertSuccessfulResponse(inventoryResponse, webService);
			AssertEquals(ErrorTypes.BusinessValidationError, inventoryResponse.Error);
			AssertEquals(@"Errors occurred when trying to unload full pallet, please correct or use Product Unload instead.

Error - Warehouse Receipt W00000001: Test Error", inventoryResponse.ErrorMessage);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);
			AssertEquals(1, receiveInNewFactory.Inventory.Count);
		}

		#endregion

		#region TestUnloadPalletInfo_ReceivedQuantityExceedsProductAllowedQuantity

		public void TestUnloadPalletInfo_ReceivedQuantityExceedsProductAllowedQuantity()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "P1");
			var partRelation = Helper.CreateProductClientRelationShip(client, product, OrgPartRelation.RelationshipTypes.Owner);
			partRelation.OU_PreventReceivingOvers = true;
			partRelation.OU_ReceiveOverageTolerancePercent = 3;

			var palletID = "PLT-123";
			var receive = Helper.CreateWhsReceive(client, warehouse);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, product, 5m);
			Helper.CreateWhsReceiveLine(receive, product, 10m, null, palletID);
			Helper.Factory.Save();

			PopluateASNLines(receive);
			receiveLine1.WE_TransactionQuantity = 6m;
			Helper.Factory.Save();

			var webService = GetNewWebService(warehouse);
			var response = webService.UnloadPalletInfo(receive.PK.ToGuid(), Guid.Empty, palletID);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals(@"Errors occurred when trying to unload full pallet, please correct or use Product Unload instead.

Error - ReceivedQuantity: Received quantity of product 'P1' exceeds the allowed quantity.", response.ErrorMessage);
		}

		#endregion

		#region TestUnloadPalletInfo_MultipleErrors

		public void TestUnloadPalletInfo_MultipleErrors()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var webService = GetNewWebService(data.Whs1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Rev1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, null, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, null, "PLT2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, null, "PLT3");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, null, "PLT4");
			Helper.Factory.Save();

			PopluateASNLines(receive);

			// Unload by Pallet, failed with receive validation error
			webService.CreateInvalidReceiveDataForTest += r =>
			{
				r.AddRowError("Receive Error");
				r.AddRowMessageError("Receive Message Error");
				r.AddRowWarning("Receive Warning");

				r.Lines.ForEach(l =>
				{
					l.AddRowError("Receive Line Error For " + l.WE_PalletID);
					l.AddRowMessageError("Receive Line Message Error For " + l.WE_PalletID);
					l.AddRowWarning("Receive Line Warning For " + l.WE_PalletID);
				});
			};

			var inventoryResponse = webService.UnloadPalletInfo(receive.PK.ToGuid(), Guid.Empty, "PLT1");
			AssertSuccessfulResponse(inventoryResponse, webService);
			AssertEquals(ErrorTypes.BusinessValidationError, inventoryResponse.Error);
			AssertEquals(@"Errors occurred when trying to unload full pallet, please correct or use Product Unload instead.

Error - Warehouse Receipt W00000001: Receive Error
Error - Docket Line: Receive Line Error For PLT1
Error - Docket Line: Receive Line Error For PLT2
Error - Docket Line: Receive Line Error For PLT3
Error - Docket Line: Receive Line Error For PLT4
Message Error - Warehouse Receipt W00000001: Receive Message Error
Message Error - Docket Line: Receive Line Message Error For PLT1
Message Error - Docket Line: Receive Line Message Error For PLT2
Message Error - Docket Line: Receive Line Message Error For PLT3
Message Error - Docket Line: Receive Line Message Error For PLT4", inventoryResponse.ErrorMessage);
		}

		#endregion

		#region TestUnloadPalletInfo_DisplaysFirstTenErrors

		public void TestUnloadPalletInfo_DisplaysFirstTenErrors()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var webService = GetNewWebService(data.Whs1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Rev1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, null, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, null, "PLT2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, null, "PLT3");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, null, "PLT4");
			Helper.Factory.Save();

			PopluateASNLines(receive);

			// Unload by Pallet, failed with receive validation error
			webService.CreateInvalidReceiveDataForTest += r =>
			{
				r.AddRowError("Receive Error");

				r.Lines.ForEach(l =>
				{
					l.AddRowError("Receive Line Error 1 For " + l.WE_PalletID);
					l.AddRowError("Receive Line Error 2 For " + l.WE_PalletID);
					l.AddRowError("Receive Line Error 3 For " + l.WE_PalletID);
					l.AddRowError("Receive Line Error 4 For " + l.WE_PalletID);
				});
			};

			var inventoryResponse = webService.UnloadPalletInfo(receive.PK.ToGuid(), Guid.Empty, "PLT1");
			AssertSuccessfulResponse(inventoryResponse, webService);
			AssertEquals(ErrorTypes.BusinessValidationError, inventoryResponse.Error);
			AssertEquals(@"Errors occurred when trying to unload full pallet, please correct or use Product Unload instead.
Displaying the first 10 of 17 errors.

Error - Warehouse Receipt W00000001: Receive Error
Error - Docket Line: Receive Line Error 1 For PLT1
Error - Docket Line: Receive Line Error 2 For PLT1
Error - Docket Line: Receive Line Error 3 For PLT1
Error - Docket Line: Receive Line Error 4 For PLT1
Error - Docket Line: Receive Line Error 1 For PLT2
Error - Docket Line: Receive Line Error 2 For PLT2
Error - Docket Line: Receive Line Error 3 For PLT2
Error - Docket Line: Receive Line Error 4 For PLT2
Error - Docket Line: Receive Line Error 1 For PLT3", inventoryResponse.ErrorMessage);
		}

		#endregion

		#region TestUnloadPalletInfo_HCCIsDefaultedAndMessageIsSentToRF

		public void TestUnloadPalletInfo_HCCOnPallet_OneProduct()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var location = data.Whs1.DefaultLocation;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 15m, null, "PLT1");
			receiveLine1.WE_WHC_NKCurrentInventoryHeldCode = "LCC";
			receiveLine1.WE_WHC_NKOriginalInventoryHeldCode = "LCC";

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, null, "PLT1");
			receiveLine2.WE_WHC_NKCurrentInventoryHeldCode = "LCC";
			receiveLine2.WE_WHC_NKOriginalInventoryHeldCode = "LCC";
			Helper.Factory.Save();

			PopluateASNLines(receive);

			var receiveLineOther = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location, "PLTA");
			receiveLineOther.WE_WHC_NKCurrentInventoryHeldCode = "LCC";
			receiveLineOther.WE_WHC_NKOriginalInventoryHeldCode = "LCC";
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var inventoryResponse = webService.UnloadPalletInfo(receive.PK.ToGuid(), Guid.Empty, "PLT1");
			AssertSuccessfulResponse(inventoryResponse, webService);
			AssertEquals(ErrorTypes.Information, inventoryResponse.Error);
			AssertEquals("This pallet contains the following Hold Code(s): 'LCC'.", inventoryResponse.ErrorMessage);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);

			AssertEquals(3, receiveInNewFactory.Inventory.Count);
			AssertEquals(true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
				.Any(inv => inv.WI_ExpectedReceiptQuantity == 10m
					&& inv.WI_InDocketLineUnits == 10m
					&& inv.WI_WL == location.PK
					&& inv.WI_PalletID == "PLTA"
					&& inv.WI_HeldCode == "LCC"
					&& inv.WI_OP == data.Part1.PK));

			AssertEquals(true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
				.Any(inv => inv.WI_ExpectedReceiptQuantity == 15m
					&& inv.WI_InDocketLineUnits == 15m
					&& inv.WI_WL == ZGuid.Empty
					&& inv.WI_PalletID == "PLT1"
					&& inv.WI_HeldCode == "LCC"
					&& inv.WI_OP == data.Part1.PK));

			AssertEquals(true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
				.Any(inv => inv.WI_ExpectedReceiptQuantity == 20m
					&& inv.WI_InDocketLineUnits == 20m
					&& inv.WI_WL == ZGuid.Empty
					&& inv.WI_PalletID == "PLT1"
					&& inv.WI_HeldCode == "LCC"
					&& inv.WI_OP == data.Part1.PK));
		}

		public void TestUnloadPalletInfo_HCCOnPallet_TwoProductsOneHCC()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var ddlLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var location = data.Whs1.DefaultLocation;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 55m, null, "PLT1");
			receiveLine1.WE_WHC_NKCurrentInventoryHeldCode = "LCC";
			receiveLine1.WE_WHC_NKOriginalInventoryHeldCode = "LCC";

			Helper.CreateWhsReceiveLine(receive, data.Part2, 20m, null, "PLT1");
			Helper.Factory.Save();

			PopluateASNLines(receive);

			//Create other line affect populate of it still has inventory.
			var receiveLineOther = Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, location, "PLTA");
			receiveLineOther.WE_WHC_NKCurrentInventoryHeldCode = "LCC";
			receiveLineOther.WE_WHC_NKOriginalInventoryHeldCode = "LCC";
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var inventoryResponse = webService.UnloadPalletInfo(receive.PK.ToGuid(), Guid.Empty, "PLT1");
			AssertSuccessfulResponse(inventoryResponse, webService);
			AssertEquals(ErrorTypes.Information, inventoryResponse.Error);
			AssertEquals("This pallet contains the following Hold Code(s): 'LCC'.", inventoryResponse.ErrorMessage);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);

			AssertEquals(3, receiveInNewFactory.Inventory.Count);
			AssertEquals(true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
				.Any(inv => inv.WI_ExpectedReceiptQuantity == 100m
					&& inv.WI_InDocketLineUnits == 100m
					&& inv.WI_WL == location.PK
					&& inv.WI_PalletID == "PLTA"
					&& inv.WI_HeldCode == "LCC"
					&& inv.WI_OP == data.Part1.PK));

			AssertEquals(true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
				.Any(inv => inv.WI_ExpectedReceiptQuantity == 55m
					&& inv.WI_InDocketLineUnits == 55m
					&& inv.WI_WL == ZGuid.Empty
					&& inv.WI_PalletID == "PLT1"
					&& inv.WI_HeldCode == "LCC"
					&& inv.WI_OP == data.Part1.PK));

			AssertEquals(true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
				.Any(inv => inv.WI_ExpectedReceiptQuantity == 20m
					&& inv.WI_InDocketLineUnits == 20m
					&& inv.WI_WL == ZGuid.Empty
					&& inv.WI_PalletID == "PLT1"
					&& inv.WI_HeldCode == string.Empty
					&& inv.WI_OP == data.Part2.PK));
		}

		public void TestUnloadPalletInfo_HCCOnPallet_TwoProductsTwoHCCs()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var ddlLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var location = data.Whs1.DefaultLocation;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 55m, null, "PLT1");
			receiveLine1.WE_WHC_NKCurrentInventoryHeldCode = "LCC";
			receiveLine1.WE_WHC_NKOriginalInventoryHeldCode = "LCC";

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 20m, null, "PLT1");
			receiveLine2.WE_WHC_NKCurrentInventoryHeldCode = "HEL";
			receiveLine2.WE_WHC_NKOriginalInventoryHeldCode = "HEL";
			Helper.Factory.Save();

			PopluateASNLines(receive);

			//Create other lines affect populate of it still has inventory.
			var receiveLineOtherA = Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, location, "PLTA");
			receiveLineOtherA.WE_WHC_NKCurrentInventoryHeldCode = "LCC";
			receiveLineOtherA.WE_WHC_NKOriginalInventoryHeldCode = "LCC";

			var receiveLineOtherB = Helper.CreateWhsReceiveLine(receive, data.Part2, 75m, location, "PLTA");
			receiveLineOtherB.WE_WHC_NKCurrentInventoryHeldCode = "HEL";
			receiveLineOtherB.WE_WHC_NKOriginalInventoryHeldCode = "HEL";
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var inventoryResponse = webService.UnloadPalletInfo(receive.PK.ToGuid(), Guid.Empty, "PLT1");
			AssertSuccessfulResponse(inventoryResponse, webService);
			AssertEquals(ErrorTypes.Information, inventoryResponse.Error);
			AssertEquals("This pallet contains the following Hold Code(s): 'LCC, HEL'.", inventoryResponse.ErrorMessage);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);

			AssertEquals(4, receiveInNewFactory.Inventory.Count);
			AssertEquals(true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
				.Any(inv => inv.WI_ExpectedReceiptQuantity == 100m
					&& inv.WI_InDocketLineUnits == 100m
					&& inv.WI_WL == location.PK
					&& inv.WI_PalletID == "PLTA"
					&& inv.WI_HeldCode == "LCC"
					&& inv.WI_OP == data.Part1.PK));

			AssertEquals(true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
				.Any(inv => inv.WI_ExpectedReceiptQuantity == 75m
					&& inv.WI_InDocketLineUnits == 75m
					&& inv.WI_WL == location.PK
					&& inv.WI_PalletID == "PLTA"
					&& inv.WI_HeldCode == "HEL"
					&& inv.WI_OP == data.Part2.PK));

			AssertEquals(true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
				.Any(inv => inv.WI_ExpectedReceiptQuantity == 55m
					&& inv.WI_InDocketLineUnits == 55m
					&& inv.WI_WL == ZGuid.Empty
					&& inv.WI_PalletID == "PLT1"
					&& inv.WI_HeldCode == "LCC"
					&& inv.WI_OP == data.Part1.PK));

			AssertEquals(true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
				.Any(inv => inv.WI_ExpectedReceiptQuantity == 20m
					&& inv.WI_InDocketLineUnits == 20m
					&& inv.WI_WL == ZGuid.Empty
					&& inv.WI_PalletID == "PLT1"
					&& inv.WI_HeldCode == "HEL"
					&& inv.WI_OP == data.Part2.PK));
		}

		#endregion

		#region TestUnloadPalletInfo_MatchingHCC

		public void TestUnloadPalletInfo_MatchingHCC_ASNLine_MatchesHCC()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, null, "PLT1");
			receiveLine1.WE_WHC_NKCurrentInventoryHeldCode = "LCC";
			receiveLine1.WE_WHC_NKOriginalInventoryHeldCode = "LCC";

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, null, "PLT2");
			Helper.Factory.Save();

			PopluateASNLines(receive);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var inventoryResponse = webService.UnloadPalletInfo(receive.PK.ToGuid(), Guid.Empty, "PLT1");
			AssertSuccessfulResponse(inventoryResponse, webService);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);

			CombineAssertions(() =>
			{
				AssertEquals(2, receiveInNewFactory.Inventory.Count);
				AssertEquals("LCC PLT1 Line", true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
					.Any(inv => inv.WI_ExpectedReceiptQuantity == 1m
						&& inv.WI_InDocketLineUnits == 1m
						&& inv.WI_WL == ZGuid.Empty
						&& inv.WI_PalletID == "PLT1"
						&& inv.WI_HeldCode == "LCC"
						&& inv.WI_OP == data.Part1.PK));

				AssertEquals("PLT2 Line", true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
					.Any(inv => inv.WI_ExpectedReceiptQuantity == 1m
						&& inv.WI_InDocketLineUnits == 0m
						&& inv.WI_WL == ZGuid.Empty
						&& inv.WI_PalletID == "PLT2"
						&& inv.WI_HeldCode == ""
						&& inv.WI_OP == data.Part1.PK));
			});
		}

		public void TestUnloadPalletInfo_MatchingHCC_ASNLine_MatchesPLTOverHCC()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, null, "PLT1");
			receiveLine1.WE_WHC_NKCurrentInventoryHeldCode = "LCC";
			receiveLine1.WE_WHC_NKOriginalInventoryHeldCode = "LCC";

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, null, "PLT2");
			Helper.Factory.Save();

			PopluateASNLines(receive);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var inventoryResponse = webService.UnloadPalletInfo(receive.PK.ToGuid(), Guid.Empty, "PLT2");
			AssertSuccessfulResponse(inventoryResponse, webService);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);

			CombineAssertions(() =>
			{
				AssertEquals(2, receiveInNewFactory.Inventory.Count);
				AssertEquals("LCC PLT1 Line", true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
					.Any(inv => inv.WI_ExpectedReceiptQuantity == 1m
						&& inv.WI_InDocketLineUnits == 0m
						&& inv.WI_WL == ZGuid.Empty
						&& inv.WI_PalletID == "PLT1"
						&& inv.WI_HeldCode == "LCC"
						&& inv.WI_OP == data.Part1.PK));

				AssertEquals("PLT2 Line", true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
					.Any(inv => inv.WI_ExpectedReceiptQuantity == 1m
						&& inv.WI_InDocketLineUnits == 1m
						&& inv.WI_WL == ZGuid.Empty
						&& inv.WI_PalletID == "PLT2"
						&& inv.WI_HeldCode == ""
						&& inv.WI_OP == data.Part1.PK));
			});
		}

		public void TestUnloadPalletInfo_ASNLine_MatchesAllPLTLines_OneHCC()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, null, "PLT1");
			receiveLine1.WE_WHC_NKCurrentInventoryHeldCode = "LCC";
			receiveLine1.WE_WHC_NKOriginalInventoryHeldCode = "LCC";

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, null, "PLT1");
			Helper.Factory.Save();

			PopluateASNLines(receive);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var inventoryResponse = webService.UnloadPalletInfo(receive.PK.ToGuid(), Guid.Empty, "PLT1");
			AssertSuccessfulResponse(inventoryResponse, webService);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);

			CombineAssertions(() =>
			{
				AssertEquals(2, receiveInNewFactory.Inventory.Count);
				AssertEquals("LCC PLT1 Line", true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
					.Any(inv => inv.WI_ExpectedReceiptQuantity == 1m
						&& inv.WI_InDocketLineUnits == 1m
						&& inv.WI_WL == ZGuid.Empty
						&& inv.WI_PalletID == "PLT1"
						&& inv.WI_HeldCode == "LCC"
						&& inv.WI_OP == data.Part1.PK));

				AssertEquals("No HCC PLT1 Line", true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
					.Any(inv => inv.WI_ExpectedReceiptQuantity == 1m
						&& inv.WI_InDocketLineUnits == 1m
						&& inv.WI_WL == ZGuid.Empty
						&& inv.WI_PalletID == "PLT1"
						&& inv.WI_HeldCode == ""
						&& inv.WI_OP == data.Part1.PK));
			});
		}

		public void TestUnloadPalletInfo_ASNLine_MatchesAllPLTLines_TwoHCCsOneProduct()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, null, "PLT1");
			receiveLine1.WE_WHC_NKCurrentInventoryHeldCode = "LCC";
			receiveLine1.WE_WHC_NKOriginalInventoryHeldCode = "LCC";
			receiveLine1.WE_LineNo = 1;

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, null, "PLT1");
			receiveLine2.WE_WHC_NKCurrentInventoryHeldCode = "HEL";
			receiveLine2.WE_WHC_NKOriginalInventoryHeldCode = "HEL";
			receiveLine2.WE_LineNo = 1;
			Helper.Factory.Save();

			PopluateASNLines(receive);
			Helper.Factory.Save();
			AssertEquals("Precondition: 1 ASNLines", 1, receive.AsnLines.Count);

			var webService = GetNewWebService(data.Whs1);
			var inventoryResponse = webService.UnloadPalletInfo(receive.PK.ToGuid(), Guid.Empty, "PLT1");
			AssertSuccessfulResponse(inventoryResponse, webService);
			AssertEquals(ErrorTypes.Information, inventoryResponse.Error);
			AssertEquals("This pallet contains the following Hold Code(s): 'LCC, HEL'.", inventoryResponse.ErrorMessage);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);

			CombineAssertions(() =>
			{
				AssertEquals(2, receiveInNewFactory.Inventory.Count);
				AssertEquals("LCC PLT1 Line", true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
					.Any(inv => inv.WI_ExpectedReceiptQuantity == 1m
						&& inv.WI_WL == ZGuid.Empty
						&& inv.WI_PalletID == "PLT1"
						&& inv.WI_HeldCode == "LCC"
						&& inv.WI_OP == data.Part1.PK));

				AssertEquals("HEL PLT1 Line", true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
					.Any(inv => inv.WI_ExpectedReceiptQuantity == 1m
						&& inv.WI_WL == ZGuid.Empty
						&& inv.WI_PalletID == "PLT1"
						&& inv.WI_HeldCode == "HEL"
						&& inv.WI_OP == data.Part1.PK));
			});
		}

		public void TestUnloadPalletInfo_ASNLine_MatchesAllPLTLines_TwoHCCsTwoProducts()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, null, "PLT1");
			receiveLine1.WE_WHC_NKCurrentInventoryHeldCode = "LCC";
			receiveLine1.WE_WHC_NKOriginalInventoryHeldCode = "LCC";

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, null, "PLT1");
			receiveLine2.WE_WHC_NKCurrentInventoryHeldCode = "HEL";
			receiveLine2.WE_WHC_NKOriginalInventoryHeldCode = "HEL";
			Helper.Factory.Save();

			PopluateASNLines(receive);
			Helper.Factory.Save();
			AssertEquals("Precondition: 2 ASNLines", 2, receive.AsnLines.Count);

			var webService = GetNewWebService(data.Whs1);
			var inventoryResponse = webService.UnloadPalletInfo(receive.PK.ToGuid(), Guid.Empty, "PLT1");
			AssertSuccessfulResponse(inventoryResponse, webService);
			AssertEquals(ErrorTypes.Information, inventoryResponse.Error);
			AssertEquals("This pallet contains the following Hold Code(s): 'LCC, HEL'.", inventoryResponse.ErrorMessage);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);

			CombineAssertions(() =>
			{
				AssertEquals(2, receiveInNewFactory.Inventory.Count);
				AssertEquals("LCC PLT1 Line", true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
					.Any(inv => inv.WI_ExpectedReceiptQuantity == 1m
						&& inv.WI_WL == ZGuid.Empty
						&& inv.WI_PalletID == "PLT1"
						&& inv.WI_HeldCode == "LCC"
						&& inv.WI_OP == data.Part1.PK));

				AssertEquals("HEL PLT1 Line", true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
					.Any(inv => inv.WI_ExpectedReceiptQuantity == 1m
						&& inv.WI_WL == ZGuid.Empty
						&& inv.WI_PalletID == "PLT1"
						&& inv.WI_HeldCode == "HEL"
						&& inv.WI_OP == data.Part2.PK));
			});
		}

		public void TestUnloadPalletInfo_ASNLine_DuplicateLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, null, "PLT1");
			receiveLine1.WE_WHC_NKCurrentInventoryHeldCode = "LCC";
			receiveLine1.WE_WHC_NKOriginalInventoryHeldCode = "LCC";
			receiveLine1.WE_LineNo = 1;

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, null, "PLT1");
			receiveLine2.WE_WHC_NKCurrentInventoryHeldCode = "LCC";
			receiveLine2.WE_WHC_NKOriginalInventoryHeldCode = "LCC";
			receiveLine2.WE_LineNo = 1;
			Helper.Factory.Save();

			PopluateASNLines(receive);
			Helper.Factory.Save();
			AssertEquals("Precondition: 1 ASNLines", 1, receive.AsnLines.Count);
			AssertEquals("Precondition: ASNLine Quantity is 2", 2m, receive.AsnLines[0].WN_Quantity);

			var webService = GetNewWebService(data.Whs1);
			var inventoryResponse = webService.UnloadPalletInfo(receive.PK.ToGuid(), Guid.Empty, "PLT1");
			AssertSuccessfulResponse(inventoryResponse, webService);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);

			CombineAssertions(() =>
			{
				AssertEquals(2, receiveInNewFactory.Inventory.Count);
				AssertEquals("LCC PLT1 Line #1", true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
					.Any(inv => inv.WI_ExpectedReceiptQuantity == 1m
						&& inv.WI_WL == ZGuid.Empty
						&& inv.WI_PalletID == "PLT1"
						&& inv.WI_HeldCode == "LCC"
						&& inv.WI_OP == data.Part1.PK));

				AssertEquals("LCC PLT1 Line #2", true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
					.Any(inv => inv.WI_ExpectedReceiptQuantity == 1m
						&& inv.WI_WL == ZGuid.Empty
						&& inv.WI_PalletID == "PLT1"
						&& inv.WI_HeldCode == "LCC"
						&& inv.WI_OP == data.Part1.PK));
			});
		}

		public void TestUnloadPalletInfo_ASNLine_RecLinesWithSameAttributesDifferentQty()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 4m, null, "PLT1");
			receiveLine1.WE_WHC_NKCurrentInventoryHeldCode = "LCC";
			receiveLine1.WE_WHC_NKOriginalInventoryHeldCode = "LCC";
			receiveLine1.WE_LineNo = 1;

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 11m, null, "PLT1");
			receiveLine2.WE_WHC_NKCurrentInventoryHeldCode = "HEL";
			receiveLine2.WE_WHC_NKOriginalInventoryHeldCode = "HEL";
			receiveLine2.WE_LineNo = 1;

			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m, null, "PLT1");
			receiveLine3.WE_WHC_NKCurrentInventoryHeldCode = "LCC";
			receiveLine3.WE_WHC_NKOriginalInventoryHeldCode = "LCC";
			receiveLine3.WE_LineNo = 1;
			Helper.Factory.Save();

			PopluateASNLines(receive);
			Helper.Factory.Save();
			AssertEquals("Precondition: 1 ASNLines", 1, receive.AsnLines.Count);
			AssertEquals("Precondition: ASNLine Quantity is 17", 17m, receive.AsnLines[0].WN_Quantity);

			var webService = GetNewWebService(data.Whs1);
			var inventoryResponse = webService.UnloadPalletInfo(receive.PK.ToGuid(), Guid.Empty, "PLT1");
			AssertSuccessfulResponse(inventoryResponse, webService);
			AssertSuccessfulResponse(inventoryResponse, webService);
			AssertEquals(ErrorTypes.Information, inventoryResponse.Error);
			AssertEquals("This pallet contains the following Hold Code(s): 'LCC, HEL'.", inventoryResponse.ErrorMessage);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Correct count of inventories created.", 3, receiveInNewFactory.Inventory.Count);
				AssertEquals("LCC PLT1 Line #1", true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
					.Any(inv => inv.WI_ExpectedReceiptQuantity == 4m
						&& inv.WI_WL == ZGuid.Empty
						&& inv.WI_PalletID == "PLT1"
						&& inv.WI_HeldCode == "LCC"
						&& inv.WI_OP == data.Part1.PK));

				AssertEquals("HEL PLT1 Line #1", true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
					.Any(inv => inv.WI_ExpectedReceiptQuantity == 11m
						&& inv.WI_WL == ZGuid.Empty
						&& inv.WI_PalletID == "PLT1"
						&& inv.WI_HeldCode == "HEL"
						&& inv.WI_OP == data.Part1.PK));

				AssertEquals("LCC PLT1 Line #2", true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
					.Any(inv => inv.WI_ExpectedReceiptQuantity == 2m
						&& inv.WI_WL == ZGuid.Empty
						&& inv.WI_PalletID == "PLT1"
						&& inv.WI_HeldCode == "LCC"
						&& inv.WI_OP == data.Part1.PK));
			});
		}

		public void TestUnloadPalletInfo_ASNLine_ReceiveLineDeletedAfterASNLineCreation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, null, "PLT1");
			receiveLine1.WE_WHC_NKCurrentInventoryHeldCode = "LCC";
			receiveLine1.WE_WHC_NKOriginalInventoryHeldCode = "LCC";

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, null, "PLT1");
			receiveLine2.WE_WHC_NKCurrentInventoryHeldCode = "HEL";
			receiveLine2.WE_WHC_NKOriginalInventoryHeldCode = "HEL";
			Helper.Factory.Save();

			PopluateASNLines(receive);
			Helper.Factory.Save();
			AssertEquals("Precondition: 2 ASNLines before receiveLine deletion.", 2, receive.AsnLines.Count);

			receiveLine2.Delete();
			Helper.Factory.Save();
			AssertEquals("Precondition: 1 Receive Lines after receiveLine deletion.", 1, receive.Lines.Count);
			AssertEquals("Precondition: 2 ASNLines after receiveLine deletion.", 2, receive.AsnLines.Count);

			var webService = GetNewWebService(data.Whs1);
			var inventoryResponse = webService.UnloadPalletInfo(receive.PK.ToGuid(), Guid.Empty, "PLT1");
			AssertSuccessfulResponse(inventoryResponse, webService);
			AssertEquals(ErrorTypes.Information, inventoryResponse.Error);
			AssertEquals("This pallet contains the following Hold Code(s): 'LCC'.", inventoryResponse.ErrorMessage);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);

			CombineAssertions(() =>
			{
				AssertEquals(2, receiveInNewFactory.Inventory.Count);
				AssertEquals("LCC PLT1 Line", true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
					.Any(inv => inv.WI_ExpectedReceiptQuantity == 1m
						&& inv.WI_TotalUnits == 1m
						&& inv.WI_WL == ZGuid.Empty
						&& inv.WI_PalletID == "PLT1"
						&& inv.WI_HeldCode == "LCC"
						&& inv.WI_OP == data.Part1.PK));

				AssertEquals("No HCC PLT1 Line", true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
					.Any(inv => inv.WI_ExpectedReceiptQuantity == 0m
						&& inv.WI_TotalUnits == 1m
						&& inv.WI_WL == ZGuid.Empty
						&& inv.WI_PalletID == "PLT1"
						&& inv.WI_HeldCode == ""
						&& inv.WI_OP == data.Part2.PK));
			});
		}

		public void TestUnloadPalletInfo_ASNLine_ReceiveLineDeletedAfterASNLineCreation_WithSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, null, "PLT1");
			receiveLine1.WE_SerialNumber = "SER1";
			receiveLine1.WE_WHC_NKCurrentInventoryHeldCode = "LCC";
			receiveLine1.WE_WHC_NKOriginalInventoryHeldCode = "LCC";

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, null, "PLT1");
			receiveLine2.WE_SerialNumber = "SER2";
			receiveLine2.WE_WHC_NKCurrentInventoryHeldCode = "HEL";
			receiveLine2.WE_WHC_NKOriginalInventoryHeldCode = "HEL";
			Helper.Factory.Save();

			PopluateASNLines(receive);
			Helper.Factory.Save();
			AssertEquals("Precondition: 2 ASNLines before receiveLine deletion.", 2, receive.AsnLines.Count);

			receiveLine2.Delete();
			Helper.Factory.Save();
			AssertEquals("Precondition: 1 Receive Lines after receiveLine deletion.", 1, receive.Lines.Count);
			AssertEquals("Precondition: 2 ASNLines after receiveLine deletion.", 2, receive.AsnLines.Count);

			var webService = GetNewWebService(data.Whs1);
			var inventoryResponse = webService.UnloadPalletInfo(receive.PK.ToGuid(), Guid.Empty, "PLT1");
			AssertSuccessfulResponse(inventoryResponse, webService);
			AssertEquals(ErrorTypes.Information, inventoryResponse.Error);
			AssertEquals("This pallet contains the following Hold Code(s): 'LCC'.", inventoryResponse.ErrorMessage);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);

			CombineAssertions(() =>
			{
				AssertEquals(2, receiveInNewFactory.Inventory.Count);
				AssertEquals("LCC PLT1 Line", true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
					.Any(inv => inv.WI_ExpectedReceiptQuantity == 1m
						&& inv.WI_TotalUnits == 1m
						&& inv.WI_WL == ZGuid.Empty
						&& inv.WI_PalletID == "PLT1"
						&& inv.WI_SerialNumber == "SER1"
						&& inv.WI_HeldCode == "LCC"
						&& inv.WI_OP == data.Part1.PK));

				AssertEquals("No HCC PLT1 Line", true, receiveInNewFactory.Inventory.Cast<WhsInventoryView>()
					.Any(inv => inv.WI_ExpectedReceiptQuantity == 0m
						&& inv.WI_TotalUnits == 1m
						&& inv.WI_WL == ZGuid.Empty
						&& inv.WI_PalletID == "PLT1"
						&& inv.WI_SerialNumber == "SER2"
						&& inv.WI_HeldCode == ""
						&& inv.WI_OP == data.Part2.PK));
			});
		}

		#endregion

		public void TestUnloadPalletInfo_PalletWithUnloadedLines()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			var year = ZDateTime.Now.Year - 1;
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "Rev1", Notify);
			var receiveLine1 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT1", new ZDate(year, 10, 12), new ZDate(year, 10, 13), "", "", "", "");
			var receiveLine2 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT1", new ZDate(year, 10, 12), new ZDate(year, 10, 13), "", "", "", "");
			webService.Factory.Save();

			// create ASN lines for receive.
			receive.PopulateASNLines();
			webService.Factory.Save();

			AssertEquals("Precondition:", 2, receive.AsnLines.Count);
			AssertEquals("Precondition:", 2, receive.Inventory.Count);

			var response = webService.UnloadPalletInfo(receive.PK.ToGuid(), data.Whs1.DefaultLocation.PK.ToGuid(), "PLT1");
			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Pallet Id 'PLT1' was already unloaded.", response.ErrorMessage);
		}

		public void TestUnloadPalletInfo_ConcurrencySaveExceptionError()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, null, "PLT1");
			Helper.Factory.Save();

			PopluateASNLines(receive);

			var webService = GetNewWebService(data.Whs1);
			var innerException = new Exception();
			var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)receive).Row, Db.Connection);
			webService.Factory.Saving += f => throw new ZSaveConcurrencyException(concurrencyException, webService.Factory);

			WhsInventoryWebServiceResponse response = null;
			AssertNoExceptionThrown(() => response = webService.UnloadPalletInfo(receive.PK.ToGuid(), Guid.Empty, "PLT1"));
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Another user has made changes to the unloading job while you have been working on it. Please restart the operation and try again.", response.ErrorMessage);
		}

		public void TestUnloadPalletInfo_CannotSaveExceptionError()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, null, "PLT1");
			Helper.Factory.Save();

			PopluateASNLines(receive);

			var webService = GetNewWebService(data.Whs1);
			webService.Factory.Saving += f => throw new ZCannotSaveException("Cannot Save Error Message", "Test");

			WhsInventoryWebServiceResponse response = null;
			AssertNoExceptionThrown(() => response = webService.UnloadPalletInfo(receive.PK.ToGuid(), Guid.Empty, "PLT1"));
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Cannot Save Error Message", response.ErrorMessage);
		}

		#region Implementation

		void AssertInventoryViewValues(WhsInventoryView line, ZString expectedProduct, ZDecimal expectedPacks, ZString expectedPacksUQ,
			ZDecimal expectedUnits, ZString expectedUnitsUQ,
			ZString expectedAttribute1, ZString expectedAttribute2, ZString expectedAttribute3, ZString expectedSerialNumber,
			ZDateTime expectedExpiryDate, ZDateTime expectedPackingDate,
			ZString expectedPalletID, WhsLocation expectedLocation)
		{
			CombineAssertions(() =>
			{
				AssertNotNull("SupplierPart", line.SupplierPart);
				AssertEquals("Product", expectedProduct, line.SupplierPart.OP_PartNum);
				AssertEquals("WE_PackQuantity", expectedPacks, line.InDocketLine.WE_PackQuantity);
				AssertEquals("WI_F3_NKPackType", expectedPacksUQ, line.WI_F3_NKPackType);
				AssertEquals("WI_InDocketLineUnits", expectedUnits, line.WI_InDocketLineUnits);
				AssertEquals("WI_ExpectedReceiptQuantity", expectedUnits, line.WI_ExpectedReceiptQuantity);
				AssertEquals("WI_UnitsUQ", expectedUnitsUQ, line.WI_UnitsUQ);
				AssertEquals("WI_PartAttrib1", expectedAttribute1, line.WI_PartAttrib1);
				AssertEquals("WI_PartAttrib2", expectedAttribute2, line.WI_PartAttrib2);
				AssertEquals("WI_PartAttrib3", expectedAttribute3, line.WI_PartAttrib3);
				AssertEquals("WI_SerialNumber", expectedSerialNumber, line.WI_SerialNumber);
				AssertEquals("WI_ExpiryDate", expectedExpiryDate, line.WI_ExpiryDate);
				AssertEquals("WI_PackingDate", expectedPackingDate, line.WI_PackingDate);
				AssertEquals("OriginalInventoryHeldCode", string.Empty, line.OriginalInventoryHeldCode);
				AssertEquals("WI_PalletID", expectedPalletID, line.WI_PalletID);
				AssertEquals("WE_WL", expectedLocation.PK, line.InDocketLine.WE_WL);
				AssertEquals("Location", expectedLocation.ToLocationString(), line.LocationString);
				AssertEquals("Logs", 0, Helper.FindLogs(line.InDocketLine.Logs, AutoEvents.AddedARecordToTheSystem, "RF").Length);
			});
		}

		#endregion

		#endregion
	}
}
