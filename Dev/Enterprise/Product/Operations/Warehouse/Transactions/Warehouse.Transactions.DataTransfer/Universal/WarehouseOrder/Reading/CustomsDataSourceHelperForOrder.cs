using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class CustomsDataSourceHelperForOrder : CustomsDataSourceHelper<WhsOrder>
	{
		public CustomsDataSourceHelperForOrder(TopLevelDataObject topLevelDataObject, IDataContextDataObject topLevelDataContext)
			: base(topLevelDataObject, topLevelDataContext)
		{
		}

		protected override string DocketType => WhsOrderDataObjectReader.OrderDocketType;

		protected override RecipientRoleType CustomsDeclarationRecipientRoleType => RecipientRoleType.BWR;

		#region Cancel Out Docket

		protected override IEnumerable<WhsInventoryView> CancelOutDocket(WhsOrder order)
		{
			EnsurePickIsFinalised(order);
			var inventoryThatWasAdjustedBack = RevertInventoryQuantitiesTakenByOrder(order);
			CancelPickForBondAndChangeOfInventory(order.Pick);

			// mark order as cancelled
			order.CancelReactivateDocket();

			return inventoryThatWasAdjustedBack;
		}

		void EnsurePickIsFinalised(WhsOrder order)
		{
			if (!order.IsPickFinalised)
			{
				this.ThrowImportFailureExceptionIfNotEmpty(new ZStringBuilder(Res.GetString("c2b8b710-a239-40f7-b43d-98a359b4066b", "Only finalized Orders with finalized Picks in a Virtual Warehouse can have their attached inventory amended when being Canceled out.")));
			}
		}

		IEnumerable<WhsInventoryView> RevertInventoryQuantitiesTakenByOrder(WhsOrder order)
		{
			var inventoryThatWasAmended = new HashSet<WhsInventoryView>();

			// amend inventory back to normal
			foreach (WhsOrderLine line in order.Lines)
			{
				foreach (var pickLine in line.PickLines)
				{
					var inventory = RevertInventoryQuantitiesTakenByOrder(order, pickLine);
					if (!inventoryThatWasAmended.Contains(inventory))
					{
						inventoryThatWasAmended.Add(inventory);
					}
				}
			}

			return inventoryThatWasAmended;
		}

		WhsInventoryView RevertInventoryQuantitiesTakenByOrder(WhsOrder order, WhsPickLine pickLine)
		{
			var inventory = pickLine.Inventory;
			inventory.WI_TotalUnits += pickLine.WZ_Units;

			var inDocketLine = inventory.InDocketLine;
			if (inventory.WI_TotalUnits > inDocketLine.WE_TransactionQuantity)
			{
				// this should never happen
				this.ThrowImportFailureExceptionIfNotEmpty(new ZStringBuilder(Res.GetString("9893a7ff-811e-4835-be9c-64de37358800",
					"Failed to put back inventory when attempting to Customs-Cancel Order {0}. Amendment has been rejected.", order.WD_ExternalReference)));
			}

			return inventory;
		}

		void CancelPickForBondAndChangeOfInventory(WhsPick pick)
		{
			var warehouse = pick.Warehouse;
			if (warehouse == null || !(warehouse.WW_IsVirtualWarehouse || pick.Orders.Cast<WhsDocket>().All(o => o.IsImportingForChangeOfInventory)))
			{
				this.ThrowImportFailureExceptionIfNotEmpty(new ZStringBuilder(Res.GetString("45c3ae3c-0e5c-406f-8b33-9b716c776611",
					"You can only cancel a pick in a Virtual Warehouse or for Change of Inventory orders.")));
			}

			// unfinalise & cancel pick
			pick.WP_PickStatus = PickStatus.Codes.Created;
			pick.WP_FinalizedDateUtc = ZDateTime.Empty;

			foreach (var pickLine in pick.GetAllPickLines())
			{
				// Unpicking a PickLine is not normally valid but since we are cancelling out the Pick in a Virtual Warehouse, doing this is fine.
				using (((IWhsPickLineInternals)pickLine).TemporarilyAllowUnpickingPickLineWithNoStockChange_DoNotUse())
				{
					pickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty;
				}
			}

			pick.CancelPick();
		}

		#endregion
	}
}
