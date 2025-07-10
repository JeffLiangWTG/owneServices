using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class WorkflowJobModuleFilter : ModuleGuidFilter
	{
		public WorkflowJobModuleFilter(ZString description, ModuleIdentifier id, IBusinessObjectCollection list)
			: base(description, id, ProcessTasksSchema.P9_FH_ProcessHeader, list)
		{
			this.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|Module|WorkflowJobModuleFilter|Workflow", "Workflow");
			this.SupportsFiltersMatchComparisonOperator = true;
		}

		public override bool HasComparisonOperator => true;

		protected override ZQuery GetQuery()
		{
			if (ComparisonOperator == ComparisonConstants.Exact || ComparisonOperator == ComparisonConstants.NotEqual)
			{
				if (Property.IsEmpty)
				{
					return new ZQuery();
				}

				return GetExactOrNotEqualQuery();
			}
			if (ComparisonOperator == ComparisonConstants.FiltersMatch)
			{
				return GetFiltersMatchQuery();
			}
			else
			{
				return GetQueryUsingFilterColumns();
			}
		}

		ZQuery GetExactOrNotEqualQuery()
		{
			var isNotIn = ComparisonOperator == ComparisonConstants.NotEqual;

			var result = new ZDBOnlyQuery(typeof(ProcessTask));
			var subQuery = new ZDBOnlySubQuery(typeof(IProcessHeader), ProcessTasksSchema.P9_FH_ProcessHeader, isNotIn);
			subQuery.AddToFilter(ProcessHeaderSchema.PK, Property);
			result.AddSubQuery(subQuery, JoinCondition.And);

			var jobHeaderSubQuery = new ZDBOnlySubQuery(typeof(IProcessHeader), ProcessHeaderSchema.FH_ParentId, isNotIn);
			jobHeaderSubQuery.AddToFilter(ProcessHeaderSchema.PK, Property);
			jobHeaderSubQuery.AddToFilter(ProcessHeaderSchema.FH_FH_ParentHeader, null);

			var taskSubQuery = new ZDBOnlyQuery(typeof(ProcessTask));
			taskSubQuery.AddSubQuery(ProcessTasksSchema.P9_ParentID, jobHeaderSubQuery, JoinCondition.And);

			result.AddToFilter(taskSubQuery, isNotIn ? JoinCondition.And : JoinCondition.Or);

			return result;
		}

		ZQuery GetFiltersMatchQuery()
		{
			var baseQuery = GetQueryForSelectedFilters();

			var taskQuery = new ZDBOnlyQuery(typeof(ProcessTask));
			var subQuery = new ZDBOnlySubQuery(typeof(IProcessHeader), ProcessTasksSchema.P9_FH_ProcessHeader);

			var subModuleFilter = SelectedFilters.Filter;
			var jobHeaderSubQuery = new ZDBOnlySubQuery(typeof(IProcessHeader), ProcessHeaderSchema.PK);
			jobHeaderSubQuery.AddToFilter(ProcessHeaderSchema.FH_FH_ParentHeader, null);
			jobHeaderSubQuery.AddToFilter(subModuleFilter);

			var jobworkflowQuery = new ZDBOnlySubQuery(typeof(IProcessHeader), ProcessHeaderSchema.PK);
			jobworkflowQuery.AddSubQuery(ProcessHeaderSchema.FH_FH_ParentHeader, jobHeaderSubQuery, JoinCondition.And);
			subQuery.AddSubQuery(jobworkflowQuery, JoinCondition.And);

			taskQuery.AddSubQuery(subQuery, JoinCondition.And);

			baseQuery.AddToFilter(taskQuery, JoinCondition.Or);

			return baseQuery;
		}
	}
}
