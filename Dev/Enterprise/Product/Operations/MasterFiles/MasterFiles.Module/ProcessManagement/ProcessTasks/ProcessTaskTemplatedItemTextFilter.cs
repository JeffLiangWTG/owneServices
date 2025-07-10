using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class ProcessTaskTemplatedItemTextFilter : TemplatedItemTextFilter
	{
		public ProcessTaskTemplatedItemTextFilter(ProcessTaskFilterBusinessObject filterBusinessObject, FilterVisibility visibility = FilterVisibility.AlwaysApplied)
			: base(GetQueryCore, visibility)
		{
			this.filterBusinessObject = filterBusinessObject;
		}

		readonly ProcessTaskFilterBusinessObject filterBusinessObject;

		protected override ZQuery GetQuery()
		{
			var isFilterApplicable =
				IsActive // The user has enabled this filter strip, so we apply their choice to this query.
				|| (!filterBusinessObject.IsInFilterRuleMode && filterBusinessObject.ShouldAddNonTemplateFilter); // This would slow down the query unnecessarily when using this for Visual Boards, if we did actually add it.

			return isFilterApplicable ? base.GetQuery() : new ZQuery();
		}

		static ZQuery GetQueryCore(ZString value)
		{
			var query = new ZQuery();

			if (value == TemplateFilterOptions.Codes.Template)
			{
				query.AddToFilter(ProcessTasksSchema.P9_ParentTableCode, ProcessTaskTemplateSchema.Constants.Prefix);
			}
			else if (value == TemplateFilterOptions.Codes.NonTemplate)
			{
				query.AddToFilter(ProcessTasksSchema.P9_ParentTableCode, SQLComparisonOperator.NotEqual, ProcessTaskTemplateSchema.Constants.Prefix);
			}

			return query;
		}
	}
}
