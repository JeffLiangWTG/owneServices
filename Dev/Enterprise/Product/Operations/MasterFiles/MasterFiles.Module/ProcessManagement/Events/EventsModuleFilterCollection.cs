using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	class EventsModuleFilterCollection : ModuleFilterCollection
	{
		protected override ZQuery GetFilterQueryCore(IEnumerable<ModuleFilter> activeModuleFiltersForQuery, FilterGroupQueryMapAction forGroups, IEnumerable<BusinessObject> bizosToApplyFiltersTo)
		{
			var query = base.GetFilterQueryCore(activeModuleFiltersForQuery, forGroups, bizosToApplyFiltersTo);
			if (DataRegistry.Instance.ProductivityWiseModeEnabled)
			{
				query.AddToFilter(StmEventSchema.SE_IsVisibleToPW, true);
			}
			return query;
		}
	}
}
