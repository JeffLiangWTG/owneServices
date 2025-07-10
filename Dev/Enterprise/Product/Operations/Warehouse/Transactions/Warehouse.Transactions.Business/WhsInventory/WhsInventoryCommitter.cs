using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsInventoryCommitter<T> : ICommittedInventoryStrategy
		where T : BusinessObject, ILineWithCommittedPickLines
	{
		protected WhsInventoryCommitter(T transactionLine, bool throwExceptionIfHasMatchingLines)
		{
			TransactionLine = Argument.NotNull(transactionLine, "transactionLine");

			if (throwExceptionIfHasMatchingLines && transactionLine is ILineWithMatchingLines<T>)
			{
				throw new InvalidOperationException("Use WhsInventoryCommitterWithMatchingLines<> when you have a transaction line with matching lines.");
			}
		}

		public WhsInventoryCommitter(T transactionLine)
			: this(transactionLine, throwExceptionIfHasMatchingLines: true)
		{
		}

		protected T TransactionLine { get; }

		#region UncommitOverPickedOrNonMatchingInventory

		public void UncommitOverPickedOrNonMatchingInventory()
		{
			UncommitNonMatchingInventory(TransactionLine);
			UncommitOverPickedInventory(TransactionLine);
			UncommitOverPickedOrNonMatchingInventoryCore();
		}

		protected virtual void UncommitOverPickedOrNonMatchingInventoryCore()
		{
		}

		#region UncommitNonMatchingInventory

		void UncommitNonMatchingInventory(T transactionLine)
		{
			var pickLines = transactionLine.PickLines;

			for (int i = pickLines.Count - 1; i >= 0; i--)
			{
				var pickLine = pickLines[i];
				if (!pickLine.IsPickedFromPutawayLocation && DoesPickLineNotMatchTransactionLine(transactionLine, pickLine))
				{
					pickLine.Delete();
				}
			}
		}

		protected bool DoesPickLineNotMatchTransactionLine(T transactionLine, WhsPickLine pickLine)
		{
			var inventoryLine = pickLine.InventoryLine;
			var location = transactionLine.LocationToCommit;
			return
				transactionLine.ProductPK != inventoryLine.WE_OP ||
				(transactionLine.TransactionInventoryStatus != inventoryLine.WE_CurrentInventoryStatus && !TransactionLineIsInTransitButHoldCodeMatchesInventoryLineStatus(transactionLine, inventoryLine)) ||
				transactionLine.TransactionInventoryHeldCode != inventoryLine.WE_WHC_NKCurrentInventoryHeldCode ||
				location == null ||
				location.PK != inventoryLine.WE_WL ||
				!transactionLine.PalletIDToCommit.EqualsIgnoringCase(inventoryLine.WE_PalletID) ||
				(!transactionLine.ArrivalDate.IsEmpty && transactionLine.ArrivalDate.Date != inventoryLine.WE_AdjustmentArrivalDate.Date) || // compare date only.
				!transactionLine.PackageGroupID.EqualsIgnoringCase(inventoryLine.WE_PackageGroupId) ||
				!(AttributeComparer.Compare(transactionLine, inventoryLine));
		}

		// Tested in WhsInventoryCommitterWithMatchingLines.TestUncommitOverPickedOrNotMatchingInventory_DoesNotUncommitInTransitInventory
		bool TransactionLineIsInTransitButHoldCodeMatchesInventoryLineStatus(T transactionLine, WhsDocketLine inventoryLine)
		{
			return transactionLine.IsInTransit &&
				((transactionLine.TransactionInventoryHeldCode.IsEmpty && inventoryLine.WE_CurrentInventoryStatus == InventoryStatus.Codes.Available) ||
				(!transactionLine.TransactionInventoryHeldCode.IsEmpty && inventoryLine.WE_CurrentInventoryStatus == InventoryStatus.Codes.Held));
		}

		#endregion

		#region UncommitOverPickedInventory

		protected void UncommitOverPickedInventory(T transactionLine)
		{
			var overCommittedQuantity = transactionLine.GetQtyCommittedToThisLine() - transactionLine.TransactionQty;
			if (overCommittedQuantity > 0m)
			{
				var pickLines = transactionLine.PickLines;

				for (int i = pickLines.Count - 1; i >= 0 && overCommittedQuantity > 0m; i--)
				{
					var pickLine = pickLines[i];
					if (overCommittedQuantity >= pickLine.WZ_Units)
					{
						overCommittedQuantity -= pickLine.WZ_Units;
						pickLine.Delete();
					}
					else
					{
						pickLine.WZ_Units -= overCommittedQuantity;
						overCommittedQuantity = 0m;
						break;
					}
				}
			}
		}

		#endregion

		#endregion

		#region CommitInventory

		public void CommitInventory()
		{
			UncommitOverPickedOrNonMatchingInventory();

			var docket = TransactionLine.ParentDocket;
			if (TransactionLine.ProductPK.IsValid && docket != null && docket.Client != null)
			{
				var transactionQty = TotalTransactionQty;
				var quantityToCommit = transactionQty - TotalQtyCommitted;
				var quantityNotCommitted = quantityToCommit;
				if (transactionQty > 0m && quantityToCommit > 0m)
				{
					quantityNotCommitted = AllocateStockToExistingPickLines(quantityToCommit);
					if (quantityNotCommitted > 0m)
					{
						quantityNotCommitted = AllocateStockToNewPickLines(quantityNotCommitted);
					}
				}
			}
		}

		#region AllocateStockToExistingPickLines

		decimal AllocateStockToExistingPickLines(decimal quantityToCommit)
		{
			quantityToCommit -= AllocateStockToPickLines(TransactionLine, quantityToCommit);
			quantityToCommit -= AllocateStockToExistingPickLinesCore(quantityToCommit);

			return quantityToCommit;
		}

		protected virtual decimal AllocateStockToExistingPickLinesCore(decimal quantityToCommit)
		{
			return 0m;
		}

		protected decimal AllocateStockToPickLines(T transactionLine, decimal uncommittedQty)
		{
			var qtyCommitted = 0m;

			foreach (var pickLine in transactionLine.PickLines)
			{
				var availableForTransfer = pickLine.Inventory.WI_AvailableToTransferQuantity;
				if (availableForTransfer > 0m)
				{
					var canCommitQty = Math.Min(availableForTransfer, uncommittedQty);
					pickLine.WZ_Units += canCommitQty;
					qtyCommitted += canCommitQty;
					uncommittedQty -= canCommitQty;
				}

				if (uncommittedQty == 0m)
				{
					break; // allocation fulfilled.
				}
			}

			return qtyCommitted;
		}

		#endregion

		#region AllocateStockToNewPickLines

		ZDecimal AllocateStockToNewPickLines(decimal quantityToCommit)
		{
			var uncommittedQty = quantityToCommit;
			var matchingInventories = GetMatchingInventories();

			var lineWithPickingDetails = TransactionLine as ILineWithPickAndPutawayDetails;
			var pickedBy = lineWithPickingDetails != null ? lineWithPickingDetails.PickedBy : ZString.Empty;

			foreach (var inventory in matchingInventories)
			{
				var canCommitQty = Math.Min(uncommittedQty, inventory.WI_AvailableToTransferQuantity);
				CreateAndLinkPickLine(inventory, canCommitQty, pickedBy);
				uncommittedQty -= canCommitQty;

				if (uncommittedQty == 0m)
				{
					break; // allocation fulfilled.
				}
			}

			SetPerPackageQuantity(matchingInventories);

			return uncommittedQty;
		}

		#region GetMatchingInventories

		IEnumerable<WhsInventoryView> GetMatchingInventories()
		{
			var filter = GetMatchingInventoriesFilter();
			var inventories = TransactionLine.Factory.Load<WhsInventoryView>(filter);
			foreach (var inventory in inventories) // For Docket Line information
			{
				inventory.Factory.AddFetchHint(WhsDocketLineSchema.Constants.TableName, inventory.WI_WE_InDocketLine);
			}

			var inventoriesToConsider = inventories.Where(inv => inv.CanCommitToTransactionLine(TransactionLine));
			var result_WithMatchingPackageGroupID = IsUSBondedTransaction ? inventoriesToConsider.Where(i => i.PackageGroupId.EqualsIgnoringCase(TransactionLine.PackageGroupID)) : inventoriesToConsider;

			var heldCode = TransactionLine.TransactionInventoryHeldCode;
			var result_WithMatchingInventoryHeldCode = result_WithMatchingPackageGroupID.Where(
					i => i.WI_HeldCode.EqualsIgnoringCase(heldCode) ||
						(TransactionLine.TransactionInventoryStatus.EqualsIgnoringCase(InventoryStatus.Codes.Held) && heldCode.IsEmpty && !i.IsDamaged)).ToArray(); // Special case for adjustments from stocktake

			// For WI_AvailableToTransferQuantity
			result_WithMatchingInventoryHeldCode.ForEach(i => i.Factory.AddFetchHint(WhsPickLineSchema.WZ_WE_InventoryLine, i.WI_WE_InDocketLine));
			result_WithMatchingInventoryHeldCode.ForEach(i => i.LoadFetchHintsForPickLines());
			return result_WithMatchingInventoryHeldCode.Where(i => i.WI_AvailableToTransferQuantity > 0).ToArray();
		}

		ZQuery GetMatchingInventoriesFilter()
		{
			var isMandatoryCheck = true;
			return WhsInventoryFilterBuilder.BuildFilter(
				TransactionLine.ParentDocket.Client,
				TransactionLine.Product.Parent,
				null,
				null,
				TransactionLine.LocationToCommit,
				TransactionLine.PalletIDToCommit,
				TransactionLine.BondedEntryKey,
				TransactionLine.ExpiryDate,
				TransactionLine.PackingDate,
				TransactionLine.PartAttrib1,
				TransactionLine.PartAttrib2,
				TransactionLine.PartAttrib3,
				TransactionLine.SerialNumber,
				TransactionLine.ArrivalDate,
				TransactionLine.TransactionInventoryStatus,
				isMandatoryCheck);
		}

		#endregion

		#region CreateAndLinkPickLine

		protected virtual void CreateAndLinkPickLine(WhsInventoryView inventory, decimal quantityToCommit, ZString pickedBy)
		{
			CreatePickLine(TransactionLine, inventory, quantityToCommit, pickedBy);
		}

		protected void CreatePickLine(T transactionLine, WhsInventoryView inventory, decimal quantityToCommit, ZString pickedBy)
		{
			var pickLine = transactionLine.PickLines.AddNew();
			pickLine.WZ_WE_InventoryLine = inventory.WI_WE_InDocketLine;
			pickLine.WZ_Units = quantityToCommit;
			pickLine.WZ_GS_NKAssignedTo = pickedBy;
		}

		#endregion

		#region SetPerPackageQuantity

		void SetPerPackageQuantity(IEnumerable<WhsInventoryView> matchingInventories)
		{
			var firstInventory = IsUSBondedTransaction ? matchingInventories.FirstOrDefault() : null;
			if (firstInventory != null)
			{
				var firstPerPackageQty = firstInventory.PerPackageQty;

				// Receive validation should handle Per Package Qty being the same 
				// for matching Inventory but do a safety check to make sure
				if (matchingInventories.Any(i => i.PerPackageQty != firstPerPackageQty))
				{
					throw new InvalidOperationException(string.Format(Culture.Invariant, "Should not have multiple matching inventory for Docket Line {0} with different Per Package Quantities.", TransactionLine.LineNo));
				}

				TransactionLine.PerPackageQty = firstPerPackageQty;
				SetPerPackageQuantityCore();
			}
		}

		protected virtual void SetPerPackageQuantityCore()
		{
		}

		#endregion

		#endregion

		#region IsUSBondedTransaction

		bool IsUSBondedTransaction
		{
			get
			{
				var docket = TransactionLine.ParentDocket;
				return docket != null && docket.IsUSBonded;
			}
		}

		#endregion

		#endregion

		// interfaces

		#region ICommittedInventoryStrategy

		#region TotalTransactionQty

		public ZDecimal TotalTransactionQty
		{
			get { return GetTotalTransactionQtyForMainTransactionLine(); }
		}

		protected virtual ZDecimal GetTotalTransactionQtyForMainTransactionLine()
		{
			return TransactionLine.TransactionQty;
		}

		#endregion

		#region TotalQtyCommitted

		public ZDecimal TotalQtyCommitted
		{
			get { return GetTotalQtyCommittedToMainTransactionLine(); }
		}

		protected virtual ZDecimal GetTotalQtyCommittedToMainTransactionLine()
		{
			return TransactionLine.GetQtyCommittedToThisLine();
		}

		#endregion

		#endregion
	}
}
