using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class ILineWithInventoryExtensions
	{
		#region AddErrorsFromCreatedInventory

		public static void AddErrorsFromCreatedInventory<T>(this T line)
			where T : BusinessObject, ILineWithInventory
		{
			foreach (WhsInventoryView inventory in line.Inventory.Where(o => o.HasErrors))
			{
				line.AddRowError(Res.GetString("0258c44c-727c-445b-8b88-a09e36abfd1e", "Could not update Inventory:\r\n{0}\r\nTry deleting this line and re-adding it.", inventory.GetErrors().ToUniqueMessageListString()));
			}
		}

		#endregion

		#region IncreaseStockInLocation

		public static void IncreaseStockInLocation<T>(this T line, ZGuid originalInDocketLineForRating, ZDecimal quantity)
			where T : BusinessObject, ILineWithInventory
		{
			line.IncreaseStockInLocation(originalInDocketLineForRating, quantity, line.TransactionInventoryStatus);
		}

		public static void IncreaseStockInLocation<T>(this T line, ZGuid originalInDocketLineForRating, ZDecimal quantity, ZString inventoryStatus)
			where T : BusinessObject, ILineWithInventory
		{
			EnsureLineIsValid(line, quantity);

			var docket = line.ParentDocket;
			var destinationInventory = line.Factory.NewWithPrimaryKey<WhsInventoryView>(line.PK.ToGuid());
			line.Inventory.Add(destinationInventory);

			destinationInventory.WI_InDocketLineType = docket.WD_DocketType;
			destinationInventory.WI_WE_InDocketLine = line.PK;
			destinationInventory.WI_ArrivalDate = line.ArrivalDate;
			destinationInventory.WI_WD = docket.PK;
			destinationInventory.WI_OH_Client = docket.WD_OH_Client;
			destinationInventory.WI_WW_Whs = docket.WD_WW_Whs;

			// inventory is the same DB row as a transaction line that created it, therefore product and pack qty are already correctly set
			// reseting product and pack type in business layer will only cause rounding errors when qty are recalculated during package type setup
			var inventoryRow = ((IBusinessObjectInternals)destinationInventory).Row;
			inventoryRow[WhsInventoryViewSchema.Constants.WI_OP] = line.ProductPK.ToGuid();
			inventoryRow[WhsInventoryViewSchema.Constants.WI_F3_NKPackType] = line.PackType;

			destinationInventory.WI_InDocketLineUnits = 0m;
			destinationInventory.WI_TotalUnits = quantity; // WI_TotalUnits must be set after WI_F3_NKPackType, so unit conversion does not cause rounding errors
			destinationInventory.WI_WL = line.TransactionLocation;
			destinationInventory.WI_PalletID = line.TransactionPalletID;
			destinationInventory.WI_WE_OriginalInDocketLineForRating = originalInDocketLineForRating;
			destinationInventory.WI_InventoryStatus = inventoryStatus;
			destinationInventory.WI_HeldCode = line.TransactionInventoryHeldCode;
			destinationInventory.SetAttributes(line);
		}

		static void EnsureLineIsValid(ILineWithInventory line, ZDecimal quantity)
		{
			if (quantity < 0m)
			{
				throw new InvalidOperationException("Should not try to create Inventory with a negative amount.");
			}

			if (line.ArrivalDate.IsEmpty)
			{
				throw new InvalidOperationException("Should not try to create Inventory if there is no Arrival Date set.");
			}

			if (line.Inventory.Count > 0)
			{
				throw new InvalidOperationException("Should not try to create Inventory if it already exists for this line.");
			}

			if (!line.CanCreateInventory)
			{
				throw new InvalidOperationException("Should not create Inventory if CanCreateInventory flag is not set.");
			}

			if (line.PackType.IsEmpty)
			{
				throw new InvalidOperationException("Should not create Inventory if PackType is not set.");
			}
		}

		#endregion
	}
}
