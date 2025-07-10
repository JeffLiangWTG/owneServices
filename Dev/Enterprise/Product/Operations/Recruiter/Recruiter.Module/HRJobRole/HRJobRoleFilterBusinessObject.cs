using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Module
{
	public class HRJobRoleFilterBusinessObject : FilterStripBusinessObject
	{
		public HRJobRoleFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);

			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Title", HRJobRoleSchema.HJ_JobTitle).MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobRoleFilter|Title", "Title");
			filters.AddTextFilter("Description", HRJobRoleSchema.HJ_JobRoleDescription).MultilingualDescription = ResString.GetMultilingualString("Recruiter|HRJobRoleFilter|Description", "Description");
		}

		#endregion

		#endregion
	}
}
