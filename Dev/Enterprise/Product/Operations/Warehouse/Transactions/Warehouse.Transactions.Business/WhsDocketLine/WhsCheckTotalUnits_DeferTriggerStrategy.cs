using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	#region WhsCheckTotalUnits_DeferTriggerStrategy

	interface IWhsCheckTotalUnits_DeferTriggerStrategy { }

	class WhsCheckTotalUnits_DeferTriggerStrategy : DeferTriggerOnUpdateConditionStrategy, IWhsCheckTotalUnits_DeferTriggerStrategy
	{
		WhsCheckTotalUnits_DeferTriggerStrategy() { }

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged
			=> new[]
			{
				WhsDocketLineSchema.WE_StockOnHand,
				WhsDocketLineSchema.WE_TransactionQuantity,
			};

		protected override bool ShouldDeferTrigger(BusinessObject businessEntity)
		{
			return base.ShouldDeferTrigger(businessEntity) && businessEntity is WhsDocketLine docketLine && docketLine.IsInventoryLine;
		}

		protected override bool ShouldInsertsBeDeferred => false;
	}

	#endregion
}
