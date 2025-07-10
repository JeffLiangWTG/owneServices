using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class GenCustomAddOnRuleFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			filters.AddTextFilter(GenCustomAddOnRuleSchema.Constants.XR_Code, GenCustomAddOnRuleSchema.XR_Code).MultilingualDescription = ResString.GetMultilingualString("XRFilter|XR_Code", "Code");
			filters.AddTextFilter(GenCustomAddOnRuleSchema.Constants.XR_Description, GenCustomAddOnRuleSchema.XR_Description).MultilingualDescription = ResString.GetMultilingualString("XRFilter|XR_Description", "Description");
			return filters;
		}

		#endregion
	}
}
