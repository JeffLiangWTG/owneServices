using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	interface IOverCommitPickLinesDeferTriggerStrategy
	{
	}

	class OverCommitPickLinesDeferTriggerStrategy : DeferTriggerOnUpdateConditionStrategy, IOverCommitPickLinesDeferTriggerStrategy
	{
		OverCommitPickLinesDeferTriggerStrategy()
		{
		}

		static IReadOnlyCollection<SchemaColumn> SchemaColumnsThatAreExcludedFromTheTrigger => new SchemaColumn[]
		{
			WhsPickLineSchema.PK,
			WhsPickLineSchema.WZ_GS_NKAssignedTo,
			WhsPickLineSchema.WZ_VerifiedEmpty,
			WhsPickLineSchema.WZ_OriginalReservedQty,
			WhsPickLineSchema.WZ_F3_NKAllocatedPackType
		};

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged => WhsPickLineSchema.All.Except(SchemaColumnsThatAreExcludedFromTheTrigger);
	}
}
