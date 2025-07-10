using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class ILineWithCommittedPickLinesExtensions
	{
		#region CheckEnoughInventoryExistsToCommit

		public static void CheckEnoughInventoryExistsToCommit<T>(this T transactionLine, ZPropertyInfo qtyInfo)
			where T : ILineWithCommittedPickLines, IBusinessObjectInternals
		{
			if (transactionLine.IsAllowedToValidateQty() && !qtyInfo.HasErrors())
			{
				var quantityCommitted = transactionLine.CommittedStrategy.TotalQtyCommitted;
				var transactionQty = transactionLine.CommittedStrategy.TotalTransactionQty;
				if (quantityCommitted < transactionQty)
				{
					var unitsValidationErrorMessage = WhsValidationHelper.GetNotEnoughStockMessage(transactionLine, quantityCommitted);
					if (!string.IsNullOrEmpty(unitsValidationErrorMessage))
					{
						qtyInfo.AddError(unitsValidationErrorMessage);
					}
				}
			}
		}

		static bool IsAllowedToValidateQty<T>(this T transactionLine)
			where T : ILineWithCommittedPickLines, IBusinessObjectInternals
		{
			return (transactionLine.IsInPreSaveValidation || transactionLine.IsFinalising) && transactionLine.LineHasProductAndClient();
		}

		static bool LineHasProductAndClient(this ILineWithCommittedPickLines transactionLine)
		{
			bool result = false;

			if (transactionLine.Product != null)
			{
				var docket = transactionLine.ParentDocket;
				result = docket != null && docket.Client != null;
			}

			return result;
		}

		#endregion

		#region DecreaseStockInLocation

		public static void DecreaseStockInLocation<T>(this T transactionLine, ZPropertyInfo transactionQtyPropertyInfo)
			where T : ILineWithCommittedPickLines, IBusinessObjectInternals
		{
			if (!transactionLine.IsFinalising)
			{
				throw new InvalidOperationException("Should not decrease Total Units on Inventory if not during Finalisation.");
			}

			var whs = transactionLine.ParentDocket?.Warehouse;
			var now = whs?.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow) ?? ZDateTimeOffset.Now;

			foreach (var pickLine in transactionLine.PickLines)
			{
				if (!pickLine.IsPicked)
				{
					pickLine.WZ_PickedDateTime = now;
				}
			}

			// Validation checks how much is committed vs the transaction quantity.
			// As committed qty is calculated from the sum of pick lines and ReduceStockIfPickLineIsPicked() makes
			// the pick line sum equal the inventory sum if it is overcommitted, we will get an error message if the
			// committed qty was too high.
			transactionLine.Validate(transactionQtyPropertyInfo);
		}

		#endregion

		#region GetQtyCommitted

		public static ZDecimal GetQtyCommittedToThisLine(this ILineWithCommittedPickLines transactionLine)
		{
			return transactionLine.PickLines.GetQtyCommitted();
		}

		#endregion

		#region ReloadChangedInventoryOnRollback

		public static void ReloadChangedInventoryOnRollback(this ILineWithCommittedPickLines line)
		{
			foreach (var pickLine in line.PickLines)
			{
				if (!pickLine.IsFinalised)
				{
					pickLine.InventoryLine.Reload();

					// when reloading inventory, the total units from the DB may have changed. it is not correct to keep 
					// picklines attached to such inventory in a picked state (as they did not actually pick stock / reduce inventory -- someone else did)
					// therefore we must unpick the picklines by clearing the pick time.
					using (((IWhsPickLineInternals)pickLine).TemporarilyAllowUnpickingPickLineWithNoStockChange_DoNotUse())
					{
						pickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty;
					}
				}
			}
		}

		#endregion
	}
}
