using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	#region WhsCheckTransactionAndPickQtyIsCorrectForWhsPickLine_OnUpdateConditionStrategy

	interface IWhsCheckTransactionAndPickQtyIsCorrectForWhsPickLine_OnUpdateConditionStrategy { }

	class WhsCheckTransactionAndPickQtyIsCorrectForWhsPickLine_OnUpdateConditionStrategy : DeferTriggerOnUpdateConditionStrategy, IWhsCheckTransactionAndPickQtyIsCorrectForWhsPickLine_OnUpdateConditionStrategy
	{
		WhsCheckTransactionAndPickQtyIsCorrectForWhsPickLine_OnUpdateConditionStrategy() { }

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged
			=> new SchemaColumn[]
			{
				WhsPickLineSchema.WZ_Units,
				WhsPickLineSchema.WZ_WE_TransactionLine,
			};
	}

	#endregion
}
