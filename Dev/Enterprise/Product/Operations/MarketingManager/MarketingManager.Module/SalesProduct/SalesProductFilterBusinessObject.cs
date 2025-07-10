using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Module
{
	public class SalesProductFilterBusinessObject : FilterStripBusinessObject
	{
		#region Module Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			result.AddTextFilter(FilterDescription.Code, OrgSalesProductSchema.MP_Code)
				.MultilingualDescription = ResString.GetMultilingualString("88d0c257-df98-48e1-ab56-33b19e1ff624", "Code");

			result.AddFiltersForTranslatableText(FilterDescription.Name, OrgSalesProductSchema.MP_Name, typeof(OrgSalesProduct), ResString.GetMultilingualString("9635d4e2-6008-4aee-9779-e6d98019beef", "Name"));

			return result;
		}

		#endregion

		#region Filter Descriptions

		public static class FilterDescription
		{
			#region SuppressResourceStringsCheckRegion

			public const string Code = "Code";
			public const string Name = "Name";

			#endregion
		}

		#endregion
	}
}
