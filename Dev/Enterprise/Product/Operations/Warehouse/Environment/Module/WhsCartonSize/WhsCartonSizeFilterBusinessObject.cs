using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Module
{
	public class WhsCartonSizeFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string Code = "Code"; // Filter description
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			AddTextFilters(result);
			return result;
		}

		#endregion

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(Schema.Code, WhsCartonSizeSchema.WCS_Code).MultilingualDescription = ResString.GetMultilingualString("WhsCartonSizeBusinessObject|Code", "Code");
		}

		#endregion
	}
}
