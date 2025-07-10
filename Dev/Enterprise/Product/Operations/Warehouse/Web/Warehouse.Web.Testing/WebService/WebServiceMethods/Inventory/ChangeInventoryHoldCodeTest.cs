using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class ChangeInventoryHoldCodeTest : WhsSecureServiceTestCase
	{
		#region TestChangeInventoryHoldCode

		public void TestChangeInventoryHoldCode_Whole()
		{
			var whs = Helper.CreateWarehouse("Warehouse1", "A", 2, 2);
			var client = Helper.CreateClient("TEST123", "Test Client");
			var product = Helper.CreateProduct(client, "PROD1");
			var docketLine = (WhsDocketLine)Helper.CreateStock(whs.PK, client.PK, product.PK, 10m);
			Helper.Factory.Save();

			var inventory = (WhsInventoryView)docketLine.Inventory.FirstOrDefault();
			AssertNotNull(inventory);

			var webService = GetNewWebService(whs);
			var holdCode = "HEL";
			var holdChangeReason = "Hold for QA check";
			var qty = 10m;
			var response = webService.ChangeInventoryHoldCode(inventory.PK.ToGuid(), holdCode, holdChangeReason, qty);

			AssertEquals("Successful response, no error message returned.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("Successful response, no error returned.", ErrorTypes.None, response.Error);

			var updatedInventory = webService.Factory.Load<WhsInventoryView>(inventory.PK);
			AssertNotNull(updatedInventory);
			AssertEquals("Original hold code is empty", true, string.IsNullOrEmpty(updatedInventory.OriginalInventoryHeldCode));
			AssertEquals("Current hold code is HEL", "HEL", updatedInventory.WI_HeldCode);
			AssertEquals("Qty is 10m", 10m, updatedInventory.WI_TotalUnits);

			var updatedDocketLine = updatedInventory.InDocketLine;
			AssertNotNull(updatedDocketLine);
			AssertEquals("Original hold code is empty", true, string.IsNullOrEmpty(updatedDocketLine.WE_WHC_NKOriginalInventoryHeldCode));
			AssertEquals("Current hold code is HEL", "HEL", updatedDocketLine.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Current hold reason is Hold for QA check", "Hold for QA check", updatedDocketLine.WE_CurrentHoldReason);
			AssertEquals("Qty is 10m", 10m, updatedDocketLine.WE_StockOnHand);
		}

		public void TestChangeInventoryHoldCode_Partial()
		{
			var whs = Helper.CreateWarehouse("Warehouse1", "A", 2, 2);
			var client = Helper.CreateClient("TEST123", "Test Client");
			var product = Helper.CreateProduct(client, "PROD1");
			var docketLine = (WhsDocketLine)Helper.CreateStock(whs.PK, client.PK, product.PK, 10m);
			Helper.Factory.Save();

			var inventory = (WhsInventoryView)docketLine.Inventory.FirstOrDefault();
			AssertNotNull(inventory);

			var webService = GetNewWebService(whs);
			var holdCode = "HEL";
			var holdChangeReason = "Hold for QA check";
			var qty = 7m;
			var response = webService.ChangeInventoryHoldCode(inventory.PK.ToGuid(), holdCode, holdChangeReason, qty);

			AssertEquals("Successful response, no error message returned.", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("Successful response, no error returned.", ErrorTypes.None, response.Error);

			var originalInventory = webService.Factory.Load<WhsInventoryView>(inventory.PK);
			AssertNotNull(originalInventory);
			AssertEquals("Original hold code is empty", true, string.IsNullOrEmpty(originalInventory.OriginalInventoryHeldCode));
			AssertEquals("Current hold code is empty", true, string.IsNullOrEmpty(originalInventory.WI_HeldCode));
			AssertEquals("Qty is 3m", 3m, originalInventory.WI_TotalUnits);

			var originalDocketLine = webService.Factory.Load<WhsDocketLine>(docketLine.PK);
			AssertNotNull(originalDocketLine);
			AssertEquals("Original hold code is empty", true, string.IsNullOrEmpty(originalDocketLine.WE_WHC_NKOriginalInventoryHeldCode));
			AssertEquals("Current hold code is empty", true, string.IsNullOrEmpty(originalDocketLine.WE_WHC_NKCurrentInventoryHeldCode));
			AssertEquals("Current hold reason is empty", true, string.IsNullOrEmpty(originalDocketLine.WE_CurrentHoldReason));
			AssertEquals("Stock on hand is 3", 3m, originalDocketLine.WE_StockOnHand);
			AssertEquals("Transaction qty is 10", 10m, originalDocketLine.WE_TransactionQuantity);

			var clonedDocketLine = webService.Factory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, docketLine.PK))[0];
			AssertEquals("Original hold code is blank", string.Empty, clonedDocketLine.WE_WHC_NKOriginalInventoryHeldCode);
			AssertEquals("Current hold code is HEL", "HEL", clonedDocketLine.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Current hold reason is Hold for QA check", "Hold for QA check", clonedDocketLine.WE_CurrentHoldReason);
			AssertEquals("Stock on hand is 7", 7m, clonedDocketLine.WE_StockOnHand);
			AssertEquals("It is not original inventory", false, clonedDocketLine.WE_IsOriginalInventory);
			AssertEquals("Transaction qty is 7", 7m, clonedDocketLine.WE_TransactionQuantity);

			var clonedInventory = (WhsInventoryView)clonedDocketLine.Inventory.FirstOrDefault();
			AssertNotNull(clonedInventory);
			AssertEquals("Original hold code is blank", string.Empty, clonedInventory.OriginalInventoryHeldCode);
			AssertEquals("Current hold code is HEL", "HEL", clonedInventory.WI_HeldCode);
			AssertEquals("Qty is 7m", 7m, clonedInventory.WI_TotalUnits);
		}

		public void TestChangeInventoryHoldCode_NoInventory()
		{
			var whs = Helper.CreateWarehouse("Warehouse1", "A", 2, 2);
			Helper.Factory.Save();

			var webService = GetNewWebService(whs);
			var holdCode = "HEL";
			var holdChangeReason = "Hold for QA check";
			var qty = 10m;
			var response = webService.ChangeInventoryHoldCode(Guid.Empty, holdCode, holdChangeReason, qty);
			AssertEquals("Failure response, error message returned.", "This inventory does not exist in the warehouse.", response.ErrorMessage);
			AssertEquals("Failure response, error type is BusinessValidationError.", ErrorTypes.BusinessValidationError, response.Error);
		}

		public void TestChangeInventoryHoldCode_HoldReasonExceedsMaximumLength()
		{
			var whs = Helper.CreateWarehouse("Warehouse1", "A", 2, 2);
			var client = Helper.CreateClient("TEST123", "Test Client");
			var product = Helper.CreateProduct(client, "PROD1");
			var docketLine = (WhsDocketLine)Helper.CreateStock(whs.PK, client.PK, product.PK, 10m);
			Helper.Factory.Save();

			var inventory = (WhsInventoryView)docketLine.Inventory.FirstOrDefault();
			AssertNotNull(inventory);

			var webService = GetNewWebService(whs);
			var holdCode = "HEL";
			var holdChangeReason = "012345678901234567890123456789012345678901234567891";
			var qty = 10m;
			var response = webService.ChangeInventoryHoldCode(inventory.PK.ToGuid(), holdCode, holdChangeReason, qty);
			AssertEquals("Failure response, error message returned.", "Hold Change Reason exceeds maximum length of 50 characters.", response.ErrorMessage);
			AssertEquals("Failure response, error type is BusinessValidationError.", ErrorTypes.BusinessValidationError, response.Error);
		}

		public void TestChangeInventoryHoldCode_QtyIsInvalid()
		{
			var whs = Helper.CreateWarehouse("Warehouse1", "A", 2, 2);
			var client = Helper.CreateClient("TEST123", "Test Client");
			var product = Helper.CreateProduct(client, "PROD1");
			var docketLine = (WhsDocketLine)Helper.CreateStock(whs.PK, client.PK, product.PK, 10m);
			Helper.Factory.Save();

			var inventory = (WhsInventoryView)docketLine.Inventory.FirstOrDefault();
			AssertNotNull(inventory);

			var webService = GetNewWebService(whs);
			var holdCode = "HEL";
			var holdChangeReason = "Hold for QA check";
			var qty = -1m;
			var response = webService.ChangeInventoryHoldCode(inventory.PK.ToGuid(), holdCode, holdChangeReason, qty);
			AssertEquals("Failure response, error message returned.", "Quantity cannot be negative or zero.", response.ErrorMessage);
			AssertEquals("Failure response, error type is BusinessValidationError.", ErrorTypes.BusinessValidationError, response.Error);

			qty = 0m;
			response = webService.ChangeInventoryHoldCode(inventory.PK.ToGuid(), holdCode, holdChangeReason, qty);
			AssertEquals("Failure response, error message returned.", "Quantity cannot be negative or zero.", response.ErrorMessage);
			AssertEquals("Failure response, error type is BusinessValidationError.", ErrorTypes.BusinessValidationError, response.Error);

			qty = 11m;
			response = webService.ChangeInventoryHoldCode(inventory.PK.ToGuid(), holdCode, holdChangeReason, qty);
			AssertEquals("Failure response, error message returned.", "Quantity cannot be greater than Available To Transfer Quantity.", response.ErrorMessage);
			AssertEquals("Failure response, error type is BusinessValidationError.", ErrorTypes.BusinessValidationError, response.Error);
		}

		public void TestChangeInventoryHoldCode_Error()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locationType = Helper.CreateLocationType("NOR", "Normal Location", false, 2, LocationClasses.Codes.NOR);
			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_WLT_LocationType = locationType.PK;
			var location2 = data.Whs1.FindLocation("A-2");
			location2.WLV_WLT_LocationType = locationType.PK;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 64m, location1, string.Empty);
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-1", "A-2");
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();

			var inventory = (WhsInventoryView)transferLine.Inventory.FirstOrDefault();
			AssertNotNull(inventory);

			var webService = GetNewWebService(data.Whs1);
			var holdCode = "HEL";
			var holdChangeReason = "Hold for QA check";
			var qty = 1m;
			var response = webService.ChangeInventoryHoldCode(inventory.PK.ToGuid(), holdCode, holdChangeReason, qty);

			AssertEquals("Failure response, error message returned.", "Failed to change inventory hold code.", response.ErrorMessage);
			AssertEquals("Failure response, error type is BusinessValidationError.", ErrorTypes.BusinessValidationError, response.Error);
		}

		#endregion
	}
}
