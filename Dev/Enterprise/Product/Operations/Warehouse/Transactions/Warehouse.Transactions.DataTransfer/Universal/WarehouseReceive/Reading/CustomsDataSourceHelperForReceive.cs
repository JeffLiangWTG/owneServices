using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class CustomsDataSourceHelperForReceive : CustomsDataSourceHelper<WhsReceive>
	{
		public CustomsDataSourceHelperForReceive(TopLevelDataObject topLevelDataObject, IDataContextDataObject topLevelDataContext)
			: base(topLevelDataObject, topLevelDataContext)
		{
		}

		protected override string DocketType => WhsReceiveDataObjectReader.ReceiveDocketType;

		protected override RecipientRoleType CustomsDeclarationRecipientRoleType => RecipientRoleType.BWI;

		#region Cancel Out Docket

		protected override IEnumerable<WhsInventoryView> CancelOutDocket(WhsReceive receive)
		{
			EnsureNoStockHasBeenOrderedBeforeAdjustingOut(receive);

			var adjustment = BondedAdjustmentCreator.AdjustOut(receive, (error) => this.ThrowImportFailureExceptionIfNotEmpty(new ZStringBuilder(error)));
			if (!adjustment.IsFinalised)
			{
				var adjustmentErrors = string.Join("\r\n", adjustment.NotificationsIncludingChildren.GetErrors().Select(e => e.Message));
				var message = new ZStringBuilder(Res.GetString("500c6f4a-5c54-43bd-8065-58092c675132",
					"{0} could not be finalized into the Warehouse for Customs Job {1} because of the following AMENDMENT error(s):\r\n{2}", DocketType, CustomsJobNo, adjustmentErrors));

				this.ThrowImportFailureExceptionIfNotEmpty(message);
			}

			return Enumerable.Empty<WhsInventoryView>();
		}

		void EnsureNoStockHasBeenOrderedBeforeAdjustingOut(WhsReceive receive)
		{
			foreach (WhsInventoryView inventory in receive.Inventory)
			{
				if (inventory.WI_TotalUnits != inventory.WI_InDocketLineUnits && IsAnyNonCancelledOrderAttachedToThisInventory(receive, inventory))
				{
					var errorMessage = Res.GetString("7cfe9da6-cf5c-496d-9a0f-efe98e02b17f", "Cannot amend Receipt {0} as some of its stock has been released.", receive.WD_DocketID);
					this.ThrowImportFailureExceptionIfNotEmpty(new ZStringBuilder(errorMessage));
				}
			}
		}

		/// <summary>
		/// this check is no longer necessary but we will leave it in place just in case we missed something
		/// </summary>
		bool IsAnyNonCancelledOrderAttachedToThisInventory(WhsReceive receive, WhsInventoryView inventory)
		{
			var pickLinesQuery = new ZQuery();
			pickLinesQuery.AddToFilter(WhsPickLineSchema.WZ_WE_InventoryLine, inventory.WI_WE_InDocketLine);

			foreach (var pickLine in receive.Factory.Load<WhsPickLine>(pickLinesQuery))
			{
				var orderLine = pickLine.DocketLine as WhsOrderLine;
				if (orderLine != null && !orderLine.Order.IsCancelled)
				{
					return true;
				}
			}

			return false;
		}

		#endregion
	}
}
