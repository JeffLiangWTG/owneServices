using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	static class WhsWorkOrderHelper
	{
		public static WhsReceiveLine AddNewInventoryFromWorkOrderLine(WorkOrderStagingLocationHelper stagingLocationHelper, WhsPick pick, WhsReceive receive, WhsComponentOrderLine referenceLine, WhsComponentOrderLine kitLineFromDisassembly, ZDecimal quantity, ZDateTimeOffset arrivalDate)
		{
			var lineToUseForLocation = kitLineFromDisassembly ?? referenceLine;
			if (lineToUseForLocation.Docket.WD_WP != pick.PK)
			{
				throw new ArgumentException("Kit line or Component passed in must be for the same Pick.");
			}

			var receiveLine = CreateNewInventory(receive, referenceLine.WE_OP, referenceLine.WE_F3_NKPackType, quantity, arrivalDate);
			var inventory = receiveLine.Inventory[0];

			// part attributes
			inventory.WI_PartAttrib1 = referenceLine.WE_PartAttrib1;
			inventory.WI_PartAttrib2 = referenceLine.WE_PartAttrib2;
			inventory.WI_PartAttrib3 = referenceLine.WE_PartAttrib3;
			inventory.WI_SerialNumber = referenceLine.WE_SerialNumber;
			inventory.WI_PackingDate = referenceLine.WE_PackingDate;
			inventory.WI_ExpiryDate = referenceLine.WE_ExpiryDate;

			// location
			inventory.WI_WL = GetLocationForInventory(pick, stagingLocationHelper, lineToUseForLocation, isDisassembly: kitLineFromDisassembly != null);

			return receiveLine;
		}

		public static void CopyAttributesFromPickedInventory(WhsReceiveLine newInventory, WhsPickLine referencePickLine)
		{
			// we need to get the original picked Inventory as Transfers does not copy Customs Data
			var pickedInventory = referencePickLine.InventoryLineForAvailableInventory;
			newInventory.SetAttributes(pickedInventory);
			var customsData = newInventory.CustomsData;
			using (customsData.SuspendUpdatingDocketLineEntryKey())
			{
				var inventoryCustomsData = pickedInventory.CustomsData;
				customsData.CopyPersistantValuesFromAnotherAttribute(inventoryCustomsData);
				customsData.WB_BondedWhsQty = inventoryCustomsData.WB_BondedWhsQty;
				customsData.WB_WB_InwardsEntry = inventoryCustomsData.WB_WB_InwardsEntry;
			}

			newInventory.WE_WE_OriginalDocketLineForRating = pickedInventory.WE_WE_OriginalDocketLineForRating;
		}

		public static WhsReceiveLine CreateNewInventory(WhsReceive receive, ZGuid product, ZString? packType, ZDecimal quantity, ZDateTimeOffset arrivalDate)
		{
			var receiveLine = receive.Lines.AddNew();
			var inventory = receiveLine.Inventory[0];
			inventory.WI_WD = receive.PK;
			inventory.WI_OH_Client = receive.WD_OH_Client;
			inventory.WI_OP = product;

			if (packType.HasValue)
			{
				inventory.WI_F3_NKPackType = packType.Value;
			}

			inventory.WI_TotalUnits = quantity;
			inventory.WI_ExpectedReceiptQuantity = inventory.WI_TotalUnits;
			inventory.WI_ArrivalDate = arrivalDate;

			return receiveLine;
		}

		public static ZDecimal GetDisassemblyQuantityFromLine(WhsWorkOrderLine line, decimal quantity)
		{
			// get BOM product quantity for a single main product
			var parentLine = line.ParentLine;
			var bomProductInventory = parentLine.BillOfMaterials.Single(pi => pi.OE_OP_Component == line.WE_OP && pi.OE_F3_NKPackType == line.WE_F3_NKPackType);

			var supplierPart = line.SupplierPart;
			var bomProductQty = supplierPart.UnitConverter.Convert(bomProductInventory.OE_ComponentQty, bomProductInventory.OE_F3_NKPackType, supplierPart.OP_StockKeepingUnit);

			return quantity * bomProductQty;
		}

		static ZGuid GetLocationForInventory(WhsPick pick, WorkOrderStagingLocationHelper stagingLocationHelper, WhsComponentOrderLine line, bool isDisassembly)
		{
			ZGuid result;

			// attempt to get the Staging Location from the existing In-Transit Transfer Line if it exists
			IEnumerable<WhsPickLine> pickLines;

			if (isDisassembly)
			{
				pickLines = line.PickLines;
			}
			else
			{
				pickLines = line.WE_WE_ParentDocketLine.IsEmpty
					? line.ChildComponentLines.SelectMany(cl => cl.PickLines)
					: line.ParentLine.PickLines;
			}

			var inTransitTransferLine = pickLines
				.Select(pl => pl.InventoryLine)
				.OfType<WhsTransferLine>() // FindByPK() will attempt to construct a transfer from the WE_WD FK. To prevent a Docket Type mismatch we specifically only look at transfer lines
				.FirstOrDefault(l => l.WE_WL.IsValid && pick.Transfers.FindByPK(l.WE_WD) != null);

			if (inTransitTransferLine != null)
			{
				result = inTransitTransferLine.WE_WL;
			}
			else
			{
				// fallback to previous behaviour
				result = stagingLocationHelper.GetStagingLocationForWorkOrderLine(line);
			}

			return result;
		}
	}
}
