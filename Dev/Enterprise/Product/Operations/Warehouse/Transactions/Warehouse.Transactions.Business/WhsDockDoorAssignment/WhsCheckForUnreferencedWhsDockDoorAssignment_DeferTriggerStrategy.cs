using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	#region WhsCheckForUnreferencedWhsDockDoorAssignment_DeferTriggerStrategy

	interface IWhsCheckForUnreferencedWhsDockDoorAssignment_DeferTriggerStrategy { }

	class WhsCheckForUnreferencedWhsDockDoorAssignment_DeferTriggerStrategy : DeferTriggerOnUpdateConditionStrategy, IWhsCheckForUnreferencedWhsDockDoorAssignment_DeferTriggerStrategy
	{
		WhsCheckForUnreferencedWhsDockDoorAssignment_DeferTriggerStrategy()
		{
		}

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged => [];
	}

	#endregion
}
