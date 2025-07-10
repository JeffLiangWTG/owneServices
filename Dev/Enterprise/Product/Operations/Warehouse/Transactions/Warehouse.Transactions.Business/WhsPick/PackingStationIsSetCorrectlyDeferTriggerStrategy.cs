using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	interface IPackingStationIsSetCorrectlyDeferTriggerStrategy
	{
	}

	class PackingStationIsSetCorrectlyDeferTriggerStrategy : DeferTriggerOnUpdateConditionStrategy, IPackingStationIsSetCorrectlyDeferTriggerStrategy
	{
		PackingStationIsSetCorrectlyDeferTriggerStrategy()
		{
		}

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged
			=> new SchemaColumn[] { WhsPickSchema.WP_WL_PackingStation };
	}
}
