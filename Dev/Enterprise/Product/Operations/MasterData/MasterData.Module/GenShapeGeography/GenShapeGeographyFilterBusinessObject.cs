using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Module
{
	public class GenShapeGeographyFilterBusinessObject : FilterStripBusinessObject
	{
		public GenShapeGeographyFilterBusinessObject()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);

			return filters;
		}

		#region Filters

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Name", GenShapeGeographySchema.SHG_Name).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GenShapeGeographyFilter|Name", "Name");
			filters.AddTextFilter("Description", GenShapeGeographySchema.SHG_Description).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GenShapeGeographyFilter|Description", "Description");
			SetActiveStatusFilter(GenShapeGeographySchema.SHG_IsActive, true);
		}

		protected override bool IsActiveStatusFilterAlwaysApplied()
		{
			return false;
		}

		#endregion

		#endregion
	}
}
