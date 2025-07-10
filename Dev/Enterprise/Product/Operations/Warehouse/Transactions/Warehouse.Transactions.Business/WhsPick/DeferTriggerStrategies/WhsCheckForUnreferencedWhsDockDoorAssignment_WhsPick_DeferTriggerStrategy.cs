using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	#region WhsCheckForUnreferencedWhsDockDoorAssignment_WhsPick_DeferTriggerStrategy

	interface IWhsCheckForUnreferencedWhsDockDoorAssignment_WhsPick_DeferTriggerStrategy { }

	class WhsCheckForUnreferencedWhsDockDoorAssignment_WhsPick_DeferTriggerStrategy : DeferTriggerOnUpdateConditionStrategy, IWhsCheckForUnreferencedWhsDockDoorAssignment_WhsPick_DeferTriggerStrategy
	{
		WhsCheckForUnreferencedWhsDockDoorAssignment_WhsPick_DeferTriggerStrategy()
		{
		}

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged => new SchemaColumn[] { WhsPickSchema.WP_WDA_DockDoorAssignment };

		protected override bool ShouldInsertsBeDeferred => false;
	}

	#endregion
}
