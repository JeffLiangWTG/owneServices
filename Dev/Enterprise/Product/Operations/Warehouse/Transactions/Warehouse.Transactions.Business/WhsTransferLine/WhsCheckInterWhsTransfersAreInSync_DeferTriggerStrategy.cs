using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	#region WhsCheckInterWhsTransfersAreInSync_DeferTriggerStrategy

	interface IWhsCheckInterWhsTransfersAreInSync_DeferTriggerStrategy { }

	class WhsCheckInterWhsTransfersAreInSync_DeferTriggerStrategy : DeferTriggerOnUpdateConditionStrategy, IWhsCheckInterWhsTransfersAreInSync_DeferTriggerStrategy
	{
		WhsCheckInterWhsTransfersAreInSync_DeferTriggerStrategy() { }

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged
			=> WhsDocketLineSchema.All.Except(SchemaColumnsThatAreExcludedFromTheTrigger);

		static SchemaColumn[] SchemaColumnsThatAreExcludedFromTheTrigger => new SchemaColumn[]
		{
				// NOTE: Do not add to this list without carefully considering whether the property should be synchronised
				WhsDocketLineSchema.PK,
				WhsDocketLineSchema.WE_IsValid,
				WhsDocketLineSchema.WE_WD,
				WhsDocketLineSchema.WE_WE_MatchingLine,
				WhsDocketLineSchema.WE_WE_ParentDocketLine,
				WhsDocketLineSchema.WE_WE_OriginalDocketLineForRating,
				WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode,
				WhsDocketLineSchema.WE_CurrentHoldReason,
				WhsDocketLineSchema.WE_CurrentInventoryStatus,
				WhsDocketLineSchema.WE_StockOnHand,
				WhsDocketLineSchema.WE_LineNo,
				WhsDocketLineSchema.WE_SubLineNo,
				WhsDocketLineSchema.WE_AdjustmentArrivalDate,
				WhsDocketLineSchema.WE_SystemLastEditTimeUtc,
				WhsDocketLineSchema.WE_SystemLastEditUser
		};

		protected override bool ShouldDeferTrigger(BusinessObject businessEntity)
		{
			return base.ShouldDeferTrigger(businessEntity) && businessEntity is WhsTransferLine transferLine && (transferLine.Docket?.IsInterWarehouseTransfer ?? false);
		}

		protected override bool ShouldInsertsBeDeferred => false;
	}

	#endregion
}
