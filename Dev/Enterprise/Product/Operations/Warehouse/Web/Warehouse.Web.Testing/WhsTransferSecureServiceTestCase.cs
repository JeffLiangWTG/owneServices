using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	abstract class WhsTransferSecureServiceTestCase : WhsSecureServiceTestCase
	{
		#region CreatePickingTransferLineInfo

		protected WhsDocketLineInfo CreatePickingTransferLineInfo(string location, string palletID, OrgHeader client, OrgSupplierPart part, string heldCode, decimal quantity)
		{
			return CreatePickingTransferLineInfo(location, palletID, client, part, heldCode, quantity, ZDate.Empty, ZDate.Empty, "", "", "");
		}

		protected WhsDocketLineInfo CreatePickingTransferLineInfo(string location, string palletID, OrgHeader client, OrgSupplierPart part, string heldCode,
			decimal quantity, ZDate eD, ZDate pD, string pA1, string pA2, string pA3)
		{
			var transferLineInfo = new WhsDocketLineInfo();
			transferLineInfo.Location = location;
			transferLineInfo.PalletID = palletID;
			if (client != null)
			{
				transferLineInfo.ClientCode = client.OH_Code;
			}
			if (part != null)
			{
				transferLineInfo.Product = new WhsProductInfo(part);
				transferLineInfo.PackUQ = part.OP_StockKeepingUnit;
				transferLineInfo.QtyUQ = part.OP_StockKeepingUnit;
			}
			else
			{
				transferLineInfo.Product = new WhsProductInfo();
			}
			transferLineInfo.InventoryHeldCode = heldCode;
			transferLineInfo.Qty = quantity;
			transferLineInfo.Attribute1 = pA1;
			transferLineInfo.Attribute2 = pA2;
			transferLineInfo.Attribute3 = pA3;
			transferLineInfo.ExpiryDate = eD.IsValid ? eD.ToDateTime() : DateTime.MinValue;
			transferLineInfo.PackingDate = pD.IsValid ? pD.ToDateTime() : DateTime.MinValue;

			return transferLineInfo;
		}

		#endregion

		#region CreatePutawayTransferLineInfo

		protected static WhsDocketLineInfo CreatePutawayTransferLineInfo(OrgSupplierPart part, string sourcePalletID, string destPalletID, string destLocation)
		{
			return CreatePutawayTransferLineInfo(part, sourcePalletID, destPalletID, destLocation, 0m);
		}

		internal static WhsDocketLineInfo CreatePutawayTransferLineInfo(OrgSupplierPart part, string sourcePalletID, string destPalletID, string destLocation, ZDecimal qty, string heldCode = "")
		{
			return CreatePutawayTransferLineInfo(part, sourcePalletID, destPalletID, destLocation, qty, heldCode, ZDate.Empty, ZDate.Empty, "", "", "");
		}

		protected static WhsDocketLineInfo CreatePutawayTransferLineInfo(OrgSupplierPart part, string sourcePalletID, string destPalletID, string destLocation, ZDecimal qty, string heldCode, ZDateTime eD, ZDateTime pD, string pA1, string pA2, string pA3)
		{
			var transferLineInfo = new WhsDocketLineInfo();
			if (part != null)
			{
				transferLineInfo.Product = new WhsProductInfo(part);
				transferLineInfo.QtyUQ = part.OP_StockKeepingUnit;
			}
			else
			{
				transferLineInfo.Product = new WhsProductInfo();
			}

			transferLineInfo.PalletID = sourcePalletID;
			transferLineInfo.DestPalletID = destPalletID;
			transferLineInfo.DestLocation = destLocation;
			transferLineInfo.Qty = qty;
			transferLineInfo.InventoryHeldCode = heldCode;
			transferLineInfo.Attribute1 = pA1;
			transferLineInfo.Attribute2 = pA2;
			transferLineInfo.Attribute3 = pA3;
			if (!eD.IsEmpty)
			{
				transferLineInfo.ExpiryDate = eD.ToDateTime();
			}
			if (!pD.IsEmpty)
			{
				transferLineInfo.PackingDate = pD.ToDateTime();
			}

			return transferLineInfo;
		}

		#endregion

		#region Asserts

		protected void AssertTransferEventsCreated(WhsTransfer transfer, int countOfJobEnteredEvent, int countOfServiceCommenced, int countOfServiceSuspended, int countOfChangeOfIdentifier, int countOfJobFinalized, int countOfServiceCompleted)
		{
			CombineAssertions(() =>
			{
				AssertTransferEventLog(transfer, AutoEvents.WarehouseJobEntered, countOfJobEnteredEvent);
				AssertTransferEventLog(transfer, AutoEvents.ServiceCommenced, countOfServiceCommenced);
				AssertTransferEventLog(transfer, AutoEvents.ServiceSuspended, countOfServiceSuspended);
				AssertTransferEventLog(transfer, AutoEvents.ChangeOfIdentifier, countOfChangeOfIdentifier);
				AssertTransferEventLog(transfer, AutoEvents.ItemDocumentJobFinalised, countOfJobFinalized);
				AssertTransferEventLog(transfer, AutoEvents.ServiceCompleted, countOfServiceCompleted);
			});
		}

		protected void AssertTransferEventLog(WhsTransfer transfer, Event expectedEventType, int expectedEventCount)
		{
			AssertEquals($"Should find {expectedEventCount} of Event: {expectedEventType.Description}.", expectedEventCount, Helper.FindLogs(transfer.Logs, expectedEventType).Length);
		}

		protected void AssertTransferPutawayResponse(TransferPutawayWebServiceResponse response, string errorMessage, bool isAllTransferLinesTransferredOrFinalised, bool isFullPalletIDTransfered, bool isSingleProductTransfered,
			bool isValidDestLocation, bool isValidDestPalletID, bool isValidProduct, bool isValidSourcePalletID)
		{
			CombineAssertions(() =>
			{
				AssertEquals("ErrorMessage", errorMessage, response.ErrorMessage);
				AssertEquals("IsAllTransferLinesTransferredOrFinalised", isAllTransferLinesTransferredOrFinalised, response.IsAllTransferLinesTransferredOrFinalised);
				AssertEquals("IsFullPalletIDTransferred", isFullPalletIDTransfered, response.IsFullPalletIDTransferred);
				AssertEquals("IsSingleProductTransferred", isSingleProductTransfered, response.IsSingleProductTransferred);
				AssertEquals("IsValidDestLocation", isValidDestLocation, response.IsValidDestLocation);
				AssertEquals("IsValidDestPalletID", isValidDestPalletID, response.IsValidDestPalletID);
				AssertEquals("IsValidProduct", isValidProduct, response.IsValidProduct);
				AssertEquals("IsValidSourcePalletID", isValidSourcePalletID, response.IsValidSourcePalletID);
			});
		}

		internal static void AssertPutawayTransferLineData(WhsTransferLine transferLine, bool expectedIsFinalised, string expectedDestLocation, string expectedDestPalletID, ZDateTimeOffset expectedPutawayByTime, string expectedPutawayBy)
		{
			var expectedInventoryStatusIfNotInTransit = transferLine.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty ? InventoryStatus.Codes.Available : InventoryStatus.Codes.Held;
			CombineAssertions(() =>
			{
				AssertEquals("IsFinalised", expectedIsFinalised, transferLine.IsFinalised);
				AssertEquals("CurrentInventoryStatus", (expectedIsFinalised || !transferLine.IsPicked) ? expectedInventoryStatusIfNotInTransit : InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
				AssertEquals("OriginalInventoryStatus", (expectedIsFinalised || !transferLine.IsPicked) ? expectedInventoryStatusIfNotInTransit : InventoryStatus.Codes.InTransit, transferLine.WE_OriginalInventoryStatus);
				AssertEquals("DestLocation", expectedDestLocation, transferLine.LocationString);
				AssertEquals("DestPalletID", expectedDestPalletID, transferLine.WE_PalletID);
				AssertEquals("PutawayByTime", expectedPutawayByTime, transferLine.WE_PutawayTime);
				AssertEquals("PutawayBy", expectedPutawayBy, transferLine.WE_GS_NKPutawayBy);
			});
		}

		#endregion

	}
}
