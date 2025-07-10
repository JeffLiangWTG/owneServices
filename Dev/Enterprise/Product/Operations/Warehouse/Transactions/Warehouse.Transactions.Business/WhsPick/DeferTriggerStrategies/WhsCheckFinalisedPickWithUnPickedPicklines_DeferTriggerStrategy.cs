using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	#region WhsCheckFinalisedPickWithUnPickedPicklines_DeferTriggerStrategy

	interface IWhsCheckFinalisedPickWithUnPickedPicklines_DeferTriggerStrategy { }

	class WhsCheckFinalisedPickWithUnPickedPicklines_DeferTriggerStrategy : DeferTriggerOnUpdateConditionStrategy, IWhsCheckFinalisedPickWithUnPickedPicklines_DeferTriggerStrategy
	{
		WhsCheckFinalisedPickWithUnPickedPicklines_DeferTriggerStrategy() { }

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged => new[] { WhsPickSchema.WP_PickStatus };

		protected override bool ShouldDeferTrigger(BusinessObject businessEntity)
		{
			return base.ShouldDeferTrigger(businessEntity) && businessEntity is WhsPick pick && pick.IsFinalised;
		}

		protected override bool ShouldInsertsBeDeferred => false;
	}

	#endregion
}
