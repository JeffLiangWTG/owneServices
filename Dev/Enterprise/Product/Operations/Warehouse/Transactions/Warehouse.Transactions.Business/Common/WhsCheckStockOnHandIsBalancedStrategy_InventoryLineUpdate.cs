using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	abstract class WhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate : DeferTriggerOnUpdateConditionStrategy
	{
		protected WhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate()
		{
		}

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged => new SchemaColumn[]
		{
			WhsDocketLineSchema.WE_TransactionQuantity,
			WhsDocketLineSchema.WE_StockOnHand,
			WhsDocketLineSchema.WE_DocketLineStatus,
		};

		protected override bool ShouldDeferTrigger(BusinessObject businessEntity)
		{
			return base.ShouldDeferTrigger(businessEntity)
				&& ShouldDeferTriggerCore((WhsDocketLine)businessEntity);
		}

		protected abstract bool ShouldDeferTriggerCore(WhsDocketLine docketLine);

		protected override bool ShouldInsertsBeDeferred => false;
	}
}
