using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	#region WhsCheckTransactionAndPickQtyIsCorrectOnUpdateWE_TransactionQuantity_DeferTriggerStrategy

	interface IWhsCheckTransactionAndPickQtyIsCorrectOnUpdateWE_TransactionQuantity_DeferTriggerStrategy { }

	class WhsCheckTransactionAndPickQtyIsCorrectOnUpdateWE_TransactionQuantity_DeferTriggerStrategy : DeferTriggerOnUpdateConditionStrategy, IWhsCheckTransactionAndPickQtyIsCorrectOnUpdateWE_TransactionQuantity_DeferTriggerStrategy
	{
		protected WhsCheckTransactionAndPickQtyIsCorrectOnUpdateWE_TransactionQuantity_DeferTriggerStrategy() { }

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged => new[] { WhsDocketLineSchema.WE_TransactionQuantity };

		protected override bool ShouldInsertsBeDeferred => false;
	}

	#endregion
}
