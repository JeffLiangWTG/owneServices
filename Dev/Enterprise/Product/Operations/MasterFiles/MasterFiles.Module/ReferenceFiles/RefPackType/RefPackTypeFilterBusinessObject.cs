using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefPackTypeFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Descriptions
		{
			public const string UOMType = "F3_UOMType";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			filters.AddTextFilter("Code", RefPackTypeSchema.F3_Code).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefPackTypeFilter|Code", "Code");
			filters.AddFiltersForTranslatableText("Description", RefPackTypeSchema.F3_Description, typeof(RefPackType), ResString.GetMultilingualString("MasterFiles|RefPackTypeFilter|Description", "Description"));

			var uomFilter = new ModuleTextFilter(Descriptions.UOMType, RefPackTypeSchema.F3_UOMType, new UOMPackTypesList());
			uomFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefPackTypeFilter|UOMType", "UOM Type");
			uomFilter.ComparisonOperator_List.Clear();

			var exactOperator = ModuleTextFilter.ComparisonConstants.GetComparisonOperatorPair(ModuleTextFilter.ComparisonConstants.Exact);
			var isBlankOperator = ModuleTextFilter.ComparisonConstants.GetComparisonOperatorPair(ModuleTextFilter.ComparisonConstants.IsBlank);
			var isNotBlankOperator = ModuleTextFilter.ComparisonConstants.GetComparisonOperatorPair(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			uomFilter.ComparisonOperator_List.Add(exactOperator);
			uomFilter.ComparisonOperator_List.Add(isBlankOperator);
			uomFilter.ComparisonOperator_List.Add(isNotBlankOperator);

			filters.AddFilter(uomFilter);

			return filters;
		}
	}
}
