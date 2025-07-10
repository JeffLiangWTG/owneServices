using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Module
{
	class PackLinesFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			var allocationIDFilter = filters.AddTextFilter("Pack Line ID", JobPackLinesSchema.JL_PackLineId);
			allocationIDFilter.MultilingualDescription = ResString.GetMultilingualString("29ad0fac-46fe-c982-47cc-3c19f158a2a3", "Pack Line ID");

			return filters;
		}
	}
}
