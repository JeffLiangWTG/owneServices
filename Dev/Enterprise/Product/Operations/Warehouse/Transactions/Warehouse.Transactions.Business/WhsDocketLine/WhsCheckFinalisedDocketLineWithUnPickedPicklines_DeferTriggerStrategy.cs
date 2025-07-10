using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	#region WhsCheckFinalisedDocketLineWithUnPickedPicklines_DeferTriggerStrategy

	interface IWhsCheckFinalisedDocketLineWithUnPickedPicklines_DeferTriggerStrategy { }

	class WhsCheckFinalisedDocketLineWithUnPickedPicklines_DeferTriggerStrategy : DeferTriggerOnUpdateConditionStrategy, IWhsCheckFinalisedDocketLineWithUnPickedPicklines_DeferTriggerStrategy
	{
		WhsCheckFinalisedDocketLineWithUnPickedPicklines_DeferTriggerStrategy() { }

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged => new[] { WhsDocketLineSchema.WE_DocketLineStatus };

		protected override bool ShouldDeferTrigger(BusinessObject businessEntity)
		{
			return base.ShouldDeferTrigger(businessEntity) && businessEntity is WhsDocketLine docketLine && docketLine.IsFinalised;
		}

		protected override bool ShouldInsertsBeDeferred => false;
	}

	#endregion
}
