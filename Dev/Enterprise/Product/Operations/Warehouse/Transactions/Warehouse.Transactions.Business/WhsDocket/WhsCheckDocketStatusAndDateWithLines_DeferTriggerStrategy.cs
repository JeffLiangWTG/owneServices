using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	#region WhsCheckDocketStatusAndDateWithLines_DeferTriggerStrategy

	interface IWhsCheckDocketStatusAndDateWithLines_DeferTriggerStrategy { }

	class WhsCheckDocketStatusAndDateWithLines_DeferTriggerStrategy : DeferTriggerOnUpdateConditionStrategy, IWhsCheckDocketStatusAndDateWithLines_DeferTriggerStrategy
	{
		WhsCheckDocketStatusAndDateWithLines_DeferTriggerStrategy() { }

		protected override IEnumerable<SchemaColumn> ColumnsThatRequireTriggerDeferralWhenChanged
			=> new SchemaColumn[]
			{
				WhsDocketSchema.WD_DocketStatus,
				WhsDocketSchema.WD_FinalisedDate,
			};

		protected override bool ShouldDeferTrigger(BusinessObject businessEntity)
		{
			return base.ShouldDeferTrigger(businessEntity) && businessEntity is WhsDocket docket && (docket.IsFinalised || IsDocketCancelOrReactive(docket));
		}

		bool IsDocketCancelOrReactive(WhsDocket docket) => docket.WD_DocketStatusInfo.HasChanges && (docket.WD_DocketStatusInfo.OriginalValue.Equals(DocketStatus.Codes.Cancelled) || docket.WD_DocketStatus == DocketStatus.Codes.Cancelled);
	}

	#endregion
}
