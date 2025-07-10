using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefJobEquipmentFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var codeFilter = filters.AddTextFilter("Code", JobEquipmentSchema.JEQ_Code);
			codeFilter.MultilingualDescription = ResString.GetMultilingualString("RefJobEquipmentFilter|Code", "Code");

			var descriptionFilter = filters.AddTextFilter("Description", JobEquipmentSchema.JEQ_Description);
			descriptionFilter.MultilingualDescription = ResString.GetMultilingualString("RefJobEquipmentFilter|Description", "Description");

			return filters;
		}
	}
}
