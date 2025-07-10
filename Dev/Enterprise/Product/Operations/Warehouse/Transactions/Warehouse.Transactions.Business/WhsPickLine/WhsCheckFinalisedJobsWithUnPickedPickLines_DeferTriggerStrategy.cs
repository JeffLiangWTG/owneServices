using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	#region WhsCheckFinalisedJobsWithUnPickedPickLines_DeferTriggerStrategy

	interface IWhsCheckFinalisedJobsWithUnPickedPickLines_DeferTriggerStrategy { }

	class WhsCheckFinalisedJobsWithUnPickedPickLines_DeferTriggerStrategy : DeferTriggerOnUpdateConditionStrategy, IWhsCheckFinalisedJobsWithUnPickedPickLines_DeferTriggerStrategy
	{
		WhsCheckFinalisedJobsWithUnPickedPickLines_DeferTriggerStrategy() { }

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged
			=> new SchemaColumn[]
			{
				WhsPickLineSchema.WZ_PickedDateTime,
				WhsPickLineSchema.WZ_WE_TransactionLine,
			};

		protected override bool ShouldInsertsBeDeferred => false;
	}

	#endregion
}
