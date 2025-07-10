using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	#region WhsCheckLinesDoNotExceedLocationCapacity_DeferTriggerStrategy

	interface IWhsCheckLinesDoNotExceedLocationCapacity_DeferTriggerStrategy { }

	class WhsCheckLinesDoNotExceedLocationCapacity_DeferTriggerStrategy : DeferTriggerOnUpdateConditionStrategy, IWhsCheckLinesDoNotExceedLocationCapacity_DeferTriggerStrategy
	{
		WhsCheckLinesDoNotExceedLocationCapacity_DeferTriggerStrategy() { }

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged
			=> new SchemaColumn[]
			{
				WhsDocketLineSchema.WE_TransactionQuantity,
				WhsDocketLineSchema.WE_StockOnHand,
				WhsDocketLineSchema.WE_WL,
			};

		protected override bool ShouldDeferTrigger(BusinessObject businessEntity)
		{
			return base.ShouldDeferTrigger(businessEntity)
				&& businessEntity is WhsAdjustmentLine adjustmentLine
				&& adjustmentLine.DoesAdjustmentHasPositiveChangesThatAffectAdjustmentLineLocationCapacity;
		}
	}

	#endregion
}
