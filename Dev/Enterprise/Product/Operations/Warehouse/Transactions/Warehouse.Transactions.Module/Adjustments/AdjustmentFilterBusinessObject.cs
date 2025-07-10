using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class AdjustmentFilterBusinessObject : DocketFilterBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = base.GetModuleFiltersCore();
			filters.AddDateFilter("Date", WhsDocketSchema.WD_BookingDate).MultilingualDescription = ResString.GetMultilingualString("deb61be2-049d-40d1-9304-f967c3f7e0f4", "Date");

			var filter = filters.AddTextFilter("Reference", WhsDocketSchema.WD_ExternalReference);
			filter.MultilingualDescription = ResString.GetMultilingualString("0ab86db8-9ddf-4fea-83a0-188e5aac8f7f", "Reference");
			filter.Category = FilterCategories.NumbersAndReferences;

			var adjustmentReasonFilter = filters.AddTextFilter("Reason Code", GetAdjustmentWithReasonCodeQuery, WarehouseDataRegistry.Instance.AdjustmentReasonCodes.Value);
			adjustmentReasonFilter.MultilingualDescription = ResString.GetMultilingualString("c5ab1ec7-8bb9-4d4d-babc-7a154157337d", "Reason Code");
			adjustmentReasonFilter.Category = FilterCategories.NumbersAndReferences;

			var adjustmentTypeFilter = filters.AddTextFilter("Adjustment Type", GetSubTypeQuery, AdjustmentTypes);
			adjustmentTypeFilter.MultilingualDescription = ResString.GetMultilingualString("D0B1C8F3-9261-4CB3-8452-63E5110351C0", "Adjustment Type");
			adjustmentTypeFilter.Category = FilterCategories.StatusAndFlags;

			AddWorkflowFilterStripsHelper(typeof(WhsAdjustment), WorkflowDescriptors.WhsAdjustmentWorkflowDescriptorCode);

			return filters;
		}

		ZQuery GetAdjustmentWithReasonCodeQuery(ZString reasonCode)
		{
			var query = new ZDBOnlyQuery(typeof(WhsAdjustment));
			query.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Adjustment);

			var subQuery = new ZDBOnlySubQuery(typeof(WhsAdjustmentLine), WhsDocketLineSchema.WE_WD);
			subQuery.AddToFilter(WhsDocketLineSchema.WE_ReasonCode, reasonCode);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region Docket Status Filter

		protected override CodeDescriptionPairList GetDocketStatusCore()
		{
			var status = base.GetDocketStatusCore();
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.New));
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.AttachedToPick));
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.Picking));
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.Putaway));
			return status;
		}

		#endregion

		#region Adjustment Type

		CodeDescriptionPairList AdjustmentTypes
		{
			get { return new AdjustmentType(); }
		}

		#endregion

		#region IncludeTransportCoFilter

		protected override bool IncludeTransportCoFilter => false;

		#endregion
	}
}
