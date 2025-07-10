using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	interface IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_Parent
	{
	}

	class WhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_Parent : DeferTriggerOnUpdateConditionStrategy, IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_Parent
	{
		WhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_Parent()
		{
		}

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged => new SchemaColumn[]
		{
			WhsDocketLineSchema.WE_TransactionQuantity,
			WhsDocketLineSchema.WE_WE_ParentDocketLine,
			WhsDocketLineSchema.WE_IsOriginalInventory,
		};

		protected override bool ShouldDeferTrigger(BusinessObject businessEntity)
		{
			var result = base.ShouldDeferTrigger(businessEntity);
			if (result)
			{
				var docketLine = (WhsDocketLine)businessEntity;
				result = (docketLine.WE_WE_ParentDocketLine.IsValid || docketLine.WE_WE_ParentDocketLineInfo.OriginalValue.IsValid)
					&& (!docketLine.WE_IsOriginalInventory || !(ZBool)docketLine.WE_IsOriginalInventoryInfo.OriginalValue);
			}

			return result;
		}

		protected override bool ShouldInsertsBeDeferred => false;
	}
}
